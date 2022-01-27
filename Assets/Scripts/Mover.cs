using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IController : IActivity, IPhysicalState {}

/// <summary>衝突判定の対象となるオブジェクト。自機と敵と弾の親クラス。</summary>
/// <remarks>
/// デフォルトでenabledにfalseを設定するため、生成しただけでは更新されない。継承先で「実体化関数」を定義する必要がある。名前を別にしたのは、各クラスで渡すべき引数が異なるため。
/// </remarks>
public abstract class MoverController : MonoBehaviour, IController
{
    protected Rigidbody2D _rigid2D;
    private const float MovingThreshold = 80 * 80;  // Rigidbody2D.MovePositionを使うか否かの基準。値自体は当てずっぽう。
    private bool _enabled = false;  // パラメータを更新するか否かのフラグ。
    private float _speed = 0.0f;
    private float _angle = 0.5f * Mathf.PI;

    public Vector2 Position
    {
        get { return transform.position; }
        set {
            if (_rigid2D.simulated)
                if (value.sqrMagnitude > MovingThreshold)
                   _rigid2D.position = value;
                else
                    _rigid2D.MovePosition(value);
            else
                transform.position = value;
        }
    }

    public float Speed
    {
        get { return _speed; }
        set
        {
            if (_enabled)
                _speed = value;
        }
    }

    public virtual float Angle
    {
        get { return _angle; }
        set
        {
            if (_enabled)
                _angle = Mathf.Repeat(value, 2f * Mathf.PI);  // 負の値が渡されても、0 <= angle < 2 pi になるように変換する。
        }
    }

    protected virtual void Awake()
    {
        _rigid2D = GetComponent<Rigidbody2D>();
        _rigid2D.simulated = false;
    }

    protected virtual void FixedUpdate()
    {
        _rigid2D.velocity = new Vector2(Speed * Mathf.Cos(Angle), Speed * Mathf.Sin(Angle)) / Time.fixedDeltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
    }

    /*// Luaに渡すために、インターフェイスで指定したメソッド以外も定義する。
    public float PosX
    {
        get { return _mover.Position.x; }
        set {
            var position = _mover.Position;
            position.x = value;
            _mover.Position = position;
        }
    }
    public float PosY
    {
        get { return _mover.Position.y; }
        set {
            var position = _mover.Position;
            position.y = value;
            _mover.Position = position;
        }
    }*/

    public virtual void Erase()
    {
        GetComponent<SpriteAnimator>().enabled = false;
        _rigid2D.simulated = false;
        _enabled = false;
    }

    public bool IsEnabled()
    {
        return _enabled;
    }

    protected void spawned()
    {
        GetComponent<SpriteAnimator>().enabled = true;
        _rigid2D.simulated = true;
        _enabled = true;
    }
}

// ラッパークラス。
public abstract class Mover<TController> : IController, ICollisionHandler, IInvincibility
    where TController : MoverController
{
    protected TController _controller;
    private ICollisionHandler _collisionHandler;
    private IInvincibility _invincibility;

    public Mover(TController controller, ICollisionHandler collisionHandler, IInvincibility invincibility)
    {
        _controller = controller;
        _collisionHandler = collisionHandler;
        _invincibility = invincibility;
    }

    public Vector2 Position { get => _controller.Position; set => _controller.Position = value; }
    public float Speed { get => _controller.Speed; set => _controller.Speed = value; }
    public float Angle { get => _controller.Angle; set => _controller.Angle = value; }
    public int Damage { get => _collisionHandler.Damage; }
    public int HitPoint { get => _collisionHandler.HitPoint; }
    public uint InvincibleCount { get => _invincibility.InvincibleCount; }

    public void Erase()
    {
        _controller.Erase();
    }

    public bool IsEnabled()
    {
        return _controller.IsEnabled();
    }

    public bool IsInvincible()
    {
        return _invincibility.IsInvincible();
    }

    public void TurnInvincible(uint frames)
    {
        _invincibility.TurnInvincible(frames);
    }
}

/// <summary>IMoverを継承したUnityEngine.GameObjectの生成クラス。</summary>
/// <remarks>
/// Controllerクラス（プレハブから複製されたGameObjectにアタッチされているもの）を子オブジェクトとして所有する。
/// 生成の際には、もし使われていないGameObjectが存在すればそれを返し、もし全てが使われていれば新たに生成する。
/// 列挙型IDの要素は複製対象のプレハブ名との一致を要請する。
/// </remarks>
public abstract class MoverGenerator<TController, ID> : MonoBehaviour
    where TController : MoverController
    where ID : Enum
{
    private List<TController> pool;

    /*public IList<TMover> ObjectsList
    {
        get {
            var temp = new List<TMover>();
            foreach (Transform child in transform)
                if (child.GetComponent<IActivity>() is var mover && mover.IsEnabled())
                    temp.Add(mover );
            return temp;
        }
    }*/

    /// <summary>未使用のMoverController型のオブジェクトを子オブジェクトから探索して、見つかればそれを返す。見つからなければ新しく生成する。</summary>
    /// <param name="id">複製するプレハブに対応する列挙名</param>
    /// <param name="position">生成する位置</param>
    /// <returns>生成されたGameObjectにアタッチされているControllerクラス</returns>
    /// <remarks>
    /// 生成されたGameObjectは描画されず、物理演算も実行されない。返り値に対して「実体化関数」を呼ぶことで、それらが有効化される。
    /// また、ステージを作成する上でGameObject自体をそのまま扱うことは少ないと判断したので、返り値はControllerクラスである。
    /// GameObject自体を扱いたい場合は、このクラスの子オブジェクトを参照すれば良い。
    /// </remarks>
    protected Mover<TController> GenerateObject(ID id, Vector2 position, Func<IController, ICollisionHandler, IInvincibility, TController> generator)
    {
        /*foreach (Transform child in transform)
            // 変数への参照数でも判定したいが、Unityから参照されているものの判別が難しい上に、C#はC++ほど簡単に参照数を取得できなさそう（単に0か否かの峻別は可能）。
            if (child.name == id.ToString() && child.GetComponent<IController>() is var controller && !controller.IsEnabled())
            {
                //child.transform.position = position;  // Rigidbody2d.simulatedがオフになっているため、Transform.positionで変更。
                controller.Position = position;
                // TODO: poolから取り出す。
                return generator(controller, child.GetComponent<ICollisionHandler>(), child.GetComponent<IInvincibility>());
            }
        //var prefab = Addressables.LoadAssetAsync<GameObject>(id.ToString()).WaitForCompletion();
        //var newObject = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        //newObject.name = prefab.name;
        //newObject.transform.parent = this.transform;
        var newObject = Addressables.InstantiateAsync(id.ToString(), position, Quaternion.identity, this.transform).WaitForCompletion();
        newObject.name = id.ToString();
        return generator(newObject.GetComponent<IController>(), newObject.GetComponent<ICollisionHandler>(), newObject.GetComponent<IInvincibility>());*/
        return null;
    }
}

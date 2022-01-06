using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public interface IMover
{
    Vector2 Position { get; set; }
    float Speed { get; set; }
    float Angle { get; set; }
    int Damage { get; }
    int HitPoint { get; }

    void Erase();
    bool IsEnabled();
    bool IsInvincible();
    void MovePosition(Vector2 position);
    void TurnInvincible(uint frames);
}

// ***ControllerクラスはMonoBehaviourクラスを継承したもの（アタッチ可能）。
// MonoBehaviorのメソッドに加えて、***と同名のクラスと同じインターフェイスを持つ。
// Controllerと名の付く抽象クラスは***Generatorクラスに共用管理される。
public abstract class MoverController<ForwardedMover> : MonoBehaviour, IMover
    where ForwardedMover : Mover  // 処理を委譲するクラス。
{
    protected ForwardedMover _mover;  // 継承先のAwakeで初期化するする必要あり。

    public Vector2 Position { get => _mover.Position; set => _mover.Position = value; }
    public float Speed { get => _mover.Speed; set => _mover.Speed = value; }
    public float Angle { get => _mover.Angle; set => _mover.Angle = value; }
    public int Damage { get => _mover.Damage; }
    public int HitPoint { get => _mover.HitPoint; }

    // Luaに渡すために、インターフェイスで指定したメソッド以外も定義する。
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
    }

    public virtual void Erase()
    {
        _mover.Erase();
    }

    public bool IsEnabled()
    {
        return _mover.IsEnabled();
    }

    public bool IsInvincible()
    {
        return _mover.IsInvincible();
    }

    public void MovePosition(Vector2 position)
    {
        _mover.MovePosition(position);
    }

    public void TurnInvincible(uint frames)
    {
        _mover.TurnInvincible(frames);
    }
}

/// <summary>衝突判定の対象となるオブジェクト。自機と敵と弾の親クラス。</summary>
/// <remarks>
/// デフォルトでenabledにfalseを設定するため、生成しただけでは更新されない。継承先で「実体化関数」を定義する必要がある。名前を別にしたのは、各クラスで渡すべき引数が異なるため。
/// </remarks>
public abstract class Mover : IMover
{
    protected Transform _transform;
    protected SpriteRenderer _spriteRenderer;
    protected Rigidbody2D _rigid2D;
    protected float _speed;  // 単位：ドット毎フレーム
    protected float _angle;  // x軸を基準とした角度。時計回りの方向を正とする。
    protected int _damage;  // 衝突時に相手に与えるダメージ。
    protected int _hitPoint;  // 体力。「消滅」と「撃破」とを区別するために弾でも設定が必須。外部からこれを参照する以外に、OnCollideに返り値を持たせる実装でも良いかもしれない。
    protected bool _enabled = false;  // パラメータを更新するか否かのフラグ。
    protected uint _invincibleCounter = 0;  // 無敵状態になっている残りのフレーム数。
    private uint _existingCounter = 0;  // enabledがtrueになってからのフレーム数。
    private readonly Vector2 ScreenMinimum, ScreenMaximum;  // 画面の左下の座標と右下の座標から、画像の大きさの半分だけ拡げた座標。

    /// <summary>画面外にあるために無効にするか否かを判定。</summary>
    private Action disableIfOutside;

    /// <summary>ゲーム内で衝突判定するオブジェクトの処理を委譲。</summary>
    /// <param name="transform">委譲される位置</param>
    /// <param name="spriteRenderer">委譲されるスプライト</param>
    /// <param name="rigid2D">委譲される物理演算クラス</param>
    /// <param name="speed">初期速度の速さ</param>
    /// <param name="angle">初期速度の方向</param>
    /// <param name="damage">衝突時に相手に与えるダメージ</param>
    /// <param name="hitPoint">体力</param>
    /// <param name="autoDisabling">画面外に出たら自動的に無効にするか否か</param>
    public Mover(in Transform transform, in SpriteRenderer spriteRenderer, in Rigidbody2D rigid2D, float speed, float angle, int damage, int hitPoint, bool autoDisabling = true)
    {
        _transform = transform;
        _spriteRenderer = spriteRenderer;
        _rigid2D = rigid2D;
        _speed = speed;
        _angle = angle;
        _damage = damage;
        _hitPoint = hitPoint;
        float width = spriteRenderer.bounds.size.x;
        float height = spriteRenderer.bounds.size.y;
        ScreenMinimum = Camera.main.ViewportToWorldPoint(Vector2.zero) - (new Vector3(width, height, 0) * 0.5f);
        ScreenMaximum = Camera.main.ViewportToWorldPoint(Vector2.one) + (new Vector3(width, height, 0) * 0.5f);

        if (autoDisabling)
            disableIfOutside = () =>
            {
                ++_existingCounter;
                if (!isInside())
                    Erase();
            };
        else
            disableIfOutside = () => {};
    }

    public Vector2 Position
    {
        get { return _transform.position; }
        set {
            if (_rigid2D.simulated)
                _rigid2D.position = value;
            else
                _transform.position = value;
        }
    }

    public float Speed
    {
        get { return _speed; }
        set {
            if (_enabled)
                _speed = value;
        }
    }
    public virtual float Angle
    {
        get { return _angle; }
        set {
            if (_enabled)
                _angle = Mathf.Repeat(value, 2f * Mathf.PI);  // 負の値が渡されても、0 <= angle < 2 pi になるように変換する。
        }
    }
    public int Damage { get => _damage; }
    public int HitPoint { get => _hitPoint; }

    /// <summary>MonoBehaviorのUpdateから呼ばれる処理。</summary>
    public virtual void Update()
    {
        _spriteRenderer.sprite = clipFromImage(Time.frameCount);
    }

    /// <summary>MonoBehaviorのFixedUpdateから呼ばれる処理。</summary>
    public virtual void FixedUpdate()
    {
        _rigid2D.velocity = new Vector2(Speed * Mathf.Cos(Angle), Speed * Mathf.Sin(Angle)) / Time.fixedDeltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
        _invincibleCounter = (_invincibleCounter > 0) ? _invincibleCounter - 1 : 0;
        disableIfOutside();
    }

    // 参考：https://nknkybigames.hatenablog.com/entry/2018/03/08/185122
    //       https://gomafrontier.com/unity/1189
    /// <summary>MonoBehaviorのOnCollisionEnter2Dから呼ばれる処理。</summary>
    public abstract void OnCollisionEnter2D(in IMover mover);

    public void Erase()
    {
        _spriteRenderer.enabled = false;
        _rigid2D.simulated = false;
        _enabled = false;
    }

    public bool IsEnabled()
    {
        return _enabled;
    }

    public bool IsInvincible()
    {
        return _invincibleCounter > 0;
    }

    // MovePositionメソッドはごく短い時間での移動にしか適さない（Rigidbody2Dのマニュアルを参照）ので、
    // 一気に動く場合はPositionプロパティ、それ以外はこのメソッドを使うこと。
    public void MovePosition(Vector2 position)
    {
        _rigid2D.MovePosition(position);
    }

    public void TurnInvincible(uint frames)
    {
        _invincibleCounter = frames;
    }

    protected virtual void spawned()
    {
        _spriteRenderer.enabled = true;
        _rigid2D.simulated = true;
        _enabled = true;
	    _existingCounter = 0;
    }

    /// <summary>現在のフレームにおける切り取られた画像を返す。</summary>
    /// <param name="currentFrames">現在までのフレーム数</param>
    /// <returns>切り取られたスプライト</returns>
    protected abstract Sprite clipFromImage(int countedFrames);

    /// <summary>オブジェクトが画面内に存在するか？</summary>
    /// <returns>存在すれば真、しなければ偽</returns>
    /// <remarks>
    /// 生成されてから最初の1秒（60フレーム）は判定せずに真を返す。画面外でも0.1秒（6フレーム）以内に画面内に戻れば真を返す。なお、ここでいう「存在」とはオブジェクトの画像の一部でも画面内にあることを意味する。
    /// </remarks>
	private bool isInside()
    {
        if (_existingCounter < 60)
            return true;
        if (this.Position.x < ScreenMinimum.x || this.Position.x > ScreenMaximum.x
                || this.Position.y < ScreenMinimum.y || this.Position.y > ScreenMaximum.y)
        {
            if (_existingCounter > 66)
                return false;
            else
                return true;
        }
        else
        {
            _existingCounter = 60;
            return true;
        }
    }
}

/// <summary>IMoverを継承したUnityEngine.GameObjectの生成クラス。</summary>
/// <remarks>
/// Controllerクラス（プレハブから複製されたGameObjectにアタッチされているもの）を子オブジェクトとして所有する。
/// 生成の際には、もし使われていないGameObjectが存在すればそれを返し、もし全てが使われていれば新たに生成する。
/// 列挙型IDの要素は複製対象のプレハブ名との一致を要請する。
/// </remarks>
public abstract class MoverGenerator<TMoverController, ID> : MonoBehaviour
    where TMoverController : IMover
    where ID : Enum
{
    public IList<TMoverController> ObjectsList
    {
        get {
            var temp = new List<TMoverController>();
            foreach (Transform child in transform)
                if (child.GetComponent<TMoverController>() is var mover && mover.IsEnabled())
                    temp.Add(mover);
            return temp;
        }
    }

    /// <summary>未使用のMoverController型のオブジェクトを子オブジェクトから探索して、見つかればそれを返す。見つからなければ新しく生成する。</summary>
    /// <param name="id">複製するプレハブに対応する列挙名</param>
    /// <param name="position">生成する位置</param>
    /// <returns>生成されたGameObjectにアタッチされているControllerクラス</returns>
    /// <remarks>
    /// 生成されたGameObjectは描画されず、物理演算も実行されない。返り値に対して「実体化関数」を呼ぶことで、それらが有効化される。
    /// また、ステージを作成する上でGameObject自体をそのまま扱うことは少ないと判断したので、返り値はControllerクラスである。
    /// GameObject自体を扱いたい場合は、このクラスの子オブジェクトを参照すれば良い。
    /// </remarks>
    public TMoverController GenerateObject(ID id, Vector2 position)
    {
        foreach (Transform child in transform)
            if (child.name == id.ToString() && child.GetComponent<TMoverController>() is var mover && !mover.IsEnabled())
            {
                child.transform.position = position;  // Rigidbody2d.simulatedがオフになっているため、Transform.positionで変更。
                return mover;
            }
        //var prefab = Addressables.LoadAssetAsync<GameObject>(id.ToString()).WaitForCompletion();
        //var newObject = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        //newObject.name = prefab.name;
        //newObject.transform.parent = this.transform;
        var newObject = Addressables.InstantiateAsync(id.ToString(), position, Quaternion.identity, this.transform).WaitForCompletion();
        newObject.name = id.ToString();
        return newObject.GetComponent<TMoverController>();
    }
}

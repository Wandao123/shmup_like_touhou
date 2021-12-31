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
    uint Damage { get; }
    int HitPoint { get; }

    void Erase();
    bool IsEnabled();
    bool IsInvincible();
    void TurnInvincible(uint frames);
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
    protected uint _damage;  // 衝突時に相手に与えるダメージ。
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
    public Mover(in Transform transform, in SpriteRenderer spriteRenderer, in Rigidbody2D rigid2D, float speed, float angle, uint damage, int hitPoint, bool autoDisabling = true)
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
                    _enabled = false;
            };
        else
            disableIfOutside = () => {};
    }

    public Vector2 Position
    {
        get => _transform.position;
        set => _rigid2D.MovePosition(value);
    }

    public float Speed
    {
        get { return _speed; }
        set {
            if (_enabled)
                _speed = value;
        }
    }
    public float Angle
    {
        get { return _angle; }
        set {
            if (_enabled)
                _angle = Mathf.Repeat(value, 2f * Mathf.PI);  // 負の値が渡されても、0 <= angle < 2 pi になるように変換する。
        }
    }
    public uint Damage { get => _damage; }
    public int HitPoint { get => _hitPoint; }

    /// <summary>MonoBehaviorのUpdateから呼ばれる処理。</summary>
    public virtual void Update()
    {
        _spriteRenderer.sprite = clipFromImage(Time.frameCount);
    }

    /// <summary>MonoBehaviorのFixedUpdateから呼ばれる処理。</summary>
    public virtual void FixedUpdate()
    {
        _rigid2D.velocity = new Vector2(_speed * Mathf.Cos(_angle), _speed * Mathf.Sin(_angle)) / Time.fixedDeltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
        _invincibleCounter = (_invincibleCounter > 0) ? _invincibleCounter - 1 : 0;
        disableIfOutside();
    }

    /// <summary>MonoBehaviorのOnCollisionEnter2Dから呼ばれる処理。</summary>
    public abstract void OnCollisionEnter2D(in IMover mover);

    public void Erase()
    {
        _enabled = false;
    }

    public bool IsEnabled()
    {
        return _enabled;
    }

    public bool IsInvincible()
    {
        return (_invincibleCounter > 0) ? true : false;
    }

    public void TurnInvincible(uint frames)
    {
        _invincibleCounter = frames;
    }

    protected virtual void spawned()
    {
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
public abstract class MoverGenerator<MoverController, ID> : MonoBehaviour
    where MoverController : IMover
    where ID : Enum
{
    public IList<MoverController> ObjectsList
    {
        get {
            var temp = new List<MoverController>();
            foreach (Transform child in transform)
                if (child.GetComponent<MoverController>() is var mover && mover.IsEnabled())
                    temp.Add(mover);
            return temp;
        }
    }

    /// <summary>未使用のMoverController型のオブジェクトを子オブジェクトから探索して、見つかればそれを返す。見つからなければ新しく生成する。</summary>
    /// <param name="id">複製するプレハブに対応する列挙名</param>
    /// <returns>生成されたGameObjectにアタッチされているControllerクラス</returns>
    /// <remarks>
    /// 生成されたGameObjectは描画されず、物理演算も実行されない。返り値に対して「実体化関数」を呼ぶことで、それらが有効化される。
    /// また、ステージを作成する上でGameObject自体をそのまま扱うことは少ないと判断したので、返り値はControllerクラスである。
    /// GameObject自体を扱いたい場合は、このクラスの子オブジェクトを参照すれば良い。
    /// </remarks>
    public MoverController GenerateObject(ID id)
    {
        foreach (Transform child in transform)
            if (child.name == id.ToString() && child.GetComponent<MoverController>() is var mover && !mover.IsEnabled())
                return mover;
        var prefab = Addressables.LoadAssetAsync<GameObject>(makePath(id)).WaitForCompletion();
        var newObject = Instantiate(prefab) as GameObject;
        newObject.name = prefab.name;
        newObject.transform.parent = this.transform;
        return newObject.GetComponent<MoverController>();
    }

    protected string makePath(ID id)
    {
        return "Assets/Prefabs/" + id.ToString() + ".prefab";
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using MoonSharp.Interpreter;

public interface IPlayerActivity : IActivity
{
    void Spawned();
}

public interface IPlayerPhysicalState : IPhysicalState
{
    bool SlowMode { get; set; }
    Vector2 Velocity { get; set; }  // 単位：ドット毎フレーム
}

public enum PlayerID
{
    // 自機。
    Reimu,
    Marisa,
    Sanae,
    // オプション。
    ReimuOption,
    MarisaOption,
    SanaeOption
}

public abstract class PlayerController : MoverController, IPlayerActivity, IPlayerPhysicalState
{
    private Vector2 _velocity = Vector2.zero;  // 入力との関係上、speedとangle（極座標系）のみならず、Cartesian座標系でも所持する。

    public bool SlowMode { get; set; } = false;  // 低速移動か否か。

    public virtual Vector2 Velocity
    {
        get { return _velocity; }
        set {
            //_velocity = value * Application.targetFrameRate * Time.deltaTime;  // 実時間を考慮する場合。
            _velocity = value;
            this.Speed = value.magnitude;
            this.Angle = Mathf.Atan2(value.y, value.x);
        }
    }

    protected override void FixedUpdate()
    {
        _rigid2D.velocity = this.Velocity / Time.fixedDeltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
    }

    /*// Luaに渡すために、インターフェイスで指定したメソッド以外も定義する。
    public void SetVelocity(float velX, float velY)
    {
        this.Velocity = new Vector2(velX, velY);
    }*/

    public void Spawned()  // 実体化関数
    {
        if (GetComponent<CollisionHandler>().HitPoint <= 0)
            return;
        spawned();
        this.Velocity = Vector2.zero;
        this.Angle = 0.5f * Mathf.PI;
        var spriteRenderer = GetComponent<SpriteRenderer>();  // 単なる無敵状態と区別するために、復活の際には半透明にする。
        var color = spriteRenderer.color;
        color.a = 0.75f;
        spriteRenderer.color = color;
    }
}

// Luaのためのラッパークラス。
[MoonSharpUserData]
public class Player : Mover<PlayerController>, IPlayerActivity, IPlayerPhysicalState
{
    public Player(PlayerController controller, ICollisionHandler collisionHandler, IInvincibility invincibility)
        : base(controller, collisionHandler, invincibility)
    {}

    public bool SlowMode { get => _controller.SlowMode; set => _controller.SlowMode = value; }

    public Vector2 Velocity { get => _controller.Velocity; set => _controller.Velocity = value; }
    public void Spawned() => _controller.Spawned();
}

public class PlayerManager : MoverManager<PlayerController, PlayerID>
{
    private readonly Vector2Int _characterSize;  // 本来はreadonlyにしたいところだが、MonoBehaviourを継承したクラスではコンストラクタが呼べないため、工夫が必要。

    public Vector2Int CharacterSize { get => _characterSize; }

    public PlayerManager()
    {
        var prefab = Addressables.LoadAssetAsync<GameObject>(((PlayerID)0).ToString()).WaitForCompletion();  // 自機のスプライトのサイズは何れも同じことを要請。
        Vector2 size = prefab.GetComponent<SpriteRenderer>().bounds.size;
        _characterSize = Vector2Int.RoundToInt(size);
        if (size - _characterSize != Vector2.zero)
            Debug.LogWarning("The width or the height of the sprite are not integer numbers: " + size.ToString());

        // 予めオブジェクトを生成しておく場合はここに記述。
    }

    public PlayerCharacterController GetPlayer()
    {
        return _pool.Where(controller => controller is PlayerCharacterController).FirstOrDefault() as PlayerCharacterController;
    }
}

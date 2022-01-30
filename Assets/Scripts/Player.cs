using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public interface IPlayerActivity : IActivity
{
    void Spawned();
}

public interface IPlayerPhysicalState : IPhysicalState
{
    bool SlowMode { get; set; }
    Vector2 Velocity { get; set; }  // 単位：ドット毎フレーム
}

public abstract class PlayerController : MoverController, IPlayerPhysicalState
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

    public override void ManagedFixedUpdate()
    {
        _rigid2D.velocity = this.Velocity / Time.fixedDeltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
    }
}

// Luaのためのラッパークラス。
public class Player : Mover<PlayerController, PlayerID>, IInvincibility, IPlayerActivity, IPlayerPhysicalState
{
    private IInvincibility _invincibility;

    public Player(GameObject go, PlayerID id)
        : base(go, id)
    {
        _invincibility = go.GetComponent<IInvincibility>();
    }

    public uint InvincibleCount { get => _invincibility.InvincibleCount; }
    public bool SlowMode { get => _controller.SlowMode; set => _controller.SlowMode = value; }
    public Vector2 Velocity { get => _controller.Velocity; set => _controller.Velocity = value; }

    public bool IsInvincible() => _invincibility.IsInvincible();
    public void TurnInvincible(uint frames) => _invincibility.TurnInvincible(frames);

    public void Spawned()  // 実体化関数
    {
        if (_collisionHandler.HitPoint <= 0)
            return;
        _activity.Spawned();
        this.Velocity = Vector2.zero;
        this.Angle = 0.5f * Mathf.PI;
        var spriteRenderer = _controller.GetComponent<SpriteRenderer>();  // 単なる無敵状態と区別するために、復活の際には半透明にする。
        var color = spriteRenderer.color;
        color.a = 0.75f;
        spriteRenderer.color = color;
    }
}

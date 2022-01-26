using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public interface IPlayerActivity : IActivity
{
    void Spawned();
}

public interface IPlayerPhysicalState : IPhysicalState  // TODO: ここのみにVelocityを書く。
{
    bool SlowMode { get; set; }
}

public abstract class PlayerController : MoverController, IPlayerActivity
{
    private Vector2 _velocity = Vector2.zero;  // 入力との関係上、speedとangle（極座標系）のみならず、Cartesian座標系でも所持する。

    public bool SlowMode { get; set; } = false;  // 低速移動か否か。

    public override Vector2 Velocity
    {
        get { return _velocity; }
        set {
            //_velocity = value * Application.targetFrameRate * Time.deltaTime;  // 実時間を考慮する場合。
			_velocity = value;
			base.Velocity = value;
        }
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
        var spriteRenderer = GetComponent<SpriteRenderer>();
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

    public void Spawned() => _controller.Spawned();
}
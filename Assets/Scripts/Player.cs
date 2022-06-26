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
}

public abstract class PlayerController : MoverController, IPlayerPhysicalState
{
    public bool SlowMode { get; set; } = false;  // 低速移動か否か。
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

    public bool IsInvincible() => _invincibility.IsInvincible();
    public void TurnInvincible(uint frames) => _invincibility.TurnInvincible(frames);

    public void Spawned()  // 実体化関数
    {
        if (_collisionHandler.HitPoint <= 0)
            return;
        _activity.Spawned();
        this.Angle = 90f;
        this.Speed = 0.0f;
        var spriteRenderer = _controller.GetComponent<SpriteRenderer>();  // 単なる無敵状態と区別するために、復活の際には半透明にする。
        var color = spriteRenderer.color;
        color.a = 0.75f;
        spriteRenderer.color = color;
    }
}

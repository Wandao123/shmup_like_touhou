using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public interface IEnemyActivity: IActivity
{
    void Spawned(float speed, float angle, int hitPoint);
}

public enum EnemyID
{
    SmallRedFairy,
    SmallBlueFairy
};

public abstract class EnemyController : MoverController, IEnemyActivity
{
    public void Spawned(float speed, float angle, int hitPoint)  // 実体化関数
    {
        spawned();
        this.Speed = speed;
        this.Angle = angle;
        GetComponent<CollisionHandler>().Initialize(1, hitPoint);  // HACK: これを外部に委譲できないか？
    }
}

// Luaのためのラッパークラス。
[MoonSharpUserData]
public struct Enemy : IEnemyActivity, IPhysicalState, ICollisionHandler, IInvincibility
{
    private EnemyController _controller;
    private ICollisionHandler _collisionHandler;
    private IInvincibility _invincibility;

    public Enemy(EnemyController controller, ICollisionHandler collisionHandler, IInvincibility invincibility)
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

    public void Erase() => _controller.Erase();
    public bool IsEnabled() => _controller.IsEnabled();
    public bool IsInvincible() => _invincibility.IsInvincible();
    public void TurnInvincible(uint frames) => _invincibility.TurnInvincible(frames);
    public void Spawned(float speed, float angle, int hitPoint) => _controller.Spawned(speed, angle, hitPoint);
}

public class EnemyManager : MoverManager<EnemyController, EnemyID>
{
    public EnemyManager()
    {
        // 予めオブジェクトを生成しておく場合はここに記述。
    }
}

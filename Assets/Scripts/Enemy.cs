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

public abstract class EnemyController : MoverController {}

// Luaのためのラッパークラス。
[MoonSharpUserData]
public class Enemy : Mover<EnemyController>, IInvincibility, IEnemyActivity
{
    private IInvincibility _invincibility;

    public Enemy(GameObject gameObject)
        : base(gameObject)
    {
        _invincibility = gameObject.GetComponent<IInvincibility>();
    }

    public uint InvincibleCount { get => _invincibility.InvincibleCount; }

    public bool IsInvincible() => _invincibility.IsInvincible();
    public void TurnInvincible(uint frames) => _invincibility.TurnInvincible(frames);

    public void Spawned(float speed, float angle, int hitPoint)  // 実体化関数
    {
        _activity.Spawned();
        this.Speed = speed;
        this.Angle = angle;
        _collisionHandler.HitPoint = hitPoint;
    }
}

public class EnemyManager : MoverManager<EnemyController, EnemyID>
{
    public EnemyManager()
    {
        // 予めオブジェクトを生成しておく場合はここに記述。
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyActivity: IActivity
{
    void Spawned(float speed, float angle, int hitPoint);
}

public abstract class EnemyController : MoverController {}

// Luaのためのラッパークラス。
public class Enemy : Mover<EnemyController, EnemyID>, IInvincibility, IEnemyActivity
{
    private IInvincibility _invincibility;

    public Enemy(GameObject go, EnemyID id)
        : base(go, id)
    {
        _invincibility = go.GetComponent<IInvincibility>();
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

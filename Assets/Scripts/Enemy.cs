using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public interface IEnemyActivity: IActivity
{
    void Spawned(float speed, float angle, int hitPoint);
}

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
public class Enemy : Mover<EnemyController>, IEnemyActivity
{
    public Enemy(EnemyController controller, ICollisionHandler collisionHandler, IInvincibility invincibility)
        : base(controller, collisionHandler, invincibility)
    {}

    public void Spawned(float speed, float angle, int hitPoint) => _controller.Spawned(speed, angle, hitPoint);
}
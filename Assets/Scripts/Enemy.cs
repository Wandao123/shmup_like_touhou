using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public interface IEnemy: IMover
{
    void Spawned(float speed, float angle, int hitPoint);
}

[MoonSharpUserData]
public abstract class EnemyController : MoverController<Enemy>, IEnemy
{
    public virtual void Spawned(float speed, float angle, int hitPoint)
    {
        _mover.Spawned(speed, angle, hitPoint);
    }
}

public abstract class Enemy : Mover, IEnemy
{
    private bool _isDamaged = false;  // 被弾したか否か。

    /// <summary>敵キャラクタの処理を委譲。</summary>
    /// <param name="transform">委譲される位置</param>
    /// <param name="spriteRenderer">委譲されるスプライト</param>
    /// <param name="rigid2D">委譲される物理演算クラス</param>
    public Enemy(in Transform transform, in SpriteRenderer spriteRenderer, in Rigidbody2D rigid2D)
        : base(transform, spriteRenderer, rigid2D, 0.0f, -0.5f * Mathf.PI, 1, 0)
    {}

    public override void Update()
    {
        base.Update();
        if (_isDamaged)  // 点滅する。Sinを使ったらどうか？
        {
            _spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
            _isDamaged = false;
        }
        else
        {
            _spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f);
        }
    }

    public override void OnCollisionEnter2D(in IMover mover)
    {
        _hitPoint -= mover.Damage;
        if (_hitPoint <= 0)
            Erase();
        else
            _isDamaged = true;
    }

    public void Spawned(float speed, float angle, int hitPoint)  // 実体化関数
    {
        base.spawned();
        this.Speed = speed;
        this.Angle = angle;
        _hitPoint = hitPoint;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public interface IBullet: IMover
{
    void Shot(float speed, float angle);
}

[MoonSharpUserData]
public abstract class BulletController : MoverController<Bullet>, IBullet
{
    public virtual void Shot(float speed, float angle)
    {
        _mover.Shot(speed, angle);
    }
}

public abstract class Bullet : Mover, IBullet
{
    /// <summary>弾の処理を委譲。</summary>
    /// <param name="transform">委譲される位置</param>
    /// <param name="spriteRenderer">委譲されるスプライト</param>
    /// <param name="rigid2D">委譲される物理演算クラス</param>
    /// <param name="damage">衝突時に相手に与えるダメージ</param>
    public Bullet(in Transform transform, in SpriteRenderer spriteRenderer, in Rigidbody2D rigid2D, int damage)
        : base(transform, spriteRenderer, rigid2D, 0.0f, -0.5f * Mathf.PI, damage, 0)
    {}

    public override float Angle
    {
        get { return base.Angle; }
        set {
            base.Angle = value;
            // 進行方向に対して、元の画像は +PI/2 の向きが正位置。よって、画像の回転角は -PI/2 される。
            if (_rigid2D.simulated)
                _rigid2D.rotation = (this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg;
            else
                _transform.rotation = Quaternion.Euler(0.0f, 0.0f, (this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _rigid2D.MoveRotation((this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg);  // 進行方向に対して、元の画像は +PI/2 の向きが正位置。よって、画像の回転角は -PI/2 される。
    }

    public override void OnCollisionEnter2D(in IMover mover)
    {
        Erase();
        _hitPoint = 0;
    }

    public void Shot(float speed, float angle)  // 実体化関数
    {
        base.spawned();
        this.Speed = speed;
        this.Angle = angle;
        _hitPoint = 1;
    }
}

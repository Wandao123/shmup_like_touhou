using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public interface IBulletActivity: IActivity
{
    void Shot(float speed, float angle);
}

public abstract class BulletController : MoverController, IBulletActivity
{
    private const float RotatingThreshold = 15 * Mathf.Deg2Rad;  // Rigidbody2D.MoveRotationを使うか否かの基準。値自体は当てずっぽう。
    //private Action rotate = () => {};  // 回転させるための関数。Rigidbody2D.rotationに代入するべきか、Rigidbody2D.MoveRotationを呼ぶべきか適宜切り替える。

    public override float Angle
    {
        get { return base.Angle; }
        set {
            base.Angle = value;
            // 進行方向に対して、元の画像は +PI/2 の向きが正位置。よって、画像の回転角は -PI/2 される。
            if (_rigid2D.simulated)
                if (value > RotatingThreshold)
                    //rotate = () => { _rigid2D.rotation = (this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg; };
                    _rigid2D.rotation = (this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg;
                else
                    //rotate = () => { _rigid2D.MoveRotation((this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg); };
                    _rigid2D.MoveRotation((this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg);
            else
                //rotate = () => { transform.rotation = Quaternion.Euler(0.0f, 0.0f, (this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg); };
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, (this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        //rotate();
    }

    public void Shot(float speed, float angle)  // 実体化関数
    {
        spawned();
        this.Speed = speed;
        this.Angle = angle;
        //GetComponent<CollisionHandler>().Initialize(getDamage(), 1);
    }
}

// Luaのためのラッパークラス。
[MoonSharpUserData]
public class Bullet : Mover<BulletController>, IBulletActivity
{
    public Bullet(BulletController controller, ICollisionHandler collisionHandler, IInvincibility invincibility)
        : base(controller, collisionHandler, invincibility)
    {}

    public void Shot(float speed, float angle) => _controller.Shot(speed, angle);
}

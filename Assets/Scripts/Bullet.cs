using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoonSharp.Interpreter;

public interface IBulletActivity: IActivity
{
    void Shot(float speed, float angle);
}

public abstract class BulletController : MoverController
{
    private const float RotatingThreshold = 15 * Mathf.Deg2Rad;  // Rigidbody2D.MoveRotationを使うか否かの基準。値自体は当てずっぽう。

    public override float Angle
    {
        get { return base.Angle; }
        set {
            base.Angle = value;
            // 進行方向に対して、元の画像は +PI/2 の向きが正位置。よって、画像の回転角は -PI/2 される。
            if (_rigid2D.simulated)
                if (value > RotatingThreshold)
                    _rigid2D.rotation = (this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg;
                else
                    _rigid2D.MoveRotation((this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg);
            else
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, (this.Angle - 0.5f * Mathf.PI) * Mathf.Rad2Deg);
        }
    }
}

// Luaのためのラッパークラス。
[MoonSharpUserData]
public class Bullet : Mover<BulletController>, IBulletActivity
{
    public Bullet(GameObject gameObject)
        : base(gameObject)
    {}

    public void Shot(float speed, float angle)  // 実体化関数
    {
        _activity.Spawned();
        this.Speed = speed;
        this.Angle = angle;
        //_collisionHandler.HitPoint = 1;
    }
}

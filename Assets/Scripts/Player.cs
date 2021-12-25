using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : Mover
{
    public bool SlowMode = false;  // 低速移動か否か。
    private Vector2 velocity = new Vector2(0.0f, 0.0f);
    public virtual Vector2 Velocity
    {
        get { return this.velocity; }
        set {
            //this.velocity = value * Application.targetFrameRate * Time.deltaTime;  // 実時間を考慮する場合。
			this.velocity = value;
			this.Speed = Velocity.magnitude;
			this.Angle = Mathf.Atan2(Velocity.y, Velocity.x);
        }
    }
}

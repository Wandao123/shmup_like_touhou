using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionHandler : CollisionHandler
{
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        _activity.Erase();
        _hitPoint = 0;
    }
}

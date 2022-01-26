using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionHandler : CollisionHandler
{
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!_activity.IsEnabled() || GetComponent<IInvincibility>().IsInvincible())
            return;
        _activity.Erase();
        --_hitPoint;
    }
}

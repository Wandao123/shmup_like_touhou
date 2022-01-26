using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollisionHandler : CollisionHandler
{
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        _hitPoint -= other.gameObject.GetComponent<ICollisionHandler>().Damage;
        if (_hitPoint <= 0)
            _activity.Erase();
    }
}

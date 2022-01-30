using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionHandler
{
    int Damage { get; }
    int HitPoint { get; set; }
}

public abstract class CollisionHandler : MonoBehaviour, ICollisionHandler
{
    protected IActivity _activity;
    [SerializeField]
    protected int _damage;  // 衝突時に相手に与えるダメージ。
    [SerializeField]
    protected int _hitPoint;  // 体力。「消滅」と「撃破」とを区別するために弾でも設定が必須。

    public int Damage { get => _damage; }
    
    public int HitPoint
    {
        get { return _hitPoint; }
        set {
            if (value <= 0)
            {
                Debug.LogWarning("'hit point' must be positive");
                _hitPoint = 0;
            }
            else
            {
                _hitPoint = value;
            }
        }
    }

    protected void Awake()
    {
        _activity = GetComponent<IActivity>();
    }

    protected abstract void OnTriggerEnter2D(Collider2D other);
}

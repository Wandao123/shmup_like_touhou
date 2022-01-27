using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionHandler
{
    int Damage { get; }
    int HitPoint { get; }
}

public abstract class CollisionHandler : MonoBehaviour, ICollisionHandler
{
    protected IActivity _activity = null;
    [SerializeField]
    protected int _damage;  // 衝突時に相手に与えるダメージ。
    [SerializeField]
    protected int _hitPoint;  // 体力。「消滅」と「撃破」とを区別するために弾でも設定が必須。

    public int Damage { get => _damage; }
    public int HitPoint { get => _hitPoint; }

    protected virtual void Awake()
    {
        _activity = GetComponent<IActivity>();
    }

    protected abstract void OnTriggerEnter2D(Collider2D other);

    public void Initialize(int damage, int hitPoint)
    {
        if (damage <= 0 || hitPoint <= 0)
        {
            Debug.LogWarning("'damage' and 'hit point' must be positive");
            damage = 0;
            hitPoint = 0;
        }
        _damage = damage;
        _hitPoint = hitPoint;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionHandler
{
    int Damage { get; }
    int HitPoint { get; }
    //bool WasDamaged { get; }  // 1つ前のフレームで被弾したか否か。
}

public abstract class CollisionHandler : MonoBehaviour, ICollisionHandler
{
    protected IActivity _activity = null;
    [SerializeField]
    protected int _damage;  // 衝突時に相手に与えるダメージ。
    [SerializeField]
    protected int _hitPoint;  // 体力。「消滅」と「撃破」とを区別するために弾でも設定が必須。
    //private bool _wasDamaged = false;

    public int Damage { get => _damage; }
    public int HitPoint { get => _hitPoint; }
    //public bool WasDamaged { get => _wasDamaged; }

    protected virtual void Awake()
    {
        _activity = GetComponent<IActivity>();
    }

    /*protected virtual void Update()
    {
        _wasDamaged = false;
    }*/

    // 参考：https://nknkybigames.hatenablog.com/entry/2018/03/08/185122
    //       https://gomafrontier.com/unity/1189
    /*protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        _wasDamaged = true;
    }*/
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

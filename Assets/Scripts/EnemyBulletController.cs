using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor;

public class EnemyBulletController : BulletController
{
    [SerializeField]
    private string _reference;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigid2D;
    //private Collider2D _collider2D;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;  // インスペクタで設定すると、プレハブ自体に表示されなくなるので、ここで設定する。
        _rigid2D = GetComponent<Rigidbody2D>();
        _rigid2D.simulated = false;
        //_collider2D = GetComponent<Collider2D>();
        //_collider2D.enabled = false;  // Physics2D.IgnoreCollisionを使う方法は相手のCollider2Dを取得する必要があるため煩雑。
        _mover = new EnemyBullet(transform, _spriteRenderer, _rigid2D);
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (_mover.IsEnabled())
            _mover.Update();
    }

    void FixedUpdate()
    {
        _mover.FixedUpdate();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _mover.OnCollisionEnter2D(collision.gameObject.GetComponent<IMover>() as IMover);
        //_collider2D.enabled = false;
    }

    /*public override void Shot(float speed, float angle)
    {
        base.Shot(speed, angle);
        _collider2D.enabled = true;
    }*/
}

/// <summary>敵弾クラス。</summary>
class EnemyBullet : Bullet
{
    private readonly Sprite _clip;

    /// <summary>敵弾の処理を委譲。</summary>
    /// <param name="transform">委譲される位置</param>
    /// <param name="spriteRenderer">委譲されるスプライト</param>
    /// <param name="rigid2D">委譲される物理演算クラス</param>
    public EnemyBullet(in Transform transform, in SpriteRenderer spriteRenderer, in Rigidbody2D rigid2D)
        : base(transform, spriteRenderer, rigid2D, 1)
    {
        _clip = spriteRenderer.sprite;
    }

    protected override Sprite clipFromImage(int countedFrames)
    {
        return _clip;
    }
}
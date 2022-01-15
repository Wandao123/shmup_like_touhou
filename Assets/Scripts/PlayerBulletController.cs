using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor;

public class PlayerBulletController : BulletController
{
    [SerializeField]
    private string _reference;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigid2D;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;  // インスペクタで設定すると、プレハブ自体に表示されなくなるので、ここで設定する。
        _rigid2D = GetComponent<Rigidbody2D>();
        _rigid2D.simulated = false;
        _mover = new PlayerBullet(transform, _spriteRenderer, _rigid2D);
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
    }
}

/// <summary>自弾クラス。</summary>
class PlayerBullet : Bullet
{
    private readonly Sprite _clip;

    /// <summary>自弾の処理を委譲。</summary>
    /// <param name="transform">委譲される位置</param>
    /// <param name="spriteRenderer">委譲されるスプライト</param>
    /// <param name="rigid2D">委譲される物理演算クラス</param>
    public PlayerBullet(in Transform transform, in SpriteRenderer spriteRenderer, in Rigidbody2D rigid2D)
        : base(transform, spriteRenderer, rigid2D, 4)
    {
        _clip = spriteRenderer.sprite;
    }

    protected override Sprite clipFromImage(int countedFrames)
    {
        return _clip;
    }
}
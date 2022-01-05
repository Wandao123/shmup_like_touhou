using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public interface IPlayer : IMover
{
    bool SlowMode { get; set; }
    Vector2 Velocity { get; set; }

    void Spawned();
}

[MoonSharpUserData]
public abstract class PlayerController : MoverController<Player>, IPlayer
{
    public bool SlowMode { get => _mover.SlowMode; set => _mover.SlowMode = value; }
    public Vector2 Velocity { get => _mover.Velocity; set => _mover.Velocity = value; }

    public virtual void Spawned()
    {
        _mover.Spawned();
    }

    // Luaに渡すために、インターフェイスで指定したメソッド以外も定義する。
    public void SetVelocity(float velX, float velY)
    {
        this.Velocity = new Vector2(velX, velY);
    }
}

public abstract class Player : Mover, IPlayer
{
    private Vector2 _velocity = Vector2.zero;  // 単位：ドット毎フレーム

    /// <summary>自機キャラクタと自機オプションの処理を委譲。</summary>
    /// <param name="transform">委譲される位置</param>
    /// <param name="spriteRenderer">委譲されるスプライト</param>
    /// <param name="rigid2D">委譲される物理演算クラス</param>
    public Player(in Transform transform, in SpriteRenderer spriteRenderer, in Rigidbody2D rigid2D)
        : base(transform, spriteRenderer, rigid2D, 0.0f, 0.5f * Mathf.PI, 1, 3, false)
    {}

    public bool SlowMode { get; set; } = false;  // 低速移動か否か。
    public virtual Vector2 Velocity
    {
        get { return _velocity; }
        set {  // 速度の方向を設定する。入力との関係上、speedとangle（極座標系）ではなくxy座標系で指定する。
            //this.velocity = value * Application.targetFrameRate * Time.deltaTime;  // 実時間を考慮する場合。
			_velocity = value;
			this.Speed = Velocity.magnitude;
			this.Angle = Mathf.Atan2(Velocity.y, Velocity.x);
        }
    }

    public override void Update()
    {
        base.Update();

        // 描画の前処理。
        if (_spriteRenderer.color.a < 1.0f) {
            var color = _spriteRenderer.color;
            if (_invincibleCounter == 0)
                color.a = 1.0f;
            else if (_invincibleCounter / 3 % 2 == 0)  // 3フレーム毎に点滅する。Sinを使ったらどうか？
                color.a = 0.0f;
            else
                color.a = 0.75f;
            _spriteRenderer.color = color;
        }
    }

    public override void OnCollisionEnter2D(in IMover mover)
    {
        if (_invincibleCounter > 0)
            return;
        Erase();
        --_hitPoint;
    }

    public void Spawned()  // 実体化関数
    {
        if (_hitPoint <= 0)
            return;
        base.spawned();
        this.Speed = 0.0f;
        this.Angle = 0.5f * Mathf.PI;
        _velocity = Vector2.zero;
        var color = _spriteRenderer.color;
        color.a = 0.75f;
        _spriteRenderer.color = color;
    }
}

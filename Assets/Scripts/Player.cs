using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer : IMover
{
    bool SlowMode { get; set; }
    Vector2 Velocity { get; set; }

    void Spawned();
}

// ***ControllerクラスはMonoBehaviourクラスを継承したもの（アタッチ可能）。
// MonoBehaviorのメソッドに加えて、***と同名のクラスと同じインターフェイスを持つ。
// Controllerと名の付く抽象クラスは***Generatorクラスに共用管理される。
public abstract class PlayerController : MonoBehaviour, IPlayer
{
    protected Player _player;

    public float Speed { get => _player.Speed; set => _player.Speed = value; }
    public float Angle { get => _player.Angle; set => _player.Angle = value; }
    public uint Damage { get => _player.Damage; }
    public int HitPoint { get => _player.HitPoint; }
    public bool SlowMode { get => _player.SlowMode; set => _player.SlowMode = value; }
    public Vector2 Velocity { get => _player.Velocity; set => _player.Velocity = value; }

    public virtual void Erase()
    {
        _player.Erase();
    }

    public bool IsEnabled()
    {
        return _player.IsEnabled();
    }

    public bool IsInvincible()
    {
        return _player.IsInvincible();
    }

    public void TurnInvincible(uint frames)
    {
        _player.TurnInvincible(frames);
    }

    public virtual void Spawned()
    {
        _player.Spawned();
    }
}

public abstract class Player : Mover, IPlayer
{
    private Vector2 _velocity = Vector2.zero;  // 単位：ドット毎フレーム

    /// <summary>自機キャラクタと自機オプションの処理を委譲。</summary>
    /// <param name="transform">委譲される位置</param>
    /// <param name="spriteRenderer">委譲されるスプライト</param>
    public Player(in Transform transform, in SpriteRenderer spriteRenderer)
        : base(transform, spriteRenderer, 0.0f, -0.5f * Mathf.PI, 1, 3, false)
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
        _enabled = false;
        --_hitPoint;
    }

    public void Spawned()  // 実体化関数
    {
        if (_hitPoint <= 0)
            return;
        base.spawned();
        this.Speed = 0.0f;
        this.Angle = -0.5f * Mathf.PI;
        _velocity = Vector2.zero;
        var color = _spriteRenderer.color;
        color.a = 0.75f;
        _spriteRenderer.color = color;
    }
}

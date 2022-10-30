using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>自機キャラクタクラス。</summary>
public class PlayerCharacterController : PlayerController
{
    [SerializeField]
    private float _highSpeed;
    [SerializeField]
    private float _lowSpeed;
    private Vector2 ScreenMinimum, ScreenMaximum;  // 画面の左下の座標と右下の座標から、画像の大きさの半分だけ縮めた座標。

    public override float Angle
    {
        get { return base.Angle; }
        set {
            value = 45f * toCardinalOrOrdinalDirectionsArea(value);
            base.Angle = value;
        }
    }

    public override float Speed
    {
        get => base.Speed;
        set => base.Speed = System.Math.Sign(value) * (SlowMode ? _lowSpeed : _highSpeed);
    }

    protected override void Start()
    {
        base.Start();
        float width = GetComponent<SpriteRenderer>().bounds.size.x;
        float height = GetComponent<SpriteRenderer>().bounds.size.y;
        ScreenMinimum = Camera.main.ViewportToWorldPoint(Vector2.zero) + (new Vector3(width, height, 0) * 0.5f);
        ScreenMaximum = Camera.main.ViewportToWorldPoint(Vector2.one) - (new Vector3(width, height, 0) * 0.5f);
    }

    public override void ManagedFixedUpdate()
    {
        base.ManagedFixedUpdate();  // 後で纏めて呼び出すと、_velocityと (_angle, _speed) がずれて、挙動が変になる。

        // 移動制限。画面外に出ないように、Moverで代入した速度を上書きする（三角関数の計算でずれるという理由もある）。この制限は速度を変化させた場合のみに適用され、位置を直接変える場合は制限しないことに注意。復活処理との兼ね合いのためである。
        if (this._velocity == Vector2.zero)
            return;
        var nextPosition = this.Position + this._velocity;
        var velocity = this._velocity;
        if (nextPosition.x < ScreenMinimum.x)
            velocity.x = ScreenMinimum.x - this.Position.x;
        else if (nextPosition.x > ScreenMaximum.x)
            velocity.x = ScreenMaximum.x - this.Position.x;
        if (nextPosition.y < ScreenMinimum.y)
            velocity.y = ScreenMinimum.y - this.Position.y;
        else if (nextPosition.y > ScreenMaximum.y)
            velocity.y = ScreenMaximum.y - this.Position.y;
        _rigid2D.velocity = velocity / Time.deltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
    }

    // -22.5° を基準に円を8等分したとき、角度angleがどの区間に属するか。
    protected uint toCardinalOrOrdinalDirectionsArea(float angle)
    {
        return (uint)System.Math.Truncate(Mathf.Repeat(angle + 22.5f, 360f) / 45f);
    }
}

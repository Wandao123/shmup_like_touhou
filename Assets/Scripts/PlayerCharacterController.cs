using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>自機キャラクタクラス。</summary>
public class PlayerCharacterController : PlayerController
{
    [SerializeField]
    private float _highSpeed;
    [SerializeField]
    private float _lowSpeed;
    private Vector2 ScreenMinimum, ScreenMaximum;  // 画面の左下の座標と右下の座標から、画像の大きさの半分だけ縮めた座標。

    public override Vector2 Velocity
    {
        get => base.Velocity;
        set => base.Velocity = value.normalized * (SlowMode ? _lowSpeed : _highSpeed);
    }

    protected override void Awake()
    {
        base.Awake();
        float width = GetComponent<SpriteRenderer>().bounds.size.x;
        float height = GetComponent<SpriteRenderer>().bounds.size.y;
        ScreenMinimum = Camera.main.ViewportToWorldPoint(Vector2.zero) + (new Vector3(width, height, 0) * 0.5f);
        ScreenMaximum = Camera.main.ViewportToWorldPoint(Vector2.one) - (new Vector3(width, height, 0) * 0.5f);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // 移動制限。画面外に出ないように、Moverで代入した速度を上書きする（三角関数の計算でずれるという理由もある）。この制限は速度を変化させた場合のみに適用され、位置を直接変える場合は制限しないことに注意。復活処理との兼ね合いのためである。
        if (this.Velocity == Vector2.zero)
            return;
        var nextPosition = this.Position + this.Velocity;
        var velocity = this.Velocity;
        if (nextPosition.x < ScreenMinimum.x)
            velocity.x = ScreenMinimum.x - this.Position.x;
        else if (nextPosition.x > ScreenMaximum.x)
            velocity.x = ScreenMaximum.x - this.Position.x;
        if (nextPosition.y < ScreenMinimum.y)
            velocity.y = ScreenMinimum.y - this.Position.y;
        else if (nextPosition.y > ScreenMaximum.y)
            velocity.y = ScreenMaximum.y - this.Position.y;
        _rigid2D.velocity = velocity / Time.fixedDeltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
    }
}

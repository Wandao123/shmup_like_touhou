using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mover : MonoBehaviour
{
    [SerializeField]
    protected SpriteRenderer spriteRenderer;
    [SerializeField]
    protected Camera mainCamera;
    protected float speed;  // 単位：ドット毎フレーム
    public float Speed { get; set; }
    protected float angle;  // x軸を基準とした角度。時計回りの方向を正とする。
    public float Angle
    {
        get { return angle; }
        set {
            this.angle = Mathf.Repeat(value, 2f * Mathf.PI);  // 負の値が渡されても、0 <= angle < 2 pi になるように変換する。
        }
    }
    protected uint damage;  // 衝突時に相手に与えるダメージ。
    public uint Damage { get; }
    protected int hitPoint;  // 体力。「消滅」と「撃破」とを区別するために弾でも設定が必須。外部からこれを参照する以外に、OnCollideに返り値を持たせる実装でも良いかもしれない。
    public int HitPoint { get; }
    protected uint invincibleCounter;  // 無敵状態になっている残りのフレーム数。

    public bool IsInvincible()
    {
        return (invincibleCounter > 0) ? true : false;
    }

    public void TurnInvincible(uint frames)
    {
        invincibleCounter = frames;
    }

    protected virtual void spawned()
    {
        enabled = true;
	    existingCounter = 0;
    }

    protected virtual void disableIfOutside()
    {
        ++existingCounter;
        if (!isInside())
            enabled = false;
    }

    /// <summary>現在のフレームにおける画像の切り取り位置を返す。</summary>
    /// <param name="currentFrames">現在までのフレーム数</param>
    /// <returns>切り取る矩形</returns>
    protected Func<int, Sprite> clipFromImage;

    private uint existingCounter;  // enabledがtrueになってからのフレーム数。

    /// <summary>オブジェクトが画面内に存在するか？</summary>
    /// <returns>存在すれば真、しなければ偽</returns>
    /// <remarks>
    /// 生成されてから最初の1秒（60フレーム）は判定せずに真を返す。画面外でも0.1秒（6フレーム）以内に画面内に戻れば真を返す。なお、ここでいう「存在」とはオブジェクトの画像の一部でも画面内にあることを意味する。
    /// </remarks>
	private bool isInside()
    {
        if (existingCounter < 60)
            return true;
        // HACK: 定数なので、本来は初期化で代入したい。Unityの継承との兼ね合いをどうするべきか？
        float width = spriteRenderer.bounds.size.x;
        float height = spriteRenderer.bounds.size.y;
        Vector2 minimum = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)) - (new Vector3(width, height, 0) * 0.5f);
        Vector2 maximum = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)) + (new Vector3(width, height, 0) * 0.5f);
        if (transform.position.x < minimum.x || transform.position.x > maximum.x
                || transform.position.y < minimum.y || transform.position.y > maximum.y) {
            if (existingCounter > 66)
                return false;
            else
                return true;
        } else {
            existingCounter = 60;
            return true;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class SpriteAnimator : MonoBehaviour
{
    [SerializeField]
    private string _reference;
    protected SpriteRenderer _spriteRenderer;
    //private AssetReferenceGameObject _reference;  // プレハブは参照できるが、スプライトは何故かできない。

    protected virtual void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
    }

    protected virtual void OnDisable()
    {
        _spriteRenderer.enabled = false;
    }

    protected virtual void OnEnable()
    {
        _spriteRenderer.enabled = true;
    }

    protected virtual void Update()
    {
        _spriteRenderer.sprite = clipFromImage(Time.frameCount);  // HACK: ポーズの時に狂う？
    }

    /// <summary>現在のフレームにおける切り取られた画像を返す。</summary>
    /// <param name="currentFrames">現在までのフレーム数</param>
    /// <returns>切り取られたスプライト</returns>
    protected abstract Sprite clipFromImage(int countedFrames);

    /// <summary>直前のフレームで被弾しているか否かを判定。</summary>
    /// <param name="collisionHandler">判定する対象</param>
    /// <returns>被弾していれば真を返すラムダ式。</returns>
    protected Func<bool> damagedDetector(ICollisionHandler collisionHandler)
    {
        int previousHitPoint = collisionHandler.HitPoint;
        return () =>
        {
            if (collisionHandler.HitPoint < previousHitPoint)
            {
                previousHitPoint = collisionHandler.HitPoint;
                return true;
            }
            else
            {
                return false;
            }
        };
    }

    // 参考：https://tsubakit1.hateblo.jp/entry/2019/06/18/000323
    //       https://light11.hatenadiary.com/entry/2019/12/30/222302
    //       https://light11.hatenadiary.com/entry/2019/03/06/002800
    //       https://light11.hatenadiary.com/entry/2021/04/13/194929
    protected IList<Sprite> loadSprite()
    {
        var spritesList = Addressables.LoadAssetAsync<IList<Sprite>>(_reference).WaitForCompletion();  // 不要なものも含めて、画像のスプライトを全て取得。
        spritesList = spritesList
            // HACK: 必要なスクリプトを抽出するもっと良い方法？
            .Where(sprite => sprite.name.Contains(this.name.Replace("(Clone)", "_")))  // 必要なものだけ抽出。ここの処理のために、アタッチされるオブジェクト名を必要なスプライトのオブジェクト名に含める必要がある。
            .OrderBy(sprite => int.Parse(Regex.Replace(sprite.name, @"[^0-9]", "")))  // スプライトがバラバラの順番でに読み込まれる可能性があるため、並び替える。
            .ToList<Sprite>();
        return spritesList;
    }
}

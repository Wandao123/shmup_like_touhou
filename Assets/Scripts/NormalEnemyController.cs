using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor;

public class NormalEnemyController : EnemyController
{
    [SerializeField]
    private string _reference;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigid2D;

    void Awake()
    {
        var temp = Addressables.LoadAssetAsync<IList<Sprite>>(_reference).WaitForCompletion();  // 不要なものも含めて、画像のスプライトを全て取得。
        //var original = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);  // インスタンスからプレハブを取得する方法だが、プレイ中は必ずNullが帰ってきてしまう。
        var spritesList = temp.Where(sprite => sprite.name.Contains(this.name.Replace("(Clone)", ""))).ToList<Sprite>();  // 必要なものだけ抽出。ここの処理のために、アタッチされるオブジェクト名を必要なスプライトのオブジェクト名に含める必要がある。
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;  // インスペクタで設定すると、プレハブ自体に表示されなくなるので、ここで設定する。
        _rigid2D = GetComponent<Rigidbody2D>();
        _rigid2D.simulated = false;
        _mover = new NormalEnemy(transform, _spriteRenderer, _rigid2D, spritesList);
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

/// <summary>雑魚敵クラス。</summary>
class NormalEnemy : Enemy
{
    private Sprite[,] _clips;
    private Func<int, Sprite> _clipFramImageFunc;
    private readonly Vector2 ScreenMinimum, ScreenMaximum;  // 画面の左下の座標と右下の座標から、画像の大きさの半分だけ縮めた座標。

    /// <summary>雑魚敵の処理を委譲。</summary>
    /// <param name="transform">委譲される位置</param>
    /// <param name="spriteRenderer">委譲されるスプライト</param>
    /// <param name="rigid2D">委譲される物理演算クラス</param>
    /// <param name="spritesList">切り分けられた画像のリスト</param>
    public NormalEnemy(in Transform transform, in SpriteRenderer spriteRenderer, in Rigidbody2D rigid2D, in IList<Sprite> spritesList)
        : base(transform, spriteRenderer, rigid2D)
    {
        _clips = new Sprite[3, 3];
        for (var i = 0; i < _clips.GetLength(0); i++)
            for (var j = 0; j < _clips.GetLength(1); j++)
                _clips[i, j] = spritesList[_clips.GetLength(1) * i + j];
    }

    protected override Sprite clipFromImage(int countedFrames)
    {
        const int DelayFrames = 6;
        const int NumSlice = 3;
        if (Speed > 0.0) {  // 30度ずつの領域に分けて、画像を切り換える。
            if (Angle == Mathf.PI / 2.0f)
			    return _clips[0, (countedFrames / DelayFrames) % NumSlice];  // どう設定するのが適切なのか？
            else if (Angle > Mathf.PI * 2.0f && Angle < Mathf.PI * 13.0f / 12.0f)
                return _clips[1, 2];
            else if (Angle > Mathf.PI * 13.0f / 12.0f && Angle <= Mathf.PI * 5.0f / 4.0f)
                return _clips[1, 1];
            else if (Angle > Mathf.PI * 5.0f / 4.0f && Angle <= Mathf.PI * 17.0f / 12.0f)
                return _clips[1, 0];
            else if (Angle >= Mathf.PI * 17.0f / 12.0f && Angle <= Mathf.PI * 19.0f / 12.0f)
                return _clips[0, (countedFrames / DelayFrames) % NumSlice];
            else if (Angle >= Mathf.PI * 19.0f / 12.0f && Angle < Mathf.PI * 7.0f / 4.0f)
                return _clips[2, 0];
            else if (Angle >= Mathf.PI * 7.0f / 4.0f && Angle < Mathf.PI * 23.0f / 12.0f)
                return _clips[2, 1];
            else
                return _clips[2, 2];
        }
        else {
            return _clips[0, (countedFrames / DelayFrames) % NumSlice];
        }
    }
}
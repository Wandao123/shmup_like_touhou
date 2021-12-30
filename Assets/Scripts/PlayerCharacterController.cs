using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerCharacterController : PlayerController
{
    [SerializeField]
    private float _highSpeed = 0.0f;
    [SerializeField]
    private float _lowSpeed = 0.0f;
    [SerializeField]
    private string _path = "";
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigid2D;

    // 参考：https://tsubakit1.hateblo.jp/entry/2019/06/18/000323
    //       https://light11.hatenadiary.com/entry/2019/12/30/222302
    //       https://light11.hatenadiary.com/entry/2019/03/06/002800
    //       https://light11.hatenadiary.com/entry/2021/04/13/194929
    void Awake()
    {
        var spritesList = Addressables.LoadAssetAsync<IList<Sprite>>(_path).WaitForCompletion();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;  // インスペクタで設定すると、プレハブ自体に表示されなくなるので、ここで設定する。
        _rigid2D = GetComponent<Rigidbody2D>();
        _rigid2D.simulated = false;
        _player = new PlayerCharacter(transform, _spriteRenderer, _rigid2D, _highSpeed, _lowSpeed, spritesList);
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (_player.IsEnabled())
            _player.Update();
    }

    void FixedUpdate()
    {
        _player.FixedUpdate();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _player.OnCollisionEnter2D(collision.gameObject.GetComponent<PlayerCharacterController>() as IMover);
    }

    public override void Erase()
    {
        _spriteRenderer.enabled = false;
        _rigid2D.simulated = false;
        base.Erase();
    }

    public override void Spawned()
    {
        _spriteRenderer.enabled = true;
        _rigid2D.simulated = true;
        base.Spawned();
    }
}

/// <summary>自機キャラクタクラス。</summary>
class PlayerCharacter : Player
{
    private Sprite[,] _clips;
    private float _highSpeed;
    private float _lowSpeed;
    private Func<int, Sprite> _clipFramImageFunc;
    private readonly Vector2 ScreenMinimum, ScreenMaximum;  // 画面の左下の座標と右下の座標から、画像の大きさの半分だけ縮めた座標。

    /// <summary>自機キャラクタの処理を委譲。</summary>
    /// <param name="transform">委譲される位置</param>
    /// <param name="spriteRenderer">委譲されるスプライト</param>
    /// <param name="rigid2D">委譲される物理演算クラス</param>
    /// <param name="highSpeed">高速移動時の速さ（単位：ドット毎フレーム）</param>
    /// <param name="lowSpeed">低速移動時の速さ（単位：ドット毎フレーム）</param>
    /// <param name="spritesList">切り分けられた画像のリスト</param>
    public PlayerCharacter(in Transform transform, in SpriteRenderer spriteRenderer, in Rigidbody2D rigid2D, float highSpeed, float lowSpeed, in IList<Sprite> spritesList)
        : base(transform, spriteRenderer, rigid2D)
    {
        _highSpeed = highSpeed;
        _lowSpeed = lowSpeed;
        _clips = new Sprite[3, 5];
        for (var i = 0; i < _clips.GetLength(0); i++)
            for (var j = 0; j < _clips.GetLength(1); j++)
                _clips[i, j] = spritesList[_clips.GetLength(1) * i + j];
        _clipFramImageFunc = clipFromImageClosure();
        float width = spriteRenderer.bounds.size.x;
        float height = spriteRenderer.bounds.size.y;
        ScreenMinimum = Camera.main.ViewportToWorldPoint(new Vector2(0, 0)) + (new Vector3(width, height, 0) * 0.5f);
        ScreenMaximum = Camera.main.ViewportToWorldPoint(new Vector2(1, 1)) - (new Vector3(width, height, 0) * 0.5f);
    }

    public override Vector2 Velocity
    {
        get => base.Velocity;
        set => base.Velocity = value.normalized * (SlowMode ? _lowSpeed : _highSpeed);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 速度の更新と移動制限。速度の更新はMoverでも行われるが、どうしても三角関数の計算でずれるので、自機クラスではここで再設定する。移動制限は速度を変化させた場合のみで、位置を直接変える場合は制限しないことに注意。復活処理との兼ね合いである。
        var nextPosition = this.Position + this.Velocity;
        var velocity = this.Velocity;
        if (nextPosition.x < ScreenMinimum.x || nextPosition.x > ScreenMaximum.x)
            velocity.x = 0.0f;
        if (nextPosition.y < ScreenMinimum.y || nextPosition.y > ScreenMaximum.y)
            velocity.y = 0.0f;
        _rigid2D.velocity = velocity / Time.fixedDeltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
 
        /*_rigid2D.MovePosition(_rigid2D.position + new Vector2(
            Mathf.Clamp(_rigid2D.position.x, ScreenMinimum.x, ScreenMaximum.x),
            Mathf.Clamp(_rigid2D.position.y, ScreenMinimum.y, ScreenMaximum.y)
        ));*/
    }

    protected override Sprite clipFromImage(int countedFrames)
    {
        return _clipFramImageFunc(countedFrames);
    }

    private Func<int, Sprite> clipFromImageClosure()
    {
        const int DelayFrames = 3;  // 左右移動の際に次の画像に切り替わるまでのフレーム数。
        const int NumSlice = 5;     // 停止時、左移動、右移動における各変化のコマ数。
        const int Period = 6;       // 停止時などに画像を繰り返す周期。
        int level = 0;              // 左右に何フレーム進んでいるか表すフラグ。-(DelayFrames * NumSlice - 1) .. DelayFrames * NumSlice - 1 の範囲を動く。
        return (int countedFrames) =>
        {
            if (Velocity.x < 0.0)
                level = Mathf.Max(level - 1, -(DelayFrames * NumSlice - 1));
            else if (Velocity.x > 0.0)
                level = Mathf.Min(level + 1, DelayFrames * NumSlice - 1);
            else
                level = (level != 0) ? (level - level / Mathf.Abs(level)) : 0;

            if (level == 0)
                return _clips[0, (countedFrames / Period) % NumSlice];
            else if (level == -(DelayFrames * NumSlice - 1))  // 4コマ目と5コマ目だけ繰り返し。
                return _clips[1, (countedFrames / Period) % 2 + 3];
            else if (level == DelayFrames * NumSlice - 1)
                return _clips[2, (countedFrames / Period) % 2 + 3];
            else if (level < 0)
                return _clips[1, -level / DelayFrames];
            else
                return _clips[2, level / DelayFrames];
        };
    }
}
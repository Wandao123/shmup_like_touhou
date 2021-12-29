using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerCharacterController : MonoBehaviour, IMover, IPlayer
{
    [SerializeField]
    private float _highSpeed = 0.0f;
    [SerializeField]
    private float _lowSpeed = 0.0f;
    [SerializeField]
    private string _path = "";
    private PlayerCharacter _playerCharacter;
    private Rigidbody2D _rigid2D;

    public float Speed { get => _playerCharacter.Speed; set => _playerCharacter.Speed = value; }
    public float Angle { get => _playerCharacter.Angle; set => _playerCharacter.Angle = value; }
    public uint Damage { get => _playerCharacter.Damage; }
    public int HitPoint { get => _playerCharacter.HitPoint; }
    public bool SlowMode { get => _playerCharacter.SlowMode; set => _playerCharacter.SlowMode = value; }
    public Vector2 Velocity { get => _playerCharacter.Velocity; set => _playerCharacter.Velocity = value; }

    // 参考：https://tsubakit1.hateblo.jp/entry/2019/06/18/000323
    //       https://light11.hatenadiary.com/entry/2019/12/30/222302
    //       https://light11.hatenadiary.com/entry/2019/03/06/002800
    //       https://light11.hatenadiary.com/entry/2021/04/13/194929
    void Awake()
    {
        var spritesList = Addressables.LoadAssetAsync<IList<Sprite>>(_path).WaitForCompletion();
        _playerCharacter = new PlayerCharacter(transform, GetComponent<SpriteRenderer>(), _highSpeed, _lowSpeed, spritesList);
        _rigid2D = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

    }

    void Update()
    {
        _playerCharacter.Update();
    }

    void FixedUpdate()
    {
        // HACK: 自機の操作では跳ね返るような挙動をするので、Translateを使った方が良い？
        _rigid2D.velocity = _playerCharacter.Velocity / Time.fixedDeltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
        //transform.Translate(_playerCharacter.Velocity);
        _playerCharacter.FixedUpdate();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _playerCharacter.OnCollisionEnter2D(collision.gameObject.GetComponent<PlayerCharacterController>() as IMover);
    }

    public void Erase()
    {
        _playerCharacter.Erase();
    }

    public bool IsEnabled()
    {
        return _playerCharacter.IsEnabled();
    }

    public bool IsInvincible()
    {
        return _playerCharacter.IsInvincible();
    }

    public void TurnInvincible(uint frames)
    {
        _playerCharacter.TurnInvincible(frames);
    }

    public void Spawned()
    {
        _playerCharacter.Spawned();
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
    /// <param name="highSpeed">高速移動時の速さ（単位：ドット毎フレーム）</param>
    /// <param name="lowSpeed">低速移動時の速さ（単位：ドット毎フレーム）</param>
    /// <param name="spritesList">切り分けられた画像のリスト</param>
    public PlayerCharacter(in Transform transform, in SpriteRenderer spriteRenderer, float highSpeed, float lowSpeed, in IList<Sprite> spritesList)
        : base(transform, spriteRenderer)
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

        // 移動制限。ただし、速度を変化させた場合のみで、位置を直接変える場合は制限しない。
        _transform.position = new Vector2(
            Mathf.Clamp(_transform.position.x, ScreenMinimum.x, ScreenMaximum.x),
            Mathf.Clamp(_transform.position.y, ScreenMinimum.y, ScreenMaximum.y)
        );
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
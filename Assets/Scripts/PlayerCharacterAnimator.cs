using System;
using UnityEngine;

public class PlayerCharacterAnimator : SpriteAnimator
{
    private Sprite[,] _clips;
    private Func<int, Sprite> _clipFromImageFunc;
    private IInvincibility _invincibility;
    private IPlayerPhysicalState _physicalState;

    protected override void Awake()
    {
        base.Awake();
        var spritesList = loadSprite();
        _clips = new Sprite[3, 5];
        for (var i = 0; i < _clips.GetLength(0); i++)
            for (var j = 0; j < _clips.GetLength(1); j++)
                _clips[i, j] = spritesList[_clips.GetLength(1) * i + j];
        _clipFromImageFunc = clipFromImageClosure();
        _invincibility = GetComponent<IInvincibility>();
        _physicalState = GetComponent<IPlayerPhysicalState>();
    }

    public override void ManagedUpdate()
    {
        base.ManagedUpdate();

        // 描画の前処理。
        if (_spriteRenderer.color.a < 1.0f)
        {
            var color = _spriteRenderer.color;
            if (!_invincibility.IsInvincible())
                color.a = 1.0f;
            else if (_invincibility.InvincibleCount / 3 % 2 == 0)  // 3フレーム毎に点滅する。Sinを使ったらどうか？
                color.a = 0.0f;
            else
                color.a = 0.75f;
            _spriteRenderer.color = color;
        }
    }

    protected override Sprite clipFromImage(int countedFrames)
    {
        return _clipFromImageFunc(countedFrames);
    }

    private Func<int, Sprite> clipFromImageClosure()
    {
        const int DelayFrames = 3;  // 左右移動の際に次の画像に切り替わるまでのフレーム数。
        const int NumSlice = 5;     // 停止時、左移動、右移動における各変化のコマ数。
        const int Period = 6;       // 停止時などに画像を繰り返す周期。
        int level = 0;              // 左右に何フレーム進んでいるか表すフラグ。-(DelayFrames * NumSlice - 1) .. DelayFrames * NumSlice - 1 の範囲を動く。
        return (int countedFrames) =>
        {
            if (_physicalState.Speed > 0.0f && (_physicalState.Angle > 112.5f && _physicalState.Angle < 247.5f))  // x軸負の向きに移動している場合。360 / 16 = 22.5 に注意。
                level = Mathf.Max(level - 1, -(DelayFrames * NumSlice - 1));
            else if (_physicalState.Speed > 0.0f && (_physicalState.Angle < 67.5f || _physicalState.Angle > 292.5f))  // x軸正の向きに移動している場合。360 / 16 = 22.5 に注意。
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

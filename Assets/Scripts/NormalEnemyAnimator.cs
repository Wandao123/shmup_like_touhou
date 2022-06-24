using System;
using UnityEngine;

public class NormalEnemyAnimator : SpriteAnimator
{
    private Sprite[,] _clips;
    private Func<bool> _datectDamaged;
    private IPhysicalState _physicalState;

    protected override void Awake()
    {
        base.Awake();
        var spritesList = loadSprite();
        _clips = new Sprite[3, 3];
        for (var i = 0; i < _clips.GetLength(0); i++)
            for (var j = 0; j < _clips.GetLength(1); j++)
                _clips[i, j] = spritesList[_clips.GetLength(1) * i + j];
        _datectDamaged = damagedDetector(GetComponent<ICollisionHandler>());
        _physicalState = GetComponent<IPhysicalState>();
    }

    public override void ManagedUpdate()
    {
        base.ManagedUpdate();
        if (_datectDamaged())  // 点滅する。Sinを使ったらどうか？
        {
            _spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            _spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f);
        }
    }

    protected override Sprite clipFromImage(int countedFrames)
    {
        const int DelayFrames = 6;
        const int NumSlice = 3;
        if (_physicalState.Speed > 0.0)
        {  // 30度ずつの領域に分けて、画像を切り換える。
            if (_physicalState.Angle == 90f)
                return _clips[0, (countedFrames / DelayFrames) % NumSlice];  // どう設定するのが適切なのか？
            else if (_physicalState.Angle > 90f && _physicalState.Angle < 195f)
                return _clips[1, 2];
            else if (_physicalState.Angle > 195f && _physicalState.Angle <= 225f)
                return _clips[1, 1];
            else if (_physicalState.Angle > 225f && _physicalState.Angle <= 255f)
                return _clips[1, 0];
            else if (_physicalState.Angle >= 255f && _physicalState.Angle <= 285f)
                return _clips[0, (countedFrames / DelayFrames) % NumSlice];
            else if (_physicalState.Angle >= 285f && _physicalState.Angle < 315f)
                return _clips[2, 0];
            else if (_physicalState.Angle >= 315f && _physicalState.Angle < 345f)
                return _clips[2, 1];
            else  // if (_physicalState.Angle >= 345f && _physicalState.Angle < 360f)
                return _clips[2, 2];
        }
        else
        {
            return _clips[0, (countedFrames / DelayFrames) % NumSlice];
        }
    }
}

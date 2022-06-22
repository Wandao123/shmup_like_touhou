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
            if (_physicalState.Angle == Mathf.PI / 2.0f)
                return _clips[0, (countedFrames / DelayFrames) % NumSlice];  // どう設定するのが適切なのか？
            else if (_physicalState.Angle > Mathf.PI * 6.0f / 12.0f && _physicalState.Angle < Mathf.PI * 13.0f / 12.0f)
                return _clips[1, 2];
            else if (_physicalState.Angle > Mathf.PI * 13.0f / 12.0f && _physicalState.Angle <= Mathf.PI * 15.0f / 12.0f)
                return _clips[1, 1];
            else if (_physicalState.Angle > Mathf.PI * 15.0f / 12.0f && _physicalState.Angle <= Mathf.PI * 17.0f / 12.0f)
                return _clips[1, 0];
            else if (_physicalState.Angle >= Mathf.PI * 17.0f / 12.0f && _physicalState.Angle <= Mathf.PI * 19.0f / 12.0f)
                return _clips[0, (countedFrames / DelayFrames) % NumSlice];
            else if (_physicalState.Angle >= Mathf.PI * 19.0f / 12.0f && _physicalState.Angle < Mathf.PI * 21.0f / 12.0f)
                return _clips[2, 0];
            else if (_physicalState.Angle >= Mathf.PI * 21.0f / 12.0f && _physicalState.Angle < Mathf.PI * 23.0f / 12.0f)
                return _clips[2, 1];
            else
                return _clips[2, 2];
        }
        else
        {
            return _clips[0, (countedFrames / DelayFrames) % NumSlice];
        }
    }
}

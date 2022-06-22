using UnityEngine;

public class BulletAnimator : SpriteAnimator
{
    private Sprite _clip;

    protected override void Awake()
    {
        base.Awake();
        _clip = _spriteRenderer.sprite;
    }

    protected override Sprite clipFromImage(int countedFrames)
    {
        return _clip;
    }
}

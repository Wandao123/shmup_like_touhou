using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInvincibility
{
    uint InvincibleCount { get; }

    bool IsInvincible();
    void TurnInvincible(uint frames);
}

/// <summary>無敵状態を司るクラス</summary>
public class Invincibility : MonoBehaviour, IInvincibility, IManagedBehaviour
{
    private uint _invincibleCounter = 0;  // 無敵状態になっている残りのフレーム数。

    public uint InvincibleCount { get => _invincibleCounter; }

    public void ManagedFixedUpdate()
    {
        _invincibleCounter = (_invincibleCounter > 0) ? _invincibleCounter - 1 : 0;
    }

    public bool IsInvincible()
    {
        return _invincibleCounter > 0;
    }

    public void TurnInvincible(uint frames)
    {
        _invincibleCounter = frames;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActivity
{
    void Erase();
    bool IsEnabled();
}

interface IManagedBehaviour
{
    void ManagedFixedUpdate();
}

public class Activity : MonoBehaviour, IActivity
{
    private bool _enabled = false;  // パラメータを更新するか否かのフラグ。

    public void Erase()
    {
        GetComponent<SpriteAnimator>().enabled = false;
        GetComponent<MoverController>().enabled = false;
        _enabled = false;
    }

    public bool IsEnabled()
    {
        return _enabled;
    }

    public void Spawned()
    {
        GetComponent<SpriteAnimator>().enabled = true;
        GetComponent<MoverController>().enabled = true;
        _enabled = true;
    }
}
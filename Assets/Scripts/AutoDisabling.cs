using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>画面外にあるために無効にするか否かを判定</summary>
public class AutoDisabling : MonoBehaviour, IManagedBehaviour
{
    private uint _existingCounter = 0;  // enabledがtrueになってからのフレーム数。
    private IActivity _activity;
    private IPhysicalState _physicalState;
    private Vector2 ScreenMinimum, ScreenMaximum;  // 画面の左下の座標と右下の座標から、画像の大きさの半分だけ拡げた座標。

    private void Awake()
    {
        _activity = GetComponent<IActivity>();
        _physicalState = GetComponent<IPhysicalState>();
    }

    private void Start()
    {
        float width = GetComponent<SpriteRenderer>().bounds.size.x;
        float height = GetComponent<SpriteRenderer>().bounds.size.y;
        ScreenMinimum = Camera.main.ViewportToWorldPoint(Vector2.zero) - (new Vector3(width, height, 0) * 0.5f);
        ScreenMaximum = Camera.main.ViewportToWorldPoint(Vector2.one) + (new Vector3(width, height, 0) * 0.5f);
    }

    public void ManagedFixedUpdate()
    {
        ++_existingCounter;
        if (!isInside())
            _activity.Erase();
    }

    public void ManagedUpdate() {}

    private void OnEnable()
    {
	    _existingCounter = 0;
    }

    /// <summary>オブジェクトが画面内に存在するか？</summary>
    /// <returns>存在すれば真、しなければ偽</returns>
    /// <remarks>
    /// 生成されてから最初の1秒（60フレーム）は判定せずに真を返す。画面外でも0.1秒（6フレーム）以内に画面内に戻れば真を返す。なお、ここでいう「存在」とはオブジェクトの画像の一部でも画面内にあることを意味する。
    /// </remarks>
	private bool isInside()
    {
        if (_existingCounter < 60)
            return true;
        if (_physicalState.Position.x < ScreenMinimum.x || _physicalState.Position.x > ScreenMaximum.x
                || _physicalState.Position.y < ScreenMinimum.y || _physicalState.Position.y > ScreenMaximum.y)
        {
            if (_existingCounter > 66)
                return false;
            else
                return true;
        }
        else
        {
            _existingCounter = 60;
            return true;
        }
    }
}

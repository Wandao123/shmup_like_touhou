using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>物理的な位置と速度を司る。SpeedとAngleは極座標での表示に対応し、VelocityはCartesian座標での表示に対応する。</summary>
public interface IPhysicalState
{
    Vector2 Position { get; set; }
    float Speed { get; set; }  // 単位：ドット毎フレーム
    float Angle { get; set; }  // x軸を基準とした角度。時計回りの方向を正とする。
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLua;

[LuaCallCSharp]
public enum EnemyID
{
    SmallRedFairy,
    SmallBlueFairy
};

public class EnemyGenerator : MoverGenerator<EnemyController, EnemyID>
{
    void Awake()
    {
        // 予めオブジェクトを生成しておく場合はここに記述。
    }
}

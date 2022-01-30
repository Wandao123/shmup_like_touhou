using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyID
{
    SmallRedFairy,
    SmallBlueFairy
};

public class EnemyManager : MoverManager<EnemyController, EnemyID>
{
    public EnemyManager()
    {
        // 予めオブジェクトを生成しておく場合はここに記述。
    }
}

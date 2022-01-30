using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyID
{
    SmallRedFairy,
    SmallBlueFairy
};

public class EnemyManager : MoverManager<Enemy, EnemyController, EnemyID>
{
    private void Awake()
    {
        // 予めオブジェクトを生成しておく場合はここに記述。
    }

    protected override Enemy makeWrapper(GameObject go, EnemyID id)
    {
        return new Enemy(go, id);
    }

    protected override bool idEquals(EnemyID id1, EnemyID id2)
    {
        return id1 == id2;
    }
}

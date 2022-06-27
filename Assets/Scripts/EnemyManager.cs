using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyID
{
    SmallRedFairy,
    SmallBlueFairy
};

[System.Serializable]
public class PreloadedEnemies : Serialize.TableBase<EnemyID, uint, PreloadedEnemiesPair> {}

[System.Serializable]
public class PreloadedEnemiesPair : Serialize.KeyAndValue<EnemyID, uint>
{
    public PreloadedEnemiesPair(EnemyID id, uint count) : base(id, count) {}
}

public class EnemyManager : MoverManager<Enemy, EnemyController, EnemyID>
{
    public EnemyManager(in Transform gameDirector, in Dictionary<EnemyID, uint> preloadedObjectsTable) : base(gameDirector, preloadedObjectsTable)
    {
        // 予めオブジェクトを生成しておく場合はここに記述。
    }

    protected override Enemy makeWrapper(GameObject go, EnemyID id)
    {
        return new Enemy(go, id);
    }

    protected override bool equal(EnemyID id1, EnemyID id2)
    {
        return id1 == id2;
    }
}

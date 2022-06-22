using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BulletID
{
    // 敵弾。
    LargeRedBullet,
    LargeBlueBullet,
    MiddleRedBullet,
    MiddleBlueBullet,
    SmallRedBullet,
    SmallBlueBullet,
    TinyRedBullet,
    TinyBlueBullet,
    ScaleRedBullet,
    ScaleBlueBullet,
    RiceRedBullet,
    RiceBlueBullet,
    DummyEnemyBullet,
    // 自弾。
    ReimuNormalBullet,
    MarisaNormalBullet,
    SanaeNormalBullet
};

[System.Serializable]
public class PreloadedBullets : Serialize.TableBase<BulletID, uint, PreloadedBulletsPair> {}

[System.Serializable]
public class PreloadedBulletsPair : Serialize.KeyAndValue<BulletID, uint>
{
    public PreloadedBulletsPair(BulletID id, uint count) : base(id, count) {}
}

public class BulletManager : MoverManager<Bullet, BulletController, BulletID>
{
    public BulletManager()
    {
        // 予めオブジェクトを生成しておく場合はここに記述。
        /*var bulletsList = new List<Bullet>();
        foreach (var pair in _preloadedObjects.GetTable())
            for (var i = 1; i <= pair.Value; i++)
            {
                var bullet = GenerateObject(pair.Key, new Vector2(0, 300));
                bullet.Shot(0.0f, 0.0f);
                bulletsList.Add(bullet);
            }
        bulletsList.Select(bullet => { bullet.Erase(); return bullet; });*/
    }

    protected override Bullet makeWrapper(GameObject go, BulletID id)
    {
        return new Bullet(go, id);
    }

    protected override bool equal(BulletID id1, BulletID id2)
    {
        return id1 == id2;
    }
}

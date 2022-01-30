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

public class BulletManager : MoverManager<BulletController, BulletID>
{
    [SerializeField]
    private PreloadedBullets _preloadedObjects;

    private void Awake()
    {
        // 予めオブジェクトを生成しておく場合はここに記述。
        var bulletsList = new List<Bullet>();
        //foreach (BulletID id in System.Enum.GetValues(typeof(BulletID)))
        /*foreach (BulletID id in new HashSet<BulletID>() {BulletID.SmallBlueBullet, BulletID.SmallRedBullet})
            for (var i = 1; i <= 1000; i++)
            {
                var bullet = GenerateObject(id, new Vector2(0, 300));
                bullet.Shot(0.0f, 0.0f);
                bulletsList.Add(bullet);
            }*/
        foreach (var pair in _preloadedObjects.GetTable())
            for (var i = 1; i <= pair.Value; i++)
            {
                var bullet = new Bullet(GenerateObject(pair.Key, new Vector2(0, 300)));
                bullet.Shot(0.0f, 0.0f);
                bulletsList.Add(bullet);
            }
        bulletsList.Select(bullet => { bullet.Erase(); return bullet; });
    }
}

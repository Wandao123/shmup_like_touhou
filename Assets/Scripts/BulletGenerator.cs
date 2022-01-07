using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

// 参考：https://qiita.com/k_yanase/items/fb64ccfe1c14567a907d
namespace Serialize {
    /// <summary>
    /// テーブルの管理クラス
    /// </summary>
    [System.Serializable]
    public class TableBase<TKey, TValue, Type> where Type : KeyAndValue<TKey, TValue>{
        [SerializeField]
        private List<Type> list;
        private Dictionary<TKey, TValue> table;


        public Dictionary<TKey, TValue> GetTable () {
            if (table == null) {
                table = ConvertListToDictionary(list);
            }
            return table;
        }

        /// <summary>
        /// Editor Only
        /// </summary>
        public List<Type> GetList () {
            return list;
        }

        static Dictionary<TKey, TValue> ConvertListToDictionary (List<Type> list) {
            Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue> ();
            foreach(KeyAndValue<TKey, TValue> pair in list){
                dic.Add(pair.Key, pair.Value);
            }
            return dic;
        }
    }

    /// <summary>
    /// シリアル化できる、KeyValuePair
    /// </summary>
    [System.Serializable]
    public class KeyAndValue<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public KeyAndValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
        public KeyAndValue(KeyValuePair<TKey, TValue> pair)
        {
            Key = pair.Key;
            Value = pair.Value;
        }
    }
}

[System.Serializable]
public class PreloadedBullets : Serialize.TableBase<BulletID, uint, PreloadedBulletsPair> {}

[System.Serializable]
public class PreloadedBulletsPair : Serialize.KeyAndValue<BulletID, uint>
{
    public PreloadedBulletsPair(BulletID id, uint count) : base(id, count) {}
}

public class BulletGenerator : MoverGenerator<BulletController, BulletID>
{
    [SerializeField]
    private PreloadedBullets _preloadedObjects;

    void Awake()
    {
        // 予めオブジェクトを生成しておく場合はここに記述。
        var bulletsList = new List<IBullet>();
        //foreach (BulletID id in System.Enum.GetValues(typeof(BulletID)))
        foreach (var pair in _preloadedObjects.GetTable())
            for (var i = 1; i <= pair.Value; i++)
            {
                var bullet = GenerateObject(pair.Key, new Vector2(0, 300));
                bullet.Shot(0.0f, 0.0f);
                bulletsList.Add(bullet);
            }
        bulletsList.Select(bullet => { bullet.Erase(); return bullet; });
    }
}

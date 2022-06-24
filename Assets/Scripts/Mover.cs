using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
//using UnityEngine.Pool;  // Unityが提供するライブラリだと、一種類のオブジェクトしかプーリングできないようなので、自前で実装する。

/// <summary>衝突判定の対象となるオブジェクト。自機と敵と弾の親クラス。</summary>
/// <remarks>
/// デフォルトでenabledにfalseを設定するため、生成しただけでは更新されない。継承先で「実体化関数」を定義する必要がある。名前を別にしたのは、各クラスで渡すべき引数が異なるため。
/// </remarks>
public abstract class MoverController : MonoBehaviour, IManagedBehaviour, IPhysicalState
{
    protected Rigidbody2D _rigid2D;
    // HACK: ベクトルではなく複素数で管理するべきか？
    protected Vector2 _velocity = Vector2.zero;  // 速度。角度と速さの組のみ、或いは速度のみを保有すれば十分だが、計算コストの観点から両方保有する。
    private const float MovingThreshold = 80 * 80;  // Rigidbody2D.MovePositionを使うか否かの基準。値自体は当てずっぽう。
    private float _angle = 90f;  // 速度を極座標表示したときの角度。単位は度数法を用いる。
    private float _speed = 0f;   // 速度を極座標表示したときの大きさ（速さ）。単位はドット毎フレーム。

    public Vector2 Position
    {
        get { return transform.position; }
        set {
            if (_rigid2D.simulated)
                if ((value - Position).sqrMagnitude > MovingThreshold)
                   _rigid2D.position = value;
                else
                    _rigid2D.MovePosition(value);
            else
                transform.position = value;
        }
    }

    public virtual float Speed
    {
        get { return _speed; }
        set { 
            if (_speed != 0.0f)
                _velocity *= value / _speed;
            else
                _velocity.Set(value * Mathf.Cos(_angle * Mathf.Deg2Rad), value * Mathf.Sin(_angle * Mathf.Deg2Rad));
            _speed = value;
        }
    }

    public virtual float Angle
    {
        get { return _angle; }
        set {
            value = Mathf.Repeat(value, 360f);  // 負の値が渡されても、0 <= angle < 2 pi になるように変換する。
            _velocity = Quaternion.Euler(0f, 0f, value - _angle) * _velocity;  // HACK: 複素数を使えばもっと簡単に書ける。
            _angle = value;
        }
    }

    public virtual void ManagedFixedUpdate()
    {
        _rigid2D.velocity = _velocity / Time.fixedDeltaTime;  // 単位：(ドット / フレーム) / (秒 / フレーム) = ドット / 秒
    }

    public void ManagedUpdate() {}

    protected virtual void Awake()
    {
        _rigid2D = GetComponent<Rigidbody2D>();
        _rigid2D.simulated = false;
    }

    protected virtual void OnDisable()
    {
        _rigid2D.simulated = false;
    }

    protected virtual void OnEnable()
    {
        _rigid2D.simulated = true;
    }
}

// Luaのためのラッパークラス。
public abstract class Mover<TController, MoverID> : IActivity, IPhysicalState, ICollisionHandler
    where TController : MoverController
    where MoverID : Enum
{
    protected Activity _activity;
    protected TController _controller;
    protected ICollisionHandler _collisionHandler;
    protected MoverID _id;

    public Mover(GameObject go, MoverID id)
    {
        _activity = go.GetComponent<Activity>();
        _controller = go.GetComponent<TController>();
        _collisionHandler = go.GetComponent<ICollisionHandler>();
        _id = id;
    }

    public Vector2 Position { get => _controller.Position; set => _controller.Position = value; }
    public float Speed { get => _controller.Speed; set => _controller.Speed = value; }
    public float Angle { get => _controller.Angle; set => _controller.Angle = value; }
    public int Damage { get => _collisionHandler.Damage; }
    public int HitPoint { get => _collisionHandler.HitPoint; set => _collisionHandler.HitPoint = value; }
    public MoverID ID { get => _id; }

    public void Erase() => _activity.Erase();
    public bool IsEnabled() => _activity.IsEnabled();
}

/// <summary>IActivityを継承したオブジェクトの管理クラス。</summary>
/// <remarks>
/// IActivityを継承クラス（プレハブから複製されたGameObjectにアタッチされているもの）をリストとして所有する。
/// 生成の際には、もし使われていないGameObjectが存在すればそれを返し、もし全てが使われていれば新たに生成する。
/// 列挙型IDの要素は複製対象のプレハブ名との一致を要請する。
/// </remarks>
public abstract class MoverManager<TMover, TController, MoverID> : IManagedBehaviour
    where TMover : Mover<TController, MoverID>
    where TController : MoverController
    where MoverID : Enum
{
    protected ICollection<(TMover mover, Func<bool> isEnabled, Action fixedUpdate, Action update)> _pool = new List<(TMover mover, Func<bool> isEnabled, Action fixedUpdate, Action update)>();

    public ICollection<TMover> ObjectsList
    {
        get {
            return _pool
                .Where(pooledObject => pooledObject.isEnabled())
                .Select(pooledObject => pooledObject.mover)
                .ToList();
        }
    }

    /// <summary>未使用のGameObjectを探索して、見つかればそれを返す。見つからなければ新しく生成する。</summary>
    /// <param name="id">複製するプレハブに対応する列挙名</param>
    /// <param name="position">生成する位置</param>
    /// <returns>生成されたGameObjectにアタッチされているControllerクラス</returns>
    /// <remarks>
    /// 生成されたGameObjectは描画されず、物理演算も実行されない。返り値に対して「実体化関数」を呼ぶことで、それらが有効化される。
    /// </remarks>
    public TMover GenerateObject(MoverID id, in Vector2 position)
    {
        // 未使用のものを検索。変数への参照数でも判定したいが、Unityから参照されているものの判別が難しい上に、C#はC++ほど簡単に参照数を取得できなさそう（単に0か否かの峻別は可能）。
        if (_pool
            .FirstOrDefault(pooledObject => equal(pooledObject.mover.ID, id) && !pooledObject.isEnabled())
            is var first
            && first != default((TMover, Func<bool>, Action, Action)))
        {
            first.mover.Position = position;
            return first.mover;
        }

        // 新しいオブジェクトの生成。
        var prefab = Addressables.LoadAssetAsync<GameObject>(id.ToString()).WaitForCompletion();
        var newObject = GameObject.Instantiate(prefab, position, Quaternion.identity) as GameObject;
        //prefab.transform.parent = transform;
        newObject.name = prefab.name;
        //var newObject = Addressables.InstantiateAsync(id.ToString(), position, Quaternion.identity, transform).WaitForCompletion();
        //newObject.name = id.ToString();
        var mover = makeWrapper(newObject, id);
        Func<bool> isEnabled = newObject.GetComponent<IActivity>().IsEnabled;
        Action fixedUpdate = default;
        Action update = default;
        foreach (var behaviour in newObject.GetComponents<IManagedBehaviour>())
        {
            fixedUpdate += behaviour.ManagedFixedUpdate;
            update += behaviour.ManagedUpdate;
        }
        _pool.Add((mover, isEnabled, fixedUpdate, update));
        return mover;
    }

    public void ManagedFixedUpdate()
    {
        // LINQを使うと遅くなる。
        //_pool.Where(pooledObject => pooledObject.IsEnabled())
        //    .Select(pooledObject => { pooledObject.ManagedFixedUpdate(); return pooledObject; });
        foreach (var pooledObject in _pool)
            if (pooledObject.isEnabled())
                pooledObject.fixedUpdate();
    }

    public void ManagedUpdate()
    {
        foreach (var pooledObject in _pool)
            if (pooledObject.isEnabled())
                pooledObject.update();
    }

    protected abstract TMover makeWrapper(GameObject go, MoverID id);  // new制約を使うと引数が渡せない上に遅くなるので、子クラスで生成用関数を定義。
    protected abstract bool equal(MoverID id1, MoverID id2);  // ジェネリックスの関係で等号が使えないので、子クラスに任せる。
}

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
            if (table is null) {
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

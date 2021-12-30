using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum PlayerID
{
    // 自機。
    Reimu,
    Marisa,
    Sanae,
    // オプション。
    ReimuOption,
    MarisaOption,
    SanaeOption
}

public class PlayerGenerator : MonoBehaviour
{
    private Vector2Int _playerSize;  // 本来はreadonlyにしたいところだが、Unityではコンストラクタが呼べないため、工夫が必要。
    private List<PlayerController> _objectsList;
    //private Dictionary<PlayerID, string> _pathsTable;

    public Vector2Int PlayerSize { get => _playerSize; }

    public IList<PlayerController> ObjectsList
    {
        get {
            var temp = new List<PlayerController>();
            foreach (var mover in _objectsList)
                if (mover.IsEnabled())
                    temp.Add(mover);
            return temp;
        }
    }

    void Awake()
    {
        var prefab = Addressables.LoadAssetAsync<GameObject>(makePath((PlayerID)0)).WaitForCompletion();  // 自機のスプライトのサイズは何れも同じことを要請。
        Vector2 size = prefab.GetComponent<SpriteRenderer>().bounds.size;
        _playerSize = Vector2Int.RoundToInt(size);
        if (size - _playerSize != Vector2.zero)
            Debug.LogWarning("The width or the height of the sprite are not integer numbers: " + size.ToString());

        _objectsList = new List<PlayerController>();

        // 予め生成しておく場合はここに記述。
    }

    public PlayerController GenerateObject(PlayerID id, Vector2 position)
    {
        foreach (var mover in _objectsList)
            if (System.Type.GetType(id.ToString()) == mover.GetType() && !mover.IsEnabled())
                return mover;
        var prefab = Addressables.LoadAssetAsync<GameObject>(makePath(id)).WaitForCompletion();
        var newObject = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        newObject.transform.parent = this.transform;
        _objectsList.Insert(0, newObject.GetComponent<PlayerController>());
        return _objectsList[0];
    }

    private string makePath(PlayerID id)
    {
        return "Assets/Prefabs/" + id.ToString() + ".prefab";
    }
}

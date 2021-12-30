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

public class PlayerGenerator : MoverGenerator<PlayerController, PlayerID>
{
    private Vector2Int _playerSize;  // 本来はreadonlyにしたいところだが、Unityではコンストラクタが呼べないため、工夫が必要。

    public Vector2Int PlayerSize { get => _playerSize; }

    void Awake()
    {
        var prefab = Addressables.LoadAssetAsync<GameObject>(makePath((PlayerID)0)).WaitForCompletion();  // 自機のスプライトのサイズは何れも同じことを要請。
        Vector2 size = prefab.GetComponent<SpriteRenderer>().bounds.size;
        _playerSize = Vector2Int.RoundToInt(size);
        if (size - _playerSize != Vector2.zero)
            Debug.LogWarning("The width or the height of the sprite are not integer numbers: " + size.ToString());

        _objectsList = new List<PlayerController>();
        _objectNoun = "";
        
        // 予めオブジェクトを生成しておく場合はここに記述。
    }
}

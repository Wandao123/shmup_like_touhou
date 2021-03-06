using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

[System.Serializable]
public class PreloadedPlayers : Serialize.TableBase<PlayerID, uint, PreloadedPlayersPair> {}

[System.Serializable]
public class PreloadedPlayersPair : Serialize.KeyAndValue<PlayerID, uint>
{
    public PreloadedPlayersPair(PlayerID id, uint count) : base(id, count) {}
}

public class PlayerManager : MoverManager<Player, PlayerController, PlayerID>
{
    private Vector2Int _characterSize;  // 本来はreadonlyにしたいところだが、MonoBehaviourを継承したクラスではコンストラクタが呼べないため、工夫が必要。

    public Vector2Int CharacterSize { get => _characterSize; }

    public PlayerManager(in Transform gameDirector, in Dictionary<PlayerID, uint> preloadedObjectsTable) : base(gameDirector, preloadedObjectsTable)
    {
        var prefab = Addressables.LoadAssetAsync<GameObject>(((PlayerID)0).ToString()).WaitForCompletion();  // 自機のスプライトのサイズは何れも同じことを要請。
        Vector2 size = prefab.GetComponent<SpriteRenderer>().bounds.size;
        _characterSize = Vector2Int.RoundToInt(size);
        if (size - _characterSize != Vector2.zero)
            Debug.LogWarning("The width or the height of the sprite are not integer numbers: " + size.ToString());

        // 予めオブジェクトを生成しておく場合はここに記述。
    }

    protected override Player makeWrapper(GameObject go, PlayerID id)
    {
        return new Player(go, id);
    }

    protected override bool equal(PlayerID id1, PlayerID id2)
    {
        return id1 == id2;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptDirector : MonoBehaviour
{
    private Vector2Int _screenBottomLeft, _screenTopRight;  // 本来はreadonlyにしたいところだが、Unityではコンストラクタが呼べないため、工夫が必要。
    private Vector2Int _characterSize;
    private ShmupInputActions _inputActions;
    [SerializeField]
    private PlayerGenerator _playerGenerator;
    private PlayerController _player;

    public Vector2Int ScreenBottomLeft { get => _screenBottomLeft; }
    public Vector2Int ScreenTopRight { get => _screenTopRight; }

    void Awake()
    {
        Vector2 bottomLeft = Camera.main.ViewportToWorldPoint(Vector2.zero);
        _screenBottomLeft = Vector2Int.RoundToInt(bottomLeft);
        Vector2 topRight = Camera.main.ViewportToWorldPoint(Vector2.one);
        _screenTopRight = Vector2Int.RoundToInt(topRight);
        if (bottomLeft - _screenBottomLeft != Vector2.zero || topRight - _screenTopRight != Vector2.zero)
            Debug.LogWarning("The width or the height of the screen are not integer numbers: " + bottomLeft.ToString() + ", " + topRight.ToString());
            
        _inputActions = new ShmupInputActions();
        _inputActions.Enable();
    }

    void Start()
    {
        _characterSize = _playerGenerator.GetComponent<PlayerGenerator>().CharacterSize;
        _player = _playerGenerator.GenerateObject(PlayerID.Reimu);
        _player.Position = new Vector2(0, _screenBottomLeft.y + _characterSize.y);
        StartCoroutine(playerScript());
    }

    void Update()
    {
        var velocity = _inputActions.Player.Move.ReadValue<Vector2>();
        _player.Velocity = velocity;
    }

    private IEnumerator playerScript()
    {
        _player.Spawned();
        _player.TurnInvincible(180);
        for (var i = 1; i <= 210; i++)
            yield return null;
        _player.Erase();
        for (var i = 1; i <= 120; i++)
            yield return null;
        _player = _playerGenerator.GenerateObject(PlayerID.Reimu);
        _player.Position = new Vector2(0, _screenBottomLeft.y + _characterSize.y);
        _player.Spawned();
        Debug.Log("Script has finished.");
    }
}

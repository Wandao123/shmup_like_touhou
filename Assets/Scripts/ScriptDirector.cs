using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptDirector : MonoBehaviour
{
    private Vector2Int _screenBottomLeft, _screenTopRight;  // 本来はreadonlyにしたいところだが、Unityではコンストラクタが呼べないため、工夫が必要。
    private Vector2Int _playerSize;
    private ShmupInputActions _inputActions;
    [SerializeField]
    private PlayerGenerator _playerGenerator;
    [SerializeField]
    private EnemyGenerator _enemyGenerator;
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
        _playerSize = _playerGenerator.GetComponent<PlayerGenerator>().CharacterSize;
        _player = _playerGenerator.GenerateObject(PlayerID.Reimu, new Vector2(0, _screenBottomLeft.y + _playerSize.y));
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
        StartCoroutine(enemyScript());
        for (var i = 1; i <= 210; i++)
            yield return null;
        _player.Position = Vector2.zero;
        for (var i = 1; i <= 60; i++)
            yield return null;
        _player.Erase();
        for (var i = 1; i <= 60; i++)
            yield return null;
        _player = _playerGenerator.GenerateObject(PlayerID.Reimu, new Vector2(0, _screenBottomLeft.y + _playerSize.y));
        _player.Spawned();
        Debug.Log("Script has finished.");
    }

    private IEnumerator enemyScript()
    {
        var smallFairy = _enemyGenerator.GenerateObject(EnemyID.SmallRedFairy, new Vector2(_screenBottomLeft.x * 0.5f, _screenTopRight.y * 0.5f));
        smallFairy.Spawned(1.0f, -0.5f * Mathf.PI, 10);
        for (var i = 1; i <= 120; i++)
            yield return null;
        for (var i = 0; i <= 360; i++)
        {
            smallFairy.Angle += Mathf.PI / 180;
            yield return null;
        }
        smallFairy.Speed = 0.0f;
    }
}

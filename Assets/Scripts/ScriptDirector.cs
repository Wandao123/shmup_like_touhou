using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptDirector : MonoBehaviour
{
    private int _screenWidth, _screenHeight;
    private int _playerWidth, _playerHeight;
    private ShmupInputActions _inputActions;
    [SerializeField]
    private GameObject _playerPrefab;  // TODO: 管理クラスに置き換える。
    private PlayerCharacterController _player;

    void Awake()
    {
        _screenWidth = 2 * (int)Camera.main.ViewportToWorldPoint(new Vector2(1, 1)).x;
        _screenHeight = 2 * (int)Camera.main.ViewportToWorldPoint(new Vector2(1, 1)).y;
        _playerWidth = (int)_playerPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        _playerHeight = (int)_playerPrefab.GetComponent<SpriteRenderer>().bounds.size.y;
        _inputActions = new ShmupInputActions();
        _inputActions.Enable();
    }

    void Start()
    {
        _player = (Instantiate(_playerPrefab, new Vector2(0, -_screenHeight / 2 + _playerHeight), Quaternion.identity) as GameObject).GetComponent<PlayerCharacterController>();
        _player.enabled = true;
        _player.TurnInvincible(180);
    }

    void Update()
    {
        var velocity = _inputActions.Player.Move.ReadValue<Vector2>();
        _player.Velocity = velocity;
    }
}

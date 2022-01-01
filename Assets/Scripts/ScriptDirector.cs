using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private PlayerController _player = null;

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
        StartCoroutine(playerScript());
        StartCoroutine(stageScript());
    }

    void Update()
    {
        
    }

    private IEnumerator wait(uint numFrames)
    {
        for (var i = 1; i <= numFrames; i++)
            yield return null;
    }

    private IEnumerator playerScript()
    {
        const int InvincibleFrames = 360;
        const int InputDelayFrames = 90;
        const int ShotDelayFrames = 6;
        const float BulletSpeed = 30.0f;

        IEnumerator initialize()
        {
            _player = _playerGenerator.GenerateObject(PlayerID.Reimu, new Vector2(0.0f, _screenBottomLeft.y - _playerSize.y + InputDelayFrames));
            _player.Spawned();
            _player.TurnInvincible(InvincibleFrames / 2);
            yield break;
        }

        IEnumerator rebirth()
        {
            if (!_player.IsEnabled() && _player.HitPoint > 0)
            {
                _player.Position = new Vector2(0.0f, _screenBottomLeft.y - _playerSize.y);
                _player.Spawned();
                _player.TurnInvincible(InvincibleFrames);
                yield return null;
                for (var i = 1; i <= InputDelayFrames; i++)
                {
                    _player.MovePosition(_player.Position + new Vector2(0.0f, 1.0f));
                    yield return null;
                }
            }
        }

        IEnumerator move()
        {
            while (true)
            {
                var velocity = _inputActions.Player.Move.ReadValue<Vector2>();
                _player.SlowMode = _inputActions.Player.Slow.WasPressedThisFrame();
                _player.Velocity = velocity;
                yield return null;
            }
        }

        IEnumerator shoot()
        {
            while (true)
            {
                if (_inputActions.Player.Shot.WasPressedThisFrame())
                {
                    //GeneratePlayerBullet
                    yield return wait(ShotDelayFrames);
                }
                else
                {
                    yield return null;
                }
            }
        }

        Action down()
        {
            int life = _player.HitPoint;
            return () => {
                if (_player.HitPoint < life)
                {
                    //GenerateEffect
                    life = _player.HitPoint;
                }
            };
        }

        yield return initialize();
        IEnumerator[] co = { shoot(), move() };
        var detectDown = down();
        do
        {
            yield return rebirth();
            for (var i = 0; i < co.Length; i++)
            {
                var status = co[i].MoveNext();
                if (!status)
                    Debug.LogWarning(co[i].ToString() + " stopped");
            }
            yield return co.Last().Current;
            detectDown();
        }
        while (true);
    }

    private IEnumerator stageScript()
    {
        var smallFairy = _enemyGenerator.GenerateObject(EnemyID.SmallRedFairy, new Vector2(_screenBottomLeft.x * 0.5f, _screenTopRight.y * 0.5f));
        smallFairy.Spawned(1.0f, -0.5f * Mathf.PI, 10);
        yield return wait(120);
        for (var i = 0; i <= 360; i++)
        {
            smallFairy.Angle += Mathf.PI / 180;
            yield return null;
        }
        smallFairy.Speed = 0.0f;
    }
}

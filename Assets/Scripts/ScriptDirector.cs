using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoonSharp.Interpreter;

public class ScriptDirector : MonoBehaviour
{
    private Vector2Int _screenBottomLeft, _screenTopRight;  // 本来はreadonlyにしたいところだが、Unityではコンストラクタが呼べないため、工夫が必要。
    private Vector2Int _playerSize;
    private ShmupInputActions _inputActions;
    [SerializeField]
    private EnemyGenerator _enemyGenerator;
    [SerializeField]
    private PlayerGenerator _playerGenerator;
    [SerializeField]
    private BulletGenerator _enemyBulletGenerator;
    [SerializeField]
    private BulletGenerator _playerBulletGenerator;
    private PlayerController _player = null;
    private Script _script;

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

        _script = new Script();
        registerClasses();
        registerConstants();
        //registerGlueFunctions();
        UserData.RegisterAssembly();
    }

    void Start()
    {
        _playerSize = _playerGenerator.GetComponent<PlayerGenerator>().CharacterSize;
        StartCoroutine(playerScript());
        StartCoroutine(stageScript());

        //DynValue result = _script.DoString("GenerateEnemy(EnemyID.SmallBlueFairy, ScreenWidth / 2, ScreenHeight / 2, 0, math.pi / 2, 8)");
        _script.DoFile("main.lua");
    }

    void Update()
    {
        
    }

    private void registerClasses()
    {
        UserData.RegisterType<IBullet>();
        UserData.RegisterType<BulletID>();
        UserData.RegisterType<IEnemy>();
        UserData.RegisterType<EnemyID>();
        UserData.RegisterType<IPlayer>();
        UserData.RegisterType<PlayerID>();
    }

    private void registerConstants()
    {
        _script.Globals["ScreenWidth"] = 2 * _screenTopRight.x;
        _script.Globals["ScreenHeight"] = 2 * _screenTopRight.y;
        _script.Globals["PlayerWidth"] = _playerSize.x;
	    _script.Globals["PlayerHeight"] = _playerSize.y;
        _script.Globals["BulletID"] = UserData.CreateStatic<BulletID>();
        _script.Globals["EnemyID"] = UserData.CreateStatic<EnemyID>();
        _script.Globals["PlayerID"] = UserData.CreateStatic<PlayerID>();
    }

    private void registerGlueFunctions()
    {
        // 座標変換。
        // Unityの座標系と異なり、Luaでは画面の左上を原点に取った上で、左から右への向きでx軸、上から下への向きでy軸を定める。
        Vector2 transformIntoVector2From(float posX, float posY)
        {
            var position = new System.Numerics.Complex(posX, posY);
            position = System.Numerics.Complex.Conjugate(position) + new System.Numerics.Complex(-_screenTopRight.x, +_screenTopRight.y);
            return new Vector2((float)position.Real, (float)position.Imaginary);
        }

        Func<EnemyID, float, float, float, float, int, IEnemy> generateEnemy =
        (EnemyID id, float posX, float posY, float speed, float angle, int hitPoint) =>
        {
            var newObject = _enemyGenerator.GenerateObject(id, transformIntoVector2From(posX, posY));
            newObject.Spawned(speed, angle, hitPoint);
            return newObject;
        };
        _script.Globals["GenerateEnemy"] = generateEnemy;
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
                _player.SlowMode = _inputActions.Player.Slow.IsPressed();
                _player.Velocity = velocity;
                yield return null;
            }
        }

        IEnumerator shoot()
        {
            while (true)
            {
                if (_inputActions.Player.Shot.IsPressed())
                {
                    _playerBulletGenerator.GenerateObject(BulletID.ReimuNormalBullet, _player.Position - new Vector2(12.0f, 0.0f)).Shot(BulletSpeed, 0.5f * Mathf.PI);
                    _playerBulletGenerator.GenerateObject(BulletID.ReimuNormalBullet, _player.Position + new Vector2(12.0f, 0.0f)).Shot(BulletSpeed, 0.5f * Mathf.PI);
                    //GenerateEffect
                    yield return wait(ShotDelayFrames);
                }
                yield return null;
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
        var smallFairy = _enemyGenerator.GenerateObject(EnemyID.SmallRedFairy, new Vector2(_screenBottomLeft.x * 0.5f, _screenTopRight.y));
        smallFairy.Spawned(1.0f, -0.5f * Mathf.PI, 15);
        yield return wait(120);
        for (var i = 0; i <= 360; i++)
        {
            if (i % 30 == 0)
                _enemyBulletGenerator.GenerateObject(BulletID.SmallRedBullet, smallFairy.Position)
                .Shot(2.0f, Mathf.Atan2(_player.Position.y - smallFairy.Position.y, _player.Position.x - smallFairy.Position.x));
            smallFairy.Angle += Mathf.PI / 180;
            yield return null;
        }
        smallFairy.Speed = 0.0f;
    }
}

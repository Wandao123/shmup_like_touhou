using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

public enum CommandID : int {
    // メニューで使用。キーの割り当てを変更不可。
    /*OK,
    Cancel,
    Left,
    Right,
    Up,
    Down,*/
    // ゲーム中で使用。
    Shot,
    Bomb,
    Slow,
    Skip,
    Leftward,
    Rightward,
    Forward,
    Backward,
    Pause,
    SIZE  // 要素数を取得するためのダミー。
}

// 参考：https://qiita.com/sevenstartears/items/b8ebd3939211b68fcaa4
//       https://eims.hatenablog.com/entry/2018/09/25/021420
public class ScriptDirector : MonoBehaviour
{
    private Vector2Int _screenBottomLeft, _screenTopRight;  // 本来はreadonlyにしたいところだが、Unityではコンストラクタが呼べないため、工夫が必要。
    private Vector2Int _playerSize;
    private ShmupInputActions _inputActions;
    private Dictionary<CommandID, Func<bool>> _mapping;
    [SerializeField]
    private EnemyGenerator _enemyGenerator;
    [SerializeField]
    private PlayerGenerator _playerGenerator;
    [SerializeField]
    private BulletGenerator _enemyBulletGenerator;
    [SerializeField]
    private BulletGenerator _playerBulletGenerator;
    private Script _script;
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
        _mapping = new Dictionary<CommandID, Func<bool>>() {
            { CommandID.Shot, _inputActions.Player.Shot.IsPressed },
            { CommandID.Bomb, _inputActions.Player.Bomb.IsPressed },
            { CommandID.Slow, _inputActions.Player.Slow.IsPressed },
            { CommandID.Skip, _inputActions.Player.Skip.IsPressed },
            { CommandID.Leftward, () => _inputActions.Player.Move.ReadValue<Vector2>().x < 0.0f },
            { CommandID.Rightward, () => _inputActions.Player.Move.ReadValue<Vector2>().x > 0.0f },
            { CommandID.Forward, () => _inputActions.Player.Move.ReadValue<Vector2>().y > 0.0f },
            { CommandID.Backward, () => _inputActions.Player.Move.ReadValue<Vector2>().y < 0.0f },
            { CommandID.Pause, _inputActions.Player.Pause.IsPressed }
        };

        _script = new Script();
        _script.Options.ScriptLoader = new MoonSharp.Interpreter.REPL.ReplInterpreterScriptLoader();
        ((ScriptLoaderBase)_script.Options.ScriptLoader).ModulePaths = ScriptLoaderBase.UnpackStringPaths("Assets/lua_scripts/?;Assets/lua_scripts/?.lua");
        _script.Options.DebugPrint = s => Debug.Log(s);
        registerClasses();
        registerConstants();
        registerGlueFunctions();
        UserData.RegisterAssembly();
    }

    void Start()
    {
        _playerSize = _playerGenerator.GetComponent<PlayerGenerator>().CharacterSize;
        //StartCoroutine(playerScript());
        //StartCoroutine(stageScript());

        _script.DoFile("Assets/lua_scripts/main.lua");
        var co = _script.CreateCoroutine(_script.Globals["Main"]);
        StartCoroutine(runLuaCoroutine(co));
    }

    void Update()
    {
        
    }

    void OnDestroy()
    {
        
    }

    private void registerClasses()
    {
        UserData.RegisterType<Vector2>();
        UserData.RegisterType<Vector2Int>();
        UserData.RegisterType<BulletID>();
        UserData.RegisterType<CommandID>();
        UserData.RegisterType<EnemyID>();
        UserData.RegisterType<PlayerID>();
    }

    private void registerConstants()
    {
        _script.Globals["ScreenTopRight"] = _screenTopRight;
        _script.Globals["ScreenBottomLeft"] = _screenBottomLeft;
        _script.Globals["ScreenTopLeft"] = Vector2Int.RoundToInt(Camera.main.ViewportToWorldPoint(new Vector2(0, 1)));
        _script.Globals["ScreenBottomRight"] = Vector2Int.RoundToInt(Camera.main.ViewportToWorldPoint(new Vector2(1, 0)));
        _script.Globals["ScreenCenter"] = Vector2Int.RoundToInt(Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 0.5f)));
        _script.Globals["ScreenTop"] = Vector2Int.RoundToInt(Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 1.0f)));
        _script.Globals["ScreenBottom"] = Vector2Int.RoundToInt(Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 0.0f)));
        _script.Globals["ScreenLeft"] = Vector2Int.RoundToInt(Camera.main.ViewportToWorldPoint(new Vector2(0.0f, 0.5f)));
        _script.Globals["ScreenRight"] = Vector2Int.RoundToInt(Camera.main.ViewportToWorldPoint(new Vector2(1.0f, 0.5f)));
        _script.Globals["PlayerSize"] = _playerSize;
        _script.Globals["BulletID"] = UserData.CreateStatic<BulletID>();
        _script.Globals["CommandID"] = UserData.CreateStatic<CommandID>();
        _script.Globals["EnemyID"] = UserData.CreateStatic<EnemyID>();
        _script.Globals["PlayerID"] = UserData.CreateStatic<PlayerID>();
        _script.Globals["Vector2"] = typeof(Vector2);
    }

    private void registerGlueFunctions()
    {
        Func<BulletID, float, float, float, float, IBullet> generateBullet =
        (BulletID id, float posX, float posY, float speed, float angle) =>
        {
            var newObject = _enemyBulletGenerator.GenerateObject(id, new Vector2(posX, posY));
            newObject.Shot(speed, angle);
            return newObject;
        };
        _script.Globals["GenerateBullet"] = generateBullet;

        Func<BulletID, float, float, float, float, IBullet> generatePlayerBullet =
        (BulletID id, float posX, float posY, float speed, float angle) =>
        {
            var newObject = _playerBulletGenerator.GenerateObject(id, new Vector2(posX, posY));
            newObject.Shot(speed, angle);
            return newObject;
        };
        _script.Globals["GeneratePlayerBullet"] = generatePlayerBullet;

        Func<EnemyID, float, float, float, float, int, IEnemy> generateEnemy =
        (EnemyID id, float posX, float posY, float speed, float angle, int hitPoint) =>
        {
            var newObject = _enemyGenerator.GenerateObject(id, new Vector2(posX, posY));
            newObject.Spawned(speed, angle, hitPoint);
            return newObject;
        };
        _script.Globals["GenerateEnemy"] = generateEnemy;

        Func<PlayerID, float, float, IPlayer> generatePlayer =
        (PlayerID id, float posX, float posY) =>
        {
            var newObject = _playerGenerator.GenerateObject(id, new Vector2(posX, posY));
            newObject.Spawned();
            return newObject;
        };
        _script.Globals["GeneratePlayer"] = generatePlayer;

        Func<CommandID, bool> getKey = (CommandID id) => _mapping[id]();
        _script.Globals["GetKey"] = getKey;

        //_script.Globals["StartCoroutine"] = (Func<DynValue, UnityEngine.Coroutine>)((DynValue func) => StartCoroutine(runLuaCoroutine(_script.CreateCoroutine(func))));
    }

    private IEnumerator runLuaCoroutine(DynValue func)
    {
        var co = func.Coroutine;
        while (true)
        {
            if (co.State == CoroutineState.Dead)
                break;
            else
                yield return co.Resume();
        }
    }

    // Luaを使わない方法。
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
            if (i % 30 == 0 && smallFairy.IsEnabled())
                _enemyBulletGenerator.GenerateObject(BulletID.SmallRedBullet, smallFairy.Position)
                .Shot(2.0f, Mathf.Atan2(_player.Position.y - smallFairy.Position.y, _player.Position.x - smallFairy.Position.x));
            smallFairy.Angle += Mathf.PI / 180;
            yield return null;
        }
        smallFairy.Speed = 0.0f;
    }
}

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
    private Vector2 _screenBottomLeft, _screenTopRight;  // 本来はreadonlyにしたいところだが、Unityではコンストラクタが呼べないため、工夫が必要。
    private Vector2Int _playerSize;
    private ShmupInputActions _inputActions;
    private Dictionary<CommandID, Func<bool>> _mapping;
    private EnemyManager _enemyManager;
    private PlayerManager _playerManager;
    private BulletManager _enemyBulletManager;
    private BulletManager _playerBulletManager;
    private Script _script;
    private Player _player = null;

    // 参考：http://tawamuredays.blog.fc2.com/blog-entry-218.html
    public delegate Range AppliedFunc<Name, Args, Range>(Name name, params Args[] args);

    void Awake()
    {
        _screenBottomLeft = Camera.main.ViewportToWorldPoint(Vector2.zero);
        _screenTopRight = Camera.main.ViewportToWorldPoint(Vector2.one);
        _enemyManager = new EnemyManager();
        _playerManager = new PlayerManager();
        _enemyBulletManager = new BulletManager();
        _playerBulletManager = new BulletManager();
            
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
        registerGlueFunctions();
        UserData.RegisterAssembly();
    }

    void Start()
    {
        _playerSize = _playerManager.CharacterSize;
        StartCoroutine(playerScript());
        StartCoroutine(stageScript());

        registerConstants();
        //_script.DoFile("Assets/lua_scripts/main.lua");
        //StartCoroutine(runLuaCoroutine(_script.Globals.Get("Main")));
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
        UserData.RegisterType<UnityEngine.Coroutine>();
        UserData.RegisterType<BulletID>();
        UserData.RegisterType<CommandID>();
        UserData.RegisterType<EnemyID>();
        UserData.RegisterType<PlayerID>();
        //UserData.RegisterType<BulletController>();
        //UserData.RegisterType<EnemyController>();
        //UserData.RegisterType<PlayerController>();
    }

    private void registerConstants()
    {
        _script.Globals["Vector2"] = typeof(Vector2);
        _script.Globals["ScreenTopRight"] = _screenTopRight;
        _script.Globals["ScreenBottomLeft"] = _screenBottomLeft;
        _script.Globals["ScreenTopLeft"] = (Vector2)Camera.main.ViewportToWorldPoint(new Vector2(0, 1));
        _script.Globals["ScreenBottomRight"] = (Vector2)Camera.main.ViewportToWorldPoint(new Vector2(1, 0));
        _script.Globals["ScreenCenter"] = (Vector2)Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
        _script.Globals["ScreenTop"] = (Vector2)Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 1.0f));
        _script.Globals["ScreenBottom"] = (Vector2)Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 0.0f));
        _script.Globals["ScreenLeft"] = (Vector2)Camera.main.ViewportToWorldPoint(new Vector2(0.0f, 0.5f));
        _script.Globals["ScreenRight"] = (Vector2)Camera.main.ViewportToWorldPoint(new Vector2(1.0f, 0.5f));
        _script.Globals["PlayerSize"] = _playerSize;
        _script.Globals["BulletID"] = UserData.CreateStatic<BulletID>();
        _script.Globals["CommandID"] = UserData.CreateStatic<CommandID>();
        _script.Globals["EnemyID"] = UserData.CreateStatic<EnemyID>();
        _script.Globals["PlayerID"] = UserData.CreateStatic<PlayerID>();
    }

    private void registerGlueFunctions()
    {
        /*Func<BulletID, float, float, float, float, IBullet> generateBullet =
        (BulletID id, float posX, float posY, float speed, float angle) =>
        {
            var newObject = _enemyBulletManager.GenerateObject(id, new Vector2(posX, posY));
            newObject.Shot(speed, angle);
            return newObject;
        };
        _script.Globals["GenerateBullet"] = generateBullet;

        Func<BulletID, float, float, float, float, IBullet> generatePlayerBullet =
        (BulletID id, float posX, float posY, float speed, float angle) =>
        {
            var newObject = _playerBulletManager.GenerateObject(id, new Vector2(posX, posY));
            newObject.Shot(speed, angle);
            return newObject;
        };
        _script.Globals["GeneratePlayerBullet"] = generatePlayerBullet;

        Func<EnemyID, float, float, float, float, int, IEnemy> generateEnemy =
        (EnemyID id, float posX, float posY, float speed, float angle, int hitPoint) =>
        {
            var newObject = _enemyManager.GenerateObject(id, new Vector2(posX, posY));
            newObject.Spawned(speed, angle, hitPoint);
            return newObject;
        };
        _script.Globals["GenerateEnemy"] = generateEnemy;

        Func<PlayerID, float, float, IPlayer> generatePlayer =
        (PlayerID id, float posX, float posY) =>
        {
            var newObject = _playerManager.GenerateObject(id, new Vector2(posX, posY));
            newObject.Spawned();
            return newObject;
        };
        _script.Globals["GeneratePlayer"] = generatePlayer;

        Func<IPlayer> getPlayer = () => _playerManager.GetPlayer();
        _script.Globals["GetPlayer"] = getPlayer;

        Func<CommandID, bool> getKey = (CommandID id) => _mapping[id]();
        _script.Globals["GetKey"] = getKey;

        AppliedFunc<DynValue, DynValue, UnityEngine.Coroutine> luaStartCoroutineWithArgs =
        (func, args) => StartCoroutine(runLuaCoroutine(func, args));
        _script.Globals["StartCoroutineWithArgs"] = luaStartCoroutineWithArgs;
        _script.DoString(@"
            function StartCoroutine(func, ...)
                StartCoroutineWithArgs(func, {...})
            end
        ");*/
    }

    private IEnumerator runLuaCoroutine(DynValue func, params DynValue[] args)
    {
        var co = _script.CreateCoroutine(func).Coroutine;
        while (co.State != CoroutineState.Dead)
        {
            co.Resume(args);
            yield return null;
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
            var playerObject = _playerManager.GenerateObject(PlayerID.Reimu, new Vector2(0.0f, _screenBottomLeft.y - _playerSize.y + InputDelayFrames));
            _player = new Player(playerObject.GetComponent<PlayerController>(), playerObject.GetComponent<ICollisionHandler>(), playerObject.GetComponent<IInvincibility>());
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
                    _player.Position += new Vector2(0.0f, 1.0f);
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
                    _playerBulletManager.GenerateObject(BulletID.ReimuNormalBullet, _player.Position - new Vector2(12.0f, 0.0f)).Shot(BulletSpeed, 0.5f * Mathf.PI);
                    _playerBulletManager.GenerateObject(BulletID.ReimuNormalBullet, _player.Position + new Vector2(12.0f, 0.0f)).Shot(BulletSpeed, 0.5f * Mathf.PI);
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
        var smallRedFairy = _enemyManager.GenerateObject(EnemyID.SmallRedFairy, new Vector2(_screenBottomLeft.x * 0.5f, _screenTopRight.y));
        smallRedFairy.Spawned(1.0f, -0.5f * Mathf.PI, 15);
        var smallBlueFairy = _enemyManager.GenerateObject(EnemyID.SmallBlueFairy, new Vector2(_screenTopRight.x * 0.5f, _screenTopRight.y));
        smallBlueFairy.Spawned(1.0f, -0.5f * Mathf.PI, 15);
        yield return wait(120);
        for (var i = 0; i <= 360; i++)
        {
            if (i % 6 == 0)
            {
                if (smallRedFairy.IsEnabled())
                    _enemyBulletManager.GenerateObject(BulletID.SmallRedBullet, smallRedFairy.Position)
                    .Shot(2.0f, Mathf.Atan2(_player.Position.y - smallRedFairy.Position.y, _player.Position.x - smallRedFairy.Position.x));
                if (smallBlueFairy.IsEnabled())
                    _enemyBulletManager.GenerateObject(BulletID.SmallBlueBullet, smallBlueFairy.Position)
                    .Shot(2.0f, Mathf.Atan2(_player.Position.y - smallBlueFairy.Position.y, _player.Position.x - smallBlueFairy.Position.x));
            }
            smallRedFairy.Angle += Mathf.Deg2Rad;
            smallBlueFairy.Angle -= Mathf.Deg2Rad;
            yield return null;
        }
        smallRedFairy.Speed = 0.0f;
        smallBlueFairy.Speed = 0.0f;
    }
}

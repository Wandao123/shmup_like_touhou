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
public class ScriptDirector : MonoBehaviour
{
    private class MyCustomScriptLoader : ScriptLoaderBase
    {
        public override object LoadFile(string file, Table globalContext)
        {
            return string.Format("print ([[A request to load '{0}' has been made]])", file);
        }

        public override bool ScriptFileExists(string name)
        {
            return true;
        }
    }

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
    private PlayerController _player = null;
    private Script _script;
    //private const string LuaPath = "Assets/lua_scripts";

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
            { CommandID.Leftward, () => _inputActions.Player.Move.ReadValue<Vector2>().x > 0.0f },
            { CommandID.Rightward, () => _inputActions.Player.Move.ReadValue<Vector2>().x < 0.0f },
            { CommandID.Forward, () => _inputActions.Player.Move.ReadValue<Vector2>().y > 0.0f },
            { CommandID.Backward, () => _inputActions.Player.Move.ReadValue<Vector2>().y < 0.0f },
            { CommandID.Pause, _inputActions.Player.Pause.IsPressed }
        };

        _script = new Script();
        //_script.Options.ScriptLoader = new EmbeddedResourcesScriptLoader();
        //((ScriptLoaderBase)_script.Options.ScriptLoader).ModulePaths = ScriptLoaderBase.UnpackStringPaths("lua_scripts/?;lua_scripts/?.lua");
        /*var textAssets = Resources.LoadAll<TextAsset>(LuaPath);
        var scripts = textAssets.ToDictionary(ta => ta.name, ta => ta.text);
        var loader = new MoonSharp.Interpreter.Loaders.UnityAssetsScriptLoader(scripts) {ModulePaths = new[] {"?"}};
        _script = new Script {Options = {ScriptLoader = loader}};*/
        _script.Options.ScriptLoader = new MyCustomScriptLoader() 
        { 
            ModulePaths = new string[] { "Assets/lua_scripts/?.lua" } 
        };
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

        //_luaenv.DoString("require 'Assets/lua_scripts/initialization_for_libraries.lua'");  // ライブラリに依存した初期化。ライブラリ毎の差異を吸収するため。
        DynValue res = _script.DoString(@"
            dofile 'main'
        ");
        //_script.DoFile("main");
        //DynValue res = _script.DoString("require 'main'");
        //DynValue res = _script.Call(_script.Globals["Main"]);
        Debug.Log(res);
        //_luaenv.DoString("require 'main'");
        //_luaenv.DoString("require 'Assets/lua_scripts/reimu.lua'");
        //StartCoroutine(_luaenv.Global.Get<Func<IEnumerator>>("RunPlayerScript")());
    }

    void Update()
    {
        
    }

    void OnDestroy()
    {
        
    }

    private byte[] customLoader(ref string filepath)
    {
        if (File.Exists(filepath))
            return File.ReadAllBytes(filepath);
        else
            return null;
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

        Func<BulletID, float, float, float, float, IBullet> generateBullet =
        (BulletID id, float posX, float posY, float speed, float angle) =>
        {
            var newObject = _enemyBulletGenerator.GenerateObject(id, transformIntoVector2From(posX, posY));
            newObject.Shot(speed, angle);
            return newObject;
        };
        _script.Globals["GenerateBullet"] = generateBullet;

        Func<BulletID, float, float, float, float, IBullet> generatePlayerBullet =
        (BulletID id, float posX, float posY, float speed, float angle) =>
        {
            var newObject = _playerBulletGenerator.GenerateObject(id, transformIntoVector2From(posX, posY));
            newObject.Shot(speed, angle);
            return newObject;
        };
        _script.Globals["GeneratePlayerBullet"] = generatePlayerBullet;

        Func<EnemyID, float, float, float, float, int, IEnemy> generateEnemy =
        (EnemyID id, float posX, float posY, float speed, float angle, int hitPoint) =>
        {
            var newObject = _enemyGenerator.GenerateObject(id, transformIntoVector2From(posX, posY));
            newObject.Spawned(speed, angle, hitPoint);
            return newObject;
        };
        _script.Globals["GenerateEnemy"] = generateEnemy;

        Func<PlayerID, float, float, IPlayer> generatePlayer =
        (PlayerID id, float posX, float posY) =>
        {
            var newObject = _playerGenerator.GenerateObject(id, transformIntoVector2From(posX, posY));
            newObject.Spawned();
            return newObject;
        };
        _script.Globals["GeneratePlayer"] = generatePlayer;

        Func<CommandID, bool> getKey = (CommandID id) => _mapping[id]();
        _script.Globals["GetKey"] = getKey;

        //Func<IEnumerator, Coroutine> invokeStartCoroutine = (IEnumerator routine) => StartCoroutine(routine);
        //Action<Coroutine> invokeStopCoroutine = (Coroutine coroutine) => StopCoroutine(coroutine);
        //_luaenv.Global.Set("csStartCoroutine", invokeStartCoroutine);
        //_luaenv.Global.Set("csStopCoroutine", invokeStopCoroutine);
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
            if (i % 30 == 0 && smallFairy.IsEnabled())
                _enemyBulletGenerator.GenerateObject(BulletID.SmallRedBullet, smallFairy.Position)
                .Shot(2.0f, Mathf.Atan2(_player.Position.y - smallFairy.Position.y, _player.Position.x - smallFairy.Position.x));
            smallFairy.Angle += Mathf.PI / 180;
            yield return null;
        }
        smallFairy.Speed = 0.0f;
    }
}

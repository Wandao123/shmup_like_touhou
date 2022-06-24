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
public class ScriptManager : IManagedBehaviour
{
    private IGameDirector _gameDirector;
    private ShmupInputActions _inputActions;
    private Dictionary<CommandID, Func<bool>> _mapping;
    private Script _script;

    // 参考：http://tawamuredays.blog.fc2.com/blog-entry-218.html
    public delegate Range AppliedFunc<Name, Args, Range>(Name name, params Args[] args);

    public ScriptManager(in IGameDirector gameDirector)
    {
        _gameDirector = gameDirector;

        // 入力関係の初期化。
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

        // Lua (MoonSharp) の設定。
        _script = new Script();
        _script.Options.ScriptLoader = new MoonSharp.Interpreter.REPL.ReplInterpreterScriptLoader();
        ((ScriptLoaderBase)_script.Options.ScriptLoader).ModulePaths = ScriptLoaderBase.UnpackStringPaths("Assets/lua_scripts/?;Assets/lua_scripts/?.lua");
        _script.Options.DebugPrint = s => _gameDirector.Print(s);
        registerClasses();
        registerConstants();
        registerGlueFunctions();
        UserData.RegisterAssembly();

        // C#のスクリプトを開始。
        //_gameDirector.StartCoroutine(playerScript());
        //_gameDirector.StartCoroutine(stageScript());

        // Luaのスクリプトを開始。
        _script.DoFile(_gameDirector.MainScriptFilename);
        _gameDirector.StartCoroutine(runLuaCoroutine(_script.Globals.Get("Main")));
    }

    public void ManagedFixedUpdate() {}

    public void ManagedUpdate()
    {
        // TODO: Unityのコルーチンに丸投げせずに、Luaのコルーチンを自前で管理するように変更。
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
        UserData.RegisterType<Bullet>();
        UserData.RegisterType<Enemy>();
        UserData.RegisterType<Player>();
    }

    private void registerConstants()
    {
        _script.Globals["Vector2"] = typeof(Vector2);
        _script.Globals["ScreenCenter"] = _gameDirector.ScreenBottomLeft + (_gameDirector.ScreenTopRight - _gameDirector.ScreenBottomLeft) * 0.5f;
        var halfOfHorizontalSide = (new Vector2(_gameDirector.ScreenTopRight.x - _gameDirector.ScreenBottomLeft.x, 0.0f)) * 0.5f;
        var halfOfVerticalSide = (new Vector2(0.0f, _gameDirector.ScreenTopRight.y - _gameDirector.ScreenBottomLeft.y)) * 0.5f;
        _script.Globals["ScreenTop"] = _gameDirector.ScreenTopRight - halfOfHorizontalSide;
        _script.Globals["ScreenBottom"] = _gameDirector.ScreenBottomLeft + halfOfHorizontalSide;
        _script.Globals["ScreenLeft"] = _gameDirector.ScreenBottomLeft + halfOfVerticalSide;
        _script.Globals["ScreenRight"] = _gameDirector.ScreenTopRight - halfOfVerticalSide;
        _script.Globals["PlayerSize"] = _gameDirector.PlayerSize;
        _script.Globals["BulletID"] = UserData.CreateStatic<BulletID>();
        _script.Globals["CommandID"] = UserData.CreateStatic<CommandID>();
        _script.Globals["EnemyID"] = UserData.CreateStatic<EnemyID>();
        _script.Globals["PlayerID"] = UserData.CreateStatic<PlayerID>();
    }

    private void registerGlueFunctions()
    {
        Func<BulletID, float, float, float, float, Bullet> generateBullet =
        (BulletID id, float posX, float posY, float speed, float angle) =>
        {
            var newObject = _gameDirector.GenerateObject(id, new Vector2(posX, posY));
            newObject.Shot(speed, angle);
            return newObject;
        };
        _script.Globals["GenerateBullet"] = generateBullet;

        Func<BulletID, float, float, float, float, Bullet> generatePlayerBullet =
        (BulletID id, float posX, float posY, float speed, float angle) =>
        {
            var newObject = _gameDirector.GenerateObject(id, new Vector2(posX, posY));
            newObject.Shot(speed, angle);
            return newObject;
        };
        _script.Globals["GeneratePlayerBullet"] = generatePlayerBullet;

        Func<EnemyID, float, float, float, float, int, Enemy> generateEnemy =
        (EnemyID id, float posX, float posY, float speed, float angle, int hitPoint) =>
        {
            var newObject = _gameDirector.GenerateObject(id, new Vector2(posX, posY));
            newObject.Spawned(speed, angle, hitPoint);
            return newObject;
        };
        _script.Globals["GenerateEnemy"] = generateEnemy;

        Func<PlayerID, float, float, Player> generatePlayer =
        (PlayerID id, float posX, float posY) =>
        {
            var newObject = _gameDirector.GenerateObject(id, new Vector2(posX, posY));
            newObject.Spawned();
            return newObject;
        };
        _script.Globals["GeneratePlayer"] = generatePlayer;

        Func<CommandID, bool> getKey = (CommandID id) => _mapping[id]();
        _script.Globals["GetKey"] = getKey;

        AppliedFunc<DynValue, DynValue, UnityEngine.Coroutine> luaStartCoroutineWithArgs =
        (func, args) => _gameDirector.StartCoroutine(runLuaCoroutine(func, args));
        _script.Globals["StartCoroutineWithArgs"] = luaStartCoroutineWithArgs;
        _script.DoString(@"
            function StartCoroutine(func, ...)
                StartCoroutineWithArgs(func, {...})
            end
        ");
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

    //**************** Luaを使わずにC#のみでゲーム・ロジックを記述する方法 ****************
    private Player _player;

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
            _player = _gameDirector.GenerateObject(PlayerID.Reimu, new Vector2(0.0f, _gameDirector.ScreenBottomLeft.y - _gameDirector.PlayerSize.y + InputDelayFrames));
            _player.Spawned();
            _player.TurnInvincible(InvincibleFrames / 2);
            yield break;
        }

        IEnumerator rebirth()
        {
            if (!_player.IsEnabled() && _player.HitPoint > 0)
            {
                _player.Position = new Vector2(0.0f, _gameDirector.ScreenBottomLeft.y - _gameDirector.PlayerSize.y);
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
                _player.Speed = Math.Sign(velocity.sqrMagnitude);
                _player.Angle = Mathf.Atan2(velocity.y, velocity.x);
                yield return null;
            }
        }

        IEnumerator shoot()
        {
            while (true)
            {
                if (_inputActions.Player.Shot.IsPressed())
                {
                    _gameDirector.GenerateObject(BulletID.ReimuNormalBullet, _player.Position - new Vector2(12.0f, 0.0f)).Shot(BulletSpeed, 0.5f * Mathf.PI);
                    _gameDirector.GenerateObject(BulletID.ReimuNormalBullet, _player.Position + new Vector2(12.0f, 0.0f)).Shot(BulletSpeed, 0.5f * Mathf.PI);
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
        var smallRedFairy = _gameDirector.GenerateObject(EnemyID.SmallRedFairy, new Vector2(_gameDirector.ScreenBottomLeft.x * 0.5f, _gameDirector.ScreenTopRight.y));
        smallRedFairy.Spawned(1.0f, -0.5f * Mathf.PI, 15);
        var smallBlueFairy = _gameDirector.GenerateObject(EnemyID.SmallBlueFairy, new Vector2(_gameDirector.ScreenTopRight.x * 0.5f, _gameDirector.ScreenTopRight.y));
        smallBlueFairy.Spawned(1.0f, -0.5f * Mathf.PI, 15);
        yield return wait(120);
        for (var i = 0; i <= 360; i++)
        {
            if (i % 6 == 0)
            {
                if (smallRedFairy.IsEnabled())
                    _gameDirector.GenerateObject(BulletID.ScaleRedBullet, smallRedFairy.Position)
                    .Shot(2.0f, Mathf.Atan2(_player.Position.y - smallRedFairy.Position.y, _player.Position.x - smallRedFairy.Position.x));
                if (smallBlueFairy.IsEnabled())
                    _gameDirector.GenerateObject(BulletID.ScaleBlueBullet, smallBlueFairy.Position)
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

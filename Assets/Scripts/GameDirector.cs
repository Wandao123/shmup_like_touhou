using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public interface IGameDirector
{
    string MainScriptFilename { get; }
    Dictionary<CommandID, Func<bool>> KeyMapping { get; }
    Vector2Int PlayerSize { get; }
    Vector2 ScreenBottomLeft { get; }
    Vector2 ScreenTopRight { get; }

    Bullet GenerateObject(BulletID id, in Vector2 position);
    Enemy GenerateObject(EnemyID id, in Vector2 position);
    Player GenerateObject(PlayerID id, in Vector2 position);
    void Print(object message);
    void Print(object message, UnityEngine.Object context); 
    Coroutine StartCoroutine(IEnumerator routine);
    Coroutine StartCoroutine(string methodName, object value=null);
}

public class GameDirector : MonoBehaviour, IGameDirector
{
    private const int ScreenWidth = 640;
    private const int ScreenHeight = 480;

    private FrameRateManager _frameRateManager;
    private IChangingSceneListener _listener;
    private ShmupInputActions _inputActions;
    private Dictionary<CommandID, Func<bool>> _mapping;
    private EnemyManager _enemyManager;
    private PlayerManager _playerManager;
    private BulletManager _enemyBulletManager;
    private BulletManager _playerBulletManager;
    private ScriptManager _scriptManager;
    [SerializeField]
    private string _mainScriptFilename = "Assets/lua_scripts/main.lua";
    [SerializeField]
    private PreloadedEnemies _preloadedEnemies;
    [SerializeField]
    private PreloadedPlayers _preloadedPlayers;
    [SerializeField]
    private PreloadedBullets _preloadedEnemyBullets;
    [SerializeField]
    private PreloadedBullets _preloadedPlayerBullets;

    public string MainScriptFilename { get => _mainScriptFilename; }
    // 本来はreadonlyな変数にしたいところだが、Unityではコンストラクタが呼べないため、プロパティで読み出す。
    public Dictionary<CommandID, Func<bool>> KeyMapping { get => _mapping; }
    public Vector2Int PlayerSize { get => _playerManager.CharacterSize; }
    public Vector2 ScreenBottomLeft { get => Camera.main.ViewportToWorldPoint(Vector2.zero); }
    public Vector2 ScreenTopRight { get => Camera.main.ViewportToWorldPoint(Vector2.one); }

    private void Awake()
    {
        // システムに関わるものの初期化。
        Screen.SetResolution(ScreenWidth, ScreenHeight, false);
        _frameRateManager = GetComponent<FrameRateManager>();

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

        // MoverManagerの子オブジェクトの初期化。
        _enemyManager = new EnemyManager(this.transform, _preloadedEnemies.GetTable());
        _playerManager = new PlayerManager(this.transform, _preloadedPlayers.GetTable());
        _enemyBulletManager = new BulletManager(this.transform, _preloadedEnemyBullets.GetTable());
        _playerBulletManager = new BulletManager(this.transform, _preloadedPlayerBullets.GetTable());
    }

    private void Start()
    {
        _listener = RootSceneAutoLoader.GetListener();
        _scriptManager = new ScriptManager(this);
    }

    private void Update()
    {
        if (KeyMapping[CommandID.Pause]())
        {
            enabled = false;
            _listener.Push("PauseScene");
        }

        if (Time.timeScale > 0f)
        {
            _scriptManager.ManagedUpdate();
            _enemyManager.ManagedFixedUpdate();
            _playerManager.ManagedFixedUpdate();
            _enemyBulletManager.ManagedFixedUpdate();
            _playerBulletManager.ManagedFixedUpdate();
            _enemyManager.ManagedUpdate();
            _playerManager.ManagedUpdate();
            _enemyBulletManager.ManagedUpdate();
            _playerBulletManager.ManagedUpdate();
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void OnDisable()
    {
        Time.timeScale = 0f;
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;
    }

    private void OnGUI()
    {
        var children = transform.childCount;
        var enemies = _enemyManager.ObjectCount;
        var players = _playerManager.ObjectCount;
        var enemyBullets = _enemyBulletManager.ObjectCount;
        var playerBullets = _playerBulletManager.ObjectCount;
        using (new GUILayout.VerticalScope())
        {
            GUILayout.Box("FPS " + _frameRateManager.AverageOfFPS.ToString());
            GUILayout.Box(string.Format("# of enabled objects {0:D" + ((int)Mathf.Log10(children) + 1).ToString() + "}/{1}",
                enemies + players + enemyBullets + playerBullets, children));
            GUILayout.Box($"# of tasks {_scriptManager.TaskCount}");
            GUILayout.FlexibleSpace();
        }
    }

    // 条件分岐するぐらいなら、初めからクラスを分割するべきか？
    public Bullet GenerateObject(BulletID id, in Vector2 position)
    {
        if (id < BulletID.DummyEnemyBullet)
            return _enemyBulletManager.GenerateObject(id, position);
        else
            return _playerBulletManager.GenerateObject(id, position);
    }

    public Enemy GenerateObject(EnemyID id, in Vector2 position) => _enemyManager.GenerateObject(id, position);
    public Player GenerateObject(PlayerID id, in Vector2 position) => _playerManager.GenerateObject(id, position);
    public void Print(object message) => Debug.Log(message);
    public void Print(object message, UnityEngine.Object context) => Debug.Log(message, context);
}

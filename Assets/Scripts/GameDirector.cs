using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameDirector
{
    string MainScriptFilename { get; }
    Vector2Int PlayerSize { get; }
    Vector2 ScreenBottomLeft { get; }
    Vector2 ScreenTopRight { get; }

    Bullet GenerateObject(BulletID id, in Vector2 position);
    Enemy GenerateObject(EnemyID id, in Vector2 position);
    Player GenerateObject(PlayerID id, in Vector2 position);
    void Print(object message);
    void Print(object message, Object context); 
    Coroutine StartCoroutine(IEnumerator routine);
    Coroutine StartCoroutine(string methodName, object value=null);
}

public class GameDirector : MonoBehaviour, IGameDirector
{
    private const int ScreenWidth = 640;
    private const int ScreenHeight = 480;

    private FrameRateManager _frameRateManager;
    private ScriptManager _scriptManager;
    private EnemyManager _enemyManager;
    private PlayerManager _playerManager;
    private BulletManager _enemyBulletManager;
    private BulletManager _playerBulletManager;
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
    public Vector2Int PlayerSize { get => _playerManager.CharacterSize; }
    public Vector2 ScreenBottomLeft { get => Camera.main.ViewportToWorldPoint(Vector2.zero); }
    public Vector2 ScreenTopRight { get => Camera.main.ViewportToWorldPoint(Vector2.one); }

    private void Awake()
    {
        Screen.SetResolution(ScreenWidth, ScreenHeight, false);
        _frameRateManager = GetComponent<FrameRateManager>();
        _enemyManager = new EnemyManager(this.transform, _preloadedEnemies.GetTable());
        _playerManager = new PlayerManager(this.transform, _preloadedPlayers.GetTable());
        _enemyBulletManager = new BulletManager(this.transform, _preloadedEnemyBullets.GetTable());
        _playerBulletManager = new BulletManager(this.transform, _preloadedPlayerBullets.GetTable());
    }

    private void Start()
    {
        _scriptManager = new ScriptManager(this);
    }

    private void FixedUpdate()
    {
        _enemyManager.ManagedFixedUpdate();
        _playerManager.ManagedFixedUpdate();
        _enemyBulletManager.ManagedFixedUpdate();
        _playerBulletManager.ManagedFixedUpdate();
    }

    private void Update()
    {
        _scriptManager.ManagedUpdate();
        _enemyManager.ManagedUpdate();
        _playerManager.ManagedUpdate();
        _enemyBulletManager.ManagedUpdate();
        _playerBulletManager.ManagedUpdate();
    }

    private void OnDestroy()
    {
        
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
    public void Print(object message, Object context) => Debug.Log(message, context);
}

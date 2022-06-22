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
    private ScriptManager _scriptManager;
    private EnemyManager _enemyManager;
    private PlayerManager _playerManager;
    private BulletManager _enemyBulletManager;
    private BulletManager _playerBulletManager;
    [SerializeField]
    private string _mainScriptFilename = "Assets/lua_scripts/main.lua";
    [SerializeField]
    private PreloadedBullets _preloadedBullets;

    public string MainScriptFilename { get => _mainScriptFilename; }
    // 本来はreadonlyな変数にしたいところだが、Unityではコンストラクタが呼べないため、プロパティで読み出す。
    public Vector2Int PlayerSize { get => _playerManager.CharacterSize; }
    public Vector2 ScreenBottomLeft { get => Camera.main.ViewportToWorldPoint(Vector2.zero); }
    public Vector2 ScreenTopRight { get => Camera.main.ViewportToWorldPoint(Vector2.one); }

    private void Awake()
    {
        _enemyManager = new EnemyManager();
        _playerManager = new PlayerManager();
        _enemyBulletManager = new BulletManager();
        _playerBulletManager = new BulletManager();
        _scriptManager = new ScriptManager(this);
    }

    private void Start()
    {
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
    }

    private void OnDestroy()
    {
        
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
    //public Coroutine RegisterCoroutine(string methodName, object value=null) => StartCoroutine(methodName, value);
    public void Print(object message) => Debug.Log(message);
    public void Print(object message, Object context) => Debug.Log(message, context);
}

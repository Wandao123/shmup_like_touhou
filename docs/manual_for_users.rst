一般向け説明書
**************

ここではLuaでの記述のために用意した関数群や、オブジェクト（自機、敵、弾、エフェクト）の追加方法を解説します。

仕様
====

- エンジンはUnityのインスペクタに指定されたファイルを読み込み、その中のMain関数を実行します。
- 全てのコルーチンとMain関数が終了していればタイトル画面に遷移します。
- オブジェクトが有効であるとき、オブジェクトの状態の更新と描画を行います。
- Unityの仕様に合わせて、座標系は画面右向きをx軸の正、画面上向きをy軸の正とします。それに伴って、x軸を基準とした反時計回りの方向を角度の正の向きと定めます。

スクリプト・リファレンス
========================

定義済み型
----------

Vector2
    2次元ベクトルを表すデータ型です。Unityの同名のクラスに対応します。
    
    #. **float x:** x座標の成分
    #. **float y:** y座標の成分
    #. メソッドはUnityEngine.CoreModule.Vector2に準じます。

定数
----

Vector2 ScreenCenter
    画面中心の座標を表します。

Vector2 ScreenTop
    画面中央上端の座標を表します。

Vector2 ScreenBottom
    画面中央下端の座標を表します。

Vector2 ScreenLeft
    画面左端中央の座標を表します。

Vector2 ScreenRight
    画面右端中央の座標を表します。

Vector2 PlayerSize
    自機のスプライト大きさを表します。x成分が横幅を、y成分が縦幅を意味します。

列挙型定数
----------

それぞれ \*\*\*ID.\ *EnumeratorName* の形でアクセスします。列挙型の定数として定義されています。

弾 (BulletID)
^^^^^^^^^^^^^

- 大弾
    - LargeRed
    - LargeBlue
- 中弾
    - MiddleRed
    - MiddleBlue
- 小弾
    - SmallRed
    - SmallBlue
- 粒弾
    - TinyRed
    - TinyBlue
- 鱗弾
    - ScaleRed
    - ScaleBlue
- 米弾
    - RicdRed
    - RicdBlue
- 自機のショット
    - ReimuNormal
    - MarisaNormal
    - SanaeNormal

入力コマンド (CommandID)
^^^^^^^^^^^^^^^^^^^^^^^^

- Shot
- Bomb
- Slow
- Skip
- Leftward
- Rightward
- Forward
- Backward
- Pause

エフェクト (EffectID)
^^^^^^^^^^^^^^^^^^^^^

*未実装*

- None
- DefetedPlayer
- RedCircle
- BlueCircle
- EnemyShotSound
- PlayerShotSound

敵 (EnemyID)
^^^^^^^^^^^^

- SmallRed
- SmallBlue

自機 (PlayerID)
^^^^^^^^^^^^^^^

- Reimu
- Marisa
- Sanae
- ReimuOption
- MarisaOption
- SanaeOption

画面 (SceneID)
^^^^^^^^^^^^^^

*未実装*

- Title（タイトル画面）
- StageClear（ステージ・クリア画面）
- AllClear（全ステージ・クリア画面）

生成関数
--------

Bullet GenerateBullet(BulletID id, float posX, float posY, float speed, float angle)
    敵弾オブジェクトを初期状態に従って生成します。

    #. **返り値:** 生成されたオブジェクト
    #. **id:** 生成する弾のID
    #. **posX:** 初期位置のx座標
    #. **posY:** 初期位置のy座標
    #. **speed:** 初期速度の大きさ（速さ）
    #. **angle:** 初期速度の角度（向き、度数法で指定）

Enemy GenerateEffect(EnemyID id, float posX = 0.e0, float posY = 0.e0)
    *未実装*

    エフェクトオブジェクトを指定の位置に生成します。

    #. **返り値:** 生成されたオブジェクト
    #. **id:** 生成するエフェクトのID
    #. **posX:** 生成位置のx座標（省略可）
    #. **posY:** 生成位置のy座標（省略可）

Enemy GenerateEnemy(EnemyID id, float posX, float posY, float speed, float angle, int hitPoint)
    敵オブジェクトを初期状態に従って生成します。

    #. **返り値:** 生成されたオブジェクト
    #. **id:** 生成する敵のID
    #. **posX:** 初期位置のx座標
    #. **posY:** 初期位置のy座標
    #. **speed:** 初期速度の大きさ（速さ）
    #. **angle:** 初期速度の角度（向き、度数法で指定）
    #. **hitPoint:** 敵の体力

Player GeneratePlayer(PlayerID id, float posX, float posY)
    自機オブジェクトを初期状態に従って生成します。

    #. **返り値:** 生成されたオブジェクト
    #. **id:** 生成する自機のID
    #. **posX:** 初期位置のx座標
    #. **posY:** 初期位置のy座標

Bullet GeneratePlayerBullet(BulletID id, float posX, float posY, float speed, float angle)
    自弾オブジェクトを初期状態に従って生成します。

    #. **返り値:** 生成されたオブジェクト
    #. **id:** 生成する弾のID
    #. **posX:** 初期位置のx座標
    #. **posY:** 初期位置のy座標
    #. **speed:** 初期速度の大きさ（速さ）
    #. **angle:** 初期速度の角度（向き、度数法で指定）


弾・敵・自機オブジェクトで共通の関数
------------------------------------

float *Object*.Angle property
    オブジェクトのx軸に対する回転角を表します。単位は度です。

int *Object*.Damage property (read only)
    当たり判定の際に相手のHitPointから差し引く数値を表します。

int *Object*.HitPoint property
    自機の場合は残機を、敵の場合は体力を表します。

\<Enum-like\> *Object*.ID
    オブジェクトの種類の識別子を表します。生成関数で指定したものと同じ値が入ります。

Vector2 *Object*.Position property
    オブジェクトの位置の座標を取得します。

float *Object*.Speed property
    オブジェクトの速さを表します。単位はドット毎フレームです。

void *Object*:Erase(void)
    オブジェクトを消去（無効に）します。

bool *Object*:IsEnabled(void)
    オブジェクトが有効ならばtrue、無効ならばfalseを返します。

敵・自機オブジェクトで共通の関数
--------------------------------

bool *Object*.InvincibleCount property
    無敵状態になっていられる残りのフレーム数を表します。

bool *Object*:IsInvincible(void)
    無敵状態か否かを真偽値で返します。

void *Object*:TurnInvincible(unsigned int frames)
    指定したフレーム数だけ無敵状態になります。

    #. **frames:** 無敵状態にするフレーム数。

弾オブジェクトの関数
--------------------

void *Bullet*:Shot(float speed, float angle)
    オブジェクトが無効の場合、指定された位置と角度でオブジェクトを有効化します。

敵オブジェクトの関数
--------------------

void *Enemy*:Spawned(float speed, float angle, int hitPoint)
    オブジェクトが無効の場合、指定された位置と角度と体力でオブジェクトを有効化します。

自機オブジェクトの関数
----------------------

bool *Player*.SlowMode property
    自機が低速移動中か否かを設定します。

void *Player*:Spawned(void)
    オブジェクトが無効の場合、現在の設定された位置でオブジェクトを有効化します。このとき、*Player*.HitPointが1減少します。

その他
------

void ChangeScene(SceneID id)
    *未実装*

    ゲーム画面からidで指定した画面に遷移します。StageClearを指定した場合、一旦ステージ・クリア画面に遷移した後にゲーム画面に戻り、Main関数の続きからスクリプトを実行します。AllClearを指定した場合、全ステージ・クリア画面に遷移した後にタイトル画面に遷移します。

    #. **id:** 遷移先の画面のID

bool GetKey(CommandID command)
    コマンド入力を検知します。commandに対応するキー/ボタンが押し続けられている間、trueを返します。

    #. **返り値:** キー/ボタンが押し続けられているか否か
    #. **command:** 検知するコマンド

bool GetKeyDown(CommandID command)
    *未実装*
 
    コマンド入力を検知します。commandに対応するキー/ボタンが押された瞬間、trueを返します。

    #. **返り値:** キー/ボタンが押されたか否か
    #. **command:** 検知するコマンド

bool GetKeyUp(CommandID command)
    *未実装*

    コマンド入力を検知します。commandに対応するキー/ボタンが離された瞬間、trueを返します。

    #. **返り値:** キー/ボタンが離されたか否か
    #. **command:** 検知するコマンド

Thread StartCoroutine(func) [overloaded]
    毎フレーム実行するコルーチンfuncを登録します。Luaのresume関数と異なり、1フレーム毎に自動的に実行されます。この関数を呼び出した時点でfuncは1回実行されます。funcには引数を渡しません。

    #. **返り値:** 登録したコルーチンに対するスレッド
    #. **func:** Luaの関数

Thread StartCoroutine(func, arg1, arg2, ...) [overloaded]
    毎フレーム実行するコルーチンfuncを登録します。Luaのresume関数と異なり、1フレーム毎に自動的に実行されます。この関数を呼び出した時点でfuncは1回実行されます。funcには arg1, arg2, ... の形式で引数を渡します。

    #. **返り値:** 登録したコルーチンに対するスレッド
    #. **func:** Luaの関数
    #. **(arg1, arg2, ...):** 引数リスト

void StopCoroutine(thread)
    指定したコルーチンを停止します。

    #. **thread:** 停止するスレッド

# 一般向け説明書

ここではLuaでの記述のために用意した関数群や、オブジェクト（自機、敵、弾、エフェクト）の追加方法を解説します。

## 仕様

- エンジンはUnityのインスペクタに指定されたファイルを読み込み、その中のMain関数を実行します。
- 全てのコルーチンとMain関数が終了していればタイトル画面に遷移します。
- オブジェクトが有効であるとき、オブジェクトの状態の更新と描画を行います。
- Unityの仕様に合わせて、座標系は画面右向きをx軸の正、画面上向きをy軸の正とします。それに伴って、x軸を基準とした反時計回りの方向を角度の正の向きと定めます。

## スクリプト・リファレンス

### 定義済み型

<dl>
    <dt>Vector2</dt>
    <dd>2次元ベクトルを表すデータ型です。Unityの同名のクラスに対応します。</dd>
    <dd>
        <ol>
            <li><b>float x: </b>x座標の成分</li>
            <li><b>float y: </b>y座標の成分</li>
            <li>メソッドはUnityEngine.CoreModule.Vector2に準じます。</li>
        </ol>
    </dd>
</dl>

### 定数

<dl>
    <dt>Vector2 ScreenCenter</dt><dd>画面中心の座標を表します。</dd>
    <dt>Vector2 ScreenTop</dt><dd>画面中央上の座標を表します。</dd>
    <dt>Vector2 ScreenBottom</dt><dd>画面中央下の座標を表します。</dd>
    <dt>Vector2 ScreenLeft</dt><dd>画面左中央の座標を表します。</dd>
    <dt>Vector2 ScreenRight</dt><dd>画面右中央の座標を表します。</dd>
    <dt>Vector2 PlayerSize</dt><dd>自機のスプライト大きさを表します。x成分が横幅を、y成分が縦幅を意味します。</dd>
</dl>

### 列挙型定数

それぞれ \*ID.*name* の形でアクセスします。列挙型の定数として定義されています。

#### 弾 (BulletID)

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

#### 入力コマンド (CommandID)

- Shot
- Bomb
- Slow
- Skip
- Leftward
- Rightward
- Forward
- Backward
- Pause

#### エフェクト (EffectID)

*未実装*
- None
- DefetedPlayer
- RedCircle
- BlueCircle
- EnemyShotSound
- PlayerShotSound

#### 敵 (EnemyID)

- SmallRed
- SmallBlue

#### 自機 (PlayerID)

- Reimu
- Marisa
- Sanae
- ReimuOption
- MarisaOption
- SanaeOption

#### 画面 (SceneID)

*未実装*
- Title（タイトル画面）
- StageClear（ステージ・クリア画面）
- AllClear（全ステージ・クリア画面）

### 生成関数

<dl>
    <dt>Bullet GenerateBullet(BulletID id, float posX, float posY, float speed, float angle)</dt>
    <dd>敵弾オブジェクトを初期状態に従って生成します。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>生成されたオブジェクト</li>
            <li><b>id: </b>生成する弾のID</li>
            <li><b>posX: </b>初期位置のx座標</li>
            <li><b>posY: </b>初期位置のy座標</li>
            <li><b>speed: </b>初期速度の大きさ（速さ）</li>
            <li><b>angle: </b>初期速度の角度（向き、度数法で指定）</li>
        </ol>
    </dd>
    <!-- -->
    <dt>Enemy GenerateEffect(EnemyID id, float posX = 0.e0, float posY = 0.e0)</dt>
    <dd><i>未実装</i></dd>
    <dd>エフェクトオブジェクトを指定の位置に生成します。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>生成されたオブジェクト</li>
            <li><b>id: </b>生成するエフェクトのID</li>
            <li><b>posX: </b>生成位置のx座標（省略可）</li>
            <li><b>posY: </b>生成位置のy座標（省略可）</li>
        </ol>
    </dd>
    <!-- -->
    <dt>Enemy GenerateEnemy(EnemyID id, float posX, float posY, float speed, float angle, int hitPoint)</dt>
    <dd>敵オブジェクトを初期状態に従って生成します。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>生成されたオブジェクト</li>
            <li><b>id: </b>生成する敵のID</li>
            <li><b>posX: </b>初期位置のx座標</li>
            <li><b>posY: </b>初期位置のy座標</li>
            <li><b>speed: </b>初期速度の大きさ（速さ）</li>
            <li><b>angle: </b>初期速度の角度（向き、度数法で指定）</li>
            <li><b>hitPoint: </b>敵の体力</li>
        </ol>
    </dd>
    <!-- -->
    <dt>Player GeneratePlayer(PlayerID id, float posX, float posY)</dt>
    <dd>自機オブジェクトを初期状態に従って生成します。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>生成されたオブジェクト</li>
            <li><b>id: </b>生成する自機のID</li>
            <li><b>posX: </b>初期位置のx座標</li>
            <li><b>posY: </b>初期位置のy座標</li>
        </ol>
    </dd>
    <!-- -->
    <dt>Bullet GeneratePlayerBullet(BulletID id, float posX, float posY, float speed, float angle)</dt>
    <dd>自弾オブジェクトを初期状態に従って生成します。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>生成されたオブジェクト</li>
            <li><b>id: </b>生成する弾のID</li>
            <li><b>posX: </b>初期位置のx座標</li>
            <li><b>posY: </b>初期位置のy座標</li>
            <li><b>speed: </b>初期速度の大きさ（速さ）</li>
            <li><b>angle: </b>初期速度の角度（向き、度数法で指定）</li>
        </ol>
    </dd>
</dl>

### 弾・敵・自機オブジェクトで共通の関数

<dl>
    <dt>float <i>Object</i>.Angle property</dt>
    <dd>オブジェクトのx軸に対する回転角を表します。単位は度です。</dd>
    <!-- -->
    <dt>int <i>Object</i>.Damage property (read only)</dt>
    <dd>当たり判定の際に相手のHitPointから差し引く数値を表します。</dd>
    <!-- -->
    <dt>int <i>Object</i>.HitPoint property</dt>
    <dd>自機の場合は残機を、敵の場合は体力を表します。</dd>
    <!-- -->
    <dt>&langle;Enum-like type&rangle; <i>Object</i>.ID</dt>
    <dd>オブジェクトの種類の識別子を表します。生成関数で指定したものと同じ値が入ります。</dd>
    <!-- -->
    <dt>Vector2 <i>Object</i>.Position property</dt>
    <dd>オブジェクトの位置の座標を取得します。</dd>
    <!-- -->
    <dt>float <i>Object</i>.Speed property</dt>
    <dd>オブジェクトの速さを表します。単位はドット毎フレームです。</dd>
    <!--
    <dt>Vector2 <i>Object</i>.Velocity property</dt>
    <dd>オブジェクトの速度を表します。単位はドット毎フレームです。</dd>-->
    <!-- -->
    <dt>void <i>Object</i>:Erase(void)</dt>
    <dd>オブジェクトを消去（無効に）します。</dd>
    <!-- -->
    <dt>bool <i>Object</i>:IsEnabled(void)</dt>
    <dd>オブジェクトが有効ならばtrue、無効ならばfalseを返します。</dd>
</dl>

### 敵・自機オブジェクトで共通の関数

<dl>
    <dt>bool <i>Object</i>.InvincibleCount property</dt>
    <dd>無敵状態になっていられる残りのフレーム数を表します。</dd>
    <!-- -->
    <dt>bool <i>Object</i>:IsInvincible(void)</dt>
    <dd>無敵状態か否かを真偽値で返します。</dd>
    <!-- -->
    <dt>void <i>Object</i>:TurnInvincible(unsigned int frames)</dt>
    <dd>指定したフレーム数だけ無敵状態になります。</dd>
    <dd>
        <ol>
            <li><b>frames: </b>無敵状態にするフレーム数。</li>
        </ol>
    </dd>
</dl>

### 弾オブジェクトの関数

<dl>
    <dt>void <i>Bullet</i>:Shot(float speed, float angle)</dt>
    <dd>オブジェクトが無効の場合、指定された位置と角度でオブジェクトを有効化します。</dd>
</dl>

### 敵オブジェクトの関数

<dl>
    <dt>void <i>Enemy</i>:Spawned(float speed, float angle, int hitPoint)</dt>
    <dd>オブジェクトが無効の場合、指定された位置と角度と体力でオブジェクトを有効化します。</dd>
</dl>

### 自機オブジェクトの関数

<dl>
    <dt>bool <i>Player</i>.SlowMode property</dt>
    <dd>自機が低速移動中か否かを設定します。</dd>
    <!-- -->
    <dt>void <i>Player</i>:Spawned(void)</dt>
    <dd>オブジェクトが無効の場合、現在の設定された位置でオブジェクトを有効化します。このとき、<i>Player</i>.HitPointが1減少します。</dd>
</dl>

### その他

<dl>
    <dt>void ChangeScene(SceneID id)</dt>
    <dd><i>未実装</i></dd>
    <dd>ゲーム画面からidで指定した画面に遷移します。StageClearを指定した場合、一旦ステージ・クリア画面に遷移した後にゲーム画面に戻り、Main関数の続きからスクリプトを実行します。AllClearを指定した場合、全ステージ・クリア画面に遷移した後にタイトル画面に遷移します。</dd>
    <dd>
        <ol>
            <li><b>id: </b>遷移先の画面のID</li>
        </ol>
    </dd>
    <!-- -->
    <dt>bool GetKey(CommandID command)</dt>
    <dd>コマンド入力を検知します。commandに対応するキー/ボタンが押し続けられている間、trueを返します。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>キー/ボタンが押し続けられているか否か</li>
            <li><b>command: </b>検知するコマンド</li>
        </ol>
    </dd>
    <!-- -->
    <dt>bool GetKeyDown(CommandID command)</dt>
    <dd><i>未実装</i></dd>
    <dd>コマンド入力を検知します。commandに対応するキー/ボタンが押された瞬間、trueを返します。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>キー/ボタンが押されたか否か</li>
            <li><b>command: </b>検知するコマンド</li>
        </ol>
    </dd>
    <!-- -->
    <dt>bool GetKeyUp(CommandID command)</dt>
    <dd><i>未実装</i></dd>
    <dd>コマンド入力を検知します。commandに対応するキー/ボタンが離された瞬間、trueを返します。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>キー/ボタンが離されたか否か</li>
            <li><b>command: </b>検知するコマンド</li>
        </ol>
    </dd>
    <!-- -->
    <dt>Thread StartCoroutine(func) [overloaded]</dt>
    <dd>毎フレーム実行するコルーチンfuncを登録します。Luaのresume関数と異なり、1フレーム毎に自動的に実行されます。この関数を呼び出した時点でfuncは1回実行されます。funcには引数を渡しません。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>登録したコルーチンに対するスレッド</li>
            <li><b>func: </b>Luaの関数</li>
        </ol>
    </dd>
    <!-- -->
    <dt>Thread StartCoroutine(func, arg1, arg2, ...) [overloaded]</dt>
    <dd>毎フレーム実行するコルーチンfuncを登録します。Luaのresume関数と異なり、1フレーム毎に自動的に実行されます。この関数を呼び出した時点でfuncは1回実行されます。funcには arg1, arg2, ... の形式で引数を渡します。</dd>
    <dd>
        <ol>
            <li><b>返り値: </b>登録したコルーチンに対するスレッド</li>
            <li><b>func: </b>Luaの関数</li>
            <li><b>(arg1, arg2, ...): </b>引数リスト</li>
        </ol>
    </dd>
    <!-- -->
    <dt>void StopCoroutine(thread)</dt>
    <dd>指定したコルーチンを停止します。</dd>
    <dd>
        <ol>
            <li><b>thread: </b>停止するスレッド</li>
        </ol>
    </dd>
</dl>

# Shmup like Touhou（仮称）

マルチプラットフォームな弾幕STG用ゲームフレームワークとして制作しています。エンジン部分はUnityで開発しており、ゲームロジック（敵や弾の動き）はLuaで記述します。

## 必要要件

### 素材

本プログラムは『東方弾幕風』の素材を使用しております。そのリンクを以下に示します。このような素材をご公開されている皆様にはこの場を借りて感謝申し上げます。素材の関係上、現在のところ本プログラムは東方Projectの二次創作ガイドラインに則ります。（今後自作の素材で置き換えるかもしれません）。

- [自機・自弾・敵画像](http://coolier.dip.jp/th_up4/index.php?id=6360)
    - Enemy.png
    - Marisadot.png
    - Reimudot.png
    - Sanaedot.png
    - Shot1.png
    - Shot2.png
    - Shot3.png
- [敵弾画像](http://coolier.dip.jp/th_up3/file/th3_4065.lzh) （えむ様）
    - shot_all.png
- [エフェクト画像](http://coolier.dip.jp/th_up3/file/th3_7474.lzh) （貴方が作る東方STG～東方弾幕風8の880様）
    - effect_circle.png
- [敵SE](http://coolier.dip.jp/th_up4/index.php?id=2637) （４１３y様）
    - enemy_damage.wav
    - enemy_vanish[エフェクトＡ].wav --> rename this to enemy_vanish_effect-A.wav
    - shot1.wav
<!--
- [被弾音](https://commons.nicovideo.jp/material/nc899) （koshibone様）
    - nc899.wav
- [ショット音](http://osabisi.sakura.ne.jp/m2/tm4/se/se_old_pack00.zip) （Osabisi様）
    - sha04.wav
-->

- 参考：https://danmakufu.wiki.fc2.com/wiki/%E7%B4%A0%E6%9D%90%E3%83%AA%E3%83%B3%E3%82%AF

### Unityのバージョン

以下のバージョンでテストしています。
- 2021.3.5f1

### 依存ライブラリ

- [MoonSharp](https://www.moonsharp.org/) == 2.0.0.0

## 操作方法

現在のところ、キーボードあるいはXInputのゲーム・パッドで操作できます。

- **移動・選択：** 方向キー
- **ショット・決定：** Zキー
- **低速移動：** Shiftキー

<!--現在の設定では3回被弾したらゲームオーバーになります。また、ゲーム開始時にデータを読み込むため、数秒程度ラグが生じます。-->

## ビルド方法（暫定）

### テスト

1. GitHubからソースコードをダウンロードする。
1. [Unity Hub](https://unity3d.com/jp/get-unity/download)をダウンロードする（それに際してはUnityのユーザー登録をする必要がある）。起動し、メニューの「インストール」から適当なバージョンのUnityをインストールする。
1. メニューの「プロジェクト」→「リストに追加」から、ダウンロードしたソースコードをUnity Hubの管理下におく。
1. リストのに追加されたプロジェクト名をクリックすると、Unityが立ち上がる。エラーが発生するが、これは必要なアセットが導入されていないのが原因である。そのまま "safe mode" ボタンを押して起動する。
1. メニューバーの "Window" → "Asset Store" をクリックして、表示された画面の "Search online" ボタンをクリックする。さもなければ、[Unity Asset Store](https://assetstore.unity.com/)をブラウザで直接開く。
1. 検索バーに "MoonSharp" と入力し、それをマイアセットに追加する。
1. Unityに戻り、メニューバーの "Window" → "Package Manager" をクリックする。開かれたウィンドウ上部の "Packages: ..." というプルダウンメニューから、"My Assets" を選択する。"MoonSharp" を選んで "Import" をクリックする。
1. 「ヒエラルキー」からGameSceneを開く。さらに「ゲーム」画面を開き、アスペクト比に「640×480」を追加する。
1. 三角形の「実行ボタン」をクリックする。

### 実行ファイルの生成

1. 問題無く[テスト](#テスト)が実行されることを確認する。
1. メニューバーの "Window" → "Asset Management" → "Addressables" → "Groups" をクリックする。開かれたウィンドウの上部にある "Build" → "New Build" → "Default Build Script" をクリックする。
1. メニューバーの "File" → "Build And Run" をクリックし、開かれたウィンドウの "Build" ボタンをクリックする。
1. ダイアログが立ち上がった場合は、実行ファイルの生成先として適当なディレクトリを指定する。

## ドキュメント

Luaによってゲームスクリプトを書く方法は[ここのマニュアル](docs/manual_for_users.rst)を参照してください。また、本フレームワークのクラス設計に関して、[PlantUML](https://plantuml.com)による図を `docs/*.pu` に置きました。適宜Javaを用いて生成するか、あるいはブラウザに[PlantUML Visualizer](https://github.com/WillBooster/plantuml-visualizer)を導入することで、GitHub上でも閲覧できます。

## ライセンス

This software is distributed under the terms of the MIT license reproduced [here](LICENSE).

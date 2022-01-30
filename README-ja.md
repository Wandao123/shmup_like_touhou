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
- 2021.2.7f1

### 依存ライブラリ

- [MoonSharp](https://www.moonsharp.org/) == 2.0.0.0

## 操作方法

- **移動・選択：** 方向キー
- **ショット・決定：** Zキー
- **低速移動：** Shiftキー

<!--現在の設定では3回被弾したらゲームオーバーになります。また、ゲーム開始時にデータを読み込むため、数秒程度ラグが生じます。-->

## ビルド方法（暫定）

1. GitHubからソースをダウンロードする。
1. Unity Hubから適当なバージョンのUnityをインストールする。
1. Unity Asset Storeから依存ライブラリをインストールする。
1. 実行ボタンをクリックする。

## ライセンス

This software is distributed under the terms of the MIT license reproduced [here](LICENSE).

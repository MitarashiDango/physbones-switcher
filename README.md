# PhysBones Switcher

## これはなに？

- VRChat 向けアバターで使用されている VRC PhysBone コンポーネントを、ゲーム内で動的にオンオフするメニューを追加するコンポーネント

## どうやって使うの

### インストール

#### VPM Repository を使用する方法

1. VPM リポジトリー<https://vpm.matcha-soft.com/repos.json>を VCC へ登録する
2. 「PhysBones Switcher」をプロジェクトへ追加する

#### UnityPackage から導入する方法

1. リリース一覧より最新の unitypackage ファイルをダウンロードする
2. ダウンロードしたファイルを Unity へドラッグ＆ドロップし、インポートする

### Unity Editor 上での操作

1. アバターオブジェクト配下に新しくゲームオブジェクトを作成し、`MA Menu Installer` と `MA Menu Group` をオブジェクトへ追加する。
2. 作成したゲームオブジェクトの子オブジェクトとして、さらにゲームオブジェクトを作成する。
3. 項番 2 で作成したゲームオブジェクトを選択し、`Add Component > PhysBones Switcher > PhysBones Switcher` の順に操作し、コンポーネントを追加する。

- 任意のメニュー階層に追加する場合、項番 1 を実施せず、項番 2 でメニュー項目を作成したい親オブジェクトのは以下に子オブジェクトを作成する。

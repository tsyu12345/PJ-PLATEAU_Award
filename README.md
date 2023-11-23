# PLATEAU Run デモ版(Ver 0.0.2)
![Alt text](/DocsImgs/demo2.png)
# 環境要件
本プロジェクトは以下の言語、パッケージを必要とし、下記記載のOSで動作を保証します。
* マシン/OS : Intel Mac / MacOS Ventura 13.3.1
* 言語環境
    * Python 3.11以上
    * Unity 2021.3.31f1
    * Xcode Ver 14.3
    * iOS 15.8以上のデバイス

# 環境構築編
本プロジェクトでは、以下の3つの環境で動作するため、clone or ダウンロード後は以下の手順で、それぞれ環境構築が必要です。

## 【Webサーバ】
 本デモのWebサーバは`FastAPI`で動作します.
 通常の`pip`による環境構築は以下の手順です。
### 1. 仮想環境の用意

 プロジェクトルートから、以下のディレクトリに移動し、仮想環境を構築します。
 ```bash
 cd WebServer
 ```
 その後、`venv`による仮想環境構築を実行します。
 ```bash
 python -m venv .venv
 ```
 仮想環境をアクティベートします。
 ```bash
 source venv/bin/activate

 after activate ...
 (.venv) $ <- この様にプロンプトの先頭に(.venv)が付き仮想環境が有効になっていることを確認します。
 ```

### 2. `pip install`の実行
 依存関係をインストールします。
```bash
pip install -r requirements.txt
```

## 【Xcode側】
XcodeがインストールされているMacであればXcode側で追加のインストール等は必要ありません。

## 【Unity側】
本プロジェクトではPLATEAUの大容量ファイルを扱います。クローン後は以下の共有フォルダから`LFSファイル一覧`にあるZipファイルをダウンロードして、指定の展開先に展開する必要があります。

`【重要】PLATEAU試験マップは現在準備中です。`
本デモでは使用しないため、この手順はスキップしても問題ありません。

### Zipファイル格納先
https://drive.google.com/open?id=1eyTGow0vdNQ8ac8KhpnXQFOiisHOQgDK&usp=drive_fs

### LFS ファイル一覧 (最終更新日: 2023/11/16)
* StreamingAssets.zip
    * PLATEAUマップのテクスチャが含まれています。
    * 展開先 : `"./PLATEAU-run Client/Assets/StreamingAssets"`

* Scenes.zip
    * PLATEAUをUnityで展開した際のシーン情報です。
    * 内容物を以下のディレクトリに展開・配置してください。
    * 展開先 : `"./PLATEAU-run Client/Assets/Scenes"`



# デモ起動編
以下の準備では、Webサーバー、iOS側アプリ、Unity側アプリの3つの環境を起動します。

この時、サーバー（Mac）とクライアント（iOS側アプリ、Unity側アプリ）は同じネットワークに接続されている必要があります。
`同じLAN（Wi-Fi）に接続されていることを確認してください。`

また、この後表示されるポート番号（今回は`8000番`）が解放されている必要があります。
`ファイアウォール等でポート番号がブロックされていないことを確認してください。`

## 1.【Webサーバ】Webサーバを起動する。
1. プロジェクトのルートから、以下のコマンドで`WebServer`ディレクトリに遷移します。
```bash
cd WebServer
```
2. 以下のコマンドを実行し、`FastAPI`サーバを起動します。
```bash
uvicorn webserver.main:app --host=0.0.0.0
```
以下のような表示がでれば完了です。

![Alt text](/DocsImgs/FastAPI-demo.jpg)

## 2. 【iOS】iOS側アプリの準備
1. Xcodeで以下のプロジェクトを読み込みます。
```bash
./MotionInputer-iOS/MotionInputer-iOS.xcodeproj
```

2. iPhoneをMacと接続し、Xcodeのビルド設定を以下の画像のように変更します。
* 接続しているデバイスの名前を選択します。
![Alt text](/DocsImgs/xcode-build-telop.jpg)

3. 下記画像の赤丸部分をクリックし、アプリケーションのビルドと実行を行います。
![Alt text](/DocsImgs/xcode-demo.jpg)
* ビルドが完了すると、iPhone側にアプリがインストールされます。
* iPhone用の開発者設定が有効になっていることを確認してください。

4. アプリケーションが起動したら、Webサーバーに表示されているIPアドレスを入力します。
* サーバーのコンソールには以下のように青字でアドレスが示されています。
    ![Alt text](/DocsImgs/webserver-ip.jpg)
    * この場合は`192.168.28.231:8000`
* このアドレスをiOS側アプリのテキストフィールドに入力します。
    ![Alt text](/DocsImgs/ios-app-ip.jpg)


##  3. 【Unity側】クライアントの実行
1. Unity Hubより以下のプロジェクトを読み込んでください。
```
./PLATEAU-run Client
```
2. デモ用シーンは以下のディレクトりにあります。Unity側から選択して読み込んでください。
```
Assets/Scenes/SampleScene
```
3. シーンを読み込んだら、いつも通り実行ボタン（▶）を押してください。

# デモの操作方法
以上の準備が完了したら、以下の方法でデモの操作ができます。
* iOS側アプリの`start Motioning`ボタンを押すと、iOS側の加速度センサーが起動し、腕の振りの動きを検知します。
    * センサー起動中は赤い表示に変わります。
    * センサーを停止する場合は、`stop Motioning`ボタンを押してください。
* 腕の振りの大きさに応じて、表示されているセンサーの値が変化します。

* センサーの値が`1.5以上`になると、キャラクターは通常の歩行から加速状態になります。
    * それ以下では通常の歩行に戻ります。


以上でデモを実行する事ができます。
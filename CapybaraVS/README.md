# CapyCSS
![sample](https://user-images.githubusercontent.com/63950487/92327819-4bc2b180-f097-11ea-832d-3077861a8dea.png)

## 特徴
* ビジュアルなスクリプトを作成することができます。
* c#で書かれたソースを修正して普通のメソッドを追加することでノードとして使用できるようにインポートする機能があります（属性の指定とビルドが必要です）。

## ターゲット環境
* .Net Core 3.1
* c＃

## 実行オプション
保存したスクリプトファイルをコマンド引数で指定すると、起動時に自動的に読み込まれます。
オプションとして-asが併せて指定されている場合、「Entry」をチェックされたノードは、スクリプトのロード後に自動で実行されます。
オプションとして-aeを指定した場合、スクリプト実行後に自動的に終了します。
オプションとして-aseを指定した場合、-asと-aeを併せて実行した場合と同様になります。
```
CapyCSS.exe script.cbs
CapyCSS.exe -as script.cbs
CapyCSS.exe -as -ae script.cbs
CapyCSS.exe -ase script.cbs
```
「SetExitCode」ノードを使って終了コードをセットできます。
-ae および -ase オプション時は、「ConsolOut」ノードでコンソールにメッセージを出力できます。
これらを使って、バッチ処理的なことができます。

## ノードのインポート
本ツールでは、c# の多くのメソッドを簡単にノード化することができます。
下記は、属性を使用してメソッドをノード化する例です。
```
[ScriptMethod]
public static System.IO.StreamReader GetReadStream(string path, string encoding = "utf-8")
{
    var encodingCode = Encoding.GetEncoding(encoding);
    return new System.IO.StreamReader(path, encodingCode);
}
```

## 操作方法
* 画面左側のコマンドメニューから項目をクリックしてノードを配置できます。
* ノードの○を左クリックしてドラッグし、ノードの□に接続できます。
* Ctrlキーを押しながら□を左クリックすると、接続しているノードを切断できます。
* Shiftキーを押しながらマウスをドラッグして範囲内のノードを複数選択します。
* Deleteキーで選択されているノードを削除できます。
* ノード名を左クリックしてノードをドラッグ移動できます（Ctrlキーを押下した状態だと、ガイドに沿って移動します）。
* 画面を左クリックして画面全体をドラッグ移動できます（選択されたノードがある場合、選択されているノードが移動します）。
* ホイールボタンの回転で表示を拡大もしくは縮小できます。
* Ctrl + Cで選択したノードをコピーできます。
* Ctrl + Vでコピーされているノードを貼り付けられます。
* Ctrl + Sでスクリプトを保存できます。
* Ctrl + Oでスクリプトを読み込みできます。
* F5でEntryをチェックされたノードをすべて実行できます。

※ノード名、引数名はダブルクリックで編集できます。ただし、変数名は画面左側の変数名一覧でのみダブルクリックで編集できます。

## スクリプトの便利な機能
* 引数の上にカーソルを置くと内容を確認できます。
* 数字と文字をドラッグアンドドロップするだけで定数ノードを作成できます。
* ノードの接続時に型キャストが可能です（オブジェクト型からのキャストはサポートしていません）。

## ヒント表示
英語と日本語をサポートします。ただし、初期状態では英語のみの機械翻訳です。
日本語で表示するには、実行後に作成されるapp.xmlファイルの内容を変更します。
```<Language>en-US</Language>```
上記の内容を次のように変更します。
```<Language>ja-JP</Language>```
再起動すると設定が適用されます。

## メソッドのインポートでサポートされるメソッド
* Static method
* Class method(thisを受け取る引数が追加されます)

## スクリプトが対応するメソッド引数
* Type: int, string, double, byte, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* Class
* Interface
* Enum
* Generics
* Default arguments
* Reference
* Overload

## スクリプトが対応するメソッドの戻り型
* Type: int, string, double, byte, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* Class
* Interface
* Enum
* Generics
* void

## メソッドからのメソッド呼び出し
Func<> および Action<> タイプの引数は、ノードを外部プロセスとして呼び出すことができます。Func<> の場合、ノード型と戻り値型が一致した場合に接続できます。Action だと無条件に接続できます。

## ライセンス
MITライセンス

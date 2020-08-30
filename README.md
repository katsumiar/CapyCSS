# CapyCSS
![sample](https://user-images.githubusercontent.com/63950487/91655061-fd3a7380-eae8-11ea-97e2-f4d868b25e97.png)

## まとめ
* メモを書くときにマウスを使って簡単にプログラムを書くことができます。
* ソースを変更してメソッドを追加することでノードとして使用できるようにインポートする機能があります（ビルドが必要です）。

## 目標
* .Net Core 3.1
* c＃

## オプション
保存したスクリプトファイルをコマンド引数で指定すると、起動時に自動的に読み込まれます。オプションに-asが指定されている場合、「public」として指定されたノードは、スクリプトのロード後に実行されます。オプションに-aeを指定した場合、スクリプト実行後に自動的に終了します。
```
CapybaraVS.exe script.cbs
CapybaraVS.exe -as script.cbs
CapybaraVS.exe -as -ae script.cbs
```

## 特徴
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
* 画面左側のメニューから項目をクリックしてノードを配置します。
* ノードの○を左クリックしてドラッグし、ノードの□に接続します。
* Ctrlキーを押しながら□を左クリックすると、ノードが切断されます。
* Shiftキーを押しながらマウスをドラッグして範囲内のノードを複数選択します。
* Deleteキーで選択されているノードを削除します。
* 左クリックでノードを移動できます（Ctrlキーを押下した状態だと、ガイドに沿って移動します）。
* 画面を左クリックしてドラッグすると、画面全体が移動します（選択されたノードがある場合、選択されているノードが移動します）。
* ホイールボタンで表示を拡大もしくは縮小します。
* Ctrl + Cで選択したノードをコピーします。
* Ctrl + Vでコピーされているノードを貼り付けます。
* Ctrl + Sでスクリプトを保存します。
※ノード名、引数名はダブルクリックで編集できます。ただし、変数名は画面左側の変数名一覧でのみダブルクリックで編集できます。

## スクリプトの便利な機能
* 引数の上にカーソルを置くと内容を確認できます。
* 数字と文字をドラッグアンドドロップするだけで定数ノードを作成できます。
* ノードの接続時に型キャストが可能です（オブジェクトからのキャストはサポートしていません）。

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

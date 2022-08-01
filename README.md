# CapyCSS
![CapyCSS_password2](https://user-images.githubusercontent.com/63950487/181904549-6c6910e5-5415-4408-a224-3f907c37e745.gif)
※新しく追加されたpassword型は、場合によって秘密属性がストリップされるので注意。<br>
※このイメージは、ScreenToGif によって作成されています。

## 0.6.0.0 での変更
* プロジェクトファイルをプロジェクトディレクトリ下で管理するようにしました。

### 0.6.0.0 で対応した不具合
* リストを管理するノードで実際の型に関係なく「IEnumerable」と表示される。
* CapyCSSのシステムの型の名前変換が正しくない。
* スクリプトの全体移動の動きが変（マウスカーソルが滑る？）。
* ショートカットノードコマンドを複数選択すると配置後にマウスカーソルが元に戻らない。
* pasword型の編集でフォーカスを失っても値が実際に反映していない。
* 新規スクリプト「New～」の状態でCtrl+sで保存時にタブ中のスクリプト名が反映されない。

## 特徴
* ビジュアルなスクリプトを作成することができます。
* c#で書かれたソースを修正してメソッドを追加することでノードとして使用できるようにインポートする機能があります（属性の指定とビルドが必要です）。
* dllをインポートしてメソッドをスクリプトで使うことができます。
* クラス指定でメソッドをインポートしてスクリプトで使うことができます。
* NuGetからパッケージをインポートしてスクリプトで使うことができます（NuGet.exe のダウンロードが必要）。
* 作者の趣味とc#をwpfの勉強と気まぐれで制作されています。

## ターゲット環境
* .NET6
* c＃

## 「The data version are incompatible.」と表示される場合の対処方法
Documentフォルダにある古い「CapyCSS」フォルダを削除して下さい。

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

## コンソール出力

コンソールから起動した場合は、「Call File」や「OutConsole」系のノードからの出力は、コンソールにも出力されます。

## ノードのインポート
本ツールでは、c# の多くのメソッドを簡単にノード化することができます。
下記は、属性を使用してメソッドをノード化する例です。
```
[ScriptMethod]
public static ICollection<T> Filtering<T>(IEnumerable<T> samples, Predicate<T> predicate)
{
    if (predicate is null)
    {
        return null;
    }
    var result = new List<T>();
    foreach (var node in samples)
    {
        if (predicate(node))
        {
            result.Add(node);
        }
    }
    return result;
}
```
この他、dll をインポートして機能を取り込むこともできます（こちらを推奨します）。

## 操作方法
* スペースキーもしくはホイールボタンの押下でコマンドウインドウを表示できます。
* コマンドウインドウを表示し、コマンドメニューの「Program」下から項目をクリックしてノードを配置できます。
* ノードの○を左クリックしてドラッグし、ノードの□に接続できます。
* Ctrlキーを押しながら□を左クリックすると、接続しているノードを切断できます（ダブルクリックでも可能になりました）。
* Shiftキーを押しながらマウスをドラッグして範囲内のノードを複数選択します。
* Deleteキーで選択されているノードを削除できます。
* ノード名を左クリックしてノードをドラッグ移動できます（Ctrlキーを押下した状態だと、ガイドに沿って移動します）。
* 画面を左クリックして画面全体をドラッグ移動できます（選択されたノードがある場合、選択されているノードが移動します）。
* ホイールボタンの回転で表示を拡大もしくは縮小できます。
* Ctrl + Aで全選択
* Ctrl + Spaceで選択解除
* Ctrl + Cで選択したノードをコピーできます。
* Ctrl + Vでコピーされているノードを貼り付けられます。
* Ctrl + Sでスクリプトを上書き保存できます。
* Ctrl + Oでスクリプトの読み込みができます。
* Ctrl + Nで新規にスクリプトを作成できます。
* Ctrl + Shift + Nでスクリプトをクリアできます。
* jキーでスクリプトの表示を画面の中央に画面に収まるように調整します。
* Ctrl + jキーでスクリプトの表示位置を画面の左上に調整します。
* F5でEntryをチェックされたノードをすべて実行できます。

※ノード名、引数名はダブルクリックで編集できます。ただし、変数名は画面左側の変数名一覧でのみダブルクリックで編集できます。

## スクリプトの便利な機能
* 引数の上にカーソルを置くと内容を確認できます。
* 数字と文字をドラッグアンドドロップするだけで定数ノードを作成できます。
* ノードの接続時に柔軟なキャスト（接続）が可能です（実行を保証するものではありません）。

## ヒント表示
※只今、調整中です。
<br>英語と日本語をサポートします。ただし、初期状態では英語のみの機械翻訳です。
<br>日本語で表示するには、実行後に作成されるapp.xmlファイルの内容を変更します。
<br>```<Language>en-US</Language>```
<br>上記の内容を次のように変更します。
<br>```<Language>ja-JP</Language>```
<br>再起動すると設定が適用されます。

## メソッドのインポートでサポートされる機能
* クラスのコンストラクタ（new する機能が取り込まれます）
* メソッド(thisを受け取る引数が追加されます)
* 静的メソッド
* ゲッター
* セッター

※メソッドの所属するクラスは、パブリックである必要があります。
<br>※抽象クラスは、対象外です。
<br>※メソッドは、パブリックである必要があります。

## スクリプトが対応するメソッドの引数の型（及び修飾子など）
* Type: int, string, double, byte, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* 配列（ver0.3.5.0 からスクリプト上での扱いにも対応）※ただし、一次元まで。
* IEnumerableを持つ型（※UI上で要素を操作できます）
* Class
* Struct
* Interface
* Delegate
* Enum
* Generics
* 初期化値
* ref（リファレンス）
* outパラメーター修飾子
* inパラメーター修飾子
* paramsパラメータ配列
* null許容型（ver0.3.4.0 からスクリプト上での扱いにも対応）

## スクリプトが対応するメソッドの戻り値の型
* Type: int, string, double, byte, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* 配列
* Class
* Struct
* Interface
* Delegate
* Enum
* Generics
* void
* null許容型

## 独自の型
* text型（UI上で複数行の編集領域を持つstring型です）
* password型（UI上で表示をマスクする為のstring型です）

## メソッドからのメソッド呼び出し
delegate 型の引数は、返し値の型と一致する型のノードと接続できます。ただし、Action だと無条件に接続できます。
デリゲートの呼び出しでは、接続されたノードの結果が返されます。

## スクリプトへのDLL自動取り込み機能
CbSTUtils.AutoImportDllList で Dll を指定するとスクリプト作成時に最初に自動で取り込まれるようになります。
スクリプトに最低限入れたい機能を追加したいときに活用して下さい。

## ライセンス
MITライセンス

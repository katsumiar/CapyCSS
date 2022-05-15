# CapyCSS
![capyCssSample04](https://user-images.githubusercontent.com/63950487/166207451-7cbb4335-be6e-4035-998c-b1521f503e4f.gif)
※このイメージは、ScreenToGif によって作成されています。

## 0.5.0.2 での変更
* UnDo/ReDoに追加しました。
* スクリプトを編集すると表示で分かるようにしました（タイトル等）。
* 変更されたスクリプト破棄時にユーザーに確認するようにしました。
* ツール終了時に保存していないスクリプトの保存確認をするようにしました。

## 0.5.0.1 での変更
* いくつかの不具合に対応しました。

## 0.5.0.0 での変更
![image](https://user-images.githubusercontent.com/63950487/167279679-3c5ab231-33a7-40ee-a086-f897969d1eb4.png)
* プロジェクトファイル（*.cbsproj）に対応しました。

## 0.4.1.2 での変更
* リスト型の内容のヒント表示を行うようにしました。
* デザインを調整しました。
* スクリプトキャンバスにcbsファイルをドロップしたとき、新しいタブを作って読み込むようにしました。
* スクリプトタブに削除用のショートカットを追加しました。
* デザインを調整しました。

## 0.4.1.1 での変更
* 選択されているノードの移動は、選択されているノードをドラッグしたときに行うようにしました。
* 単体ノード同様に複数選択及びグループの移動でグリッド線に沿う移動操作を可能にしました。

## 0.4.1.0 での変更
* デザインをGray系に変更しました。

## 0.4.0.0 での変更
* スクリプトをc#に変換する機能を「Convert C#」ボタン及びコマンドウインドウの「Command」の下に「Convert C#」として追加しました。
* Sequenceノードを廃止し、VoidSequecneとResultSequenceの２つのシーケンスノードを用意しました。
* スクリプトノードのライブラリ周りを見直しました。
* 仮引数の管理をデリゲートが管理するようにしました。
* ルートノードは、強制的にNot Use Cacheをチェックするようにしました。
* コンストラクタノードの場合は、リンク先にself引数が有る場合、強制的にNot Use Cacheをチェックしないようにしました。
* ノードの返し値がVoidの場合は、強制的にNot Use Cacheをチェックするようにしました。
* ノードのリンク先が1つの場合は、強制的にNot Use Cacheをチェックするようにしました。
* ノードのリンク先に1つでもデリゲートがあれば強制的にNot Use Cacheをチェックするようにしました。
* ref, out, in 修飾された引数に接続したノードは、強制的にNot Use Cacheのチェックを外すようにしました。
* Entry Point Nameにタグ名を指定してExecuteボタンを押すとそのタグの中にあるエントリーポイント名が空のエントリーポイントが呼ばれるようにしました。
* Execute ALLボタンを無くしました。
* コンストラクターノードを色替えするようにしました。
* self引数を色替えするようにしました。
* ノード上のプロパティ表現を他と分けました。
* ノード名の変更をクリックからダブルクリックに変更しました。
* ノードの接続解除をダブルクリックでも可能にしました。
* よく使うスクリプト機能のショートカットボタンを追加しました。
* DummyArgumentsノードのデザインを変更しました。
* cbsファイルの読み込み時にフォーマットに対して簡単なチェックをするようにしました。
* ノード接続線開放時の型検索処理を改善しました。
* nullノードを用意しました。このnullは、キャスト不要で代入できます。
* メソッド呼び出し周りを整理しました（該当条件を厳密にチェックするようにしましたが、まだ型制約周りに問題が残っています）。
* スクリプト実行時に最初に変数を初期化するようにしました。
* スクリプト取り込み用のアトリビュートクラスをCapyCSSattributeクラスライブラリに分離しました。
* 基本的なノードライブラリをCapyCSSbaseクラスライブラリに分離しました（UIの絡む処理は、まだ分離していません）。


### 0.5.0.1 で対応した不具合
* コマンドツリーのコマンドの状態を正しく反映するようにしました。
* 型名の変換で場合によって落ちる問題に対応しました。
* 型制約で new() 制約を正しく判定するようにしました。
* 型制約でクラス名/構造体名指定での制約を正しく判定するようにしました。
* 型制約でインターフェイス制約を正しく判定するようにしました。

### 0.4.1.2 で対応した不具合
* リスト変数に要素追加機能が残っていましたが、変数はスクリプト実行時に初期化されるようになったのでこの機能を消しました。
* リスト変数に要素を指定してリストをセットするノードがコンストラクタデザインになっていなかった問題に対応しました。
* ノードから接続線を伸ばしてコマンドリスト表示後に検索文字列を消してコマンドウィンドウを閉じると、同じ操作をしたときにコマンドリストの検索が働かない問題に対応しました。
* スクリプト実行中にタブによるスクリプト切り替えが可能な問題に対応しました。
* スクリプトタブが複数ある時、一番先頭のタブを削除するとタブ切り替えが上手く働かない問題に対応しました。

### 0.4.1.1 で対応した不具合
* 場合によりグリッド線からズレて沿うようになっていた問題に対応しました。

### 0.4.0.1 で対応した不具合
* スクリプト実行後にc#へ変換すると正しくない型で変換される場合がある問題に対応しました。

### 0.4.0.0 で対応した不具合
* staticメソッドの引数にref修飾があった場合、値が正しくリファレンスされていなかった問題に対応しました。
* 公開されているのノードを消した後にExecuteボタンを押すとアサートが出る問題に対応しました。
* メッセージ類が場合により正しく表示されない問題に対応しました。
* ノードの引数の型名が正しく表示されない場合がある問題に対応しました。
* Nullableでnullが正しく処理されない場合がある問題に対応しました。
* ForeachFilesノードのpathにnullを渡したら0を返すようにしました。
* Function関連のジェネリック要素を持つノード作成時のジェネリックパラメータ指定時にFunctionで指定されている制約が働かない問題に対応しました。
* RunもしくはExecuteボタン押下時にアサーションが出る問題に対応しました。
* コマンドウインドウからインポートしたメソッドのリストを表示して、インポートを伴うファイルを読み込むとアサートが発生する問題に対応しました。
* デリゲート周りでnullが使えなかった問題に対応しました。
* nullが入ったリスト型で引数のUIで要素追加操作が行える問題に対応しました。

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
* 配列（ver0.3.5.0 からスクリプト上での扱いにも対応）
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

※リテラルノードのみ独自のtext型を用意しています。string型とobject型へ代入可能です。
<br>※オーバーロードに対応しています。

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

## メソッドからのメソッド呼び出し
delegate 型の引数は、返し値の型と一致する型のノードと接続できます。ただし、Action だと無条件に接続できます。
デリゲートの呼び出しでは、接続されたノードの結果が返されます。

## ライセンス
MITライセンス

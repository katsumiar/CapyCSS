#CapyCSS

##まとめ
* メモを書くときにマウスを使って簡単にプログラムを書くことができます。
* ソースを変更してメソッドを追加することでノードとして使用できるようにインポートする機能があります（ビルドが必要です）。

##目標
* .Net Core 3.1
* c＃

##オプション
保存したスクリプトファイルをコマンド引数で指定すると、起動時に自動的に読み込まれます。オプションに-asが指定されている場合、「public」として指定されたノードは、スクリプトのロード後に実行されます。オプションに-aeを指定した場合、スクリプト実行後に自動的に終了します。
```
CapybaraVS.exe script.cbs
CapybaraVS.exe -as script.cbs
CapybaraVS.exe -as -ae script.cbs
```

##特性
これは、属性を使用して、作成されたメソッドをノード化するサンプルです。
```
[ScriptMethod]
public static System.IO.StreamReader GetReadStream(string fileName, string encoding = "utf-8")
{
    var encodingCode = Encoding.GetEncoding(encoding);
    return new System.IO.StreamReader(fileName, encodingCode);
}
```

##操作方法
※画面左側のメニューから項目をクリックしてノードを配置します。
* 左クリックしてドラッグし、ノードを円と正方形に接続します。
* Ctrlキーを押しながら四角形を左クリックすると、ノードが切断されます。
* Shiftキーを押しながらマウスをドラッグして範囲を選択します。
* Deleteキーで選択したノードを削除します。
* 左クリックでノードを移動します（Ctrlキーを押したままにすると、ガイドに沿って移動します）。
* 画面を左クリックしてドラッグすると、画面全体が移動します（選択した場合、選択したものが移動します）。
* ホイールボタンで表示をスケーリングします。
* Ctrl + Cで選択したノードをコピーします。
* Ctrl + Vで貼り付けます（貼り付け後に選択）。
* Ctrl + Sで保存します。
※ノード名、引数名、変数名をダブルクリックで編集（画面左側の変数名一覧でのみ変更可能）

##スクリプトの便利な機能
※カーソルを動かすだけで、ノードが処理した内容を確認できます。
* 数字と文字をドラッグアンドドロップするだけで、定数ノードを作成できます。
* ノードの接続時に型キャストをサポートします（オブジェクトからのキャストはサポートしていません）。

##ヒント表示
英語と日本語をサポートします。ただし、初期状態では英語のみの機械翻訳です。
日本語で表示するには、実行後に作成されるapp.xmlファイルの内容を変更します。
```<Language>en-US</Language>```
上記の内容を次のように変更します。
```<Language>ja-JP</Language>```
再起動すると設定が適用されます。

##メソッドのインポートでサポートされるメソッド
* Static method
* Class method(thisを受け取る引数が追加されます)

##スクリプトが対応するメソッド引数
* Type: int, string, double, byte, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* Class
* Interface
* Enum
* Generics
* Default arguments
* Reference(Used as a variable reference)
* Overload(Different node types)

##スクリプトが対応するメソッドの戻り型
* Type: int, string, double, byte, sbyte, long, short, ushort, uint, ulong, char, float, decimal, bool, object
* Class
* Interface
* Enum
* Generics
* void

##メソッドからのメソッド呼び出し
「Func <>」および「Action <>」タイプの引数は、ノードを外部プロセスとして呼び出すことができます。 「Func <>」の場合、ノード型と戻り値型が一致した場合に接続できます。

##スクリプト実行フロー
以下の方法でリフレクションで公開するメソッドを見つけてメニューに登録してください。

```ScriptImplement.ImplemantScriptMethods(TreeMenuNode node)```

パブリックメソッドは、AutoImplementFunctionクラスで以下のメソッドによって登録および管理されます。

```bool AutoImplementFunction.ImplAsset(MultiRootConnector col, bool noThreadMode = false, DummyArgumentsControl dummyArgumentsControl = null)```

（イベントを呼び出す機能を持つメソッドは、AutoImplementEventFunctionクラスによって管理されます。）

メニューに登録されているパブリックメソッドをクリックすると、以下の方法でBaseWorkCanvasにコマンドが登録されます。

```TreeMenuNodeCommand CommandCanvas.CreateEventCanvasCommand(Func<object> action, TreeMenuNode vm = null)```

コマンド登録されたコマンドは、キャンバス（BaseWorkCanvas）をクリックすると以下の方法で実行されます。

```void BaseWorkCanvas.ProcessCommand(Point setPos)```

キャンバスに配置されるノードは、コマンドの実行時に作成されるMultiRootConnectorによって作成されます。
MultiRootConnectorの内容は、以下のメソッドによって形成されます。

```void MultiRootConnector.MakeFunctionType()```

ここで呼び出されたAutoImplementFunction.ImplAssetは、以下のメソッドでMultiRootConnectorのFunctionにpublicメソッド呼び出しプロセスを登録します。

```
void MultiRootConnector.MakeFunction(
            string name,
            string hint,
            Func<ICbVSValue> retType,
            List<ICbVSValue> argumentList,
            Func<List<ICbVSValue>, CbPushList, ICbVSValue> func
            )
```

UI上のノードの戻り値は、RootConnectorによって管理されます。
このRootConnectorには、パブリックメソッドを呼び出す機能もあります。
UIの引数はLinkConnectorで管理され、List関数の引数はLinkConnectorのLinkConnectorListで管理されます。

スクリプトが実行されると、RootConnectorは自身に接続されているノードからRootConnectorの情報（戻り値）を引き出します。
取得した引数からpublicメソッドを呼び出し、自身の戻り値を計算します。
このようにして、RootConnectorからLinkConnectorを取得し、最終結果を計算します。
また、基本的には、結果が得られたら、RootConnectorからの結果のみを参照することで処理が高速化されます。
ただし、強制がチェックされるノードとイベント呼び出し（Func <>）は毎回計算されます。
毎回計算する必要があるノード（状態を持つノード）については、必要に応じて[強制]をオンにする必要があります。

##ライセンス
MITライセンス

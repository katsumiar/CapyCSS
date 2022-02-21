namespace CapyCSSattribute
{
    /// <summary>
    /// CapyCSS用アセンブリ取り込み用アトリビュートインターフェイスです。
    /// </summary>
    public interface IScriptArribute
    {
        /// <summary>
        /// メニュー用のパスです。
        /// </summary>
        string Path { get; }
        /// <summary>
        /// メソッド名です。
        /// </summary>
        string MethodName { get; }
        /// <summary>
        /// trueならノードに旧仕様であることを表示します。
        /// </summary>
        bool OldSpecification { get; }
        /// <summary>
        /// tureなら取り込みを拒否します。
        /// </summary>
        bool DefaultHide { get; }
        /// <summary>
        /// trueなら実行可ノードになります。
        /// </summary>
        bool IsRunable { get; }
    }

    /// <summary>
    /// CapyCSSで取り込む為のクラス用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ScriptClassAttribute : Attribute, IScriptArribute
    {
        private string path;            // メニュー用のパス
        private bool oldSpecification;  // 古い仕様のメソッド（ユーザーに廃止を促すのに使用します）
        private bool defaultHide;       // 非表示指定
        /// <inheritdoc/>
        public string Path => path;
        /// <inheritdoc/>
        public string MethodName => null;
        /// <inheritdoc/>
        public bool OldSpecification => oldSpecification;
        /// <inheritdoc/>
        public bool DefaultHide => defaultHide;
        /// <inheritdoc/>
        public bool IsRunable => false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">メニュー用のパス</param>
        /// <param name="defaultHide">取り込みを拒否するか？</param>
        /// <param name="oldSpecification">古い仕様のメソッドか？</param>
        public ScriptClassAttribute(string path = "", bool defaultHide = true, bool oldSpecification = false)
        {
            if (path != "" && !path.EndsWith("."))
            {
                path += ".";
            }
            this.path = path;
            this.oldSpecification = oldSpecification;
            this.defaultHide = defaultHide;
        }
    }

    /// <summary>
    /// CapyCSSで取り込む為のメソッド用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ScriptMethodAttribute : Attribute, IScriptArribute
    {
        private string path;            // メニュー用のパス
        private string methodName;      // メソッド名
        private bool oldSpecification;  // 古い仕様のメソッド（ユーザーに廃止を促すのに使用します）
        private bool isRunable;         // 任意実行可能ノード指定
        /// <inheritdoc/>
        public string Path => path;
        /// <inheritdoc/>
        public string MethodName => methodName;
        /// <inheritdoc/>
        public bool OldSpecification => oldSpecification;
        /// <inheritdoc/>
        public bool DefaultHide => false;
        /// <inheritdoc/>
        public bool IsRunable => isRunable;
        /// <summary>
        /// CapyCSSで取り込む為のメソッド用属性です。
        /// </summary>
        /// <param name="path">メニュー用のパス</param>
        /// <param name="methodName">メソッド名</param>
        /// <param name="oldSpecification">古い仕様のメソッドか</param>
        /// <param name="isRunable">任意実行可能ノード指定</param>
        public ScriptMethodAttribute(string path = "", string methodName = null, bool oldSpecification = false, bool isRunable = false)
        {
            if (path != "" && !path.EndsWith("."))
            {
                path += ".";
            }
            this.path = path;
            this.methodName = methodName;
            this.oldSpecification = oldSpecification;
            this.isRunable = isRunable;
        }
    }

    /// <summary>
    /// CapyCSSで取り込む為の引数用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ScriptParamAttribute : Attribute
    {
        private string name;    // 引数名
        /// <summary>
        /// 引数名を参照します。
        /// </summary>
        public string ParamName => name;
        /// <summary>
        /// CapyCSSで取り込む為の引数用属性です。
        /// </summary>
        /// <param name="name">引数名</param>
        public ScriptParamAttribute(string name)
        {
            this.name = name;
        }
    }
}
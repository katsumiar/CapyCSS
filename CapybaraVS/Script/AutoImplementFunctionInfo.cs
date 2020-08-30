using System;
using System.Collections.Generic;
using static CapybaraVS.Script.ScriptImplement;

namespace CapybaraVS.Script
{
    /// <summary>
    /// アセット情報
    /// </summary>
    public class AutoImplementFunctionInfo
    {
        /// <summary>
        /// アセット識別子
        /// </summary>
        public string assetCode;
        /// <summary>
        /// メニュー用ヒントメッセージ
        /// </summary>
        public string hint;
        /// <summary>
        /// ノード用ヒントメッセージ
        /// </summary>
        public string nodeHint;
        /// <summary>
        /// メニュー用アセット名
        /// </summary>
        public string menuTitle;
        /// <summary>
        /// ノード用アセット名
        /// </summary>
        public string funcTitle;
        /// <summary>
        /// メソッドの所属クラスの型
        /// </summary>
        public Type classType;
        /// <summary>
        /// 返り値型の定義
        /// </summary>
        public Func<ICbValue> returnType;
        /// <summary>
        /// 引数型の定義リスト
        /// </summary>
        public List<ArgumentInfoNode> argumentTypeList;
    }
}

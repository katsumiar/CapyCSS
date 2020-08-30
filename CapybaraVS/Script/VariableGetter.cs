using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using System;

namespace CapybaraVS.Script
{
    /// <summary>
    /// 変数参照仲介クラス
    /// </summary>
    public class VariableGetter
    {
        int id = 0;
        string valueName = "!ERROR!";

        /// <summary>
        /// 変数の登録ID
        /// </summary>
        public int Id => id;
        bool is_error = true;
        
        /// <summary>
        /// 引数の参照に失敗したか？
        /// </summary>
        public bool IsError => is_error;
        MultiRootConnector owner = null;

        /// <summary>
        /// 登録された適切な表示用の名前を返す
        /// </summary>
        public string MakeName => owner.GetVariableName(valueName);
        public VariableGetter(MultiRootConnector owner, Func<string, string> func = null, int index = 0)
        {
            this.owner = owner;
            try
            {
                id = (int)(this.owner.GetAttachVariableId(index));
                valueName = CommandCanvas.ScriptWorkStack.Find(id).Name;

                // 名前生成処理を登録
                if (func != null)
                    owner.GetVariableName = func;

                // 変数アセットは値の編集を禁止
                this.owner.LinkConnectorControl.CaptionReadOnly = true;

                // 参照関係を登録する
                CommandCanvas.ScriptWorkStack.Link(id, this.owner);

                is_error = false;
            }
            catch (Exception ex)
            {
                this.owner.ExceptionFunc(ex);
            }
        }
    }
}

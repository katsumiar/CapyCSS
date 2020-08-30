using System;
using System.Collections.Generic;
using CbVS.Script;

namespace CapybaraVS.Script
{
    /// <summary>
    /// 仮引数のスタックを管理するクラスです。
    /// </summary>
    public class DummyArgumentsStack
    {
        public class Node
        {
            public bool IsReturn = true;
            public Func<bool> IsInvalid = () => false;
            public Func<CbFuncArguments.INDEX, ICbValue> CbVSValue = null;
            public Node(Func<bool> IsInvalid, CbFuncArguments funcArguments)
            {
                this.IsInvalid = IsInvalid;
                this.CbVSValue = (idx) => funcArguments[idx];
            }
        }
        private List<Node> cbVSValues = new List<Node>();
        public void Push(Node value)
        {
            cbVSValues.Add(value);
        }
        public bool IsInvalid()
        {
            if (cbVSValues.Count != 0)
                return !cbVSValues[cbVSValues.Count - 1].IsReturn;
            return false;
        }
        public bool IsEmpty()
        {
            return cbVSValues.Count == 0;
        }
        public bool IsGetValue()
        {
            if (IsEmpty())
                return true;    // そもそも登録がされていないなら登録されたものとする
            if (cbVSValues.Count == 0)
                return false;
            return !cbVSValues[cbVSValues.Count - 1].IsInvalid();
        }
        public ICbValue GetValue(CbFuncArguments.INDEX index = CbFuncArguments.INDEX.ARG_1)
        {
            if (IsEmpty())
                return null;    // そもそも登録されていなかったら値が無いので null を返す
            return cbVSValues[cbVSValues.Count - 1].CbVSValue(index);
        }
        public void InvalidReturn()
        {
            if (cbVSValues.Count != 0)
                cbVSValues[cbVSValues.Count - 1].IsReturn = false;
        }
        public void Pop()
        {
            if (cbVSValues.Count != 0)
                cbVSValues.RemoveAt(cbVSValues.Count - 1);
        }
    }
}

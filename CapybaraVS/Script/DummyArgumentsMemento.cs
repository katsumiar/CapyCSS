using System;
using System.Collections.Generic;
using System.Diagnostics;
using CbVS.Script;

namespace CapyCSS.Script
{
    /// <summary>
    /// 仮引数のスタックを管理するクラスです。
    /// </summary>
    public class DummyArgumentsMemento
    {
        public class _DummyArguments
        {
            public class Node
            {
                public CbFuncArguments funcArguments;
                public Node(CbFuncArguments funcArguments)
                {
                    this.funcArguments = funcArguments;
                }
            }
            private static ulong lastId = 0;
            private static IDictionary<ulong, Node> cbVSValues = new Dictionary<ulong, Node>();
            public static ulong CreateAdd(CbFuncArguments funcArguments)
            {
                cbVSValues.Add(++lastId, new Node(funcArguments));
                return lastId;
            }
            public static bool ExistId(ulong id)
            {
                return cbVSValues.ContainsKey(id);
            }
            public static ICbValue GetValue(ulong id, CbFuncArguments.INDEX index)
            {
                if (!ExistId(id))
                    return null;    // そもそも登録されていなかったら値が無いので null を返す
                return cbVSValues[id].funcArguments[index];
            }
            public static void Remove(ulong id)
            {
                Debug.Assert(ExistId(id));
                cbVSValues.Remove(id);
            }
        }

        private ulong id = 0;

        public DummyArgumentsMemento()
        {
        }

        public void Regist(params object[] argument)
        {
            CbFuncArguments argumentRef = new CbFuncArguments();
            for (int i = 0; i < argument.Length; ++i)
            {
                argumentRef[(CbFuncArguments.INDEX)i] ??= CbObject.Create();
                argumentRef[(CbFuncArguments.INDEX)i].Data = argument[i];
            }
            id = _DummyArguments.CreateAdd(argumentRef);
        }

        public bool IsInvalid()
        {
            return !_DummyArguments.ExistId(id);
        }

        public bool CanValue(CbFuncArguments.INDEX index)
        {
            if (IsInvalid())
                return true;
            return _DummyArguments.GetValue(id, index) != null;
        }

        public ICbValue GetValue(CbFuncArguments.INDEX index = CbFuncArguments.INDEX.ARG_1)
        {
            if (IsInvalid())
                return null;
            return _DummyArguments.GetValue(id, index);
        }

        public void Unregist()
        {
            if (id == 0)
            {
                // 登録されていない
                return;
            }
            _DummyArguments.Remove(id);
        }
    }
}

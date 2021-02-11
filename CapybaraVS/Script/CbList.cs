using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace CbVS.Script
{
    public interface ICbList : ICbValue
    {
        /// <summary>
        /// List<適切な型> に変換します。
        /// </summary>
        /// <returns>List<適切な型>のインスタンス</returns>
        object ConvertOriginalTypeList(MultiRootConnector col, DummyArgumentsStack cagt);

        /// <summary>
        /// List<適切な型> に変換します。
        /// </summary>
        /// <param name="dummyArgumentsControl"></param>
        /// <param name="cagt"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        object ConvertOriginalTypeList(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt, MultiRootConnector col = null);

        ObservableCollection<LinkConnector> LinkConnectors { get; }

        /// <summary>
        /// 配列型か？
        /// </summary>
        bool IsArrayType { get; set; }

        List<ICbValue> Value { get; set; }

        string ItemName { get; }

        public Func<ICbValue> CreateTF { get; }

        ICbValue this[int index] { get; set; }

        void RemoveAt(int index);

        /// <summary>
        /// リストの要素数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// リストのコピー
        /// </summary>
        /// <param name="toList"></param>
        void CopyTo(ICbList toList);

        /// <summary>
        /// リストに要素を追加します。
        /// </summary>
        /// <param name="cbVSValue"></param>
        void Append(ICbValue cbVSValue);

        /// <summary>
        /// リストの持つ変数の持つ値に cbVSValue が持つ値が含まれるかを返します。
        /// </summary>
        /// <param name="cbVSValue">調べる変数</param>
        /// <returns>true なら含まれる</returns>
        bool Contains(ICbValue cbVSValue);

        /// <summary>
        /// List<> のインスタンスから内容をコピーします。
        /// </summary>
        /// <param name="list"></param>
        void CopyFrom(object list);

        /// <summary>
        /// リストをクリアします。
        /// </summary>
        void Clear();
    }

    public class CbList
    {
        /// <summary>
        /// CbXXX型の List<> 型を作成します。
        /// </summary>
        /// <param name="original">オリジナルのList<T>のTの型</param>
        /// <returns>型</returns>
        public static Type GetCbType(Type original)
        {
            if (original is null)
            {
                return null;
            }
            return typeof(CbList<>).MakeGenericType(original);
        }

        /// <summary>
        /// CbList<T>に要素を追加します。
        /// </summary>
        /// <param name="instance">CbList<T>のインスタンス</param>
        /// <param name="originalNode">T の型</param>
        /// <param name="data">追加する要素</param>
        public static void Append(ICbValue instance, Type originalNode, ICbValue data)
        {
            var listType = CbList.GetCbType(originalNode);
            MethodInfo addMethod = listType.GetMethod("Append");
            addMethod.Invoke(instance, new Object[] { data });
        }

        /// <summary>
        /// CbList<T>に要素を追加します。
        /// </summary>
        /// <param name="instance">CbList<T>のインスタンス</param>
        /// <param name="originalNode">T の型</param>
        public static void Append(ICbValue instance, Type originalNode)
        {
            Append(instance, originalNode, CbST.CbCreate(originalNode));
        }

        /// <summary>
        /// CbXXX型の List<> 変数を作成します。
        /// </summary>
        /// <param name="original">オリジナルのList<T>のTの型</param>
        /// <param name="name">変数名</param>
        /// <returns>CbList<original>型の変数</returns>
        public static ICbValue Create(Type original, string name = "")
        {
            object result = CbList.GetCbType(original).InvokeMember(
                        "GetCbFunc",
                        BindingFlags.InvokeMethod,
                        null,
                        null,
                        new object[] { name }
                        );
            return result as ICbValue;
        }

        /// <summary>
        /// CbXXX型の List<> 変数の型を作成します。
        /// </summary>
        /// <param name="original">オリジナルのList<T>のTの型</param>
        /// <returns>CbList<original>型の型</returns>
        public static Func<ICbValue> CreateTF(Type original)
        {
            return () => CbList.Create(original);
        }

        /// <summary>
        /// CbXXX型の List<> 変数の型を作成します。
        /// </summary>
        /// <param name="original">オリジナルのList<T>のTの型</param>
        /// <returns>CbList<original>型の型</returns>
        public static Func<string, ICbValue> CreateNTF(Type original)
        {
            return (name) => CbList.Create(original, name);
        }

        public static LinkConnector ConvertLinkConnector(CommandCanvas ownerCommandCanvas, ICbValue cbVSValue)
        {
            var linkConnector = new LinkConnector()
            {
                OwnerCommandCanvas = ownerCommandCanvas,
                ValueData = cbVSValue
            };
            return linkConnector;
        }

        public static StackNode ConvertStackNode(CommandCanvas ownerCommandCanvas, ICbValue cbVSValue)
        {
            var stackNode = new StackNode(ownerCommandCanvas)
            {
                ValueData = cbVSValue
            };
            return stackNode;
        }
    }

    /// <summary>
    /// List<>型
    /// </summary>
    /// <typeparam name="T">オリジナルの型</typeparam>
    public class CbList<T> : BaseCbValueClass<List<ICbValue>>, ICbValueListClass<List<ICbValue>>, ICbShowValue, ICbList
    {
        private bool nullFlg = true;

        public override Type MyType => typeof(CbList<T>);

        public override CbST CbType
        {
            get
            {
                return new CbST(
                    CapybaraVS.Script.CbType.Func,
                    OriginalType.FullName   // 型名を持っていないとスクリプト読み込み時に再現できない
                    );
            }
        }

        public CbList()
        {
            _ListNodeType = CbST.CbCreateTF(typeof(T));
        }

        public CbList(List<ICbValue> n, string name = "")
        {
            Value = n;
            _ListNodeType = CbST.CbCreateTF(typeof(T));
            Name = name;
        }

        public string DataString
        {
            get
            {
#if !SHOW_LINK_ARRAY
                string text = $"{CbSTUtils.CbTypeNameList[nameof(CbList)]} {Value.Count}-{ItemName}" + Environment.NewLine;
                foreach (var node in Value)
                {
                    text += node.ValueString + Environment.NewLine;
                }
#else
                string text = TypeName + Environment.NewLine;
#endif
                return text;
            }
        }

        public bool IsArrayType { get; set; } = false;

        public override Type OriginalReturnType => typeof(T);

        public override Type OriginalType
        {
            get
            {
                if (IsArrayType)
                {
                    // 配列として振る舞う

                    return CbST.GetTypeEx(OriginalReturnType.FullName + "[]");
                }
                return typeof(List<>).MakeGenericType(typeof(T));
            }
        }

        /// <summary>
        /// ノードの型名
        /// </summary>
        public string ItemName => CbSTUtils._GetTypeName(typeof(T));

        private Func<ICbValue> _ListNodeType = null;

        public override Func<ICbValue> NodeTF => _ListNodeType;

        public override string TypeName
        {
            get
            {
                if (IsArrayType)
                {
                    // 配列として振る舞う

                    if (NodeTF is null)
                    {
                        return $"[]";
                    }
                    else
                    {
                        return $"{ItemName}[]";
                    }
                }
                if (NodeTF is null)
                {
                    return $"{CbSTUtils.CbTypeNameList[nameof(CbList)]}<>";
                }
                else
                {
                    return $"{CbSTUtils.CbTypeNameList[nameof(CbList)]}<{ItemName}>";
                }
            }
        }

        public override List<ICbValue> Value
        {
            get => _value;
            set
            {
                if (value is null)
                {
                    nullFlg = true;
                    value = null;
                }
                else
                {
                    nullFlg = false;
                }
                _value = value;
            }
        }

        public override string ValueString
        {
            get
            {
                string baseName = "[" + TypeName + "()]";
                if (IsError)
                    return ERROR_STR;
                if (nullFlg)
                    return baseName + NULL_STR;
                return baseName;
            }
            set => new NotImplementedException();
        }

        public override bool IsDelegate => NodeTF() is ICbEvent;

        public override bool IsReadOnlyValue { get; set; } = true;

        /// <summary>
        /// リストの管理する要素数を参照します。
        /// </summary>
        public int Count => Value.Count;

        public ICbValue this[int index]
        {
            get => Value[index];
            set { Value[index] = value; }
        }

        /// <summary>
        /// リストから指定位置の要素を取り除きます。
        /// </summary>
        /// <param name="index">要素を取り除く位置</param>
        public void RemoveAt(int index)
        {
            Value.RemoveAt(index);
        }

        /// <summary>
        /// リストの内容をtoListに比較的高速にコピーします。
        /// </summary>
        /// <param name="toList">コピー先のリスト</param>
        public void CopyTo(ICbList toList)
        {
            if (toList.Count > 0 && Count > 0 && (toList as CbList<T>) != null)
            {
                // 差分のコピー

                int len = Math.Min(toList.Count, Value.Count);
                int i = 0;
                for (; i < len; ++i)
                {
                    toList[i].CopyValue(Value[i]);
                }
                int remaining = toList.Count - Value.Count;
                if (remaining != 0)
                {
                    if (remaining > 0)
                    {
                        // 多すぎる配列を消す

                        while (remaining-- > 0)
                        {
                            toList.RemoveAt(i);
                        }
                    }
                    else
                    {
                        // 足りない配列を足す

                        remaining = Math.Abs(remaining);
                        for (int j = 0; j < remaining; ++j)
                        {
                            var addNode = NodeTF();
                            addNode.CopyValue(Value[i + j]);
                            toList.Append(addNode);
                        }
                    }
                }
            }
            else
            {
                (toList as CbList<T>).Value = new List<ICbValue>(Value);
            }
        }

        /// <summary>
        /// リストに要素を追加します。
        /// </summary>
        /// <param name="cbVSValue">追加要素</param>
        public void Append(ICbValue cbVSValue)  // CbList.Append にリフレクションとして参照されている
        {
            var addData = NodeTF();
            addData.CopyValue(cbVSValue);
            Value.Add(addData);
        }

        public ObservableCollection<LinkConnector> LinkConnectors
        {
            get
            {
                ObservableCollection<LinkConnector> ret = new ObservableCollection<LinkConnector>();
                foreach (var node in Value)
                {
                    ret.Add(CbList.ConvertLinkConnector(null, node));
                }
                return ret;
            }
        }

        /// <summary>
        /// リストの持つ変数の持つ値に cbVSValue が持つ値が含まれるかを返します。
        /// </summary>
        /// <param name="cbVSValue">調べる変数</param>
        /// <returns>true なら含まれる</returns>
        public bool Contains(ICbValue cbVSValue)
        {
            foreach (var node in Value)
            {
                if (node.Equal(cbVSValue))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// List<適切な型> に変換します。
        /// </summary>
        /// <returns>List<適切な型>のインスタンス</returns>
        public object ConvertOriginalTypeList(MultiRootConnector col, DummyArgumentsStack cagt)
        {
            return ConvertOriginalTypeList(null, cagt, col);
        }

        /// <summary>
        /// List<適切な型> に変換します。
        /// </summary>
        /// <param name="dummyArgumentsControl"></param>
        /// <param name="cagt"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public object ConvertOriginalTypeList(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt, MultiRootConnector col = null)
        {
            var listNodeType = NodeTF();

            var genericType = typeof(List<>).MakeGenericType(typeof(T));
            List<T> originalCopyList = (List<T>)Activator.CreateInstance(genericType);

            if (listNodeType is ICbEvent)
            {
                // CbFunc のノードを持つ CbList を Func<> 及び Action<> 用の List に変換する 
                Debug.Assert(listNodeType.CbType.LiteralType == CapybaraVS.Script.CbType.Func);

                // 仮引数用の型を作成
                // Func<> 及び Action<> 用の変換

                // 仮引数コントロールを作成
                dummyArgumentsControl ??= new DummyArgumentsControl(col);

                foreach (var node in Value)
                {
                    ICbEvent cbEvent2 = node as ICbEvent;
                    if (cbEvent2.CallBack is null)
                        ((dynamic)originalCopyList).Add(null);
                    else
                        originalCopyList.Add((T)cbEvent2.GetCallBackOriginalType(dummyArgumentsControl, cagt));
                }
            }
            else
            {
                // CbList を List に変換する
                foreach (var node in Value)
                {
                    if (node is CbObject cbObject)
                    {
                        ICbValue cbValue = cbObject.Data as ICbValue;
                        originalCopyList.Add((T)cbValue.Data);
                    }
                    else
                    {
                        originalCopyList.Add((T)node.Data);
                    }
                }
            }
            if (IsArrayType)
            {
                // 配列に変換する

                return originalCopyList.ToArray();
            }
            return originalCopyList;
        }

        /// <summary>
        /// List<> のインスタンスから内容をコピーします。
        /// </summary>
        /// <param name="list"></param>
        public void CopyFrom(object list)
        {
            Clear();

            if (IsArrayType)
            {
                // 配列から内容をコピーします。

                foreach (var nd in (Array)list)
                {
                    ICbValue val = NodeTF();
                    val.Data = nd; 
                    Append(val);
                }
                return;
            }

            foreach (var nd in (List<T>)list)
            {
                ICbValue val = NodeTF();
                val.Data = nd;
                Append(val);
            }
        }

        /// <summary>
        /// リストを空にします。
        /// </summary>
        public void Clear()
        {
            Value?.Clear();
        }

        public static CbList<T> Create(string name = "")
        {
            return new CbList<T>(new List<ICbValue>(), name);
        }

        public static CbList<T> Create(List<ICbValue> n, string name = "")
        {
            return new CbList<T>(n, name);
        }

        public Func<ICbValue> CreateTF => () => Create();
        public Func<ICbValue> CreateNTF(string name) => () => Create(name);

        public static Func<ICbValue> TF => () => CbList<T>.Create();
        public static Func<string, ICbValue> NTF => (name) => CbList<T>.Create(name);

        public static ICbValue GetCbFunc(string name) => Create(name);    // リフレクションで参照されている。
    }
}

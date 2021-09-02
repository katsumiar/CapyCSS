using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script;
using System;
using System.Collections;
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

        /// <summary>
        /// 配列型か？
        /// </summary>
        bool IsArrayType { get; set; }

        /// <summary>
        /// 実際に取り込んだ型の情報
        /// </summary>
        Type SourceType { get; set; }

        /// <summary>
        /// リストの要素を追加できるか？
        /// </summary>
        bool HaveAdd { get; }

        bool AddLock { get; set; }

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
        /// リストに要素を追加します。
        /// </summary>
        /// <param name="cbVSValue"></param>
        ICbValue Append(ICbValue cbVSValue);

        /// <summary>
        /// リストの持つ変数の持つ値に cbVSValue が持つ値が含まれるかを返します。
        /// </summary>
        /// <param name="cbVSValue">調べる変数</param>
        /// <returns>true なら含まれる</returns>
        bool Contains(ICbValue cbVSValue);

        /// <summary>
        /// ICollection<> のインスタンスから内容をコピーします。
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
        /// CbXXX型の ICollection<> 型を作成します。
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
        /// CbXXX型の ICollection<> 変数を作成します。
        /// </summary>
        /// <param name="original">オリジナルの型</param>
        /// <param name="name">変数名</param>
        /// <returns>CbList<オリジナルの要素の型>型の変数</returns>
        public static ICbValue Create(Type original, string name = "")
        {
            ICbList result = CbList.GetCbType(original.GenericTypeArguments[0]).InvokeMember(
                        nameof(CbList<int>.GetCbFunc),
                        BindingFlags.InvokeMethod,
                        null,
                        null,
                        new object[] { name }
                        ) as ICbList;
            result.SourceType = original;
            return result;
        }

        public static StackNode ConvertStackNode(CommandCanvas ownerCommandCanvas, ICbValue cbVSValue)
        {
            var stackNode = new StackNode(ownerCommandCanvas)
            {
                ValueData = cbVSValue
            };
            return stackNode;
        }

        /// <summary>
        /// 引数付きのインターフェイスを持っているか判定します。
        /// </summary>
        /// <param name="type">判定する型</param>
        /// <param name="interfaceType">インターフェイスの型</param>
        /// <returns>インターフェイスを持っているならtrue</returns>
        public static bool HaveInterface(Type type, Type interfaceType)
        {
            if (type.GenericTypeArguments.Length == 0)
                return false;

            Type arg = type.GenericTypeArguments[0];
            Type openedType = interfaceType;
            Type collectionType = openedType.MakeGenericType(arg);

            return collectionType.IsAssignableFrom(type) ||
               type.GetGenericTypeDefinition() == interfaceType;
        }
    }

    /// <summary>
    /// ICollection<>型
    /// </summary>
    /// <typeparam name="T">オリジナルのList<T>のTの型</typeparam>
    public class CbList<T> : BaseCbValueClass<List<ICbValue>>, ICbValueListClass<List<ICbValue>>, ICbShowValue, ICbList
    {
        private bool nullFlg = true;

        public override Type MyType => typeof(CbList<T>);

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
                    text += node.ValueUIString + Environment.NewLine;
                }
#else
                string text = TypeName + Environment.NewLine;
#endif
                return text;
            }
        }

        public override bool IsList => true;
        public override ICbList GetListValue => this;

        public bool IsArrayType { get; set; } = false;

        public Type SourceType { get; set; }

        /// <summary>
        /// UI上で Add 可能か？
        /// </summary>
        public bool HaveAdd
        {
            get
            {
                if (AddLock)
                {
                    return false;
                }
                return CbList.HaveInterface(SourceType, typeof(IEnumerable<>));
            }
        }

        public bool AddLock { get; set; } = false;

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
                else if (SourceType != null)
                {
                    return SourceType;
                }
                return typeof(ICollection<>).MakeGenericType(typeof(T));
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
                string typeName;
                if (SourceType != null)
                {
                    return CbSTUtils._GetTypeName(SourceType);
                }
                else
                {
                    typeName = CbSTUtils.CbTypeNameList[nameof(CbList)];
                }
                if (NodeTF is null)
                {
                    return $"{typeName}<>";
                }
                else
                {
                    return $"{typeName}<{ItemName}>";
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

        /// <summary>
        /// 値のUI上の文字列表現
        /// </summary>
        public override string ValueUIString
        {
            get
            {
                string baseName = "[" + TypeName + "()]";
                if (IsError)
                    return CbSTUtils.ERROR_STR;
                if (IsNull)
                    return baseName + CbSTUtils.UI_NULL_STR;
                return baseName;
            }
        }

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        public override string ValueString
        {
            get
            {
                if (IsNull)
                {
                    return CbSTUtils.NULL_STR;
                }
                else
                {
                    string result = "";
                    for (int i = 0; i < Count; ++i)
                    {
                        result += Value[i].ValueString + Environment.NewLine;
                    }
                    return result;
                }
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
        /// リストに要素を追加します。
        /// </summary>
        /// <param name="cbVSValue">追加要素</param>
        public ICbValue Append(ICbValue cbVSValue)
        {
            var addData = NodeTF();
            addData.Set(cbVSValue);
            Value.Add(addData);
            return addData;
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
            ICollection<T> originalCopyList = (ICollection<T>)Activator.CreateInstance(genericType);

            if (listNodeType is ICbEvent)
            {
                // CbFunc のノードを持つ CbList を Func<> 及び Action<> 用の List に変換する 

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
                T[] ts = new T[originalCopyList.Count];
                int index = 0;
                foreach (T node in originalCopyList)
                {
                    ts[index++] = node;
                }
                return ts;// originalCopyList.ToArray();
            }
            return originalCopyList;
        }

        /// <summary>
        /// 内容をコピーします。
        /// </summary>
        /// <param name="list"></param>
        public void CopyFrom(object list)
        {
            Clear();

            if (list is ICbValue cbValue && cbValue.IsList)
            {
                ICbList cbList = cbValue.GetListValue;
                if (cbList.Count == 0)
                    return;

                if (cbList[0] is ICbClass)
                {
                    // 要素は、参照渡し

                    for (int i = 0; i < cbList.Count; ++i)
                    {
                        Value.Add(cbList[i]);   // ※Append だとコピーになる
                    }
                    return;
                }
                // 要素をコピー
                foreach (var node in (IEnumerable<ICbValue>)cbList.Data)
                {
                    Append(node);
                }
                return;
            }

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

            foreach (var nd in (IEnumerable<T>)list)
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

        public override bool IsNull => nullFlg;

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

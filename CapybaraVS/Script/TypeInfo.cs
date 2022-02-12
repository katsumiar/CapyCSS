//#define SHOW_LINK_ARRAY   // リスト型を接続したときにリストの要素をコピーして表示する

using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Windows;
using System.Reflection;
using static CapyCSS.Controls.MultiRootConnector;
using static CapyCSS.Script.ScriptImplement;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Media;
using CbVS.Script;
using CapyCSS.Script;
using System.Collections;
using System.Threading;

namespace CapyCSS.Script
{
    /// <summary>
    /// 型を管理する
    /// ※管理している型の復元及び複製機能を持っている
    /// </summary>
    public class CbST
    {
        private static List<Assembly> recentlyReferencedAssemblyList = new List<Assembly>();
        private static ReaderWriterLockSlim recentlyReferencedAssemblyListRWlock = new ReaderWriterLockSlim();

        private static List<Tuple<string, Type>> cacheTypeList = new List<Tuple<string, Type>>();
        private static ReaderWriterLockSlim cacheTypeListRWlock = new ReaderWriterLockSlim();

        private const int MAX_recentlyReferencedAssembly = 6;
        private const int MAX_cacheType = 300;

        /// <summary>
        /// 型情報を参照します。
        /// ※インポートしたモジュールの型情報も参照できます。
        /// </summary>
        /// <param name="name">型名</param>
        /// <returns>型情報</returns>
        public static Type GetTypeEx(string name)
        {
            Type _GetTypeEx(string name)
            {
                var type = Type.GetType(name);
                if (type is null)
                {
                    try
                    {
                        recentlyReferencedAssemblyListRWlock.EnterReadLock();
                        foreach (var assembly in recentlyReferencedAssemblyList)
                        {
                            type = Array.Find(assembly.GetTypes(), n => n.FullName == name);
                            if (type != null)
                            {
                                return type;
                            }
                        }
                    }
                    finally
                    {
                        if (recentlyReferencedAssemblyListRWlock.IsReadLockHeld)
                        {
                            recentlyReferencedAssemblyListRWlock.ExitReadLock();
                        }
                    }

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = Array.Find(assembly.GetTypes(), n => n.FullName == name);
                        if (type != null)
                        {
                            setMostRecentlyReferencedAssemblyList(assembly);
                            return type;
                        }
                    }
                }
                return type;
            }

            try
            {
                cacheTypeListRWlock.EnterReadLock();
                string findName = new string(name.Reverse().ToArray());
                var cache = cacheTypeList.Find(n => n.Item1 == findName);
                if (cache != null)
                {
                    return cache.Item2;
                }
            }
            finally
            {
                if (cacheTypeListRWlock.IsReadLockHeld)
                {
                    cacheTypeListRWlock.ExitReadLock();
                }
            }
            try
            {
                cacheTypeListRWlock.EnterWriteLock();
                var type = _GetTypeEx(name);
                if (cacheTypeList.Count > MAX_cacheType)
                {
                    cacheTypeList.RemoveAt(MAX_cacheType);
                }
                cacheTypeList.Insert(0, new Tuple<string, Type>(new string(name.Reverse().ToArray()), type));
                return type;
            }
            finally
            {
                if (cacheTypeListRWlock.IsWriteLockHeld)
                {
                    cacheTypeListRWlock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// 型情報参照のためのアセンブリをキャッシュします。
        /// </summary>
        /// <param name="name">アセンブリ</param>

        private static void setMostRecentlyReferencedAssemblyList(Assembly assembly)
        {
            try
            {
                recentlyReferencedAssemblyListRWlock.EnterWriteLock();
                if (!recentlyReferencedAssemblyList.Contains(assembly))
                {
                    if (recentlyReferencedAssemblyList.Count > MAX_recentlyReferencedAssembly)
                    {
                        recentlyReferencedAssemblyList.RemoveAt(MAX_recentlyReferencedAssembly);
                    }
                    recentlyReferencedAssemblyList.Insert(0, assembly);
                }
            }
            finally
            {
                if (recentlyReferencedAssemblyListRWlock.IsWriteLockHeld)
                {
                    recentlyReferencedAssemblyListRWlock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// 型情報を参照します。
        /// ※インポートしたモジュールの型情報も参照できます。
        /// </summary>
        /// <param name="type">型情報</param>
        /// <returns>型情報</returns>
        public static Type GetTypeEx(Type type)
        {
            return GetTypeEx(type.FullName);
        }

        /// <summary>
        /// 列挙体のメンバの名前を取得します。
        /// ※インポートしたモジュールの列挙体も参照できます。
        /// </summary>
        /// <param name="type">型情報</param>
        /// <returns>名前</returns>
        public static string[] GetEnumNames(Type type)
        {
            return Enum.GetNames(GetTypeEx(type));
        }

        /// <summary>
        /// 対応するCbXXX型を求めます。
        /// </summary>
        /// <param name="type">オリジナルの型情報</param>
        /// <returns>CbXXX型の型情報</returns>
        public static Type ConvertCbType(Type type)
        {
            switch (type.Name)
            {
                case nameof(Byte): return typeof(CbByte);
                case nameof(SByte): return typeof(CbSByte);
                case nameof(Int16): return typeof(CbShort);
                case nameof(Int32): return typeof(CbInt);
                case nameof(Int64): return typeof(CbLong);
                case nameof(UInt16): return typeof(CbUShort);
                case nameof(UInt32): return typeof(CbUInt);
                case nameof(UInt64): return typeof(CbULong);
                case nameof(Char): return typeof(CbChar);
                case nameof(Single): return typeof(CbFloat);
                case nameof(Double): return typeof(CbDouble);
                case nameof(Decimal): return typeof(CbDecimal);
                case nameof(Boolean): return typeof(CbBool);
                case nameof(String): return typeof(CbString);
                case nameof(Object): return typeof(CbObject);
                case nameof(CbText): return typeof(CbText);
                case nameof(CbImagePath): return typeof(CbImagePath);
                default:
                    break;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type param = type.GenericTypeArguments[0];
                switch (param.Name)
                {
                    case nameof(Byte): return typeof(CbNullableByte);
                    case nameof(SByte): return typeof(CbNullableSByte);
                    case nameof(Int16): return typeof(CbNullableShort);
                    case nameof(Int32): return typeof(CbNullableInt);
                    case nameof(Int64): return typeof(CbNullableLong);
                    case nameof(UInt16): return typeof(CbNullableUShort);
                    case nameof(UInt32): return typeof(CbNullableUInt);
                    case nameof(UInt64): return typeof(CbNullableULong);
                    case nameof(Char): return typeof(CbNullableChar);
                    case nameof(Single): return typeof(CbNullableFloat);
                    case nameof(Double): return typeof(CbNullableDouble);
                    case nameof(Decimal): return typeof(CbNullableDecimal);
                    case nameof(Boolean): return typeof(CbNullableBool);
                    default:
                        break;
                }
                if (CbStruct.IsStruct(param))
                {
                    // 構造体

                    Type cbStructType = typeof(CbNullableStruct<>).MakeGenericType(param);
                    return cbStructType;
                }
                if (param.IsEnum)
                {
                    // 列挙型

                    Type cbEnumType = typeof(CbNullableEnum<>).MakeGenericType(param);
                    return cbEnumType;
                }
                Debug.Assert(false);
                return null;
            }

            if (type.IsEnum)
            {
                Type openedType = typeof(CbEnum<>);
                Type cbEnumType = openedType.MakeGenericType(type);
                return cbEnumType;
            }

            if (CbStruct.IsStruct(type))
            {
                Type openedType = typeof(CbStruct<>);
                Type cbStructType = openedType.MakeGenericType(type);
                return cbStructType;
            }

            if (type.IsValueType)
            {
                // IsEnumとIsStructを抜けてきた未知の値型（今の所、対応する予定は無い）
                // IntPtr 型など

                return null;
            }

            if (type.IsClass || type.IsInterface)
            {
                Type openedType = typeof(CbClass<>);
                Type cbClassType = openedType.MakeGenericType(type);
                return cbClassType;
            }

            if (type.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                Debug.Assert(false);
            }

            if (CbSTUtils.IsDelegate(type))
            {
                var returnType = CbSTUtils.GetDelegateReturnType(type);
                if (CbSTUtils.IsVoid(returnType))
                {
                    return CbFunc.GetFuncType(type, typeof(CbVoid));
                }
                else
                {
                    return CbFunc.GetFuncType(type, returnType);
                }
            }

            return null;
        }

        /// <summary>
        /// 名前無し変数の名前
        /// </summary>
        private static readonly string NO_NAME = "";

        /// <summary>
        /// 対応する CbXXX 型の型を作成します。
        /// </summary>
        /// <typeparam name="T">オリジナルの型</typeparam>
        /// <param name="name">変数名</param>
        /// <returns>CbXXX 型の型</returns>
        public static Func<ICbValue> CbCreateTF<T>()
        {
            return () => CbCreate(typeof(T), NO_NAME);
        }

        /// <summary>
        /// 対応する CbXXX 型の型を作成します。
        /// </summary>
        /// <typeparam name="T">オリジナルの型</typeparam>
        /// <param name="name">変数名</param>
        /// <returns>CbXXX 型の型</returns>
        public static Func<ICbValue> CbCreateTF(Type type)
        {
            return () => CbCreate(type, NO_NAME);
        }

        /// <summary>
        /// 対応する CbXXX 型の型を作成します。
        /// </summary>
        /// <typeparam name="T">オリジナルの型</typeparam>
        /// <param name="name">変数名</param>
        /// <returns>CbXXX 型の型</returns>
        public static Func<string, ICbValue> CbCreateNTF<T>()
        {
            return (_name) => CbCreate(typeof(T), _name);
        }

        /// <summary>
        /// 対応する CbXXX 型の型を作成します。
        /// </summary>
        /// <typeparam name="T">オリジナルの型</typeparam>
        /// <param name="name">変数名</param>
        /// <returns>CbXXX 型の型</returns>
        public static Func<string, ICbValue> CbCreateNTF(Type type)
        {
            return (_name) => CbCreate(type, _name);
        }

        /// <summary>
        /// 対応する CbXXX 型の変数を作成します。
        /// </summary>
        /// <typeparam name="T">オリジナルの型</typeparam>
        /// <param name="name">変数名</param>
        /// <returns>CbXXX 型の変数</returns>
        public static ICbValue CbCreate<T>(string name = "", object value = null)
        {
            var variable = CbCreate(typeof(T), name);
            if (value != null)
            {
                variable.Data = value;
            }
            return variable;
        }

        /// <summary>
        /// 対応する CbXXX 型の変数を作成します。
        /// </summary>
        /// <param name="geneType">オリジナルのジェネリック型</param>
        /// <param name="argType">ジェネリックの引数型</param>
        /// <param name="name">変数名</param>
        /// <returns>CbXXX 型の変数</returns>
        public static ICbValue CbCreate(Type geneType, Type argType, string name = "", object value = null)
        {
            Type type = geneType.MakeGenericType(argType);
            var variable = CbCreate(type, name);
            if (value != null)
            {
                variable.Data = value;
            }
            return variable;
        }

        /// <summary>
        /// 対応する CbXXX 型の変数を作成します。
        /// </summary>
        /// <param name="geneType">オリジナルのジェネリック型</param>
        /// <param name="argTypes">ジェネリックの引数型</param>
        /// <param name="name">変数名</param>
        /// <returns>CbXXX 型の変数</returns>
        public static ICbValue CbCreate(Type geneType, Type[] argTypes, string name = "", object value = null)
        {
            Type type = geneType.MakeGenericType(argTypes);
            var variable = CbCreate(type, name);
            if (value != null)
            {
                variable.Data = value;
            }
            return variable;
        }

        /// <summary>
        /// 対応する CbXXX 型の変数を作成します。
        /// </summary>
        /// <typeparam name="T">オリジナルの型</typeparam>
        /// <param name="name">変数名</param>
        /// <returns>CbXXX 型の変数</returns>
        public static ICbValue CbCreate(Type type, string name = "", object value = null)
        {
            var variable = _CbCreate(type, name, false);
            if (value != null)
            {
                variable.Data = value;
            }
            return variable;
        }

        private static ICbValue _CbCreate(Type type, string name, bool isCancelClass)
        {
            string typeName = type.Name;

            if (!isCancelClass && type.IsByRef)
            {
                // リファレンス情報を消す

                typeName = typeName.Replace("&", "");
            }

            switch (typeName)
            {
                case nameof(Byte): return CbByte.Create(name);
                case nameof(SByte): return CbSByte.Create(name);
                case nameof(Int16): return CbShort.Create(name);
                case nameof(Int32): return CbInt.Create(name);
                case nameof(Int64): return CbLong.Create(name);
                case nameof(UInt16): return CbUShort.Create(name);
                case nameof(UInt32): return CbUInt.Create(name);
                case nameof(UInt64): return CbULong.Create(name);
                case nameof(Char): return CbChar.Create(name);
                case nameof(Single): return CbFloat.Create(name);
                case nameof(Double): return CbDouble.Create(name);
                case nameof(Decimal): return CbDecimal.Create(name);
                case nameof(Boolean): return CbBool.Create(name);
                case nameof(String): return CbString.Create(name);
                case nameof(Object): return CbObject.Create(name);
                case nameof(CbText): return CbText.Create(name);
                case nameof(CbImagePath): return CbImagePath.Create(name);
                default:
                    break;
            }

            if (type.IsArray)
            {
                // 配列は、List<>に置き換える

                string _mame = "xxx";
                if (type.FullName != null)
                    _mame = type.FullName;
                else if (type.Name != null)
                    _mame = type.Name;
                Type tType = CbST.GetTypeEx(_mame);
                if (tType != null)
                {
                    Type element = tType.GetElementType();
                    if (element != null)
                    {
                        Type collectionType = typeof(List<>).MakeGenericType(element);
                        var ret = CbList.Create(collectionType, name);
                        if (ret.IsList)
                        {
                            ICbList cbList = ret.GetListValue;
                            cbList.IsArrayType = true;
                        }
                        return ret;
                    }
                }
                if (type.ContainsGenericParameters)
                {
                    return CbGeneMethArg.NTF(name, type, type.GetGenericArguments(), false);
                }
            }

            if (type.IsEnum)
            {
                // 列挙型

                return CbEnumTools.EnumValue(type, name);
            }

            if (!type.IsGenericType && CbSTUtils.IsDelegate(type))
            {
                // デリゲート型

                return CbFunc.FuncValue(type, typeof(CbVoid), name);
            }

            if (CbSTUtils.HaveGenericParamater(type))
            {
                // 確定していない型なので仮の型に差し替える

                if (!type.IsPublic)
                    return null;

                if (CbSTUtils.IsDelegate(type))
                {
                    // 確定していないデリゲート型

                    return CbGeneMethArg.NTF(name, type, type.GetGenericArguments(), true);
                }
                return CbGeneMethArg.NTF(name, type, type.GetGenericArguments(), false);
            }

            if (type.IsGenericType)
            {
                // ジェネリック型

                if (CbList.HaveInterface(type, typeof(IEnumerable<>)))
                {
                    // リスト管理（UIで特別扱い）

                    if (type.GenericTypeArguments.Length > 1)
                        return null;

                    return CbList.Create(type, name);
                }

                if (CbSTUtils.IsDelegate(type))
                {
                    // デリゲート型

                    var returnType = CbSTUtils.GetDelegateReturnType(type);
                    if (CbSTUtils.IsVoid(returnType))
                    {
                        return CbFunc.FuncValue(type, typeof(CbVoid), name);
                    }
                    else
                    {
                        return CbFunc.FuncValue(type, returnType, name);
                    }
                }

                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Null許容型

                    if (type.GenericTypeArguments.Length > 1)
                        return null;

                    Type param = type.GenericTypeArguments[0];
                    switch (param.Name)
                    {
                        case nameof(Byte): return CbNullableByte.Create(name);
                        case nameof(SByte): return CbNullableSByte.Create(name);
                        case nameof(Int16): return CbNullableShort.Create(name);
                        case nameof(Int32): return CbNullableInt.Create(name);
                        case nameof(Int64): return CbNullableLong.Create(name);
                        case nameof(UInt16): return CbNullableUShort.Create(name);
                        case nameof(UInt32): return CbNullableUInt.Create(name);
                        case nameof(UInt64): return CbNullableULong.Create(name);
                        case nameof(Char): return CbNullableChar.Create(name);
                        case nameof(Single): return CbNullableFloat.Create(name);
                        case nameof(Double): return CbNullableDouble.Create(name);
                        case nameof(Decimal): return CbNullableDecimal.Create(name);
                        case nameof(Boolean): return CbNullableBool.Create(name);
                        default:
                            break;
                    }
                    if (CbStruct.IsStruct(param))
                    {
                        // 構造体

                        return CbStruct.NullableStructValue(param, name);
                    }
                    if (param.IsEnum)
                    {
                        // 列挙型

                        return CbEnumTools.NullableEnumValue(param, name);
                    }
                    Debug.Assert(false);
                    return null;
                }

                // その他のジェネリックは、構造体かクラスとして扱う
                if (CbStruct.IsStruct(type))
                {
                    return CbStruct.StructValue(type, name);
                }
                return CbClass.ClassValue(type, name);
            }

            if (CbStruct.IsStruct(type))
            {
                // 構造体

                if (type.FullName == "System.Void")
                {
                    Debug.Assert(false);
                    return null;
                }

                if (!isCancelClass)
                {
                    var ret = CbStruct.StructValue(type, name);
                    if (ret != null && ret is ICbStruct cbStruct)
                    {
                        return _CbCreate(cbStruct.OriginalReturnType, name, true);
                    }
                }
                return CbStruct.StructValue(type, name);
            }

            if (type.IsClass || type.IsInterface)
            {
                // クラス

                var elements = CbSTUtils.GetGenericIEnumerables(type);
                if (elements.Count() != 0)
                {
                    // IEnumerable<> を持っているのでリストとして扱う（ただし、オリジナルのデータ形式も保存する）

                    var elementType = elements.First();    // 最初に見つかった要素のみを対象とする（妥協）
                    var chgType = CbList.Create(typeof(IEnumerable<>).MakeGenericType(new Type[] { elementType }), name);
                    (chgType as ICbList).CastType = type; // 元の型にキャスト（メソッドを呼ぶときは、オリジナルのデータ形式を参照する）
                    return chgType;
                }

                if (!isCancelClass)
                {
                    var ret = CbClass.ClassValue(type, name);
                    if (ret != null && ret is ICbClass cbClass)
                    {
                        return _CbCreate(cbClass.OriginalReturnType, name, true);
                    }
                }
                return CbClass.ClassValue(type, name);
            }

            return null;
        }
    }

    //----------------------------------------------------------------------------------------
    public interface ICbVSValueBase
        : IDisposable
    {
        /// <summary>
        /// 自分の型情報を参照します。
        /// </summary>
        Type MyType { get; }

        /// <summary>
        /// オリジナルの返し値の型情報を参照します。
        /// </summary>
        Type OriginalReturnType { get; }

        /// <summary>
        /// オリジナルの型情報を参照します。
        /// </summary>
        Type OriginalType { get; }
    }

    public interface IUIShowValue
        : IDisposable
    {
        /// <summary>
        /// 値のUI上の文字列表現
        /// </summary>
        string ValueUIString { get; }

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        string ValueString { get; set; }
    }

    /// <summary>
    /// CbVSValue機能インターフェイス
    /// </summary>
    public interface ICbValue 
        : ICbVSValueBase
        , IUIShowValue
    {
        /// <summary>
        /// 変数生成用型情報
        /// </summary>
        Func<ICbValue> NodeTF { get; }
        /// <summary>
        /// 型の名前
        /// </summary>
        string TypeName { get; }
        /// <summary>
        /// 変数名
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 変数名は変更不可か？
        /// </summary>
        bool IsReadOnlyName { get; }
        /// <summary>
        /// 引数時ref修飾されているか？
        /// </summary>
        bool IsByRef { get; set; }
        /// <summary>
        /// 引数時out修飾されているか？
        /// </summary>
        bool IsOut { get; set; }
        /// <summary>
        /// 引数時in修飾されているか？
        /// </summary>
        bool IsIn { get; set; }
        /// <summary>
        /// リテラルかどうか？
        /// </summary>
        bool IsLiteral { get; set; }
        /// <summary>
        /// デリゲート型かどうか？
        /// </summary>
        bool IsDelegate { get; }
        /// <summary>
        /// リストか否か？（ToArray も含めるので ICollection ではない）
        /// </summary>
        bool IsList { get; }
        /// <summary>
        /// リスト形式の値を返します。
        /// </summary>
        ICbList GetListValue { get; }
        /// <summary>
        /// 変数の値は変更不可か？
        /// </summary>
        bool IsReadOnlyValue { get; set; }
        /// <summary>
        /// UIで変数の値を表示するか？
        /// </summary>
        bool IsVisibleValue { get; }
        /// <summary>
        /// 値の変化後に動かす必要のある処理です。
        /// </summary>
        Action<object> ReturnAction { set; get; }
        /// <summary>
        /// 変数の値
        /// </summary>
        object Data { get; set; }
        /// <summary>
        /// 変数の値は文字列表示が可能か？
        /// </summary>
        bool IsStringableValue { get; }
        bool IsNull { get; }
        /// <summary>
        /// null許容型か？
        /// </summary>
        bool IsNullable { get; }
        bool IsAssignment(ICbValue obj, bool isCast = false);
        bool IsError { get; set; }
        string ErrorMessage { get; set; }
        void Set(ICbValue n);
        void Add(ICbValue n);
        void Subtract(ICbValue n);
        void Multiply(ICbValue n);
        void Divide(ICbValue n);
        void Modulo(ICbValue n);
        bool Equal(ICbValue n);
        bool GreaterThanOrEqual(ICbValue n);
        bool GreaterThan(ICbValue n);
        bool LessThanOrEqual(ICbValue n);
        bool LessThan(ICbValue n);
    }

    /// <summary>
    /// 指定の型にCbVSValue機能インターフェイスを指定する
    /// </summary>
    /// <typeparam name="T">CbVSValue機能インターフェイスを指定する型</typeparam>
    public interface ICbValueClass<T> 
        : ICbValue
    {
        //ICbYSValueClass<T> Create(T n);
        T Value { get; set; }
    }

    /// <summary>
    /// リスト用インターフェイス
    /// </summary>
    public interface ICbValueList
    {
        /// <summary>
        /// リストの要素を作成するイベント
        /// </summary>
        Func<ICbValue> NodeTF { get; }
    }

    /// <summary>
    /// CbVSValue機能インターフェイスを持ったリスト用インターフェイス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICbValueListClass<T> 
        : ICbValueList
        , ICbValueClass<T>
    {
    }

    /// <summary>
    /// enum型用インターフェイス
    /// </summary>
    public interface ICbValueEnum
        : ICbValue
    {
        /// <summary>
        /// enum型の要素リスト
        /// </summary>
        string[] ElementList { get; }
    }

    /// <summary>
    /// CbVSValue機能インターフェイスを持ったenum型用インターフェイス
    /// </summary>
    /// <typeparam name="T">enum型</typeparam>
    public interface ICbValueEnumClass<T> 
        : ICbValueEnum
        , ICbValueClass<T>
    {
    }

    /// <summary>
    /// 実行可能アセットインターフェイス
    /// </summary>
    public interface ICbExecutable
    {
        object RequestExecute(List<object> functionStack, DummyArgumentsStack preArgument);
    }

    /// <summary>
    /// データ表示インターフェイス（※TODO 実装を見直す）
    /// </summary>
    public interface ICbShowValue
    {
        string DataString { get; }
    }

    /// <summary>
    /// void型を表現するクラスです。
    /// </summary>
    public class CbVoid
        : ICbShowValue
    {
        public string DataString => "(void)";
        public static Func<ICbValue> TF = () => CbClass<CbVoid>.Create();
        public static Func<string, ICbValue> NTF = (name) => CbClass<CbVoid>.Create(name);
        public static Type T => typeof(CbClass<CbVoid>);
        public static bool Is(Type type)
        {
            return type == CbVoid.T || type == typeof(CbVoid);
        }
        public static CbVoid Create()
        {
            return new CbVoid();
        }
    }

    /// <summary>
    /// ジェネリックメソッドの引数型を表現するクラスです。
    /// </summary>
    public class CbGeneMethArg 
        : ICbShowValue
    {
        public Type ArgumentType = null;
        public Type[] GeneArgTypes = null;
        public bool IsEvent = false;
        public string DataString => "(GMA)";
        public static Func<ICbValue> TF = () => CbClass<CbGeneMethArg>.Create();
        public static Func<string, Type, Type[], bool, ICbValue> NTF = (name, argType, geneArgTypes, isEvent) =>
        {
            var ret = CbClass<CbGeneMethArg>.Create(name);
            ret.Value = new CbGeneMethArg();
            ret.Value.ArgumentType = argType;
            ret.Value.GeneArgTypes = geneArgTypes;
            ret.Value.IsEvent = isEvent;
            return ret;
        };
        public static Type T => typeof(CbClass<CbGeneMethArg>);
        public static bool Is(Type type)
        {
            return type == CbGeneMethArg.T || type == typeof(CbGeneMethArg);
        }
    }

    //----------------------------------------------------------------------------------------
    /// <summary>
    /// 型定義用ベースクラス
    /// </summary>
    /// <typeparam name="T">型</typeparam>
    public class BaseCbValueClass<T>
    {
        public virtual Func<ICbValue> NodeTF => () => CbST.CbCreate(OriginalType);

        protected T _value;

        public virtual bool IsAssignment(ICbValue obj, bool isCast)
        {
            if (obj is ParamNameOnly)
                return false;

            if (IsList)
            {
                ICbList cbList = GetListValue;

                if (isCast && cbList.IsArrayType && obj.IsList)
                    return true;    // ToArrya() を行う特殊なキャスト

                if (isCast && obj.IsList)
                {
                    ICbList ListObj = obj.GetListValue;
                    if (ListObj.IsArrayType)
                        return true;    // List<>(array) を行う特殊なキャスト
                }

                if (OriginalType.IsAssignableFrom(obj.OriginalType))
                    return true;
            }
            return CbSTUtils.IsAssignment(OriginalType, obj.OriginalType, isCast);
        }

        public bool IsError { get; set; } = false;

        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// 変数の持つ値を参照します。
        /// ※ object として扱う場合は Data を参照します。
        /// </summary>
        public virtual T Value 
        {
            get => _value;
            set
            {
                _value = value;
                isNull = false;
            }
        }

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        public virtual string ValueString
        {
            get => ValueUIString;
            set { }
        }

        /// <summary>
        /// 型の名前を参照します。
        /// </summary>
        public virtual string TypeName
        {
            get
            {
                string typeName;
                if (Value is null)
                {
                    typeName = CbSTUtils._GetTypeName(OriginalType);
                }
                else
                {
                    typeName = CbSTUtils.GetTypeName(Value as object);
                }
                return typeName;
            }
        }

        /// <summary>
        /// 自分の型情報を参照します。
        /// </summary>
        public virtual Type MyType => throw new NotImplementedException();

        /// <summary>
        /// オリジナルの返し値の型（Func, Action, List 以外は OriginalReturnType と同じ）を参照します。
        /// </summary>
        public virtual Type OriginalReturnType => typeof(T);

        /// <summary>
        /// オリジナルの型（Func, Action, List 以外は OriginalReturnType と同じ）を参照します。
        /// </summary>
        public virtual Type OriginalType
        {
            get
            {
                if (IsNullable)
                {
                    return typeof(Nullable<>).MakeGenericType(new Type[] { typeof(T) });
                }
                return typeof(T);
            }
        }

        /// <summary>
        /// 引数時ref修飾されているか？
        /// </summary>
        public bool IsByRef 
        {
            get => isByRef || IsOut;
            set 
            {
                isByRef = value;
            }
        }
        private bool isByRef = false;
        /// <summary>
        /// 引数時out修飾されているか？
        /// </summary>
        public bool IsOut { get; set; } = false;
        /// <summary>
        /// 引数時in修飾されているか？
        /// </summary>
        public bool IsIn { get; set; } = false;
        /// <summary>
        /// リテラルかどうか？
        /// ※変数以外は、原則リテラル
        /// </summary>
        public bool IsLiteral { get; set; } = true;
        /// <summary>
        /// デリゲート型かどうか？
        /// </summary>
        public virtual bool IsDelegate => false;

        /// <summary>
        /// リストか否か？（ToArray も含めるので ICollection ではない）
        /// </summary>
        public virtual bool IsList => false;

        /// <summary>
        /// リスト形式の値を返します。
        /// </summary>
        public virtual ICbList GetListValue
        {
            get
            {
                Debug.Assert(IsList);
                return this as ICbList;
            }
        }

        /// <summary>
        /// 変数名を参照します。
        /// </summary>
        public virtual string Name { get; set; } = "";

        /// <summary>
        /// 変数名は変更禁止か？
        /// </summary>
        public virtual bool IsReadOnlyName => false;

        /// <summary>
        /// 値のUI上の文字列表現
        /// 変数の持つ値を文字列として参照します。
        /// </summary>
        public virtual string ValueUIString
        {
            get
            {
                if (IsError)
                    return CbSTUtils.ERROR_STR;
                if (IsNull)
                    return CbSTUtils.UI_NULL_STR;
                return Value.ToString();
            }
        }

        /// <summary>
        /// 値の変化後に動かす必要のある処理です。
        /// </summary>
        public Action<object> ReturnAction { set; get; } = null;

        /// <summary>
        /// 変数の持つ値は変更禁止か？
        /// </summary>
        public virtual bool IsReadOnlyValue { get; set; } = false;

        /// <summary>
        /// 変数の値は UI での表示を許されるか？
        /// </summary>
        public virtual bool IsVisibleValue => true;

        /// <summary>
        /// 変数の持つ値を object として参照します。
        /// ※ 型を厳密に扱う場合は Value を参照します。
        /// </summary>
        public virtual object Data {
            get
            {
                Debug.Assert(!(IsNullable && IsNull));
                return Value as object;
            }
            set
            {
                if (value == null)
                {
                    isNull = true;
                }
                else if (CbScript.IsCalcable(typeof(T)))
                {
                    // ただのキャストでは sbyte から int への変換などで例外が出るので ChangeType を使って変換する

                    Value = (T)Convert.ChangeType(value, typeof(T));
                }
                else
                {
                    // 複雑なものは dynamic に処理する

                    Value = (dynamic)value;
                }
            }
        }

        /// <summary>
        /// 変数の持つ値は文字列に置き換えてやり取り可能か？
        /// </summary>
        public virtual bool IsStringableValue => true;

        /// <summary>
        /// 変数の持つ値は null か？
        /// </summary>
        public virtual bool IsNull { get => isNull; }

        /// <summary>
        /// null許容型か？
        /// </summary>
        public virtual bool IsNullable => false;

        protected bool isNull = false;

        public virtual void Set(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);

                ReturnAction = null;
                if (IsNullable && n.IsNull)
                {
                    //TODO ここは将来Nullableなクラスへのsetをoverrideして分離する
                    // Nullable<> に null を代入

                    // 値型に null の状態を持つ必要がある
                    isNull = true;
                }
                else
                {
                    if (n is CbObject cbObject)
                    {
                        n = (dynamic)cbObject.ValueTypeObject;
                    }

                    if (GetType() == typeof(CbBool) || GetType() == typeof(CbNullableBool))
                    {
                        //TODO ここは将来CbBoolとCbNullableBoolクラスのsetをoverrideして分離する

                        if (n.Data.GetType() == typeof(bool))
                        {
                            Data = n.Data;
                        }
                        else
                        {
                            Data = (dynamic)n.Data != 0;
                        }
                    }
                    else if (CbScript.IsCalcable(typeof(T)))
                    {
                        // ただのキャストでは sbyte から int への変換などで例外が出るので ChangeType を使って変換する

                        Value = (T)Convert.ChangeType(n.Data, typeof(T));

                        // 参照渡しの為のリアクションのコピー
                        ReturnAction = n.ReturnAction;
                    }
                    else if (!IsList && n.IsList)
                    {
                        //TODO ここは将来CbListクラスのsetをoverrideして分離する
                        // リストはオリジナルの型にしないと代入できない

                        ICbList cbList = n.GetListValue;
                        
                        Value = (T)cbList.ConvertOriginalTypeList(null, null);

                        if (this is ICbClass cbClass)
                        {
                            // List は、参照型なので Value の値が更新されると cbList に戻す必要がある。

                            cbClass.ReturnAction = (val) => cbList.CopyFrom(val);
                        }
                    }
                    else
                    {
                        if (IsNullable)
                        {
                            //TODO ここは将来Nullableなクラスのsetをoverrideして分離する

                            Value = (dynamic)Convert.ChangeType(n.Data, OriginalReturnType);
                        }
                        else
                        {
                            Value = (dynamic)n.Data;
                        }
                    }
                    if (IsList && n.IsList)
                    {
                        //TODO ここは将来CbListクラスのsetをoverrideして分離する
                        // ※IEnumerableを持ったクラスの場合、リストにはオリジナルのデータがある場合があるのでコピーする

                        (this as ICbList).OriginalData = (n as ICbList).OriginalData;
                        ReturnAction = n.ReturnAction;
                    }

                    isNull = n.IsNull;
                }
                IsLiteral = n.IsLiteral;
                if (IsError)
                {
                    IsError = false;
                    ErrorMessage = "";
                }
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual void Add(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                Value += (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual void Subtract(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                Value -= (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual void Multiply(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                Value *= (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual void Divide(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                Value /= (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual void Modulo(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                Value %= (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual bool Equal(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                return Value == (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual bool GreaterThanOrEqual(ICbValue n) 
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                return Value >= (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual bool GreaterThan(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                return Value > (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual bool LessThanOrEqual(ICbValue n) 
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                return Value <= (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public virtual bool LessThan(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);
                return Value < (dynamic)n.Data;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }
        public void ClearWork()
        {
            ReturnAction = null;
            if (IsNullable)
            {
                Data = null;
            }
        }
    }

    //----------------------------------------------------------------------------------------
    /// <summary>
    /// 型がstringでデータとして名前を扱う名前だけのパラメータクラス
    /// </summary>
    public class ParamNameOnly
        : ICbValue
    {
        public Func<ICbValue> NodeTF { get => throw new NotImplementedException(); }

        public virtual string TypeName => "";

        public Type MyType => typeof(ParamNameOnly);

        public Type OriginalReturnType => typeof(string);

        public Type OriginalType => typeof(string);

        private string name = "";
        public string Name { get => name; set { name = value; } }

        public bool IsReadOnlyName { get; set; } = false;

        public bool IsLiteral { get; set; } = false;

        public bool IsByRef { get; set; } = false;

        public virtual bool IsDelegate => false;

        public virtual bool IsList => false;

        public virtual ICbList GetListValue => null;

        public string ValueUIString { get => name; set { name = value; } }

        public string ValueString { get; set; } = null;

        public Action<object> ReturnAction { set; get; } = null;

        public bool IsReadOnlyValue { get; set; } = true;

        public bool IsVisibleValue => false;

        public bool IsStringableValue => true;

        public bool IsAssignment(ICbValue obj, bool isCast)
        {
            return obj != null;
        }

        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; } = "";

        public bool IsNull { get => false; }

        public bool IsNullable { get; set; } = false;

        public object Data 
        { 
            get => name;
            set
            {
                if (value is string str)
                    name = str;
                else
                    throw new NotImplementedException();
            }
        }

        public bool IsOut { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsIn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ParamNameOnly(string name, bool readOnly = false)
        {
            Name = name;
            IsReadOnlyName = readOnly;
        }

        public static ParamNameOnly Create(string name = "", bool readOnly = false)
        {
            var ret = new ParamNameOnly(name, readOnly);
            return ret;
        }

        public void Set(ICbValue n) { }
        public void Add(ICbValue n) { }
        public void Subtract(ICbValue n) { }
        public void Multiply(ICbValue n) { }
        public void Divide(ICbValue n) { }
        public void Modulo(ICbValue n) { }
        public bool Equal(ICbValue n) { return false; }
        public bool GreaterThanOrEqual(ICbValue n) { return false; }
        public bool GreaterThan(ICbValue n) { return false; }
        public bool LessThanOrEqual(ICbValue n) { return false; }
        public bool LessThan(ICbValue n) { return false; }

        public Func<ICbValue> TF = () => ParamNameOnly.Create();
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

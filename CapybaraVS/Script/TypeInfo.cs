//#define SHOW_LINK_ARRAY   // リスト型を接続したときにリストの要素をコピーして表示する

using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Windows;
using System.Reflection;
using static CapybaraVS.Controls.MultiRootConnector;
using static CapybaraVS.Script.ScriptImplement;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Media;
using CbVS.Script;

namespace CapybaraVS.Script
{
    public enum CbType // この定義は将来的に不要になる予定
    {
        none,
        Int,
        String,
        Double,
        Byte,
        Sbyte,
        Long,
        Short,
        UShort,
        UInt,
        ULong,
        Char,
        Float,
        Decimal,
        Bool,
        Object,
        Class,
        Func,
    }

    public enum CbCType // この定義は将来的に不要になる予定
    {
        none,
        List,
        Enum,
        Func,
        Class,
    }

    /// <summary>
    /// 型を管理する
    /// ※管理している型の復元及び複製機能を持っている
    /// </summary>
    public class CbST
    {
        #region XML定義
        [XmlRoot(nameof(CbST))]
        public class _AssetXML<OwnerClass>
            where OwnerClass : CbST
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    self.LiteralType = LiteralType;
                    self.ObjectType = ObjectType;

                    if (LiteralType == CbType.Func)
                    {
                        // 型情報を元に型を再現する。
                        // 将来的にこちらにまとめる予定。

                        self.Value = CbCreate(Type.GetType(ClassName));
                        self.LiteralTypeForCbTypeFunc = ClassName;
                    }
                    else if (ObjectType == CbCType.Class)
                    {
                        Type openedType = typeof(CbClass<>); //CapybaraVS.Script.CbClass`1
                        Type argType = Type.GetType(ClassName);
                        Type cbClassType = openedType.MakeGenericType(argType);

                        self.Value = cbClassType.InvokeMember("Create", BindingFlags.InvokeMethod,
                                  null, null, new object[] { "" }) as ICbValue;
                    }
                    else if (self.ObjectType == CbCType.Enum)
                    {
                        Type openedType = typeof(CbEnum<>); //CapybaraVS.Script.CbEnum`1
                        Type argType = Type.GetType(ClassName);
                        Type cbEnumType = openedType.MakeGenericType(argType);

                        self.Value = cbEnumType.InvokeMember("Create", BindingFlags.InvokeMethod,
                                  null, null, new object[] { "" }) as ICbValue;
                    }
                    else if (ObjectType == CbCType.List && LiteralType == CbType.Class)
                    {
                        Func<ICbValue> listNodeType =
                            () =>
                            {
                                Type openedType = typeof(CbClass<>); //CapybaraVS.Script.CbClass`1
                                Type argType = Type.GetType(ClassName);
                                Type cbClassType = openedType.MakeGenericType(argType);

                                return cbClassType.InvokeMember("Create", BindingFlags.InvokeMethod,
                                          null, null, new object[] { "" }) as ICbValue;
                            };

                        self.Value = listNodeType();
                    }

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<CbST>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    LiteralType = self.LiteralType;
                    ObjectType = self.ObjectType;
                    if (LiteralType == CbType.Func)
                    {
                        // 型名を保存する

                        ClassName = self.LiteralTypeForCbTypeFunc;

                        if (self.Value is ICbList cbList)
                        {
                            ListNodeValue = new List<string>();
                            if (LiteralType == CbType.Class)
                            {
                                // ノードがクラスタイプなので型名を保存する

                                //ClassName = cbList.ItemName;
                            }
                            foreach (var nd in cbList.Value)
                            {
                                ListNodeValue.Add(nd.ValueString);
                            }
                        }
                    }
                    else if (self.Value != null)
                    {
                        if (self.ObjectType == CbCType.Class)
                        {
                            ClassName = (self.Value as ICbClass).ItemName;
                        }
                        else if (self.ObjectType == CbCType.Enum)
                        {
                            ClassName = (self.Value as ICbEnum).ItemName;
                        }
                        else if (self.Value is ICbList cbList)
                        {
                            Debug.Assert(false);    // ここは使用しない

                            ListNodeValue = new List<string>();
                            if (LiteralType == CbType.Class)
                            {
                                // ノードがクラスタイプなので型名を保存する

                                ClassName = cbList.ItemName;
                            }
                            foreach (var nd in cbList.Value)
                            {
                                ListNodeValue.Add(nd.ValueString);
                            }
                        }
                    }
                };
            }
#region 固有定義
            public CbType LiteralType { get; set; } = CbType.none;
            public CbCType ObjectType { get; set; } = CbCType.none;
            public List<string> ListNodeValue { get; set; } = null;
            public string ClassName = null;
#endregion
        }
        public _AssetXML<CbST> AssetXML { get; set; } = null;
#endregion

        /// <summary>
        /// 型の指定を自由にする
        /// </summary>
        public static CbST FreeType => new CbST(CbType.none);

        /// <summary>
        /// 型の指定をイベント型にする
        /// </summary>
        public static CbST FreeFuncType => new CbST(CbType.none, CbCType.Func);

        /// <summary>
        /// 型の指定をリスト型にする
        /// </summary>
        public static CbST FreeListType => new CbST(CbType.none, CbCType.List);

        public static CbST Int => new CbST(CbType.Int);
        public static CbST Bool => new CbST(CbType.Bool);
        public static CbST String => new CbST(CbType.String);
        public static CbST Double => new CbST(CbType.Double);
        public static CbST Object => new CbST(CbType.Object);

        /// <summary>
        /// 型の指定を enum 型にする
        /// </summary>
        public static CbST Enum => new CbST(CbType.none, CbCType.Enum);

        public CbST()
        {
            AssetXML = new _AssetXML<CbST>(this);
        }

        /// <summary>
        /// LiteralType が CbType.Func のときに変わりに参照する値の型 
        /// </summary>
        public string LiteralTypeForCbTypeFunc { get; set; } = null;

        /// <summary>
        /// Value を参考にクラスの変数を返します。
        /// </summary>
        /// <param name="name">変数名</param>
        /// <returns>CbClass<T>型の変数</returns>
        private ICbValue ClassValue(string name)
        {
            if (ObjectType != CbCType.Class)
                return null;

            return ClassValue(Value, name);
        }

        /// <summary>
        /// CbClass<T> 型なら同様の型の変数を返します。
        /// </summary>
        /// <param name="value">参考変数</param>
        /// <param name="name">変数名</param>
        /// <returns>CbClass<T>型の変数</returns>
        public static ICbValue ClassValue(ICbValue value, string name)
        {
            if (value is ICbClass cbClass)
            {
                return ClassValue(cbClass.OriginalReturnType, name);
            }
            return null;
        }

        /// <summary>
        /// オリジナル型情報から CbClass<type>型の変数を返します。
        /// </summary>
        /// <param name="type">オリジナルのクラスの型</param>
        /// <param name="name"></param>
        /// <returns>CbClass<T>型の変数</returns>
        public static ICbValue ClassValue(Type type, string name)
        {
            string typeName = type.FullName;
            if (type.IsByRef)
            {
                // リファレンス（スクリプト変数接続）

                typeName = typeName.Replace("&", "");
                type = Type.GetType(typeName);
            }
            Type openedType = typeof(CbClass<>); //CapybaraVS.Script.CbClass`1
            Type cbClassType = openedType.MakeGenericType(type);

            object result = cbClassType.InvokeMember("Create", BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        /// <summary>
        /// Value を参考に共用体の変数を返します。
        /// </summary>
        /// <param name="name">変数名</param>
        /// <returns>CbEnum<T>型の変数</returns>
        private ICbValue EnumValue(string name)
        {
            if (ObjectType != CbCType.Enum)
                return null;

            return EnumValue(Value, name);
        }

        /// <summary>
        /// CbEnum<T> 型なら同様の型の変数を返します。
        /// </summary>
        /// <param name="value">参考変数</param>
        /// <param name="name">変数名</param>
        /// <returns>CbEnum<T>型の変数</returns>
        public static ICbValue EnumValue(ICbValue value, string name)
        {
            if (value is ICbEnum cbEnum)
            {
                return EnumValue(Type.GetType(cbEnum.ItemName), name);
            }
            return null;
        }

        /// <summary>
        /// オリジナル型情報から CbEnum<type>型の変数を返します。
        /// </summary>
        /// <param name="type">オリジナルの共用体の型</param>
        /// <param name="name">変数名</param>
        /// <returns>CbEnum<type>型の変数</returns>
        public static ICbValue EnumValue(Type type, string name)
        {
            string typeName = type.FullName;
            if (type.IsByRef)
            {
                // リファレンス（スクリプト変数接続）

                typeName = typeName.Replace("&", "");
                type = Type.GetType(typeName);
            }
            Type openedType = typeof(CbEnum<>); //CapybaraVS.Script.CbEnum`1
            Type cbEnumType = openedType.MakeGenericType(type);

            object result = cbEnumType.InvokeMember("Create", BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        /// <summary>
        /// Func<object, type> 型の CbFunc<type> 型の変数を返します。
        /// </summary>
        /// <param name="type">オリジナルの元々の型</param>
        /// <param name="retType">オリジナルの返し値の型</param>
        /// <param name="name">変数名</param>
        /// <returns>CbFunc<type> 型の変数</returns>
        public static ICbValue FuncValue(Type type, Type retType, string name)
        {
            string typeName = type.FullName;
            if (type.IsByRef)
            {
                // リファレンス（スクリプト変数接続）

                typeName = typeName.Replace("&", "");
                type = Type.GetType(typeName);
            }

            foreach (var arg in type.GenericTypeArguments)
            {
                Type cbType = ConvertCbType(arg);
                if (cbType is null)
                    return null;
            }

            return CbFunc.CreateFuncFromOriginalType(type, retType, name);
        }

        /// <summary>
        /// メソッド呼び出し用引数情報リストの中にイベント型を含んでいるかを判定します。
        /// </summary>
        /// <param name="arguments">メソッド呼び出し用引数情報リスト</param>
        /// <returns>イベント型を含んでいるなら true</returns>
        public static bool ConteinsEvent(List<ArgumentInfoNode> arguments)
        {
            if (arguments is null)
                return false;
            foreach (var node in arguments)
            {
                if (node.CreateArgument() is ICbEvent)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// CbXXX 型からオリジナルの型を求めます。
        /// </summary>
        /// <param name="type">CbXXX の型</param>
        /// <returns>オリジナルの型</returns>
        public static Type ConvertOriginalType(Type type)
        {
            switch (type.Name)
            {
                case nameof(CbByte): return typeof(Byte);
                case nameof(CbSByte): return typeof(SByte);
                case nameof(CbShort): return typeof(Int16);
                case nameof(CbInt): return typeof(Int32);
                case nameof(CbLong): return typeof(Int64);
                case nameof(CbUShort): return typeof(UInt16);
                case nameof(CbUInt): return typeof(UInt32);
                case nameof(CbULong): return typeof(UInt64);
                case nameof(CbChar): return typeof(Char);
                case nameof(CbFloat): return typeof(Single);
                case nameof(CbDouble): return typeof(Double);
                case nameof(CbDecimal): return typeof(Decimal);
                case nameof(CbBool): return typeof(Boolean);
                case nameof(CbString): return typeof(String);
                case nameof(CbObject): return typeof(Object);
                case nameof(CbVoid): return typeof(CbVoid);
                default:
                    break;
            }

            if (type.GetGenericTypeDefinition() == typeof(CbFunc<,>))
            {
                return type.GetGenericArguments()[0];
            }
            if (type.GetGenericTypeDefinition() == typeof(CbEnum<>))
            {
                return type.GetGenericArguments()[0];
            }
            if (type.GetGenericTypeDefinition() == typeof(CbClass<>))
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        /// <summary>
        /// 対応する CbXXX 型を求めます。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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
                default:
                    break;
            }

            if (type.IsEnum)
            {
                Type openedType = typeof(CbEnum<>);
                Type cbEnumType = openedType.MakeGenericType(type);
                return cbEnumType;
            }

            if (type.IsClass)
            {
                Type openedType = typeof(CbClass<>);
                Type cbClassType = openedType.MakeGenericType(type);
                return cbClassType;
            }

            if (type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Debug.Assert(false);
            }

            if (CbFunc.IsActionType(type))
            {
                return CbFunc.GetFuncType(type, typeof(CbVoid));
            }

            if (CbFunc.IsFuncType(type))
            {
                return CbFunc.GetFuncType(type, type.GenericTypeArguments.Last());
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
                default:
                    break;
            }

            if (type.IsEnum)
            {
                return EnumValue(type, name);
            }

            if (type == typeof(Action))
            {
                if (CbFunc.IsActionType(type))
                {
                    return FuncValue(type, typeof(CbVoid), name);
                }
            }

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    if (type.GenericTypeArguments.Length > 1)
                        return null;

                    return CbList.Create(type.GenericTypeArguments[0], name);
                }

                if (CbFunc.IsActionType(type))
                {
                    return FuncValue(type, typeof(CbVoid), name);
                }

                if (CbFunc.IsFuncType(type))
                {
                    return FuncValue(type, type.GenericTypeArguments.Last(), name);
                }

                // その他のジェネリックは、クラスとして扱う
                return ClassValue(type, name);
            }

            if (type.IsClass || type.IsInterface)
            {
                if (!isCancelClass)
                {
                    var ret = ClassValue(type, name);
                    if (ret != null && ret is ICbClass cbClass)
                    {
                        //return _CbCreate(Type.GetType(cbClass.ItemName), name, true);
                        return _CbCreate(cbClass.OriginalReturnType, name, true);
                    }
                }
                return ClassValue(type, name);
            }

            return null;
        }



        //================================================================================================
        // 以下は、将来的に無くす（型情報をベタに持つ予定）
        //================================================================================================

        /// <summary>
        /// 値の型
        /// </summary>
        public CbType LiteralType { get; set; } = CbType.none;

        /// <summary>
        /// 値の種類
        /// </summary>
        public CbCType ObjectType { get; set; } = CbCType.none;

        /// <summary>
        /// 型復元の為の参照値
        /// </summary>
        public ICbValue Value = null;

        public CbST(CbType LiteralType, CbCType ObjectType = CbCType.none)
        {
            AssetXML = new _AssetXML<CbST>(this);
            this.LiteralType = LiteralType;
            this.ObjectType = ObjectType;
        }

        public CbST(CbST cbTypeInfo)
        {
            AssetXML = new _AssetXML<CbST>(this);
            LiteralType = cbTypeInfo.LiteralType;
            ObjectType = cbTypeInfo.ObjectType;
            Value = cbTypeInfo.Value;
            LiteralTypeForCbTypeFunc = cbTypeInfo.LiteralTypeForCbTypeFunc;
        }

        public CbST(CbType type, string typeName)
        {
            AssetXML = new _AssetXML<CbST>(this);
            LiteralType = type;
            LiteralTypeForCbTypeFunc = typeName; // LiteralType の失われた情報を補完する
        }

        /// <summary>
        /// 型情報の一致を判定します。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool Eq(CbST target)
        {
            if (ObjectType == CbCType.Class && target.ObjectType == CbCType.Class)
            {
                if (Value.TypeName == target.Value.TypeName)
                    return true;
                return false;
            }
            return LiteralType == target.LiteralType && ObjectType == target.ObjectType;
        }

        /// <summary>
        /// 変数から CbXXX 型の変数生成用型情報を作成します。
        /// </summary>
        /// <param name="value"></param>
        public CbST(ICbValue value)
        {
            AssetXML = new _AssetXML<CbST>(this);
            if (value is ICbList)
            {
                Value = value;
                LiteralType = Value.CbType.LiteralType;
                ObjectType = Value.CbType.ObjectType;
                LiteralTypeForCbTypeFunc = value.CbType.LiteralTypeForCbTypeFunc;
            }
            else if (value is ICbEnum)
            {
                Value = value;
                LiteralType = CbType.Object;
                ObjectType = CbCType.Enum;
            }
            else if (value is ICbClass)
            {
                Value = value;
                LiteralType = CbType.Class;
                ObjectType = CbCType.Class;
            }
            else
            {
                Value = value;
                LiteralType = value.CbType.LiteralType;
                ObjectType = Value.CbType.ObjectType;
                LiteralTypeForCbTypeFunc = value.CbType.LiteralTypeForCbTypeFunc;
            }
        }

        /// <summary>
        /// 対応する CbType 型を求めます。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static CbType ConvertEnumCbType(Type type)
        {
            switch (type.Name)
            {
                case nameof(Byte): return CbType.Byte;
                case nameof(SByte): return CbType.Sbyte;
                case nameof(Int16): return CbType.Short;
                case nameof(Int32): return CbType.Int;
                case nameof(Int64): return CbType.Long;
                case nameof(UInt16): return CbType.UShort;
                case nameof(UInt32): return CbType.UInt;
                case nameof(UInt64): return CbType.ULong;
                case nameof(Char): return CbType.Char;
                case nameof(Single): return CbType.Float;
                case nameof(Double): return CbType.Double;
                case nameof(Decimal): return CbType.Decimal;
                case nameof(Boolean): return CbType.Bool;
                case nameof(String): return CbType.String;
                case nameof(Object): return CbType.Object;
                default:
                    break;
            }

            if (type.IsEnum)
            {
                return CbType.Object;
            }

            if (type.IsClass)
            {
                return CbType.Object;
            }

            Debug.Assert(false);
            return CbType.none;
        }

        /// <summary>
        /// CbXXX 型の変数生成用型情報からオリジナルの型を求めます。
        /// </summary>
        /// <param name="cbTypeInfo">CbXXX 型の変数生成用型情報</param>
        /// <returns>オリジナルの型</returns>
        public static Type GetOriginalType(CbST cbTypeInfo)
        {
            switch (cbTypeInfo.LiteralType)
            {
                case CbType.Int: return typeof(int);
                case CbType.String: return typeof(string);
                case CbType.Double: return typeof(double);
                case CbType.Byte: return typeof(byte);
                case CbType.Sbyte: return typeof(sbyte);
                case CbType.Long: return typeof(long);
                case CbType.Short: return typeof(short);
                case CbType.UShort: return typeof(ushort);
                case CbType.UInt: return typeof(uint);
                case CbType.ULong: return typeof(ulong);
                case CbType.Char: return typeof(char);
                case CbType.Float: return typeof(float);
                case CbType.Decimal: return typeof(decimal);
                case CbType.Bool: return typeof(bool);
                case CbType.Object: return typeof(object);
                case CbType.Func: return Type.GetType(cbTypeInfo.LiteralTypeForCbTypeFunc);
            }
            
            if (cbTypeInfo.Value is ICbValue cbVSValue)
            {
                return cbVSValue.OriginalType;
            }

            return null;
        }

        /// <summary>
        /// ObjectType を無効にしたCbXXX 型の変数作成用型情報を参照します。
        /// </summary>
        /// <returns></returns>
        public CbST GetObjectTypeNone()
        {
            var newInfo = new CbST(this);
            newInfo.ObjectType = CbCType.none;
            return newInfo;
        }

        /// <summary>
        /// 対応する CbXXX 型の変数を作成します。
        /// </summary>
        /// <param name="cbTypeInfo">CbXXX 型の変数作成用型情報</param>
        /// <param name="name">変数名</param>
        /// <param name="forcedLiteral">true なら Func型の場合でも変数を返します。</param>
        /// <returns>CbXXX 型の変数</returns>
        public static ICbValue Create(
            CbST cbTypeInfo, 
            string name = ""
            )
        {
            if (cbTypeInfo.LiteralType == CbType.none)
                return null;

            if (cbTypeInfo.LiteralType == CbType.Func)
            {
                // Func タイプは、そのままの値が Value に入っている

                return CbCreate(Type.GetType(cbTypeInfo.LiteralTypeForCbTypeFunc), name);
            }

            if (cbTypeInfo.ObjectType == CbCType.none)
            {
                return Create(cbTypeInfo.LiteralType, name, cbTypeInfo.Value);
            }
            else if (cbTypeInfo.ObjectType == CbCType.Class)
            {
                return cbTypeInfo.ClassValue(name);
            }
            else if (cbTypeInfo.ObjectType == CbCType.Enum)
            {
                return cbTypeInfo.EnumValue(name);
            }
            else if (cbTypeInfo.ObjectType == CbCType.List)
            {
                return CbList.Create(GetOriginalType(cbTypeInfo), name);
            }
            else if (cbTypeInfo.ObjectType == CbCType.Func)
            {
                if (cbTypeInfo.Value != null)
                {
                    // 完全な型の変数を持っている

                    return cbTypeInfo.Value;
                }

                // 以下、システム実装ファンクションノードのための処理

                switch (cbTypeInfo.LiteralType)
                {
                    case CbType.Int: return CbFunc<Func<object, int>, CbInt>.Create(name);
                    case CbType.String: return CbFunc<Func<object, string>, CbString>.Create(name);
                    case CbType.Double: return CbFunc<Func<object, double>, CbDouble>.Create(name);
                    case CbType.Byte: return CbFunc<Func<object, byte>, CbByte>.Create(name);
                    case CbType.Sbyte: return CbFunc<Func<object, sbyte>, CbSByte>.Create(name);
                    case CbType.Long: return CbFunc<Func<object, long>, CbLong>.Create(name);
                    case CbType.Short: return CbFunc<Func<object, short>, CbShort>.Create(name);
                    case CbType.UShort: return CbFunc<Func<object, ushort>, CbUShort>.Create(name);
                    case CbType.UInt: return CbFunc<Func<object, uint>, CbUInt>.Create(name);
                    case CbType.ULong: return CbFunc<Func<object, ulong>, CbULong>.Create(name);
                    case CbType.Char: return CbFunc<Func<object, char>, CbChar>.Create(name);
                    case CbType.Float: return CbFunc<Func<object, float>, CbFloat>.Create(name);
                    case CbType.Decimal: return CbFunc<Func<object, decimal>, CbDecimal>.Create(name);
                    case CbType.Bool: return CbFunc<Func<object, bool>, CbBool>.Create(name);
                    case CbType.Object: return CbFunc<Func<object, object>, CbObject>.Create(name);
                    case CbType.Class:
                        if (cbTypeInfo.Value is ICbClass cbClass)
                        {
                            Type openedCbClass = typeof(CbClass<>); //CapybaraVS.Script.CbClass`1
                            Type classType = Type.GetType(cbClass.ItemName);
                            Type cbClassType = openedCbClass.MakeGenericType(classType);

                            if (classType == typeof(CbVoid))
                            {
                                Type cbFuncType2 =
                                    typeof(CbFunc<,>).MakeGenericType(
                                        typeof(Func<>).MakeGenericType(new Type[] { typeof(Func<object>) }),
                                        cbClassType
                                        );
                                return cbFuncType2.InvokeMember("Create", BindingFlags.InvokeMethod,
                                            null, null, new object[] { name }) as ICbValue;
                            }

                            Type cbFuncType =
                                typeof(CbFunc<,>).MakeGenericType(
                                    typeof(Func<,>).MakeGenericType(new Type[] { typeof(Func<object>), cbClassType }),
                                    cbClassType
                                    );
                            return cbFuncType.InvokeMember("Create", BindingFlags.InvokeMethod,
                                        null, null, new object[] { name }) as ICbValue;
                        }
                        break;
                    default:
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// 対応する CbXXX 型の変数を返します。
        /// CbType.Class の場合は、init を参考に復元します。
        /// </summary>
        /// <param name="cbType">型の種類</param>
        /// <param name="name">変数名</param>
        /// <param name="init">CbType.Class 用の参考変数</param>
        /// <returns>CbXXX 型の変数</returns>
        private static ICbValue Create(CbType cbType, string name = "", ICbValue init = null)
        {
            switch (cbType)
            {
                case CbType.Int: return CbInt.Create(name);
                case CbType.String: return CbString.Create("", name);
                case CbType.Double: return CbDouble.Create(name);
                case CbType.Byte: return CbByte.Create(name);
                case CbType.Sbyte: return CbSByte.Create(name);
                case CbType.Long: return CbLong.Create(name);
                case CbType.Short: return CbShort.Create(name);
                case CbType.UShort: return CbUShort.Create(name);
                case CbType.UInt: return CbUInt.Create(name);
                case CbType.ULong: return CbULong.Create(name);
                case CbType.Char: return CbChar.Create(name);
                case CbType.Float: return CbFloat.Create(name);
                case CbType.Decimal: return CbDecimal.Create(name);
                case CbType.Bool: return CbBool.Create(name);
                case CbType.Object: return CbObject.Create(name);
                case CbType.Class: return ClassValue(init, name);
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// 対応する CbXXX 型の変数生成用型情報を返します。
        /// </summary>
        /// <param name="cbTypeInfo">CbXXX 型の変数作成用型情報</param>
        /// <param name="name">変数名</param>
        /// <param name="forcedLiteral">true なら Func型の場合でも変数を返します。</param>
        /// <returns>対応する CbXXX 型の変数生成用型情報</returns>
        public static Func<ICbValue> CreateTF(
            CbST cbTypeInfo, 
            string name = ""
            )
        {
            if (cbTypeInfo.LiteralType == CbType.none)
                return null;
            return () => Create(cbTypeInfo, name);
        }
    }

    //----------------------------------------------------------------------------------------
    public interface ICbVSValueBase
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

    /// <summary>
    /// CbVSValue機能インターフェイス
    /// </summary>
    public interface ICbValue : ICbVSValueBase
    {
        /// <summary>
        /// CbST型情報を参照します。
        /// </summary>
        CbST CbType { get; }
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
        /// 値の文字列表現
        /// </summary>
        string ValueString { get; set; }
        /// <summary>
        /// 変数の値は変更不可か？
        /// </summary>
        bool IsReadOnlyValue { get; set; }
        /// <summary>
        /// UIで変数の値を表示するか？
        /// </summary>
        bool IsVisibleValue { get; }
        /// <summary>
        /// 変数の値
        /// </summary>
        object Data { get; set; }
        void CopyValue(ICbValue cbVSValue);
        /// <summary>
        /// 変数の値は文字列表示が可能か？
        /// </summary>
        bool IsStringableValue { get; }
        bool IsNull { get; }
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
    public interface ICbValueClass<T> : ICbValue
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
    public interface ICbValueListClass<T> : ICbValueList, ICbValueClass<T>
    {
    }

    /// <summary>
    /// enum型用インターフェイス
    /// </summary>
    public interface ICbValueEnum : ICbValue
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
    public interface ICbValueEnumClass<T> : ICbValueEnum, ICbValueClass<T>
    {
    }

    /// <summary>
    /// 実行可能アセットインターフェイス
    /// </summary>
    public interface ICbExecutable
    {
        void RequestExecute(List<object> functionStack, DummyArgumentsStack preArgument);
    }

    /// <summary>
    /// データ表示インターフェイス
    /// </summary>
    public interface ICbShowValue
    {
        string DataString { get; }
    }

    /// <summary>
    /// void型を表現するクラスです。
    /// </summary>
    public class CbVoid : ICbShowValue
    {
        public string DataString => "(void)";
        public static Func<ICbValue> TF = () => CbClass<CbVoid>.Create();
        public static Func<string, ICbValue> NTF = (name) => CbClass<CbVoid>.Create(name);
        public static Type T => typeof(CbClass<CbVoid>);
        public static bool Is(Type type)
        {
            return type == CbVoid.T || type == typeof(CbVoid);
        }
    }

    //----------------------------------------------------------------------------------------
    /// <summary>
    /// 型定義用ベースクラス
    /// </summary>
    /// <typeparam name="T">型</typeparam>
    public class BaseCbValueClass<T>
    {
        protected const string ERROR_STR = "[ERROR]";
        protected const string NULL_STR = "<null>";

        /// <summary>
        /// CbST型情報を参照します。
        /// </summary>
        public virtual CbST CbType
        {
            get
            {
                return new CbST(
                    Script.CbType.Func,
                    typeof(T).FullName   // 型名を持っていないとスクリプト読み込み時に再現できない
                    );
            }
        }

        public virtual Func<ICbValue> NodeTF => () => CbST.Create(CbType);

        protected T _value;
        
        public virtual bool IsAssignment(ICbValue obj, bool isCast)
        {
            if (obj is ParamNameOnly)
                return false;
            if (this is ICbList)
                if (TypeName != obj.TypeName)
                    return false;
            return CbSTUtils.IsAssignment(TypeName, obj.TypeName, OriginalType, obj.OriginalType, isCast);
        }

        public bool IsError { get; set; } = false;

        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// 変数の持つ値を参照します。
        /// ※ object として扱う場合は Data を参照します。
        /// </summary>
        public virtual T Value { get => _value; set { _value = value; } }

        /// <summary>
        /// 型の名前を参照します。
        /// </summary>
        public virtual string TypeName => CbSTUtils.GetTypeName(Value as object);

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
        public virtual Type OriginalType => typeof(T);

        /// <summary>
        /// 変数名を参照します。
        /// </summary>
        public virtual string Name { get; set; } = "";

        /// <summary>
        /// 変数名は変更禁止か？
        /// </summary>
        public virtual bool IsReadOnlyName => false;

        /// <summary>
        /// 変数の持つ値を文字列として参照します。
        /// </summary>
        public virtual string ValueString { get; set; }

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
        public virtual object Data { get => Value as object; set { Value = (T)value; } }

        /// <summary>
        /// 変数の持つ値をコピーします（ディープコピーではない）。
        /// </summary>
        /// <param name="cbVSValue"></param>
        public virtual void CopyValue(ICbValue cbVSValue)
        {
            if (!(this is ICbList) && cbVSValue is ICbList cbList)
            {
                // リストはオリジナルの型にしないと代入できない

                Value = (T)cbList.ConvertOriginalTypeList(null, null);
            }
            else
            {
                Data = cbVSValue.Data;
            }

            IsError = cbVSValue.IsError;
            ErrorMessage = cbVSValue.ErrorMessage;
        }

        /// <summary>
        /// 変数の持つ値は文字列に置き換えてやり取り可能か？
        /// </summary>
        public virtual bool IsStringableValue => true;

        /// <summary>
        /// 変数の持つ値は null か？
        /// </summary>
        public virtual bool IsNull { get => Value is null; }

        public virtual void Set(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);

                if (n is CbObject cbObject)
                {
                    Value = (dynamic)cbObject.ValueTypeObject.Data;
                }
                else
                    Value = (dynamic)n.Data;
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
    }

    //----------------------------------------------------------------------------------------
    /// <summary>
    /// 型がstringでデータとして名前を扱う名前だけのパラメータクラス
    /// </summary>
    public class ParamNameOnly : ICbValue
    {
        public CbST CbType { get => throw new NotImplementedException(); }

        public Func<ICbValue> NodeTF { get => throw new NotImplementedException(); }

        public virtual string TypeName => "";

        public Type MyType => typeof(ParamNameOnly);

        public Type OriginalReturnType => typeof(string);

        public Type OriginalType => typeof(string);

        private string name = "";
        public string Name { get => name; set { name = value; } }

        public bool IsReadOnlyName { get; set; } = false;

        public string ValueString { get => name; set { name = value; } }

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

        public virtual void CopyValue(ICbValue cbVSValue)
        {
            Data = cbVSValue.Data;
            IsError = cbVSValue.IsError;
            ErrorMessage = cbVSValue.ErrorMessage;
            if (this is ICbEvent cbEvent && cbVSValue is ICbEvent cbEven2)
            {
                cbEvent.CallBack = cbEven2.CallBack;
            }
        }

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
    }
}

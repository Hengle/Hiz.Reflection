using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

namespace QuickReflection
{
    abstract class MemberToken : IEquatable<MemberToken>
    {
        #region 静态

        protected const char GenericSeparator = '`';

        protected const string ArraySeparator = ", ";
        protected const char MemberSeparator = '.';
        protected const char GenericPrefix = '<';
        protected const char GenericSuffix = '>';
        protected const char MethodPrefix = '(';
        protected const char MethodSuffix = ')';
        protected const char IndexPrefix = '[';
        protected const char IndexSuffix = ']';

        public const BindingFlags Default = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags Declared = Default | BindingFlags.DeclaredOnly;

        #endregion

        #region 构造属性
        public abstract MemberTypes MemberType { get; }

        Type _Type;
        public Type Type { get { return _Type; } }

        string _MemberName;
        public string MemberName { get { return _MemberName; } }

        public MemberToken(Type type, string name)
        {
            if (type == null)
                throw new ArgumentNullException();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException();

            this._Type = type;
            this._MemberName = name;
        }

        protected static void CheckTypeArray(Type[] types)
        {
            if (types != null)
            {
                var length = types.Length;
                for (var i = 0; i < length; i++)
                    if (types[i] == null)
                        throw new ArgumentNullException();
            }
        }

        #endregion

        #region 相等比较
        public virtual bool Equals(MemberToken other)
        {
            return other != null
                && this.MemberType == other.MemberType
                && this._Type == other._Type
                && this._MemberName == other._MemberName;
        }
        public override bool Equals(object other)
        {
            return base.Equals(other as MemberToken);
        }

        // 计算两个数组是否相同
        protected static bool SequenceEquals<T>(T[] left, T[] right)
        {
            return (left == right) || (left == null && right == null) || (left != null && right != null && SequenceEqualsPrivate(left, right));
        }
        static bool SequenceEqualsPrivate<T>(T[] left, T[] right)
        {
            var length = left.Length;
            if (length != right.Length)
                return false;

            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < length; i++)
                if (!comparer.Equals(left[i], right[i]))
                    return false;

            return true;
        }
        #endregion

        #region 哈希计算
        int _HashCode = 0;
        public override int GetHashCode()
        {
            if (_HashCode == 0)
                _HashCode = CombineHashCodes(this.GetHashCodes());
            return _HashCode;
        }
        protected virtual int[] GetHashCodes()
        {
            var hashes = new[] {
                this.MemberType.GetHashCode(),
                this._Type.GetHashCode(),
                this._MemberName.GetHashCode(),
            };

            return hashes;
        }
        protected static int CombineHashCodes(IEnumerable<Type> types)
        {
            var hashes = types.Select(i => i.GetHashCode()).ToArray();
            return CombineHashCodes(hashes);
        }
        static int CombineHashCodes(int[] hashes)
        {
            var length = hashes.Length;

            // 摘要数组长度
            var value = length;

            while (length-- > 0)
                value = CombineHashCodes(value, hashes[length]);

            return value;
        }
        static int CombineHashCodes(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }
        #endregion

        #region 成员签名

        string _Signature;
        public override string ToString()
        {
            if (_Signature == null)
            {
                var builder = new StringBuilder();
                this.Signature(builder);
                _Signature = builder.ToString();
            }
            return _Signature;
        }
        protected virtual void Signature(StringBuilder builder)
        {
            AppendTypeName(builder, this.Type);
            builder.Append(MemberSeparator);
            builder.Append(this.MemberName);
        }

        protected static void AppendTypeName(StringBuilder builder, Type type)
        {
            if (type.IsGenericType)
            {
                builder.Append(type.Name.Remove(type.Name.LastIndexOf(GenericSeparator)));

                builder.Append(GenericPrefix);
                AppendTypeArray(builder, type.GetGenericArguments());
                builder.Append(GenericSuffix);
            }
            else
                builder.Append(type.Name);
        }
        protected static void AppendTypeArray(StringBuilder builder, Type[] types)
        {
            var length = types.Length;
            var index = 0;
            AppendTypeName(builder, types[index++]);

            while (index < length)
            {
                builder.Append(ArraySeparator);
                AppendTypeName(builder, types[index++]);
            }
        }
        #endregion
    }

    /// <summary>
    /// 字段签名
    /// </summary>
    class FieldToken : MemberToken
    {
        #region 构造属性
        public override MemberTypes MemberType
        {
            get { return MemberTypes.Field; }
        }

        bool _IsGet;
        public bool IsGet { get { return _IsGet; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="get">是否读取操作</param>
        public FieldToken(Type type, string name, bool get)
            : base(type, name)
        {
            this._IsGet = get;
        }
        #endregion

        public override bool Equals(MemberToken other)
        {
            return base.Equals(other) && this.Equals((FieldToken)other);
        }
        bool Equals(FieldToken other)
        {
            return this._IsGet == other._IsGet;
        }

        protected override int[] GetHashCodes()
        {
            var temp = base.GetHashCodes();

            var length = temp.Length;
            var index = length++;
            var hashes = new int[length];
            Array.Copy(temp, hashes, index);
            hashes[index] = this._IsGet.GetHashCode();

            return hashes;
        }

        protected override void Signature(StringBuilder builder)
        {
            base.Signature(builder);

            builder.Append(MemberSeparator);
            if (this._IsGet)
                builder.Append("Get");
            else
                builder.Append("Set");
        }
    }

    /// <summary>
    /// 属性签名
    /// </summary>
    class PropertyToken : MemberToken
    {
        #region 构造属性
        public override MemberTypes MemberType
        {
            get { return MemberTypes.Property; }
        }

        bool _IsGet;
        public bool IsGet { get { return _IsGet; } }

        Type[] _Index;
        public Type[] Index { get { return _Index; } }
        public bool HasIndex
        {
            get
            {
                return _Index != null && _Index.Length > 0;
            }
        }

        public PropertyToken(Type type, string name, bool get, Type[] index = null)
            : base(type, name)
        {
            CheckTypeArray(index);

            this._IsGet = get;
            this._Index = index;
        }
        #endregion

        public override bool Equals(MemberToken other)
        {
            return base.Equals(other) && this.Equals((PropertyToken)other);
        }
        bool Equals(PropertyToken other)
        {
            return this._IsGet == other._IsGet && SequenceEquals(this._Index, other._Index);
        }

        protected override int[] GetHashCodes()
        {
            var temp = base.GetHashCodes();

            var length = temp.Length;
            var index = length++;

            if (this.HasIndex)
                length++;

            var hashes = new int[length];
            Array.Copy(temp, hashes, index);
            hashes[index++] = this._IsGet.GetHashCode();

            if (this.HasIndex)
                hashes[index] = CombineHashCodes(this._Index);

            return hashes;
        }

        protected override void Signature(StringBuilder builder)
        {
            base.Signature(builder);

            if (this.HasIndex)
            {
                builder.Append(IndexPrefix);
                AppendTypeArray(builder, this._Index);
                builder.Append(IndexSuffix);
            }

            builder.Append(MemberSeparator);
            if (this._IsGet)
                builder.Append("Get");
            else
                builder.Append("Set");
        }
    }

    /// <summary>
    /// 方法签名
    /// </summary>
    class MethodToken : MemberToken
    {
        #region 构造属性
        public override MemberTypes MemberType
        {
            get { return MemberTypes.Method; }
        }

        Type[] _Parameters;
        public Type[] Parameters { get { return _Parameters; } }
        public bool HasParameters
        {
            get
            {
                return _Parameters != null && _Parameters.Length > 0;
            }
        }

        Type[] _Generics;
        public Type[] Generics { get { return _Generics; } }
        public bool HasGenerics
        {
            get
            {
                return _Generics != null && _Generics.Length > 0;
            }
        }

        public MethodToken(Type type, string name, Type[] parameters = null, Type[] generics = null)
            : base(type, name)
        {
            CheckTypeArray(parameters);
            CheckTypeArray(generics);

            this._Parameters = parameters;
            this._Generics = generics;
        }
        #endregion

        public override bool Equals(MemberToken other)
        {
            return base.Equals(other) && this.Equals((MethodToken)other);
        }
        bool Equals(MethodToken temp)
        {
            return SequenceEquals(this._Parameters, temp._Parameters) && SequenceEquals(this._Generics, temp._Parameters);
        }

        protected override int[] GetHashCodes()
        {
            var temp = base.GetHashCodes();

            var length = temp.Length;
            var index = length;

            if (this.HasGenerics)
                length++;
            if (this.HasParameters)
                length++;

            var hashes = new int[length];
            Array.Copy(temp, hashes, index);

            if (this.HasGenerics)
                hashes[index] = CombineHashCodes(_Generics);
            if (this.HasParameters)
                hashes[index++] = CombineHashCodes(_Parameters);

            return hashes;
        }

        protected override void Signature(StringBuilder builder)
        {
            base.Signature(builder);

            if (this.HasGenerics)
            {
                builder.Append(GenericPrefix);
                AppendTypeArray(builder, this._Generics);
                builder.Append(GenericSuffix);
            }

            builder.Append(MethodPrefix);
            if (this.HasParameters)
                AppendTypeArray(builder, this._Parameters);
            builder.Append(MethodSuffix);
        }
    }

    // class EventToken : MemberToken
    // {
    //     public override MemberTypes MemberType
    //     {
    //         get { return MemberTypes.Event; }
    //     }
    // }
    // class ConstructorToken : MemberToken
    // {
    //     public override MemberTypes MemberType
    //     {
    //         get { return MemberTypes.Constructor; }
    //     }
    // }
}
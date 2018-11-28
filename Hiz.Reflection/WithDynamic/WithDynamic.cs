using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Hiz.Reflection
{
    public class ReflectionWithDynamic : ReflectionServiceBase
    {
        const string ArgumentNameMember = "member";

        #region Field

        public override Func<TField> MakeGetter<TField>(FieldInfo member)
        {
            // return base.MakeGet<TField>(member);

            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (!member.IsStatic)
                throw Error.OnlyStaticMember(ArgumentNameMember);

            return (Func<TField>)InternalFieldGetWithDynamic(member, null, typeof(TField));
        }
        public override Func<TInstance, TField> MakeGetter<TInstance, TField>(FieldInfo member)
        {
            // return base.MakeGet<TInstance, TField>(member);

            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (member.IsStatic)
                throw Error.OnlyInstanceMember(ArgumentNameMember);

            return (Func<TInstance, TField>)InternalFieldGetWithDynamic(member, typeof(TInstance), typeof(TField));
        }

        public override Action<TField> MakeSetter<TField>(FieldInfo member)
        {
            // return base.MakeSet<TField>(member);

            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (!member.IsStatic)
                throw Error.OnlyStaticMember(ArgumentNameMember);
            if (member.IsInitOnly || member.IsLiteral)
                throw Error.FieldDoesNotHaveSetter(ArgumentNameMember);

            return (Action<TField>)InternalFieldSetWithDynamic(member, null, typeof(TField));
        }
        public override Action<TInstance, TField> MakeSetter<TInstance, TField>(FieldInfo member)
        {
            // return base.MakeSet<TInstance, TField>(member);

            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (member.IsStatic)
                throw Error.OnlyInstanceMember(ArgumentNameMember);
            if (member.IsInitOnly || member.IsLiteral)
                throw Error.FieldDoesNotHaveSetter(ArgumentNameMember);

            return (Action<TInstance, TField>)InternalFieldSetWithDynamic(member, typeof(TInstance), typeof(TField));
        }

        Delegate MakeGetter(FieldInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);

            return InternalFieldGetWithDynamic(member, null, null);
        }
        Delegate MakeSetter(FieldInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (member.IsInitOnly || member.IsLiteral)
                throw Error.FieldDoesNotHaveSetter(ArgumentNameMember);

            return InternalFieldGetWithDynamic(member, null, null);
        }

        /* 读取字段
         * IL:
         * TField Function(TInstance instance) {
         * 1. 载入第一参数:
         *    ldarg.0
         * 2. 转换实例类型: (如果需要)
         * 
         * 
         * 3. 读取字段:
         *    {OpCode} {FieldTypeName} {TargetTypeName::FieldName}
         *    {OpCode}:
         *      1). 实例: ldfld;
         *      2). 静态: ldsfld;
         *    {FieldTypeName}:
         *      1). 标量类型: {TypeName};
         *      2). 引用类型: class {TypeName}
         *      3). 数值类型: valuetype {TypeName}
         * 4. 转换结果类型: (如果需要)
         * 
         * 
         * 5. 退出方法
         *    ret
         * }
         */
        Delegate InternalFieldGetWithDynamic(FieldInfo member, Type instance, Type value)
        {
            var field = member.FieldType;
            if (value == null)
                value = field;

            if (!member.IsStatic)
            {
                var reflected = member.ReflectedType;
                if (instance == null)
                    instance = reflected;

                var @dynamic = CreateDynamicMethod(value, new[] { instance }, _DynamicOptionsField, member);
                var il = @dynamic.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                ConvertIfNeeded(il, reflected, instance); //

                il.Emit(OpCodes.Ldfld, member);
                ConvertIfNeeded(il, value, field); //

                il.Emit(OpCodes.Ret);

                return @dynamic.CreateDelegate(TypeMemberGetInstance.MakeGenericType(new[] { instance, value }));
            }
            else
            {
                if (instance != null)
                    throw new ArgumentException();

                var @dynamic = CreateDynamicMethod(value, null, _DynamicOptionsField, member);
                var il = @dynamic.GetILGenerator();

                il.Emit(OpCodes.Ldsfld, member);
                ConvertIfNeeded(il, value, field); //

                il.Emit(OpCodes.Ret);

                return @dynamic.CreateDelegate(TypeMemberGetStatic.MakeGenericType(new[] { value }));
            }
        }

        /* 修改字段
         * IL:
         * void Action(TInstance instance, TField value) {
         * 1. 载入第一参数:
         *    ldarg.0
         * 2. 转换实例类型: (如果需要)
         * 
         * 
         * 3. 载入第二参数:
         *    ldarg.1
         * 4. 转换字段类型: (如果需要)
         * 
         * 
         * 5. 修改字段:
         *    {OpCode} {FieldTypeName} {TargetTypeName::FieldName}
         *    {OpCode}:
         *      1). 实例: stfld;
         *      2). 静态: stsfld;
         *    {FieldTypeName}:
         *      1). 标量类型: "{TypeName}";
         *      2). 引用类型: "class {TypeName}";
         *      3). 数值类型: "valuetype {TypeName}";
         * 
         * 6. 退出方法
         *    ret
         * }
         */
        Delegate InternalFieldSetWithDynamic(FieldInfo member, Type instance, Type value)
        {
            var field = member.FieldType;
            if (value == null)
                value = field;

            if (!member.IsStatic)
            {
                var reflected = member.ReflectedType;
                if (instance == null)
                    instance = reflected;

                var @dynamic = CreateDynamicMethod(null, new[] { instance, value }, _DynamicOptionsField, member);
                var il = @dynamic.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                ConvertIfNeeded(il, reflected, instance); //

                il.Emit(OpCodes.Ldarg_1);
                ConvertIfNeeded(il, field, value); //

                il.Emit(OpCodes.Stfld, member);

                il.Emit(OpCodes.Ret);

                return @dynamic.CreateDelegate(TypeMemberSetInstance.MakeGenericType(new[] { instance, value }));
            }
            else
            {
                if (instance != null)
                    throw new ArgumentException();

                var @dynamic = CreateDynamicMethod(null, new[] { value }, _DynamicOptionsField, member);
                var il = @dynamic.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                ConvertIfNeeded(il, field, value); //

                il.Emit(OpCodes.Stsfld, member);

                il.Emit(OpCodes.Ret);

                return @dynamic.CreateDelegate(TypeMemberSetStatic.MakeGenericType(new[] { value }));
            }
        }

        // 字段读写动态方法创建配置
        DynamicMethodOptions _DynamicOptionsField = DynamicMethodOptions.WithTypeReflected(true);
        public void SetDynamicMethodOptionsOfField(DynamicMethodOptions options)
        {
            if (options == null)
                throw new ArgumentNullException();

            _DynamicOptionsField = options;
        }

        #endregion

        #region Property

        public override Func<TProperty> MakeGetter<TProperty>(PropertyInfo member)
        {
            // return base.MakeGet<TProperty>(member);

            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (!member.CanRead)
                throw Error.PropertyDoesNotHaveGetter(ArgumentNameMember);
            var method = member.GetGetMethod(true);
            if (!method.IsStatic)
                throw Error.OnlyStaticMember(ArgumentNameMember);

            return (Func<TProperty>)InternalPropertyGetWithDynamic(member, null, typeof(TProperty));
        }
        public override Func<TInstance, TProperty> MakeGetter<TInstance, TProperty>(PropertyInfo member)
        {
            // return base.MakeGet<TInstance, TProperty>(member);

            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (!member.CanRead)
                throw Error.PropertyDoesNotHaveGetter(ArgumentNameMember);
            var method = member.GetGetMethod(true);
            if (method.IsStatic)
                throw Error.OnlyInstanceMember(ArgumentNameMember);

            return (Func<TInstance, TProperty>)InternalPropertyGetWithDynamic(member, typeof(TInstance), typeof(TProperty));
        }

        public override Action<TProperty> MakeSetter<TProperty>(PropertyInfo member)
        {
            // return base.MakeSet<TProperty>(member);

            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (!member.CanWrite)
                throw Error.PropertyDoesNotHaveSetter(ArgumentNameMember);
            var method = member.GetSetMethod(true);
            if (!method.IsStatic)
                throw Error.OnlyStaticMember(ArgumentNameMember);

            return (Action<TProperty>)InternalPropertySetWithDynamic(member, null, typeof(TProperty));
        }
        public override Action<TInstance, TProperty> MakeSetter<TInstance, TProperty>(PropertyInfo member)
        {
            // return base.MakeSet<TInstance, TProperty>(member);

            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (!member.CanWrite)
                throw Error.PropertyDoesNotHaveSetter(ArgumentNameMember);
            var method = member.GetSetMethod(true);
            if (method.IsStatic)
                throw Error.OnlyInstanceMember(ArgumentNameMember);

            return (Action<TInstance, TProperty>)InternalPropertySetWithDynamic(member, typeof(TInstance), typeof(TProperty));
        }

        Delegate MakeGetter(PropertyInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (!member.CanRead)
                throw Error.PropertyDoesNotHaveGetter(ArgumentNameMember);

            return InternalPropertyGetWithDynamic(member, null, null);
        }
        Delegate MakeSetter(PropertyInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull(ArgumentNameMember);
            if (!member.CanWrite)
                throw Error.PropertyDoesNotHaveSetter(ArgumentNameMember);

            return InternalPropertySetWithDynamic(member, null, null);
        }

        /* 读取属性
         * // IL代码: 实际访问 get_PropertyName() 方法;
         * 
         * 1. 载入第一参数 (隐藏实例变量)
         *    ldarg.0
         * 2. 转换实例类型: (如果需要)
         * 
         * 
         * 3. 读取属性:
         *    1). 实例: callvirt instance {MethodReturnTypeName} {TargetTypeName::get_PropertyName()}
         *    2). 静态: call {MethodReturnTypeName} {TargetTypeName::get_PropertyName()}
         *      
         * 4. 转换结果类型: (如果需要)
         * 
         * 5. 退出方法
         *    ret
         */
        Delegate InternalPropertyGetWithDynamic(PropertyInfo member, Type instance, Type value)
        {
            var method = member.GetGetMethod(true);

            var property = member.PropertyType;
            if (value == null)
                value = property;

            if (!method.IsStatic)
            {
                var reflected = method.ReflectedType;
                if (instance == null)
                    instance = reflected;

                var @dynamic = CreateDynamicMethod(value, new[] { instance }, _DynamicOptionsProperty, member);
                var il = @dynamic.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                ConvertIfNeeded(il, reflected, instance); //

                if (reflected.IsValueType)
                    il.EmitCall(OpCodes.Call, method, null);
                else
                    il.EmitCall(OpCodes.Callvirt, method, null);

                ConvertIfNeeded(il, value, property); //

                il.Emit(OpCodes.Ret);

                return @dynamic.CreateDelegate(TypeMemberGetInstance.MakeGenericType(new[] { instance, value }));
            }
            else
            {
                if (instance != null)
                    throw Error.StaticObjectCannotConvertType("instance");

                var @dynamic = CreateDynamicMethod(value, null, _DynamicOptionsProperty, member);
                var il = @dynamic.GetILGenerator();

                il.EmitCall(OpCodes.Call, method, null);
                ConvertIfNeeded(il, value, property); //

                il.Emit(OpCodes.Ret);

                return @dynamic.CreateDelegate(TypeMemberGetStatic.MakeGenericType(new[] { value }));
            }
        }

        /* 修改属性
         * // IL代码: 实际访问 set_PropertyName() 方法;
         * 
         * 1. 载入第一参数:
         *    ldarg.0
         * 2. 转换实例类型: (如果需要)
         * 
         * 
         * 3. 载入第二参数:
         *    ldarg.1
         * 4. 转换字段类型: (如果需要)
         * 
         * 
         * 5. 修改字段:
         *    {OpCode} void {TargetTypeName}::set_PropertyName({PropertyTypeName})
         *    {OpCode}:
         *      1). 实例: callvirt instance;
         *      2). 静态: call;
         *    {PropertyTypeName}:
         *      1). 标量类型: "{TypeName}";
         *      2). 引用类型: "class {TypeName}";
         *      3). 数值类型: "valuetype {TypeName}";
         * 
         * 6. 退出方法
         *    ret
         */
        Delegate InternalPropertySetWithDynamic(PropertyInfo member, Type instance, Type value)
        {
            var method = member.GetSetMethod(true);

            var property = member.PropertyType;
            if (value == null)
                value = property;

            if (!method.IsStatic)
            {
                var reflected = method.ReflectedType;
                if (instance == null)
                    instance = reflected;

                var @dynamic = CreateDynamicMethod(null, new[] { instance, value }, _DynamicOptionsProperty, member);
                var il = @dynamic.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                ConvertIfNeeded(il, reflected, instance); //

                il.Emit(OpCodes.Ldarg_1);
                ConvertIfNeeded(il, property, value); //

                if (reflected.IsValueType)
                    il.EmitCall(OpCodes.Call, method, null);
                else
                    il.EmitCall(OpCodes.Callvirt, method, null);

                il.Emit(OpCodes.Ret);

                return @dynamic.CreateDelegate(TypeMemberSetInstance.MakeGenericType(new[] { instance, value }));
            }
            else
            {
                if (instance != null)
                    throw Error.StaticObjectCannotConvertType("instance");

                var @dynamic = CreateDynamicMethod(null, new[] { value }, _DynamicOptionsProperty, member);
                var il = @dynamic.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                ConvertIfNeeded(il, property, value); //

                il.EmitCall(OpCodes.Call, method, null);

                il.Emit(OpCodes.Ret);

                return @dynamic.CreateDelegate(TypeMemberSetStatic.MakeGenericType(new[] { value }));
            }
        }

        // 属性读写动态方法创建配置
        DynamicMethodOptions _DynamicOptionsProperty = DynamicMethodOptions.WithTypeReflected(true);
        public void SetDynamicMethodOptionsOfProperty(DynamicMethodOptions options)
        {
            if (options == null)
                throw new ArgumentNullException();

            _DynamicOptionsProperty = options;
        }

        #endregion

        #region PropertyIndexer (Only Instance)

        // Func<TInstance, object[], TProperty> MakeGetIndexer<TInstance, TProperty>(PropertyInfo member)
        // {
        //     throw new NotImplementedException();
        // }
        // Action<TInstance, object[], TProperty> MakeSetIndexer<TInstance, TProperty>(PropertyInfo member)
        // {
        //     throw new NotImplementedException();
        // }

        #endregion

        #region Internal

        /* IL 类型转换:
         * ConvertIfNeeded(Type source, Type target) {
         *    if (source != target) {
         *       if target.IsAssignableFrom(source) { // 协变
         *          1). 引用类型无需转换 (隐式转换);
         *          2). 数值类型需要装箱 (隐式转换):
         *              box {SourceType}
         *       } else if source.IsAssignableFrom(target) { // 逆变
         *          1). 引用类型需要转换:
         *              castclass {TargetType}
         *          2). 数值类型需要拆箱:
         *              unbox.any {TargetType}
         *       } else { // 如果实现类型转换运算
         *          1). 标量类型: conv.{TargetType} // 例如: int > long; long > uint;
         *          2). 引用类型: call class {TargetType} {MethodType}::{MethodName}(class {SourceType})
         *          3). 数值类型: call valuetype {TargetType} {MethodType}::{MethodName}(valuetype {SourceType})
         *          //. {MethodName}: op_Explicit 显式转换; op_Implicit 隐式转换;
         *       }
         *    }
         * }
         * 
         * 协变转换例如:
         * object s = ""; // 隐式转换
         * object i = 0; // 装箱
         * ISerializable d = DateTime.Today; // 装箱
         * 
         * 逆变转换例如:
         * object o = ...;
         * string s = (string)o; // 显式转换
         * int i = (int)o; // 拆箱
         */
        static void ConvertIfNeeded(ILGenerator il, Type target, Type source)
        {
            // if (il == null)
            //     throw new ArgumentNullException();
            // if (source == null)
            //     throw new ArgumentNullException();
            // if (target == null)
            //     throw new ArgumentNullException();

            if (source != target)
            {
                // 协变/装箱 (隐式转换)
                if (target.IsAssignableFrom(source))
                {
                    // Object i = String; // 隐式转换
                    // BaseClass i = DerivedClass // 隐式转换
                    // Object i = ValueType // 装箱
                    // Interface i = ValueType // 装箱
                    if (source.IsValueType)
                    {
                        // target: 引用类型/接口类型
                        il.Emit(OpCodes.Box, source);
                    }
                    // else
                    // {
                    //     // source/target: 引用类型/接口类型;
                    // }
                }
                // 逆变/拆箱 (显式转换)
                // 编译时会通过 但运行时可能抛出无效类型转换异常 InvalidCastException.
                else if (source.IsAssignableFrom(target))
                {
                    // String i = (String)Object; // 显式转换
                    // DerivedClass i = (DerivedClass)BaseClass; // 显式转换
                    // ValueType i = (ValueType)Object; // 拆箱
                    // ValueType i = (ValueType)Interface; // 拆箱
                    if (target.IsValueType)
                    {
                        // source: 引用类型/接口类型
                        il.Emit(OpCodes.Unbox_Any, target);
                    }
                    else
                    {
                        // target/source: 引用类型/接口类型;
                        il.Emit(OpCodes.Castclass, target);
                    }
                }
                else
                {
                    throw new NotSupportedException("暂不支持非协变逆变的类型转换");
                }
            }
        }

        /* https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.dynamicmethod(v=vs.100).aspx
         * 
         * 如果动态方法被匿名承载，则它位于系统提供的程序集中，因此独立于其他代码。
         * 默认情况下，动态方法不能访问任何非公共数据。
         * 如果已授予匿名承载的动态方法带有 ReflectionPermissionFlag.RestrictedMemberAccess 标志的 ReflectionPermission，则它可以受限制地跳过 JIT 编译器的可见性检查。
         * 非公共成员可由动态方法访问的程序集的信任级别必须与发出该动态方法的调用堆栈的信任级别（或其子集）相同。
         * 
         * 
         * 如果动态方法与您指定的类型关联，则该动态方法可以访问该类型的所有成员，无论是何访问级别。
         * 此外，还可以跳过 JIT 可见性检查。 这使得动态方法可以访问在相同模块或任何程序集的任何其他模块中声明的其他类型的私有数据。
         * 您可以将动态方法与任何类型关联，但是您的代码必须被授予带有 ReflectionPermissionFlag.RestrictedMemberAccess 和 ReflectionPermissionFlag.MemberAccess 标志的 ReflectionPermission。 
         * 
         * 
         * 如果该动态方法与您指定的模块关联，则该动态方法对于该模块在全局范围内有效。
         * 它可以访问模块中的所有类型和这些类型的所有 internal 成员。
         * 您可以将动态方法与任意模块关联，无论该模块是否由您创建，只要可以通过包含您的代码的调用堆栈满足带有 ReflectionPermissionFlag.RestrictedMemberAccess 标志的 ReflectionPermission 的要求。
         * 如果 ReflectionPermissionFlag.MemberAccess 标志包含在授予中，则该动态方法可以跳过 JIT 编译器的可见性检查，并可以访问在该模块中或任何程序集的任何其他模块中声明的所有类型的私有数据。
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="return">返回类型</param>
        /// <param name="parameters">方法参数类型集合</param>
        /// <param name="options">配置</param>
        /// <param name="member">反射成员</param>
        /// <returns></returns>
        static DynamicMethod CreateDynamicMethod(Type @return, Type[] parameters, DynamicMethodOptions options, MemberInfo member)
        {
            switch (options.Hosted)
            {
                case DynamicMethodHosted.Anonymous:
                    return new DynamicMethod(string.Empty, @return, parameters, options.SkipVisibility);

                case DynamicMethodHosted.TypeSpecify:
                    return new DynamicMethod(string.Empty, @return, parameters, options.Type, options.SkipVisibility);
                case DynamicMethodHosted.TypeReflected:
                    return new DynamicMethod(string.Empty, @return, parameters, member.ReflectedType, options.SkipVisibility);

                case DynamicMethodHosted.ModuleSpecify:
                    return new DynamicMethod(string.Empty, @return, parameters, options.Module, options.SkipVisibility);
                case DynamicMethodHosted.ModuleReflected:
                    return new DynamicMethod(string.Empty, @return, parameters, member.Module, options.SkipVisibility);
            }
            throw new NotSupportedException();
        }

        #endregion
    }

    public class DynamicMethodOptions
    {
        public readonly bool SkipVisibility;

        DynamicMethodOptions(DynamicMethodHosted hosted, bool skip)
        {
            switch (hosted)
            {
                case DynamicMethodHosted.Anonymous:
                case DynamicMethodHosted.TypeReflected:
                case DynamicMethodHosted.ModuleReflected:
                    this._Hosted = hosted;
                    break;
                default:
                    throw new ArgumentException();
            }

            this.SkipVisibility = skip;
        }

        DynamicMethodOptions(Type type, bool skip)
        {
            this._Hosted = DynamicMethodHosted.TypeSpecify;
            this._Type = type;
            this.SkipVisibility = skip;
        }

        DynamicMethodOptions(Module module, bool skip)
        {
            this._Hosted = DynamicMethodHosted.ModuleSpecify;
            this._Module = module;
            this.SkipVisibility = skip;
        }

        readonly DynamicMethodHosted _Hosted;
        public DynamicMethodHosted Hosted { get { return _Hosted; } }

        readonly Type _Type;
        public Type Type { get { return _Type; } }

        readonly Module _Module;
        public Module Module { get { return _Module; } }

        // AnonymousMethodDelegate 匿名方法委托
        // 匿名承载
        public static DynamicMethodOptions WithAnonymous(bool skip = false)
        {
            return new DynamicMethodOptions(DynamicMethodHosted.Anonymous, skip);
        }
        // 关联反射所在类型
        public static DynamicMethodOptions WithTypeReflected(bool skip = false)
        {
            return new DynamicMethodOptions(DynamicMethodHosted.TypeReflected, skip);
        }
        // 关联反射所在模块
        public static DynamicMethodOptions WithModuleReflected(bool skip = false)
        {
            return new DynamicMethodOptions(DynamicMethodHosted.ModuleReflected, skip);
        }
        // 关联特定类型
        public static DynamicMethodOptions WithTypeSpecify(Type type, bool skip = false)
        {
            if (type == null)
                throw new ArgumentNullException();
            return new DynamicMethodOptions(type, skip);
        }
        // 关联特定模块
        public static DynamicMethodOptions WithModuleSpecify(Module module, bool skip = false)
        {
            if (module == null)
                throw new ArgumentNullException();
            return new DynamicMethodOptions(module, skip);
        }
    }

    public enum DynamicMethodHosted
    {
        // 匿名承载
        Anonymous = 0,

        // 关联反射所在类型
        TypeReflected,
        // 关联特定类型
        TypeSpecify,

        // 关联反射所在模块
        ModuleReflected,
        // 关联特定模块
        ModuleSpecify,
    }
}

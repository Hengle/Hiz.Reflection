using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hiz.Reflection
{
    partial class ReflectionWithExpression
    {
        #region Property.Predefined/T1-T2

        /// <summary>
        /// 用于静态属性 (支持 实例/属性 类型转换)
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public override Func<TProperty> MakeGetter<TProperty>(PropertyInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.CanRead)
                throw Error.PropertyDoesNotHaveGetter("member");
            var method = member.GetGetMethod(true);
            if (!method.IsStatic)
                throw Error.OnlyStaticMember("member");

            /* 示例:
             * class Model {
             *     static string PropertyStatic { get; set; }
             * }
             * 
             * 情况1: PropertyGet<string>()
             * string Function() {
             *     return Model.PropertyStatic;
             * }
             * 
             * 情况2: PropertyGet<object>()
             * object Function() {
             *     return (object)Model.PropertyStatic;
             * }
             * 
             * 格式:
             * TProperty Function()
             * {
             *     return (TProperty)Static.PropertyStatic;
             * }
             */
            var lambda = InternalPropertyGetWithExpression(member, true, null, typeof(TProperty));
            return (Func<TProperty>)lambda.Compile();
        }

        /// <summary>
        /// 用于实例属性 (支持 实例/属性 类型转换)
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public override Func<TInstance, TProperty> MakeGetter<TInstance, TProperty>(PropertyInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.CanRead)
                throw Error.PropertyDoesNotHaveGetter("member");
            var method = member.GetGetMethod(true);
            if (method.IsStatic)
                throw Error.OnlyInstanceMember("member");

            /* 示例:
             * class Model {
             *     string PropertyInstance { get; set; }
             * }
             * 
             * 情况1: PropertyGet<Instance, string>()
             * string Function(Model instance) {
             *     return instance.PropertyInstance;
             * }
             * 
             * 情况2: PropertyGet<Instance, object>()
             * object Function(Model instance) {
             *     return (object)instance.PropertyInstance;
             * }
             * 
             * 情况3: PropertyGet<object, object>()
             * object Function(object instance) {
             *     return (object)((Model)instance).PropertyInstance;
             * }
             * 
             * 格式:
             * TProperty Function(object instance)
             * {
             *     return (TProperty)((TInstance)instance).PropertyInstance;
             * }
             */
            var lambda = InternalPropertyGetWithExpression(member, false, typeof(TInstance), typeof(TProperty));
            return (Func<TInstance, TProperty>)lambda.Compile();
        }

        /// <summary>
        /// 用于静态属性 (支持 实例/属性 类型转换)
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public override Action<TProperty> MakeSetter<TProperty>(PropertyInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.CanWrite)
                throw Error.PropertyDoesNotHaveSetter("member");
            var method = member.GetSetMethod(true);
            if (!method.IsStatic)
                throw Error.OnlyStaticMember("member");

            /* 示例:
             * class Model {
             *     static string PropertyStatic { get; set; }
             * }
             * 
             * 情况1: PropertySet<string>()
             * void Action(string property) {
             *     Model.PropertyStatic = property;
             * }
             * 
             * 情况2: PropertySet<object>()
             * void Action(object property) {
             *     Model.PropertyStatic = (string)property;
             * }
             * 
             * 格式:
             * void Action(object property)
             * {
             *     Static.PropertyStatic = (TProperty)property;
             * }
             */
            var lambda = InternalPropertySetWithExpression(member, true, null, typeof(TProperty));
            return (Action<TProperty>)lambda.Compile();
        }

        /// <summary>
        /// 用于实例属性 (支持 实例/属性 类型转换)
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public override Action<TInstance, TProperty> MakeSetter<TInstance, TProperty>(PropertyInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.CanWrite)
                throw Error.PropertyDoesNotHaveSetter("member");
            var method = member.GetSetMethod(true);
            if (method.IsStatic)
                throw Error.OnlyInstanceMember("member");

            /* 示例:
             * class Model {
             *     string PropertyInstance { get; set; }
             * }
             * 
             * 情况1: PropertySet<Model, string>()
             * void Action(Model instance, string property) {
             *     instance.PropertyInstance = property;
             * }
             * 
             * 情况2: PropertySet<Model, object>()
             * void Action(Model instance, object property) {
             *     instance.PropertyInstance = (string)property;
             * }
             * 
             * 情况3: PropertySet<object, object>()
             * void Action(object instance, object property) {
             *     ((Model)instance).PropertyInstance = (string)property;
             * }
             * 
             * 格式:
             * void Action(object instance, object property)
             * {
             *     ((TInstance)instance).PropertyInstance = (TProperty)property;
             * }
             */
            var lambda = InternalPropertySetWithExpression(member, false, typeof(TInstance), typeof(TProperty));
            return (Action<TInstance, TProperty>)lambda.Compile();
        }

        static LambdaExpression InternalPropertyGetWithExpression(PropertyInfo member, bool? @static, Type @object, Type @return)
        {
            if (!@static.HasValue)
                @static = member.GetGetMethod(true).IsStatic;

            // Expression.Lambda.Parameters
            IEnumerable<ParameterExpression> parameters;

            // 实例转换
            Expression convert;
            if (!@static.Value)
            {
                // 定义实例参数
                var reflected = member.ReflectedType;
                var instance = Expression.Parameter(@object ?? reflected, NameInstance);

                convert = ConvertIfNeeded(reflected, instance);

                parameters = new[] { instance };
            }
            else
            {
                if (@object != null)
                    throw Error.StaticObjectCannotConvertType("@object");

                convert = null;

                parameters = null;
            }

            /* 1. 通过属性方法生成取值的表达式:
             *    var body = ConvertIfNeeded(Expression.Call(convert, property.GetGetMethod(true), (IEnumerable<Expression>)null), result);
             *    DebugView:
             *    .Call $instance.get_PropertyName();
             * 
             * 2. 通过属性直接生成取值的表达式:
             *    DebugView:
             *    $instance.PropertyName;
             * 
             * 不过编译之后, 两者 IL 代码应该是一致的. IL 读取属性都是通过 get_PropertyName();
             * 如果将来实现: 将表达式输出为字符串格式, 那么 "方法2" 更加合理, 因此不再考虑 "方法1".
             * 并且通过测试: 两者编译后的方法运行性能一样;
             */

            var body = ConvertIfNeeded(@return, Expression.Property(convert, member));
            var lambda = Expression.Lambda(body, null, false, parameters);
            return lambda;
        }

        static LambdaExpression InternalPropertySetWithExpression(PropertyInfo member, bool? @static, Type @object, Type @return)
        {
            if (!@static.HasValue)
                @static = member.GetSetMethod(true).IsStatic;

            // Expression.Lambda.Parameters
            IEnumerable<ParameterExpression> parameters;

            // 定义属性参数
            var type = member.PropertyType;
            var value = Expression.Parameter(@return ?? type, NameValue);

            Expression convert;
            if (!@static.Value)
            {
                // 定义实例参数
                var reflected = member.ReflectedType;
                var instance = Expression.Parameter(@object ?? reflected, NameInstance);

                convert = ConvertIfNeeded(reflected, instance);

                parameters = new[] { instance, value };
            }
            else
            {
                // if (@object != null)
                //     throw Error.StaticObjectCannotConvertType("@object");

                convert = null;

                parameters = new[] { value };
            }

            /* 1. 通过属性方法生成赋值的表达式:
             *    var block = Expression.Call(convert, property.GetSetMethod(true), (IEnumerable<Expression>)new[] { ConvertIfNeeded(value, type) });
             *    DebugView:
             *    .Call $instance.set_String($value)
             * 
             * 2. 通过属性直接生成赋值的表达式:
             *    DebugView:
             *    $instance.String = $value
             * 
             * 不过编译之后, 两者 IL 代码应该是一致的. IL 保存属性都是通过 set_PropertyName();
             * 如果将来实现: 将表达式输出为字符串格式, 那么 "方法2" 更加合理, 因此不再考虑 "方法1".
             * 并且通过测试: 两者编译后的方法运行性能一样;
             */

            // 属性赋值
            var assign = Expression.Assign(Expression.Property(convert, member), ConvertIfNeeded(type, value));
            // 显式指定无返回值: Void;
            // 对于自动判断委托类型, 此处不能省略, 否则将会返回 "带有返回值的委托类型": Func<TInstance, TProperty, TProperty>;
            var block = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)null, (IEnumerable<Expression>)new[] { assign });

            var lambda = Expression.Lambda(block, null, false, parameters);
            return lambda;
        }

        #endregion

        #region Property.UserDefined/T0

        public Delegate MakeGet(PropertyInfo member, Type @delegate = null)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.CanRead)
                throw Error.PropertyDoesNotHaveGetter("member");

            LambdaExpression lambda;
            if (@delegate != null)
                lambda = InternalPropertyGetWithExpression(member, @delegate);
            else
                lambda = InternalPropertyGetWithExpression(member, null, null, null);

            return lambda.Compile();
        }

        public Delegate MakeSet(PropertyInfo member, Type @delegate = null)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.CanWrite)
                throw Error.PropertyDoesNotHaveSetter("member");

            LambdaExpression lambda;
            if (@delegate != null)
                lambda = InternalPropertySetWithExpression(member, @delegate);
            else
                lambda = InternalPropertySetWithExpression(member, null, null, null);

            return lambda.Compile();
        }

        static LambdaExpression InternalPropertyGetWithExpression(PropertyInfo member, Type @delegate)
        {
            // var @delegate = typeof(TDelegate);
            if (!@delegate.IsDelegate())
                throw new ArgumentException("不是委托类型", "TDelegate");

            var invoke = @delegate.GetMethod(DelegateInvoke);
            if (invoke.ReturnType == TypeVoid)
                throw new ArgumentException("委托必须有返回值", "TDelegate");

            var targets = invoke.GetParameters();
            var @static = member.GetGetMethod(true).IsStatic;
            var length = @static ? 0 : 1;
            if (length != targets.Length)
                throw new ArgumentException("委托参数数量错误", "TDelegate");

            /* class ClassModel : ICloneable {
             *     Int32 Field;
             *     static Int32 StaticField;
             * }
             * 
             * struct StructModel : ICloneable {
             *     Int32 Field;
             *     static Int32 StaticField;
             * }
             * 
             * static object GetClassStaticMember() {
             *     return (object)ClassModel.StaticField;
             * }
             * static Int32 GetStructStaticMember() {
             *     return StructModel.StaticField;
             * }
             * 
             * Int32 GetClassMember(object instance) {
             *     return ((ClassModel)instance).Field;
             * }
             * object GetStructMember(ref StructModel instance) {
             *     return (object)instance.Field;
             * }
             */
            IEnumerable<ParameterExpression> parameters;
            Expression convert;
            if (!@static)
            {
                var o = targets[0];
                if (o.IsOut)
                    throw new ArgumentException("实例参数不能使用 out 修饰", "TDelegate");

                var instance = Expression.Parameter(o.ParameterType, o.Name);
                convert = ConvertIfNeeded(member.ReflectedType, instance);
                parameters = new[] { instance };
            }
            else
            {
                convert = null;
                parameters = null;
            }
            var body = ConvertIfNeeded(invoke.ReturnType, Expression.Property(convert, member));
            //
            var lambda = Expression.Lambda(@delegate, body, null, false, parameters);
            return lambda;
        }

        static LambdaExpression InternalPropertySetWithExpression(PropertyInfo member, Type @delegate)
        {
            // var @delegate = typeof(TDelegate);
            if (!@delegate.IsDelegate())
                throw new ArgumentException("不是委托类型", "TDelegate");

            var invoke = @delegate.GetMethod(DelegateInvoke);
            if (invoke.ReturnType != TypeVoid)
                throw new ArgumentException("委托必须无返回值", "TDelegate");

            var targets = invoke.GetParameters();
            var @static = member.GetGetMethod(true).IsStatic;
            var length = @static ? 1 : 2;
            if (length != targets.Length)
                throw new ArgumentException("委托参数数量错误", "TDelegate");

            /* class ClassModel : ICloneable {
             *     Int32 Field;
             *     static Int32 StaticField;
             * }
             * 
             * struct StructModel : ICloneable {
             *     Int32 Field;
             *     static Int32 StaticField;
             * }
             * 
             * 
             * static void SetClassStaticMember(Int32 value) {
             *     ClassModel.StaticField = value;
             * }
             * static void SetStructStaticMember(object value) {
             *     StructModel.StaticField = (Int32)value;
             * }
             * 
             * void SetClassMember(ClassModel instance, Int32 value) {
             *     instance.Field = value;
             * }
             * void SetClassMember(ref object instance, object value) {
             *     ((ClassModel)instance).Field = (Int32)value;
             * }
             * void SetStructMember(ref StructModel instance, object value) {
             *     instance.Field = (Int32)value;
             * }
             * 
             * void SetStructMember(ref object instance, object value) {
             *     var variable = (StructModel)instance; // Load
             *     variable.Field = (Int32)value; // Assign
             *     instance = (object)variable; // Save
             * }
             */
            Expression body;
            IEnumerable<ParameterExpression> parameters;
            //
            if (!@static)
            {
                var o = targets[0];
                if (o.IsOut)
                    throw new ArgumentException("实例参数不能使用 out 修饰", "TDelegate");

                var r = targets[1];
                var instance = Expression.Parameter(o.ParameterType, o.Name);
                var value = Expression.Parameter(r.ParameterType, r.Name);

                var reflected = member.ReflectedType;
                if (instance.IsByRef && reflected.IsValueType && reflected != instance.Type) // instance.Type == @object.GetElementType();
                {
                    var variable = Expression.Variable(reflected);
                    var load = Expression.Assign(variable, ConvertIfNeeded(reflected, instance));
                    var assign = Expression.Assign(Expression.Property(variable, member), ConvertIfNeeded(member.PropertyType, value));
                    var save = Expression.Assign(instance, ConvertIfNeeded(instance.Type, variable));
                    body = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)new[] { variable }, (IEnumerable<Expression>)new[] { load, assign, save });
                }
                else
                {
                    body = Expression.Assign(Expression.Property(ConvertIfNeeded(reflected, instance), member), ConvertIfNeeded(member.PropertyType, value));
                    // var block = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)null, (IEnumerable<Expression>)new[] { assign });
                }

                parameters = new[] { instance, value };
            }
            else
            {
                var r = targets[0];
                var value = Expression.Parameter(r.ParameterType, r.Name);

                body = Expression.Assign(Expression.Property(null, member), ConvertIfNeeded(member.PropertyType, value));
                // var block = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)null, (IEnumerable<Expression>)new[] { assign });

                parameters = new[] { value };
            }
            //
            var lambda = Expression.Lambda(@delegate, body, null, false, parameters);
            return lambda;
        }

        #endregion
    }
}

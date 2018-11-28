using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hiz.Reflection
{
    partial class ReflectionWithExpression
    {
        #region Field.Predefined/T1-T2

        /* 字段一定可以读取
         * 
         * |=============================================================================================|
         * |                  | IsStatic | IsInitOnly | IsLiteral | Example                              |
         * |==================|==========|============|===========|======================================|
         * | Instance         |          |            |           | string Field = null;                 |
         * | InstanceReadonly |          | Y          |           | readonly string Field = null;        |
         * | Static           | Y        |            |           | static string Field = null;          |
         * | StaticReadonly   | Y        | Y          |           | static readonly string Field = null; |
         * | Const            | Y        |            | Y         | const string Field = null;           |
         * |======================================================|======================================|
         */

        /// <summary>
        /// 用于静态字段 (支持类型转换)
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public override Func<TField> MakeGetter<TField>(FieldInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");

            /* 示例:
             * class Model {
             *     static string FieldStatic;
             * }
             * 
             * 情况1: FieldGet<string>()
             * string Function() {
             *     return Model.FieldStatic;
             * }
             * 
             * 情况2: FieldGet<object>()
             * object Function() {
             *     return (object)Model.FieldStatic;
             * }
             * 
             * 格式:
             * TField Function() {
             *     return (TField)Static.FieldStatic;
             * }
             */
            var lambda = InternalFieldGetWithExpression(member, null, typeof(TField));
            return (Func<TField>)lambda.Compile();
        }

        /// <summary>
        /// 用于实例字段 (支持类型转换)
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public override Func<TInstance, TField> MakeGetter<TInstance, TField>(FieldInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");

            /* 示例:
             * class Model {
             *     string FieldInstance;
             * }
             * 
             * 情况1: FieldGet<Model, string>()
             * string Function(Model instance) {
             *     return instance.FieldInstance;
             * }
             * 
             * 情况2: FieldGet<Model, object>()
             * object Function(Model instance) {
             *     return (object)instance.FieldInstance;
             * }
             * 
             * 情况3: FieldGet<object, object>()
             * object Function(object instance) {
             *     return (object)((Instance)instance).FieldInstance;
             * }
             * 
             * 格式:
             * TField Function(TInstance instance)
             * {
             *     return (TField)((TInstance)instance).FieldInstance;
             * }
             */
            var lambda = InternalFieldGetWithExpression(member, typeof(TInstance), typeof(TField));
            return (Func<TInstance, TField>)lambda.Compile();
        }

        /// <summary>
        /// 用于静态字段 (支持类型转换)
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public override Action<TField> MakeSetter<TField>(FieldInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");
            if (member.IsInitOnly || member.IsLiteral)
                throw Error.FieldDoesNotHaveSetter("member");

            /* 示例:
             * class Model {
             *     static string FieldStatic;
             * }
             * 
             * 情况1: FieldSet<string>()
             * void Action(string value) {
             *     Model.FieldStatic = value;
             * }
             * 
             * 情况2: FieldSet<object>()
             * void Action(object value) {
             *     Model.FieldStatic = (string)value;
             * }
             * 
             * 格式:
             * void Action(object value)
             * {
             *     Static.FieldStatic = (TField)value;
             * }
             */
            var lambda = InternalFieldSetWithExpression(member, null, typeof(TField));
            return (Action<TField>)lambda.Compile();
        }

        /// <summary>
        /// 用于实例字段 (支持类型转换)
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public override Action<TInstance, TField> MakeSetter<TInstance, TField>(FieldInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");
            if (member.IsInitOnly || member.IsLiteral)
                throw Error.FieldDoesNotHaveSetter("member");

            /* 示例:
             * class Model {
             *     string FieldInstance;
             * }
             * 
             * 情况1: FieldSet<Model, string>()
             * void Action(Model instance, string value) {
             *     instance.FieldInstance = value;
             * }
             * 
             * 情况2: FieldSet<Model, object>()
             * void Action(Model instance, object value) {
             *     instance.FieldInstance = (string)field;
             * }
             * 
             * 情况3: FieldSet<object, object>()
             * void Action(object instance, object value) {
             *     ((Model)instance).FieldInstance = (string)value;
             * }
             * 
             * 格式:
             * void Action(object instance, object value)
             * {
             *     ((TInstance)instance).FieldInstance = (TField)value;
             * }
             */
            var lambda = InternalFieldSetWithExpression(member, typeof(TInstance), typeof(TField));
            return (Action<TInstance, TField>)lambda.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="object">typeof(TInstance); null: 不对 instance 进行类型转换; other: (TInstance)instance;</param>
        /// <param name="return">typeof(TField); null: 不对 instance.Field 进行类型转换; other: (TField)instance.Field</param>
        /// <returns></returns>
        static LambdaExpression InternalFieldGetWithExpression(FieldInfo member, Type @object, Type @return)
        {
            // Expression.Lambda.Parameters
            IEnumerable<ParameterExpression> parameters;

            // 实例转换
            Expression convert;
            if (!member.IsStatic)
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
                    throw Error.StaticObjectCannotConvertType("TInstance");

                convert = null;

                parameters = null;
            }

            // Expression.Lambda.Body
            var body = ConvertIfNeeded(@return, Expression.Field(convert, member));

            var lambda = Expression.Lambda(body, null, false, parameters);
            return lambda;
        }

        static LambdaExpression InternalFieldSetWithExpression(FieldInfo member, Type @object, Type @return)
        {
            // Expression.Lambda.Parameters
            IEnumerable<ParameterExpression> parameters;

            // 定义字段赋值参数
            var type = member.FieldType;
            var value = Expression.Parameter(@return ?? type, NameValue);

            Expression convert;
            if (!member.IsStatic)
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

            // 字段赋值
            var assign = Expression.Assign(Expression.Field(convert, member), ConvertIfNeeded(type, value));

            // 显式指定无返回值: Void;
            // 对于自动判断委托类型, 此处不能省略, 否则将会返回 "带有返回值的委托类型": Func<TInstance, TProperty, TProperty>;
            var block = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)null, (IEnumerable<Expression>)new[] { assign });

            var lambda = Expression.Lambda(block, null, false, parameters);
            return lambda;
        }

        #endregion

        #region Field.UserDefined/T0

        public Delegate MakeGet(FieldInfo member, Type @delegate = null)
        {
            if (member == null)
                throw Error.ArgumentNull("member");

            LambdaExpression lambda;
            if (@delegate != null)
                lambda = InternalFieldGetWithExpression(member, @delegate);
            else
                lambda = InternalFieldGetWithExpression(member, null, null);

            return lambda.Compile();
        }

        public Delegate MakeSet(FieldInfo member, Type @delegate = null)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsInitOnly || member.IsLiteral)
                throw Error.FieldDoesNotHaveSetter("member");

            LambdaExpression lambda;
            if (@delegate != null)
                lambda = InternalFieldSetWithExpression(member, @delegate);
            else
                lambda = InternalFieldSetWithExpression(member, null, null);

            return lambda.Compile();
        }

        static LambdaExpression InternalFieldGetWithExpression(FieldInfo member, Type @delegate)
        {
            // var @delegate = typeof(TDelegate);
            if (!@delegate.IsDelegate())
                throw new ArgumentException("不是委托类型", "TDelegate");

            var invoke = @delegate.GetMethod(DelegateInvoke);
            if (invoke.ReturnType == TypeVoid)
                throw new ArgumentException("委托必须有返回值", "TDelegate");

            var targets = invoke.GetParameters();
            var length = member.IsStatic ? 0 : 1;
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
            if (!member.IsStatic)
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
            var body = ConvertIfNeeded(invoke.ReturnType, Expression.Field(convert, member));
            //
            var lambda = Expression.Lambda(@delegate, body, null, false, parameters);
            return lambda;
        }

        static LambdaExpression InternalFieldSetWithExpression(FieldInfo member, Type @delegate)
        {
            // var @delegate = typeof(TDelegate);
            if (!@delegate.IsDelegate())
                throw new ArgumentException("不是委托类型", "TDelegate");

            var invoke = @delegate.GetMethod(DelegateInvoke);
            if (invoke.ReturnType != TypeVoid)
                throw new ArgumentException("委托必须无返回值", "TDelegate");

            var targets = invoke.GetParameters();
            var length = member.IsStatic ? 1 : 2;
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
            if (!member.IsStatic)
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
                    var assign = Expression.Assign(Expression.Field(variable, member), ConvertIfNeeded(member.FieldType, value));
                    var save = Expression.Assign(instance, ConvertIfNeeded(instance.Type, variable));
                    body = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)new[] { variable }, (IEnumerable<Expression>)new[] { load, assign, save });
                }
                else
                {
                    body = Expression.Assign(Expression.Field(ConvertIfNeeded(reflected, instance), member), ConvertIfNeeded(member.FieldType, value));
                    // var block = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)null, (IEnumerable<Expression>)new[] { assign });
                }

                parameters = new[] { instance, value };
            }
            else
            {
                var r = targets[0];
                var value = Expression.Parameter(r.ParameterType, r.Name);

                body = Expression.Assign(Expression.Field(null, member), ConvertIfNeeded(member.FieldType, value));
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
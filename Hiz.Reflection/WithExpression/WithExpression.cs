using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hiz.Reflection
{
    partial class ReflectionWithExpression : ReflectionServiceBase
    {
        #region Internal

        /// <summary>
        /// 转至指定类型
        /// </summary>
        /// <param name="target">赋值目标类型</param>
        /// <param name="expression"></param>
        /// <returns></returns>
        static Expression ConvertIfNeeded(Type target, Expression expression)
        {
            /* System.Linq.Expressions.Expression
             * static BinaryExpression Assign(Expression left, Expression right) {
             *     Expression.RequiresCanWrite(left);
             *     Expression.RequiresCanRead(right);
             *     ValidateType(left.Type);
             *     ValidateType(right.Type);
             *     if (!AreReferenceAssignable(left.Type, right.Type)) {
             *         throw Error.ExpressionTypeDoesNotMatchAssignment(right.Type, left.Type);
             *     }
             *     return new AssignBinaryExpression(left, right);
             * }
             * 
             * static void ValidateType(Type type) {
             *     if (type.IsGenericTypeDefinition) {
             *         throw Error.TypeIsGeneric(type);
             *     }
             *     if (type.ContainsGenericParameters) {
             *         throw Error.TypeContainsGenericParameters(type);
             *     }
             * }
             * 
             * static bool AreReferenceAssignable(Type target, Type source) {
             *     if (AreEquivalent(target, source))
             *         return true;
             *     if (!target.IsValueType && !source.IsValueType && target.IsAssignableFrom(source))
             *         return true;
             *     return false;
             * }
             * 
             * static bool AreEquivalent(Type type, Type other) {
             *     return type == other || type.IsEquivalentTo(other); // 仅仅针对 COM 对象;
             * }
             * 
             * 
             * Test:
             * var value = Expression.Variable(typeof(object));
             * if (typeof(object).IsAssignableFrom(typeof(Int32))) { // 虽然可以分配实例
             *     // 抛出异常: "System.Int32" 类型的表达式不能用于类型 "System.Object" 的赋值.
             *     Expression.Assign(value, Expression.Constant(0xFF));
             * }
             */
            if (expression == null)
                throw new ArgumentNullException();
            if (target == null)
                throw new ArgumentNullException();

            var source = expression.Type;
            if (source != target) // 忽略 COM 对象
            {
                // 判断两个类型是否可以引用赋值: 派生类型赋给基类 / 引用类型赋给接口;
                if (target.IsValueType || source.IsValueType || !target.IsAssignableFrom(source))
                {
                    // 内部实现: 自动支持用户定义转换
                    return Expression.Convert(expression, target, null);
                }
            }
            return expression;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byrefs"></param>
        /// <param name="call"></param>
        /// <param name="return">结果类型转换; 空值表示不变;</param>
        /// <returns></returns>
        static BlockExpression InternalMakeByRefBlock(IEnumerable<ByRefTuple> byrefs, MethodCallExpression call, Type @return)
        {
            if (call.Type == TypeVoid)
            {
                /* 例子:
                 * class Model {
                 *     void Test(byte a, ref uint b, out long c);
                 * }
                 * 
                 * Universal:
                 * TResult Function(TInstance instance, object[] coalition) {
                 *     Block(uint var1, long var2) {
                 *         var1 = (uint)coalition[1];
                 *         var2 = (long)coalition[2];
                 *         instance.Action((int)coalition[0], ref var1, out var2);
                 *         coalition[1] = (object)var1;
                 *         coalition[2] = (object)var2;
                 *         return default(TResult);
                 *     }
                 * }
                 * 
                 * ByRefWrapper:
                 * void Action(Model instance, byte a, ByRef<uint> b, ByRef<long> c) {
                 *     Block<System.Void>(uint var1, long var2) {
                 *         var1 = b.Value;
                 *         var2 = c.Value;
                 *         instance.Test(a, ref var1, out var2);
                 *         b.Value = var1;
                 *         c.Value = var2;
                 *     }
                 * }
                 */
                var variables = byrefs.Select(i => i.Variable);
                var expressions = Enumerable.Empty<Expression>()
                   .Concat(byrefs.Where(i => i.Load != null).Select(i => i.Load))
                   .Concat(new[] { call })
                   .Concat(byrefs.Select(i => i.Save));

                if (@return != null && @return != TypeVoid)
                {
                    /* 追加返回值的类型:
                     * (TResult)Block {
                     *     Action(...);
                     *     return default(TResult);
                     * }
                     */
                    return Expression.Block(@return, variables, expressions.Concat(new[] { Expression.Default(@return) }));
                }
                else
                {
                    /* 追加返回值的类型:
                     * void Block {
                     *     Action(...);
                     * }
                     */
                    return Expression.Block(TypeVoid, variables, expressions);
                }
            }
            else
            {
                /* 例子:
                 * class Model {
                 *     int Test(byte a, ref uint b, out long c);
                 * }
                 * 
                 * Universal:
                 * TResult Function(TInstance instance, object[] coalition) {
                 *     Block(uint var1, long var2, TResult result) {
                 *         var1 = (uint)coalition[1];
                 *         var2 = (long)coalition[2];
                 *         result = (TResult)((Model)instance).Test((byte)coalition[0], ref var1, out var2);
                 *         coalition[1] = (object)var1;
                 *         coalition[2] = (object)var2;
                 *         return result;
                 *     }
                 * }
                 * 
                 * ByRefWrapper:
                 * void Function(Model instance, byte a, ByRef<uint> b, ByRef<long> c) {
                 *     Block(uint var1, long var2) {
                 *         var1 = b.Value;
                 *         var2 = c.Value;
                 *         instance.Test(a, ref var1, out var2); // 只调用方法, 不返回结果;
                 *         b.Value = var1;
                 *         c.Value = var2;
                 *     }
                 * }
                 */
                if (@return != null && @return == TypeVoid)
                {
                    var variables = byrefs.Select(i => i.Variable);
                    var expressions = Enumerable.Empty<Expression>()
                       .Concat(byrefs.Where(i => i.Load != null).Select(i => i.Load))
                       .Concat(new[] { call })
                       .Concat(byrefs.Select(i => i.Save));
                    return Expression.Block(TypeVoid, variables, expressions);
                }
                else
                {
                    var result = Expression.Variable(@return ?? call.Type);
                    var variables = byrefs.Select(i => i.Variable).Concat(new[] { result });
                    var expressions = Enumerable.Empty<Expression>()
                       .Concat(byrefs.Where(i => i.Load != null).Select(i => i.Load))
                       .Concat(new[] { Expression.Assign(result, ConvertIfNeeded(result.Type, call)) })
                       .Concat(byrefs.Select(i => i.Save))
                       .Concat(new[] { result });
                    return Expression.Block(result.Type, variables, expressions);
                }
            }
        }
        class ByRefTuple
        {
            // 定义变量: T variable;
            public readonly ParameterExpression Variable;
            // 读取参数 => 存入变量: variable = byref;
            public readonly BinaryExpression Load; // LoadParameter => SaveVariable;
            // 取出变量 => 写回参数: byref = variable;
            public readonly BinaryExpression Save; // SaveParameter <= LoadVariable;

            /* out:
             * Lambda(out object result) {
             *     int variable;
             *     Method(out variable);
             *     result = (object)variable; // Save
             * }
             */
            public ByRefTuple(ParameterExpression variable, BinaryExpression save)
            {
                Variable = variable;
                // Load = null; // out 无需读取外部参值;
                Save = save;
            }

            /* ref:
             * Lambda(ref object result) {
             *     int variable;
             *     variable = (int)result; // Load
             *     Method(ref variable);
             *     result = (object)variable; // Save
             * }
             */
            public ByRefTuple(ParameterExpression variable, BinaryExpression save, BinaryExpression load)
            {
                Variable = variable;
                Load = load;
                Save = save;
            }
        }

        #endregion

        #region ByRefWrapper 暂时取消 等待改进
        // static readonly Type TypeWrapper = typeof(ByRef<>); // 包装参数: ref/out

        // static bool IsByRefWrapper(Type type)
        // {
        //     return type.IsGenericType && type.GetGenericTypeDefinition() == TypeWrapper;
        // }

        // // Key: Type;
        // // Value: typeof(ByRefWrapper<Key>).GetProperty("Value");
        // static readonly IDictionary<Type, PropertyInfo> _WrapperValues = new Dictionary<Type, PropertyInfo>();
        // static readonly object _WrapperValuesLock = new object();
        // static PropertyInfo GetWrapperValueMember(Type value) // Type.IsByRef = False
        // {
        //     PropertyInfo property;
        //     lock (_WrapperValuesLock)
        //     {
        //         if (!_WrapperValues.TryGetValue(value, out property))
        //         {
        //             property = TypeWrapper.MakeGenericType(value).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
        //             _WrapperValues.Add(value, property);
        //         }
        //     }
        //     return property;
        // }

        // // static PropertyInfo GetWrapperValueMemberWithWrapper(Type wrapper)
        // // {
        // //     PropertyInfo property;
        // //     lock (_WrapperValuesLock)
        // //     {
        // //         var value = wrapper.GetGenericArguments()[0];
        // //         if (!_WrapperValues.TryGetValue(value, out property))
        // //         {
        // //             property = wrapper.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
        // //             _WrapperValues.Add(value, property);
        // //         }
        // //     }
        // //     return property;
        // // }
        #endregion
    }
}

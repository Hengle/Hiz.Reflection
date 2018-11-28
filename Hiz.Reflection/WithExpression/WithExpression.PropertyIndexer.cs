using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hiz.Reflection
{
    partial class ReflectionWithExpression
    {
        #region PropertyIndexer.Universal/T2

        public override Func<TInstance, object[], TProperty> MakeGetterOfIndexer<TInstance, TProperty>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanRead)
                throw Error.PropertyDoesNotHaveGetter("indexer");
            if (indexer.GetGetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = InternalIndexerGetWithExpression(indexer, typeof(TInstance), typeof(TProperty));
            return (Func<TInstance, object[], TProperty>)lambda.Compile();
        }
        public override Action<TInstance, object[], TProperty> MakeSetterOfIndexer<TInstance, TProperty>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanWrite)
                throw Error.PropertyDoesNotHaveSetter("indexer");
            if (indexer.GetSetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            /* 说明:
             * 
             * class Model {
             *     public string this[int x, int y] { get; set; }
             * }
             * 
             * 最优情况: Set<Model, string>()
             * void Action(Model instance, object[] indices, string property) {
             *     instance[(int)indices[0], (int)indices[1]] = property;
             * }
             * 
             * 适中情况: Set<Model, object>()
             * void Action(Instance instance, object[] indices, object property) {
             *     instance[(int)indices[0], (int)indices[1]] = (string)property;
             * }
             * 
             * 最长情况: Set<object, object>()
             * void Action(object instance, object[] indices, object property) {
             *     ((Model)instance)[(int)indices[0], (int)indices[1]] = (string)property;
             * }
             * 
             * 最长格式:
             * void Action(object instance, object[] indices, object value)
             * {
             *     ((TInstance)instance)[(TIndex0)indices[0], (TIndex1)indices[1], ...] = (TProperty)property;
             * }
             */
            var lambda = InternalIndexerSetWithExpression(indexer, typeof(TInstance), typeof(TProperty));
            return (Action<TInstance, object[], TProperty>)lambda.Compile();
        }

        static LambdaExpression InternalIndexerGetWithExpression(PropertyInfo indexer, Type @object, Type @return)
        {
            var indexes = indexer.GetIndexParameters();
            var length = indexes.Length;
            if (length == 0)
                throw Error.IsNotIndexerProperty("indexer");

            // 定义实例参数
            var reflected = indexer.ReflectedType;
            var instance = Expression.Parameter(@object ?? reflected, NameInstance);
            // 定义索引组合参数
            var coalition = Expression.Parameter(TypeObjectArray, NameIndexes);
            // 定义属性索引参数数组
            var arguments = new Expression[length];
            for (var i = 0; i < length; i++)
            {
                arguments[i] = ConvertIfNeeded(indexes[i].ParameterType, Expression.ArrayAccess(coalition, Expression.Constant(i)));
            }
            // 访问属性
            var property = Expression.Property(ConvertIfNeeded(reflected, instance), indexer, arguments);
            //
            // 属性类型转换
            var convert = ConvertIfNeeded(@return, property);
            var lambda = Expression.Lambda(convert, null, false, (IEnumerable<ParameterExpression>)new[] { instance, coalition });
            return lambda;
        }
        static LambdaExpression InternalIndexerSetWithExpression(PropertyInfo indexer, Type @object, Type @return)
        {
            var indexes = indexer.GetIndexParameters();
            var length = indexes.Length;
            if (length == 0)
                throw Error.IsNotIndexerProperty("indexer");

            // 定义实例参数
            var reflected = indexer.ReflectedType;
            var instance = Expression.Parameter(@object ?? reflected, NameInstance);
            // 定义索引组合参数
            var coalition = Expression.Parameter(TypeObjectArray, NameIndexes);
            // 定义属性索引参数数组
            var arguments = new Expression[length];
            for (var i = 0; i < length; i++)
            {
                arguments[i] = ConvertIfNeeded(indexes[i].ParameterType, Expression.ArrayAccess(coalition, Expression.Constant(i)));
            }
            // 访问属性
            var property = Expression.Property(ConvertIfNeeded(reflected, instance), indexer, arguments);
            //
            // 定义属性参数
            var type = indexer.PropertyType;
            var value = Expression.Parameter(@return ?? type, NameValue);
            // 进行属性赋值
            var assign = Expression.Assign(property, ConvertIfNeeded(type, value));
            // 显式指定无返回值: Void
            var block = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)null, (IEnumerable<Expression>)new[] { assign });
            var lambda = Expression.Lambda(block, null, false, (IEnumerable<ParameterExpression>)new[] { instance, coalition, value });
            return lambda;
        }

        #endregion

        #region PropertyIndexer.UserDefined/T0-T1

        /// <summary>
        /// 需要手动转换返回类型; TProperty Func(TInstance, TIndex0, TIndex1, ..., TIndex13);
        /// 最大支持索引数量: 14; 预设 Func 最多参数16 - 实例 = 15; 为跟 SetIndexer 统一, 降为 14;
        /// 索引数量 14 以上请使用自定义委托类型.
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public Delegate MakeGetIndexer(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanRead)
                throw Error.PropertyDoesNotHaveGetter("indexer");
            if (indexer.GetGetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = InternalIndexerGetWithExpression(indexer, null);
            return lambda.Compile();
        }

        /// <summary>
        /// 需要手动转换返回类型; void Action(TInstance, TIndex0, TIndex1, ..., TIndex13, TProperty);
        /// 最大支持索引数量: 14; 预设 Action 最多参数16 - 实例 - 属性值参 = 14;
        /// 索引数量 14 以上请使用自定义委托类型.
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public Delegate MakeSetIndexer(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanWrite)
                throw Error.PropertyDoesNotHaveSetter("indexer");
            if (indexer.GetSetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = InternalIndexerSetWithExpression(indexer, null);
            return lambda.Compile();
        }

        public override TDelegate MakeGetterOfIndexer<TDelegate>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanRead)
                throw Error.PropertyDoesNotHaveGetter("indexer");
            if (indexer.GetGetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = (Expression<TDelegate>)InternalIndexerGetWithExpression(indexer, typeof(TDelegate));
            return lambda.Compile();
        }
        public override TDelegate MakeSetterOfIndexer<TDelegate>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanWrite)
                throw Error.PropertyDoesNotHaveSetter("indexer");
            if (indexer.GetSetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = (Expression<TDelegate>)InternalIndexerSetWithExpression(indexer, typeof(TDelegate));
            return lambda.Compile();
        }

        static LambdaExpression InternalIndexerGetWithExpression(PropertyInfo member, Type @delegate)
        {
            // var @delegate = typeof(TDelegate);
            if (!@delegate.IsDelegate())
                throw new ArgumentException("不是委托类型", "TDelegate");

            var sources = member.GetIndexParameters();
            var length = sources.Length;
            if (length == 0)
                throw Error.IsNotIndexerProperty("indexer");

            LambdaExpression lambda;
            if (@delegate != null)
            {
                if (!@delegate.IsDelegate())
                    throw new ArgumentException("不是委托类型", "TDelegate");

                var invoke = @delegate.GetMethod(DelegateInvoke);
                if (invoke.ReturnType == TypeVoid)
                    throw new ArgumentException("委托必须有返回值", "TDelegate");

                var targets = invoke.GetParameters();
                if (targets.Length != length + 1)
                    throw new ArgumentException("委托参数数量不对", "TDelegate");

                var parameters = targets.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                var arguments = sources.Select((p, i) => ConvertIfNeeded(p.ParameterType, parameters[i + 1])).ToArray();
                var property = Expression.Property(ConvertIfNeeded(member.ReflectedType, parameters[0]), member, (IEnumerable<Expression>)arguments);
                var body = ConvertIfNeeded(invoke.ReturnType, property);

                /* TProperty Get(object instance, object index0, object index1, ...) {
                 *     return (TProperty)((TInstance)instance)[(TIndex0)index0, (TIndex0)index1, ...];
                 * }
                 */
                lambda = Expression.Lambda(@delegate, body, null, false, (IEnumerable<ParameterExpression>)parameters);
            }
            else
            {
                if (length > MaximumIndexes)
                    throw new NotSupportedException();

                var instance = Expression.Parameter(member.ReflectedType, NameInstance);
                var arguments = sources.Select(i => Expression.Parameter(i.ParameterType, i.Name)).ToArray();
                var property = Expression.Property(instance, member, (IEnumerable<Expression>)arguments);

                var parameters = new[] { instance }.Concat(arguments).ToArray();

                /* TProperty Get(TInstance instance, TIndex0 index0, TIndex0 index1, ...) {
                 *     return instance[index0, index1, ...];
                 * }
                 */
                lambda = Expression.Lambda(property, null, false, (IEnumerable<ParameterExpression>)parameters);
            }

            return lambda;
        }
        static LambdaExpression InternalIndexerSetWithExpression(PropertyInfo member, Type @delegate)
        {
            // var @delegate = typeof(TDelegate);
            if (!@delegate.IsDelegate())
                throw new ArgumentException("不是委托类型", "TDelegate");

            var sources = member.GetIndexParameters();
            var length = sources.Length;
            if (length == 0)
                throw Error.IsNotIndexerProperty("indexer");

            LambdaExpression lambda;
            if (@delegate != null)
            {
                if (!@delegate.IsDelegate())
                    throw new ArgumentException("不是委托类型", "TDelegate");

                var invoke = @delegate.GetMethod(DelegateInvoke);
                if (invoke.ReturnType != TypeVoid)
                    throw new ArgumentException("委托必须无返回值", "TDelegate");

                var targets = invoke.GetParameters();
                if (targets.Length != length + 2) // + FirstParameter:TInstance + LastParameter:TProperty;
                    throw new ArgumentException("委托参数数量不对", "TDelegate");

                Expression body;
                var parameters = targets.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                var instance = parameters[0]; // FirstParameter: TInstance;
                var arguments = sources.Select((p, i) => ConvertIfNeeded(p.ParameterType, parameters[i + 1])).ToArray();
                var reflected = member.ReflectedType;
                if (instance.IsByRef && reflected.IsValueType && reflected != instance.Type) // instance.Type == array[0].GetElementType();
                {
                    /* void Set(ref object instance, object index0, object index1, ..., object value) {
                     *     TReflected variable;
                     *     variable = (TReflected)instance;
                     *     variable[(TIndex0)index0, (TIndex0)index1, ...] = (TProperty)value;
                     *     instance = (TArgument)variable;
                     * }
                     */
                    var variable = Expression.Variable(reflected);
                    var load = Expression.Assign(variable, ConvertIfNeeded(reflected, instance));
                    var assign = Expression.Assign(Expression.Property(variable, member, (IEnumerable<Expression>)arguments), ConvertIfNeeded(member.PropertyType, parameters.Last()));
                    var save = Expression.Assign(instance, ConvertIfNeeded(instance.Type, variable));
                    body = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)new[] { variable }, (IEnumerable<Expression>)new[] { load, assign, save });
                }
                else
                {
                    /* void Set(object instance, object index0, object index1, ..., object value) {
                     *     ((TInstance)instance)[(TIndex0)index0, (TIndex0)index1, ...] = (TProperty)value;
                     * }
                     */
                    var property = Expression.Property(ConvertIfNeeded(reflected, instance), member, (IEnumerable<Expression>)arguments);
                    body = Expression.Assign(property, ConvertIfNeeded(member.PropertyType, parameters.Last()));
                }
                lambda = Expression.Lambda(@delegate, body, null, false, (IEnumerable<ParameterExpression>)parameters);
            }
            else
            {
                if (length > MaximumIndexes)
                    throw new NotSupportedException("索引数量过多, 请改用自定义委托类型.");

                var instance = Expression.Parameter(member.ReflectedType, NameInstance);
                var arguments = sources.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                var property = Expression.Property(instance, member, (IEnumerable<Expression>)arguments);

                var value = Expression.Parameter(member.PropertyType, NameValue);
                var assign = Expression.Assign(property, value);
                // 显式指定无返回值: Void
                var block = Expression.Block(TypeVoid, (IEnumerable<ParameterExpression>)null, (IEnumerable<Expression>)new[] { assign });

                var parameters = new[] { instance }.Concat(arguments).Concat(new[] { value }).ToArray();

                /* void Set(TInstance instance, TIndex0 index0, TIndex0 index1, ..., TProperty value) {
                 *     instance[index0, index1, ...] = value;
                 * }
                 */
                lambda = Expression.Lambda(block, null, false, (IEnumerable<ParameterExpression>)parameters);
            }

            return lambda;
        }

        #endregion

        #region PropertyIndexer.Predefined/T3-T5

        public Func<TInstance, T1, TProperty> MakeGetIndexer<TInstance, T1, TProperty>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanRead)
                throw Error.PropertyDoesNotHaveGetter("indexer");
            if (indexer.GetGetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = (Expression<Func<TInstance, T1, TProperty>>)InternalIndexerGetWithExpression(indexer, typeof(Func<TInstance, T1, TProperty>));
            return lambda.Compile();
        }
        public Func<TInstance, T1, T2, TProperty> MakeGetIndexer<TInstance, T1, T2, TProperty>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanRead)
                throw Error.PropertyDoesNotHaveGetter("indexer");
            if (indexer.GetGetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = (Expression<Func<TInstance, T1, T2, TProperty>>)InternalIndexerGetWithExpression(indexer, typeof(Func<TInstance, T1, T2, TProperty>));
            return lambda.Compile();
        }
        public Func<TInstance, T1, T2, T3, TProperty> MakeGetIndexer<TInstance, T1, T2, T3, TProperty>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanRead)
                throw Error.PropertyDoesNotHaveGetter("indexer");
            if (indexer.GetGetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = (Expression<Func<TInstance, T1, T2, T3, TProperty>>)InternalIndexerGetWithExpression(indexer, typeof(Func<TInstance, T1, T2, T3, TProperty>));
            return lambda.Compile();
        }

        public Action<TInstance, T1, TProperty> MakeSetIndexer<TInstance, T1, TProperty>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanWrite)
                throw Error.PropertyDoesNotHaveSetter("indexer");
            if (indexer.GetSetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = (Expression<Action<TInstance, T1, TProperty>>)InternalIndexerSetWithExpression(indexer, typeof(Action<TInstance, T1, TProperty>));
            return lambda.Compile();
        }
        public Action<TInstance, T1, T2, TProperty> MakeSetIndexer<TInstance, T1, T2, TProperty>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanWrite)
                throw Error.PropertyDoesNotHaveSetter("indexer");
            if (indexer.GetSetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = (Expression<Action<TInstance, T1, T2, TProperty>>)InternalIndexerSetWithExpression(indexer, typeof(Action<TInstance, T1, T2, TProperty>));
            return lambda.Compile();
        }
        public Action<TInstance, T1, T2, T3, TProperty> MakeSetIndexer<TInstance, T1, T2, T3, TProperty>(PropertyInfo indexer)
        {
            if (indexer == null)
                throw Error.ArgumentNull("indexer");
            if (!indexer.CanWrite)
                throw Error.PropertyDoesNotHaveSetter("indexer");
            if (indexer.GetSetMethod(true).IsStatic)
                throw Error.OnlyInstanceMember("indexer");

            var lambda = (Expression<Action<TInstance, T1, T2, T3, TProperty>>)InternalIndexerSetWithExpression(indexer, typeof(Action<TInstance, T1, T2, T3, TProperty>));
            return lambda.Compile();
        }

        // public RefAction<TInstance, T1, TProperty> MakeSetIndexerRef<TInstance, T1, TProperty>(PropertyInfo indexer) where TInstance : struct
        // {
        //     return null;
        // }
        // public RefAction<TInstance, T1, T2, TProperty> MakeSetIndexerRef<TInstance, T1, T2, TProperty>(PropertyInfo indexer) where TInstance : struct
        // {
        //     return null;
        // }
        // public RefAction<TInstance, T1, T2, T3, TProperty> MakeSetIndexerRef<TInstance, T1, T2, T3, TProperty>(PropertyInfo indexer) where TInstance : struct
        // {
        //     return null;
        // }

        #endregion
    }
}

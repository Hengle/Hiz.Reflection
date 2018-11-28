using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hiz.Reflection
{
    /* v1.2: 2017-03-28: 支持 自定义的委托类型; 实例参数支持 ref 修饰;
     * v1.1: 2012-07-19: 优化 ParameterType = Object 处理方式. 增加 Property/Field 相关方法.
     * v1.0: 2012-02-16: 实现 MakeInvokeUniversal 支持 ref 参数;
     */
    public static class FastHelper
    {
        static readonly ReflectionWithExpression _Service = new ReflectionWithExpression();

        #region Field.Predefined/T1-T2

        /// <summary>
        /// 用于静态字段 (支持类型转换)
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Func<TField> MakeGetter<TField>(this FieldInfo member)
        {
            return _Service.MakeGetter<TField>(member);
        }

        /// <summary>
        /// 用于实例字段 (支持类型转换)
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Func<TInstance, TField> MakeGetter<TInstance, TField>(this FieldInfo member)
        {
            return _Service.MakeGetter<TInstance, TField>(member);
        }

        /// <summary>
        /// 用于静态字段 (支持类型转换)
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Action<TField> MakeSetter<TField>(this FieldInfo member)
        {
            return _Service.MakeSetter<TField>(member);
        }

        /// <summary>
        /// 用于实例字段 (支持类型转换)
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Action<TInstance, TField> MakeSetter<TInstance, TField>(this FieldInfo member)
        {
            return _Service.MakeSetter<TInstance, TField>(member);
        }

        #endregion

        #region Field.UserDefined/T0

        public static Delegate MakeGetter(this FieldInfo member, Type @delegate = null)
        {
            return _Service.MakeGet(member, @delegate);
        }

        public static Delegate MakeSetter(this FieldInfo member, Type @delegate = null)
        {
            return _Service.MakeSet(member, @delegate);
        }

        #endregion

        #region Property.Predefined/T1-T2

        /// <summary>
        /// 用于静态属性 (支持 实例/属性 类型转换)
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Func<TProperty> MakeGetter<TProperty>(this PropertyInfo member)
        {
            return _Service.MakeGetter<TProperty>(member);
        }

        /// <summary>
        /// 用于实例属性 (支持 实例/属性 类型转换)
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Func<TInstance, TProperty> MakeGetter<TInstance, TProperty>(this PropertyInfo member)
        {
            return _Service.MakeGetter<TInstance, TProperty>(member);
        }

        /// <summary>
        /// 用于静态属性 (支持 实例/属性 类型转换)
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Action<TProperty> MakeSetter<TProperty>(this PropertyInfo member)
        {
            return _Service.MakeSetter<TProperty>(member);
        }

        /// <summary>
        /// 用于实例属性 (支持 实例/属性 类型转换)
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Action<TInstance, TProperty> MakeSetter<TInstance, TProperty>(this PropertyInfo member)
        {
            return _Service.MakeSetter<TInstance, TProperty>(member);
        }

        #endregion

        #region Property.UserDefined/T0

        public static Delegate MakeGetter(this PropertyInfo member, Type @delegate = null)
        {
            return _Service.MakeGet(member, @delegate);
        }

        public static Delegate MakeSetter(this PropertyInfo member, Type @delegate = null)
        {
            return _Service.MakeSet(member, @delegate);
        }

        #endregion

        #region PropertyIndexer.Universal/T2

        public static Func<TInstance, object[], TProperty> MakeGetterOfIndexer<TInstance, TProperty>(this PropertyInfo indexer)
        {
            return _Service.MakeGetterOfIndexer<TInstance, TProperty>(indexer);
        }
        public static Action<TInstance, object[], TProperty> MakeSetterOfIndexer<TInstance, TProperty>(this PropertyInfo indexer)
        {
            return _Service.MakeSetterOfIndexer<TInstance, TProperty>(indexer);
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
        public static Delegate MakeGetterOfIndexer(this PropertyInfo indexer)
        {
            return _Service.MakeGetIndexer(indexer);
        }

        /// <summary>
        /// 需要手动转换返回类型; void Action(TInstance, TIndex0, TIndex1, ..., TIndex13, TProperty);
        /// 最大支持索引数量: 14; 预设 Action 最多参数16 - 实例 - 属性值参 = 14;
        /// 索引数量 14 以上请使用自定义委托类型.
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static Delegate MakeSetterOfIndexer(this PropertyInfo indexer)
        {
            return _Service.MakeSetIndexer(indexer);
        }

        public static TDelegate MakeGetterOfIndexer<TDelegate>(this PropertyInfo indexer)
        {
            return _Service.MakeGetterOfIndexer<TDelegate>(indexer);
        }
        public static TDelegate MakeSetterOfIndexer<TDelegate>(this PropertyInfo indexer)
        {
            return _Service.MakeSetterOfIndexer<TDelegate>(indexer);
        }

        #endregion

        #region PropertyIndexer.Predefined/T3-T5

        public static Func<TInstance, T1, TProperty> MakeGetterOfIndexer<TInstance, T1, TProperty>(this PropertyInfo indexer)
        {
            return _Service.MakeGetIndexer<TInstance, T1, TProperty>(indexer);
        }
        public static Func<TInstance, T1, T2, TProperty> MakeGetterOfIndexer<TInstance, T1, T2, TProperty>(this PropertyInfo indexer)
        {
            return _Service.MakeGetIndexer<TInstance, T1, T2, TProperty>(indexer);
        }
        public static Func<TInstance, T1, T2, T3, TProperty> MakeGetterOfIndexer<TInstance, T1, T2, T3, TProperty>(this PropertyInfo indexer)
        {
            return _Service.MakeGetIndexer<TInstance, T1, T2, T3, TProperty>(indexer);
        }

        public static Action<TInstance, T1, TProperty> MakeSetterOfIndexer<TInstance, T1, TProperty>(this PropertyInfo indexer)
        {
            return _Service.MakeSetIndexer<TInstance, T1, TProperty>(indexer);
        }
        public static Action<TInstance, T1, T2, TProperty> MakeSetterOfIndexer<TInstance, T1, T2, TProperty>(this PropertyInfo indexer)
        {
            return _Service.MakeSetIndexer<TInstance, T1, T2, TProperty>(indexer);
        }
        public static Action<TInstance, T1, T2, T3, TProperty> MakeSetterOfIndexer<TInstance, T1, T2, T3, TProperty>(this PropertyInfo indexer)
        {
            return _Service.MakeSetIndexer<TInstance, T1, T2, T3, TProperty>(indexer);
        }

        // public static RefAction<TInstance, T1, TProperty> MakeSetterOfIndexerByRef<TInstance, T1, TProperty>(this PropertyInfo indexer) where TInstance : struct
        // {
        //     return null;
        // }
        // public static RefAction<TInstance, T1, T2, TProperty> MakeSetterOfIndexerByRef<TInstance, T1, T2, TProperty>(this PropertyInfo indexer) where TInstance : struct
        // {
        //     return null;
        // }
        // public static RefAction<TInstance, T1, T2, T3, TProperty> MakeSetterOfIndexerByRef<TInstance, T1, T2, T3, TProperty>(this PropertyInfo indexer) where TInstance : struct
        // {
        //     return null;
        // }

        #endregion

        #region Method.UserDefined/T1

        public static TDelegate MakeInvoker<TDelegate>(this MethodInfo member)
        {
            return _Service.MakeInvoker<TDelegate>(member);
        }

        #endregion

        #region Method.Universal/T2

        /// <summary>
        /// 转成通用方法: object MethodBase.Invoke(object obj, object[] parameters);
        /// TResult Func(TInstance instance, object[] array) {
        ///     string arg1 = (string)array[1];
        ///     double arg2;
        ///     TResult result = (TResult)instance.Method((int)array[0], ref arg1, out arg2);
        ///     array[1] = arg1;
        ///     array[2] = arg2;
        ///     return result;
        /// }
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Func<TInstance, object[], TResult> MakeInvokerByUniversal<TInstance, TResult>(this MethodInfo member)
        {
            return _Service.MakeInvokerByUniversal<TInstance, TResult>(member);
        }

        #endregion

        #region Method.Predefined/T0-T5

        public static Action<TInstance> MakeAction<TInstance>(this MethodInfo member)
        {
            return _Service.MakeAction<TInstance>(member);
        }
        public static Action<TInstance, T1> MakeAction<TInstance, T1>(this MethodInfo member)
        {
            return _Service.MakeAction<TInstance, T1>(member);
        }
        public static Action<TInstance, T1, T2> MakeAction<TInstance, T1, T2>(this MethodInfo member)
        {
            return _Service.MakeAction<TInstance, T1, T2>(member);
        }
        public static Action<TInstance, T1, T2, T3> MakeAction<TInstance, T1, T2, T3>(this MethodInfo member)
        {
            return _Service.MakeAction<TInstance, T1, T2, T3>(member);
        }

        public static Func<TInstance, TResult> MakeFunction<TInstance, TResult>(this MethodInfo member)
        {
            return _Service.MakeFunction<TInstance, TResult>(member);
        }
        public static Func<TInstance, T1, TResult> MakeFunction<TInstance, T1, TResult>(this MethodInfo member)
        {
            return _Service.MakeFunction<TInstance, T1, TResult>(member);
        }
        public static Func<TInstance, T1, T2, TResult> MakeFunction<TInstance, T1, T2, TResult>(this MethodInfo member)
        {
            return _Service.MakeFunction<TInstance, T1, T2, TResult>(member);
        }
        public static Func<TInstance, T1, T2, T3, TResult> MakeFunction<TInstance, T1, T2, T3, TResult>(this MethodInfo member)
        {
            return _Service.MakeFunction<TInstance, T1, T2, T3, TResult>(member);
        }

        public static Action MakeActionStatic(this MethodInfo member)
        {
            return _Service.MakeActionStatic(member);
        }
        public static Action<T1> MakeActionStatic<T1>(this MethodInfo member)
        {
            return _Service.MakeActionStatic<T1>(member);
        }
        public static Action<T1, T2> MakeActionStatic<T1, T2>(this MethodInfo member)
        {
            return _Service.MakeActionStatic<T1, T2>(member);
        }
        public static Action<T1, T2, T3> MakeActionStatic<T1, T2, T3>(this MethodInfo member)
        {
            return _Service.MakeActionStatic<T1, T2, T3>(member);
        }

        public static Func<TResult> MakeFunctionStatic<TResult>(this MethodInfo member)
        {
            return _Service.MakeFunctionStatic<TResult>(member);

        }
        public static Func<T1, TResult> MakeFunctionStatic<T1, TResult>(this MethodInfo member)
        {
            return _Service.MakeFunctionStatic<T1, TResult>(member);

        }
        public static Func<T1, T2, TResult> MakeFunctionStatic<T1, T2, TResult>(this MethodInfo member)
        {
            return _Service.MakeFunctionStatic<T1, T2, TResult>(member);

        }
        public static Func<T1, T2, T3, TResult> MakeFunctionStatic<T1, T2, T3, TResult>(this MethodInfo member)
        {
            return _Service.MakeFunctionStatic<T1, T2, T3, TResult>(member);

        }

        #endregion
    }
}
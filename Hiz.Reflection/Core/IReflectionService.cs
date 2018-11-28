using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hiz.Reflection
{
    interface IReflectionService
    {
        #region Field

        // 静态
        Func<TField> MakeGetter<TField>(FieldInfo member);
        Action<TField> MakeSetter<TField>(FieldInfo member);
        // 实例
        Func<TInstance, TField> MakeGetter<TInstance, TField>(FieldInfo member);
        Action<TInstance, TField> MakeSetter<TInstance, TField>(FieldInfo member);

        #endregion

        #region Property

        // 静态
        Func<TProperty> MakeGetter<TProperty>(PropertyInfo member);
        Action<TProperty> MakeSetter<TProperty>(PropertyInfo member);
        // 实例
        Func<TInstance, TProperty> MakeGetter<TInstance, TProperty>(PropertyInfo member);
        Action<TInstance, TProperty> MakeSetter<TInstance, TProperty>(PropertyInfo member);

        #endregion

        #region PropertyIndexer

        // 定义委托: 支持 ref 修饰实例参数;
        TDelegate MakeGetterOfIndexer<TDelegate>(PropertyInfo indexer);
        TDelegate MakeSetterOfIndexer<TDelegate>(PropertyInfo indexer);

        // 通用方法
        Func<TInstance, object[], TProperty> MakeGetterOfIndexer<TInstance, TProperty>(PropertyInfo indexer);
        Action<TInstance, object[], TProperty> MakeSetterOfIndexer<TInstance, TProperty>(PropertyInfo indexer);

        #endregion

        #region Method

        // 定义委托: 支持 ref 修饰实例参数;
        TDelegate MakeInvoker<TDelegate>(MethodInfo member);

        // 通用方法
        Func<TInstance, object[], TResult> MakeInvokerByUniversal<TInstance, TResult>(MethodInfo member);

        #endregion

        #region Constructor

        /* TInstance New(T1 arg1, T2 arg2, ...) {
         *     return new Model(arg1, arg2, ...);
         * }
         */
        TDelegate MakeConstructor<TDelegate>(ConstructorInfo constructor);

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hiz.Reflection
{
    public abstract class ReflectionServiceBase : IReflectionService
    {
        #region Constants

        //TODO: 检查表达式的参数命名

        // ParameterName // LambdaExpression: 参数名称允许重复, 不会自动修正, 但是编译之后运行正常;
        protected const string NameInstance = "instance";
        protected const string NameValue = "value";
        protected const string NameIndexes = "indexes";
        protected const string DelegateInvoke = "Invoke";

        // static readonly Type TypeDelegate = typeof(Delegate);
        protected static readonly Type TypeVoid = typeof(void);
        protected static readonly Type TypeObject = typeof(object);
        protected static readonly Type TypeObjectArray = typeof(object[]);

        // 预设方法最大参数数量 (不算返回结果类型)
        protected const int MaximumArity = 16;
        protected const int MaximumArityRef = 5;
        protected static readonly Type[] TypeActions = new[] {
            /* 00 */typeof(Action), // 零个参数; 无返回值;
            /* 01 */typeof(Action<>),
            /* 02 */typeof(Action<,>),
            /* 03 */typeof(Action<,,>),
            /* 04 */typeof(Action<,,,>),
            /* 05 */typeof(Action<,,,,>),
            /* 06 */typeof(Action<,,,,,>),
            /* 07 */typeof(Action<,,,,,,>),
            /* 08 */typeof(Action<,,,,,,,>),
            /* 09 */typeof(Action<,,,,,,,,>),
            /* 10 */typeof(Action<,,,,,,,,,>),
            /* 11 */typeof(Action<,,,,,,,,,,>),
            /* 12 */typeof(Action<,,,,,,,,,,,>),
            /* 13 */typeof(Action<,,,,,,,,,,,,>),
            /* 14 */typeof(Action<,,,,,,,,,,,,,>),
            /* 15 */typeof(Action<,,,,,,,,,,,,,,>),
            /* 16 */typeof(Action<,,,,,,,,,,,,,,,>)
        };
        protected static readonly Type[] TypeFunctions = new[] {
            /* 00 */typeof(Func<>), // 零个参数; 带返回值;
            /* 01 */typeof(Func<,>),
            /* 02 */typeof(Func<,,>),
            /* 03 */typeof(Func<,,,>),
            /* 04 */typeof(Func<,,,,>),
            /* 05 */typeof(Func<,,,,,>),
            /* 06 */typeof(Func<,,,,,,>),
            /* 07 */typeof(Func<,,,,,,,>),
            /* 08 */typeof(Func<,,,,,,,,>),
            /* 09 */typeof(Func<,,,,,,,,,>),
            /* 10 */typeof(Func<,,,,,,,,,,>),
            /* 11 */typeof(Func<,,,,,,,,,,,>),
            /* 12 */typeof(Func<,,,,,,,,,,,,>),
            /* 13 */typeof(Func<,,,,,,,,,,,,,>),
            /* 14 */typeof(Func<,,,,,,,,,,,,,,>),
            /* 15 */typeof(Func<,,,,,,,,,,,,,,,>),
            /* 16 */typeof(Func<,,,,,,,,,,,,,,,,>)
        };

        protected const int MaximumIndexes = 14;

        // void Action(TPropertyOrField value);
        protected static readonly Type TypeMemberSetStatic = TypeActions[1];
        // TPropertyOrField Function();
        protected static readonly Type TypeMemberGetStatic = TypeFunctions[0];

        // void Action(TInstance instance, TPropertyOrField value);
        protected static readonly Type TypeMemberSetInstance = TypeActions[2];
        // TPropertyOrField Function(TInstance instance);
        protected static readonly Type TypeMemberGetInstance = TypeFunctions[1];

        // void Action(TInstance instance, Object[] indexes, TProperty value);
        protected static readonly Type TypeIndexerSetInstance = TypeActions[3];
        // TProperty Function(TInstance instance, Object[] indexes);
        protected static readonly Type TypeIndexerGetInstance = TypeFunctions[2];

        #endregion

        #region IReflectionService
        public virtual Func<TField> MakeGetter<TField>(FieldInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual Action<TField> MakeSetter<TField>(FieldInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual Func<TInstance, TField> MakeGetter<TInstance, TField>(FieldInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual Action<TInstance, TField> MakeSetter<TInstance, TField>(FieldInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual Func<TProperty> MakeGetter<TProperty>(PropertyInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual Action<TProperty> MakeSetter<TProperty>(PropertyInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual Func<TInstance, TProperty> MakeGetter<TInstance, TProperty>(PropertyInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual Action<TInstance, TProperty> MakeSetter<TInstance, TProperty>(PropertyInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual TDelegate MakeGetterOfIndexer<TDelegate>(PropertyInfo indexer)
        {
            throw new NotImplementedException();
        }

        public virtual TDelegate MakeSetterOfIndexer<TDelegate>(PropertyInfo indexer)
        {
            throw new NotImplementedException();
        }

        public virtual Func<TInstance, object[], TProperty> MakeGetterOfIndexer<TInstance, TProperty>(PropertyInfo indexer)
        {
            throw new NotImplementedException();
        }

        public virtual Action<TInstance, object[], TProperty> MakeSetterOfIndexer<TInstance, TProperty>(PropertyInfo indexer)
        {
            throw new NotImplementedException();
        }

        public virtual TDelegate MakeInvoker<TDelegate>(MethodInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual Func<TInstance, object[], TResult> MakeInvokerByUniversal<TInstance, TResult>(MethodInfo member)
        {
            throw new NotImplementedException();
        }

        public virtual TDelegate MakeConstructor<TDelegate>(ConstructorInfo constructor)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
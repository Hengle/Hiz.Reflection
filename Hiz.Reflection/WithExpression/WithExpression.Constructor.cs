using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Hiz.Reflection
{
    partial class ReflectionWithExpression
    {
        /* TODO:
         * Expression<Func<int, int?, string, ClassModel>> lambda = (int x, int? y, string z) => new ClassModel() { Int32 = x, Int32Nullable = y, String = z };
         */

        /* 通用格式:
         * T Func(object[] arguments, object[] bindings) {
         *     return new T((TArg0)arguments[0], (TArg1)arguments[1], ...) {
         *         Property0 = (TMember0)bindings[0],
         *         Property1 = (TMember1)bindings[1],
         *         ...
         *     };
         * }
         * 
         * 预设格式: 限制无参构造 (可以追加属性/字段赋值)
         * T Func(TMember0 binding0, TMember1 binding1, ...) where T : new()
         * {
         *     return new T() {
         *         Property0 = binding0,
         *         Property1 = binding1,
         *         ...
         *     }
         * }
         * 
         * 预设格式: 限制带参构造 (不能追加属性/字段赋值)
         * T Func(TArg0 arg0, TArg1 arg1, ...) {
         *     return new T(arg0, arg1, ...);
         * }
         * 
         * 或者两者组合?
         * T Func(TArg0 arg0, TArg1 arg1, ..., TMember0 binding0, TMember1 binding1, ...) {
         *     return new T(arg0, arg1, ...) {
         *         Property0 = binding0,
         *         Property1 = binding1,
         *         ...
         *     };
         * }
         */

        public override TDelegate MakeConstructor<TDelegate>(ConstructorInfo constructor)
        {
            var @new = Expression.New(constructor, (IEnumerable<Expression>)null);
            var lambda = Expression.Lambda<TDelegate>(@new, null, false, (IEnumerable<ParameterExpression>)null);
            return lambda.Compile();
        }
    }
}

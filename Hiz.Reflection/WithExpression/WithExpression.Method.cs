using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hiz.Reflection
{
    partial class ReflectionWithExpression
    {
        #region Method.UserDefined/T0-T1

        public override TDelegate MakeInvoker<TDelegate>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");

            var lambda = InternalMethodInvokeWithExpression<TDelegate>(member);
            return lambda.Compile();
        }

        // OK
        static Expression<TDelegate> InternalMethodInvokeWithExpression<TDelegate>(MethodInfo member)
        {
            var @delegate = typeof(TDelegate);
            if (!@delegate.IsDelegate())
                throw new ArgumentException("不是委托类型", "TDelegate");

            var invoke = @delegate.GetMethod(DelegateInvoke);
            // if ((method.ReturnType == TypeVoid && invoke.ReturnType != TypeVoid) || (method.ReturnType != TypeVoid && invoke.ReturnType == TypeVoid))
            //     throw new ArgumentException("委托返回类型不符", "TDelegate"); // 改为允许这种情况

            var targets = invoke.GetParameters(); // TDelegate.Method.Parameters;
            var sources = member.GetParameters(); // TInstance.Method.Parameters;

            var offset = member.IsStatic ? 0 : 1; // TDelegate.Method.Parameters[0] = TInstance;
            var length = sources.Length;
            if (length + offset != targets.Length)
                throw new ArgumentException("委托参数数量不对", "TDelegate");

            #region 参数转换
            IList<ByRefTuple> byrefs = null;
            var arguments = new Expression[length];
            var parameters = new ParameterExpression[length + offset]; // 使用 TDelegate 参数命名;
            for (var i = 0; i < length; i++) // i: IndexOf: TInstance.Method.Parameters;
            {
                var k = i + offset; // k: IndexOf: TDelegate.Method.Parameters;
                var s = sources[i];
                var t = targets[k];

                var source = s.ParameterType;
                if (!source.IsByRef) // ByVal // 按值传递参数
                {
                    /* Lambda(object value) {
                     *     Method((TParameter)value);
                     * }
                     */
                    var parameter = Expression.Parameter(t.ParameterType, t.Name);
                    parameters[k] = parameter;
                    arguments[i] = ConvertIfNeeded(source, parameter);
                }
                else // ref/out // 引用传递参数
                {
                    var target = t.ParameterType;
                    if (target.IsByRef) // ref/out
                    {
                        if (!s.IsOut && t.IsOut)
                        {
                            /* 如果原始修饰: out, 外部修饰: ref, 可以兼容;
                             * Lambda(ref object result) {
                             *     int variable;
                             *     Method(out variable); // 没有用到传进参值;
                             *     result = (object)variable; // Save
                             * }
                             * 
                             * 如果原始修饰: ref, 外部修饰: out, 则不兼容;
                             * Lambda(out object result) {
                             *     int variable = (int)result; // 抛出异常;
                             *     Method(ref variable);
                             *     result = (object)variable; // Save
                             * }
                             */
                            throw new ArgumentException("委托参数修饰无法兼容", "TDelegate");
                        }
                        if (source != target)
                        {
                            /* 暂不允许出现这种情况 (是否实现?):
                             * Lambda(ref object result) {
                             *     int variable;
                             *     variable = (int)result; // Load
                             *     Method(ref variable); // ref/out 修饰要求: 传入变量类型必须完全相等参数类型;
                             *     result = (object)variable; // Save
                             * }
                             */
                            //throw new ArgumentException("ref/out 参数禁止协变", "TDelegate");

                            source = source.GetElementType(); // non-ref;
                            var parameter = Expression.Parameter(t.ParameterType, t.Name); // T&

                            // 1. 定义一个临时变量: T variable;
                            var variable = Expression.Variable(source);
                            // 2. 将参数值赋予变量: variable = (T)parameter;
                            var load = s.IsOut ? null : Expression.Assign(variable, ConvertIfNeeded(variable.Type, parameter));
                            // 3. 调用方法: Method(ref/out variable);
                            // 4. 将输出值存入参数: parameter = (TParameter)variable;
                            var save = Expression.Assign(parameter, ConvertIfNeeded(parameter.Type, variable));

                            if (byrefs == null)
                                byrefs = new List<ByRefTuple>();
                            byrefs.Add(new ByRefTuple(variable, save, load));

                            parameters[k] = parameter;
                            arguments[i] = variable;
                        }
                        else
                        {
                            var parameter = Expression.Parameter(t.ParameterType, t.Name); // T&
                            parameters[k] = parameter;
                            arguments[i] = parameter;
                        }
                    }
                    #region 暂时取消 等待改进: ByRef<T>
                    // else if (IsByRefWrapper(target)) // ByRef<T>
                    // {
                    //     source = source.GetElementType(); // non-ref;
                    //     target = target.GetGenericArguments()[0]; // non-ByRefWrapper;
                    //     // if (source != target)
                    //     // {
                    //     //     throw new ArgumentException("ref/out 参数禁止协变", "TDelegate");
                    //     // }
                    //     var parameter = Expression.Parameter(t.ParameterType, t.Name); // ByRef<T>
                    //     var property = Expression.Property(parameter, GetWrapperValueMember(target)); // TParameter

                    //     // 1. 定义一个临时变量: T variable;
                    //     var variable = Expression.Variable(source);
                    //     // 2. 将包装值赋予变量: variable = (T)wrapper.Value;
                    //     var load = s.IsOut ? null : Expression.Assign(variable, ConvertIfNeeded(variable.Type, property));
                    //     // 3. 调用方法: Method(ref/out variable);
                    //     // 4. 将输出值存入包装: wrapper.Value = (TParameter)variable;
                    //     var save = Expression.Assign(property, ConvertIfNeeded(property.Type, variable));

                    //     if (byrefs == null)
                    //         byrefs = new List<ByRefTuple>();
                    //     byrefs.Add(new ByRefTuple(variable, save, load));

                    //     parameters[k] = parameter;
                    //     arguments[i] = variable;
                    // }
                    #endregion
                    else
                    {
                        throw new ArgumentException("委托参数修饰不对", "TDelegate");
                    }
                }
            }
            #endregion

            #region 方法调用
            MethodCallExpression call;
            if (!member.IsStatic)
            {
                var t = targets[0];
                if (t.IsOut)
                    throw new ArgumentException("实例参数不能使用 out 修饰", "TDelegate");

                var instance = Expression.Parameter(t.ParameterType, t.Name);
                parameters[0] = instance;

                var reflected = member.ReflectedType;
                if (instance.IsByRef && reflected.IsValueType && reflected != instance.Type) // ref 修饰的值类型参数
                {
                    /* void Invoke(ref object instance, object arg0, object arg0, ...) {
                     *     TReflected variable;
                     *     variable = (TReflected)instance;
                     *     variable.Method(TArg0)arg0, (TArg1)arg1, ...);
                     *     instance = (TInstance)variable;
                     *     // return default(TDelegate.ReturnType) // if TDelegate.ReturnType != void;
                     * }
                     * 
                     * TResult Invoke(ref object instance, object arg0, object arg1, ...) {
                     *     TReflected variable;
                     *     TResult result;
                     *     variable = (TReflected)instance;
                     *     result = (TResult)variable.Method(TArg0)arg0, (TArg1)arg1, ...);
                     *     instance = (TInstance)variable;
                     *     return result;
                     * }
                     */
                    var variable = Expression.Variable(reflected);
                    var load = Expression.Assign(variable, ConvertIfNeeded(variable.Type, instance));
                    var save = Expression.Assign(instance, ConvertIfNeeded(instance.Type, variable));
                    if (byrefs == null)
                        byrefs = new List<ByRefTuple>();
                    byrefs.Add(new ByRefTuple(variable, save, load));

                    call = Expression.Call(variable, member, arguments);
                }
                #region 没有意义 暂时取消
                // else if (IsByRefWrapper(instance.Type)) // 实例使用 ByRef 包装没有意义, 对于值类型的方法调用, 性能更差;
                // {
                //     var property = Expression.Property(instance, GetWrapperValueMember(instance.Type.GetGenericArguments()[0]));

                //     var variable = Expression.Variable(reflected);
                //     var load = Expression.Assign(variable, ConvertIfNeeded(property, variable.Type));
                //     var save = Expression.Assign(property, ConvertIfNeeded(variable, property.Type));
                //     if (byrefs == null)
                //         byrefs = new List<ByRefTuple>();
                //     byrefs.Add(new ByRefTuple(variable, save, load));

                //     call = Expression.Call(variable, member, arguments);
                // }
                #endregion
                else
                {
                    call = Expression.Call(ConvertIfNeeded(reflected, instance), member, arguments);
                }
            }
            else
            {
                call = Expression.Call(null, member, arguments);
            }

            Expression body;
            if (byrefs == null || byrefs.Count == 0) // 没有用到引用参数
            {
                body = call;
            }
            else
            {
                body = InternalMakeByRefBlock(byrefs, call, invoke.ReturnType);
            }
            #endregion

            var lambda = Expression.Lambda<TDelegate>(body, null, false, (IEnumerable<ParameterExpression>)parameters);
            return lambda;
        }

        // 暂不公开: 改用定义委托方式更好.
        /// <summary>
        /// 使用预设委托类型: Action/Func; 需要手动转换返回类型; ref/out 修饰使用 ByRef(T) 替代;
        /// 最大支持参数数量: 静态方法 16; 实例方法 15;
        /// </summary>
        /// <param name="member"></param>
        /// <param name="byref">实例参数是否使用 ref 修饰; 仅对实例成员有效;</param>
        /// <returns></returns>
        static Delegate MakeInvoke(MethodInfo member, bool byref = false)
        {
            if (member == null)
                throw Error.ArgumentNull("member");

            var lambda = InternalMethodInvokeWithExpression(member, false);
            return lambda.Compile();
        }

        // OK
        static LambdaExpression InternalMethodInvokeWithExpression(MethodInfo member, bool byref)
        {
            if (member == null)
                throw new MissingMemberException();

            var arity = (!member.IsStatic && byref) ? MaximumArityRef : MaximumArity;
            if (!member.IsStatic)
                arity--;

            // 获取方法参数集合
            var sources = member.GetParameters();
            var length = sources.Length;
            if (length > arity)
                throw new NotSupportedException("参数数量超过预设委托最大定义");

            #region 转换方法参数
            // 用于内部调用方法参数: Expression.Call()
            Expression[] arguments = null;
            // 用于生成委托签名参数: Expression.Lambda()
            ParameterExpression[] parameters = null;
            // 记录引用参数处理逻辑.
            IList<ByRefTuple> byrefs = null;
            if (length > 0)
            {
                arguments = new Expression[length];
                parameters = new ParameterExpression[length];

                for (var i = 0; i < length; i++)
                {
                    var s = sources[i];
                    var source = s.ParameterType;

                    // 引用参数需要特殊处理...
                    if (source.IsByRef)
                    {
                        throw new NotSupportedException("ByRef");

                        #region 暂时取消 等待改进: ByRef<T>
                        // source = source.GetElementType();

                        // var info = GetWrapperValueMember(source); // typeof(ByRef<TParameter>).GetProperty("Value");
                        // var parameter = Expression.Parameter(info.ReflectedType); // ByRef<TParameter>
                        // var property = Expression.Property(parameter, info); // TParameter

                        // // 1. 定义一个临时变量: T variable;
                        // var variable = Expression.Variable(source);
                        // // 2. 将包装值赋予变量: variable = wrapper.Value;
                        // var load = s.IsOut ? null : Expression.Assign(variable, property);
                        // // 3. 调用方法: Method(ref variable);
                        // // 4. 将输出值存入包装: wrapper.Value = variable;
                        // var save = Expression.Assign(property, variable);

                        // if (byrefs == null)
                        //     byrefs = new List<ByRefTuple>(length);
                        // byrefs.Add(new ByRefTuple(variable, save, load));

                        // arguments[i] = variable;
                        // parameters[i] = parameter;
                        #endregion
                    }
                    else
                    {
                        arguments[i] = parameters[i] = Expression.Parameter(source);
                    }
                }
            }
            #endregion

            #region 方法调用
            MethodCallExpression call;
            if (!member.IsStatic)
            {
                var reflected = member.ReflectedType;
                var instance = Expression.Parameter(!byref ? reflected : reflected.MakeByRefType());
                if (instance.IsByRef && reflected.IsValueType && reflected != instance.Type) // ref 修饰的值类型参数
                {
                    var variable = Expression.Variable(reflected);
                    var load = Expression.Assign(variable, ConvertIfNeeded(variable.Type, instance));
                    var save = Expression.Assign(instance, ConvertIfNeeded(instance.Type, variable));
                    if (byrefs == null)
                        byrefs = new List<ByRefTuple>();
                    byrefs.Add(new ByRefTuple(variable, save, load));

                    call = Expression.Call(variable, member, arguments);
                }
                else
                {
                    call = Expression.Call(instance, member, arguments);
                }

                parameters = parameters != null ? new[] { instance }.Concat(parameters).ToArray() : new[] { instance };
            }
            else
            {
                call = Expression.Call(member, arguments);
            }

            Expression body;
            if (byrefs == null || byrefs.Count == 0) // 没有用到引用参数
            {
                body = call;
            }
            else
            {
                body = InternalMakeByRefBlock(byrefs, call, null);
            }
            #endregion

            var lambda = Expression.Lambda(body, null, false, (IEnumerable<ParameterExpression>)parameters);
            return lambda;
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
        public override Func<TInstance, object[], TResult> MakeInvokerByUniversal<TInstance, TResult>(MethodInfo member)
        {
            if (member == null)
                throw new ArgumentNullException();

            var lambda = InternalMethodInvokeWithExpression(member, typeof(TInstance), typeof(TResult), true, false);
            return (Func<TInstance, object[], TResult>)lambda.Compile();
        }

        // OK
        /// <summary>
        /// 实例 无返回值 无参数值: void Action(TInstance);
        /// 实例 无返回值 有参数值: void Action(TInstance, object[]);
        /// 实例 有返回值 无参数值: TResult Func(TInstance);
        /// 实例 有返回值 有参数值: TResult Func(TInstance, object[]);
        /// 
        /// 静态 无返回值 无参数值: void Action();
        /// 静态 无返回值 有参数值: void Action(object[]);
        /// 静态 有返回值 无参数值: TResult Func();
        /// 静态 有返回值 有参数值: TResult Func(object[]);
        /// </summary>
        /// <param name="member"></param>
        /// <param name="object">实例类型转换; 空值表示不变; 对于静态方法此处如果有值, 则将创建实例参数, 但不使用;</param>
        /// <param name="return">结果类型转换; 空值表示不变;</param>
        /// <param name="always">是否总是创建 object[] parameters 参数; 即使方法没有参数;</param>
        /// <param name="byref">实例参数是否使用 ref 修饰; 静态成员只在 @object 参数有值时才有效;</param>
        /// <returns></returns>
        static LambdaExpression InternalMethodInvokeWithExpression(MethodInfo member, Type @object, Type @return, bool always, bool byref)
        {
            #region 转换参数
            // 获取方法参数集合
            var sources = member.GetParameters();
            var length = sources.Length;

            // 用于内部调用方法参数: Expression.Call()
            Expression[] arguments = null;

            // 定义方法组合参数; 强制; 即使方法没有参数;
            var coalition = (length > 0 || always) ? Expression.Parameter(TypeObjectArray) : null;

            // 记录引用参数处理逻辑.
            IList<ByRefTuple> byrefs = null;
            if (length > 0)
            {
                arguments = new Expression[length];

                for (var i = 0; i < length; i++)
                {
                    var index = Expression.Constant(i);

                    var s = sources[i];
                    var source = s.ParameterType;
                    if (source.IsByRef)
                    {
                        // 1. 定义一个临时变量: TParameter variable;
                        var variable = Expression.Variable(source.GetElementType());
                        // 2. 将组合参数值赋予变量: variable = (TParameter)coalition[index];
                        var load = s.IsOut ? null : Expression.Assign(variable, ConvertIfNeeded(variable.Type, Expression.ArrayAccess(coalition, index)));
                        // 3. 调用执行方法: Method(ref variable);
                        // 4. 将输出值存入组合参数: coalition[index] = (object)variable;
                        var save = Expression.Assign(Expression.ArrayAccess(coalition, index), ConvertIfNeeded(TypeObject, variable));
                        if (byrefs == null)
                            byrefs = new List<ByRefTuple>();
                        byrefs.Add(new ByRefTuple(variable, save, load));

                        arguments[i] = variable;
                    }
                    else
                    {
                        arguments[i] = ConvertIfNeeded(source, Expression.ArrayAccess(coalition, index));
                    }
                }
            }
            #endregion

            #region 方法调用
            // 用于生成委托签名参数: Expression.Lambda()
            IEnumerable<ParameterExpression> parameters;

            // byref 暂不实现;

            // 调用方法
            MethodCallExpression call;
            if (!member.IsStatic)
            {

                // 定义实例参数
                var reflected = member.ReflectedType;
                var instance = Expression.Parameter(@object ?? reflected);
                call = Expression.Call(ConvertIfNeeded(reflected, instance), member, (IEnumerable<Expression>)arguments);

                parameters = coalition != null ? new[] { instance, coalition } : new[] { instance };
            }
            else
            {
                // if (@object != null)
                //     throw new NotSupportedException("静态方法不能转换实例参数类型");

                if (@object != null)
                {
                    var instance = Expression.Parameter(@object);
                    parameters = coalition != null ? new[] { instance, coalition } : new[] { instance };
                }
                else
                {
                    parameters = coalition != null ? new[] { coalition } : null;
                }

                call = Expression.Call(null, member, (IEnumerable<Expression>)arguments);
            }

            Expression body;
            if (byrefs == null || byrefs.Count == 0) // 没有用到 ref/out 参数
            {
                if (call.Type == TypeVoid)
                {
                    if (@return != null && @return != TypeVoid)
                    {
                        /* 追加返回值的类型:
                         * Block<TResult> {
                         *     Action(...);
                         *     return default(TResult);
                         * }
                         */
                        body = Expression.Block(call, Expression.Default(@return)); // Expression.Constant(null);
                    }
                    else
                    {
                        body = call;
                    }
                }
                else
                {
                    /* 转换返回值的类型:
                     * return (TResult)Function(...);
                     */
                    body = ConvertIfNeeded(@return ?? call.Type, call);
                }
            }
            else
            {
                body = InternalMakeByRefBlock(byrefs, call, @return ?? call.Type);
            }
            #endregion

            var lambda = Expression.Lambda(body, parameters);
            return lambda;
        }

        #endregion

        #region Method.Predefined/T0-T5

        // Action:T1-T4; Function:T2-T5; ActionStatic:T0-T3; FunctionStatic:T1-T4

        public Action<TInstance> MakeAction<TInstance>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");

            return InternalMethodInvokeWithExpression<Action<TInstance>>(member).Compile();
        }
        public Action<TInstance, T1> MakeAction<TInstance, T1>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");

            return InternalMethodInvokeWithExpression<Action<TInstance, T1>>(member).Compile();
        }
        public Action<TInstance, T1, T2> MakeAction<TInstance, T1, T2>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");

            return InternalMethodInvokeWithExpression<Action<TInstance, T1, T2>>(member).Compile();
        }
        public Action<TInstance, T1, T2, T3> MakeAction<TInstance, T1, T2, T3>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");

            return InternalMethodInvokeWithExpression<Action<TInstance, T1, T2, T3>>(member).Compile();
        }

        public Func<TInstance, TResult> MakeFunction<TInstance, TResult>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");

            return InternalMethodInvokeWithExpression<Func<TInstance, TResult>>(member).Compile();
        }
        public Func<TInstance, T1, TResult> MakeFunction<TInstance, T1, TResult>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");

            return InternalMethodInvokeWithExpression<Func<TInstance, T1, TResult>>(member).Compile();
        }
        public Func<TInstance, T1, T2, TResult> MakeFunction<TInstance, T1, T2, TResult>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");

            return InternalMethodInvokeWithExpression<Func<TInstance, T1, T2, TResult>>(member).Compile();
        }
        public Func<TInstance, T1, T2, T3, TResult> MakeFunction<TInstance, T1, T2, T3, TResult>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.IsStatic)
                throw Error.OnlyInstanceMember("member");

            return InternalMethodInvokeWithExpression<Func<TInstance, T1, T2, T3, TResult>>(member).Compile();
        }

        public Action MakeActionStatic(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");

            return InternalMethodInvokeWithExpression<Action>(member).Compile();
        }
        public Action<T1> MakeActionStatic<T1>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");

            return InternalMethodInvokeWithExpression<Action<T1>>(member).Compile();
        }
        public Action<T1, T2> MakeActionStatic<T1, T2>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");

            return InternalMethodInvokeWithExpression<Action<T1, T2>>(member).Compile();
        }
        public Action<T1, T2, T3> MakeActionStatic<T1, T2, T3>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");

            return InternalMethodInvokeWithExpression<Action<T1, T2, T3>>(member).Compile();
        }

        public Func<TResult> MakeFunctionStatic<TResult>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");

            return InternalMethodInvokeWithExpression<Func<TResult>>(member).Compile();
        }
        public Func<T1, TResult> MakeFunctionStatic<T1, TResult>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");

            return InternalMethodInvokeWithExpression<Func<T1, TResult>>(member).Compile();
        }
        public Func<T1, T2, TResult> MakeFunctionStatic<T1, T2, TResult>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");

            return InternalMethodInvokeWithExpression<Func<T1, T2, TResult>>(member).Compile();
        }
        public Func<T1, T2, T3, TResult> MakeFunctionStatic<T1, T2, T3, TResult>(MethodInfo member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (!member.IsStatic)
                throw Error.OnlyStaticMember("member");

            return InternalMethodInvokeWithExpression<Func<T1, T2, T3, TResult>>(member).Compile();
        }

        #endregion
    }
}

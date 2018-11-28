using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Hiz.Reflection
{
    class DynamicEmit
    {
        ILGenerator il;
        public DynamicEmit(ILGenerator il)
        {
            this.il = il;
        }

        #region OK

        public void LoadField(FieldInfo field)
        {
            if (field.IsStatic)
            {
                /* 将静态字段的值推送到计算堆栈上。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldsfld(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将指定字段的值推送到堆栈上。
                 * 
                 * ldsfld 指令将静态（在类的所有实例中共享）字段的值推送到堆栈上。返回类型是与传递的元数据标记 field 关联的类型。
                 * ldsfld 指令可以有一个 Volatile 前缀。
                 * 
                 * 下面的 Emit 方法重载可以使用 ldsfld 操作码：
                 * ILGenerator.Emit(OpCode, FieldInfo)
                 */
                il.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                /* 查找对象中其引用当前位于计算堆栈的字段的值。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldfld(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将对象引用（或指针）推送到堆栈上。
                 * 2. 从堆栈中弹出对象引用（或指针）；找到对象中指定字段的值。
                 * 3. 将字段中存储的值推送到堆栈上。
                 * 
                 * ldfld 指令将位于对象中的字段的值推送到堆栈上。
                 * 对象在堆栈上必须以对象引用（O 类型）、托管指针（& 类型）、非托管指针（native int 类型）、瞬态指针（* 类型）或值类型的实例的形式存在。
                 * 可验证代码中不允许使用非托管指针。
                 * 对象的字段由必须引用字段成员的元数据标记指定。
                 * 返回类型与此字段关联的类型相同。
                 * 字段可以是实例字段（在此情况下对象不得是空引用）或静态字段。
                 * 
                 * ldfld 指令可以用 Unaligned 和 Volatile 前缀中的任意一个或同时以两者开头。
                 * 
                 * 如果对象为空且字段不是静态的，则引发 NullReferenceException。
                 * 如果未在元数据中找到指定字段，则引发 MissingFieldException。这通常是在将 Microsoft 中间语言 (MSIL) 指令转换为本机代码时而不是在运行时检查。
                 * 
                 * 下面的 Emit 方法重载可以使用 ldfld 操作码：
                 * ILGenerator.Emit(OpCode, FieldInfo)
                 */
                il.Emit(OpCodes.Ldfld, field);
            }
        }
        public void LoadFieldAddress(FieldInfo field)
        {
            if (field.IsStatic)
            {
                /* 将静态字段的地址推送到计算堆栈上。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldsflda(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将特定字段的地址推送到堆栈上。
                 * 
                 * ldsflda 指令将静态（在类的所有实例中共享）字段的地址推送到堆栈上。
                 * 如果元数据标记 field 表示其内存是被托管的类型，则该地址可被表示为瞬态指针（* 类型）。 否则，它对应于非托管指针（native int 类型）。
                 * 请注意，field 可以是静态全局变量，它具有在其中内存是非托管的分配的相对虚拟地址（该字段距离基址的偏移量，在基址其包含的 PE 文件被加载到内存中）。
                 * 
                 * ldsflda 指令可以有一个 Volatile 前缀。
                 * 
                 * 如果未在元数据中找到 field，则引发 MissingFieldException。 该异常通常是在将 Microsoft 中间语言 (MSIL) 指令转换为本机代码时而不是在运行时进行检查。
                 * 
                 * 下面的 Emit 方法重载可以使用 ldsflda 操作码：
                 * ILGenerator.Emit(OpCode, FieldInfo)
                 */
                il.Emit(OpCodes.Ldsflda, field);
            }
            else
            {
                /* 查找对象中其引用当前位于计算堆栈的字段的地址。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldflda(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将对象引用（或指针）推送到堆栈上。
                 * 2. 从堆栈中弹出对象引用（或指针）；找到对象中指定字段的地址。
                 * 3. 将指定字段的地址推送到堆栈上。
                 * 
                 * ldflda 指令将位于对象中的字段的地址推送到堆栈上。
                 * 对象在堆栈上必须以对象引用（O 类型）、托管指针（& 类型）、非托管指针（native int 类型）、瞬态指针（* 类型）或值类型的实例的形式存在。
                 * 可验证代码中不允许使用非托管指针。 对象的字段由必须引用字段成员的元数据标记指定。
                 * 由 ldflda 返回的值是托管指针（& 类型），除非对象被作为非托管指针推送到堆栈上，在此情况下，返回地址还是非托管指针（native int 类型）。
                 * 
                 * ldflda  指令可以用 Unaligned 和 Volatile 前缀中的任意一个或同时以两者开头。
                 * 
                 * 如果对象不在正从其中访问它的应用程序域内，则引发 InvalidOperationException。 不在正访问的应用程序域内的字段的地址不能被加载。
                 * 如果对象为空且字段不是静态的，则引发 NullReferenceException。
                 * 如果未在元数据中找到指定字段，则引发 MissingFieldException。 这通常是在将 Microsoft 中间语言 (MSIL) 指令转换为本机代码时而不是在运行时检查。
                 * 
                 * 下面的 Emit 方法重载可以使用 ldflda 操作码：
                 * ILGenerator.Emit(OpCode, FieldInfo)
                 */
                il.Emit(OpCodes.Ldflda, field);
            }
        }
        public void StoreField(FieldInfo field)
        {
            if (field.IsStatic)
                /* 用来自计算堆栈的值替换静态字段的值。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.stsfld(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将值推送到堆栈上。
                 * 2. 从堆栈中弹出值并将它存储在 field 中。
                 * 
                 * stsfld 指令用来自堆栈的值替换静态字段的值。 field 是必须表示静态字段成员的元数据标记。
                 * 
                 * stsfld 指令可以将 Volatile 作为前缀。
                 * 
                 * 如果未在元数据中找到 field，则引发 MissingFieldException。 这通常是在将 Microsoft 中间语言 (MSIL) 指令转换为本机代码时而不是在运行时检查。 
                 * 
                 * 下面的 Emit 方法重载可以使用 stsfld 操作码：
                 * ILGenerator.Emit(OpCode, FieldInfo)
                 */
                il.Emit(OpCodes.Stsfld, field);
            else
                /* 用新值替换在对象引用或指针的字段中存储的值。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.stfld(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将对象引用（或指针）推送到堆栈上。
                 * 2. 将值推送到堆栈上。
                 * 3. 从堆栈中弹出该值和对象引用/指针；用所提供的值替换对象中 field 的值。
                 * 
                 * stfld 指令替换的对象（类型 O）的字段的值，或通过指针（类型 native int、& 或 *）替换为给定的值。
                 * Field 是表示字段成员引用的元数据标记。
                 * stfld  指令可以具有 Unaligned 和 Volatile 中任意一个的前缀或全部这二者的前缀。
                 * 
                 * 如果对象引用或指针是空引用并且该字段是静态的，则引发 NullReferenceException。
                 * 如果未在元数据中找到 field，则引发 MissingFieldException。 此异常通常是在将 Microsoft 中间语言 (MSIL) 指令转换为本机代码时而不是在运行时进行检测。
                 * 
                 * 下面的 Emit 方法重载可以使用 stfld 操作码：
                 * ILGenerator.Emit(OpCode, FieldInfo)
                 */
                il.Emit(OpCodes.Stfld, field);
        }

        public void Return()
        {
            /* 从当前方法返回，并将返回值（如果存在）从调用方的计算堆栈推送到被调用方的计算堆栈上。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ret(v=vs.100).aspx
             * 
             * 堆栈转换行为依次为：
             * 1. 从被调用方的计算堆栈中弹出返回值。
             * 2. 将步骤 1 中获取的返回值推送到调用方的计算堆栈中。
             * 
             * 如果在被调用方的计算堆栈上不存在返回值，则不返回任何值（没有被调用方或调用方方法的堆栈转换行为）。
             * 当前方法的返回值类型（如果有）确定要从堆栈顶部获取的并被复制到调用当前方法的方法的堆栈上的值的类型。 当前方法的计算堆栈必须为空，只有要返回的值除外。
             * 
             * ret 指令不能用于将控制转移出 try、filter、catch 或 finally 块。 从 try 或 catch 内，将 Leave 指令与 ret 指令的目标（该目标在所有封闭的异常块之外）一起使用。
             * 因为 filter 和 finally 块在逻辑上是异常处理的一部分并且不是在其中嵌入其代码的方法，所以正确生成的 Microsoft 中间语言 (MSIL) 指令不执行从 filter 或 finally 内返回的方法。
             * 
             * 下面的 Emit 方法重载可以使用 ret 操作码：
             * ILGenerator.Emit(OpCode)
             */
            il.Emit(OpCodes.Ret);
        }

        public void Box(Type type)
        {
            /* 将值类转换为对象引用（O 类型）。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.box(v=vs.100).aspx
             * 
             * 堆栈转换行为依次为：
             * 1. 将值类型推送到堆栈上。
             * 2. 从堆栈中弹出值类型；执行 box 操作。
             * 3. 将对结果的“已装箱”值类型的对象引用推送到堆栈上。
             * 
             * 值类型在 Common Language Infrastructure (CLI) 内具有两个单独的表示形式。
             * 当值类型嵌入在其他对象内或堆栈上时使用的“原始”形式。
             * “已装箱”形式，其中值类型的数据被包装（装箱）到对象中以便该对象可以作为独立实体存在。
             * 
             * box 指令将“原始”（未装箱）值类型转换成对象引用（O 类型）。 这是通过创建新对象并将值类型的数据复制到新分配的对象中来实现的。
             * valTypeToken 是元数据标记，它指示堆栈上的值类型的类型。
             * 
             * 如果内存不能满足请求的需要，则引发 OutOfMemoryException。
             * 如果找不到类，则引发 TypeLoadException。 此异常通常是在将 Microsoft 中间语言 (MSIL) 转换为本机代码时而不是在运行时进行检测。
             * 
             * 下面的 Emit 方法重载可以使用 box 操作码：
             * ILGenerator.Emit(OpCode, Type)
             */
            il.Emit(OpCodes.Box, type);
        }
        public void Unbox(Type type)
        {
            /* 将值类型的已装箱的表示形式转换为其未装箱的形式。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.unbox(v=vs.100).aspx
             * 
             * 堆栈转换行为依次为：
             * 1. 将对象引用推送到堆栈上。
             * 2. 从堆栈中弹出对象引用并取消装箱为值类型指针。
             * 3. 将值类型指针推送到堆栈上。
             * 
             * 值类型在 Common Language Infrastructure (CLI) 内具有两个单独的表示形式。
             * 当值类型嵌入在其他对象内时使用的“原始”形式。
             * “已装箱”形式，其中值类型的数据被包装（装箱）到对象中以便该对象可以作为独立实体存在。
             * 
             * unbox 指令将对象引用（O 类型，值类型的已装箱的表示形式）转换为值类型指针（托管指针，& 类型，其未装箱的形式）。
             * 提供的值类型 (valType) 是元数据标记，它指示在已装箱的对象内包含的值类型的类型。
             * 
             * 与 Box 不同（它需要复制值类型以用于对象中），unbox 不要求从对象复制值类型。 通常它只计算已出现在已装箱的对象内的值类型的地址。
             * 
             * 如果对象未被装箱为 valType，则引发 InvalidCastException。
             * 如果对象引用是空引用，则引发 NullReferenceException。
             * 如果不能找到值类型 valType，则引发 TypeLoadException。此异常通常是在将 Microsoft 中间语言 (MSIL) 指令转换为本机代码时而不是在运行时进行检测。
             * 
             * 下面的 Emit 方法重载可以使用 unbox 操作码：
             * ILGenerator.Emit(OpCode, Type)
             */
            il.Emit(OpCodes.Unbox, type);
        }
        public void UnboxAny(Type type)
        {
            /* 将指令中指定类型的已装箱的表示形式转换成未装箱形式。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.unbox_any(v=vs.100).aspx
             * 
             * 堆栈转换行为依次为：
             * 1. 将对象引用 obj 推送到堆栈上。
             * 2. 从堆栈中弹出对象引用，取消装箱到指令中指定的类型。
             * 3. 将结果对象引用或值类型推送到堆栈上。
             * 
             * 当应用于值类型的已装箱形式时，unbox.any 指令提取 obj（类型为 O）中包含的值，因此等效于 unbox 后跟 ldobj。
             * 当应用于引用类型时，unbox.any 指令与 castclass 效果相同。
             * 如果操作数 typeTok 为泛型类型参数，则运行时行为由为该泛型类型参数指定的类型决定。
             * 如果 obj 不是已装箱类型，则引发 InvalidCastException。
             * 如果 obj 是空引用，则引发 NullReferenceException。
             * 
             * 下面的 Emit 方法重载可以使用 unbox.any 操作码：
             * ILGenerator.Emit(OpCode, Type)
             */
            il.Emit(OpCodes.Unbox_Any, type);
        }
        public void CastClass(Type type)
        {
            /* 尝试将引用传递的对象转换为指定的类。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.castclass(v=vs.100).aspx
             * 
             * 堆栈转换行为依次为：
             * 1. 将对象引用推送到堆栈上。
             * 2. 从堆栈中弹出对象引用；引用的对象被转换为指定的 class。
             * 3. 如果成功，则新对象引用被推送到堆栈上。
             * 
             * castclass 指令尝试将堆栈顶部的对象引用（O 类型）转换为指定的类。 由指示所需类的元数据标记指定新类。
             * 如果位于堆栈顶部的对象的类不实现该新类（假定新类是接口）并且不是该新类的派生类，则引发 InvalidCastException。
             * 如果对象引用是空引用，则 castclass 成功并将新对象作为空引用返回。
             * 
             * 如果 obj 不能被转换为类，则引发 InvalidCastException。
             * 如果找不到 class，则引发 TypeLoadException。此异常通常是在将 Microsoft 中间语言 (MSIL) 指令转换为本机代码时而不是在运行时进行检测。
             * 
             * 下面的 Emit 方法重载可以使用 castclass 操作码：
             * ILGenerator.Emit(OpCode, Type)
             */
            il.Emit(OpCodes.Castclass, type);
        }

        public void Call(MethodInfo method, Type[] optionals = null)
        {
            /* 调用由传递的方法说明符指示的方法。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.call(v=vs.100).aspx
             * 
             * 堆栈转换行为依次为：
             * 1. 将从 arg1 到 argN 的方法参数推送到堆栈上。
             * 2. 从堆栈中弹出从 argN 到 arg1 的方法参数；通过这些参数执行方法调用并将控制转移到由方法说明符表示的方法。 完成后，被调用方方法生成返回值并将其发送给调用方。 
             * 3. 将返回值推送到堆栈上。
             * 
             * call 指令调用由通过该指令传递的方法说明符指示的方法。
             * 方法说明符是元数据标记，它指示将调用的方法和将被传递到该方法的放到堆栈上的参数的数目、类型和顺序以及要使用的调用约定。
             * 
             * tail (Tailcall) 前缀指令可以紧靠 call 指令之前，以指定转移控制之前应释放当前方法状态。
             * 如果调用将控制转移到比原始方法信任级别更高的方法，则不释放堆栈帧。 而是继续无提示执行，就像尚未提供 tail 一样。
             * 元数据标记带有用来确定是对静态方法、实例方法、虚方法还是全局函数进行调用所需的足够的信息。
             * 在所有这些情况中，目标地址完全从方法说明符确定（与用于调用虚方法的 Callvirt 指令相比，后者的目标地址还取决于 Callvirt 前推送的实例引用的运行时类型。）
             * 
             * 将参数按从左到右的顺序放到堆栈上。 即，先计算第一个参数并将其放到堆栈上，然后处理第二个参数，接着处理第三个参数，直到将所有需要的参数都按降序放置在堆栈的顶部为止。
             * 有三个重要的特殊情况：
             * 1. 对实例方法（或虚方法）的调用必须在任何用户可见的参数前推送该实例引用。 实例引用不得是空引用。 元数据中带有的签名不在参数列表中为 this 指针包含项；它而是使用位来指示方法是否需要传递 this 指针。 
             * 2. 使用 call（而不是 callvirt）调用虚方法是有效的；这指示将使用方法指定的类（而不是从所调用的对象动态指定的类）来解析该方法。 
             * 3. 请注意，可以通过 call 或 callvirt 指令调用委托的 Invoke 方法。
             * 
             * 如果系统安全机制没有授予调用方对被调用方法的访问权限，则可能引发 SecurityException。 在 Microsoft 中间语言 (MSIL) 指令被转换为本机代码时（而不是在运行时），可能进行安全检查。
             * 
             * 对值类型调用 System.Object 的方法时，考虑使用 constrained 前缀和 callvirt 指令，而不是发出 call 指令。
             * 因而不需要根据值类型是否重写方法来发送不同 IL，从而避免了潜在的版本问题。
             * 对值类型调用接口方法时考虑使用 constrained 前缀，因为实现接口方法的值类型方法可以用 MethodImpl 更改。这些问题在 Constrained 操作码里有更详细的说明。
             * 
             * 下面的 Emit 方法重载可以使用 call 操作码：
             * ILGenerator.Emit(OpCode, MethodInfo)
             * ILGenerator.EmitCall(OpCode, MethodInfo, Type[])
             */
            if (optionals == null || optionals.Length == 0)
                il.Emit(OpCodes.Call, method);
            else
                il.EmitCall(OpCodes.Call, method, optionals);
        }
        public void CallVirtual(MethodInfo method, Type[] optionals = null)
        {
            /* 对对象调用后期绑定方法，并且将返回值推送到计算堆栈上。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.callvirt(v=vs.100).aspx
             * 
             * 堆栈转换行为依次为：
             * 1. 将对象引用 obj 推送到堆栈上。
             * 2. 将从 arg1 到 argN 的方法参数推送到堆栈上。
             * 3. 从堆栈中弹出从 arg1 到 argN 的方法参数和对象引用 obj；通过这些参数执行方法调用并将控制转移到由方法元数据标记引用的 obj 中的方法。 完成后，被调用方方法生成返回值并将其发送给调用方。 
             * 4. 将返回值推送到堆栈上。
             * 
             * callvirt 指令对对象调用后期绑定方法。 也就是说，方法的选择是基于 obj 的运行时类型，而不是在方法指针中可见的编译时类。可以使用 Callvirt 来调用虚拟和实例方法。
             * tail 前缀可以紧靠 callvirt 指令之前 (Tailcall)，以指定在转移控制前应释放当前堆栈帧。如果调用将控制转移到比原始方法信任级别更高的方法，将不释放堆栈帧。
             * 
             * 方法元数据标记提供要调用的方法的名称、类和签名。与 obj 关联的类是属于实例的类。
             * 如果该类定义匹配指示的方法名称和签名的非静态方法，则调用该方法。否则按顺序检查此类的基类链中的所有类。如果未找到任何方法，则是错误的。
             * 
             * Callvirt 在调用方法前从计算堆栈中弹出该对象和关联的参数。
             * 如果该方法具有返回值，则在方法完成后将该返回值推送到堆栈上。
             * 在被调用方，obj 参数被作为参数 0 访问，arg1 被作为参数 1 访问，依此类推。
             * 
             * 将参数按从左到右的顺序放到堆栈上。即，先计算第一个参数并将其放到堆栈上，然后处理第二个参数，接着处理第三个参数，直到将所有需要的参数都按降序放置在堆栈的顶部为止。
             * 必须在任何用户可见参数前推送实例引用 obj（callvirt 始终需要的）。签名（在元数据标记中携带）不需要在参数列表中为该指针包含项。
             * 
             * 请注意，还可以使用 Call 指令调用虚方法。
             * 
             * 如果在与 obj 关联的类中或其任何基类中未能找到具有指示名称和签名的非静态方法，则引发 MissingMethodException。
             * 此异常通常是在将 Microsoft 中间语言 (MSIL) 指令转换为本机代码时而不是在运行时进行检测。
             * 
             * 如果 obj 为空，则引发 NullReferenceException。
             * 
             * 如果系统安全机制没有授予调用方对被调用方法的访问权限，则引发 SecurityException。 在 CIL 被转换为本机代码时（而不是在运行时），可能进行安全检查。
             * 
             * 对值类型调用 System.Object 的方法时，考虑使用 constrained 前缀和 callvirt 指令。
             * 因而不需要根据值类型是否重写方法来发送不同 IL，从而避免了潜在的版本问题。
             * 对值类型调用接口方法时考虑使用 constrained 前缀，因为实现接口方法的值类型方法可以用 MethodImpl 更改。
             * 这些问题在 Constrained 操作码里有更详细的说明。
             * 
             * 下面的 Emit 方法重载可以使用 callvirt 操作码：
             * ILGenerator.Emit(OpCode, MethodInfo)
             * ILGenerator.EmitCall(OpCode, MethodInfo, Type[])
             */
            if (optionals == null || optionals.Length == 0)
                il.Emit(OpCodes.Callvirt, method);
            else
                il.EmitCall(OpCodes.Callvirt, method, optionals);
        }

        public void Calling(CallingConventions convention, Type @return, Type[] parameters, Type[] optionals)
        {
            /* 通过调用约定描述的参数调用在计算堆栈上指示的方法（作为指向入口点的指针）。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.calli(v=vs.100).aspx
             */
            il.EmitCalli(OpCodes.Calli, convention, @return, parameters, optionals);
        }
        // public void CallingUnmanaged(CallingConvention convention, Type @return, Type[] parameters)
        // {
        //     il.EmitCalli(OpCodes.Calli, convention, @return, parameters);
        // }

        public void LoadArgument(int index)
        {
            switch (index)
            {
                case 0:
                    /* 将索引为 0 的参数加载到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldarg_0(v=vs.100).aspx
                     * 
                     * 堆栈转换行为依次为：
                     * 1. 将索引为 0 的参数值推送到堆栈上。
                     * 
                     * ldarg.0 指令是用于加载索引为 0 的参数值的有效编码。、
                     * 
                     * ldarg.0 指令将索引为 0 的参数推送到计算堆栈上。
                     * 可以使用 ldarg.0 指令通过从传入的参数中复制值类型或基元值来将它们加载到堆栈上。
                     * 参数值的类型与当前方法签名指定的参数类型相同。
                     * 
                     * 只能保存长度小于 4 个字节的整数值的参数在加载到堆栈上时被扩展为 int32 类型。
                     * 浮点值被扩展为它们的本机大小（F 类型）。
                     * 
                     * 下面的 Emit 方法重载可以使用 ldarg.0 操作码：
                     * ILGenerator.Emit(OpCode)
                     */
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    /* 将索引为 1 的参数加载到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldarg_1(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    /* 将索引为 2 的参数加载到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldarg_2(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    /* 将索引为 3 的参数加载到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldarg_3(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    {
                        if (index < 0 || index > 0xFFFF)
                            throw new ArgumentOutOfRangeException();

                        if (index <= 0xFF)
                            /* 将参数（由指定的短格式索引值引用）加载到计算堆栈上。
                             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldarg_s(v=vs.100).aspx
                             * 
                             * 堆栈转换行为依次为：
                             * 1. 将 index 处的参数值推送到堆栈上。
                             * 
                             * ldarg.s 指令是用于加载索引为从 4 到 255 的参数的有效编码。
                             * 
                             * ldarg.s 指令将索引为 index（从 0 向上对参数进行索引）的参数推送到计算堆栈上。
                             * 可以使用 ldarg.s 指令通过从传入的参数中复制值类型或基元值来将它们加载到堆栈上。 参数值的类型与当前方法签名指定的参数类型相同。
                             * 
                             * 对于采用变长参数列表的过程，ldarg.s 指令只能用于初始固定参数，而不能用于签名的可变部分中的参数（有关更多详细信息，请参见 Arglist 指令）。
                             * 
                             * 只能保存长度小于 4 个字节的整数值的参数在加载到堆栈上时被扩展为 int32 类型。 浮点值被扩展为它们的本机大小（F 类型）。
                             * 
                             * 下面的 Emit 方法重载可以使用 ldarg.s 操作码：
                             * ILGenerator.Emit(OpCode, byte)
                             */
                            il.Emit(OpCodes.Ldarg_S, (byte)index);
                        else
                            /* 将参数（由指定的长格式索引值引用）加载到计算堆栈上。
                             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldarg(v=vs.100).aspx
                             * 
                             * 堆栈转换行为依次为：
                             * 1. 将 index 处的参数值推送到堆栈上。
                             * 
                             * ldarg 指令将索引为 index（从 0 向上对参数进行索引）的参数推送到计算堆栈上。
                             * 可以使用 ldarg 指令通过从传入的参数中复制值类型或基元值来将它们加载到堆栈上。 参数值的类型与当前方法签名指定的参数类型相同。
                             * 
                             * 对于采用变长参数列表的过程，ldarg 指令只能用于初始固定参数，而不能用于签名的可变部分中的参数（有关更多详细信息，请参见 Arglist 指令）。
                             * 
                             * 只能保存长度小于 4 个字节的整数值的参数在加载到堆栈上时被扩展为 int32 类型。 浮点值被扩展为它们的本机大小（F 类型）。
                             * 
                             * 下面的 Emit 方法重载可以使用 ldarg 操作码：
                             * ILGenerator.Emit(OpCode, short)
                             */
                            il.Emit(OpCodes.Ldarg, (short)index);
                    }
                    break;
            }
        }
        public void LoadArgumentAddress(int index)
        {
            if (index < 0 || index > 0xFFFF)
                throw new ArgumentOutOfRangeException();

            if (index <= 0xFF)
                /* 以短格式将参数地址加载到计算堆栈上。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldarga_s(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将索引为 index 的参数的地址 addr 推送到堆栈上。
                 * 
                 * ldarga.s （ldarga 的短格式）应用于参数号 0 到 255，并且是效率更高的编码。
                 * 
                 * ldarga.s 指令获取索引为 index 的参数的地址（类型为 *），其中参数是从 0 向上进行索引的。 地址 addr 总是与目标计算机上的自然边界对齐。
                 * 
                 * 对于采用变长参数列表的过程，ldarga.s 指令只能用于初始固定参数，而不能用于签名的可变部分中的参数。
                 * 
                 * ldarga.s 用于按引用参数传递。 对于其他情况，应使用 Ldarg_S 和 Starg_S。
                 * 
                 * 下面的 Emit 方法重载可以使用 ldarga.s 操作码：
                 * ILGenerator.Emit(OpCode, byte)
                 */
                il.Emit(OpCodes.Ldarga_S, (byte)index);
            else
                /* 以长格式将参数地址加载到计算堆栈上。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldarga(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将索引为 index 的参数的地址 addr 推送到堆栈上。
                 * 
                 * ldarga 指令获取索引为 index 的参数的地址（类型为 *），其中参数是从 0 向上进行索引的。 地址 addr 总是与目标计算机上的自然边界对齐。 
                 * 
                 * 对于采用变长参数列表的过程，ldarga 指令只能用于初始固定参数，而不能用于签名的可变部分中的参数。
                 * 
                 * ldarga 用于按引用参数传递。 对于其他情况，应使用 Ldarg 和 Starg。
                 * 
                 * 下面的 Emit 方法重载可以使用 ldarga 操作码：
                 * ILGenerator.Emit(OpCode, short)
                 */
                il.Emit(OpCodes.Ldarga, (short)index);
        }

        public void LoadLocal(int index)
        {
            switch (index)
            {
                case 0:
                    /* 将索引 0 处的局部变量加载到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldloc_0(v=vs.100).aspx
                     * 
                     * 堆栈转换行为依次为：
                     * 1. 将索引 0 处的局部变量值推送到堆栈上。
                     * 
                     * ldloc.0 是对于 Ldloc 非常有效的编码，它允许访问索引 0 处的局部变量。
                     * 
                     * 该值的类型与局部变量的类型相同，后者是在方法头中指定的。
                     * 长度小于 4 个字节的局部变量在加载到堆栈上时会被扩展为 int32 类型。 浮点值被扩展为它们的本机大小（F 类型）。
                     * 
                     * 下面的 Emit 方法重载可以使用 ldloc.0 操作码：
                     * ILGenerator.Emit(OpCode)
                     */
                    il.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    /* 将索引 1 处的局部变量加载到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldloc_1(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    /* 将索引 2 处的局部变量加载到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldloc_2(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    /* 将索引 3 处的局部变量加载到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldloc_3(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    {
                        if (index < 0 || index > 0xFFFF)
                            throw new ArgumentOutOfRangeException();

                        if (index <= 0xFF)
                            /* 将特定索引处的局部变量加载到计算堆栈上（短格式）。
                             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldloc_s(v=vs.100).aspx
                             * 
                             * 堆栈转换行为依次为：
                             * 1. 将指定索引处的局部变量推送到堆栈上。
                             * 
                             * ldloc.s 指令将所传递的索引处的局部变量号的内容推送到计算堆栈上，其中局部变量从 0 开始向上进行编号。
                             * 如果方法上的初始化标志为真，则在输入方法前将局部变量初始化为 0。 采用短格式可能有 256 (2^8) 个 (0-255) 局部变量，它是比 ldloc 更有效的编码。
                             * 
                             * 该值的类型与局部变量的类型相同，后者是在方法头中指定的。 请参阅第一部分。
                             * 长度小于 4 个字节的局部变量在加载到堆栈上时会被扩展为 int32 类型。 浮点值被扩展为它们的本机大小（F 类型）。
                             * 
                             * 下面的 Emit 方法重载可以使用 ldloc.s 操作码：
                             * ILGenerator.Emit(OpCode, LocalBuilder)
                             * ILGenerator.Emit(OpCode, byte)
                             */
                            il.Emit(OpCodes.Ldloc_S, (byte)index);
                        else
                            /* 将指定索引处的局部变量加载到计算堆栈上。
                             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldloc(v=vs.100).aspx
                             * 
                             * 堆栈转换行为依次为：
                             * 1. 将指定索引处的局部变量推送到堆栈上。
                             * 
                             * ldloc 指令将所传递的索引处的局部变量号的内容推送到计算堆栈上，其中局部变量从 0 开始向上进行编号。
                             * 只有在方法上的初始化标志为真时，才在输入方法前将局部变量初始化为 0。
                             * 可能有 65,535 (2^16-1) 个局部变量 (0-65,534)。 因为实现很可能使用 2 字节整数来跟踪局部变量的索引以及给定方法的局部变量的总数，所以 65,535 无效。
                             * 如果已经使索引 65535 有效，则它将要求更大范围的整数以通过此方法跟踪局部变量号。
                             * 
                             * ldloc.0 、ldloc.1、ldloc.2 和 ldloc.3 指令为访问前四个局部变量提供有效的编码。
                             * 
                             * 该值的类型与局部变量的类型相同，后者是在方法头中指定的。 请参阅第一部分。
                             * 长度小于 4 个字节的局部变量在加载到堆栈上时会被扩展为 int32 类型。 浮点值被扩展为它们的本机大小（F 类型）。
                             * 
                             * 下面的 Emit 方法重载可以使用 ldloc 操作码：
                             * ILGenerator.Emit(OpCode, LocalBuilder)
                             * ILGenerator.Emit(OpCode, short)
                             */
                            il.Emit(OpCodes.Ldloc, (short)index);
                    }
                    break;
            }
        }
        public void LoadLocal(LocalBuilder local)
        {
            if (local == null)
                throw new ArgumentNullException();

            if (local.LocalIndex <= 0xFF)
                il.Emit(OpCodes.Ldloc_S, local);
            else
                il.Emit(OpCodes.Ldloc, local);
        }
        public void LoadLocalAddress(int index)
        {
            if (index < 0 || index > 0xFFFF)
                throw new ArgumentOutOfRangeException();

            if (index <= 0xFF)
                /* 将位于特定索引处的局部变量的地址加载到计算堆栈上（短格式）。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldloca_s(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将存储在指定索引处的局部变量中的地址推送到堆栈上。
                 * 
                 * ldloca.s 指令将所传递的索引处的局部变量号的地址推送到堆栈上，其中局部变量从 0 开始向上进行编号。
                 * 推送到堆栈上的值已经正确对齐，可以用于 Ldind_I 和 Stind_I 等指令。 结果是瞬态指针（* 类型）。
                 * 
                 * ldloca.s 指令为使用局部变量 0 到 255 提供有效的编码。
                 * 
                 * 下面的 Emit 方法重载可以使用 ldloca.s 操作码：
                 * ILGenerator.Emit(OpCode, byte)
                 */
                il.Emit(OpCodes.Ldloca_S, (byte)index);
            else
                /* 将位于特定索引处的局部变量的地址加载到计算堆栈上。
                 * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldloca(v=vs.100).aspx
                 * 
                 * 堆栈转换行为依次为：
                 * 1. 将存储在指定索引处的局部变量中的地址推送到堆栈上。
                 * 
                 * ldloca 指令将所传递的索引处的局部变量号的地址推送到堆栈上，其中局部变量从 0 开始向上进行编号。
                 * 推送到堆栈上的值已经正确对齐，可以用于 Ldind_I 和 Stind_I 等指令。 结果是瞬态指针（* 类型）。
                 * 
                 * 下面的 Emit 方法重载可以使用 ldloca 操作码：
                 * ILGenerator.Emit(OpCode, short)
                 */
                il.Emit(OpCodes.Ldloca, (short)index);
        }
        public void StoreLocal(int index)
        {
            switch (index)
            {
                case 0:
                    /* 从计算堆栈的顶部弹出当前值并将其存储到索引 0 处的局部变量列表中。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.stloc_0(v=vs.100).aspx
                     * 
                     * 堆栈转换行为依次为：
                     * 1. 将值从堆栈中弹出并放在索引 0 处的局部变量中。
                     * 
                     * stloc.0 指令从计算堆栈中弹出位于顶部的值，并将其移动到索引为 0 的局部变量中。 值的类型必须与当前方法的本地签名所指定的局部变量的类型匹配。
                     * 
                     * stloc.0 是用于将值存入局部变量 0 的非常有效的编码。
                     * 
                     * 在将值保存到只能容纳长度小于 4 个字节的整数值的局部变量中时，会在将该值从堆栈移动到局部变量中时将其截断。
                     * 将浮点值从其本机大小（F 类型）舍入到与该参数关联的大小。
                     * 
                     * 下面的 Emit 方法重载可以使用 stloc.0 操作码：
                     * ILGenerator.Emit(OpCode)
                     */
                    il.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    /* 从计算堆栈的顶部弹出当前值并将其存储到索引 1 处的局部变量列表中。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.stloc_1(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    /* 从计算堆栈的顶部弹出当前值并将其存储到索引 2 处的局部变量列表中。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.stloc_2(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    /* 从计算堆栈的顶部弹出当前值并将其存储到索引 3 处的局部变量列表中。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.stloc_3(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    {
                        if (index < 0 || index > 0xFFFF)
                            throw new ArgumentOutOfRangeException();

                        if (index <= 0xFF)
                            /* 从计算堆栈的顶部弹出当前值并将其存储在局部变量列表中的 index 处（短格式）。
                             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.stloc_s(v=vs.100).aspx
                             * 
                             * 堆栈转换行为依次为：
                             * 1. 从堆栈中弹出值并将其放在局部变量 index 中。
                             * 
                             * stloc.s 指令从计算堆栈中弹出位于顶部的值并将其移动到局部变量号 index 中，其中局部变量从 0 向上进行编号。
                             * 值的类型必须与当前方法的本地签名所指定的局部变量的类型匹配。
                             * 
                             * stloc.s 指令提供有效的编码以用于局部变量 0 到 255。
                             * 
                             * 在将值保存到只能容纳长度小于 4 个字节的整数值的局部变量中时，会在将该值从堆栈移动到局部变量中时将其截断。
                             * 将浮点值从其本机大小（F 类型）舍入到与该参数关联的大小。
                             * 
                             * 下面的 Emit 方法重载可以使用 stloc.s 操作码：
                             * ILGenerator.Emit(OpCode, LocalBuilder)
                             * ILGenerator.Emit(OpCode, byte)
                             */
                            il.Emit(OpCodes.Stloc_S, (byte)index);
                        else
                            /* 从计算堆栈的顶部弹出当前值并将其存储到指定索引处的局部变量列表中。
                             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.stloc(v=vs.100).aspx
                             * 
                             * 堆栈转换行为依次为：
                             * 1. 从堆栈中弹出值并将其放在局部变量 index 中。
                             * 
                             * stloc 指令从计算堆栈中弹出位于顶部的值并将其移动到局部变量号 index 中，其中局部变量从 0 向上进行编号。
                             * 值的类型必须与当前方法的本地签名所指定的局部变量的类型匹配。
                             * 
                             * 在将值保存到只能容纳长度小于 4 个字节的整数值的局部变量中时，会在将该值从堆栈移动到局部变量中时将其截断。
                             * 将浮点值从其本机大小（F 类型）舍入到与该参数关联的大小。
                             * 
                             * 正确的 Microsoft 中间语言 (MSIL) 指令要求 index 是有效局部索引。
                             * 对于 stloc 指令，index 必须介于范围 0 到 65534 之间（包括 0 和 65534），需特别注意的是，65535 是无效的。
                             * 不包括 65535 的原因是实际的：实现很可能将使用 2 字节整数跟踪两个局部的索引以及给定方法的局部索引的总数。
                             * 如果已经使索引 65535 有效，则它将要求更大范围的整数以通过此方法跟踪局部变量号。
                             * 
                             * 下面的 Emit 方法重载可以使用 stloc 操作码：
                             * ILGenerator.Emit(OpCode, LocalBuilder)
                             * ILGenerator.Emit(OpCode, short)
                             */
                            il.Emit(OpCodes.Stloc, (short)index);
                    }
                    break;
            }
        }
        public void StoreLocal(LocalBuilder local)
        {
            if (local == null)
                throw new ArgumentNullException();

            if (local.LocalIndex <= 0xFF)
                il.Emit(OpCodes.Stloc_S, local);
            else
                il.Emit(OpCodes.Stloc, local);
        }

        public void TailCall()
        {
            /* 执行后缀的方法调用指令，以便在执行实际调用指令前移除当前方法的堆栈帧。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.tailcall(v=vs.100).aspx
             * 
             * tail 前缀指令必须紧位于 Call、Calli 或 Callvirt 指令之前。
             * 它指示在执行调用指令前应移除当前方法的堆栈帧。
             * 它还暗指从随后调用返回的值也是由当前方法返回的值，并因此可以将该调用转换为跨方法跳转。
             * 
             * 该值堆栈必须为空，只是正由随后调用转移的参数除外。
             * 调用指令后面的指令必须是 ret。 因此唯一有效的代码序列是 tail. call（或 calli 或 callvirt）。
             * 正确的 Microsoft 中间语言 (MSIL) 指令不得分支到 call 指令，但它们可以分支到后面的 Ret。
             * 
             * 当控制从不受信任的代码转移到受信任的代码时，不能放弃当前帧，因为这将危害代码标识安全性。
             * .NET Framework 安全检查因此可以导致忽略 tail，剩下一个标准 Call 指令。
             * 同样，为了允许在调用返回后发生已同步区域的退出，当用于退出被标记为已同步的方法时忽略 tail 前缀。
             * 
             * 下面的 Emit 方法重载可以使用 tail 操作码：
             * ILGenerator.Emit(OpCode)
             */
            il.Emit(OpCodes.Tailcall);
        }

        public void LoadConst(byte value)
        {
            /* 将提供的 int8 值作为 int32 推送到计算堆栈上（短格式）。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldc_i4_s(v=vs.100).aspx
             */
            il.Emit(OpCodes.Ldc_I4_S, value);
        }
        public void LoadConst(int value)
        {
            switch (value)
            {
                case -1:
                    /* 将整数值 -1 作为 int32 推送到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldc_i4_m1(v=vs.100).aspx
                     */
                    il.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    /* 将整数值 0 作为 int32 推送到计算堆栈上。
                     */
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    /* 将整数值 1 作为 int32 推送到计算堆栈上。
                     */
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    /* 将整数值 2 作为 int32 推送到计算堆栈上。
                     */
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    /* 将整数值 3 作为 int32 推送到计算堆栈上。
                     */
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    /* 将整数值 4 作为 int32 推送到计算堆栈上。
                     */
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    /* 将整数值 5 作为 int32 推送到计算堆栈上。
                     */
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    /* 将整数值 6 作为 int32 推送到计算堆栈上。
                     */
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    /* 将整数值 7 作为 int32 推送到计算堆栈上。
                     */
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    /* 将整数值 8 作为 int32 推送到计算堆栈上。
                     */
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    /* 将所提供的 int32 类型的值作为 int32 推送到计算堆栈上。
                     * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldc_i4(v=vs.100).aspx
                     * 
                     * 堆栈转换行为依次为：
                     * 1. 将值 num 推送到堆栈上。
                     * 
                     * 请注意，对于整数 -128 到 127 有特殊的简短编码（并因此更有效），对于整数 -1 到 8 尤其有特殊的简短编码。
                     * 所有简短编码都会将 4 字节整数推入堆栈。 较长的编码用于 8 字节整数以及 4 和 8 字节浮点数，并且用于不适合短格式的 4 字节值。
                     * 
                     * 有三种方法可以将 8 字节整数常数推送到堆栈上:
                     * 1. 使用 Ldc_I8 指令用于必须以超过 32 位表示的常数。
                     * 2. 使用 Ldc_I4 指令（后跟 Conv_I8）用于需要 9 到 32 位的常数。
                     * 3. 使用短格式指令（后跟 Conv_I8）用于可以 8 位或更少位表示的常数。
                     * 
                     * 下面的 Emit 方法重载可以使用 ldc.i4 操作码：
                     * ILGenerator.Emit(OpCode, int)
                     */
                    il.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }
        public void LoadConst(long value)
        {
            /* 将所提供的 int64 类型的值作为 int64 推送到计算堆栈上。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldc_i8(v=vs.100).aspx
             * 
             * 下面的 Emit 方法重载可以使用 ldc.i8 操作码：
             * ILGenerator.Emit(OpCode, long)
             */
            il.Emit(OpCodes.Ldc_I8, value);
        }
        public void LoadConst(float value)
        {
            /* 将所提供的 float32 类型的值作为 F (float) 类型推送到计算堆栈上。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldc_r4(v=vs.100).aspx
             * 
             * 下面的 Emit 方法重载可以使用 ldc.r4 操作码：
             * ILGenerator.Emit(OpCode, single)
             */
            il.Emit(OpCodes.Ldc_R4, value);
        }
        public void LoadConst(double value)
        {
            /* 将所提供的 float64 类型的值作为 F (float) 类型推送到计算堆栈上。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldc_r8(v=vs.100).aspx
             * 
             * 下面的 Emit 方法重载可以使用 ldc.r8 操作码：
             * ILGenerator.Emit(OpCode, double)
             */
            il.Emit(OpCodes.Ldc_R8, value);
        }

        public void LoadNull()
        {
            /* 将空引用（O 类型）推送到计算堆栈上。
             * https://msdn.microsoft.com/zh-cn/library/system.reflection.emit.opcodes.ldnull(v=vs.100).aspx
             * 
             * 堆栈转换行为依次为：
             * 1. 将空对象引用推送到堆栈上。
             * 
             * ldnull 将空引用（O 类型）推送到堆栈上。 这用于在用数据填充位置前或在位置被拒绝时初始化位置。 
             * 
             * ldnull 提供与大小无关的空引用。
             * 
             * 下面的 Emit 方法重载可以使用 ldnull 操作码：
             * ILGenerator.Emit(OpCode)
             */
            il.Emit(OpCodes.Ldnull);
        }

        /* 此 API 支持 .NET Framework 基础结构，不适合在代码中直接使用。
         * 此指令为保留指令。
         * Prefix1
         * Prefix2
         * Prefix3
         * Prefix4
         * Prefix5
         * Prefix6
         * Prefix7
         * Prefixref
         */

        #region Convert

        public void ConvertSByte(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为有符号 int8 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I1_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为有符号 int8 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I1);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 int8，然后将其扩展（填充）为 int32。
                il.Emit(OpCodes.Conv_Ovf_I1);
            }
        }
        public void ConvertInt16(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为有符号 int16 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I2_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为有符号 int16 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I2);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 int16，然后将其扩展（填充）为 int32。
                il.Emit(OpCodes.Conv_I2);
            }
        }
        public void ConvertInt32(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为有符号 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I4_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为有符号 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I4);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 int32。
                il.Emit(OpCodes.Conv_I4);
            }
        }
        public void ConvertInt64(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为有符号 int64，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I8_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为有符号 int64，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I8);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 int64。
                il.Emit(OpCodes.Conv_I8);
            }
        }
        public void ConvertByte(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为 unsigned int8 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U1_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为 unsigned int8 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U1);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 unsigned int8，然后将其扩展为 int32。
                il.Emit(OpCodes.Conv_U1);
            }
        }
        public void ConvertUInt16(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为 unsigned int16 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U2_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为 unsigned int16 并将其扩展为 int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U2);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 unsigned int16，然后将其扩展为 int32。
                il.Emit(OpCodes.Conv_U2);
            }
        }
        public void ConvertUInt32(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为 unsigned int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U4_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为 unsigned int32，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U4);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 unsigned int32，然后将其扩展为 int32。
                il.Emit(OpCodes.Conv_U4);
            }
        }
        public void ConvertUInt64(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为 unsigned int64，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U8_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为 unsigned int64，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U8);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 unsigned int64，然后将其扩展为 int64。
                il.Emit(OpCodes.Conv_U8);
            }
        }

        public void ConvertSingle()
        {
            // 将位于计算堆栈顶部的值转换为 float32。
            il.Emit(OpCodes.Conv_R4);
        }
        public void ConvertDouble()
        {
            // 将位于计算堆栈顶部的值转换为 float64。
            il.Emit(OpCodes.Conv_R8);
        }

        public void ConvertNativeInt(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为有符号 native int，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为有符号 native int，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_I);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 native int。
                il.Emit(OpCodes.Conv_I);
            }
        }
        public void ConvertNativeIntUnsigned(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将位于计算堆栈顶部的无符号值转换为 unsigned native int，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U_Un);
                }
                else
                {
                    // 将位于计算堆栈顶部的有符号值转换为 unsigned native int，并在溢出时引发 System.OverflowException。
                    il.Emit(OpCodes.Conv_Ovf_U);
                }
            }
            else
            {
                // 将位于计算堆栈顶部的值转换为 unsigned native int，然后将其扩展为 native int。
                il.Emit(OpCodes.Conv_U);
            }
        }

        public void ConvertSingleFromInt()
        {
            // Convert unsigned integer to floating-point.
            // 将位于计算堆栈顶部的无符号整数值转换为 float32。
            il.Emit(OpCodes.Conv_R_Un);
        }

        #endregion

        public void Add(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将两个无符号整数值相加，执行溢出检查，并且将结果推送到计算堆栈上。
                    il.Emit(OpCodes.Add_Ovf_Un);
                }
                else
                {
                    // 将两个整数相加，执行溢出检查，并且将结果推送到计算堆栈上。
                    il.Emit(OpCodes.Add_Ovf);
                }
            }
            else
            {
                // 将两个值相加并将结果推送到计算堆栈上。
                il.Emit(OpCodes.Add);
            }
        }
        public void Subtract(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 从另一值中减去一个无符号整数值，执行溢出检查，并且将结果推送到计算堆栈上。
                    il.Emit(OpCodes.Sub_Ovf_Un);
                }
                else
                {
                    // 从另一值中减去一个整数值，执行溢出检查，并且将结果推送到计算堆栈上。
                    il.Emit(OpCodes.Sub_Ovf);
                }
            }
            else
            {
                // 从其他值中减去一个值并将结果推送到计算堆栈上。
                il.Emit(OpCodes.Sub);
            }
        }
        public void Multiply(bool overflow = false, bool unsigned = false)
        {
            if (overflow)
            {
                if (unsigned)
                {
                    // 将两个无符号整数值相乘，执行溢出检查，并将结果推送到计算堆栈上。
                    il.Emit(OpCodes.Mul_Ovf_Un);
                }
                else
                {
                    // 将两个整数值相乘，执行溢出检查，并将结果推送到计算堆栈上。
                    il.Emit(OpCodes.Mul_Ovf);
                }
            }
            else
            {
                // 将两个值相乘并将结果推送到计算堆栈上。
                il.Emit(OpCodes.Mul);
            }
        }
        public void Divide(bool unsigned = false)
        {
            if (unsigned)
            {
                // 两个无符号整数值相除并将结果 ( int32 ) 推送到计算堆栈上。
                il.Emit(OpCodes.Div_Un);
            }
            else
            {
                // 将两个值相除并将结果作为浮点（F 类型）或商（int32 类型）推送到计算堆栈上。
                il.Emit(OpCodes.Div);
            }
        }
        public void Modulo(bool unsigned = false)
        {
            if (unsigned)
            {
                // 将两个无符号值相除并将余数推送到计算堆栈上。
                il.Emit(OpCodes.Rem_Un);
            }
            else
            {
                // 将两个值相除并将余数推送到计算堆栈上。
                il.Emit(OpCodes.Rem);
            }
        }

        #endregion

        //
        // 摘要: 
        //     计算两个值的按位“与”并将结果推送到计算堆栈上。
        public static readonly OpCode And;
        //
        // 摘要: 
        //     返回指向当前方法的参数列表的非托管指针。
        public static readonly OpCode Arglist;
        //
        // 摘要: 
        //     如果两个值相等，则将控制转移到目标指令。
        public static readonly OpCode Beq;
        //
        // 摘要: 
        //     如果两个值相等，则将控制转移到目标指令（短格式）。
        public static readonly OpCode Beq_S;
        //
        // 摘要: 
        //     如果第一个值大于或等于第二个值，则将控制转移到目标指令。
        public static readonly OpCode Bge;
        //
        // 摘要: 
        //     如果第一个值大于或等于第二个值，则将控制转移到目标指令（短格式）。
        public static readonly OpCode Bge_S;
        //
        // 摘要: 
        //     当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令。
        public static readonly OpCode Bge_Un;
        //
        // 摘要: 
        //     当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令（短格式）。
        public static readonly OpCode Bge_Un_S;
        //
        // 摘要: 
        //     如果第一个值大于第二个值，则将控制转移到目标指令。
        public static readonly OpCode Bgt;
        //
        // 摘要: 
        //     如果第一个值大于第二个值，则将控制转移到目标指令（短格式）。
        public static readonly OpCode Bgt_S;
        //
        // 摘要: 
        //     当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令。
        public static readonly OpCode Bgt_Un;
        //
        // 摘要: 
        //     当比较无符号整数值或不可排序的浮点型值时，如果第一个值大于第二个值，则将控制转移到目标指令（短格式）。
        public static readonly OpCode Bgt_Un_S;
        //
        // 摘要: 
        //     如果第一个值小于或等于第二个值，则将控制转移到目标指令。
        public static readonly OpCode Ble;
        //
        // 摘要: 
        //     如果第一个值小于或等于第二个值，则将控制转移到目标指令（短格式）。
        public static readonly OpCode Ble_S;
        //
        // 摘要: 
        //     当比较无符号整数值或不可排序的浮点型值时，如果第一个值小于或等于第二个值，则将控制转移到目标指令。
        public static readonly OpCode Ble_Un;
        //
        // 摘要: 
        //     当比较无符号整数值或不可排序的浮点值时，如果第一个值小于或等于第二个值，则将控制权转移到目标指令（短格式）。
        public static readonly OpCode Ble_Un_S;
        //
        // 摘要: 
        //     如果第一个值小于第二个值，则将控制转移到目标指令。
        public static readonly OpCode Blt;
        //
        // 摘要: 
        //     如果第一个值小于第二个值，则将控制转移到目标指令（短格式）。
        public static readonly OpCode Blt_S;
        //
        // 摘要: 
        //     当比较无符号整数值或不可排序的浮点型值时，如果第一个值小于第二个值，则将控制转移到目标指令。
        public static readonly OpCode Blt_Un;
        //
        // 摘要: 
        //     当比较无符号整数值或不可排序的浮点型值时，如果第一个值小于第二个值，则将控制转移到目标指令（短格式）。
        public static readonly OpCode Blt_Un_S;
        //
        // 摘要: 
        //     当两个无符号整数值或不可排序的浮点型值不相等时，将控制转移到目标指令。
        public static readonly OpCode Bne_Un;
        //
        // 摘要: 
        //     当两个无符号整数值或不可排序的浮点型值不相等时，将控制转移到目标指令（短格式）。
        public static readonly OpCode Bne_Un_S;

        //
        // 摘要: 
        //     无条件地将控制转移到目标指令。
        public static readonly OpCode Br;
        //
        // 摘要: 
        //     无条件地将控制转移到目标指令（短格式）。
        public static readonly OpCode Br_S;
        //
        // 摘要: 
        //     向 Common Language Infrastructure (CLI) 发出信号以通知调试器已撞上了一个断点。
        public static readonly OpCode Break;
        //
        // 摘要: 
        //     如果 value 为 false、空引用（Visual Basic 中的 Nothing）或零，则将控制转移到目标指令。
        public static readonly OpCode Brfalse;
        //
        // 摘要: 
        //     如果 value 为 false、空引用或零，则将控制转移到目标指令。
        public static readonly OpCode Brfalse_S;
        //
        // 摘要: 
        //     如果 value 为 true、非空或非零，则将控制转移到目标指令。
        public static readonly OpCode Brtrue;
        //
        // 摘要: 
        //     如果 value 为 true、非空或非零，则将控制转移到目标指令（短格式）。
        public static readonly OpCode Brtrue_S;


        //
        // 摘要: 
        //     比较两个值。如果这两个值相等，则将整数值 1 (int32) 推送到计算堆栈上；否则，将 0 (int32) 推送到计算堆栈上。
        public static readonly OpCode Ceq;
        //
        // 摘要: 
        //     比较两个值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
        public static readonly OpCode Cgt;
        //
        // 摘要: 
        //     比较两个无符号的或不可排序的值。如果第一个值大于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
        public static readonly OpCode Cgt_Un;
        //
        // 摘要: 
        //     如果值不是有限数，则引发 System.ArithmeticException。
        public static readonly OpCode Ckfinite;
        //
        // 摘要: 
        //     比较两个值。如果第一个值小于第二个值，则将整数值 1 (int32) 推送到计算堆栈上；反之，将 0 (int32) 推送到计算堆栈上。
        public static readonly OpCode Clt;
        //
        // 摘要: 
        //     比较无符号的或不可排序的值 value1 和 value2。如果 value1 小于 value2，则将整数值 1 (int32 ) 推送到计算堆栈上；反之，将
        //     0 ( int32 ) 推送到计算堆栈上。
        public static readonly OpCode Clt_Un;
        //
        // 摘要: 
        //     约束要对其进行虚方法调用的类型。
        public static readonly OpCode Constrained;





        //
        // 摘要: 
        //     将指定数目的字节从源地址复制到目标地址。
        public static readonly OpCode Cpblk;
        //
        // 摘要: 
        //     将位于对象（&、* 或 native int 类型）地址的值类型复制到目标对象（&、* 或 native int 类型）的地址。
        public static readonly OpCode Cpobj;

        //
        // 摘要: 
        //     复制计算堆栈上当前最顶端的值，然后将副本推送到计算堆栈上。
        public static readonly OpCode Dup;
        //
        // 摘要: 
        //     将控制从异常的 filter 子句转移回 Common Language Infrastructure (CLI) 异常处理程序。
        public static readonly OpCode Endfilter;
        //
        // 摘要: 
        //     将控制从异常块的 fault 或 finally 子句转移回 Common Language Infrastructure (CLI) 异常处理程序。
        public static readonly OpCode Endfinally;
        //
        // 摘要: 
        //     将位于特定地址的内存的指定块初始化为给定大小和初始值。
        public static readonly OpCode Initblk;
        //
        // 摘要: 
        //     将位于指定地址的值类型的每个字段初始化为空引用或适当的基元类型的 0。
        public static readonly OpCode Initobj;
        //
        // 摘要: 
        //     测试对象引用（O 类型）是否为特定类的实例。
        public static readonly OpCode Isinst;
        //
        // 摘要: 
        //     退出当前方法并跳至指定方法。
        public static readonly OpCode Jmp;



        //
        // 摘要: 
        //     按照指令中指定的类型，将指定数组索引中的元素加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem;
        //
        // 摘要: 
        //     将位于指定数组索引处的 native int 类型的元素作为 native int 加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_I;
        //
        // 摘要: 
        //     将位于指定数组索引处的 int8 类型的元素作为 int32 加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_I1;
        //
        // 摘要: 
        //     将位于指定数组索引处的 int16 类型的元素作为 int32 加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_I2;
        //
        // 摘要: 
        //     将位于指定数组索引处的 int32 类型的元素作为 int32 加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_I4;
        //
        // 摘要: 
        //     将位于指定数组索引处的 int64 类型的元素作为 int64 加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_I8;
        //
        // 摘要: 
        //     将位于指定数组索引处的 float32 类型的元素作为 F 类型（浮点型）加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_R4;
        //
        // 摘要: 
        //     将位于指定数组索引处的 float64 类型的元素作为 F 类型（浮点型）加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_R8;
        //
        // 摘要: 
        //     将位于指定数组索引处的包含对象引用的元素作为 O 类型（对象引用）加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_Ref;
        //
        // 摘要: 
        //     将位于指定数组索引处的 unsigned int8 类型的元素作为 int32 加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_U1;
        //
        // 摘要: 
        //     将位于指定数组索引处的 unsigned int16 类型的元素作为 int32 加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_U2;
        //
        // 摘要: 
        //     将位于指定数组索引处的 unsigned int32 类型的元素作为 int32 加载到计算堆栈的顶部。
        public static readonly OpCode Ldelem_U4;
        //
        // 摘要: 
        //     将位于指定数组索引的数组元素的地址作为 & 类型（托管指针）加载到计算堆栈的顶部。
        public static readonly OpCode Ldelema;
        //
        // 摘要: 
        //     将指向实现特定方法的本机代码的非托管指针（native int 类型）推送到计算堆栈上。
        public static readonly OpCode Ldftn;


        //
        // 摘要: 
        //     将 native int 类型的值作为 native int 间接加载到计算堆栈上。
        public static readonly OpCode Ldind_I;
        //
        // 摘要: 
        //     将 int8 类型的值作为 int32 间接加载到计算堆栈上。
        public static readonly OpCode Ldind_I1;
        //
        // 摘要: 
        //     将 int16 类型的值作为 int32 间接加载到计算堆栈上。
        public static readonly OpCode Ldind_I2;
        //
        // 摘要: 
        //     将 int32 类型的值作为 int32 间接加载到计算堆栈上。
        public static readonly OpCode Ldind_I4;
        //
        // 摘要: 
        //     将 int64 类型的值作为 int64 间接加载到计算堆栈上。
        public static readonly OpCode Ldind_I8;
        //
        // 摘要: 
        //     将 float32 类型的值作为 F (float) 类型间接加载到计算堆栈上。
        public static readonly OpCode Ldind_R4;
        //
        // 摘要: 
        //     将 float64 类型的值作为 F (float) 类型间接加载到计算堆栈上。
        public static readonly OpCode Ldind_R8;
        //
        // 摘要: 
        //     将对象引用作为 O（对象引用）类型间接加载到计算堆栈上。
        public static readonly OpCode Ldind_Ref;
        //
        // 摘要: 
        //     将 unsigned int8 类型的值作为 int32 间接加载到计算堆栈上。
        public static readonly OpCode Ldind_U1;
        //
        // 摘要: 
        //     将 unsigned int16 类型的值作为 int32 间接加载到计算堆栈上。
        public static readonly OpCode Ldind_U2;
        //
        // 摘要: 
        //     将 unsigned int32 类型的值作为 int32 间接加载到计算堆栈上。
        public static readonly OpCode Ldind_U4;


        //
        // 摘要: 
        //     将从零开始的、一维数组的元素的数目推送到计算堆栈上。
        public static readonly OpCode Ldlen;


        //
        // 摘要: 
        //     将地址指向的值类型对象复制到计算堆栈的顶部。
        public static readonly OpCode Ldobj;


        //
        // 摘要: 
        //     推送对元数据中存储的字符串的新对象引用。
        public static readonly OpCode Ldstr;
        //
        // 摘要: 
        //     将元数据标记转换为其运行时表示形式，并将其推送到计算堆栈上。
        public static readonly OpCode Ldtoken;
        //
        // 摘要: 
        //     将指向实现与指定对象关联的特定虚方法的本机代码的非托管指针（native int 类型）推送到计算堆栈上。
        public static readonly OpCode Ldvirtftn;
        //
        // 摘要: 
        //     退出受保护的代码区域，无条件将控制转移到特定目标指令。
        public static readonly OpCode Leave;
        //
        // 摘要: 
        //     退出受保护的代码区域，无条件将控制转移到目标指令（缩写形式）。
        public static readonly OpCode Leave_S;
        //
        // 摘要: 
        //     从本地动态内存池分配特定数目的字节并将第一个分配的字节的地址（瞬态指针，* 类型）推送到计算堆栈上。
        public static readonly OpCode Localloc;
        //
        // 摘要: 
        //     将对特定类型实例的类型化引用推送到计算堆栈上。
        public static readonly OpCode Mkrefany;

        //
        // 摘要: 
        //     对一个值执行求反并将结果推送到计算堆栈上。
        public static readonly OpCode Neg;
        //
        // 摘要: 
        //     将对新的从零开始的一维数组（其元素属于特定类型）的对象引用推送到计算堆栈上。
        public static readonly OpCode Newarr;
        //
        // 摘要: 
        //     创建一个值类型的新对象或新实例，并将对象引用（O 类型）推送到计算堆栈上。
        public static readonly OpCode Newobj;
        //
        // 摘要: 
        //     如果修补操作码，则填充空间。尽管可能消耗处理周期，但未执行任何有意义的操作。
        public static readonly OpCode Nop;
        //
        // 摘要: 
        //     计算堆栈顶部整数值的按位求补并将结果作为相同的类型推送到计算堆栈上。
        public static readonly OpCode Not;
        //
        // 摘要: 
        //     计算位于堆栈顶部的两个整数值的按位求补并将结果推送到计算堆栈上。
        public static readonly OpCode Or;
        //
        // 摘要: 
        //     移除当前位于计算堆栈顶部的值。
        public static readonly OpCode Pop;



        //
        // 摘要: 
        //     指定后面的数组地址操作在运行时不执行类型检查，并且返回可变性受限的托管指针。
        public static readonly OpCode Readonly;
        //
        // 摘要: 
        //     检索嵌入在类型化引用内的类型标记。
        public static readonly OpCode Refanytype;
        //
        // 摘要: 
        //     检索嵌入在类型化引用内的地址（& 类型）。
        public static readonly OpCode Refanyval;





        //
        // 摘要: 
        //     再次引发当前异常。
        public static readonly OpCode Rethrow;
        //
        // 摘要: 
        //     将整数值左移（用零填充）指定的位数，并将结果推送到计算堆栈上。
        public static readonly OpCode Shl;
        //
        // 摘要: 
        //     将整数值右移（保留符号）指定的位数，并将结果推送到计算堆栈上。
        public static readonly OpCode Shr;
        //
        // 摘要: 
        //     将无符号整数值右移（用零填充）指定的位数，并将结果推送到计算堆栈上。
        public static readonly OpCode Shr_Un;
        //
        // 摘要: 
        //     将提供的值类型的大小（以字节为单位）推送到计算堆栈上。
        public static readonly OpCode Sizeof;
        //
        // 摘要: 
        //     将位于计算堆栈顶部的值存储到位于指定索引的参数槽中。
        public static readonly OpCode Starg;
        //
        // 摘要: 
        //     将位于计算堆栈顶部的值存储在参数槽中的指定索引处（短格式）。
        public static readonly OpCode Starg_S;
        //
        // 摘要: 
        //     用计算堆栈中的值替换给定索引处的数组元素，其类型在指令中指定。
        public static readonly OpCode Stelem;
        //
        // 摘要: 
        //     用计算堆栈上的 native int 值替换给定索引处的数组元素。
        public static readonly OpCode Stelem_I;
        //
        // 摘要: 
        //     用计算堆栈上的 int8 值替换给定索引处的数组元素。
        public static readonly OpCode Stelem_I1;
        //
        // 摘要: 
        //     用计算堆栈上的 int16 值替换给定索引处的数组元素。
        public static readonly OpCode Stelem_I2;
        //
        // 摘要: 
        //     用计算堆栈上的 int32 值替换给定索引处的数组元素。
        public static readonly OpCode Stelem_I4;
        //
        // 摘要: 
        //     用计算堆栈上的 int64 值替换给定索引处的数组元素。
        public static readonly OpCode Stelem_I8;
        //
        // 摘要: 
        //     用计算堆栈上的 float32 值替换给定索引处的数组元素。
        public static readonly OpCode Stelem_R4;
        //
        // 摘要: 
        //     用计算堆栈上的 float64 值替换给定索引处的数组元素。
        public static readonly OpCode Stelem_R8;
        //
        // 摘要: 
        //     用计算堆栈上的对象 ref 值（O 类型）替换给定索引处的数组元素。
        public static readonly OpCode Stelem_Ref;




        //
        // 摘要: 
        //     在所提供的地址存储 native int 类型的值。
        public static readonly OpCode Stind_I;
        //
        // 摘要: 
        //     在所提供的地址存储 int8 类型的值。
        public static readonly OpCode Stind_I1;
        //
        // 摘要: 
        //     在所提供的地址存储 int16 类型的值。
        public static readonly OpCode Stind_I2;
        //
        // 摘要: 
        //     在所提供的地址存储 int32 类型的值。
        public static readonly OpCode Stind_I4;
        //
        // 摘要: 
        //     在所提供的地址存储 int64 类型的值。
        public static readonly OpCode Stind_I8;
        //
        // 摘要: 
        //     在所提供的地址存储 float32 类型的值。
        public static readonly OpCode Stind_R4;
        //
        // 摘要: 
        //     在所提供的地址存储 float64 类型的值。
        public static readonly OpCode Stind_R8;
        //
        // 摘要: 
        //     存储所提供地址处的对象引用值。
        public static readonly OpCode Stind_Ref;


        //
        // 摘要: 
        //     将指定类型的值从计算堆栈复制到所提供的内存地址中。
        public static readonly OpCode Stobj;


        //
        // 摘要: 
        //     实现跳转表。
        public static readonly OpCode Switch;

        //
        // 摘要: 
        //     引发当前位于计算堆栈上的异常对象。
        public static readonly OpCode Throw;
        //
        // 摘要: 
        //     指示当前位于计算堆栈上的地址可能没有与紧接的 ldind、stind、ldfld、stfld、ldobj、stobj、initblk 或 cpblk 指令的自然大小对齐。
        public static readonly OpCode Unaligned;


        //
        // 摘要: 
        //     指定当前位于计算堆栈顶部的地址可以是易失的，并且读取该位置的结果不能被缓存，或者对该地址的多个存储区不能被取消。
        public static readonly OpCode Volatile;
        //
        // 摘要: 
        //     计算位于计算堆栈顶部的两个值的按位异或，并且将结果推送到计算堆栈上。
        public static readonly OpCode Xor;
    }
}
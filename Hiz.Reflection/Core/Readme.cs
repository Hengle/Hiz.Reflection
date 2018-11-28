using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiz.Reflection
{
    /* ParameterInfo
     * https://msdn.microsoft.com/zh-cn/library/system.reflection.parameterinfo(v=vs.100).aspx
     * 
     * 参数修饰:
     * 
     * ref TParameter: (Visual Basic: ByRef) // 不能协变 传入参数类型必须一样;
     * ParameterInfo: {
     *   IsIn = false; // 不会自动添加: System.Runtime.InteropServices.InAttribute;
     *   IsOut = false;
     *   Attributes = None;
     *   GetCustomAttributes(false): { };
     *   ParameterType = TParameter& (IsIsClass = true) {
     *     IsByRef = true;
     *     HasElementType = true;
     *     GetElementType() = typeof(TParameter);
     *   }
     * }
     * 
     * out TParameter: (Visual Basic: 无等效项) // 不能协变 传入参数类型必须一样;
     * ParameterInfo: {
     *   IsIn = false;
     *   IsOut = true;
     *   Attributes = Out;
     *   GetCustomAttributes(false): { System.Runtime.InteropServices.OutAttribute }; // 自动添加
     *   ParameterType = TParameter& (IsIsClass = true) {
     *     IsIsClass = true;
     *     IsByRef = true;
     *     HasElementType = true;
     *     GetElementType() = typeof(TParameter);
     *   }
     * }
     * 
     * TParameter: (Visual Basic: ByVal) // 不加修饰 // 支持协变传入参数
     * ParameterInfo: {
     *   IsIn = false;
     *   IsOut = false;
     *   Attributes = None;
     *   GetCustomAttributes(false): { };
     *   ParameterType = TParameter {
     *     IsByRef = false;
     *   }
     * }
     */

    /* PropertyIndexer
     * https://msdn.microsoft.com/zh-cn/library/6x16t2tx.aspx
     * 
     * Indexer:
     * 默认情况下，C# 索引器在元数据中显示为具有“Item”名称的索引属性。
     * 但是，类库开发人员可以使用 IndexerNameAttribute 特性来更改元数据中索引器的名称。
     * 例如，String 类具有一个名为 Chars 的索引器。
     * 使用 C# 以外的语言创建的索引属性可以具有“Item”以外的名称。
     * 
     * 索引器值不属于变量；因此，不能将索引器值作为 ref 或 out 参数进行传递。
     * 
     * 索引器必须为实例成员。
     */

    /* CustomReflectionContext
     * https://msdn.microsoft.com/zh-cn/library/system.reflection.context.customreflectioncontext(v=vs.110).aspx
     */

    /* 协变/逆变
     * 
     * 协变: 派生类转基类; 隐式转换; 转换一定成功;
     * 逆变: 基类转派生类; 显式转换; 转换可能失败; 比如 基类变量 实际存储派生实例 才能转换成功;
     */

    /* MemberType 成员类型: Property
     * PropertyType 属性类型
     * DeclaringType 直接声明该成员所属的类型: 基类 / 派生(如果派生类重载此成员)
     * ReflectedType 
     */

    /* 对象构造器: Constructor
     * 成员访问器: Accessor
     * 属性索引器: Indexer
     * 方法调用器: Invoker
     */

    /* 利用表达式树构建委托改善反射性能
     * http://www.cnblogs.com/lemontea/archive/2013/02/04/2891281.html
     */

    /* 通用方法签名:
     * object MethodBase.Invoke(object instance, object[] parameters);
     * 
     * 参考文献:
     * http://www.cnblogs.com/JeffreyZhao/archive/2008/11/24/invoke-method-by-lambda-expression.html
     */

    /* Type.MakeByRefType()
     * https://msdn.microsoft.com/zh-cn/library/system.type.makebyreftype(v=vs.100).aspx
     */

    /* 图解C#的值类型，引用类型，栈，堆，ref，out
     * http://www.cnblogs.com/lemontea/p/3159282.html
     */
}

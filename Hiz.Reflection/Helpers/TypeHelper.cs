using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hiz.Reflection
{
    public static class TypeHelper
    {
        /* 显式/隐式 自定义转换操作符
         * 不允许进行以基类为转换源或目标的用户定义转换. (包括 Object)
         */

        public static bool IsDelegate(this Type type)
        {
            return type.IsSubclassOf(typeof(MulticastDelegate));
        }

        // 判断两个类型是否可以引用赋值: 派生类型赋给基类 / 引用类型赋给接口;
        static bool AreReferenceAssignable(Type target, Type source)
        {
            /* Type.IsAssignableFrom(c)
             * https://msdn.microsoft.com/zh-cn/library/system.type.isassignablefrom(v=vs.100).aspx
             * 
             * 如果满足下列任一条件:
             * 1. c 和当前 Type 表示同一类型: this == other;
             * 2. 当前 Type 位于 c 的继承层次结构中: c : this;
             * 3. 当前 Type 是 c 实现的接口: c : IThis;
             * 4. c 表示一个值类型，并且当前实例表示 Nullable<c>: typeof(int?).IsAssignableFrom(typeof(int)) == true;
             * 5. c 是泛型类型参数且当前 Type 表示 c 的约束之一: 
             * void Test() {
             *     var type = typeof(Stream);
             *     var generic = typeof(GenericWithConstraint<>);
             *     var c = generic.GetGenericArguments()[0]; // T
             *     // type.IsAssignableFrom(c) == true;
             * }
             * class GenericWithConstraint<T> where T : Stream { }
             */
            if (AreEquivalent(target, source))
                return true;
            if (!target.IsValueType && !source.IsValueType && target.IsAssignableFrom(source))
                return true;
            return false;
        }

        // 判断两个类型是否相等
        static bool AreEquivalent(Type type, Type other)
        {
            /* Type.IsEquivalentTo() // 仅仅针对 COM 对象;
             * https://msdn.microsoft.com/zh-cn/library/system.type.isequivalentto(v=vs.100).aspx
             * 
             * 从 .NET Framework 4 版 开始，公共语言运行时支持将 COM 类型的类型信息直接嵌入到托管程序集中，而不是要求托管程序集从 interop 程序集获取 COM 类型的类型信息。
             * 由于嵌入的类型信息只包含托管程序集实际所使用的类型和成员，因此两个托管程序集可能会具有相同 COM 类型的截然不同的视图。
             * 每个托管程序集使用不同的 Type 对象来表示各自的 COM 类型视图。
             * 公共语言运行时支持这些不同视图之间的类型等效性，这些类型包括接口、结构、枚举和委托。
             * 
             * 类型等效性意味着，在两个托管程序集之间传递的 COM 对象在接收程序集中可以转换为适当的托管类型。
             * IsEquivalentTo 方法使程序集可以确定从另一个程序集获得的 COM 对象的 COM 标识与第一个程序集自己的嵌入式互操作类型之一相同，因此可以强制转换为该类型。
             * 
             * 有关更多信息，请参见 类型等效性和嵌入的互操作类型。
             * https://msdn.microsoft.com/zh-cn/library/dd997297(v=vs.100).aspx
             * 
             * System.Type:
             * public virtual bool IsEquivalentTo(Type other) {
             *     return this == other;
             * }
             * 
             * System.RuntimeType (Internal):
             * public override bool IsEquivalentTo(Type other) {
             *     var runtime = other as RuntimeType;
             *     if (runtime != null) {
             *         return this == runtime || RuntimeTypeHandle.IsEquivalentTo(this, runtime);
             *     }
             *     return false;
             * }
             * 
             * System.RuntimeTypeHandle:
             * [SecuritySafeCritical]
             * [MethodImpl(MethodImplOptions.InternalCall)]
             * internal static extern bool IsEquivalentTo(RuntimeType rtType1, RuntimeType rtType2);
             */
            if (type == null)
                throw new ArgumentNullException();
            return type == other || type.IsEquivalentTo(other);
        }

        // 获得显式指定特性 (排除编译自动生成特性)
        public static AttributeCollection GetExplicitAttributes(PropertyDescriptor property)
        {
            /* 代码来自: System.ComponentModel.DataAnnotations.ValidationAttributeStore.TypeStoreItem.GetExplicitAttributes();
             * 
             * PropertyDescriptor.Attributes = 属性对应类型所申明的特性 + 属性自己所附加的特性;
             * 例如:
             * [TestOne("FromType")]
             * class Model {
             *     [TestTwo("FromProperty")]
             *     Model Property1 { get; set; }
             *     
             *     [TestOne("ReplaceType")]
             *     [TestTwo("FromProperty")]
             *     Model Property2 { get; set; }
             * }
             * [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
             * TestOneAttribute : Attribute {
             *     public TestOneAttribute(string name) { }
             * }
             * [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
             * TestTwoAttribute : Attribute {
             *     public TestTwoAttribute(string name) { }
             * }
             * 
             * 结果:
             * Property1.Attributes = { TestOne("FromType"), TestTwo("FromProperty") };
             * Property2.Attributes = { TestOne("ReplaceType"), TestTwo("FromProperty") };
             * 
             * 调用方法之后:
             * (Property1) => { TestTwo("FromProperty") };
             * (Property1) => { TestOne("ReplaceType"), TestTwo("FromProperty") };
             */
            var attributes = property.Attributes.Cast<Attribute>().ToList();
            var removed = false;
            foreach (Attribute a in TypeDescriptor.GetAttributes(property.PropertyType)) // 获取自动特性
            {
                var i = attributes.Count;
                while (--i >= 0) // 从后往前移除
                {
                    // We must use ReferenceEquals since attributes could Match if they are the same.
                    // Only ReferenceEquals will catch actual duplications.
                    if (object.ReferenceEquals(a, attributes[i]))
                    {
                        attributes.RemoveAt(i);
                        removed = true;

                        // attributes 会存在多个相同实例吗? 如果没有, 此处应该 break 开始新的一轮查询;
                        // break;
                    }
                }
            }
            return removed ? new AttributeCollection(attributes.ToArray()) : property.Attributes;
        }

        /* 成员定义继承关系: (.Net 4.5)
         * System.Reflection.MemberInfo
         * |-> System.Type (Abstract)
         *     |=> System.Reflection.TypeInfo (Abstract)
         *         |=> System.RuntimeType (Internal)
         * |-> MethodBase
         *     |-> ConstructorInfo
         *         |-> RuntimeConstructorInfo (Internal)
         *     |-> MethodInfo
         *         |-> RuntimeMethodInfo (Internal)
         * |-> FieldInfo
         *     |-> RuntimeFieldInfo (Internal)
         * |-> PropertyInfo (Abstract)
         *     |-> RuntimePropertyInfo (Internal)
         * |-> EventInfo
         *     |-> RuntimeEventInfo (Internal)
         */

        /* System.ComponentModel.MemberDescriptor
         * |=> EventDescriptor
         * |=> PropertyDescriptor
         * https://msdn.microsoft.com/zh-cn/library/system.componentmodel.memberdescriptor(v=vs.100).aspx
         * 
         * Category => CategoryAttribute; AttributeTargets = All;
         * Description => DescriptionAttribute; AttributeTargets = All;
         * DesignTimeOnly => DesignOnlyAttribute; AttributeTargets = All;
         * DisplayName => DisplayNameAttribute; AttributeTargets = Class|Method|Property|Event;
         * IsBrowsable => BrowsableAttribute; AttributeTargets = All;
         * 
         * TypeDescriptor.GetProperties(type); 返回结果不含 OnlySet 属性;
         */

        /* ICustomTypeDescriptor
         * https://msdn.microsoft.com/zh-cn/library/system.componentmodel.icustomtypedescriptor(v=vs.100).aspx
         */

        #region Attribute 特性相关

        /* [Flags]
         * public enum AttributeTargets
         * {
         *     // 可以对程序集应用特性。
         *     // 例如: [assembly: AssemblyVersion("1.0.0.0")]
         *     Assembly = 0x01,
         * 
         *     // 可以对模块应用特性。
         *     // 例如: [module: DefaultCharSet(CharSet.Unicode)]
         *     Module = 0x02,
         * 
         *     // 可以对类应用特性。
         *     Class = 0x04,
         * 
         *     // 可以对结构应用特性，即值类型。
         *     Struct = 0x08,
         * 
         *     // 可以对枚举应用特性。
         *     Enum = 0x10,
         * 
         *     // 可以对构造函数应用特性。
         *     Constructor = 0x20,
         * 
         *     // 可以对方法应用特性。
         *     Method = 0x40,
         * 
         *     // 可以对属性应用特性。
         *     Property = 0x80,
         * 
         *     // 可以对字段应用特性。
         *     Field = 0x0100,
         * 
         *     // 可以对事件应用特性。
         *     Event = 0x0200,
         * 
         *     // 可以对接口应用特性。
         *     Interface = 0x0400,
         * 
         *     // 可以对参数应用特性。
         *     Parameter = 0x0800,
         * 
         *     // 可以对委托应用特性。
         *     Delegate = 0x1000,
         * 
         *     // 可以对返回值应用特性。
         *     ReturnValue = 0x2000,
         * 
         *     // 可以对泛型参数应用特性。
         *     GenericParameter = 0x4000,
         * 
         *     // 可以对任何应用程序元素应用特性。
         *     All = 0x7FFF,
         * }
         */

        /* System.RuntimeType { // inherit 有用
         *     public override object[] GetCustomAttributes(bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)typeof(object), inherit);
         *     }
         *     public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)attributeType.UnderlyingSystemType, inherit);
         *     }
         * }
         * 
         * System.Reflection.RuntimeMethodInfo { // inherit 有用
         *     public override object[] GetCustomAttributes(bool inherit) {
         *         CustomAttribute.GetCustomAttributes(this, (RuntimeType)typeof(object), inherit);
         *     }
         *     public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)attributeType.UnderlyingSystemType, inherit);
         *     }
         * }
         * 
         * System.Reflection.RuntimeConstructorInfo { // inherit 无用
         *     public override object[] GetCustomAttributes(bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)typeof(object));
         *     }
         *     public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)attributeType.UnderlyingSystemType);
         *     }
         * }
         * 
         * System.Reflection.RuntimePropertyInfo { // inherit 无用
         *     public override object[] GetCustomAttributes(bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)typeof(object));
         *     }
         *     public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)attributeType.UnderlyingSystemType);
         *     }
         * }
         * 
         * System.Reflection.RuntimeEventInfo { // inherit 无用
         *     public override object[] GetCustomAttributes(bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)typeof(object));
         *     }
         *     public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)attributeType.UnderlyingSystemType);
         *     }
         * }
         * 
         * System.Reflection.RuntimeFieldInfo { // inherit 无用
         *     public override object[] GetCustomAttributes(bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)typeof(object));
         *     }
         *     public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
         *         return CustomAttribute.GetCustomAttributes(this, (RuntimeType)attributeType.UnderlyingSystemType);
         *     }
         * }
         * 
         * inherit 有用: Type/Method;
         * inherit 无用: Constructor/Property/Event/Field;
         */

        /* System.Reflection.CustomAttribute (Internal) {
         *     internal static object[] GetCustomAttributes(RuntimeType type, RuntimeType caType, bool inherit);
         *     internal static object[] GetCustomAttributes(RuntimeMethodInfo method, RuntimeType caType, bool inherit);
         *     // 
         *     internal static object[] GetCustomAttributes(RuntimeConstructorInfo ctor, RuntimeType caType);
         *     internal static object[] GetCustomAttributes(RuntimePropertyInfo property, RuntimeType caType);
         *     internal static object[] GetCustomAttributes(RuntimeEventInfo e, RuntimeType caType);
         *     internal static object[] GetCustomAttributes(RuntimeFieldInfo field, RuntimeType caType);
         *     // 
         *     internal static object[] GetCustomAttributes(RuntimeParameterInfo parameter, RuntimeType caType);
         *     internal static object[] GetCustomAttributes(RuntimeAssembly assembly, RuntimeType caType);
         *     internal static object[] GetCustomAttributes(RuntimeModule module, RuntimeType caType);
         *     //
         *     //
         *     internal static bool IsDefined(RuntimeType type, RuntimeType caType, bool inherit);
         *     internal static bool IsDefined(RuntimeMethodInfo method, RuntimeType caType, bool inherit);
         *     //
         *     internal static bool IsDefined(RuntimeConstructorInfo ctor, RuntimeType caType);
         *     internal static bool IsDefined(RuntimePropertyInfo property, RuntimeType caType)
         *     internal static bool IsDefined(RuntimeEventInfo e, RuntimeType caType);
         *     internal static bool IsDefined(RuntimeFieldInfo field, RuntimeType caType);
         *     //
         *     internal static bool IsDefined(RuntimeParameterInfo parameter, RuntimeType caType);
         *     internal static bool IsDefined(RuntimeAssembly assembly, RuntimeType caType);
         *     internal static bool IsDefined(RuntimeModule module, RuntimeType caType);
         * }
         */

        /* System.Attribute
         * 
         * // TypeInfo/ConstructorInfo/MethodInfo/FieldInfo: 调用原类实现 IsDefined();
         * // PropertyInfo/EventInfo: 调用本类实现 InternalIsDefined(); // if (inherit) 遍历父级;
         * IsDefined(MemberInfo element, Type attributeType, bool inherit);
         * |=> IsDefined(MemberInfo element, Type attributeType); inherit = true;
         * 
         * // element.Member.MemberType == Method: 调用本类实现 InternalParamIsDefined();
         * // 其它情况调用原类实现: IsDefined();
         * IsDefined(ParameterInfo element, Type attributeType, bool inherit)
         * |=> IsDefined(ParameterInfo element, Type attributeType); inherit = true;
         * 
         * IsDefined(Assembly element, Type attributeType, bool inherit); // 调用原类实现 IsDefined();
         * |=> IsDefined(Assembly element, Type attributeType); inherit = true;
         * 
         * IsDefined(Module element, Type attributeType, bool inherit); // 调用原类实现 IsDefined();
         * |=> IsDefined(Module element, Type attributeType); inherit = false; // 异于 GetCustomAttributes();
         * 
         * 
         * // TypeInfo/ConstructorInfo/MethodInfo/FieldInfo: 调用原类实现 GetCustomAttributes();
         * // PropertyInfo/EventInfo: 调用本类实现 InternalGetCustomAttributes(); // if (inherit) 遍历父级查找可继承的特性;
         * GetCustomAttributes(MemberInfo element, Type type, bool inherit);
         * |=> GetCustomAttributes(MemberInfo element, Type type); inherit = true;
         * GetCustomAttributes(MemberInfo element, bool inherit); attributeType = typeof(Attribute);
         * |=> GetCustomAttributes(MemberInfo element); inherit = true;
         * 
         * // if (element.Member.MemberType == Method & inherit): InternalParamGetCustomAttributes();
         * // else: 调用原类实现: GetCustomAttributes();
         * GetCustomAttributes(ParameterInfo element, Type attributeType, bool inherit);
         * |=> GetCustomAttributes(ParameterInfo element, Type attributeType); inherit = true;
         * GetCustomAttributes(ParameterInfo element, bool inherit); attributeType = typeof(Attribute);
         * |=> GetCustomAttributes(ParameterInfo element); inherit = true;
         * 
         * System.Reflection.Assembly.GetCustomAttributes(Type attributeType, bool inherit);
         * |=> GetCustomAttributes(Assembly element, Type attributeType, bool inherit); // 调用原类实现
         *     |=> GetCustomAttributes(Assembly element, Type attributeType); inherit = true;
         * |=> GetCustomAttributes(Assembly element, bool inherit); attributeType = typeof(Attribute); // 调用原类实现
         *     |=> GetCustomAttributes(Assembly element); inherit = true;
         * 
         * System.Reflection.Module.GetCustomAttributes(Type attributeType, bool inherit);
         * |=> GetCustomAttributes(Module element, Type attributeType, bool inherit) // 调用原类实现
         *     |=> GetCustomAttributes(Module element, Type attributeType); inherit = true;
         * |=> GetCustomAttributes(Module element, bool inherit); attributeType = typeof(Attribute); // 调用原类实现
         *     |=> GetCustomAttributes(Module element); inherit = true;
         */

        /* 接口的特性并不会被实现类继承...(顾名思义, 类仅仅是实现接口, 并非继承.)
         * 
         * 基类的类型/构造函数/字段/属性/事件/方法, 全都附加特性, 那么:
         * 1. 派生类 直接继承基类, 没对成员进行任何重载:
         * typeof(TDerived).GetCustomAttributes(true) => 返回基类可继承的特性; false: 返回空集;
         * typeof(TDerived).Constructor.GetCustomAttributes(true/false) => 返回空集;
         * typeof(TDerived).Field/Property/Event/Method.GetCustomAttributes(true/false) => 返回基类所有特性(包括不可继承特性);
         * 2. 派生类, 重载属性/事件/方法(字段不可重载), 并且没有附加任何特性:
         * typeof(TDerived).GetCustomAttributes(true) => 返回基类可继承的特性; false: 返回空集;
         * typeof(TDerived).Constructor.GetCustomAttributes(true/false) => 返回空集;
         * typeof(TDerived).Method.GetCustomAttributes(true) => 返回基类可继承的特性; false: 返回空集;
         * typeof(TDerived).Property/Event.GetCustomAttributes(true/false) => 返回空集;
         * 3. 派生类, 重载属性/事件/方法, 并且没有附加基类相同特性:
         *    1). Type/Method 使用以下规则
         *    |====================================================================================|
         *    | Inherited | AllowMultiple | GetCustomAttributes(true) | GetCustomAttributes(false) |
         *    |===========|===============|===========================|============================|
         *    | True      | False         | Derived (Overwrite Base)  | Derived                    |
         *    | True      | True          | Base + Derived            | Only Derived               |
         *    | False     | False/True    | Derived                   | Derived                    |
         *    |=======================================================|============================|
         *    2). Constructor/Property/Event 使用以下规则
         *    typeof(TDerived).Constructor/Property/Event.GetCustomAttributes(true/false) => 返回派生特性;
         */

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiz.Reflection
{
    // 没有简化使用方式之前, 暂不公开.
    // TODO: 改进 ByRefWrapper

    /* 传入方法之前需初始化.
     * 
     * void TestOutput(out int value);
     * 转换: void Action<ByRef<int>>(ByRef<int> arg1);
     * 使用:
     * {
     *     int value;
     *     var byref = By.Out(out value);
     *     Action(byref);
     *     value = byref.Value; // NewValue;
     * }
     * 
     * void TestUpdate(ref int value);
     * 转换: void Action<ByRef<int>>(ByRef<int> arg1);
     * 使用:
     * {
     *     var value = 0xFF; // OldValue;
     *     var byref = By.Ref(ref value);
     *     Action(byref);
     *     value = byref.Value; // NewValue;
     * }
     * 
     * 
     * 改进目标: 不用额外实例 ByRefWrapper;
     * int value1 = 100;
     * int value2;
     * Action(By.Ref(ref value1), By.Out(out value));
     * // 方法调用之后变量将被更新;
     */

    // 包装 ByRef 参数;
    //class ByRef<T>
    //{
    //    // T* _Address;
    //    T _Value;
    //    public T Value
    //    {
    //        get { return _Value; }
    //        set { _Value = value; }
    //    }

    //    internal ByRef(T value)
    //    {
    //        this._Value = value;
    //    }

    //    // public ByRef()
    //    // {
    //    //     // this._Value = default(T);
    //    // }
    //    // public ByRef(ref T value)
    //    // {
    //    //     this._Value = value;
    //    // }

    //    /* Nullable<T> {
    //     *     public static implicit operator Nullable<T>(T value) { // 隐式转换
    //     *         return new Nullable<T>(value);
    //     *     }
    //     * 
    //     *     public static explicit operator T(Nullable<T> value) { // 显式转换
    //     *         return value.Value;
    //     *     }
    //     * }
    //     */
    //}

    //interface IByRefWrapper<T>
    //{
    //    T Value { get; set; }
    //}

    //static class By
    //{
    //    // public static ByRef<T> Ref<T>(T value)
    //    // {
    //    //     return new ByRef<T>(value);
    //    // }
    //    // public static ByRef<T> Out<T>()
    //    // {
    //    //     return new ByRef<T>(default(T));
    //    // }

    //    public static ByRef<T> Ref<T>(ref T value)
    //    {
    //        return new ByRef<T>(value);
    //    }

    //    public static ByRef<T> Out<T>(out T value)
    //    {
    //        value = default(T);
    //        return new ByRef<T>(value);
    //    }
    //}
}
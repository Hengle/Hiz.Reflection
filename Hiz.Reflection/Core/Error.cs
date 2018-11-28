using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiz.Reflection
{
    internal static class Error
    {
        internal static Exception StaticObjectCannotConvertType(string name)
        {
            return new ArgumentException("静态方法不能转换实例参数类型", name);
        }

        internal static Exception ArgumentNull(string name)
        {
            return new ArgumentNullException(name);
        }

        internal static Exception OnlyStaticMember(string name)
        {
            return new ArgumentException("仅限静态成员", name);
        }
        internal static Exception OnlyInstanceMember(string name)
        {
            return new ArgumentException("仅限实例成员", name);
        }

        internal static Exception FieldDoesNotHaveSetter(string name)
        {
            return new ArgumentException("字段不可赋值", name);
        }
        internal static Exception PropertyDoesNotHaveGetter(string name)
        {
            return new ArgumentException("属性不可读取", name);
        }
        internal static Exception PropertyDoesNotHaveSetter(string name)
        {
            return new ArgumentException("属性不可赋值", name);
        }
        internal static Exception IsNotIndexerProperty(string name)
        {
            return new ArgumentException("不是索引属性", name);
        }
    }
}

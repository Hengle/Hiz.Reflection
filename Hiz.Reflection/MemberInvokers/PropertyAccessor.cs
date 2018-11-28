using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hiz.Reflection
{
    // class PropertyAccessor : PropertyAccessor<object, object>
    // {
    // }
    // class PropertyAccessor<TObject> : PropertyAccessor<TObject, object>
    // {
    // }
    class PropertyAccessor<TObject, TProperty> where TObject : class
    {
        readonly bool _IsStatic;
        readonly Func<TObject, TProperty> _Getter;
        readonly Action<TObject, TProperty> _Setter;
        internal PropertyAccessor(bool @static, Func<TObject, TProperty> getter, Action<TObject, TProperty> setter)
        {
            this._IsStatic = @static;
            this._Getter = getter;
            this._Setter = setter;
        }

        // 用于实例
        public TProperty GetValue(TObject instance)
        {
            if (_Getter == null)
                throw new InvalidOperationException();
            if (_IsStatic)
                throw new InvalidOperationException();
            if (instance == null)
                throw new ArgumentNullException();

            return this._Getter(instance);
        }
        public void SetValue(TObject instance, TProperty value)
        {
            if (_Setter == null)
                throw new InvalidOperationException();
            if (_IsStatic)
                throw new InvalidOperationException();
            if (instance == null)
                throw new ArgumentNullException();

            this._Setter(instance, value);
        }

        // 用于静态
        public TProperty GetValue()
        {
            if (_Getter == null)
                throw new InvalidOperationException();
            if (!_IsStatic)
                throw new InvalidOperationException();

            return this._Getter(default(TObject));
        }
        public void SetValue(TProperty value)
        {
            if (_Setter == null)
                throw new InvalidOperationException();
            if (!_IsStatic)
                throw new InvalidOperationException();

            this._Setter(default(TObject), value);
        }
    }
}
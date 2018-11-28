using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiz.Reflection
{
    class FieldAccessor<TObject, TField> where TObject : class
    {
        readonly bool _IsStatic;
        readonly Func<TObject, TField> _Getter;
        readonly Action<TObject, TField> _Setter;
        FieldAccessor(bool @static, Func<TObject, TField> getter, Action<TObject, TField> setter)
        {
            this._IsStatic = @static;
            this._Getter = getter;
            this._Setter = setter;
        }

        // 用于实例
        public TField GetValue(TObject instance)
        {
            if (_Getter == null)
                throw new InvalidOperationException();
            if (_IsStatic)
                throw new InvalidOperationException();
            if (instance == null)
                throw new ArgumentNullException();

            return this._Getter(instance);
        }
        public void SetValue(TObject instance, TField value)
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
        public TField GetValue()
        {
            if (_Getter == null)
                throw new InvalidOperationException();
            if (!_IsStatic)
                throw new InvalidOperationException();

            return this._Getter(default(TObject));
        }
        public void SetValue(TField value)
        {
            if (_Setter == null)
                throw new InvalidOperationException();
            if (!_IsStatic)
                throw new InvalidOperationException();

            this._Setter(default(TObject), value);
        }
    }
}
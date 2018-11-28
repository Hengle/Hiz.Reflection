using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiz.Reflection
{
    class PropertyIndexer<TObject, TProperty>
    {
        readonly Func<TObject, object[], TProperty> _Getter;
        readonly Action<TObject, object[], TProperty> _Setter;
        PropertyIndexer(Func<TObject, object[], TProperty> getter,Action<TObject, object[], TProperty> setter)
        {
            this._Getter = getter;
            this._Setter = setter;
        }

        // 仅限实例
        public TProperty GetValue(TObject instance, object[] indexes)
        {
            if (_Getter == null)
                throw new InvalidOperationException();
            if (instance == null)
                throw new ArgumentNullException();

            return this._Getter(instance, indexes);
        }

        // 仅限实例
        public void SetValue(TObject instance, object[] indexes, TProperty value)
        {
            if (_Setter == null)
                throw new InvalidOperationException();
            if (instance == null)
                throw new ArgumentNullException();

            this._Setter(instance, indexes, value);
        }
    }
}
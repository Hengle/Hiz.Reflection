using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiz.Reflection
{
    class MethodInvoker<TObject, TResult> where TObject : class
    {
        readonly bool _IsStatic;
        readonly Func<TObject, object[], TResult> _Invoker;
        MethodInvoker(bool @static, Func<TObject, object[], TResult> invoker)
        {
            this._IsStatic = @static;
            this._Invoker = invoker;
        }

        public TResult Invoke(TObject instance, params object[] parameters)
        {
            if (_Invoker == null)
                throw new InvalidOperationException();

            if (!this._IsStatic)
            {
                if (instance == null)
                    throw new ArgumentNullException();

                return this._Invoker(instance, parameters);
            }
            else
            {
                // if (instance != null)
                //     throw new ArgumentException();

                return this._Invoker(default(TObject), parameters);
            }
        }
    }
}
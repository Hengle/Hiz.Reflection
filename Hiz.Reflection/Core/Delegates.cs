using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiz.Reflection
{
    /* Delegate for an Action< ref T1, T2>
     * https://stackoverflow.com/questions/2030303/delegate-for-an-action-ref-t1-t2
     */
    public delegate void RefAction<TObject>(ref TObject instance);
    public delegate void RefAction<TObject, in T1>(ref TObject instance, T1 arg1);
    public delegate void RefAction<TObject, in T1, in T2>(ref TObject instance, T1 arg1, T2 arg2);
    public delegate void RefAction<TObject, in T1, in T2, in T3>(ref TObject instance, T1 arg1, T2 arg2, T3 arg3);
    public delegate void RefAction<TObject, in T1, in T2, in T3, in T4>(ref TObject instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public delegate TResult RefFunc<TObject, out TResult>(ref TObject instance);
    public delegate TResult RefFunc<TObject, in T1, out TResult>(ref TObject instance, T1 arg1);
    public delegate TResult RefFunc<TObject, in T1, in T2, out TResult>(ref TObject instance, T1 arg1, T2 arg2);
    public delegate TResult RefFunc<TObject, in T1, in T2, in T3, out TResult>(ref TObject instance, T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult RefFunc<TObject, in T1, in T2, in T3, in T4, out TResult>(ref TObject instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}
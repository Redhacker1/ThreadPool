using System;

namespace BadThreadPool
{
    public class ThreadTaskRequest
    {

        /// <summary>
        /// Method the thread is supposed to call when created
        /// </summary>
        public Func<object> Method { get; }

        public readonly CallbackArgs<object> CallbackArgs;

        public object Result;

        public bool BHasRun = false;

        public readonly Action<CallbackArgs<object>> Callback;


        public ThreadTaskRequest(Func<object> method)
        {
            Method = method;
            Callback = null;
        }

        public ThreadTaskRequest(Func<object> method, Action<CallbackArgs<object>> callback)
        {
            Method = method;
            Callback = callback;
            CallbackArgs = new CallbackArgs<object>();
        }

        public ThreadTaskRequest(Func<object> method, Action<CallbackArgs<object>> callback, CallbackArgs<object> perams)
        {
            Method = method;
            Callback = callback;
            CallbackArgs = perams;
        }
    }
}

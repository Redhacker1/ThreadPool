using System.Collections.Generic;

namespace BadThreadPool
{
    public class CallbackArgs<T>
    {
        readonly Dictionary<string, object> _arguments = new Dictionary<string, object>();

        T _threadResult;

        public object GetThreadResult(out bool success)
        {

            if(_threadResult != null)
            {
                success = true;
                return _threadResult;
            }

            success = false;
            return null;
        }

        public void SetThreadResult(T result)
        {
            _threadResult = result;
        }

        public object GetArg(string name, out bool success)
        {
            try
            {
                success = true;
                return _arguments[name];
            }
            catch
            {
                success = false;
                return null;
            }
        }

        public void SetArg(string name, object item, out bool success)
        {
            try
            {
                success = true;
                _arguments[name] = item;
            }
            catch
            {
                success = false;
            }
        }
    }
}

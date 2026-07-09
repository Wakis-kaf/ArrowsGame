using Framework.Runtime.LogSystem;

using System;

namespace Framework.Utils
{
    public static class FunctionUtility
    {
        public static bool SafeCall(Action action, string preLog = "", string postLog = "", string errorLog = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(preLog)) Log.Info(preLog);
                action?.Invoke();
                if (!string.IsNullOrEmpty(postLog)) Log.Info(postLog);
                return true;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(postLog))
                    Log.Fatal($"call function error! message: {ex.Message}");
                else
                    Log.Fatal($"{errorLog} : {ex}");
                return false;
            }
        }

        public static bool SafeCall<T>(Action<T> action, T a)
        {
            try
            {
                action?.Invoke(a);
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal($"call function error!] message: {ex.Message} \n {ex.StackTrace}");
                return false;
            }
        }

        public static bool SafeCall<T, T2>(Action<T, T2> action, T a, T2 a2)
        {
            try
            {
                action?.Invoke(a, a2);
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal($"call function error!] message: {ex}");
                return false;
            }
        }

        public static bool SafeCall<T, T2, T3>(Action<T, T2, T3> action, T a, T2 a2, T3 a3)
        {
            try
            {
                action?.Invoke(a, a2, a3);
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal($"call function error!] message: {ex}");
                return false;
            }
        }

        public static bool SafeCall<T, T2, T3, T4>(Action<T, T2, T3, T4> action, T a, T2 a2, T3 a3, T4 a4)
        {
            try
            {
                action?.Invoke(a, a2, a3, a4);
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal($"call function error!] message: {ex}");
                return false;
            }
        }

        public static R SafeInvoke<R>(Func<R> func)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception ex)
            {
                Log.Fatal($"call function error!] message: {ex}");
            }
            return default;
        }

        public static R SafeInvoke<T, R>(Func<T, R> func, T a1)
        {
            try
            {
                return func.Invoke(a1);
            }
            catch (Exception ex)
            {
                Log.Fatal($"call function error!] message: {ex}");
            }
            return default;
        }

        public static R SafeInvoke<T, T2, R>(Func<T, T2, R> func, T a1, T2 a2)
        {
            try
            {
                return func.Invoke(a1, a2);
            }
            catch (Exception ex)
            {
                Log.Fatal($"call function error!] message: {ex}");
            }
            return default;
        }

        public static R SafeInvoke<T, T2, T3, R>(Func<T, T2, T3, R> func, T a1, T2 a2, T3 a3)
        {
            try
            {
                return func.Invoke(a1, a2, a3);
            }
            catch (Exception ex)
            {
                Log.Fatal($"call function error!] message: {ex}");
            }
            return default;
        }

        public static R SafeInvoke<T, T2, T3, T4, R>(Func<T, T2, T3, T4, R> func, T a1, T2 a2, T3 a3, T4 a4)
        {
            try
            {
                return func.Invoke(a1, a2, a3, a4);
            }
            catch (Exception ex)
            {
                Log.Fatal($"call function error!] message: {ex}");
            }
            return default;
        }
    }
}
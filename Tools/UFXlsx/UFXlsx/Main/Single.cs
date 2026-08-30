using System;

namespace UFXlsx.Main
{
    public class Single<T> where T : class, new()
    {
        private static T _mInstance;

        public static T Instance
        {
            get
            {
                if (_mInstance == null)
                {
                    _mInstance = Activator.CreateInstance<T>();
                }
                return _mInstance;
            }
        }
    }
}
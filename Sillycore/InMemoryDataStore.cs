using System.Collections.Generic;

namespace Sillycore
{
    public class InMemoryDataStore
    {
        protected Dictionary<string, object> Data = new Dictionary<string, object>();

        public void SetData(string key, object value)
        {
            Data.Add(key, value);
        }

        public object GetData(string key)
        {
            return Data.ContainsKey(key) ? Data[key] : null;
        }

        public T GetData<T>(string key)
        {
            return (T)GetData(key);
        }
    }
}
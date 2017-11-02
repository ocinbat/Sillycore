using System.Collections.Generic;

namespace Sillycore
{
    public class InMemoryDataStore
    {
        protected Dictionary<string, object> Data = new Dictionary<string, object>();

        public void Set(string key, object value)
        {
            if (Data.ContainsKey(key))
            {
                Data[key] = value;
            }
            else
            {
                Data.Add(key, value);
            }
        }

        public object Get(string key)
        {
            return Data.ContainsKey(key) ? Data[key] : null;
        }

        public T Get<T>(string key)
        {
            return (T)Get(key);
        }

        public void Delete(string key)
        {
            if (Data.ContainsKey(key))
            {
                Data.Remove(key);
            }
        }
    }
}
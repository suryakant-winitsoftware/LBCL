using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Winit.Modules.Common.Model.Classes
{
    public class DataManager : Interfaces.IDataManager
    {
        public Dictionary<string, object> Data { get; set; }
        public DataManager() {
            Data = new Dictionary<string, object>();
        }
        // Add or Update Data
        public void SetData(string key, object value)
        {
            Data[key] = value;
        }

        // Read Data
        public object GetData(string key)
        {
            if (Data.TryGetValue(key, out object value))
            {
                return value;
            }
            return null; // Key not found
        }

        // Delete Data
        public bool DeleteData(string key)
        {
            return Data.Remove(key);
        }
    }
}

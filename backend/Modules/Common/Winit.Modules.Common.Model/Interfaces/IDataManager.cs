using System;
using System.Collections.Generic;
using System.Text;

namespace Winit.Modules.Common.Model.Interfaces
{
    public interface IDataManager
    {
        Dictionary<string, object> Data { get; set; }
        public void SetData(string key, object value);
        public object GetData(string key);
        public bool DeleteData(string key);
    }
}

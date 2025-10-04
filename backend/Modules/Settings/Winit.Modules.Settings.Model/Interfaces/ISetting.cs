using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Setting.Model.Interfaces
{
    public interface ISetting:IBaseModel
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
        public bool IsEditable { get; set; }
    }
}

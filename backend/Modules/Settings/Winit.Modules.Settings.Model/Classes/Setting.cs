using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Setting.Model.Interfaces;

namespace Winit.Modules.Setting.Model.Classes
{
    public class Setting:BaseModel, ISetting
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
        public bool IsEditable { get; set; }
    }
}

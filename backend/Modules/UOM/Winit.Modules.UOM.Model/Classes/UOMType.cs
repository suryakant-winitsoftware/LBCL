using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.UOM.Model.Interfaces;

namespace Winit.Modules.UOM.Model.Classes
{
    public class UOMType : BaseModel, IUOMType
    {
        public string Name { get; set; }
        public string Label { get; set; }
       
    }
}

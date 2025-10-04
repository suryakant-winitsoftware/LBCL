using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKUClass.Model.Interfaces
{
    public interface ISKUClass:IBaseModel
    {
      
        public string CompanyUID { get; set; }
        public string ClassName { get; set; }
        public string Description { get; set; }
        public string ClassLabel { get; set; }
    }
}

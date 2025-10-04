using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Base.Model
{
    public class RestBaseModel : Base.Model.IRestBaseModel
    {
        
        public string ErrorMessage { get; set; }
        public bool IsValid { get; set; }
    }
}

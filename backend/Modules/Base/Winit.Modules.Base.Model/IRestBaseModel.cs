using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Base.Model
{
    public interface IRestBaseModel
    {
     
        public string ErrorMessage { get; set; }
        public bool IsValid { get; set; }
    }
}

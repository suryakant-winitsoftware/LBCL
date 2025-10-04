using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ErrorHandling.Model.Interfaces
{
    public interface IErrorDetailModel : IBaseModel
    {
        public string ErrorCode { get; set; }
        public int Severity { get; set; }
        public string Category { get; set; }
        public string Platform { get; set; }
        public string Module { get; set; }
        public string SubModule { get; set; }
      

    }
}

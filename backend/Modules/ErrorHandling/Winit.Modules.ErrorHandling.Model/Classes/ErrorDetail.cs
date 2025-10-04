using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ErrorHandling.Model.Interfaces;

namespace Winit.Modules.ErrorHandling.Model.Classes
{
    public class ErrorDetail : BaseModel, IErrorDetail
    {
        public string ErrorCode { get; set; }
        public int Severity { get; set; }
        public string Category { get; set; }
        public string Platform { get; set; }
        public string Module { get; set; }
        public string SubModule { get; set; }
        public string ShortDescription { get; set; }
        public string LanguageCode { get; set; }
        public string Description { get; set; }
        public string Cause { get; set; }
        public string Resolution { get; set; }
        public string DetailUID { get; set; }
    }
}

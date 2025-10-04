using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ErrorHandling.Model.Interfaces
{
    public interface IErrorDetail:IBaseModel
    {
        string ErrorCode { get; set; }
        int Severity { get; set; }
        string Category { get; set; }
        string Platform { get; set; }
        string Module { get; set; }
        string SubModule { get; set; }
        string ShortDescription { get; set; }
        string LanguageCode { get; set; }
        string Description { get; set; }
        string Cause { get; set; }
        string Resolution { get; set; }
        string DetailUID { get; set; }
    }
}

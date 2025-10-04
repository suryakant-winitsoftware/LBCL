using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface ISchemeExtendHistory : IBaseModel
    {
        string SchemeType { get; set; }
        string SchemeUid { get; set; }
        string ActionType { get; set; }
        DateTime? OldDate { get; set; }
        DateTime? NewDate { get; set; }
        string Comments { get; set; }
        string UpdatedByEmpUid { get; set; }
        string UpdatedBy { get; set; }
        DateTime? UpdatedOn { get; set; }
    }
}

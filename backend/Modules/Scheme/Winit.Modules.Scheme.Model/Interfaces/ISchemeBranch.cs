using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface ISchemeBranch : IBaseModel
    {
        string LinkedItemType { get; set; }
        string LinkedItemUID { get; set; }
        string BranchCode { get; set; }
    }
}

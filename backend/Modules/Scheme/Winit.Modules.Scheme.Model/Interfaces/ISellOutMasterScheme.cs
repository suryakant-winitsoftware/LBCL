using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Classes;

namespace Winit.Modules.Scheme.Model.Interfaces;

public interface ISellOutMasterScheme
{
    bool IsNew { get; set; }
    public ISellOutSchemeHeader? SellOutSchemeHeader { get; set; }
    public List<ISellOutSchemeLine>? SellOutSchemeLines { get; set; }
    public ApprovalRequestItem? ApprovalRequestItem { get; set; }
    public ApprovalStatusUpdate? ApprovalStatusUpdate { get; set; }
}

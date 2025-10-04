using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.AuditTrail.Model.Classes;

namespace Winit.Modules.AuditTrail.BL.Interfaces
{
    public interface IAuditTrailHelper
    {
        Task<bool> PublishAuditTrailEntry(IAuditTrailEntry auditTrailEntry);
        AuditTrailEntry CreateAuditTrailEntry(
        string linkedItemType, string linkedItemUID, string commandType,
        string docNo, string? jobPositionUID, string empUID, string empName,
        Dictionary<string, object> newData, string? originalDataId = null,
        List<ChangeLog>? changeData = null);
    }
}

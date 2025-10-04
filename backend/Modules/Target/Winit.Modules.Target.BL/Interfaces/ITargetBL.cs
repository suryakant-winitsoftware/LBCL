using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Target.Model.Classes;
using Winit.Modules.Target.Model.Interfaces;

namespace Winit.Modules.Target.BL.Interfaces
{
    public interface ITargetBL
    {
        Task<IEnumerable<ITarget>> GetAllTargetsAsync(TargetFilter filter);
        Task<ITarget> GetTargetByIdAsync(long id);
        Task<ITarget> CreateTargetAsync(ITarget target);
        Task<ITarget> UpdateTargetAsync(ITarget target);
        Task<bool> DeleteTargetAsync(long id);
        Task<IEnumerable<TargetSummary>> GetTargetSummaryAsync(string userLinkedUid, int year, int month);
        Task<PagedResult<ITarget>> GetPagedTargetsAsync(TargetFilter filter);
        Task<int> BulkCreateTargetsAsync(IEnumerable<ITarget> targets);
        Task<bool> ReplaceTargetsForPeriodAsync(string userLinkedUid, int year, int month, IEnumerable<ITarget> newTargets);
    }
}
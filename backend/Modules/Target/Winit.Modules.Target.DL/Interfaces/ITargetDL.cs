using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Target.Model.Classes;
using Winit.Modules.Target.Model.Interfaces;

namespace Winit.Modules.Target.DL.Interfaces
{
    public interface ITargetDL
    {
        Task<IEnumerable<ITarget>> GetAllTargetsAsync(TargetFilter filter);
        Task<ITarget> GetTargetByIdAsync(long id);
        Task<ITarget> CreateTargetAsync(ITarget target);
        Task<ITarget> UpdateTargetAsync(ITarget target);
        Task<bool> DeleteTargetAsync(long id);
        Task<int> BulkInsertTargetsAsync(IEnumerable<ITarget> targets);
        Task<IEnumerable<TargetSummary>> GetTargetSummaryAsync(string userLinkedUid, int year, int month);
        Task<bool> DeleteTargetsByFilterAsync(string userLinkedUid, string? customerUid, int year, int month);
        Task<IEnumerable<ITarget>> GetPagedTargetsAsync(TargetFilter filter);
        Task<int> GetTargetsCountAsync(TargetFilter filter);
    }
}
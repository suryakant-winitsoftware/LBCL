using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.Target.Model.Classes;
using Winit.Modules.Target.Model.Interfaces;
using Winit.Modules.Target.BL.Interfaces;
using Winit.Modules.Target.DL.Interfaces;

namespace Winit.Modules.Target.BL.Classes
{
    public class TargetBL : ITargetBL
    {
        private readonly ITargetDL _targetDL;

        public TargetBL(ITargetDL targetDL)
        {
            _targetDL = targetDL;
        }

        public async Task<IEnumerable<ITarget>> GetAllTargetsAsync(TargetFilter filter)
        {
            return await _targetDL.GetAllTargetsAsync(filter);
        }

        public async Task<ITarget> GetTargetByIdAsync(long id)
        {
            return await _targetDL.GetTargetByIdAsync(id);
        }

        public async Task<ITarget> CreateTargetAsync(ITarget target)
        {
            // Validate target
            ValidateTarget(target);
            
            // Set default customer type if not provided
            if (!string.IsNullOrEmpty(target.CustomerLinkedUid) && string.IsNullOrEmpty(target.CustomerLinkedType))
            {
                target.CustomerLinkedType = "Customer";
            }
            
            target.CreatedTime = DateTime.UtcNow;
            target.ModifiedTime = DateTime.UtcNow;
            
            return await _targetDL.CreateTargetAsync(target);
        }

        public async Task<ITarget> UpdateTargetAsync(ITarget target)
        {
            // Validate target
            ValidateTarget(target);
            
            // Set default customer type if not provided
            if (!string.IsNullOrEmpty(target.CustomerLinkedUid) && string.IsNullOrEmpty(target.CustomerLinkedType))
            {
                target.CustomerLinkedType = "Customer";
            }
            
            target.ModifiedTime = DateTime.UtcNow;
            
            return await _targetDL.UpdateTargetAsync(target);
        }

        public async Task<bool> DeleteTargetAsync(long id)
        {
            return await _targetDL.DeleteTargetAsync(id);
        }

        public async Task<IEnumerable<TargetSummary>> GetTargetSummaryAsync(string userLinkedUid, int year, int month)
        {
            return await _targetDL.GetTargetSummaryAsync(userLinkedUid, year, month);
        }

        public async Task<PagedResult<ITarget>> GetPagedTargetsAsync(TargetFilter filter)
        {
            var data = await _targetDL.GetPagedTargetsAsync(filter);
            var totalCount = await _targetDL.GetTargetsCountAsync(filter);
            
            return new PagedResult<ITarget>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<int> BulkCreateTargetsAsync(IEnumerable<ITarget> targets)
        {
            if (targets == null || !targets.Any())
            {
                throw new ArgumentException("No targets provided");
            }

            // Validate all targets and handle backward compatibility
            foreach (var target in targets)
            {
                ValidateTarget(target);
                
                // Set default customer type if not provided
                if (!string.IsNullOrEmpty(target.CustomerLinkedUid) && string.IsNullOrEmpty(target.CustomerLinkedType))
                {
                    target.CustomerLinkedType = "Customer";
                }
            }

            return await _targetDL.BulkInsertTargetsAsync(targets);
        }

        public async Task<bool> ReplaceTargetsForPeriodAsync(string userLinkedUid, int year, int month, IEnumerable<ITarget> newTargets)
        {
            // Delete existing targets for the period
            await _targetDL.DeleteTargetsByFilterAsync(userLinkedUid, null, year, month);
            
            // Insert new targets
            if (newTargets != null && newTargets.Any())
            {
                await _targetDL.BulkInsertTargetsAsync(newTargets);
            }
            
            return true;
        }


        private void ValidateTarget(ITarget target)
        {
            if (string.IsNullOrEmpty(target.UserLinkedType))
                throw new ArgumentException("UserLinkedType is required");
            
            if (string.IsNullOrEmpty(target.UserLinkedUid))
                throw new ArgumentException("UserLinkedUid is required");
            
            if (target.TargetMonth < 1 || target.TargetMonth > 12)
                throw new ArgumentException("TargetMonth must be between 1 and 12");
            
            if (target.TargetYear < 2020 || target.TargetYear > 2030)
                throw new ArgumentException("TargetYear must be between 2020 and 2030");
            
            if (target.TargetAmount < 0)
                throw new ArgumentException("TargetAmount cannot be negative");
        }

    }
}
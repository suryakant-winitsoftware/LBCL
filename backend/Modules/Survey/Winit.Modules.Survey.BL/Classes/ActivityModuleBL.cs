using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.BL.Interfaces;
using Winit.Modules.Survey.DL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.BL.Classes
{
    public class ActivityModuleBL : Interfaces.IActivityModuleBL
    {
        protected readonly IActivityModuleDL _activityModuleDL;
        public ActivityModuleBL(IActivityModuleDL activityModuleDL)
        {
            _activityModuleDL = activityModuleDL;
        }
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IActivityModule>> GetAllActivityModuleDeatils(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _activityModuleDL.GetAllActivityModuleDeatils(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
    }
}

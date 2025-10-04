using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.DL.Interfaces
{
    public interface IActivityModuleDL
    {
        Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IActivityModule>> GetAllActivityModuleDeatils(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    }
}

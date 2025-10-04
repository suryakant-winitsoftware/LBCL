using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.DL.Interfaces
{
    public interface IStoreAsmMappingDL
    {
        Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping>> SelectAllStoreAsmMapping(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<List<IStoreAsmMapping>> GetExistingCustomersList(List<IStoreAsmMapping> storeAsmMappings);
        Task<List<IStoreAsmMapping>> GetExistingEmpList(List<string> EmpCodes);
        Task<List<IStoreAsmMapping>> CheckBranchSameOrNot(List<IStoreAsmMapping> storeAsmMappings);
        Task<int> CUAsmMapping(List<IAsmDivisionMapping> asmDivisionMapping);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreAsmMappingBL
    {
        Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping>> SelectAllStoreAsmMapping(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<List<IStoreAsmMapping>> GetExistingCustomersList(List<IStoreAsmMapping> validRecords, List<IStoreAsmMapping> invalidRecords);
        Task<List<IStoreAsmMapping>> GetExistingEmpList(List<string> EmpCodes);
        Task<int> CUAsmMapping(List<IAsmDivisionMapping> asmDivisionMapping);
    }
}

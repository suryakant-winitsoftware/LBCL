using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Store.BL.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreAsmDivisionMappingViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<IAsmDivisionMapping> StoreAsmMappingGridViewRecords { get; set; }
        public List<StoreAsmMapping> StoreAsmMappingErrorRecords { get; set; }
        Task PopulateViewModel();
        Task OnFileUploadInsertIntoDB(List<StoreAsmMapping> schemeExcludeMappingRecords);
        Task OnFilterApply(Dictionary<string, string> keyValuePairs);
        Task OnSorting(SortCriteria sortCriteria);
        Task PageIndexChanged(int pageNumber);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISchemeExcludeMappingViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }
        public List<ISchemeExcludeMapping> SchemeExcludeGridViewRecords { get; set; }
        public List<SchemeExcludeMapping> SchemeExcludeErrorRecords { get; set; }
        Task PopulateViewModel();
        Task OnFileUploadInsertIntoDB(List<SchemeExcludeMapping> schemeExcludeMappingRecords);
        Task OnFilterApply(Dictionary<string, string> keyValuePairs);
        Task OnSorting(SortCriteria sortCriteria);
        Task PageIndexChanged(int pageNumber);
    }
}

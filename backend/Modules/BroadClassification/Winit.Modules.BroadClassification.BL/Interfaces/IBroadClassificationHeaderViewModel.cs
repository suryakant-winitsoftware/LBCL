using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Interfaces;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.BroadClassification.BL.Interfaces
{
    public interface IBroadClassificationHeaderViewModel:ITableGridViewModel
    {
        public List<IBroadClassificationHeader> broadClassificationHeaderslist { get; set; }
        public IBroadClassificationHeader viewBroadClassificationHeaderLineData { get; set; }
        Task PopulateViewModel();
        public List<IListItem> ClassificationTypes { get; set; }
        Task PopulateBroadClassificationHeaderDetailsByUID(string UID);
        Task<bool> CreateUpdateBroadClassificationHeaderData(IBroadClassificationHeader broadClassificationHeader, bool Operation);
        Task<string> DeleteBroadClassificationHeaderData(object uID);
        Task ApplyFilter(List<FilterCriteria> filterCriterias);
        //public int PageNumber { get; set; }
        //public int PageSize { get; set; }
        Task PageIndexChanged(int pageNumber);

    }
}

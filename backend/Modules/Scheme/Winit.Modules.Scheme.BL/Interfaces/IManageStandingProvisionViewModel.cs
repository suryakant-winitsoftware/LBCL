using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface IManageStandingProvisionViewModel
    {
        int PageNumber { get; set; }
        int PageSize { get; set; }
        int TotalItems { get; set; }
        Task OnPageChange(int pageNumber);
        List<IStandingProvisionScheme> StandingProvisionSchemes { get; set; }
        List<ISchemeExtendHistory> SchemeExtendHistories { get; set; }
        List<ISKUGroup> SKUGroup { get; set; }
        List<ISKUGroupType> SKUGroupType { get; set; }
        Task PopulateViewModel();
        Task OnFilterApply(Dictionary<string, string> filterCriteria);
        Task GetschemeExtendHistoryBySchemeUID(string standingProvisionUID);
    }
}

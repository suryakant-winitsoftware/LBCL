using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface IManageSchemeViewModel
    {
        bool StateRestored { get; set; }
        List<IManageScheme> ManageSchemes { get; set; }
        List<ISchemeExtendHistory> SchemeExtendHistories { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> ChannelPartner { get; set; }
        int TotalItems { get; set; }
        int PageSize { get; set; }
        int CurrentPage { get; set; }
        Task OnPageChange(int pageNumber);
        Task PopulateViewModel();
        Task OnFilterApply(Dictionary<string, string> filterCriteria);
        Task GetschemeExtendHistoryBySchemeUID(string schemeUID);
    }
}

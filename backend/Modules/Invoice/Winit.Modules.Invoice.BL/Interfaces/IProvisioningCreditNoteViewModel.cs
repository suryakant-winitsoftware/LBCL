using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Invoice.Model.Interfaces;

namespace Winit.Modules.Invoice.BL.Interfaces
{
    public interface IProvisioningCreditNoteViewModel
    {
        List<IProvisioningCreditNoteView> DisplayCreditNoteList { get; set; }
        Task LoadDataAsync(bool status = false);
        Task ApproveSelectedAsync();
        Task OnFilterApply(List<UIModels.Common.Filter.FilterModel> ColumnsForFilter, Dictionary<string, string> filterCriteria, string selectectab);
    }
}

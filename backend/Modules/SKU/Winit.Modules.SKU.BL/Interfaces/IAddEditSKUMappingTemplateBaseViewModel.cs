using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface IAddEditSKUMappingTemplateBaseViewModel
    {
        string SKUGroupLabel { get; set; }
        string SKUGroupTypeLabel { get; set; }
        SKUTemplate SKUTemplate { get; set; }
        List<SKUTemplateLine> SKUTemplateLineList { get; set; }
        List<ISelectionItem> SKUGroupsTypes { get; set; }
        List<ISelectionItem> SKUGroups { get; set; }
        bool IsAdd { get; set; }
        bool IsExcluded { get; set; }
        Task PopulateViewModel();
        Task AddMapping();
        Task SaveMapping();
        Task OnSKUGroupTypeSelected(DropDownEvent dropDownEvent);
        Task OnSKUGroupSelected(DropDownEvent dropDownEvent);
    }
}

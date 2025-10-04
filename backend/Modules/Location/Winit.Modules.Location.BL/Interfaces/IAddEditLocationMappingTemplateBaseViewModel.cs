using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Location.BL.Interfaces
{
    public interface IAddEditLocationMappingTemplateBaseViewModel
    {
        string Locationlabel {  get; set; }
        string LocationTypelabel {  get; set; }
        LocationTemplate LocationTemplate { get; set; }
        LocationTemplateLine LocationTemplateLine { get; set; }
        List<LocationTemplateLine> LocationTemplateLineList { get; set; }
        List<ISelectionItem> LocationTypesForDD { get; set; }
        List<ISelectionItem> Locations { get; set; }
        bool IsAdd { get; set; }
        bool IsExcluded { get; set; }
        Task PopulateViewModel();
        Task AddMapping();
        Task SaveMapping();
        Task OnLocationTypeSelected(DropDownEvent dropDownEvent);
        Task OnLocationSelected(DropDownEvent dropDownEvent);
    }
}

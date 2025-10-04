using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Location.BL.Interfaces
{
    public interface ILocationMasterBaseViewModel
    {
        string SearchField { get; set; }
        List<LocationData> LocationMasterForUIs { get; set; }
        List<Winit.Modules.Store.Model.Classes.StoreGroupData> DisplayStoreGroupData { get; set; }
        List<LocationData> DispayLocationMasterForUIs { get; set; }
        bool IsLocationHierarchyData { get; set; }
        string SelectedLocationOrStoreGroupUID { get; set; }
        dynamic objectData { get; set; }
        Task OnSeach(string searchField);
        Task PopulateViewModel();
        void OnSelected(LocationData locationMasterForUI);
        void OnSelected(StoreGroupData channelMasterData);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Vehicle.BL.Interfaces
{
    public  interface IAddEditMaintainVanViewModel
    {
      
        public List<SelectionItem> VanTypeSelectionItems { get; set; }
        public string OrgUID { get; set; }
        public IVehicle VEHICLE { get; set; }
        Task SaveUpdateVanItem(IVehicle vehicle, bool Iscreate);
        Task PopulateMaintainVanEditDetails(string vehicleuid);
        Task PopulateViewModel(string uid);

    }
}

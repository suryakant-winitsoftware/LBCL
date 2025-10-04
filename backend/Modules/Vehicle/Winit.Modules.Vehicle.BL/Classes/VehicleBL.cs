using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Vehicle.BL.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Vehicle.BL.Classes
{
    public class VehicleBL:IVehicleBL
    {
        protected readonly DL.Interfaces.IVehicleDL _VehicleBL = null;
        public VehicleBL(DL.Interfaces.IVehicleDL VehicleBL)
        {
            _VehicleBL = VehicleBL;
        }
        public async Task<PagedResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>> SelectAllVehicleDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias,bool isCountRequired,string OrgUID)
        {
            return await _VehicleBL.SelectAllVehicleDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, OrgUID);
        }
        public async Task<Winit.Modules.Vehicle.Model.Interfaces.IVehicle> GetVehicleByUID(string UID)
        {
            return await _VehicleBL.GetVehicleByUID(UID);
        }
        public async Task<int> CreateVehicle(Winit.Modules.Vehicle.Model.Interfaces.IVehicle createVehicle)
        {
            return await _VehicleBL.CreateVehicle(createVehicle);
        }
        public async Task<int> UpdateVehicleDetails(Winit.Modules.Vehicle.Model.Interfaces.IVehicle updateVehicle)
        {
            return await _VehicleBL.UpdateVehicleDetails(updateVehicle);
        }
        public async Task<int> DeleteVehicleDetails(string Code)
        {
            return await _VehicleBL.DeleteVehicleDetails(Code);
        }

        public async Task<List<IVehicleStatus>> GetAllVehicleStatusDetailsByEmpUID(string empUID)
        {
            return await _VehicleBL.GetAllVehicleStatusDetailsByEmpUID(empUID);
        }
    }
}

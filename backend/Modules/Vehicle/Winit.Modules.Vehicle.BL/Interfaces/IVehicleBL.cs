using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Vehicle.BL.Interfaces
{
    public interface IVehicleBL
    {
        Task<PagedResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>> SelectAllVehicleDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired,string OrgUID);
        Task<Winit.Modules.Vehicle.Model.Interfaces.IVehicle> GetVehicleByUID(string UID);
        Task<int> CreateVehicle(Winit.Modules.Vehicle.Model.Interfaces.IVehicle createVehicle);
        Task<int> UpdateVehicleDetails(Winit.Modules.Vehicle.Model.Interfaces.IVehicle updateVehicle);
        Task<int> DeleteVehicleDetails(string UID);
        Task<List<IVehicleStatus>> GetAllVehicleStatusDetailsByEmpUID(string empUID);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Vehicle.Model.Interfaces
{
    public interface IVehicle:IBaseModel
    {

        public string? CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string VehicleNo { get; set; }
        public string RegistrationNo { get; set; }
        public string Model { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime TruckSIDate { get; set; }
        public DateTime RoadTaxExpiryDate { get; set; }
        public DateTime InspectionDate { get; set; }

        // Additional fields for vehicle specifications
        public decimal? WeightLimit { get; set; }
        public decimal? Capacity { get; set; }
        public decimal? LoadingCapacity { get; set; }
        public string? WarehouseCode { get; set; }
        public string? LocationCode { get; set; }
        public string? TerritoryUID { get; set; }
    }
}

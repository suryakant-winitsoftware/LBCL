using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace Winit.Modules.DropDowns.BL.Interfaces
{
    public interface IDropDownsBL
    {
        Task<IEnumerable<ISelectionItem>> GetEmpDropDown(string orgUID, bool getDataByLoginId = false);
        Task<IEnumerable<ISelectionItem>> GetRouteDropDown(string orgUID);
        Task<IEnumerable<ISelectionItem>> GetVehicleDropDown(string parentUID);
        Task<IEnumerable<ISelectionItem>> GetRequestFromDropDown(string parentUID);
        Task<IEnumerable<ISelectionItem>> GetDistributorDropDown();
        Task<IEnumerable<ISelectionItem>> GetCustomerDropDown(string franchiseeOrgUID);
        Task<IEnumerable<ISelectionItem>> GetDistributorChannelDropDown(string parentUID);
        Task<IEnumerable<ISelectionItem>> GetWareHouseTypeDropDown(string parentUID);
        Task<IEnumerable<ISelectionItem>> GetCustomersByRouteUIDDropDown(string routeUID);
    }
}

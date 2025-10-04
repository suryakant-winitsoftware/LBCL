using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.DropDowns.BL.Classes
{
    public class DropDownsBL : Interfaces.IDropDownsBL
    {
        protected readonly Winit.Modules.DropDowns.DL.Interfaces.IDropDownsDL _dropDownsDL;
        public DropDownsBL(Winit.Modules.DropDowns.DL.Interfaces.IDropDownsDL dropDownsDL)
        {
            _dropDownsDL = dropDownsDL;
        }
        public async Task<IEnumerable<ISelectionItem>> GetEmpDropDown(string orgUID, bool getDataByLoginId = false)
        {
            return await _dropDownsDL.GetEmpDropDown(orgUID, getDataByLoginId);
        }
        public async Task<IEnumerable<ISelectionItem>> GetRouteDropDown(string orgUID)
        {
            return await _dropDownsDL.GetRouteDropDown(orgUID);
        }
        public async Task<IEnumerable<ISelectionItem>> GetVehicleDropDown(string parentUID)
        {
            return await _dropDownsDL.GetVehicleDropDown(parentUID);
        }
        public async Task<IEnumerable<ISelectionItem>> GetRequestFromDropDown(string parentUID)
        {
            return await _dropDownsDL.GetRequestFromDropDown(parentUID);
        }
        public async Task<IEnumerable<ISelectionItem>> GetDistributorDropDown()
        {
            return await _dropDownsDL.GetDistributorDropDown();
        }
        public async Task<IEnumerable<ISelectionItem>> GetCustomerDropDown(string franchiseeOrgUID)
        {
            return await _dropDownsDL.GetCustomerDropDown(franchiseeOrgUID);
        }
        public async Task<IEnumerable<ISelectionItem>> GetDistributorChannelDropDown(string parentUID)
        {
            return await _dropDownsDL.GetDistributorChannelDropDown(parentUID);
        }
        public async Task<IEnumerable<ISelectionItem>> GetWareHouseTypeDropDown(string parentUID)
        {
            return await _dropDownsDL.GetWareHouseTypeDropDown(parentUID);
        }
        public async Task<IEnumerable<ISelectionItem>> GetCustomersByRouteUIDDropDown(string routeUID)
        {
            return await _dropDownsDL.GetCustomersByRouteUIDDropDown(routeUID);
        }
    }
}

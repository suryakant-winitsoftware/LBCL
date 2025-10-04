using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Address.BL.Classes
{
    public class AddressBL : AddressBaseBL, Interfaces.IAddressBL
    {
        protected readonly DL.Interfaces.IAddressDL _addressDL = null;
        public AddressBL(DL.Interfaces.IAddressDL addressDL)
        {
            _addressDL = addressDL;
        }
        public async Task<PagedResponse<Winit.Modules.Address.Model.Interfaces.IAddress>> SelectAllAddressDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _addressDL.SelectAllAddressDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<Winit.Modules.Address.Model.Interfaces.IAddress> GetAddressDetailsByUID(string UID)
        {
            return await _addressDL.GetAddressDetailsByUID(UID);
        }
        public async Task<int> CreateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress createAddress)
        {
            return await _addressDL.CreateAddressDetails(createAddress);
        }
        public async Task<int> CreateAddressDetailsList(List<Winit.Modules.Address.Model.Interfaces.IAddress> createAddress)
        {
            return await _addressDL.CreateAddressDetailsList(createAddress);
        }

        public async Task<int> UpdateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress updateAddress)
        {
            return await _addressDL.UpdateAddressDetails(updateAddress);
        }
        public async Task<int> UpdateAddressDetails(string addressUID, string latitude, string longitude)
        {
            return await _addressDL.UpdateAddressDetails(addressUID, latitude, longitude);
        }
        public async Task<int> DeleteAddressDetails(String UID)
        {
            return await _addressDL.DeleteAddressDetails(UID);
        }
    }
}

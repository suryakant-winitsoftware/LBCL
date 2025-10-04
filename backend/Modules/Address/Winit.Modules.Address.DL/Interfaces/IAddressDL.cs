using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Address.DL.Interfaces
{
    public interface IAddressDL
    {
        Task<PagedResponse<Winit.Modules.Address.Model.Interfaces.IAddress>> SelectAllAddressDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Address.Model.Interfaces.IAddress> GetAddressDetailsByUID(string UID);
        Task<int> CreateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress createAddress);
        Task<int> CreateAddressDetailsList(List<Winit.Modules.Address.Model.Interfaces.IAddress> createAddress);
        Task<int> UpdateAddressDetails(Winit.Modules.Address.Model.Interfaces.IAddress updateAddress);
        Task<int> UpdateAddressDetails(string addressUID, string latitude, string longitude);
        Task<int> DeleteAddressDetails(String UID);
    }
}

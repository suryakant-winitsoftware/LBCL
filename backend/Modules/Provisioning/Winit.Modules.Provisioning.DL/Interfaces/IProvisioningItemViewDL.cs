using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.DL.Interfaces
{
    public interface IProvisioningItemViewDL
    {
        Task<IProvisionItemView> GetProvisioningLineItemDetailsByUID(string uID);
        Task<PagedResponse<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView>> SelectProvisioningLineItemsDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uid);
    }
}

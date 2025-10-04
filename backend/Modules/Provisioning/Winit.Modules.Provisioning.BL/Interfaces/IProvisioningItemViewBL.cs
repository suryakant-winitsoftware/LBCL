using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.BL.Interfaces
{
    public interface IProvisioningItemViewBL
    {
        Task<IProvisionItemView> GetProvisioningLineItemDetailsByUID(string uID);
        Task<PagedResponse<IProvisionItemView>> SelectProvisioningLineItemsDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uID);
    }
}

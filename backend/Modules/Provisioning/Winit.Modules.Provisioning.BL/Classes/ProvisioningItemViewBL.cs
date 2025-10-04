using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.BL.Classes
{
    public class ProvisioningItemViewBL : Interfaces.IProvisioningItemViewBL
    {
        protected readonly DL.Interfaces.IProvisioningItemViewDL _provisioningItemViewDL;
        public ProvisioningItemViewBL(DL.Interfaces.IProvisioningItemViewDL provisioningItemDL)
        {
            _provisioningItemViewDL = provisioningItemDL;
        }

        public Task<IProvisionItemView> GetProvisioningLineItemDetailsByUID(string uID)
        {
            return _provisioningItemViewDL.GetProvisioningLineItemDetailsByUID(uID);
        }

        public Task<PagedResponse<IProvisionItemView>> SelectProvisioningLineItemsDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uID)
        {
            return  _provisioningItemViewDL.SelectProvisioningLineItemsDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired , uID);
        }

        
    }
}

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.DL.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Provisioning.DL.Classes
{
    public class PGSQLProvisioningItemViewDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IProvisioningItemViewDL
    {
        public PGSQLProvisioningItemViewDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public Task<IProvisionItemView> GetProvisioningLineItemDetailsByUID(string uID)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<IProvisionItemView>> SelectProvisioningLineItemsDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string uid)
        {
            throw new NotImplementedException();
        }
        
    }
}

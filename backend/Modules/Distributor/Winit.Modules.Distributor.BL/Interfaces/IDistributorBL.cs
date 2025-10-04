using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Distributor.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Distributor.BL.Interfaces
{
    public interface IDistributorBL
    {
        Task<PagedResponse<Winit.Modules.Distributor.Model.Interfaces.IDistributor>> SelectAllDistributors(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> CreateDistributor(Winit.Modules.Distributor.Model.Classes.DistributorMasterView distributorMasterView);
        Task<DistributorMasterView> GetDistributorDetailsByUID(string UID);
    }
}

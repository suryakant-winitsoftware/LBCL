using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
//using static WINITRepository.Classes.Customers.PostgreSQLCustomerRepository;

namespace WINITServices.Interfaces
{
    public interface IOrgService
    {
        Task<IEnumerable<WINITSharedObjects.Models.Org>> GetOrgDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

        Task<WINITSharedObjects.Models.Org> GetOrgByOrgCode(string orgCode);

        Task<WINITSharedObjects.Models.Org> CreateOrg(Org createOrg);
        Task<int> UpdateOrg(WINITSharedObjects.Models.Org updateOrg);
        Task<int> DeleteOrg(string orgCode);
    }
}

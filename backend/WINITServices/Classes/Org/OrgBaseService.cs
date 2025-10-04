using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace WINITServices.Classes.Org
{
    public abstract class OrgBaseService : Interfaces.IOrgService
    {
        protected readonly WINITRepository.Interfaces.Org.IOrgRepository _orgRepository;
        public OrgBaseService(WINITRepository.Interfaces.Org.IOrgRepository orgRepository)
        {
            _orgRepository = orgRepository;
        }
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Org>> GetOrgDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias);

        public abstract Task<WINITSharedObjects.Models.Org> GetOrgByOrgCode(string orgCode);

        public abstract Task<WINITSharedObjects.Models.Org> CreateOrg(WINITSharedObjects.Models.Org createOrg);
        public abstract Task<int> UpdateOrg(WINITSharedObjects.Models.Org updateOrg);
        public abstract Task<int> DeleteOrg(string orgCode);

    }
}

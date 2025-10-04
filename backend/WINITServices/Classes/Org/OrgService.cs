using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WINITRepository.Interfaces.Products;
using WINITServices.Classes.Currency;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Settings.PostgreSQLSettingsRepository;

namespace WINITServices.Classes.Org
{
    public class OrgService : OrgBaseService
    {
        public OrgService(WINITRepository.Interfaces.Org.IOrgRepository orgRepository) : base(orgRepository)
        {

        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.Org>> GetOrgDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _orgRepository.GetOrgDetails(sortCriterias, pageNumber, pageSize, filterCriterias);
        }

        public async override Task<WINITSharedObjects.Models.Org> GetOrgByOrgCode(string orgCode)
        {
            return await _orgRepository.GetOrgByOrgCode(orgCode);
        }
        public async override Task<WINITSharedObjects.Models.Org> CreateOrg(WINITSharedObjects.Models.Org createOrg)
        {
            return await _orgRepository.CreateOrg(createOrg);
        }

        public async override Task<int> UpdateOrg(WINITSharedObjects.Models.Org updateOrg)
        {
            return await _orgRepository.UpdateOrg(updateOrg);
        }

        public async override Task<int> DeleteOrg(string orgCode)
        {
            return await _orgRepository.DeleteOrg(orgCode);
        }
    }
}

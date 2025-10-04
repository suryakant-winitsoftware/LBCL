using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface IStandingProvisionSchemeDL
    {
        Task<PagedResponse<IStandingProvisionScheme>> SelectAllStandingConfiguration(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> DeleteStandingConfiguration(string UID);
        Task<int> CUStandingProvisionScheme(IStandingProvisionSchemeMaster standingProvisionSchemeMaster);
        Task<IStandingProvisionSchemeMaster> GetStandingConfigurationMasterByUID(string UID);

        Task<int> UpdateStandingConfiguration(IStandingProvisionScheme standingProvision);
        Task<int> UpdateSchemeMappingData(string schemeUID)
        {
            throw new NotImplementedException();
        }
        Task<int> ChangeEndDate(IStandingProvisionScheme standingProvisionScheme)
        {
            throw new NotImplementedException();
        }
        Task<Dictionary<string, StandingSchemeResponse>> GetStandingSchemesByOrgUidAndSKUUid(string orgUid, DateTime orderDate, List<SKUFilter> skuFilterList)
        {
            throw new NotImplementedException();
        }
        Task<Dictionary<string, StandingSchemeResponse>> GetStandingSchemesByPOUid(string POUid, List<SKUFilter> skuFilterList)
        {
            return null;
        }
    }
}


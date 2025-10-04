using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;

using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Modules.Scheme.DL.Classes;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.Promotion.Model.Classes;

namespace Winit.Modules.Scheme.BL.Classes;

public class StandingProvisionSchemeBL : IStandingProvisionSchemeBL
{
    private readonly Winit.Modules.Scheme.DL.Interfaces.IStandingProvisionSchemeDL _standingProvisionSchemeDL;
    ISchemeBranchBL _schemeBranchBL { get; }
    ISchemeBroadClassificationBL _broadClassificationBL { get; }
    ISchemeOrgBL _schemeOrgBL { get; }
    public StandingProvisionSchemeBL(IStandingProvisionSchemeDL standingProvisionSchemeDL, ISchemeBranchBL schemeBranchBL, ISchemeBroadClassificationBL broadClassificationBL, ISchemeOrgBL schemeOrgBL)
    {
        _standingProvisionSchemeDL = standingProvisionSchemeDL;
        _schemeBranchBL = schemeBranchBL;
        _broadClassificationBL = broadClassificationBL;
        _schemeOrgBL = schemeOrgBL;
    }

    public async Task<PagedResponse<IStandingProvisionScheme>> SelectAllStandingConfiguration(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired)
    {
        return await _standingProvisionSchemeDL.SelectAllStandingConfiguration(
            sortCriterias,
            pageNumber,
            pageSize,
            filterCriterias,
            isCountRequired
        );
    }

    public async Task<int> DeleteStandingConfiguration(string UID)
    {
        return await _standingProvisionSchemeDL.DeleteStandingConfiguration(UID);
    }



    public async Task<int> CUStandingProvisionScheme(IStandingProvisionSchemeMaster standingProvisionSchemeMaster)
    {
        int cnt = 0;
        cnt += await _standingProvisionSchemeDL.CUStandingProvisionScheme(standingProvisionSchemeMaster);
        cnt += await _schemeBranchBL.CDBranches(standingProvisionSchemeMaster.SchemeBranches, standingProvisionSchemeMaster.StandingProvisionScheme.UID);
        cnt += await _broadClassificationBL.CDBroadClassification(standingProvisionSchemeMaster.SchemeBroadClassifications, standingProvisionSchemeMaster.StandingProvisionScheme.UID);
        cnt += await _schemeOrgBL.CDOrgs(standingProvisionSchemeMaster.SchemeOrgs, standingProvisionSchemeMaster.StandingProvisionScheme.UID);
        if (standingProvisionSchemeMaster.IsFinalApproval)
        {
            UpdateSchemeMappingData(standingProvisionSchemeMaster.StandingProvisionScheme.UID);
        }
        return cnt;
    }
    public async Task<IStandingProvisionSchemeMaster> GetStandingConfigurationMasterByUID(string UID)
    {
        IStandingProvisionSchemeMaster standingProvisionSchemeMaster =
         await _standingProvisionSchemeDL.GetStandingConfigurationMasterByUID(UID);
        standingProvisionSchemeMaster.SchemeBroadClassifications = await _broadClassificationBL.GetSchemeBroadClassificationByLinkedItemUID(UID);
        standingProvisionSchemeMaster.SchemeBranches = await _schemeBranchBL.GetSchemeBranchesByLinkedItemUID(UID);
        standingProvisionSchemeMaster.SchemeOrgs = await _schemeOrgBL.GetSchemeOrgByLinkedItemUID(UID);
        return standingProvisionSchemeMaster;
    }
    public async Task<int> UpdateStandingConfiguration(IStandingProvisionScheme standingProvision)
    {
        return await _standingProvisionSchemeDL.UpdateStandingConfiguration(standingProvision);
    }
    public async Task<Dictionary<string, StandingSchemeResponse>> GetStandingSchemesByOrgUidAndSKUUid(string orgUid, DateTime orderDate, List<SKUFilter> skuFilterList)
    {
        return await _standingProvisionSchemeDL.GetStandingSchemesByOrgUidAndSKUUid(orgUid, orderDate, skuFilterList);
    }
    public async Task<Dictionary<string, StandingSchemeResponse>> GetStandingSchemesByPOUid(string POUid, List<SKUFilter> skuFilterList)
    {
        return await _standingProvisionSchemeDL.GetStandingSchemesByPOUid(POUid, skuFilterList);
    }
    public async Task<int> ChangeEndDate(IStandingProvisionScheme standingProvisionScheme)
    {
        int cnt = await _standingProvisionSchemeDL.ChangeEndDate(standingProvisionScheme);
        if (cnt > 0)
        {
            UpdateSchemeMappingData(standingProvisionScheme.UID);
        }
        return cnt;
    }
    private async Task UpdateSchemeMappingData(string standingProvisionUID)
    {
        Task.Run(() => _standingProvisionSchemeDL.UpdateSchemeMappingData(standingProvisionUID));
    }
}

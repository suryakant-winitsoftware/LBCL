using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.util;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SellInSchemeBL : ISellInSchemeBL
    {
        ISellInSchemeDL _sellSchemeDL;
        IWalletDL _walletDL;
        ISchemeBranchBL _schemeBranchBL { get; }
        ISchemeBroadClassificationBL _broadClassificationBL { get; }
        ISchemeOrgBL _schemeOrgBL { get; }
        public SellInSchemeBL(ISellInSchemeDL sellSchemeDL, IWalletDL walletDL, ISchemeBranchBL schemeBranchBL, ISchemeBroadClassificationBL broadClassificationBL, ISchemeOrgBL schemeOrgBL)
        {
            _sellSchemeDL = sellSchemeDL;
            _walletDL = walletDL;
            _schemeBranchBL = schemeBranchBL;
            _broadClassificationBL = broadClassificationBL;
            _schemeOrgBL = schemeOrgBL;
        }
        public async Task<PagedResponse<ISellInSchemeHeader>> SelectAllSellInHeader(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _sellSchemeDL.SelectAllSellInHeader(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<ISellInSchemeLine>> SelectAllSellInDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _sellSchemeDL.SelectAllSellInDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<ISellInSchemeLine> GetSellInDetailByUID(string UID)
        {
            return await _sellSchemeDL.GetSellInDetailByUID(UID);
        }

        public async Task<int> CreateSellInDetail(ISellInSchemeLine sellInDetail)
        {
            return await _sellSchemeDL.CreateSellInDetail(sellInDetail);
        }

        public async Task<int> UpdateSellInDetail(ISellInSchemeLine sellInDetail)
        {
            return await _sellSchemeDL.UpdateSellInDetail(sellInDetail);
        }

        public async Task<int> DeleteSellInDetail(string UID)
        {
            return await _sellSchemeDL.DeleteSellInDetail(UID);
        }
        public async Task<int> DeleteSellInHeader(string UID)
        {
            return await _sellSchemeDL.DeleteSellInHeader(UID);
        }
        public async Task<int> UpdateSellInHeader(ISellInSchemeHeader SellInHeader)
        {
            return await _sellSchemeDL.UpdateSellInHeader(SellInHeader);
        }
        public async Task<int> CreateSellInHeader(ISellInSchemeHeader SellInHeader)
        {
            return await _sellSchemeDL.CreateSellInHeader(SellInHeader);
        }
        public async Task<ISellInSchemeHeader> GetSellInHeaderByUID(string UID)
        {
            return await _sellSchemeDL.GetSellInHeaderByUID(UID);
        }
        public async Task<int> CUSellInHeader(ISellInSchemeDTO sellInDTO)
        {
            int cnt = 0;

            cnt = await _sellSchemeDL.CUSellInHeader(sellInDTO);
            cnt += await _schemeBranchBL.CDBranches(sellInDTO.SchemeBranches, sellInDTO.SellInHeader.UID);
            cnt += await _broadClassificationBL.CDBroadClassification(sellInDTO.SchemeBroadClassifications, sellInDTO.SellInHeader.UID);
            cnt += await _schemeOrgBL.CDOrgs(sellInDTO.SchemeOrgs, sellInDTO.SellInHeader.UID);
            if (cnt > 0 && sellInDTO != null && sellInDTO.ApprovalStatusUpdate != null && sellInDTO.ApprovalStatusUpdate.IsFinalApproval)
            {
                cnt += await _sellSchemeDL.UpdateSellInHeaderAfterFinalApproval(sellInDTO.SellInHeader);
                _sellSchemeDL.UpdateSellInMappingData(sellInDTO.SellInHeader.UID);
            }
            return cnt;
        }



        public async Task<int> CreateSellInScheme(ISellInSchemeDTO sellInDTO)
        {
            int cnt = 0;
            cnt += await _sellSchemeDL.CreateSellInHeader(sellInDTO.SellInHeader);
            cnt += await _sellSchemeDL.CreateSellInDetail(sellInDTO.SellInSchemeLines);
            if (cnt > 0 && sellInDTO.SchemeBranches != null && sellInDTO.SchemeBranches.Count > 0)
            {
                cnt += await _schemeBranchBL.CreateSchemeBranches(sellInDTO.SchemeBranches);
            }
            if (cnt > 0 && sellInDTO.SchemeBroadClassifications != null && sellInDTO.SchemeBroadClassifications.Count > 0)
            {
                cnt += await _broadClassificationBL.CreateSchemeBroadClassifications(sellInDTO.SchemeBroadClassifications);
            }
            if (cnt > 0 && sellInDTO.SchemeOrgs != null && sellInDTO.SchemeOrgs.Count > 0)
            {
                cnt += await _schemeOrgBL.CreateSchemeOrgs(sellInDTO.SchemeOrgs);
            }
            return cnt;
        }
        public async Task<ISellInSchemeDTO> GetSellInSchemeByOrgUid(string OrgUid, DateTime OrderDate)
        {
            return await _sellSchemeDL.GetSellInSchemeByOrgUid(OrgUid, OrderDate);
        }
        public async Task<List<IStandingProvision>> GetStandingProvisionAmountByChannelPartnerUID(string channelPartnerUID)
        {
            return await _sellSchemeDL.GetStandingProvisionAmountByChannelPartnerUID(channelPartnerUID);
        }
        public async Task<ISellInSchemeDTO> GetSellInMasterByHeaderUID(string UID)
        {
            ISellInSchemeDTO sellInSchemeDTO = new SellInSchemeDTO();
            sellInSchemeDTO.SellInHeader = await _sellSchemeDL.GetSellInHeaderByUID(UID);
            sellInSchemeDTO.SellInSchemeLines = await _sellSchemeDL.GetSellInDetailByHeaderUID(UID);
            sellInSchemeDTO.Wallet = await _walletDL.GetWalletByOrgUID(sellInSchemeDTO.SellInHeader.FranchiseeOrgUID);
            sellInSchemeDTO.SchemeBroadClassifications = await _broadClassificationBL.GetSchemeBroadClassificationByLinkedItemUID(UID);
            sellInSchemeDTO.SchemeBranches = await _schemeBranchBL.GetSchemeBranchesByLinkedItemUID(UID);
            sellInSchemeDTO.SchemeOrgs = await _schemeOrgBL.GetSchemeOrgByLinkedItemUID(UID);
            return sellInSchemeDTO;
        }

        public async Task<List<ISellInSchemePO>> GetSellInSchemesByOrgUidAndSKUUid(string orgUid, List<string>? sKUUIDs = null)
        {
            return await _sellSchemeDL.GetSellInSchemesByOrgUidAndSKUUid(orgUid, sKUUIDs);
        }
        public async Task<List<ISellInSchemePO>> GetExistSellInSchemesByPOUid(string POHeaderUID)
        {
            return await _sellSchemeDL.GetExistSellInSchemesByPOUid(POHeaderUID);
        }
    }
}

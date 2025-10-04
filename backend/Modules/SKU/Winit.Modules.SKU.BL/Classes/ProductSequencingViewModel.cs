using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Web.SKU;



namespace Winit.Modules.SKU.BL.Classes
{
    public class ProductSequencingViewModel : ProductSequencingWebViewModel
    {

        public ProductSequencingViewModel(IServiceProvider serviceProvider,
     IFilterHelper filter,
     ISortHelper sorter,
     IListHelper listHelper,
     IAppUser appUser,

                 Winit.Shared.Models.Common.IAppConfig appConfigs,
                 Base.BL.ApiService apiService
             ) : base(serviceProvider, filter, sorter, listHelper, appUser, appConfigs, apiService)
        {

          
        }

        public override async Task PopulateViewModel(string dataNeededFor = null, string pageName = null, string apiParam = null, string apiName = null)
        {
            await GetSKUMasterData();
            await PrepareDataForTable(apiParam);

        }
        public override async Task ApplyFilter(Dictionary<string, string> filterCriterias,string ActiveTab)
        {
            try
            {
                await FetchSKUUID(filterCriterias);
                
                await PrepareDataForTable(ActiveTab);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }


        public async Task FetchSKUUID(Dictionary<string, string> filterCriterias)
        {
            FilterCriterias.Clear();

            string searchTerm = filterCriterias.ContainsKey("Name") ? filterCriterias["Name"].ToLower() : string.Empty;
            if (!searchTerm.IsNullOrEmpty())
            {
                var matchesSkuList = SkuList.Where(sku =>
                                    sku.Name.ToLower().Contains(searchTerm) ||
                                    sku.Code.ToLower().Contains(searchTerm)
                                ).ToList();

                // Check if matchesSkuList is empty
                if (matchesSkuList.Count == 0)
                {
                    
                    // If matchesSkuList is empty, add searchTerm to skuUIDs
                    var skuUIDs = new[] { searchTerm };
                    FilterCriterias.Add(new FilterCriteria("sku_uid", skuUIDs, FilterType.In));
                }
                else
                {
                   
                    // Extracting the UIDs from matchesSkuList
                    var skuUIDs = matchesSkuList.Select(sku => sku.UID).ToArray();
                    FilterCriterias.Add(new FilterCriteria("sku_uid", skuUIDs, FilterType.In));
                }

                
            }

        }
        
        public override async Task PrepareDataForTable(string SeqType)
        {
            try
            {
                //await GetSKUSequenceData();
                SkuSequence = await GetSKUSequenceData(SeqType);
                DisplaySkuSequencelist = new List<SkuSequenceUI>();
                FilterSkuSequencelist = new List<SkuSequenceUI>();
                //var joinedData = from seq in SkuSequence
                //                 join sku in SkuList on seq.SKUUID equals sku.UID
                //                 select new { seq, sku };

                foreach (var item in SkuSequence)
                {
                    Model.Classes.SKU sku = (Model.Classes.SKU)SkuList.FirstOrDefault(s => s.UID == item.SKUUID);
                    if (sku != null)
                    {
                        DisplaySkuSequencelist.Add(new SkuSequenceUI
                        {
                            SKUCode = sku?.Code ?? "N/A",
                            SKUName = sku?.Name ?? "N/A",
                            FranchiseeOrgUID = item.FranchiseeOrgUID,
                            SKUUID = item.SKUUID,
                            BUOrgUID = item.BUOrgUID,
                            UID = item.UID,
                            CreatedBy = item.CreatedBy,
                            CreatedTime = item.CreatedTime,
                            Id = item.Id,
                            ModifiedBy = item.ModifiedBy,
                            ModifiedTime = item.ModifiedTime,
                            ServerModifiedTime = item.ServerModifiedTime,
                            SeqType = item.SeqType,
                            SerialNo = item.SerialNo,
                            ServerAddTime = item.ServerAddTime,
                            SS = item.SS

                        });
                    }
                }
                //foreach (var item in joinedData)
                //{
                //    DisplaySkuSequencelist.Add(new SkuSequenceUI
                //    {
                //        SKUCode = item.sku?.Code ?? "N/A",
                //        SKUName = item.sku?.Name ?? "N/A",
                //        FranchiseeOrgUID = item.seq.FranchiseeOrgUID,
                //        SKUUID = item.seq.SKUUID,
                //        BUOrgUID = item.seq.BUOrgUID,
                //        UID = item.seq.UID,
                //        CreatedBy = item.seq.CreatedBy,
                //        CreatedTime = item.seq.CreatedTime,
                //        Id = item.seq.Id,
                //        ModifiedBy = item.seq.ModifiedBy,
                //        ModifiedTime = item.seq.ModifiedTime,
                //        ServerModifiedTime = item.seq.ServerModifiedTime,
                //        SeqType = item.seq.SeqType,
                //        SerialNo = item.seq.SerialNo,
                //        ServerAddTime = item.seq.ServerAddTime,
                //        SS = item.seq.SS

                //    });
                //}
                DisplaySkuSequencelist = DisplaySkuSequencelist.OrderBy(r => r.SerialNo).ToList();
                FilterSkuSequencelist = DisplaySkuSequencelist.OrderBy(r => r.SerialNo).ToList();

                //  SkuSequenceExtendedlistOriginal = SkuSequenceExtendedlist;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

        }

        public override void MatchingNewExistingUIDs()
        {
            FindAddUIDs();
            FindExistingUIDs();
        }
        public void FindAddUIDs()
        {
            AllUIDs = DisplaySkuSequencelist
                 .Where(item => item.IsSelected)
                 .Select(item => item.UID)
                 .ToList();
        }

        public void FindExistingUIDs()
        {

            if (FilterSkuSequencelist != null && FilterSkuSequencelist.Count != 0)
            {
                ExistingUIDs = FilterSkuSequencelist
                    .Where(item => item.IsSelected)
                    .Select(item => item.UID)
                    .ToList();
            }
        }
        public async Task<List<SkuSequence>> GetSKUSequenceData(string SeqType)
        {
            try
            {


                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageNumber = 1;
                pagingRequest.PageSize = int.MaxValue;
                pagingRequest.FilterCriterias = FilterCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
               $"{_appConfigs.ApiBaseUrl}SkuSequence/SelectAllSkuSequenceDetails?SeqType={SeqType}",
               HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<SkuSequence> fetchedapiData = JsonConvert.DeserializeObject<PagedResponse<SkuSequence>>(data);
                    if (fetchedapiData.PagedData != null)
                    {
                        SkuSequenceList = fetchedapiData.PagedData.ToList();
                    }

                }

            }

            catch (Exception ex)
            {
                // Handle exceptions
            }
            return SkuSequenceList;
        }

        public override List<SkuSequence> PrepareListOfUidtForDelete()
        {
            var skuSKUSequenceList = new List<SkuSequence>();
            foreach (var selectedSKU in DisplaySkuSequencelist)
            {
                if (selectedSKU.IsSelected == true)
                {
                    SkuSequence skuSequence = new SkuSequence
                    {
                        UID = selectedSKU.UID,
                        ActionType = Winit.Shared.Models.Enums.ActionType.Delete
                    };
                    skuSKUSequenceList.Add(skuSequence);
                }

            }
            return skuSKUSequenceList;
        }
        public async Task<List<Winit.Modules.SKU.Model.Classes.SKUMasterData>> GetSKUMasterData()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageSize = 10;
                pagingRequest.PageNumber = 1;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData",HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData> selectionSKUs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>>(data);
                    if (selectionSKUs.PagedData != null)
                    {
                        SkuMasterData = selectionSKUs.PagedData.ToList();
                        SkuAttributesList = await FindCompleteSKUAttributes(SkuMasterData);
                        SkuList = await FindCompleteSKU(SkuMasterData);
                    }
                }

            }

            catch (Exception ex)
            {
                // Handle exceptions
            }
            return SkuMasterData;
        }
        public override async Task<bool> PrepareDataForSaveUpdate()
        {
            var skuSequenceList = new List<SkuSequence>();
            foreach (var selectedSKU in DisplaySkuSequencelist/*.Where(item => item.SeqType == ActiveTab)*/)
            {
                //if (StoredTab.Contains(selectedSKU.SeqType) || selectedSKU.UID == null)
                //{
                SkuSequence skuSequence = new SkuSequence
                {
                    FranchiseeOrgUID = selectedSKU.FranchiseeOrgUID != null ? selectedSKU.FranchiseeOrgUID : "WINIT",
                    SKUUID = selectedSKU.SKUUID,
                    BUOrgUID = selectedSKU.BUOrgUID != null ? selectedSKU.BUOrgUID : "2d893d92-dc1b-5904-934c-621103a900e3",
                    UID = selectedSKU.UID != null ? selectedSKU.UID : Guid.NewGuid().ToString(),
                    CreatedBy = _appUser.Emp.UID,
                    CreatedTime = selectedSKU.CreatedTime != null ? selectedSKU.CreatedTime : DateTime.Now,

                    ModifiedBy = _appUser.Emp.UID,
                    ModifiedTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    SeqType = selectedSKU.SeqType,
                    SerialNo = selectedSKU.SerialNo,
                    ServerAddTime = DateTime.Now,
                    SS = selectedSKU.SS,
                };
                skuSequenceList.Add(skuSequence);
            }

            if (await CreateUpdateDeleteSKUs(skuSequenceList))
            {

                return true;
            }
            else { return false; }
        }
        public override async Task<bool> CreateUpdateDeleteSKUs(List<SkuSequence> skuSequenceList)
        {
            string jsonBody = JsonConvert.SerializeObject(skuSequenceList);
            string apiUrl = $"{_appConfigs.ApiBaseUrl}SkuSequence/CUDSkuSequence";
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Post, skuSequenceList);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);

                return apiResponse.IsSuccess; // This assumes that apiResponse.IsSuccess is a bool.

            }

            return false;
        }
    }
}

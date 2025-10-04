using Winit.Modules.Base.BL;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.PriceLadder.BL.Interfaces;
using Winit.Modules.PriceLadder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PriceLadder.BL.Classes
{
    public class PriceLadderingBaseViewModel : IPriceLadderingViewModel
    {
        public string ViewingMessage { get; set; }
        public List<IPriceLaddering> PriceLadderings { get; set; }
        List<ISelectionItem> BroadClassificationDDL { get; set; }
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private readonly ApiService _apiService; 
        public Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering PopUpPriceLadderingObj { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> PopUpSkuDetailsList { get; set; }
        public List<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLadderingItemView> PriceLadderingList { get; set; }
        public List<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering> PriceLadderingsListToShowInGrid { get; set; }
        public List<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering> PriceLadderingSubList { get; set; }
        public Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering SelectedPriceLaddering { get; set; }
        protected PagingRequest PagingRequest = new PagingRequest()
        {
            FilterCriterias = [],
            IsCountRequired=true
        };
        public int TotalPriceLadderRecord { get; set; }
        public List<SortCriteria> SortCriterias = new List<SortCriteria>();
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public PriceLadderingBaseViewModel(Winit.Shared.Models.Common.IAppConfig appConfigs,
                ApiService apiService)
        {
            this._appConfigs = appConfigs;
            this._apiService = apiService;
            PriceLadderingList = [];
            PriceLadderingsListToShowInGrid = [];
            PriceLadderingSubList = [];
            BroadClassificationDDL= new List<ISelectionItem>();
            PagingRequest.PageNumber = PageNumber;
            PagingRequest.PageSize = PageSize;
            PagingRequest.SortCriterias = new List<SortCriteria>();
        }

        public async Task GetThePriceLadderingList()
        {
            // PriceLadderingList = await GetAllPriceLaddering(pagingRequest);
            PriceLadderingsListToShowInGrid = await GetPriceLadders();
            // PriceLadderingSubList = await GetRelatedData(SelectedPriceLaddering);
        }
        public async Task OnProductCategoryClick(IPriceLaddering item, IPriceLaddering subItem)
        {
            try
            {
                PopUpPriceLadderingObj = item;
                PopUpPriceLadderingObj.ProductCategoryId = subItem.ProductCategoryId;
                PopUpSkuDetailsList = await GetSkuDetailsFromProductCategoryId((int)subItem.ProductCategoryId);
            }
            catch (Exception)
            {

            }
        }
        public async Task GetRelatedRowData()
        {
            PriceLadderingSubList = await GetRelatedData(SelectedPriceLaddering);
        }

        // api calling
        //public async Task<List<ISelectionItem>> GetModuleDropdownValuesAsync(string dropdownType)
        //{
        //    if (dropdownType=="OrgUnit")
        //    {
        //        return await Task.FromResult(new List<ISelectionItem>
        //        {
        //            new SelectionItem { UID = "90", Code = "90", Label = "90" }
        //        });

        //    }
        //    else if (dropdownType=="Division")
        //    {
        //        return await Task.FromResult(new List<ISelectionItem>
        //        {
        //            new SelectionItem { UID = "10", Code = "10", Label = "10" },
        //            new SelectionItem { UID = "20", Code = "20", Label = "20" }
        //        });

        //    }
        //    else if (dropdownType=="Branch")
        //    {
        //        return await Task.FromResult(new List<ISelectionItem>
        //        {
        //            new SelectionItem { UID = "KOLKATA", Code = "KOLKATA", Label = "KOLKATA" },
        //            new SelectionItem { UID = "PUNE", Code = "PUNE", Label = "PUNE" }
        //        });

        //    }
        //    else if (dropdownType=="BroadClassification")
        //    {
        //        return await Task.FromResult(new List<ISelectionItem>
        //        {
        //            new SelectionItem { UID = "SSD", Code = "SSD", Label = "SSD" },
        //            new SelectionItem { UID = "DIST", Code = "DIST", Label = "DIST" }
        //        });
        //    }
        //    return default;

        //}
        public async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {

            PagingRequest.FilterCriterias!.Clear();
            if (filterCriteria is not null)
            {

                foreach (var item in filterCriteria)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        switch (item.Key)
                        {

                            case nameof(IPriceLaddering.OperatingUnit):
                                if (item.Value.Contains(","))
                                {
                                    string[] values = item.Value.Split(',');
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.OperatingUnit), values, FilterType.In));
                                }
                                else
                                {
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.OperatingUnit), item.Value, FilterType.Equal));
                                }
                                break;
                            case nameof(IPriceLaddering.Division):
                                if (item.Value.Contains(","))
                                {
                                    string[] values = item.Value.Split(',');
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.Division), values, FilterType.In));
                                }
                                else
                                {
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.Division), item.Value, FilterType.Equal));
                                }
                                break;
                            case nameof(IPriceLaddering.Branch):
                                if (item.Value.Contains(","))
                                {
                                    string[] values = item.Value.Split(',');
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.Branch), values, FilterType.In));
                                }
                                else
                                {
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.Branch), item.Value, FilterType.Equal));
                                }
                                break;
                            case nameof(IPriceLaddering.BroadCustomerClassification):
                                if (item.Value.Contains(","))
                                {
                                    string[] values = item.Value.Split(',');
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.BroadCustomerClassification), values, FilterType.In));
                                }
                                else
                                {
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.BroadCustomerClassification), item.Value, FilterType.Equal));
                                }
                                break;
                            case nameof(IPriceLaddering.SkuCode):
                                if (item.Value.Contains(","))
                                {
                                    string[] values = item.Value.Split(',');
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.SkuCode), values, FilterType.In));
                                }
                                else
                                {
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IPriceLaddering.SkuCode), item.Value, FilterType.Equal));
                                }
                                break;
                            default:
                                PagingRequest?.FilterCriterias.Add(new FilterCriteria(item.Key, item.Value, FilterType.Equal));
                                break;
                        }
                    }

                }

            }
            await GetThePriceLadderingList();
        }
        public async Task<List<IPriceLadderingItemView>> GetAllPriceLaddering(PagingRequest pagingRequest)
        {
            try
            {
                ApiResponse<PagedResponse<IPriceLadderingItemView>> apiResponse =
                   await _apiService.FetchDataAsync<PagedResponse<IPriceLadderingItemView>>(
                   $"{_appConfigs.ApiBaseUrl}PriceLaddering/SelectAllThePriceLaddering",
                   HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {
                    return apiResponse.Data.PagedData.ToList<IPriceLadderingItemView>();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return [];
        }
        public async Task<List<IPriceLaddering>> GetPriceLadders()
        {
            try
            {

                ApiResponse<PagedResponse<IPriceLaddering>> apiResponse =
                   await _apiService.FetchDataAsync<PagedResponse<IPriceLaddering>>(
                   $"{_appConfigs.ApiBaseUrl}PriceLaddering/GetPriceLadders",
                   HttpMethod.Post, PagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {
                    TotalPriceLadderRecord=apiResponse.Data.TotalCount;
                    GetViewingMessage();
                    return apiResponse.Data.PagedData.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return [];
        }
        public async Task<List<ISKU>> GetSkuDetailsFromProductCategoryId(int ProductcategoryId)
        {
            try
            {

                ApiResponse<List<ISKU>> apiResponse =
                   await _apiService.FetchDataAsync<List<ISKU>>(
                   $"{_appConfigs.ApiBaseUrl}PriceLaddering/GetSkuDetailsFromProductCategoryId",
                   HttpMethod.Post, ProductcategoryId);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return [];
        }
        public async Task<List<IPriceLaddering>> GetRelatedData(IPriceLaddering priceLaddering)
        {
            try
            {
                ApiResponse<List<IPriceLaddering>> apiResponse =
                   await _apiService.FetchDataAsync<List<IPriceLaddering>>(
                   $"{_appConfigs.ApiBaseUrl}PriceLaddering/GetRelatedData",
                   HttpMethod.Post, priceLaddering);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return [];
        }
        public async Task<List<ISelectionItem>> GetBroadClassificationDDValues()
        {

            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                ApiResponse<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>> apiResponse =
           await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>>
           ($"{_appConfigs.ApiBaseUrl}BroadClassificationHeader/GetBroadClassificationHeaderDetails",
           HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess & apiResponse.Data != null && apiResponse.Data?.PagedData != null)
                {
                    BroadClassificationDDL.Clear();
                    foreach (var item in apiResponse.Data.PagedData)
                    {
                        ISelectionItem selectionItem = new SelectionItem()
                        {
                            UID = item.UID,
                            Code = item.Name,
                            Label = item.Name,
                        };
                        BroadClassificationDDL.Add(selectionItem);
                    }
                    return BroadClassificationDDL;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return default;
        }
        public async Task<List<ISelectionItem>> GetAllDivisionDDValues()
        {
            try
            {
                ApiResponse<List<Winit.Modules.Org.Model.Classes.Org>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Org.Model.Classes.Org>>(
                    $"{_appConfigs.ApiBaseUrl}Org/GetDivisions",
                    HttpMethod.Post, string.Empty);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    var divisionDetails = CommonFunctions.ConvertToSelectionItems(apiResponse.Data, new List<string> { "Code", "Code", "Name" });

                    return divisionDetails;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<ISelectionItem>> GetAllBranchDDLValues()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                ApiResponse<PagedResponse<IBranch>> apiResponse =
                   await _apiService.FetchDataAsync<PagedResponse<IBranch>>(
                   $"{_appConfigs.ApiBaseUrl}Branch/SelectAllBranchDetails",
                   HttpMethod.Post, pagingRequest);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {

                    var branches = apiResponse.Data.PagedData.ToList<IBranch>();
                    var branchDDL = (CommonFunctions.ConvertToSelectionItems<IBranch>(branches, new List<string> { "UID", "Code", "Name" }));
                    return branchDDL;

                }
            }
            catch (Exception)
            {
                throw;
            }
            return [];
        }
        public async Task<List<ISelectionItem>> GetOUDetailsFromAPIAsync()
        {
            try
            {
                string OrgType = "OU";
                ApiResponse<List<Winit.Modules.Org.Model.Classes.Org>> apiResponse = await _apiService.FetchDataAsync
                 <List<Winit.Modules.Org.Model.Classes.Org>>(
                 $"{_appConfigs.ApiBaseUrl}Org/GetOrgByOrgTypeUID?OrgTypeUID=" + OrgType,
                 HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    var orgData = apiResponse.Data.ToList<IOrg>();
                    var organisationUnit = CommonFunctions.ConvertToSelectionItems(orgData, new List<string> { "Code", "Code", "Name" });
                    return organisationUnit;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PagingRequest.PageNumber=pageNumber;
            PageNumber=pageNumber;
            await GetThePriceLadderingList();
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            PagingRequest.SortCriterias=SortCriterias;
            await GetThePriceLadderingList();
        }
        private void GetViewingMessage()
        {
            int startRecord = 0;
            int endRecord = 0;
            if (TotalPriceLadderRecord == 0)
            {
                ViewingMessage= $"You are viewing {startRecord}-{endRecord} out of {TotalPriceLadderRecord}";
            }
            else
            {
                startRecord = ((PageNumber - 1) * PageSize) + 1;
                endRecord = Math.Min(PageNumber * PageSize, TotalPriceLadderRecord);
                ViewingMessage= $"You are viewing {startRecord}-{endRecord} out of {TotalPriceLadderRecord}";
            }

        }
    }
}

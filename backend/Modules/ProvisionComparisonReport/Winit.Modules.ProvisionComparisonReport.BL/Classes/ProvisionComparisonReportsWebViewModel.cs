using Newtonsoft.Json;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.ProvisionComparisonReport.Model.Classes;
using Winit.Modules.ProvisionComparisonReport.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ProvisionComparisonReport.BL.Classes
{
    public class ProvisionComparisonReportsWebViewModel : ProvisionComparisonReportsBaseViewModel
    {
        public ProvisionComparisonReportsWebViewModel(Winit.Shared.Models.Common.IAppConfig appConfigs,
        ApiService apiService, IAppUser appUser) : base(appConfigs, apiService, appUser)
        {

        }
        public override async Task GetAllProvisionComparisonReport()
        {
            try
            {
                PagingRequest.PageNumber = PageNumber;
                PagingRequest.PageSize = PageSize;
                PagingRequest.IsCountRequired = true;
                PagingRequest.SortCriterias = this.SortCriterias ?? new List<SortCriteria>();
                string apiUrl = $"{_appConfigs.ApiBaseUrl}Invoice/GetProvisionComparisonReport";
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Post, PagingRequest);
                if (apiResponse == null)
                {
                    Console.WriteLine("Error: API response is null.");
                    return;
                }

                if (!apiResponse.IsSuccess)
                {
                    Console.WriteLine($"Error: API call failed with message: {apiResponse.ErrorMessage}");
                    return;
                }

                if (string.IsNullOrWhiteSpace(apiResponse.Data))
                {
                    Console.WriteLine("Error: API response data is empty.");
                    return;
                }
                try
                {
                    if (apiResponse?.Data == null)
                    {
                        Console.WriteLine("Error: API response data is null.");
                        ProvisionsComparisonReportView = new List<IProvisionComparisonReportView>();
                        return;
                    }
                    //var pagedResponse = JsonConvert.DeserializeObject<PagedResponse<ProvisionComparisonReportView>>(apiResponse.Data);
                    var pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<ProvisionComparisonReportView>>>(apiResponse.Data);

                    if (pagedResponse != null)
                    {
                        ProvisionsComparisonReportView= pagedResponse.Data.PagedData.OfType<IProvisionComparisonReportView>().ToList();
                        TotalProvisionComparisonReportRecord =pagedResponse.Data.TotalCount;


                    }
                    else
                    {
                        Console.WriteLine("this is newtonsoft Failed to deserialize the response.");
                    }
                }
                catch (JsonSerializationException jsonEx)
                {
                    Console.WriteLine($"JSON Deserialization Error: {jsonEx.Message}");
                    ProvisionsComparisonReportView = new List<IProvisionComparisonReportView>(); // Fail-safe: Assign empty list
                }

            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error: {httpEx.Message}");
            }
            catch (TaskCanceledException taskEx)
            {
                Console.WriteLine($"Task canceled error: {taskEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
        public override async Task ExportProvisionComparisonReportBasedOnFilter()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias=PagingRequest.FilterCriterias;
                pagingRequest.SortCriterias = this.SortCriterias ?? new List<SortCriteria>();
                string apiUrl = $"{_appConfigs.ApiBaseUrl}Invoice/GetProvisionComparisonReport";
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Post, pagingRequest);
                if (apiResponse == null)
                {
                    Console.WriteLine("Error: API response is null.");
                    return;
                }

                if (!apiResponse.IsSuccess)
                {
                    Console.WriteLine($"Error: API call failed with message: {apiResponse.ErrorMessage}");
                    return;
                }

                if (string.IsNullOrWhiteSpace(apiResponse.Data))
                {
                    Console.WriteLine("Error: API response data is empty.");
                    return;
                }
                try
                {
                    if (apiResponse?.Data == null)
                    {
                        Console.WriteLine("Error: API response data is null.");
                        ProvisionsComparisonReportViewInExportExcel = new List<IProvisionComparisonReportView>();
                        return;
                    }
                    var pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<ProvisionComparisonReportView>>>(apiResponse.Data);


                    if (pagedResponse != null && pagedResponse.Data != null)
                    {
                        if (pagedResponse != null && pagedResponse.Data.PagedData != null)
                        {
                            ProvisionsComparisonReportViewInExportExcel = pagedResponse.Data.PagedData.Cast<IProvisionComparisonReportView>().ToList();
                        }
                        else
                        {
                            Console.WriteLine("Error: API response data is null.");
                            ProvisionsComparisonReportViewInExportExcel = new List<IProvisionComparisonReportView>();
                            return;
                        }

                    }

                    else
                    {
                        ProvisionsComparisonReportViewInExportExcel = new List<IProvisionComparisonReportView>();
                        Console.WriteLine("this is newtonsoft Failed to deserialize the response.");
                    }
                }
                catch (JsonSerializationException jsonEx)
                {
                    Console.WriteLine($"JSON Deserialization Error: {jsonEx.Message}");
                    ProvisionsComparisonReportViewInExportExcel = new List<IProvisionComparisonReportView>(); // Fail-safe: Assign empty list
                }

            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error: {httpEx.Message}");
            }
            catch (TaskCanceledException taskEx)
            {
                Console.WriteLine($"Task canceled error: {taskEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
        public override async Task<List<ISelectionItem>> GetChannelPartnerDDLValues()
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<Winit.Modules.Store.Model.Interfaces.IStore>> apiResponse = await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Interfaces.IStore>>(
            $"{_appConfigs.ApiBaseUrl}Store/GetChannelPartner?jobPositionUid={_appUser.SelectedJobPosition.UID}",
            HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    var channelPartners = new List<ISelectionItem>();
                    foreach (var s in apiResponse.Data)
                    {
                        ISelectionItem selectionItem = new SelectionItem()
                        {
                            UID = s.UID,
                            Code = s.Code,
                            Label = $"[{s.Code}] {s.Name}",
                        };
                        channelPartners.Add(selectionItem);
                    }
                    return channelPartners;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return default;
        }
        public override async Task<List<ISelectionItem>> GetBroadClassificationDDLValues()
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
                    var broadClassificationDDL = new List<ISelectionItem>();
                    foreach (var item in apiResponse.Data.PagedData)
                    {
                        ISelectionItem selectionItem = new SelectionItem()
                        {
                            UID = item.UID,
                            Code = item.Name,
                            Label = item.Name,
                        };
                        broadClassificationDDL.Add(selectionItem);
                    }
                    return broadClassificationDDL;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return default;
        }
        public override async Task<List<ISelectionItem>> GetBranchDDLValues()
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
        public override async Task<List<ISelectionItem>> GetSalesOfficeDDLValues()
        {
            return CommonFunctions.ConvertToSelectionItems(await GetSalesOffice(), new List<string> { "UID", "Code", "Name" });
        }
        public override async Task<List<ISelectionItem>> GetProvisionTypeDDLValues()
        {
            return CommonFunctions.ConvertToSelectionItems(await GetListItemsByCodesFromApiAsync(new List<string> { "PROVISION_TYPE" }), new List<string> { "UID", "Code", "Name" });
        }
        private async Task<List<ISalesOffice>> GetSalesOffice()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.FilterCriterias ??= new List<FilterCriteria>();
                //pagingRequest.SortCriterias = this.SortCriterias;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}SalesOffice/SelectAllSalesOfficeDetails",
                    HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<PagedResponse<SalesOffice>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<SalesOffice>>>(apiResponse.Data);
                    return pagedResponse.Data.PagedData.ToList<ISalesOffice>();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        private async Task<List<IListItem>> GetListItemsByCodesFromApiAsync(List<string> codes)
        {
            try
            {
                Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new();
                listItemRequest.Codes = codes;
                ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> apiResponse =
                    await _apiService.FetchDataAsync<PagedResponse<ListHeader.Model.Classes.ListItem>>(
                        $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                        HttpMethod.Post, listItemRequest);
                if (apiResponse != null && apiResponse.IsSuccess)
                {
                    return apiResponse.Data.PagedData.ToList<IListItem>();
                }
                return new();
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public override async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await GetAllProvisionComparisonReport();
        }
        public override async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            try
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
                                case "StartDate": // condition for both StartDate and EndDate
                                case "EndDate":
                                    if (filterCriteria.ContainsKey("StartDate") &&
                                        filterCriteria.ContainsKey("EndDate"))
                                    {
                                        string fromDateValue = ConvertIntoFormat(filterCriteria["StartDate"]);
                                        string toDateValue = ConvertIntoFormat(filterCriteria["EndDate"]);

                                        // Check if the filter already exists
                                        bool filterExists = PagingRequest?.FilterCriterias.Any(fc =>
                                            fc.Name == "InvoiceDate" &&
                                            fc.Type == FilterType.Between) ?? false;

                                        if (!filterExists)
                                        {
                                            string[] dateValues = { fromDateValue, toDateValue };
                                            PagingRequest?.FilterCriterias.Add(
                                                new FilterCriteria("InvoiceDate", dateValues, FilterType.Between));
                                        }
                                    }
                                    break;
                                case "ChannelPartner":
                                    if (item.Value.Contains(","))
                                    {
                                        string[] values = item.Value.Split(',');
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("ChannelPartner", values, FilterType.In));
                                    }
                                    else
                                    {
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("ChannelPartner", item.Value, FilterType.Equal));
                                    }
                                    break;
                                case "BroadClassification":
                                    if (item.Value.Contains(","))
                                    {
                                        string[] values = item.Value.Split(',');
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("BroadClassification", values, FilterType.In));
                                    }
                                    else
                                    {
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("BroadClassification", item.Value, FilterType.Equal));
                                    }
                                    break;

                                case "Branch":
                                    if (item.Value.Contains(","))
                                    {
                                        string[] values = item.Value.Split(',');
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("Branch", values, FilterType.In));
                                    }
                                    else
                                    {
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("Branch", item.Value, FilterType.Equal));
                                    }
                                    break;

                                case "SalesOffice":
                                    if (item.Value.Contains(","))
                                    {
                                        string[] values = item.Value.Split(',');
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("SalesOffice", values, FilterType.In));
                                    }
                                    else
                                    {
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("SalesOffice", item.Value, FilterType.Equal));
                                    }
                                    break;
                                case "ProvisionType":
                                    if (item.Value.Contains(","))
                                    {
                                        string[] values = item.Value.Split(',');
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("ProvisionType", values, FilterType.In));
                                    }
                                    else
                                    {
                                        PagingRequest?.FilterCriterias.Add(new FilterCriteria("ProvisionType", item.Value, FilterType.Equal));
                                    }
                                    break;
                            }
                        }

                    }

                }
                await GetAllProvisionComparisonReport();
            }
            catch (Exception ex)
            {

            }
        }
        public string ConvertIntoFormat(string value)
        {
            try
            {
                string dateValueString = value;
                DateTime dateValue;

                if (DateTime.TryParse(dateValueString, out dateValue))
                {
                    return dateValue.ToString("yyyy-MM-dd");
                    // Use the formattedDate as needed
                }
                return value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override async Task PageIndexChanged(int pageNumber)
        {
            PageNumber=pageNumber;
            await GetAllProvisionComparisonReport();
        }
    }
}

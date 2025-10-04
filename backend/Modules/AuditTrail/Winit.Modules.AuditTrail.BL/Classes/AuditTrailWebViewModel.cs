//using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.AuditTrail.Model.Constant;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.AuditTrail.BL.Classes
{
    public class AuditTrailWebViewModel : AuditTrailBaseViewModel
    {

        public AuditTrailWebViewModel(IServiceProvider serviceProvider,
            IAppUser appUser,
            Shared.Models.Common.IAppConfig appConfigs,
            Base.BL.ApiService apiService) : base(serviceProvider, appUser, appConfigs, apiService)
        {

        }
        public override async Task PopulateViewModel()
        {
            try
            {
                // Initialize the paging request
                PagingRequest.PageNumber = PageNumber;
                PagingRequest.PageSize = PageSize;
                PagingRequest.IsCountRequired = true;
                PagingRequest.SortCriterias = this.SortCriterias ?? new List<SortCriteria>
                {
                    new SortCriteria("ModifiedTime", SortDirection.Desc)
                };

                // Construct the API URL
                string apiUrl = $"{_appConfigs.AuditTrialApiBaseUrl}AuditTrail/GetAuditTrailsAsyncByPaging";

                // Log the API URL
                Console.WriteLine($"API URL: {apiUrl}");

                // Fetch data from the API
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Get);

                // Validate the API response
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

                // Attempt to deserialize the response
                List<AuditTrailEntry>? list = null;
                try
                {
                    list = JsonConvert.DeserializeObject<List<AuditTrailEntry>>(apiResponse.Data);
                }

                catch (JsonSerializationException jsonEx)
                {
                    Console.WriteLine($"Error deserializing API response: {jsonEx.Message}");
                    return;
                }

                // Validate the deserialized response
                if (list== null || !list.Any())
                {
                    Console.WriteLine("Error: No data found in the API response.");
                    return;
                }

                // Map the data to the desired list
                AuditTrailEntry = list.Cast<IAuditTrailEntry>().ToList();
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
        public override async Task GetAuditTrailAsync()
        {
            try
            {
                // Initialize the paging request
                PagingRequest.PageNumber = PageNumber;
                PagingRequest.PageSize = PageSize;
                PagingRequest.IsCountRequired = true;
                PagingRequest.SortCriterias = this.SortCriterias ?? new List<SortCriteria>();
                // Construct the API URL
                string apiUrl = $"{_appConfigs.AuditTrialApiBaseUrl}AuditTrail/GetAuditTrailsAsyncByPaging";

                // Log the API URL
                Console.WriteLine($"API URL: {apiUrl}");

                // Fetch data from the API
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Post, PagingRequest);

                // Validate the API response
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
                        AuditTrailEntry = new List<IAuditTrailEntry>();
                        return;
                    }
                    var pagedResponse = JsonConvert.DeserializeObject<PagedResponse<AuditTrailEntry>>(apiResponse.Data);

                    if (pagedResponse != null)
                    {
                        AuditTrailEntry = pagedResponse.PagedData.Cast<IAuditTrailEntry>().ToList() ?? new List<IAuditTrailEntry>();
                        TotalAuditTrailItems = pagedResponse.TotalCount;

                        // Print total count
                        //Console.WriteLine($"this is newtonsoft Total Count: {TotalAuditTrailItems}");

                        // Print each AuditTrailEntry in JSON format
                        //foreach (var auditTrail in auditTrailList)
                        //{
                        //    Console.WriteLine($"this is newtonsoft Audit Trail: {JsonConvert.SerializeObject(auditTrail, Formatting.Indented)}");
                        //}
                    }
                    else
                    {
                        Console.WriteLine("this is newtonsoft Failed to deserialize the response.");
                    }
                    //var pagedResponse1 = System.Text.Json.JsonSerializer.Deserialize<PagedResponse<AuditTrailEntry>>(apiResponse.Data, new JsonSerializerOptions
                    //{
                    //    PropertyNameCaseInsensitive = true
                    //});

                    //if (pagedResponse1 != null)
                    //{
                    //    List<AuditTrailEntry> auditTrailList = pagedResponse.PagedData.ToList();
                    //    int totalCount = pagedResponse.TotalCount;

                    //    // Print total count
                    //    Console.WriteLine($"Total Count: {totalCount}");

                    //    // Print each AuditTrailEntry
                    //    foreach (var auditTrail in auditTrailList)
                    //    {
                    //        Console.WriteLine($"Audit Trail: {System.Text.Json.JsonSerializer.Serialize(auditTrail, new JsonSerializerOptions { WriteIndented = true })}");
                    //    }
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Failed to deserialize the response.");
                    //}
                    //var pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<AuditTrailEntry>>>(jsonApiResponse);
                    //var pagedResponse1 =JsonConvert.DeserializeObject<ApiResponse<PagedResponse<AuditTrailEntry>>>(apiResponse.Data);

                    //if (pagedResponse?.Data == null)
                    //{
                    //    Console.WriteLine("Error: Deserialized API response is null.");
                    //    AuditTrailEntry = new List<IAuditTrailEntry>();
                    //    return;
                    //}

                    //TotalAuditTrailItems = pagedResponse.Data.TotalCount;
                    //AuditTrailEntry = pagedResponse.Data.PagedData?.Cast<IAuditTrailEntry>().ToList() ?? new List<IAuditTrailEntry>();
                }
                catch (JsonSerializationException jsonEx)
                {
                    Console.WriteLine($"JSON Deserialization Error: {jsonEx.Message}");
                    AuditTrailEntry = new List<IAuditTrailEntry>();  // Fail-safe: Assign empty list
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
        public override async Task GetAuditTrailByIdAndPopulateViewModel(string id, bool isChangeDataRequired = true)
        {
            try
            {
                // Construct the API URL
                string apiUrl = $"{_appConfigs.AuditTrialApiBaseUrl}AuditTrail/GetAuditTrailByIdAsync?id={id}&isChangeDataRequired={isChangeDataRequired}";

                // Log the API URL
                Console.WriteLine($"API URL: {apiUrl}");

                // Fetch data from the API
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Get);

                // Validate the API response
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

                // Attempt to deserialize the response
                AuditTrailEntry? data = null;
                try
                {
                    data = JsonConvert.DeserializeObject<AuditTrailEntry>(apiResponse.Data);
                    //OriginalAuditTrailEntry=JsonConvert.DeserializeObject<AuditTrailEntry>(apiResponse.Data);
                }
                catch (JsonSerializationException jsonEx)
                {
                    Console.WriteLine($"Error deserializing API response: {jsonEx.Message}");
                    return;
                }

                // Validate the deserialized response
                if (data == null)
                {
                    Console.WriteLine("Error: No data found in the API response.");
                    return;
                }

                // Map the data to the desired list
                CurrentAuditTrailEntry = data;
                FilteredChangeData=data?.ChangeData;
                ChangeData= JsonConvert.DeserializeObject<List<ChangeLog>>(
                                JsonConvert.SerializeObject(data?.ChangeData)
                            );
                //OriginalAuditTrailEntry = JsonConvert.DeserializeObject<AuditTrailEntry>(JsonConvert.SerializeObject(data));

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

        public override async Task LoadOldRequestData()
        {
            try
            {
                // Construct the API URL
                string apiUrl = $"{_appConfigs.AuditTrialApiBaseUrl}AuditTrail/GetAuditTrailByIdAsync?id={CurrentAuditTrailEntry.OriginalDataId}&isChangeDataRequired={false}";

                // Log the API URL
                Console.WriteLine($"API URL: {apiUrl}");

                // Fetch data from the API
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Get);

                // Validate the API response
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

                // Attempt to deserialize the response
                AuditTrailEntry? data = null;
                try
                {
                    data = JsonConvert.DeserializeObject<AuditTrailEntry>(apiResponse.Data);
                    //OriginalAuditTrailEntry=JsonConvert.DeserializeObject<AuditTrailEntry>(apiResponse.Data);
                }
                catch (JsonSerializationException jsonEx)
                {
                    Console.WriteLine($"Error deserializing API response: {jsonEx.Message}");
                    return;
                }

                // Validate the deserialized response
                if (data == null)
                {
                    Console.WriteLine("Error: No data found in the API response.");
                    return;
                }

                // Map the data to the desired list
                //CurrentAuditTrailEntry = data;
                //OriginalAuditTrailEntry = JsonConvert.DeserializeObject<AuditTrailEntry>(JsonConvert.SerializeObject(data));
                OriginalAuditTrailEntry = data;

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
        public override async Task OnFilterApply(Dictionary<string, string> filterCriteria)
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
                            case nameof(IAuditTrailEntry.LinkedItemType):
                                if (item.Value.Contains(","))
                                {
                                    string[] values = item.Value.Split(',');
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IAuditTrailEntry.LinkedItemType), values, FilterType.In));
                                }
                                else
                                {
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IAuditTrailEntry.LinkedItemType), item.Value, FilterType.Equal));
                                }
                                break;
                            case nameof(IAuditTrailEntry.DocNo):
                                PagingRequest?.FilterCriterias.Add(
                                new FilterCriteria(nameof(IAuditTrailEntry.DocNo), item.Value, FilterType.Contains));
                                break;
                            case nameof(IAuditTrailEntry.CommandType):
                                if (item.Value.Contains(","))
                                {
                                    string[] values = item.Value.Split(',');
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IAuditTrailEntry.CommandType), values, FilterType.In));
                                }
                                else
                                {
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IAuditTrailEntry.CommandType), item.Value, FilterType.Equal));
                                }
                                break;
                            case AuditTrailConst.StartDate: // condition for both StartDate and EndDate
                            case AuditTrailConst.EndDate:
                                if (filterCriteria.ContainsKey(AuditTrailConst.StartDate) &&
                                    filterCriteria.ContainsKey(AuditTrailConst.EndDate))
                                {
                                    string fromDateValue = filterCriteria[AuditTrailConst.StartDate];
                                    string toDateValue = filterCriteria[AuditTrailConst.EndDate];

                                    // Check if the filter already exists
                                    bool filterExists = PagingRequest?.FilterCriterias.Any(fc =>
                                        fc.Name == nameof(IAuditTrailEntry.CommandDate) &&
                                        fc.Type == FilterType.Between) ?? false;

                                    if (!filterExists)
                                    {
                                        string[] dateValues = { fromDateValue, toDateValue };
                                        PagingRequest?.FilterCriterias.Add(
                                            new FilterCriteria(nameof(IAuditTrailEntry.CommandDate), dateValues, FilterType.Between));
                                    }
                                }
                                break;

                            case nameof(IAuditTrailEntry.EmpName):
                                PagingRequest?.FilterCriterias.Add(
                                new FilterCriteria(nameof(IAuditTrailEntry.EmpName), item.Value, FilterType.Contains));
                                break;
                            case nameof(IAuditTrailEntry.HasChanges):
                                if (item.Value.Contains(","))
                                {
                                    string[] values = item.Value.Split(',');
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IAuditTrailEntry.HasChanges), values, FilterType.In));
                                }
                                else
                                {
                                    PagingRequest?.FilterCriterias.Add(new FilterCriteria(nameof(IAuditTrailEntry.HasChanges), item.Value, FilterType.Equal));
                                }
                                break;

                        }
                    }

                }

            }
            await GetAuditTrailAsync();
        }
        public override async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await GetAuditTrailAsync();
        }
        public override async Task PageIndexChanged(int pageNumber)
        {
            PageNumber=pageNumber;
            await GetAuditTrailAsync();
        }
        public override async Task<List<ISelectionItem>> GetModuleDropdownValuesAsync()
        {
            try
            {

                var listItemRequest = new ListItems
                {
                    ListItemRequest = new ListItemRequest
                    {
                        Codes = new List<string> { AuditTrailConst.AuditTrailCode }, // Assuming 'uid' is a string representing a code
                        isCountRequired = true            // Set this to true as required
                    },
                    PagingRequest = new PagingRequest
                    {
                        PageNumber = PageNumber,     // Example: setting page number
                        PageSize = PageSize       // Example: setting page size
                    }
                };
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.SortCriterias = this.SortCriterias;
                if (listItemRequest != null)
                {
                    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByListHeaderCodes",
                    HttpMethod.Post, listItemRequest);
                    if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                    {
                        ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> pagedResponse = JsonConvert.DeserializeObject<ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>>(apiResponse.Data);
                        if (pagedResponse != null && pagedResponse.Data != null && pagedResponse.Data.PagedData != null)
                        {
                            List<ISelectionItem> selectionItems = pagedResponse.Data?.PagedData?
                            .Select(item => (ISelectionItem)new SelectionItem
                            {
                                UID = item?.Code,
                                Code = item?.Code,
                                Label = item?.Name
                            })
                            .ToList();
                            return selectionItems;
                        }
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
    }
}

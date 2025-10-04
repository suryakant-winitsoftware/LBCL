using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Web.Store;

using WinIt.Pages.Base;

namespace WinIt.Pages.CustDetails
{
    public partial class CustomerInfo : BaseComponentBase
    {
        [Parameter] public Winit.UIModels.Web.Store.CustomerInformationModel _CustomerInformation { get; set; } = new Winit.UIModels.Web.Store.CustomerInformationModel();
        [Parameter] public EventCallback<Winit.UIModels.Web.Store.CustomerInformationModel> OnCustomerInformationSaved { get; set; }
        [Parameter] public EventCallback<string> UidReturn { get; set; }
        [Parameter] public EventCallback<string> Failure { get; set; }
        [Parameter] public string UID { get; set; }
        public bool isEdited { get; set; } = false;

        private string customerCodeErrorMessage = "";
        private string customerNumberErrorMessage = "";
        private string customerNameErrorMessage = "";
        private string customerAliasNameErrorMessage = "";

        List<ListitemModel> listItems { get; set; } = new List<ListitemModel>();

        private string btnName = "Save";
        private static IBrowserFile selectedImage;
        HttpClient http = new HttpClient();
        PagingRequest paging = new PagingRequest();
        string messages;


        protected override async Task OnInitializedAsync()
        {

            if (UID != null)
            {
                await GetCustomerInfo();
                btnName = "Update";

            }
            else
            {
                _CustomerInformation.Number = DateTime.Now.ToString("ddMMyyyyhhmmss");
                _CustomerInformation.Code = "C" + DateTime.Now.ToString("ddMMyyyyhhmmss");
            }
            LoadResources(null, _languageService.SelectedCulture);
            await getListItemsAsync();
        }
       
        private void ChangeIsEditedOrNot()
        {
            isEdited = true;
        }
        public bool IsComponentEdited()
        {
            return isEdited;
        }

        public void ChangeSelection(ChangeEventArgs e, string Type)
        {
            ChangeIsEditedOrNot();
            switch (Type)
            {
                case "CustomerType":
                    _CustomerInformation.Type = e.Value.ToString();
                    break;
                case "PriceType":
                    _CustomerInformation.PriceType = e.Value.ToString();
                    break;
                case "BlockedBy":
                    _CustomerInformation.BlockedByEmpUID = e.Value.ToString();
                    break;
                case "RouteType":
                    break;
                case "BDM":
                    _CustomerInformation.ProspectEmpUID = e.Value.ToString();
                    break;

            }
        }

        private async Task getListItemsAsync()
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
             $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes?Codes=RouteType&Codes=CustomerType&Codes=Designation&Codes=PriceType&isCountRequired=true",
             HttpMethod.Post, "{}");
            if (apiResponse.Data != null)
            {
                var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                PagedResponse<ListitemModel> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<ListitemModel>>(data);
                if (pagedResponse != null)
                {
                    try
                    {
                        if (pagedResponse.TotalCount > 0)
                        {
                            listItems = (List<ListitemModel>)pagedResponse.PagedData;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

            }
        }


        private void HandleErorMessage()
        {
            if (string.IsNullOrWhiteSpace(_CustomerInformation.Code))
            {
                customerCodeErrorMessage = @Localizer["customer_code_is_required."];
            }
            else
            {
                customerCodeErrorMessage = "";
            }

            if (string.IsNullOrWhiteSpace(_CustomerInformation.Number))
            {
                customerNumberErrorMessage = @Localizer["customer_number_is_required."];
            }
            else
            {
                customerNumberErrorMessage = "";
            }
            if (string.IsNullOrWhiteSpace(_CustomerInformation.Name))
            {
                customerNameErrorMessage = @Localizer["customer_name_is_required."];
            }
            else
            {
                customerNameErrorMessage = "";
            }
            if (string.IsNullOrWhiteSpace(_CustomerInformation.AliasName))
            {
                customerAliasNameErrorMessage = @Localizer["customer_alias_name_is_required."];
            }
            else
            {
                customerAliasNameErrorMessage = "";
            }

        }

        protected async Task GetCustomerInfo()
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}Store/SelectStoreByUID?UID=" + UID, HttpMethod.Get, null);
            if (apiResponse.Data != null)
            {
                _CustomerInformation = JsonConvert.DeserializeObject<CustomerInformationModel>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
            }
        }
       
        private async Task Save()
        {
            if (string.IsNullOrEmpty(_CustomerInformation.Code) || 
                string.IsNullOrEmpty(_CustomerInformation.Number)|| 
                string.IsNullOrEmpty(_CustomerInformation.Name)|| 
                string.IsNullOrEmpty(_CustomerInformation.AliasName))
            {
                HandleErorMessage();
            }
            if (_CustomerInformation.UID == null || _CustomerInformation.UID == "")
            {
                _CustomerInformation.UID = Guid.NewGuid().ToString();
                _CustomerInformation.CreatedTime = DateTime.Now;
                _CustomerInformation.ModifiedTime = DateTime.Now;
                _CustomerInformation.ServerAddTime = DateTime.Now;
                _CustomerInformation.ServerModifiedTime = DateTime.Now;
                _CustomerInformation.CreatedByEmpUID = "7ee9f49f-26ea-4e89-8264-674094d805e1";
                _CustomerInformation.CompanyUID = "8D006B71-7DFD-4831-B132-F4B53F2C4C7rrytyhtjhyyF1";
                _CustomerInformation.CreatedByJobPositionUID = "df7bc6e2-273a-4ea4-90c6-ff1670d2b477";
                _CustomerInformation.CountryUID = "b86e9f24-d8d3-42ba-9e02-bd6cfc69245f";
                _CustomerInformation.RegionUID = "f147b975-ac53-4ffb-84a8-b0bddb89e13d";
                _CustomerInformation.CityUID = "2d893d92-dc1b-5904-934c-621103a900e39784s123";
                _CustomerInformation.CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
                _CustomerInformation.ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39";

            }

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
           /*  $"{_appConfigs.ApiBaseUrl}Store/SelectAllStore"*/"https://netcoretest.winitsoftware.com/api/store/CreateStore",
             HttpMethod.Post, _CustomerInformation);

            if (apiResponse.StatusCode == 200)
            {
                await UidReturn.InvokeAsync(_CustomerInformation.UID);
            }
            else
            {
                await Failure.InvokeAsync(apiResponse.StatusCode + apiResponse.ErrorMessage);
            }


        }
        public static void HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedImage = e.File;
        }
        private async Task UploadImage()
        {
            if (selectedImage != null)
            {
                var buffer = new byte[selectedImage.Size];
                object value = await selectedImage.OpenReadStream().ReadAsync(buffer);

                _CustomerInformation.StoreImage = $"data:{selectedImage.ContentType};base64,{Convert.ToBase64String(buffer)}";

            }

        }


    }
}

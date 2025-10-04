using Newtonsoft.Json;
using System.Globalization;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.EarlyPaymentConfiguration
{
    public class EarlyPaymentConfigurationWebViewModel : EarlyPaymentConfigurationBaseViewModel
    {
        protected readonly ApiService _apiService;
        public EarlyPaymentConfigurationWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            _apiService = apiService;
        }
        public override async Task GetCustomers(string CustomerCode)
        {
            try
            {
                selectedDate = DateTime.ParseExact(_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                Winit.Shared.Models.Common.ApiResponse<AccCustomer[]> response = await _apiService.FetchDataAsync<AccCustomer[]>(
                    $"{_appConfig.ApiBaseUrl}CollectionModule/GetAllCustomersBySalesOrgCode?SessionUserCode=" + CustomerCode,
                    HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    Responsedata = response.Data;
                }

                if (Responsedata.Any()) // Check if Responsedata has elements
                {
                    selectedValue1 = Responsedata.First().UID + "|" + Responsedata.First().Name + "|" + Responsedata.First().Code;
                    selectedValueText = Responsedata.First().Name;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " collection.razor exception");
            }
        }
        public override async Task<string> AddEarlyPayment(IEarlyPaymentDiscountConfiguration EarlyPayment)
        {
            try
            {
                EarlyPayment.Discount_Type = "Percentage";
                EarlyPayment.IsActive = false;
                EarlyPayment.Applicable_OnOverDue_Customers = false;
                EarlyPayment.Applicable_OnPartial_Payments = false;
                EarlyPayment.Created_By = _appUser.Emp.UID;
                EarlyPayment.Modified_By = _appUser.Emp.UID;
                Winit.Shared.Models.Common.ApiResponse<string> response = await _apiService.FetchDataAsync<string>($"{_appConfig.ApiBaseUrl}CollectionModule/AddEarlyPayment", HttpMethod.Post, EarlyPayment);
                if (response.StatusCode == 200)
                {
                    return response.Data;
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " AddEarlyPayment.razor exception");
            }
        }

        public override async Task<List<IEarlyPaymentDiscountConfiguration>> GetConfigurationDetails()
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
           $"{_appConfig.ApiBaseUrl}CollectionModule/GetConfigurationDetails", HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    Winit.Shared.Models.Common.ApiResponse<List<EarlyPaymentDiscountConfiguration>> response = JsonConvert.DeserializeObject<ApiResponse<List<EarlyPaymentDiscountConfiguration>>>(apiResponse.Data);
                    if (response != null)
                    {
                        return response.Data.ToList<IEarlyPaymentDiscountConfiguration>();
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }
    }
}

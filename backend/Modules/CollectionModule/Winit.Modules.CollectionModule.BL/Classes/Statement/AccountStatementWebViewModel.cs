using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.Statement
{
    public class AccountStatementWebViewModel : AccountStatementBaseViewModel
    {
        protected ApiService _apiService;
        public AccountStatementWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            _apiService = apiService;
        }
        public override async Task StatementReportCustomers(string CustomerCode)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<Winit.Modules.Store.Model.Classes.Store[]> response = await _apiService.FetchDataAsync<Winit.Modules.Store.Model.Classes.Store[]>(
                    $"{_appConfig.ApiBaseUrl}CollectionModule/GetAllCustomersBySalesOrgCode?SessionUserCode=" + CustomerCode,
                    HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    _UsersList = response.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " Statementreport.razor exception");
            }
        }
        public override async Task ViewAccountStatement(string FromDate, string ToDate, string Customer)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<Model.Classes.AccStoreLedger>> response = await _apiService.FetchDataAsync<List<Model.Classes.AccStoreLedger>>($"{_appConfig.ApiBaseUrl}CollectionModule/GetAccountStatement?StoreUID=" + Customer + "&FromDate=" + FromDate + "&ToDate=" + ToDate, HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    oStatementDisplay = response.Data;
                }

                Winit.Shared.Models.Common.ApiResponse<List<Model.Classes.AccPayable>> responsepay = await _apiService.FetchDataAsync<List<Model.Classes.AccPayable>>($"{_appConfig.ApiBaseUrl}CollectionModule/GetAccountStatementPay?StoreUID=" + Customer, HttpMethod.Get, null);
                if (responsepay != null && responsepay.Data != null)
                {
                    oStatementDisplayPayable = responsepay.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " Statementreport.razor exception");
            }
        }
    }
}

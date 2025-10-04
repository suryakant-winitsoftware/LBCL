using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.DashBoard.BL.Interfaces;
using Winit.Modules.DashBoard.Model.Classes;
using Winit.Modules.DashBoard.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.DashBoard.BL.Classes
{
    public class BranchSalesReportWebViewModel : BranchSalesReportBaseViewModel
    {
        ApiService _apiService { get; }
        IAppConfig _appConfig { get; }
        public BranchSalesReportWebViewModel(ApiService apiService, IAppConfig appConfig)
        {
            _apiService = apiService;
            _appConfig = appConfig;
        }

        protected override async Task GetBranchSalesReport()
        {
            ApiResponse<PagedResponse<IBranchSalesReport>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<IBranchSalesReport>>(
                    $"{_appConfig.ApiBaseUrl}DashBoard/GetBranchSalesReport",
                    HttpMethod.Post, PagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                BranchSalesReport.Clear();
                BranchSalesReport.AddRange(apiResponse.Data.PagedData);
                TotalItems = apiResponse.Data.TotalCount;
            }
        }
        public  async Task<List<IBranchSalesReport>> GetBranchSalesReportExport()
        {
            ApiResponse<PagedResponse<IBranchSalesReport>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<IBranchSalesReport>>(
                    $"{_appConfig.ApiBaseUrl}DashBoard/GetBranchSalesReport?isForExport={true}",
                    HttpMethod.Post, PagingRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null ? apiResponse.Data.PagedData.ToList() : [];
         
        }
        public  async Task GetAsmViewByBranchCode(string branchCode)
        {
            
            ApiResponse<List<IBranchSalesReportAsmview>> apiResponse =
                await _apiService.FetchDataAsync<List<IBranchSalesReportAsmview>>(
                    $"{_appConfig.ApiBaseUrl}DashBoard/GetAsmViewByBranchCode?branchCode={CommonFunctions.GetEncodedStringValue(branchCode)}",
                    HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                BranchSalesReportAsmviews.Clear();
                BranchSalesReportAsmviews.AddRange(apiResponse.Data);
            }
        }
        public  async Task GetOrgViewByBranchCode(string branchCode)
        {
            ApiResponse<List<IBranchSalesReportOrgview>> apiResponse =
                await _apiService.FetchDataAsync<List<IBranchSalesReportOrgview>>(
                    $"{_appConfig.ApiBaseUrl}DashBoard/GetOrgViewByBranchCode?branchCode={CommonFunctions.GetEncodedStringValue(branchCode)}",
                    HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                BranchSalesReportOrgviews.Clear();
                BranchSalesReportOrgviews.AddRange(apiResponse.Data);
            }
        }
    }
}

using Microsoft.AspNetCore.Components;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Role.BL.Interfaces;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Role.BL.Classes;

public class PermissionDataHelper:IPermissionDataHelper
{
    ApiService _apiService;
    IAppConfig _appConfig;
    public IAppUser _appUser { get; }
    NavigationManager _navigationManager;
    public PermissionDataHelper(ApiService apiService, IAppConfig appConfig, IAppUser appUser, NavigationManager navigationManager)
    {
        _apiService = apiService;
        _appConfig = appConfig;
        _appUser = appUser;
        _navigationManager = navigationManager;
    }
    public async Task<IPermission> GetPermissionByPage(string roleUID,bool isPrincipLeRole,string pageRoute)
    {
       

        ApiResponse<IPermission> apiResponse =
              await _apiService.FetchDataAsync<IPermission>(
              $"{_appConfig.ApiBaseUrl}Role/GetPermissionByRoleAndPage?roleUID={roleUID}&relativePath={pageRoute}&isPrincipleRole={isPrincipLeRole}",
              HttpMethod.Get);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            return apiResponse.Data;
        }
        return default;
    }
    public async Task<List<ISKUV1>> GetAllSKUs(PagingRequest pagingRequest)
    {
        try
        {
            ApiResponse<PagedResponse<ISKUV1>> apiResponse =
               await _apiService.FetchDataAsync<PagedResponse<ISKUV1>>(
               $"{_appConfig.ApiBaseUrl}SKU/SelectAllSKUDetails",
               HttpMethod.Post, pagingRequest);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return [];
    }
}

using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.UIClasses
{
    public class SKUClassGroupWebViewModel : SKUClassGroupBaseViewModel
    {
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private readonly Winit.Modules.Base.BL.ApiService _apiService;

        public SKUClassGroupWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            IAppConfig appConfigs,
            Base.BL.ApiService apiService) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting)
        {
            _appConfigs = appConfigs;
            _apiService = apiService;
        }

        protected override async Task<List<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup>> GetSKUClassGroups()
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageSize = PageSize;
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.IsCountRequired = true;
                pagingRequest.FilterCriterias = SKUClassGroupFilterCriterias;
                pagingRequest.SortCriterias = SKUClassGroupSortCriterials;
                if (pagingRequest.SortCriterias != null)
                {
                    var dSortCriteria = pagingRequest.SortCriterias.Find(p => p.SortParameter == "ModifiedTime");
                    if (dSortCriteria == null)
                    {
                        pagingRequest.SortCriterias.Add(new SortCriteria("ModifiedTime", SortDirection.Desc));
                    }
                }

                ApiResponse<PagedResponse<Winit.Modules.SKUClass.Model.Classes.SKUClassGroup>> apiResponse =
                    await _apiService
                        .FetchDataAsync<PagedResponse<Winit.Modules.SKUClass.Model.Classes.SKUClassGroup>>(
                            $"{_appConfigs.ApiBaseUrl}SKUClassGroup/SelectAllSKUClassGroupDetails",
                            HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    TotalSKUClassGroupItemsCount = apiResponse.Data.TotalCount;
                    return apiResponse.Data.PagedData.OfType<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup>()
                        .ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return new();
        }

        protected async override Task<bool> DeleteSKUClassGroupMaster(string skuClassGroupUID)
        {
            try
            {
                ApiResponse<string> apiResponse =
                    await _apiService.FetchDataAsync(
                        $"{_appConfigs.ApiBaseUrl}SKUClassGroup/DeleteSKUClassGroupMaster?sKUClassGroupUID={skuClassGroupUID}",
                        HttpMethod.Delete);
                if (apiResponse != null)
                {
                    return apiResponse.IsSuccess;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
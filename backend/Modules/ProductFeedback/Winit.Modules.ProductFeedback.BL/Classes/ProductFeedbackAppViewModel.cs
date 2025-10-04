using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.StoreMaster.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ProductFeedback.BL.Classes
{
    public class ProductFeedbackAppViewModel : ProductFeedbackBaseViewModel
    {
        Winit.Modules.FileSys.BL.Interfaces.IFileSysBL _fileSysBL { set; get; }
        IAppUser _appUser { set; get; }
        ApiService _apiService { set; get; }
        Shared.Models.Common.IAppConfig _appConfig { set; get; }
        IStoreBL _storeBL { set; get; }
        ISKUBL _skuBL { get; set; }
        public ProductFeedbackAppViewModel(IServiceProvider serviceProvider, IFilterHelper filter,
            ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting,
            IDataManager dataManager, Shared.Models.Common.IAppConfig appConfig, ApiService apiService, IStoreBL storeBL, Winit.Modules.FileSys.BL.Interfaces.IFileSysBL fileSysBL, ISKUBL skuBL)
            : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfig)
        {
            _appUser = appUser;
            _apiService = apiService;
            _appConfig = appConfig;
            _storeBL = storeBL;
            _skuBL = skuBL;
            _fileSysBL = fileSysBL;
        }

        public override async Task PopulateViewModel()
        {
            try
            {
                //store data
                var data  = await _storeBL.GetStoreByRouteUIDWithoutAddress(_appUser.SelectedRoute.UID);
                storeItemViews.AddRange(CommonFunctions.ConvertToSelectionItems(data, e => e.UID, e => e.Code, e => $"[{e.Code}] {e.Name}"));

                //sku data
                var sortCriterias = new List<SortCriteria>();
                var pageNumber = 1;
                var pageSize = int.MaxValue;
                var filterCriterias = new List<FilterCriteria>();
                var isCountRequired = false;

                SkuMasterItems = await _skuBL.PrepareSKUMaster(_appUser.OrgUIDs, null, null, null);
                SkuItems = SkuMasterItems.Select(p => p.SKU).ToList();


                skuItemViews.AddRange(CommonFunctions.ConvertToSelectionItems(SkuMasterItems.ToList(), e => e.SKU.UID, e => e.SKU.Code, e => $"{e.SKU.Name}"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override async Task SaveFileSys(bool IsSuccess, string ProductFeedbackUID)
        {
            try
            {
                if (IsSuccess && ImageFileSysList != null && ImageFileSysList.Any())
                {
                    try
                    {
                        ImageFileSysList?.ForEach(e => e.LinkedItemUID = $"{ProductFeedbackUID}");
                        await SaveCapturedImagesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error while saving captured images: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task SaveCapturedImagesAsync()
        {
            // Save the images in bulk using the provided file system logic
            int saveResult = await _fileSysBL.CreateFileSysForBulk(ImageFileSysList);

            // Throw an exception if the image save operation failed
            if (saveResult < 0)
            {
                throw new Exception("Failed to save the captured images.");
            }
        }
    }
}

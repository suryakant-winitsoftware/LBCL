using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.ExpiryCheck.BL.Interfaces;
using Winit.Modules.ExpiryCheck.Model.Classes;
using Winit.Modules.ExpiryCheck.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.ExpiryCheck.BL.Classes
{
    public class ExpiryCheckAppViewModel : ExpiryCheckBaseViewModel
    {
        IAppUser _appUser { set; get; }
        ApiService _apiService { set; get; }
        IExpiryCheckExecutionBL _expiryCheckExecutionBL { get; set; }
        Shared.Models.Common.IAppConfig _appConfig { set; get; }
        private readonly ISKUBL _skuBL;

        public ExpiryCheckAppViewModel(IServiceProvider serviceProvider, IFilterHelper filter,
            ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting,
            IDataManager dataManager, Shared.Models.Common.IAppConfig appConfig, ApiService apiService, 
            IExpiryCheckExecutionBL expiryCheckExecutionBL, ISKUBL skuBL)
            : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfig)
        {
            _appUser = appUser;
            _apiService = apiService;
            _appConfig = appConfig;
            _expiryCheckExecutionBL = expiryCheckExecutionBL;
            _skuBL = skuBL;
        }
        public override async Task PopulateViewModel()
        {
            try
            {
                await PopulateSkus();
                expiryCheckHeader = new ExpiryCheckExecution
                {
                    Id = 0,
                    UID = Guid.NewGuid().ToString(),
                    CreatedBy = _appUser.Emp.UID,
                    ModifiedBy = _appUser.Emp.UID,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    BeatHistoryUID = _appUser.SelectedBeatHistory.UID,
                    StoreHistoryUID = _appUser.SelectedCustomer.StoreHistoryUID,
                    RouteUID = _appUser.SelectedRoute.UID,
                    StoreUID = _appUser.SelectedCustomer.StoreUID,
                    JobPositionUID = _appUser.SelectedJobPosition.UID,
                    EmpUID = _appUser.Emp.UID,
                    ExecutionTime = DateTime.Now,
                    SS = 1 // Active status
                };
                expiryCheckHeader.Lines = new List<IExpiryCheckExecutionLine>();
            }
            catch (Exception ex)
            {

            }

        }
        public override async Task<string> OnSubmitExpiryCheck()
        {
            try
            {
                return await _expiryCheckExecutionBL.CreateAsync(expiryCheckHeader);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task PopulateSkus()
        {
            try
            {
                // Get SKUs from SKUMaster
                List<ISKUMaster> SkuMasterItems = await _skuBL.PrepareSKUMaster(_appUser.OrgUIDs, null, null, null);

                // Filter SKUs based on AllowedSKUs if the list exists
                if (_appUser?.SelectedCustomer?.AllowedSKUs != null && _appUser.SelectedCustomer.AllowedSKUs.Any())
                {
                    SkuMasterItems = SkuMasterItems.Where(sku => _appUser.SelectedCustomer.AllowedSKUs.Contains(sku.SKU.UID)).ToList();
                }

                // Convert SKUMaster items to ExpiryCheckItem format
                availableProducts = SkuMasterItems.Select(skuMaster => new ExpiryCheckItem 
                { 
                    SKUCode = skuMaster.SKU.Code,
                    SKUName = skuMaster.SKU.Name,
                    UOM = skuMaster.SKU.BaseUOM,
                    LastVisitPrice = 0.0M,
                    RSP = 0.0M,
                    ShelfPrice = 0.0M,
                    SKUImage = !string.IsNullOrEmpty(skuMaster.SKU.SKUImage) 
                        ? _appConfig.ApiDataBaseUrl + skuMaster.SKU.SKUImage 
                        : _appConfig.ApiDataBaseUrl + "Data/SKU/no_image_available.jpg",
                    SKUUID = skuMaster.SKU.UID
                }).ToList();

                /* Original hardcoded list commented out for reference
                availableProducts = new List<ExpiryCheckItem>
                {
                    new ExpiryCheckItem { SKUCode = "Makhana_7-2663", SKUName = "Aachari Munchies Farmley Pillow Pouch 33 g", UOM = "PCS", LastVisitPrice = 0.0M, RSP = 0.0M, ShelfPrice = 0.0M, SKUImage = _appConfig.ApiDataBaseUrl + "Data/SKU/Makhana_7-2663.jpg", SKUUID = "Makhana_7-2663" },
                    new ExpiryCheckItem { SKUCode = "Makhana_7-30037", SKUName = "Achaari Munchies Farmley Pillow Pouch 25g-Ladi", UOM = "PCS", LastVisitPrice = 0.0M, RSP = 0.0M, ShelfPrice = 0.0M, SKUImage = _appConfig.ApiDataBaseUrl + "Data/SKU/Makhana_7-30037.jpg", SKUUID = "Makhana_7-30037" },
                    // ... rest of the hardcoded items ...
                };
                */

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
    }
}

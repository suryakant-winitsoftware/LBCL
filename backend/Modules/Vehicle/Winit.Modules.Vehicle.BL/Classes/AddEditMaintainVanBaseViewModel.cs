using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.Vehicle.BL.Interfaces;
using Winit.Modules.Base.Model;
using Newtonsoft.Json;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Vehicle.BL.Classes
{
    public abstract class AddEditMaintainVanBaseViewModel:IAddEditMaintainVanViewModel
    {
        public List<SelectionItem> VanTypeSelectionItems { get; set; }
        public string OrgUID { get; set; }
        public IVehicle VEHICLE { get; set; }
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private List<string> _propertiesToSearch = new List<string>();
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public AddEditMaintainVanBaseViewModel(IServiceProvider serviceProvider,
             IFilterHelper filter,
             ISortHelper sorter,
             IListHelper listHelper,
             IAppUser appUser,
             IAppSetting appSetting,
             IDataManager dataManager,
             Winit.Shared.Models.Common.IAppConfig appConfigs,
             Base.BL.ApiService apiService
         )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            // Initialize common properties or perform other common setup
            VEHICLE = new Winit.Modules.Vehicle.Model.Classes.Vehicle();
            VanTypeSelectionItems = new List<SelectionItem>();

            // Property set for Search
            
            _appConfigs = appConfigs;
            _apiService = apiService;
        }
        public async Task SaveUpdateVanItem(IVehicle vehicle, bool Iscreate)
        {
            if (Iscreate)
            {
                vehicle.CompanyUID = _appUser?.Emp?.CompanyUID;
                vehicle.OrgUID = OrgUID;
                vehicle.Type = "Truck";
                await CreateUpdateVanData(vehicle, true);
            }
            else
            {
                await CreateUpdateVanData(vehicle, false);
            }
        }
       
        public async virtual Task PopulateViewModel(string uid)
        {
            await PopulateMaintainVanEditDetails(uid);
        }
        //public async void InstilizeFieldsForEditPage(IVehicle vehicleDetails)
        //{ 
        //    if (vehicleDetails == null)
        //    {
        //        await SetEditForVehicle(vehicleDetails);
        //    }
        //}
        //public async Task SetEditForVehicle(IVehicle editvehicle)
        //{

        //    VEHICLE = editvehicle;
        //}
        public async Task PopulateMaintainVanEditDetails(string vehicleuid)
        {
            VEHICLE = await GetMaintainVanEditDetails(vehicleuid);
        }
       
        public abstract Task<bool> CreateUpdateVanData(Winit.Modules.Vehicle.Model.Interfaces.IVehicle vehicle, bool IsCreate);
        public abstract Task<Winit.Modules.Vehicle.Model.Interfaces.IVehicle> GetMaintainVanEditDetails(string vehicleuid);

    }
}

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Route.BL.Classes
{
    public class AddEditRouteLoadBaseViewModel : IAddEditRouteLoadViewModel
    {
        public List<string> AllUIDs { get; set; }
        public List<string> ExistingUIDs { get; set; }
        public List<Modules.Route.Model.Classes.Route> RouteList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public List<ISelectionItem> RouteListForSelection { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SkuUOMList { get; set; }

        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        public List<Winit.Modules.SKU.Model.Classes.SKUMasterData> SkuMasterData;
        public IServiceProvider _serviceProvider;
        public IFilterHelper _filter;
        public ISortHelper _sorter;
        //private readonly IPromotionManager _promotionManager;
        //private readonly Interfaces.IReturnOrderAmountCalculator _amountCalculator;
        public IListHelper _listHelper;
        IEnumerable<ISKUMaster> SKUMasterList;
        public IAppUser _appUser;
        public List<string> _propertiesToSearch = new List<string>();
        public IAppConfig _appConfigs;
        public Base.BL.ApiService _apiService;
        public RouteLoadTruckTemplateViewDTO DisplayRouteLoadTruckTemplateViewDTO { get; set; }
        public RouteLoadTruckTemplateViewDTO FilterRouteLoadTruckTemplateViewDTO { get; set; }

        public string SelectedRoute { get; set; }
        public int LineNumber { get; set; }
        public string SelectedRouteUID { get; set; }
        public string SelectedRouteName { get; set; }

        public string RouteLoadTruckTemplateUID { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }
        public AddEditRouteLoadBaseViewModel(IServiceProvider serviceProvider,
     IFilterHelper filter,
     ISortHelper sorter,
     IListHelper listHelper,
     IAppUser appUser,
        IAppConfig appConfigs,
     Base.BL.ApiService apiService
          )
        {

            _filter = filter;
            _sorter = sorter;
            _serviceProvider = serviceProvider;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            _appUser = appUser;

        }
        public virtual async Task PopulateViewModel(string apiParam = null)
        {
            throw new NotImplementedException();
        }
        public virtual async Task<bool> CreateUpdateIRouteLoadTruckTemplateDTO()
        {
            throw new NotImplementedException();
        }
        public virtual async Task OnselectRoue(DropDownEvent dropDown)
        {
            throw new NotImplementedException();
        }
        public virtual async Task GetRoutes()
        {
            throw new NotImplementedException();
        }
        public virtual async Task GetSKUMasterData()
        {
            throw new NotImplementedException();
        }
        public virtual Task ApplySearch(string searchString)
        {
            throw new NotImplementedException();
        }
        public virtual void CreateRouteLoadTruckTemplate()
        {
            throw new NotImplementedException();
        }

        public virtual void CreateInstancesOfTemplateDTO()
        {
            throw new NotImplementedException();
        }
        public virtual void CreateRouteLoadTruckTemplateLine(ISelectionItem selectionItem)
        {
            throw new NotImplementedException();
        }
        public virtual void MatchingNewExistingUIDs()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> DeleteSelectedTemplates()
        {
            throw new NotImplementedException();
        }
    }
}

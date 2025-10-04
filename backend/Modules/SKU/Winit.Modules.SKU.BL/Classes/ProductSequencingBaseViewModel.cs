using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes
{
    public class ProductSequencingBaseViewModel : IProductSequencingViewModel
    {
        public List<FilterCriteria> FilterCriterias { get; set; }
        public IServiceProvider _serviceProvider;
        public  IFilterHelper _filter;
        public  ISortHelper _sorter;
        //private readonly IPromotionManager _promotionManager;
        //private readonly Interfaces.IReturnOrderAmountCalculator _amountCalculator;
        public  IListHelper _listHelper;
        IEnumerable<ISKUMaster> SKUMasterList;
        public  IAppUser _appUser;
        public List<string> _propertiesToSearch = new List<string>();
        public Winit.Shared.Models.Common.IAppConfig _appConfigs;
        public Winit.Modules.Base.BL.ApiService _apiService;
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        // private Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderBL _returnOrderBL;
        public Winit.Modules.SKU.BL.Interfaces.ISKUBL _sKUBL;
        public List<Winit.Modules.SKU.Model.Classes.SKUMasterData> SkuMasterData;
        public List<SkuSequence> SkuSequenceList { get; set; }
        public List<SkuSequence> SkuSequence { get; set; }
        public List<string> AllUIDs { get; set; }
        public List<string> ExistingUIDs { get; set; }
        public List<SkuSequenceUI> DisplaySkuSequencelist { get; set; }
        public List<SkuSequenceUI> FilterSkuSequencelist { get; set; }
        public ProductSequencingBaseViewModel(IServiceProvider serviceProvider,
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

        public virtual async Task PopulateViewModel(string dataNeededFor = null, string pageName = null, string apiParam = null, string apiName = null)
        {
            throw new NotImplementedException();

        }
        public virtual List<SkuSequence> PrepareListOfUidtForDelete()
        {
            throw new NotImplementedException();
        }
        public virtual async Task<bool> PrepareDataForSaveUpdate()
        {
            throw new NotImplementedException();
        }
        public virtual async Task ApplyFilter(Dictionary<string, string> filterCriterias, string ActiveTab)
        {
            throw new NotImplementedException();

        }
        public virtual async Task<bool> CreateUpdateDeleteSKUs(List<SkuSequence> skuSequenceList)
        {
            throw new NotImplementedException();
        }

        public virtual async Task PrepareDataForTable(string seqType)
        {
            throw new NotImplementedException();
        }
        public virtual void MatchingNewExistingUIDs()
        {
            throw new NotImplementedException();
        }


            public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKU>> FindCompleteSKU(IEnumerable<SKUMasterData> sKUMasters)
        {
            var skuList = new List<Winit.Modules.SKU.Model.Interfaces.ISKU>(); // Declare the list here

            try
            {
                if (sKUMasters != null)
                {
                    foreach (var skuMaster in sKUMasters)
                    {

                        skuList.Add(skuMaster.SKU);

                    }
                }
            }
            catch (Exception ex)
            {

            }

            return skuList;
        }
        public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>> FindCompleteSKUAttributes(IEnumerable<SKUMasterData> sKUMasters)
        {
            var skuAttributesList = new List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>(); // Declare the list here

            try
            {
                if (sKUMasters != null)
                {
                    foreach (var skuMaster in sKUMasters)
                    {
                        foreach (var skuAttributes in skuMaster.SKUAttributes)
                        {
                            skuAttributesList.Add(skuAttributes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
            }

            return skuAttributesList;
        }
    }
}

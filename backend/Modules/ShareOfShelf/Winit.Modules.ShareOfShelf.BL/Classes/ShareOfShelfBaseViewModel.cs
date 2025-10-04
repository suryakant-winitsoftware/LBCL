using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.ShareOfShelf.BL.Interfaces;
using Winit.Modules.ShareOfShelf.DL.Interfaces;
using Winit.Modules.ShareOfShelf.Model.Classes;
using Winit.Modules.ShareOfShelf.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ShareOfShelf.BL.Classes
{
    public abstract class ShareOfShelfBaseViewModel : IShareOfShelfViewModel
    {
        // Injection
        private IServiceProvider _serviceProvider;
        protected readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;

        private readonly IListHelper _listHelper;
        protected readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        protected readonly IDataManager _dataManager;
        private readonly IAppConfig _appConfig;
        public List<ISosLine> ShareOfShelfLines { get;  set; }
        public List<ISosHeaderCategoryItemView> SosHeaderCategory { get; set; }
        public ISosHeader SosHeader { get; set; }
        public string StoreHistoryUID { get; set; }


        //Constructor
        public ShareOfShelfBaseViewModel(IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper,
                IAppUser appUser,
                IAppSetting appSetting,
                IDataManager dataManager,
                IAppConfig appConfig
                )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;

            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            _appConfig = appConfig;
            ShareOfShelfLines = new List<ISosLine>();


        }
        public async Task PopulateViewModel()
        {
            await GetSosHeaderDetailsByStoreUID();
            if(SosHeader != null)
            {
                await GetTheAllCategoriesBySosHeaderUID(SosHeader.UID);
            }
            await Task.CompletedTask;
        }

        public void InitializeShareOfShelfLines(ISosHeaderCategoryItemView sosHeaderCategoryItemView)
        {
            if (sosHeaderCategoryItemView == null)
            {
                throw new InvalidOperationException("SosHeader must be initialized before initializing ShareOfShelfLines.");
            }
            ShareOfShelfLines.Clear();
            ShareOfShelfLines = new List<ISosLine>();

            
            var ourBrandLine = new SosLine
            {
                UID = GenerateUID(),
                SosHeaderCategoryUID = sosHeaderCategoryItemView.SOSHeaderCategoryUID ?? string.Empty,
                ItemGroupCode = "OurBrand", 
                ShelvesInMeter = 0,
                ShelvesInBlock = 0,
                TotalSpace = 0,
                CreatedBy = _appUser?.Emp?.UID,
                ModifiedBy = _appUser?.Emp?.UID,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                SS = 0
            };
            ShareOfShelfLines.Add(ourBrandLine);

            for (int i = 1; i <= 3; i++)
            {
                var competitorLine = CreateNewLine($"Competitor {i}", sosHeaderCategoryItemView.SOSHeaderCategoryUID ?? string.Empty);
                ShareOfShelfLines.Add(competitorLine);
            }
        }
        private string GenerateUID()
        {
            return Guid.NewGuid().ToString();
        }
        private SosLine CreateNewLine(string itemGroupCode, string headerUID)
        {
            return new SosLine
            {
                UID = GenerateUID(),
                SosHeaderCategoryUID = headerUID,
                ItemGroupCode = itemGroupCode,
                ShelvesInMeter = 0,
                ShelvesInBlock = 0,
                TotalSpace = 0,
                CreatedBy = _appUser?.Emp?.UID,
                ModifiedBy = _appUser?.Emp?.UID,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                SS = 0
            };
        }
        public virtual async Task<int> SaveLines()
        {
            await Task.CompletedTask;
            return 0;
        }
        public virtual async Task GetSosHeaderDetailsByStoreUID()
        {
            await Task.CompletedTask;
        }
        public virtual async Task GetTheAllCategoriesBySosHeaderUID(string SosHeader)
        {
            await Task.CompletedTask;
        }

        private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired = true)
        {
            baseModel.CreatedBy = _appUser?.Emp?.UID;
            baseModel.ModifiedBy = _appUser?.Emp?.UID;
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            baseModel.SS = 0;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
        {
            baseModel.ModifiedBy = _appUser?.Emp?.UID;
            baseModel.ModifiedTime = DateTime.Now;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.ListHeader.BL.Classes;
using Winit.Modules.ListHeader.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ListHeader.BL.Classes
{
    public abstract class ViewReasonsBaseViewModel:IViewReasonsViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        public Winit.Modules.ListHeader.Model.Interfaces.IListHeader UID { get; set; }
        public  Winit.Modules.ListHeader.Model.Interfaces.IListHeader Name { get; set; }
        public List<IListItem> listItems { get; set; }
        public IListItem listItem { get; set; }
        public List<IListHeader> ListHeaders { get; set; }
        public IListHeader listHeader { get; set; }
        public List<SortCriteria> SortCriterias { get; set; }

        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Modules.Common.BL.Interfaces.IAppUser _appUser;
        public ViewReasonsBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService, Common.BL.Interfaces.IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            ListHeaders = new List<IListHeader>();
            listItems = new List<IListItem>();
            SortCriterias = new List<SortCriteria>();
            _appUser = appUser;
        }
        public virtual async Task PopulateViewModel()
        {
            ListHeaders.Clear();
            ListHeaders.AddRange(await GetReasonsListHeaderData());
        }
        public async Task PageIndexChanged(int pageNumber)
        {
            PageNumber = pageNumber;
            await PopulateListItemData(ListHeaders[0].UID);
        }
        public async Task ApplySort(SortCriteria sortCriteria)
        {
            SortCriterias.Clear();
            SortCriterias.Add(sortCriteria);
            await PopulateListItemData(ListHeaders[0].UID);
        }
        public string uid;
        public async Task<bool> CheckUserExistsAsync(string code)
        {
            if (listItems == null || listItems.Count <= 0)
            {
                PopulateListItemData(uid);

            }
            return await Task.FromResult(
                listItems.Any(u => u.Code == code)
            );
        }
        public async Task PopulateListItemData(string uid)
        {
            listItems= await GetReasonsListItemData(uid);
        }
        public async Task PopulatetViewReasonsforEditDetailsData(string Uid)
        {
            listItem = await GetViewReasonsforEditDetailsData(Uid);
        }
        //public async Task SaveReasonsData()
        //{
        //    await SaveReasonsData_data();
        //}
        //public async Task UpdateViewReasons(string uid)
        //{
        //    //AppVersion.DeviceId = string.IsNullOrWhiteSpace(deviceid) ? "N/A" : deviceid;
        //    await UpdateViewReasons_data(listItem);
        //}
        public async Task SaveUpdateReasons(IListItem listItem, bool Iscreate)
        {
            if (Iscreate)
            {
                listItem.UID = Guid.NewGuid().ToString();
                //  string OrgUID= _appUser.SelectedJobPosition.OrgUID;
              
                await CreateUpdateReasonsData(listItem, true);
            }
            else
            {
                await CreateUpdateReasonsData(listItem, false);
            }
        }
        public async Task<string> DeleteViewReasonItem(string UID)
        {
            return await DeleteReason(UID);
        }
        public void AddCreateFields(IBaseModel baseModel, bool IsUIDRequired)
        {

            baseModel.CreatedBy = _appUser.Emp.UID;
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        public void AddUpdateFields(IBaseModel baseModel)
        {
            baseModel.ModifiedBy = _appUser.Emp.UID;
            baseModel.ModifiedTime = DateTime.Now;
        }
        public abstract Task<List<IListHeader>> GetReasonsListHeaderData();
        public abstract  Task<List<IListItem>> GetReasonsListItemData(string uid);
        public abstract Task<IListItem> GetViewReasonsforEditDetailsData(string orguid);
       // public abstract Task<bool> UpdateViewReasons_data(IListItem itemUID);
      //  public abstract Task<bool> SaveReasonsData_data();
        public abstract Task<string> DeleteReason(string uid);
        public abstract Task<bool> CreateUpdateReasonsData(Winit.Modules.ListHeader.Model.Interfaces.IListItem ListItem, bool IsCreate);

    }
}

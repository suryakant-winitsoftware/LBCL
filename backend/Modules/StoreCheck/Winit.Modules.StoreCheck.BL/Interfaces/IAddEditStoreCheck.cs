using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.BL.Interfaces
{
    public interface IAddEditStoreCheck
    {

        public string BeatHistoryUID { get; set; }

        public string StoreHistoryUID { get; set; }
        Task PopulateViewModel(string apiParam = null);
        public List<IStoreCheckItemView> DisplayStoreCheckItemView { get; set; }
        Task GetStoreCheckHistory(string beatHistoryUID, string storeHistoryUID);
        public QtyCaptureMode QtyMode { get; set; }
        Task GetStoreCheckItemHistory(string storeCheckHistoryUID);
        public IStoreCheckHistoryView DisplayStoreCheckHistoryView { get; set; }
        public List<IStoreCheckItemHistoryViewList> DisplayStoreCheckItemHistoryListView { get; set; }
        public IStoreCheckItemUomQty DisplayStoreCheckItemUomQty { get; set; }
        Task<bool> CreateUpdateStoreCheck(string beatHistoryUID, string storeHistoryUID);
        //Task<IStoreCheckItemUomQty> GetStoreCheckItemUomQty(IStoreCheckItemHistoryViewList rowDetails);
        Task<List<IStoreCheckItemExpiryDREHistory>> GetStoreCheckItemExpiryDREHistory(string storeCheckItemHistoryUID);
        Task<bool> CreateStoreCheckItemUomQty(string skuUid, string storeCheckItemHistoryUID);
        public List<IStoreCheckItemExpiryDREHistory> DisplayStoreCheckItemExpiryDreHistory { get; set; }
        Task<bool> CreateStoreCheckItemDREHistory();
        List<IStoreCheckItemExpiryDREHistory> CreateStoreCheckItemExpiryDREHistory(IStoreCheckItemHistoryViewList rowDetails);
        Task<IStoreCheckItemUomQty> GetStoreCheckItemUomOuterQty(IStoreCheckItemHistoryViewList rowDetails);
        Task<IStoreCheckItemUomQty> GetStoreCheckItemUomBaseQty(IStoreCheckItemHistoryViewList rowDetails);
        Task<List<IStoreCheckItemHistoryViewList>> ItemSearch(string searchString);
        List<IFileSys> ImageFileSysList { get; set; }
        bool IsFocusSelected { get; set; }
        string SearchString { get; set; }
    }
}

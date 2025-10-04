using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ListHeader.BL.Interfaces
{
    public  interface IViewReasonsViewModel
    {
        public Winit.Modules.ListHeader.Model.Interfaces.IListHeader UID { get; set; }
        public Winit.Modules.ListHeader.Model.Interfaces.IListHeader Name { get; set; }
        public List<IListHeader> ListHeaders { get; set; }
        public IListHeader listHeader { get; set; }
        public List<IListItem> listItems { get; set; }
        public IListItem listItem { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItemsCount { get; set; }
        Task PopulatetViewReasonsforEditDetailsData(string Uid);
        Task PopulateViewModel();
        Task PopulateListItemData(string name);
        //Task SaveReasonsData();
        //Task UpdateViewReasons(string uid);
        Task SaveUpdateReasons(IListItem listItem, bool Iscreate);
        Task<string> DeleteViewReasonItem(string UID);
        Task ApplySort(SortCriteria sortCriteria);
        Task PageIndexChanged(int pageNumber);
        Task<bool> CheckUserExistsAsync(string code);

    }
}

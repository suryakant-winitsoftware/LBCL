using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.BL.Interfaces
{
    public interface IShareOfShelfViewModel
    {
        List<Winit.Modules.ShareOfShelf.Model.Interfaces.ISosLine> ShareOfShelfLines { get; set; }
        List<ISosHeaderCategoryItemView> SosHeaderCategory { get; set; }  
        Winit.Modules.ShareOfShelf.Model.Interfaces.ISosHeader SosHeader { get; set; }
        string StoreHistoryUID { get; set; }


        //methods
        Task PopulateViewModel();
        void InitializeShareOfShelfLines(ISosHeaderCategoryItemView sosHeaderCategoryItemView);
        Task<int> SaveLines();
        //Task<List<Winit.Modules.ShareOfShelf.Model.Interfaces.ISosLine>> GetShareOfShelfLines(string SosHeaderCategoryUID);
    }
}

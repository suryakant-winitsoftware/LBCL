using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ShareOfShelf.Model.Classes;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.DL.Interfaces
{
    public interface IShareOfShelfDL
    {
        Task<ISosHeader> GetSosHeaderDetails(string StoreUID);
        Task<IEnumerable<ISosHeaderCategoryItemView>> GetCategories(string SosHeaderUID);
        Task<IEnumerable<ISosLine>> GetShelfDataByCategoryUID(string CategoryUID);
        Task<int> SaveShelfData(IEnumerable<ISosLine> ShelfData);
    }
}

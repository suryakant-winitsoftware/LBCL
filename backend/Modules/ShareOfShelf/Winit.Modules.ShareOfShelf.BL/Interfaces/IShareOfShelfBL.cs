using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.BL.Interfaces
{
    public interface IShareOfShelfBL
    {
        Task<ISosHeader> GetSosHeaderDetails(string StoreUID);
        Task<IEnumerable<ISosHeaderCategoryItemView>> GetCategories(string SosHeaderUID);
        Task<IEnumerable<ISosLine>> GetShelfDataByCategoryUID(string CategoryUID);
        Task<int> SaveShelfData(IEnumerable<ISosLine> ShelfData);
    }
}

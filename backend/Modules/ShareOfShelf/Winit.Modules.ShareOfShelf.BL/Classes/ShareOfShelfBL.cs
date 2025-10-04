using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ShareOfShelf.BL.Interfaces;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.BL.Classes
{
    public class ShareOfShelfBL : IShareOfShelfBL
    {
        protected readonly Winit.Modules.ShareOfShelf.DL.Interfaces.IShareOfShelfDL _shareOfShelfDL;
        public ShareOfShelfBL(Winit.Modules.ShareOfShelf.DL.Interfaces.IShareOfShelfDL shareOfShelfDL)
        {
            _shareOfShelfDL = shareOfShelfDL;
        }

        public async Task<IEnumerable<ISosHeaderCategoryItemView>> GetCategories(string SosHeaderUID)
        {
           return  await _shareOfShelfDL.GetCategories(SosHeaderUID);
        }

        public async Task<IEnumerable<ISosLine>> GetShelfDataByCategoryUID(string CategoryUID)
        {
            return await _shareOfShelfDL.GetShelfDataByCategoryUID(CategoryUID);
        }

        public async Task<ISosHeader> GetSosHeaderDetails(string StoreUID)
        {
            return await _shareOfShelfDL.GetSosHeaderDetails(StoreUID);
        }

        public async Task<int> SaveShelfData(IEnumerable<ISosLine> ShelfData)
        {
            return await _shareOfShelfDL.SaveShelfData(ShelfData);
        }
    }
}

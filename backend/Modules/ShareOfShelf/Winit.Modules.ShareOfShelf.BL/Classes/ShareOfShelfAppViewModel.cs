using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.ShareOfShelf.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ShareOfShelf.BL.Classes
{
    public class ShareOfShelfAppViewModel : ShareOfShelfBaseViewModel
    {
        Winit.Modules.ShareOfShelf.BL.Interfaces.IShareOfShelfBL _shareOfShelfBL { get; set; }
        public ShareOfShelfAppViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter, ISortHelper sorter, IListHelper listHelper,
            IAppUser appUser, IAppSetting appSetting, IDataManager dataManager,
            IAppConfig appConfig, Interfaces.IShareOfShelfBL shareOfShelfBL)
            : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfig)
        {
            _shareOfShelfBL = shareOfShelfBL;
        }


        //Api calls
       

        public override async Task GetSosHeaderDetailsByStoreUID()
        {
            SosHeader = await _shareOfShelfBL.GetSosHeaderDetails(StoreHistoryUID);
        }
        public override async Task GetTheAllCategoriesBySosHeaderUID(string SosHeader)
        {
            var result = await _shareOfShelfBL.GetCategories(SosHeader);
            SosHeaderCategory = result.ToList();
        }

        public override async Task<int> SaveLines()
        {
            return await _shareOfShelfBL.SaveShelfData(ShareOfShelfLines);
        }

    }
}

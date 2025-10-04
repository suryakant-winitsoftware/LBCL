using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ProductFeedback.BL.Interfaces
{
    public interface IProductFeedbackViewModel
    {
        public List<ISelectionItem> storeItemViews { get; set; }
        public List<ISelectionItem> skuItemViews { get; set; }
        public List<ISKU> SkuItems { get; set; }
        public List<ISKUMaster> SkuMasterItems { get; set; }
        public List<IFileSys> ImageFileSysList { get; set; }
        public string FolderPathImages { get; set; }
        Task PopulateViewModel();
        Task SaveFileSys(bool IsSuccess, string ProductFeedbackUID);
    }
}

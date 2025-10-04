using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.StockAudit.Model.Classes;
using Winit.Modules.StockAudit.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.StockAudit.BL.Interfaces
{
    public interface IAddEditStockAudit
    {
        public List<IStockAuditItemView> SaleableStockAuditItemView { get; set; }
        public List<IStockAuditItemView> StockAuditItemViews { get; set; }
        public List<IStockAuditItemView> DisplaySaleableStockAudit { get; set; }

        public List<IStockAuditItemView> DisplayFocStockAudit { get; set; }
        public List<IStockAuditItemView> FilteredStockAuditItemViews { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SkuUOMList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        Task GetSKUMasterData();
        public string ActiveTab { get; set; }
        Task<bool> AddStock();
        Task AddClonedItemToList(IStockAuditItemView item);
        List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForClone(IStockAuditItemView stockAuditItemView);
        List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForDDL(IStockAuditItemView salesOrderItemView);
        Task<List<IStockAuditItemView>> ItemSearch(string searchString, List<IStockAuditItemView> StockAuditItemViews);

        Task RemoveItemFromList(IStockAuditItemView salesOrderItemView);
        public string SelectedRouteUID { get; set; }
        Task PopulateViewModel(string apiParam = null);
     
        public DateTime PageLoadTime { get; set; }
     
        public DateTime SaleConfirmTime { get; set; }
        public WHStockAuditRequestTemplateModel PendingWHStockAuditRequest { get; set; }
    }
}

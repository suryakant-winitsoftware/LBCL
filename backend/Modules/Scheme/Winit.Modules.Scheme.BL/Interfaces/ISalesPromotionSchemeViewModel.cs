using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISalesPromotionSchemeViewModel : ISchemeViewModelBase
    {
        public Winit.Modules.Scheme.Model.Interfaces.ISalesPromotionScheme SalesPromotion { get; set; }
        public List<Winit.Modules.Scheme.Model.Interfaces.ISalesPromotionScheme> SalesPromotions { get; set; }
        public bool ShowPOTab { get; set; }
        //public bool IsEditMode { get; set; }
        public bool IsViewMode { get; set; }
        string PromotionUID { get; set; }
        public string AttachedDocument { get; set; }
        List<ISelectionItem> ActivityType { get; set; }
        List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> ApprovedFiles { get; set; }
        List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> SalesPromotionFiles { get; set; }
        Task PopulateViewModel();
        Task PopulateViewModelForExecuteSales();
        Task<bool> CreateSalesPromotion();
        Task<bool> UpdateSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO);
        Task DeleteSalesPromotion();
        Task<bool> ApproveSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO);
        Task<bool> RejectSalesPromotion(SalesPromotionSchemeApprovalDTO salesPromotionSchemeApprovalDTO);
        void OnActivityTypeChanged(DropDownEvent dropDownEvent);
        Task SetEditModeOnClick();
    }
}

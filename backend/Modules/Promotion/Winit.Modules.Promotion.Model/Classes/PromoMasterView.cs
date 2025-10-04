using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromoMasterView
    {
        public bool IsNew { get; set; }
        public PromotionView PromotionView { get; set; }
        public List<PromoOrderView> PromoOrderViewList { get; set; }
        public List<PromoOrderItemView> PromoOrderItemViewList { get; set; }
        public List<PromoOfferView> PromoOfferViewList { get; set; }
        public List<PromoOfferItemView> PromoOfferItemViewList { get; set; }
        public List<PromoConditionView> PromoConditionViewList { get; set; }
        public List<ItemPromotionMapView> ItemPromotionMapViewList { get; set; }
        public ApprovalRequestItem? ApprovalRequestItem { get; set; }
        public ApprovalStatusUpdate? ApprovalStatusUpdate { get; set; }
        public List<ISchemeBranch> SchemeBranches { get; set; }
        public List<ISchemeBroadClassification> SchemeBroadClassifications { get; set; }
        public List<ISchemeOrg> SchemeOrgs { get; set; }
        public bool IsFinalApproval { get; set; }
        public PromotionVolumeCapView PromotionVolumeCap { get; set; }
        public List<PromotionHierarchyCapView> PromotionHierarchyCapViewList { get; set; }
        public List<PromotionPeriodCapView> PromotionPeriodCapViewList { get; set; }

    }
}

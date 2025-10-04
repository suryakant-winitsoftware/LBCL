using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Promotion.BL.Interfaces
{
    public interface IPromotionBL
    {
        Task<PagedResponse<Winit.Modules.Promotion.Model.Interfaces.IPromotion>> GetPromotionDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<int> CUDPromotionMaster(Winit.Modules.Promotion.Model.Classes.PromoMasterView promoMasterView);
        //Task<Winit.Modules.Promotion.Model.Interfaces.IPromoMaster> GetPromotionMasterByUID(string UID);
        Task<int> PopulateItemPromotionMap(string promotionUID);
        Task<int> CreateDMSPromotionByJsonData(List<string> applicablePromotions);
        Task<Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>?> GetDMSPromotionByPromotionUIDs(List<string> promotionUIDs);
        Task<List<string>?> GetApplicablePromotionUIDs(List<string> orgUIDs);
        Task<IEnumerable<Winit.Modules.Promotion.Model.Classes.DmsPromotion>> CreateDMSPromotionByPromotionUID(string PromotionUID);
        //Task<List<Dictionary<string, DmsPromotion>>> CreateDMSPromotionByPromoUID(string applicablePromotioListCommaSeparated, string promotionType);
        //Task<Dictionary<string, DmsPromotion>> CreateDMSPromotionByPromoUID(string applicablePromotioListCommaSeparated, string promotionType);

        Task<Winit.Modules.Promotion.Model.Classes.PromoMasterView> GetPromotionDetailsByUID(string UID);
        List<AppliedPromotionView> ApplyPromotion(string applicablePromotionUIDs, PromotionHeaderView promoHeaderView, 
            Dictionary<string, DmsPromotion> promoDictionary, PromotionPriority promotionPriority);
        Task<int> GetPromotionDetailsValidated(string PromotionUID,string OrgUID,string PromotionCode,string PriorityNo, bool isNew);
        Task<int> DeletePromotionDetailsByUID(string PromotionUID);
        Task<Dictionary<string, List<string>>?> LoadStorePromotionMap(List<string> orgUIDs);
        Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>> SelectItemPromotionMapByPromotionUIDs(List<string> promotionUIDs);
        Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IPromotionData>?> GetPromotionData();
        Task<int> UpdatePromotion(Winit.Modules.Promotion.Model.Classes.PromotionView updatePromotionView);
        Task<int> DeletePromoOrderItems(List<string> UIDs);
        Task<int> DeletePromotionSlabByPromoOrderUID(string promoOrderUID);
        Task<int> ChangeEndDate(PromotionView promotionView);
    }
}

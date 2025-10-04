using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Promotion.DL.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Promotion.DL.Classes
{
    public class SQlitePromotionDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IPromotionDL
    {
        public SQlitePromotionDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public List<AppliedPromotionView> ApplyPromotion(string applicablePromotionUIDs, PromotionHeaderView promoHeaderView, 
            Dictionary<string, DmsPromotion> promoDictionary, PromotionPriority promotionPriority)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateDMSPromotionByJsonData(DmsPromotion dmsPromotion)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateDMSPromotionByJsonData(List<string> applicablePromotions)
        {
            throw new NotImplementedException();
        }

        public Task<int> PopulateItemPromotionMap(string promotionUID)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DmsPromotion>> CreateDMSPromotionByPromotionUID(string PromotionUID)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, DmsPromotion>> CreateDMSPromotionByPromoUID(string applicablePromotioListCommaSeparated, string promotionType)
        {
            throw new NotImplementedException();
        }

        public Task<int> CUDPromotionMaster(PromoMasterView promoMasterView)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeletePromotionDetailsByUID(string PromotionUID)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>?> GetApplicablePromotionUIDs(List<string> orgUIDs)
        {
            List<string>? promotionUIDList = null;
            try
            {
                string commaSeperatedUIDs = string.Join("','", orgUIDs);
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    
                };

                var query = string.Format(@"SELECT DISTINCT P.uid AS PromotionUID                   
                             FROM promotion P                        
                             INNER JOIN selection_map_criteria SC ON SC.linked_item_uid = P.uid  
                             AND P.org_uid IN ('{0}')                
                             AND SC.linked_item_type = 'Promotion'  
                             AND Date(P.valid_from) <= Date('now','localtime')
                             AND (P.valid_upto IS NULL OR Date(P.valid_upto) >= Date('now','localtime') )                 
                             AND P.is_active = 1                        
                             INNER JOIN selection_map_details SD ON SC.UID = SD.selection_map_criteria_uid                     
                             AND SD.selection_value IS NOT NULL", commaSeperatedUIDs);

                DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    promotionUIDList = new List<string>();
                    foreach (DataRow row in dt.Rows)
                    {
                        promotionUIDList.Add(ToString(row["PromotionUID"]));
                    }
                }
            }
            catch (Exception Ex)
            {
                throw;
            }
            return promotionUIDList;
        }

        public async Task<Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>?> GetDMSPromotionByPromotionUIDs(List<string> promotionUIDs)
        {
            Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>? dMSPromotionDictionary = null;
            try
            {

                string commaSeperatedUIDs = string.Join("','", promotionUIDs);
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                };

                var query = string.Format(@"SELECT * FROM promotion_data WHERE promotion_uid IN ('{0}')", commaSeperatedUIDs);

                DataTable dt = await ExecuteQueryDataTableAsync(query, parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    dMSPromotionDictionary = new Dictionary<string, DmsPromotion>();
                    foreach (DataRow row in dt.Rows)
                    {
                        string jsonData = ToString(row["Data"]);
                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            Winit.Modules.Promotion.Model.Classes.DmsPromotion? dmsPromotion = JsonConvert.DeserializeObject<Winit.Modules.Promotion.Model.Classes.DmsPromotion>(jsonData);
                            if (dmsPromotion != null)
                            {
                                dMSPromotionDictionary[dmsPromotion.UID] = dmsPromotion;
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                throw;
            }
            return dMSPromotionDictionary;
        }

        public Task<PagedResponse<IPromotion>> GetPromotionDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

        public Task<PromoMasterView> GetPromotionDetailsByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetPromotionDetailsValidated(string PromotionUID, string OrgUID, string PromotionCode, string PriorityNo, bool isNew)
        {
            throw new NotImplementedException();
        }
        public async Task<Dictionary<string, List<string>>?> LoadStorePromotionMap(List<string> orgUIDs)
        {
            Dictionary<string, List<string>>? storePromotionMapDictionary = null;

            try
            {
                string commaSeperatedUIDs = string.Join("','", orgUIDs);

                string query = string.Format(@"SELECT SPM.store_uid, SPM.promotion_uids FROM store_promotion_map SPM
                                            INNER JOIN Store S ON S.UID = SPM.store_uid
                                            AND S.franchisee_org_uid IN ('{0}')", commaSeperatedUIDs); 
                DataTable dt = await ExecuteQueryDataTableAsync(query);

                if (dt != null && dt.Rows.Count > 0)
                {
                    storePromotionMapDictionary = new Dictionary<string, List<string>>();
                    foreach (DataRow row in dt.Rows)
                    {
                        storePromotionMapDictionary[ToString(row["store_uid"])] = ToString(row["promotion_uids"]).Split(",").ToList();
                    }
                }
            }
            catch (Exception Ex)
            {
                throw;
            }
            return storePromotionMapDictionary;
        }
        public async Task<IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>> SelectItemPromotionMapByPromotionUIDs(List<string> promotionUIDs)
        {
            string commaSeperatedUIDs = string.Join("','", promotionUIDs);
            var sql = string.Format(@"SELECT uid AS UID, sku_type AS SKUType, sku_type_uid AS SKUTypeUID, promotion_uid AS PromotionUID 
            FROM item_promotion_map 
            WHERE promotion_uid IN ('{0}')", commaSeperatedUIDs);
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>().GetType();
            IEnumerable<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap> ItemPromotionMapDetails = await ExecuteQueryAsync<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap>(sql, null, type);
            return ItemPromotionMapDetails;
        }

        public Task<IEnumerable<IPromotionData>?> GetPromotionData()
        {
            throw new NotImplementedException();
        }
        public async Task<int> UpdatePromotion(Winit.Modules.Promotion.Model.Classes.PromotionView updatePromotionView)
        {
            throw new NotImplementedException();
        }
    }

}

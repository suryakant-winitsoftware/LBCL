using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ShareOfShelf.DL.Interfaces;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.DL.Classes
{
    public class SQLiteShareOfShelfDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IShareOfShelfDL
    {
        public SQLiteShareOfShelfDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<IEnumerable<ISosHeaderCategoryItemView>> GetCategories(string sosHeaderUID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?> { { "SosHeaderUID", sosHeaderUID } };
                var sql = """
                        SELECT  DISTINCT sg.code as BrandCode,shc.sos_header_uid AS SosHeaderUID,
                        shc.uid as SOSHeaderCategoryUID,
                        shc.item_group_uid AS ItemGroupUID,
                         fs.relative_path RelativePath, fs.display_name  as DisplayName
                        FROM sos_header_category shc
                        JOIN file_sys fs ON fs.linked_item_uid = shc.uid AND fs.linked_item_type = 'SHARE_SHELF'
                        JOIN sku_group sg ON sg.uid = shc.item_group_uid
                        --JOIN sku_group_type sgt ON sgt.uid = sg.sku_group_type_uid
                        WHERE shc.item_group_type = 'Brand' AND shc.sos_header_uid= @SosHeaderUID
                        """;
                return await ExecuteQueryAsync<ISosHeaderCategoryItemView>(sql, parameters);
            }
            catch
            {
                throw;
            }
          
        }
        public async Task<IEnumerable<ISosLine>> GetShelfDataByCategoryUID(string categoryUID)
        {
            try
            {
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "CategoryUID", categoryUID }
            };

                var sql = @"
                        SELECT 
                            id AS Id, 
                            uid AS UID, 
                            sos_header_category_uid AS SosHeaderCategoryUID, 
                            competitor_name AS CompetitorName,
                            shelves_in_meters AS ShelvesInMeters,
                            shelves_in_blocks AS ShelvesInBlocks,
                            total_space AS TotalSpace
                        FROM sos_line
                        WHERE sos_header_category_uid = @CategoryUID";

                var shelfData = await ExecuteQueryAsync<ISosLine>(sql, parameters);
                return shelfData;
            }
            catch
            {
                throw;
            }
          
        }

        public async Task<ISosHeader> GetSosHeaderDetails(string storeUID/*, DateTime activityDate*/)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "StoreUID", storeUID }
                //{ "ActivityDate", activityDate }
            };

            var sql = @"
                        SELECT 
                            id AS Id, 
                            uid AS UID, 
                            created_by AS CreatedBy, 
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy, 
                            modified_time AS ModifiedTime, 
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime, 
                            ss AS SS, 
                            store_uid AS StoreUID, 
                            is_completed AS IsCompleted,
                            store_history_uid AS StoreHistoryUID, 
                            beat_history_uid AS BeatHistoryUID, 
                            route_uid AS RouteUID,
                            activity_date AS ActivityDate, 
                            job_position_uid AS JobPositionUID, 
                            emp_uid AS EmpUID
                        FROM sos_header
                        WHERE store_uid = @StoreUID --AND activity_date = @ActivityDate";

            var sosHeader = await ExecuteSingleAsync<ISosHeader>(sql, parameters);
            return sosHeader;
        }

        public async Task<int> SaveShelfData(IEnumerable<ISosLine> shelfData)
        {
            int retValue = 0;

            if (shelfData == null || !shelfData.Any())
            {
                return retValue;
            }
            try
            {
                var sql = @"
                    INSERT INTO sos_line (
                         uid, created_by, created_time, modified_by, modified_time, 
                        server_add_time, server_modified_time, ss, sos_header_category_uid, 
                        item_group_code, shelves_in_meter, shelves_in_block, total_space
                    )
                    VALUES (
                         @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                        @ServerAddTime, @ServerModifiedTime, @SS, @SosHeaderCategoryUID, 
                        @ItemGroupCode, @ShelvesInMeter, @ShelvesInBlock, @TotalSpace
                    )";


                retValue = await ExecuteNonQueryAsync(sql, shelfData);
                return retValue;
            }
            catch (Exception ex) {
                throw ex;
            }
           
        }

    }
}

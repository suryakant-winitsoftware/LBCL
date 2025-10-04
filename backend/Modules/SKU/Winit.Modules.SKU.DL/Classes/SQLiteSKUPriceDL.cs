using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class SQLiteSKUPriceDL:Winit.Modules.Base.DL.DBManager.SqliteDBManager,ISKUPriceDL
    {
        public SQLiteSKUPriceDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string? type = null)
        {
            try
            {
                var sql = new StringBuilder("SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy," +
                                            "modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime," +
                                            "sku_price_list_uid AS SKUPriceListUID, sku_code AS SKUCode, uom AS UOM, price AS Price, default_ws_price AS DefaultWSPrice," +
                                            "default_ret_price AS DefaultRetPrice, dummy_price AS DummyPrice, mrp AS MRP, price_upper_limit AS PriceUpperLimit," +
                                            "price_lower_limit AS PriceLowerLimit, status AS Status, valid_from AS ValidFrom, valid_upto AS ValidUpto, is_active AS IsActive," +
                                            "is_tax_included AS IsTaxIncluded, version_no AS VersionNo, sku_uid AS SKUUID, is_latest AS IsLatest FROM sku_price");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM sku_price");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    // If count required then add filters to count
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }

                //if (pageNumber > 0 && pageSize > 0)
                //{ 
                //    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}

                //Data
                IEnumerable<Model.Interfaces.ISKUPrice> sKUPrices = await ExecuteQueryAsync<Model.Interfaces.ISKUPrice>(sql.ToString(), parameters);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>
                {
                    PagedData = sKUPrices,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> SelectSKUPriceByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                        sku_price_list_uid AS SKUPriceListUID, sku_code AS SKUCode, uom AS UOM, price AS Price, default_ws_price AS DefaultWSPrice,
                        default_ret_price AS DefaultRetPrice, dummy_price AS DummyPrice, mrp AS MRP, price_upper_limit AS PriceUpperLimit,
                        price_lower_limit AS PriceLowerLimit, status AS Status, valid_from AS ValidFrom, valid_upto AS ValidUpto, is_active AS IsActive,
                        is_tax_included AS IsTaxIncluded, version_no AS VersionNo, sku_uid AS SKUUID, is_latest AS IsLatest FROM sku_price 
                        FROM sku_price WHERE uid= @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUPrice>().GetType();
            Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPriceDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>(sql, parameters, type);
            return sKUPriceDetails;
        }
        public async Task<int> CreateSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice)
        {
            try
            {
                var sql = @"INSERT INTO sku_price (uid,created_by,created_time,modified_by,modified_time,server_add_time,server_modified_time,sku_price_list_uid,
                        sku_code,uom,price,default_ws_price,default_ret_price,dummy_price,mrp,price_upper_limit,price_lower_limit,status,valid_from,valid_upto,
                        is_active,is_tax_included,version_no,sku_uid)
                        VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@SKUPriceListUID ,@SKUCode ,@UOM ,@Price ,@DefaultWSPrice ,
                        @DefaultRetPrice ,@DummyPrice ,@MRP ,@PriceUpperLimit ,@PriceLowerLimit ,@Status ,@ValidFrom ,@ValidUpto ,@IsActive ,@IsTaxIncluded ,@VersionNo,@SKUUID);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",sKUPrice.UID},
                   {"CreatedBy",sKUPrice.CreatedBy},
                   {"CreatedTime",sKUPrice.CreatedTime},
                   {"ModifiedBy",sKUPrice.ModifiedBy},
                   {"ModifiedTime",sKUPrice.ModifiedTime},
                   {"ServerAddTime",sKUPrice.ServerAddTime},
                   {"ServerModifiedTime",sKUPrice.ServerModifiedTime},
                   {"SKUPriceListUID",sKUPrice.SKUPriceListUID},
                   {"SKUCode",sKUPrice.SKUCode},
                   {"UOM",sKUPrice.UOM},
                   {"Price",sKUPrice.Price},
                   {"DefaultWSPrice",sKUPrice.DefaultWSPrice},
                   {"DefaultRetPrice",sKUPrice.DefaultRetPrice},
                   {"DummyPrice",sKUPrice.DummyPrice},
                   {"MRP",sKUPrice.MRP},
                   {"PriceUpperLimit",sKUPrice.PriceUpperLimit},
                   {"PriceLowerLimit",sKUPrice.PriceLowerLimit},
                   {"Status",sKUPrice.Status},
                   {"ValidFrom",sKUPrice.ValidFrom},
                   {"ValidUpto",sKUPrice.ValidUpto},
                   {"IsActive",sKUPrice.IsActive},
                   {"IsTaxIncluded",sKUPrice.IsTaxIncluded},
                   {"VersionNo",sKUPrice.VersionNo},
                   {"SKUUID",sKUPrice.SKUUID}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUPriceList(List<Winit.Modules.SKU.Model.Classes.SKUPrice> sKUPrice)
        {
            throw new NotImplementedException();
        }
        public async Task<int> UpdateSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice)
        {
            try
            {
                var sql = @"UPDATE sku_price SET 
                           modified_by=@ModifiedBy
                           ,modified_time=@ModifiedTime
                           ,server_modified_time =@ServerModifiedTime
                           ,sku_price_list_uid =@SKUPriceListUID
                           ,sku_code =@SKUCode
                           ,uom =@UOM
                           ,price =@Price
                           ,default_ws_price=@DefaultWSPrice
                           ,default_ret_price =@DefaultRetPrice
                           ,dummy_price=@DummyPrice
                           ,mrp =@MRP
                           ,price_upper_limit =@PriceUpperLimit
                           ,price_lower_limit =@PriceLowerLimit
                           ,status =@Status
                           ,valid_from =@ValidFrom
                           ,valid_upto =@ValidUpto
                           ,is_active=@IsActive
                           ,is_tax_included=@IsTaxIncluded
                           ,version_no=@VersionNo
                             WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",sKUPrice.UID},
                   {"ModifiedBy",sKUPrice.ModifiedBy},
                   {"ModifiedTime",sKUPrice.ModifiedTime},
                   {"ServerModifiedTime",sKUPrice.ServerModifiedTime},
                   {"SKUPriceListUID",sKUPrice.SKUPriceListUID},
                   {"SKUCode",sKUPrice.SKUCode},
                   {"UOM",sKUPrice.UOM},
                   {"Price",sKUPrice.Price},
                   {"DefaultWSPrice",sKUPrice.DefaultWSPrice},
                   {"DefaultRetPrice",sKUPrice.DefaultRetPrice},
                   {"DummyPrice",sKUPrice.DummyPrice},
                   {"MRP",sKUPrice.MRP},
                   {"PriceUpperLimit",sKUPrice.PriceUpperLimit},
                   {"PriceLowerLimit",sKUPrice.PriceLowerLimit},
                   {"Status",sKUPrice.Status},
                   {"ValidFrom",sKUPrice.ValidFrom},
                   {"ValidUpto",sKUPrice.ValidUpto},
                   {"IsActive",sKUPrice.IsActive},
                   {"IsTaxIncluded",sKUPrice.IsTaxIncluded},
                   {"VersionNo",sKUPrice.VersionNo}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSKUPrice(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM sku_price WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

      
        public Task<(List<ISKUPriceList>, List<ISKUPrice>,int)> SelectSKUPriceViewByUID(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string skuPriceUID)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateSKUPriceView(SKUPriceViewDTO sKUPriceViewDTO)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateSKUPriceView(SKUPriceViewDTO sKUPriceViewDTO)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateStandardPriceForSKU(string skuUID)
        {
            throw new NotImplementedException();
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SelectAllSKUPriceDetails_BySKUUIDs(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, List<string> skuUIDs)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT 
                                                        sp.id AS Id,
                                                        sp.uid AS Uid,
                                                        sp.created_by AS CreatedBy,
                                                        sp.created_time AS CreatedTime,
                                                        sp.modified_by AS ModifiedBy,
                                                        sp.modified_time AS ModifiedTime,
                                                        sp.server_add_time AS ServerAddTime,
                                                        sp.server_modified_time AS ServerModifiedTime,
                                                        sp.sku_price_list_uid AS SkuPriceListUid,
                                                        sp.sku_code AS SkuCode,S.name as SKUName,
                                                        sp.uom AS Uom,
                                                        sp.price AS Price,
                                                        sp.default_ws_price AS DefaultWsPrice,
                                                        sp.default_ret_price AS DefaultRetPrice,
                                                        sp.dummy_price AS DummyPrice,
                                                        sp.mrp AS Mrp,
                                                        sp.price_upper_limit AS PriceUpperLimit,
                                                        sp.price_lower_limit AS PriceLowerLimit,
                                                        sp.status AS Status,
                                                        sp.valid_from AS ValidFrom,
                                                        sp.valid_upto AS ValidUpto,
                                                        sp.is_active AS IsActive,
                                                        sp.is_tax_included AS IsTaxIncluded,
                                                        sp.version_no AS VersionNo,
                                                        sp.sku_uid AS SkuUid,
                                                        sp.is_latest AS IsLatest
                                                    FROM sku_price sp
                                                      where sp.sku_uid IN @SKUUIDs
                                                    ) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) FROM(SELECT 
                                                        sp.id AS Id,
                                                        sp.uid AS Uid,
                                                        sp.created_by AS CreatedBy,
                                                        sp.created_time AS CreatedTime,
                                                        sp.modified_by AS ModifiedBy,
                                                        sp.modified_time AS ModifiedTime,
                                                        sp.server_add_time AS ServerAddTime,
                                                        sp.server_modified_time AS ServerModifiedTime,
                                                        sp.sku_price_list_uid AS SkuPriceListUid,
                                                        sp.sku_code AS SkuCode,
                                                        sp.uom AS Uom,
                                                        sp.price AS Price,
                                                        sp.default_ws_price AS DefaultWsPrice,
                                                        sp.default_ret_price AS DefaultRetPrice,
                                                        sp.dummy_price AS DummyPrice,
                                                        sp.mrp AS Mrp,
                                                        sp.price_upper_limit AS PriceUpperLimit,
                                                        sp.price_lower_limit AS PriceLowerLimit,
                                                        sp.status AS Status,
                                                        sp.valid_from AS ValidFrom,
                                                        sp.valid_upto AS ValidUpto,
                                                        sp.is_active AS IsActive,
                                                        sp.is_tax_included AS IsTaxIncluded,
                                                        sp.version_no AS VersionNo,
                                                        sp.sku_uid AS SkuUid,
                                                        sp.is_latest AS IsLatest,S.name as SKUName
                                                    FROM sku_price sp
                                                      where sp.sku_uid In @SKUUIDs)AS SUBQUERY");
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "SKUUIDs",skuUIDs }
                };

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<Model.Interfaces.ISKUPrice> sKUPrices = await ExecuteQueryAsync<Model.Interfaces.ISKUPrice>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>
                {
                    PagedData = sKUPrices,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<string>> GetApplicablePriceListByStoreUID(string storeUID, string storeType)
        {
            try
            {
                string sql = string.Empty;
                Dictionary<string, object> parameters = new Dictionary<string, object>
             {
             { "storeUID", storeUID }
             };

                if (storeType == WINITSharedObjects.Constants.StoreType.FR)
                {
                    sql = @"SELECT UID FROM sku_price_list 
                    WHERE selection_group = 'Org' 
                      AND selection_type = 'Org' 
                      AND selection_uid = @storeUID 
                      AND type = 'FR' 
                      AND is_active = true 
                      AND status = 'Published' 
                      AND getdate() BETWEEN valid_from AND valid_upto;";
                }
                else if (storeType == WINITSharedObjects.Constants.StoreType.FRC)
                {
                    // Need to change
                    sql = @"SELECT UID FROM sku_price_list 
                    WHERE selection_group = 'Org' 
                      AND selection_type = 'Org' 
                      AND selection_uid = @storeUID 
                      AND type = 'FR' 
                      AND is_active = true 
                      AND status = 'Published' 
                      AND getdate() BETWEEN valid_from AND valid_upto;";
                }

                return await ExecuteQueryAsync<string>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }
    }
}

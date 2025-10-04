using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes;

public class MSSQLSKUDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUDL
{
    public MSSQLSKUDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU>> SelectAllSKUDetails(List<SortCriteria> sortCriterias, int pageNumber,
                                                                                                  int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {

            var sql = new StringBuilder("""
                            SELECT * FROM (
                            SELECT 
                                S.id AS Id, 
                                S.uid AS UID, 
                                S.created_by AS CreatedBy, 
                                S.created_time AS CreatedTime, 
                                S.modified_by AS ModifiedBy, 
                                S.modified_time AS ModifiedTime, 
                                S.server_add_time AS ServerAddTime, 
                                S.server_modified_time AS ServerModifiedTime, 
                                S.company_uid AS CompanyUID, 
                                S.org_uid AS OrgUID, 
                                S.code AS Code, 
                                S.name AS Name, 
                                S.arabic_name AS ArabicName, 
                                S.alias_name AS AliasName, 
                                S.long_name AS LongName, 
                                S.base_uom AS BaseUOM, 
                                S.outer_uom AS OuterUOM, 
                                S.from_date AS FromDate,
                                S.to_date AS ToDate, 
                                S.is_stockable AS IsStockable, 
                                S.parent_uid AS ParentUID, 
                                S.is_active AS IsActive, 
                                S.is_third_party As IsThirdParty, 
                                S.supplier_org_uid AS SupplierOrgUID,
                                FSC.relative_path + '/' + FSC.file_name AS CatalogueURL,
                                '[' + O.code + '] ' + O.name AS L1, 
                                '[' + DC.code + '] ' + DC.name AS L2,
                                '[' + SAC.code + '] ' + SAC.value AS L3,
                                '[' + SASC.code + '] ' + SASC.value AS L4,
                                '[' + SAB.code + '] ' + SAB.value AS L5,
                                '[' + SASB.code + '] ' + SASB.value AS L6,
                                sed.model_code as modelcode,
                                sed.year,
                                sed.type,
                                sed.product_type,
                                sed.category,
                                sed.tonnage,
                                sed.capacity,
                                sed.star_rating as starrating,
                                sed.product_category_id as productcategoryid,
                                sed.product_category_name as productcategoryname,
                                sed.item_series as itemseries,
                                sed.hsn_code as hsncode,
                                sed.division
                            FROM sku S
                            LEFT JOIN file_sys FS ON FS.linked_item_type = 'SKU' AND FS.linked_item_uid = S.uid  
                                AND FS.file_sys_type = 'Image' AND FS.is_default = 1
                            LEFT JOIN file_sys FSC ON FSC.linked_item_type = 'SKU' AND FSC.linked_item_uid = S.uid
                                AND FSC.file_sys_type = 'Catalogue'
                            LEFT JOIN org O ON O.uid = S.org_uid
                            LEFT JOIN org DC ON DC.uid = S.supplier_org_uid	
                            LEFT JOIN sku_attributes SAC ON SAC.sku_uid = S.uid AND SAC.type = 'Category'
                            LEFT JOIN sku_attributes SASC ON SASC.sku_uid = S.uid AND SASC.type = 'Sub Category'
                            LEFT JOIN sku_attributes SAB ON SAB.sku_uid = S.uid AND SAB.type = 'Brand'
                            LEFT JOIN sku_attributes SASB ON SASB.sku_uid = S.uid AND SASB.type = 'Sub Brand'
                            LEFT JOIN sku_ext_data sed ON S.uid = sed.sku_uid 
                            WHERE S.is_active = 1) AS skus
                            """);
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder("""
                                            SELECT count(*) FROM (
                                            SELECT 
                                                S.id AS Id, 
                                                S.uid AS UID, 
                                                S.created_by AS CreatedBy, 
                                                S.created_time AS CreatedTime, 
                                                S.modified_by AS ModifiedBy, 
                                                S.modified_time AS ModifiedTime, 
                                                S.server_add_time AS ServerAddTime, 
                                                S.server_modified_time AS ServerModifiedTime, 
                                                S.company_uid AS CompanyUID, 
                                                S.org_uid AS OrgUID, 
                                                S.code AS Code, 
                                                S.name AS Name, 
                                                S.arabic_name AS ArabicName, 
                                                S.alias_name AS AliasName, 
                                                S.long_name AS LongName, 
                                                S.base_uom AS BaseUOM, 
                                                S.outer_uom AS OuterUOM, 
                                                S.from_date AS FromDate,
                                                S.to_date AS ToDate, 
                                                S.is_stockable AS IsStockable, 
                                                S.parent_uid AS ParentUID, 
                                                S.is_active AS IsActive, 
                                                S.is_third_party As IsThirdParty, 
                                                S.supplier_org_uid AS SupplierOrgUID,
                                                FSC.relative_path + '/' + FSC.file_name AS CatalogueURL,
                                                '[' + O.code + '] ' + O.name AS L1, 
                                                '[' + DC.code + '] ' + DC.name AS L2,
                                                '[' + SAC.code + '] ' + SAC.value AS L3,
                                                '[' + SASC.code + '] ' + SASC.value AS L4,
                                                '[' + SAB.code + '] ' + SAB.value AS L5,
                                                '[' + SASB.code + '] ' + SASB.value AS L6,
                                                sed.model_code as modelcode,
                                                sed.year,
                                                sed.type,
                                                sed.product_type,
                                                sed.category,
                                                sed.tonnage,
                                                sed.capacity,
                                                sed.star_rating as starrating,
                                                sed.product_category_id as productcategoryid,
                                                sed.product_category_name as productcategoryname,
                                                sed.item_series as itemseries,
                                                sed.hsn_code as hsncode,
                                                sed.division
                                            FROM sku S
                                            LEFT JOIN file_sys FS ON FS.linked_item_type = 'SKU' AND FS.linked_item_uid = S.uid  
                                                AND FS.file_sys_type = 'Image' AND FS.is_default = 1
                                            LEFT JOIN file_sys FSC ON FSC.linked_item_type = 'SKU' AND FSC.linked_item_uid = S.uid
                                                AND FSC.file_sys_type = 'Catalogue'
                                            LEFT JOIN org O ON O.uid = S.org_uid
                                            LEFT JOIN org DC ON DC.uid = S.supplier_org_uid	
                                            LEFT JOIN sku_attributes SAC ON SAC.sku_uid = S.uid AND SAC.type = 'Category'
                                            LEFT JOIN sku_attributes SASC ON SASC.sku_uid = S.uid AND SASC.type = 'Sub Category'
                                            LEFT JOIN sku_attributes SAB ON SAB.sku_uid = S.uid AND SAB.type = 'Brand'
                                            LEFT JOIN sku_attributes SASB ON SASB.sku_uid = S.uid AND SASB.type = 'Sub Brand'
                                            LEFT JOIN sku_ext_data sed ON S.uid = sed.sku_uid 
                                            WHERE S.is_active = 1) AS skus
                                            """);
            }
            var parameters = new Dictionary<string, object>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKU>(filterCriterias, sbFilterCriteria, parameters);

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
                    sql.Append($@" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }
            IEnumerable<Model.Interfaces.ISKU> skuList = await ExecuteQueryAsync<Model.Interfaces.ISKU>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU>
            {
                PagedData = skuList,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Winit.Modules.SKU.Model.Interfaces.ISKU> SelectSKUByUID(string UID)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
        var sql = @"SELECT 
                id AS Id,
                uid AS UID,
                ss AS SS,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                company_uid AS CompanyUID,
                org_uid AS OrgUID,
                code AS Code,
                name AS Name,
                arabic_name AS ArabicName,
                alias_name AS AliasName,
                long_name AS LongName,
                base_uom AS BaseUOM,
                outer_uom AS OuterUOM,
                from_date AS FromDate,
                to_date AS ToDate,
                is_stockable AS IsStockable,
                parent_uid AS ParentUID,
                is_active AS IsActive,
                is_third_party AS IsThirdParty,
                supplier_org_uid AS SupplierOrgUID
                --sku_image AS SKUImage,
                --catalogue_url AS CatalogueURL
            FROM 
                sku
            WHERE 
                uid = @UID";
        Winit.Modules.SKU.Model.Interfaces.ISKU skuDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKU>(sql, parameters);
        return skuDetails;
    }
    public async Task<int> CreateSKU(Winit.Modules.SKU.Model.Interfaces.ISKUV1 sku)
    {
        int retVal = -1;
        try
        {
            var sql = @"INSERT INTO sku (uid, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, company_uid, org_uid, code, name,
                        arabic_name, alias_name, long_name, base_uom, outer_uom, from_date,
                        to_date, is_stockable, parent_uid, is_active, is_third_party, supplier_org_uid)
                        VALUES
                        (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,
                        @CompanyUID,@OrgUID,@Code,@Name,@ArabicName,@AliasName,@LongName,@BaseUOM,@OuterUOM,@FromDate,
                        @ToDate,@IsStockable,@ParentUID,@IsActive,@IsThirdParty,@SupplierOrgUID);";
            retVal = await ExecuteNonQueryAsync(sql, sku);
            if (retVal > 0)
            {
                retVal += await CreateSKUExtData(sku);
            }
            return retVal;
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<int> CreateSKUExtData(Winit.Modules.SKU.Model.Interfaces.ISKU sku)
    {
        int retVal = -1;
        try
        {
            var sql = """
                                INSERT INTO public.sku_ext_data (id, uid, created_by, created_time, modified_by, modified_time,
                                server_add_time, server_modified_time, ss, sku_uid) 
                       VALUES ( @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @SkuUid);
                """;
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Uid",sku.UID},
                {"CreatedBy",sku.CreatedBy},
                {"CreatedTime",sku.CreatedTime},
                {"ModifiedBy",sku.ModifiedBy},
                {"ModifiedTime",sku.ModifiedTime},
                {"ServerAddTime",sku.ServerAddTime},
                {"ServerModifiedTime",sku.ServerModifiedTime},
                {"SS",sku.SS??0},
                {"SkuUid",sku.UID},
            };
            retVal = await ExecuteNonQueryAsync(sql, parameters);
            return retVal;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> UpdateSKU(Winit.Modules.SKU.Model.Interfaces.ISKU sku)
    {
        try
        {
            var sql = @"UPDATE sku 
                SET 
                    modified_by = @ModifiedBy, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime, 
                    company_uid = @CompanyUID, 
                    org_uid = @OrgUID, 
                    code = @Code, 
                    name = @Name, 
                    arabic_name = @ArabicName, 
                    alias_name = @AliasName, 
                    long_name = @LongName, 
                    base_uom = @BaseUOM, 
                    outer_uom = @OuterUOM, 
                    from_date = @FromDate, 
                    to_date = @ToDate, 
                    is_stockable = @IsStockable, 
                    parent_uid = @ParentUID, 
                    is_active = @IsActive, 
                    is_third_party = @IsThirdParty, 
                    supplier_org_uid = @SupplierOrgUID 
                WHERE 
                    uid = @UID;";
            return await ExecuteNonQueryAsync(sql, sku);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteSKU(string UID)
    {
        try
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "UID", UID }
        };

            var sql = @"
            DELETE FROM sku
            WHERE uid = @UID;

            DELETE FROM sku_attributes
            WHERE sku_uid = @UID;";

            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (SqlException ex)
        {
            if (ex.Number == 547)
            {
                throw new Exception("This SKU is already used by other processes, so you can't delete it.");
            }
            throw;
        }
        catch (Exception)
        {
            throw new Exception("An error occurred while deleting.");
        }
    }


    public async Task<(List<Model.Interfaces.ISKU>, List<Model.Interfaces.ISKUConfig>, List<Model.Interfaces.ISKUUOM>, List<Model.Interfaces.ISKUAttributes>, List<Model.Interfaces.ITaxSkuMap>)>
                PrepareSKUMaster(List<string> orgUIDs, List<string> DistributionChannelUIDs, List<string> skuUIDs, List<string> attributeTypes)
    {
        try
        {
            Dictionary<string, object?> skuParameters = new Dictionary<string, object?>();
            var skuSql = new StringBuilder(@"SELECT S.id AS Id, S.uid AS UID, S.created_by AS CreatedBy, S.created_time AS CreatedTime, 
                                             S.modified_by AS ModifiedBy, S.modified_time AS ModifiedTime, S.server_add_time AS ServerAddTime, 
                                             S.server_modified_time AS ServerModifiedTime, S.company_uid AS CompanyUID, S.org_uid AS OrgUID, 
                                             S.code AS Code, S.name AS Name, S.arabic_name AS ArabicName, S.alias_name AS AliasName, 
                                             S.long_name AS LongName, S.base_uom AS BaseUOM, S.outer_uom AS OuterUOM, S.from_date AS FromDate, 
                                             S.to_date AS ToDate, S.is_stockable AS IsStockable, S.parent_uid AS ParentUID, S.is_active AS IsActive, 
                                             S.is_third_party As IsThirdParty, S.supplier_org_uid AS SupplierOrgUID
                                             FROM sku S
                                             WHERE is_active = 1");
            if (orgUIDs != null && orgUIDs.Any())
            {
                skuSql.Append($" AND org_uid IN @ORGUIDs");
                skuParameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                skuSql.Append($" AND uid IN @UIDs");
                skuParameters.Add("UIDs", skuUIDs);
            }

            Dictionary<string, object?> skuConfigParameters = new Dictionary<string, object?>();
            var skuConfigSql = new StringBuilder(@"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                             modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                                             server_modified_time AS ServerModifiedTime, org_uid AS OrgUID, distribution_channel_org_uid AS DistributionChannelOrgUID, 
                                             sku_uid AS SKUUID, can_buy AS CanBuy, can_sell AS CanSell, buying_uom AS BuyingUOM, selling_uom AS SellingUOM, 
                                             is_active AS IsActive 
                                             FROM sku_config WHERE is_active = 1");

            if (orgUIDs != null && orgUIDs.Any())
            {
                skuConfigSql.Append($" AND org_uid IN @ORGUIDs");
                skuConfigParameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                skuConfigSql.Append($" AND sku_uid IN @SKUUIDs");
                skuConfigParameters.Add("SKUUIDs", skuUIDs);
            }
            if (DistributionChannelUIDs != null && DistributionChannelUIDs.Any())
            {
                skuConfigSql.Append($" AND distribution_channel_org_uid IN @DistributionChannelUIDs");
                skuConfigParameters.Add("DistributionChannelUIDs", DistributionChannelUIDs);
            }
            Dictionary<string, object?> skuUOMParameters = new Dictionary<string, object?>();
            var skuUomSql = new StringBuilder(@"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                             modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                                             server_modified_time AS ServerModifiedTime, sku_uid AS SKUUID, code AS Code, name AS Name, 
                                             label AS Label, barcodes AS Barcodes, is_base_uom AS IsBaseUOM, is_outer_uom AS IsOuterUOM, 
                                             multiplier AS Multiplier, length AS Length, depth AS Depth, width AS Width, height AS Height, 
                                             volume AS Volume, weight AS Weight, gross_weight AS GrossWeight, dimension_unit AS DimensionUnit, 
                                             volume_unit AS VolumeUnit, weight_unit AS WeightUnit, gross_weight_unit AS GrossWeightUnit, liter AS Liter, kgm AS KGM 
                                             FROM sku_uom WHERE 1=1");

            if (skuUIDs != null && skuUIDs.Any())
            {
                skuUomSql.Append($" AND sku_uid IN @SKUUIDs;");
                skuUOMParameters.Add("SKUUIDs", skuUIDs);
            }

            Dictionary<string, object?> skuAttributesparameters = new Dictionary<string, object?>();
            var skuAttributesSql = new StringBuilder(@"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                                    modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                                                    server_modified_time AS ServerModifiedTime, sku_uid AS SKUUID, type AS Type, code AS Code, 
                                                    value AS Value, parent_type AS ParentType FROM sku_attributes WHERE 1=1 ");

            if (skuUIDs != null && skuUIDs.Any())
            {
                skuAttributesSql.Append($" AND sku_uid IN @SKUUIDs");
                skuAttributesparameters.Add("SKUUIDs", skuUIDs);
            }
            if (attributeTypes != null && attributeTypes.Any())
            {
                skuAttributesSql.Append($" AND type IN @attributeTypes;");
                skuAttributesparameters.Add("attributeTypes", attributeTypes);
            }

            Dictionary<string, object?> taxSkuMapparameters = new Dictionary<string, object?>();
            var taxSkuMapSql = new StringBuilder(@"SELECT DISTINCT TSM.sku_uid AS SKUUID, TSM.tax_uid AS TaxUID 
                                 FROM org O 
                                 INNER JOIN tax_group_taxes TGT ON TGT.tax_group_uid = O.tax_group_uid 
                                 INNER JOIN tax T ON T.uid= TGT.tax_uid AND T.applicable_at = 'Item'
                                 INNER JOIN tax_sku_map TSM ON TSM.tax_uid = TGT.tax_uid 
                                 WHERE 1=1 ");

            if (orgUIDs != null && orgUIDs.Any())
            {
                taxSkuMapSql.Append($" AND O.uid IN @ORGUIDs;");
                taxSkuMapparameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                taxSkuMapSql.Append($" AND TSM.sku_uid IN @SKUUIDs");
                taxSkuMapparameters.Add("SKUUIDs", skuUIDs);
            }
            var skuTask = GetSKUsForMaster(orgUIDs, skuUIDs);
            var skuConfigTask = ExecuteQueryAsync<Model.Interfaces.ISKUConfig>(skuConfigSql.ToString(), skuConfigParameters);
            var skuUomTask = ExecuteQueryAsync<Model.Interfaces.ISKUUOM>(skuUomSql.ToString(), skuUOMParameters);
            var skuAttributesTask = ExecuteQueryAsync<Model.Interfaces.ISKUAttributes>(skuAttributesSql.ToString(), skuAttributesparameters);
            var taxSkuMapTask = ExecuteQueryAsync<Model.Interfaces.ITaxSkuMap>(taxSkuMapSql.ToString(), taxSkuMapparameters);

            await Task.WhenAll(skuTask, skuConfigTask, skuUomTask, skuAttributesTask, taxSkuMapTask);

            return (await skuTask, await skuConfigTask, await skuUomTask, await skuAttributesTask, await taxSkuMapTask);
        }
        catch (Exception ex)
        {
            throw new AggregateException("One or more tasks failed.", ex);
        }
    }

    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUListView>> SelectAllSKUDetailsWebView(List<SortCriteria> sortCriterias, 
        int pageNumber, int pageSize,
      List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder havingfilter = new StringBuilder();
            Dictionary<string, object?> parameters = new Dictionary<string, object?>();
            if (filterCriterias is not null)
            {
                var isActiveFilter = filterCriterias.Find(e => "IsActive".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (isActiveFilter != null)
                {
                    havingfilter.Append($" AND s.Is_Active = {Convert.ToInt32(isActiveFilter.Value)}");
                    filterCriterias.Remove(isActiveFilter);
                }
                var skuCodeNameFilter = filterCriterias.Find(e => "skucodeandname".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (skuCodeNameFilter != null)
                {
                    havingfilter.Append(" AND (s.code like '%' + @skucodeandname+ '%' or  s.long_name like '%' + @skucodeandname+ '%') ");
                    parameters.Add("skucodeandname", skuCodeNameFilter.Value);
                    filterCriterias.Remove(skuCodeNameFilter);
                }

                var divisionFilter = filterCriterias.Find(e => "DivisionUIDs".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (divisionFilter is not null)
                {
                    havingfilter.Append(" AND s.supplier_org_uid IN @DivisonUIDs ");
                    parameters.Add("DivisonUIDs", JsonConvert.DeserializeObject<List<string>>(divisionFilter.Value.ToString()));
                    filterCriterias.Remove(divisionFilter);
                }

                var attributeTypeFilter = filterCriterias.Find(e => "AttributeType".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (attributeTypeFilter is not null)
                {
                    havingfilter.Append(" AND SUM(CASE WHEN sa.[type] IN @AttributeTypes THEN 1 ELSE 0 END) > 0 ");
                    parameters.Add("AttributeTypes", JsonConvert.DeserializeObject<List<string>>(attributeTypeFilter.Value.ToString()));
                    filterCriterias.Remove(attributeTypeFilter);
                }

                var attributeValueFilter = filterCriterias.Find(e => "AttributeValue".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (attributeValueFilter is not null)
                {
                    havingfilter.Append(" AND SUM(CASE WHEN sa.[value] IN @AttributeValue THEN 1 ELSE 0 END) > 0 ");
                    parameters.Add("AttributeValue", JsonConvert.DeserializeObject<List<string>>(attributeValueFilter.Value.ToString()));
                    filterCriterias.Remove(attributeValueFilter);
                }
            }
            var sql = new StringBuilder(
                $"""
                SELECT * FROM(
                SELECT 
                	s.id as id,
                    s.uid AS SKUUID,
                    s.code AS SKUCode, 
                    s.long_name AS SKULongName, 
                    s.is_active AS IsActive,
                    s.modified_time AS SKUModifiedTime,
                    s.supplier_org_uid as DivisionUID,
                    sed.product_category_id AS ProductCategoryId,
                    STRING_AGG(sa.[type], ', ') AS attribute_types,
                    STRING_AGG(sa.value, ', ') AS attribute_values
                FROM 
                    sku s
                LEFT JOIN 
                    sku_attributes sa ON s.uid = sa.sku_uid
                INNER JOIN 
                    sku_ext_data sed on sed.sku_uid=s.uid
                GROUP BY 
                   s.id, s.uid, s.name, s.code, s.long_name, s.is_active, s.modified_time, s.supplier_org_uid,  s.is_active,sed.product_category_id
                HAVING 
                    
                    1=1 {havingfilter.ToString()}
                                ) AS subquery WHERE 1=1
                """);
            StringBuilder sqlCount = null;
            if (isCountRequired)
            {
                sqlCount = new StringBuilder($"""
                                    SELECT COUNT(1) AS Cnt FROM(
                                        SELECT 
                                    	s.id as id,
                                        s.uid AS SKUUID,
                                        s.code AS SKUCode, 
                                        s.long_name AS SKULongName, 
                                        s.is_active AS IsActive,
                                        s.modified_time AS SKUModifiedTime,
                                        s.supplier_org_uid as DivisionUID,
                                        sed.product_category_id AS ProductCategoryId,
                                        STRING_AGG(sa.[type], ', ') AS attribute_types,
                                        STRING_AGG(sa.value, ', ') AS attribute_values
                                    FROM 
                                        sku s
                                    LEFT JOIN 
                                        sku_attributes sa ON s.uid = sa.sku_uid
                                    INNER JOIN 
                                        sku_ext_data sed on sed.sku_uid=s.uid
                                    GROUP BY 
                                       s.id, s.uid, s.name, s.code, s.long_name, s.is_active, s.modified_time, s.supplier_org_uid, s.is_active, sed.product_category_id
                                    HAVING 
                                        
                                        1=1 {havingfilter.ToString()})AS subquery WHERE 1=1 
                                    """);
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

            IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUListView> skuListViewDetails =
                await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISKUListView>(
                    sql.ToString(), parameters);

            int totalCount = -1;
            if (isCountRequired && sqlCount != null)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUListView> pagedResponse =
                new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUListView>
                {
                    PagedData = skuListViewDetails,
                    TotalCount = totalCount
                };
            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<(List<Model.Interfaces.ISKU>, List<Model.Interfaces.ISKUConfig>, List<Model.Interfaces.ISKUUOM>,
List<Model.Interfaces.ISKUAttributes>, List<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields>,
List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>)>
SelectSKUMasterByUID(string UID)
    {
        try
        {
            Dictionary<string, object> Parameters = new Dictionary<string, object>
        {
            { "UID", UID },
        };

            var skuSql = new StringBuilder(@"SELECT 
                id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                company_uid AS CompanyUID,
                org_uid AS OrgUID,
                code AS Code,
                name AS Name,
                arabic_name AS ArabicName,
                alias_name AS AliasName,
                long_name AS LongName,
                base_uom AS BaseUOM,
                outer_uom AS OuterUOM,
                from_date AS FromDate,
                to_date AS ToDate,
                is_stockable AS IsStockable,
                parent_uid AS ParentUID,
                is_active AS IsActive,
                is_third_party AS IsThirdParty,
                supplier_org_uid AS SupplierOrgUID
            FROM 
                sku
            WHERE 
                uid = @UID;");

            var skuConfigSql = new StringBuilder(@"SELECT id AS Id,
                   uid AS UID,
                   created_by AS CreatedBy,
                   created_time AS CreatedTime,
                   modified_by AS ModifiedBy,
                   modified_time AS ModifiedTime,
                   server_add_time AS ServerAddTime,
                   server_modified_time AS ServerModifiedTime,
                   org_uid AS OrgUid,
                   distribution_channel_org_uid AS DistributionChannelOrgUid,
                   sku_uid AS SKUUID,
                   can_buy AS CanBuy,
                   can_sell AS CanSell,
                   buying_uom AS BuyingUom,
                   selling_uom AS SellingUom,
                   is_active AS IsActive
            FROM sku_config
            WHERE is_active = 1 AND sku_uid = @UID;");

            var skuUomSql = new StringBuilder(@"SELECT id AS Id,
                   uid AS UID,
                   created_by AS CreatedBy,
                   created_time AS CreatedTime,
                   modified_by AS ModifiedBy,
                   modified_time AS ModifiedTime,
                   server_add_time AS ServerAddTime,
                   server_modified_time AS ServerModifiedTime,
                   sku_uid AS SkuUid,
                   code AS Code,
                   name AS Name,
                   label AS Label,
                   barcodes AS Barcodes,
                   is_base_uom AS IsBaseUom,
                   is_outer_uom AS IsOuterUom,
                   multiplier AS Multiplier,
                   length AS Length,
                   depth AS Depth,
                   width AS Width,
                   height AS Height,
                   volume AS Volume,
                   weight AS Weight,
                   gross_weight AS GrossWeight,
                   dimension_unit AS DimensionUnit,
                   volume_unit AS VolumeUnit,
                   weight_unit AS WeightUnit,
                   gross_weight_unit AS GrossWeightUnit,
                   liter AS Liter,
                   kgm AS KGM
            FROM sku_uom
            WHERE sku_uid = @UID;");

            var skuAttributesSql = new StringBuilder(@"SELECT id AS Id,
                   uid AS UID,
                   created_by AS CreatedBy,
                   created_time AS CreatedTime,
                   modified_by AS ModifiedBy,
                   modified_time AS ModifiedTime,
                   server_add_time AS ServerAddTime,
                   server_modified_time AS ServerModifiedTime,
                   sku_uid AS SKUUID,
                   type AS Type,
                   code AS Code,
                   value AS Value,
                   parent_type AS ParentType
            FROM sku_attributes
            WHERE sku_uid = @UID;");

            var customSKUFieldsSql = new StringBuilder(@"SELECT id AS Id,
                                                    uid AS UID,
                                                    created_by AS CreatedBy,
                                                    created_time AS CreatedTime,
                                                    modified_by AS ModifiedBy,
                                                    modified_time AS ModifiedTime,
                                                    server_add_time AS ServerAddTime,
                                                    server_modified_time AS ServerModifiedTime,
                                                    sku_uid AS SKUUID,
                                                    custom_field AS CustomField 
                                            FROM custom_sku_fields 
                                            WHERE sku_uid = @UID;");

            var fileSysSql = new StringBuilder(@"SELECT 
                    id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    linked_item_uid AS LinkedItemUid,
                    linked_item_type AS LinkedItemType,
                    file_sys_type AS FileSysType,
                    file_type AS FileType,
                    parent_file_sys_uid AS ParentFileSysUid,
                    is_directory AS IsDirectory,
                    file_name AS FileName,
                    display_name AS DisplayName,
                    file_size AS FileSize,
                    relative_path AS RelativePath,
                    latitude AS Latitude,
                    longitude AS Longitude,
                    created_by_job_position_uid AS CreatedByJobPositionUid,
                    created_by_emp_uid AS CreatedByEmpUid,
                    is_default AS IsDefault
            FROM file_sys
            WHERE linked_item_uid = @UID;");

            var skuTask = ExecuteQueryAsync<Model.Interfaces.ISKU>(skuSql.ToString(), Parameters);
            var skuConfigTask = ExecuteQueryAsync<Model.Interfaces.ISKUConfig>(skuConfigSql.ToString(), Parameters);
            var skuUomTask = ExecuteQueryAsync<Model.Interfaces.ISKUUOM>(skuUomSql.ToString(), Parameters);
            var skuAttributesTask = ExecuteQueryAsync<Model.Interfaces.ISKUAttributes>(skuAttributesSql.ToString(), Parameters);
            var customSKUFieldsTask = ExecuteQueryAsync<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields>(customSKUFieldsSql.ToString(), Parameters);
            var fileSysTask = ExecuteQueryAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(fileSysSql.ToString(), Parameters);

            await Task.WhenAll(skuTask, skuConfigTask, skuUomTask, skuAttributesTask, customSKUFieldsTask, fileSysTask);

            return (await skuTask, await skuConfigTask, await skuUomTask, await skuAttributesTask, await customSKUFieldsTask, await fileSysTask);
        }
        catch (Exception)
        {
            throw;
        }
    }


    public Task<int> CRUDWinitCache(string key, string value, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<ISKUMaster>> GetWinitCache(string key, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }
    public async virtual Task<List<ISKU>> GetSKUsForMaster(List<string> orgUIDs, List<string> skuUIDs)
    {
        try
        {
            var skuSql = new StringBuilder(@"SELECT S.id AS Id, S.uid AS UID, S.created_by AS CreatedBy, S.created_time AS CreatedTime, S.modified_by AS ModifiedBy,
                                                S.modified_time AS ModifiedTime, S.server_add_time AS ServerAddTime, S.server_modified_time AS ServerModifiedTime,
                                                S.company_uid AS CompanyUID, S.org_uid AS OrgUID, S.code AS Code, S.name AS Name, S.arabic_name AS ArabicName,
                                                S.alias_name AS AliasName, S.long_name AS LongName, S.base_uom AS BaseUOM, S.outer_uom AS OuterUOM, S.from_date AS FromDate,
                                                S.to_date AS ToDate, S.is_stockable AS IsStockable, S.parent_uid AS ParentUID, S.is_active AS IsActive, 
                                                S.is_third_party As IsThirdParty, S.supplier_org_uid AS SupplierOrgUID,
                                                FSC.relative_path + '/' + FSC.file_name AS CatalogueURL
                                                FROM sku S
                                                LEFT JOIN file_sys FS ON FS.linked_item_type = 'SKU' AND FS.linked_item_uid = S.uid
                                                AND FS.file_sys_type = 'Image' AND FS.is_default = 1
                                                LEFT JOIN file_sys FSC ON FSC.linked_item_type = 'SKU' AND FSC.linked_item_uid = S.uid
                                                AND FSC.file_sys_type = 'Catalogue'
                                                WHERE is_active = 1");
            Dictionary<string, object?> parameters = new Dictionary<string, object?>();
            if (orgUIDs != null && orgUIDs.Any())
            {
                skuSql.Append($" AND org_uid = Any(@ORGUIDs)");
                parameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                skuSql.Append($" AND uid = @SKUUIDs");
                parameters.Add("SKUUIDs", skuUIDs);
            }
            return await ExecuteQueryAsync<Model.Interfaces.ISKU>(skuSql.ToString(), parameters);

        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs)
    {
        Dictionary<string, List<string>> storeLinkedItems = new Dictionary<string, List<string>>();

        string storeQuery = string.Empty;
        if (storeUIDs != null && storeUIDs.Count > 0)
        {
            storeQuery = "WHERE store_uid IN @StoreUID";
        }

        string query = $@"
            ;WITH Store AS (
	            SELECT store_uid AS StoreUID, broad_classification AS BroadClassificationUID,  branch_uid AS BranchUID 
                FROM VW_StoreImpAttributes
	            {storeQuery}
            )
            SELECT s.StoreUID, smc.linked_item_uid AS LinkedItemUID
            FROM selection_map_criteria smc 
            JOIN Store s ON 1=1  -- Ensures the Store CTE is available
            WHERE is_active = 1 AND linked_item_type = @LinkedItemType AND
                -- Check if has_customer is true and valid customers exist
                (
                    smc.has_customer = 0 
                    OR (smc.customer_count > 0 AND EXISTS (
                        SELECT 1 FROM selection_map_details smd
                        WHERE smd.selection_map_criteria_uid = smc.uid
                        AND smd.selection_group = 'Customer'
                        AND smd.type_uid = 'Store'
                        AND smd.selection_value = s.StoreUID
                    ))
                )
                -- Check if has_sales_team is true and valid sales teams exist
                AND (
                    smc.has_sales_team = 0 
                    OR (smc.sales_team_count > 0 AND EXISTS (
                        SELECT 1 FROM selection_map_details smd
                        WHERE smd.selection_map_criteria_uid = smc.uid
                        AND smd.selection_group = 'SalesTeam'
                        AND smd.type_uid = 'BroadClassification'
                        AND smd.selection_value = s.BroadClassificationUID
                    ))
                )
                -- Check if has_location is true and valid locations exist
                AND (
                    smc.has_location = 0 
                    OR (smc.location_count > 0 AND EXISTS (
                        SELECT 1 FROM selection_map_details smd
                        WHERE smd.selection_map_criteria_uid = smc.uid
                        AND smd.selection_group = 'Location'
                        AND smd.type_uid = 'Branch'
                        AND smd.selection_value = s.BranchUID
                    ))
                )";

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
             {"LinkedItemType",  linkedItemType}
        };

        var result = await ExecuteQueryAsync<(string StoreUID, string LinkedItemUID)>(query, parameters);

        foreach (var row in result)
        {
            if (!storeLinkedItems.ContainsKey(row.StoreUID))
            {
                storeLinkedItems[row.StoreUID] = new List<string>();
            }
            storeLinkedItems[row.StoreUID].Add(row.LinkedItemUID);
        }
        return storeLinkedItems;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes;

public class PGSQLSKUDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISKUDL
{
    Winit.Shared.CommonUtilities.Common.CommonFunctions commonFunctions = new();
    public PGSQLSKUDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU>> SelectAllSKUDetails(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {

            var sql = new StringBuilder(@"SELECT 
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
                    supplier_org_uid AS SupplierOrgUID,
                    sku_image AS SKUImage,
                    catalogue_url AS CatalogueURL
                FROM 
                    sku");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM sku");
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

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKU>().GetType();
            IEnumerable<Model.Interfaces.ISKU> skuList = await ExecuteQueryAsync<Model.Interfaces.ISKU>(sql.ToString(), parameters, type);
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
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKU>().GetType();
        Winit.Modules.SKU.Model.Interfaces.ISKU skuDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKU>(sql, parameters, type);
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
                VALUES(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,
                @CompanyUID,@OrgUID,@Code,@Name,@ArabicName,@AliasName,@LongName,@BaseUOM,@OuterUOM,@FromDate,
                @ToDate,@IsStockable,@ParentUID,@IsActive,@IsThirdParty,@SupplierOrgUID);";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",sku.UID},
                {"CreatedBy",sku.CreatedBy},
                {"CreatedTime",sku.CreatedTime},
                {"ModifiedBy",sku.ModifiedBy},
                {"ModifiedTime",sku.ModifiedTime},
                {"ServerAddTime",sku.ServerAddTime},
                {"ServerModifiedTime",sku.ServerModifiedTime},
                {"CompanyUID",sku.CompanyUID},
                {"OrgUID",sku.OrgUID},
                {"Code",sku.Code},
                {"Name",sku.Name},
                {"ArabicName",sku.ArabicName},
                {"AliasName",sku.AliasName},
                {"LongName",sku.LongName},
                {"BaseUOM",sku.BaseUOM},
                {"OuterUOM",sku.OuterUOM},
                {"FromDate",sku.FromDate},
                {"ToDate",sku.ToDate},
                {"IsStockable",sku.IsStockable},
                {"ParentUID",sku.ParentUID},
                {"IsActive",sku.IsActive},
                {"IsThirdParty",sku.IsThirdParty},
                {"SupplierOrgUID",sku.SupplierOrgUID},           };
            retVal = await ExecuteNonQueryAsync(sql, parameters);
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
    private async Task<int> CreateSKUExtData(Winit.Modules.SKU.Model.Interfaces.ISKUV1 sku)
    {
        int retVal = -1;
        try
        {
            var sql = """
                                INSERT INTO public.sku_ext_data ( uid, created_by, created_time, modified_by, modified_time,
                                server_add_time, server_modified_time, ss, sku_uid,hsn_code) 
                       VALUES ( @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @SkuUid,@HSNCode);
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
                {"HSNCode",sku.HSNCode},
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
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
               {"UID",sku.UID},
               {"ModifiedBy",sku.ModifiedBy},
               {"ModifiedTime",sku.ModifiedTime},
               {"ServerModifiedTime",sku.ServerModifiedTime},
               {"CompanyUID",sku.CompanyUID},
               {"OrgUID",sku.OrgUID},
               {"Code",sku.Code},
               {"Name",sku.Name},
               {"ArabicName",sku.ArabicName},
               {"AliasName",sku.AliasName},
               {"LongName",sku.LongName},
               {"BaseUOM",sku.BaseUOM},
               {"OuterUOM",sku.OuterUOM},
               {"FromDate",sku.FromDate},
               {"ToDate",sku.ToDate},
               {"IsStockable",sku.IsStockable},
               {"ParentUID",sku.ParentUID},
               {"IsActive",sku.IsActive},
               {"IsThirdParty",sku.IsThirdParty},
               {"SupplierOrgUID",sku.SupplierOrgUID}
            };
            return await ExecuteNonQueryAsync(sql, parameters);
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
        catch (PostgresException ex)
        {
            if (ex.SqlState == "23503")
            {
                throw new("This sku already used by other processes, so you can't delete this.");
            }
            throw;
        }
        catch (Exception)
        {
            throw new("An error occurred while deleting.");
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
                                             WHERE is_active = true");
            if (orgUIDs != null && orgUIDs.Any())
            {
                skuSql.Append($" AND org_uid = ANY(@ORGUIDs)");
                skuParameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                skuSql.Append($" AND uid = ANY(@UIDs)");
                skuParameters.Add("UIDs", skuUIDs);
            }

            Dictionary<string, object?> skuConfigParameters = new Dictionary<string, object?>();
            var skuConfigSql = new StringBuilder(@"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                             modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                                             server_modified_time AS ServerModifiedTime, org_uid AS OrgUID, distribution_channel_org_uid AS DistributionChannelOrgUID, 
                                             sku_uid AS SKUUID, can_buy AS CanBuy, can_sell AS CanSell, buying_uom AS BuyingUOM, selling_uom AS SellingUOM, 
                                             is_active AS IsActive 
                                             FROM sku_config WHERE is_active = true");

            if (orgUIDs != null && orgUIDs.Any())
            {
                skuConfigSql.Append($" AND org_uid = ANY(@ORGUIDs)");
                skuConfigParameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                skuConfigSql.Append($" AND sku_uid = ANY(@SKUUIDs)");
                skuConfigParameters.Add("SKUUIDs", skuUIDs);
            }
            if (DistributionChannelUIDs != null && DistributionChannelUIDs.Any())
            {
                skuConfigSql.Append($" AND distribution_channel_org_uid = ANY(@DistributionChannelUIDs)");
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
                skuUomSql.Append($" AND sku_uid = ANY(@SKUUIDs)");
                skuUOMParameters.Add("SKUUIDs", skuUIDs);
            }

            Dictionary<string, object?> skuAttributesparameters = new Dictionary<string, object?>();
            var skuAttributesSql = new StringBuilder(@"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                                                    modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
                                                    server_modified_time AS ServerModifiedTime, sku_uid AS SKUUID, type AS Type, code AS Code, 
                                                    value AS Value, parent_type AS ParentType FROM sku_attributes WHERE 1=1 ");

            if (skuUIDs != null && skuUIDs.Any())
            {
                skuAttributesSql.Append($" AND sku_uid = ANY(@SKUUIDs)");
                skuAttributesparameters.Add("SKUUIDs", skuUIDs);
            }
            if (attributeTypes != null && attributeTypes.Any())
            {
                skuAttributesSql.Append($" AND type = ANY(@attributeTypes)");
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
                taxSkuMapSql.Append($" AND O.uid = ANY(@ORGUIDs)");
                taxSkuMapparameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                taxSkuMapSql.Append($" AND TSM.sku_uid = ANY(@SKUUIDs)");
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
                    havingfilter.Append($" AND s.Is_Active = {CommonFunctions.GetBooleanValue(isActiveFilter.Value)}");
                    filterCriterias.Remove(isActiveFilter);
                }
                var skuCodeNameFilter = filterCriterias.Find(e => "skucodeandname".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (skuCodeNameFilter != null)
                {
                    havingfilter.Append(" AND (s.code like '%' || @skucodeandname || '%' or  s.long_name like '%' || @skucodeandname || '%') ");
                    parameters.Add("skucodeandname", skuCodeNameFilter.Value);
                    filterCriterias.Remove(skuCodeNameFilter);
                }

                var divisionFilter = filterCriterias.Find(e => "DivisionUIDs".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (divisionFilter is not null)
                {
                    havingfilter.Append(" AND s.supplier_org_uid =  ANY(@DivisonUIDs) ");
                    parameters.Add("DivisonUIDs", JsonConvert.DeserializeObject<List<string>>(divisionFilter.Value.ToString()));
                    filterCriterias.Remove(divisionFilter);
                }

                var attributeTypeFilter = filterCriterias.Find(e => "AttributeType".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (attributeTypeFilter is not null)
                {
                    havingfilter.Append(" AND SUM(CASE WHEN sa.type = ANY(@AttributeTypes) THEN 1 ELSE 0 END) > 0 ");
                    parameters.Add("AttributeTypes", JsonConvert.DeserializeObject<List<string>>(attributeTypeFilter.Value.ToString()));
                    filterCriterias.Remove(attributeTypeFilter);
                }

                var attributeValueFilter = filterCriterias.Find(e => "AttributeValue".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (attributeValueFilter is not null)
                {
                    havingfilter.Append(" AND SUM(CASE WHEN sa.code = ANY(@AttributeValue) THEN 1 ELSE 0 END) > 0 ");
                    parameters.Add("AttributeValue", JsonConvert.DeserializeObject<List<string>>(attributeValueFilter.Value.ToString()));
                    filterCriterias.Remove(attributeValueFilter);
                }
                
                // Add ParentUID filter support
                var parentUIDFilter = filterCriterias.Find(e => "ParentUID".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (parentUIDFilter is not null)
                {
                    havingfilter.Append(" AND s.parent_uid = @ParentUID ");
                    parameters.Add("ParentUID", parentUIDFilter.Value.ToString());
                    filterCriterias.Remove(parentUIDFilter);
                }
                
                // Add support for multiple ParentUIDs (for filtering by multiple brands)
                var parentUIDsFilter = filterCriterias.Find(e => "ParentUIDs".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
                if (parentUIDsFilter is not null)
                {
                    havingfilter.Append(" AND s.parent_uid = ANY(@ParentUIDs) ");
                    parameters.Add("ParentUIDs", JsonConvert.DeserializeObject<List<string>>(parentUIDsFilter.Value.ToString()));
                    filterCriterias.Remove(parentUIDsFilter);
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
                        s.parent_uid AS ParentUID,
                        sed.product_category_id AS ProductCategoryId,
                        STRING_AGG(sa.type, ', ') AS attribute_types,
                        STRING_AGG(sa.value, ', ') AS attribute_values
                    FROM 
                        sku s
                    LEFT JOIN 
                        sku_attributes sa ON s.uid = sa.sku_uid
                    LEFT JOIN 
                        sku_ext_data sed on sed.sku_uid = s.uid
                    GROUP BY 
                        s.id, s.uid, s.code, s.long_name, s.is_active, s.modified_time, s.supplier_org_uid, s.parent_uid, sed.product_category_id
                    HAVING 
                        1=1 {havingfilter.ToString()}
                ) AS subquery 
                WHERE 1=1
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
                                            s.parent_uid AS ParentUID,
                                            sed.product_category_id AS ProductCategoryId,
                                            STRING_AGG(sa.type, ', ') AS attribute_types,
                                            STRING_AGG(sa.value, ', ') AS attribute_values
                                        FROM 
                                            sku s
                                        LEFT JOIN 
                                            sku_attributes sa ON s.uid = sa.sku_uid
                                        LEFT JOIN 
                                            sku_ext_data sed on sed.sku_uid = s.uid
                                        GROUP BY 
                                            s.id, s.uid, s.code, s.long_name, s.is_active, s.modified_time, s.supplier_org_uid, s.parent_uid, sed.product_category_id
                                        HAVING 
                                            1=1 {havingfilter.ToString()}
                                    ) AS subquery 
                                    WHERE 1=1
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
                   --is_active = true 
                     uid = @UID;");

            Type skuType = _serviceProvider.GetRequiredService<Model.Interfaces.ISKU>().GetType();
            List<Model.Interfaces.ISKU> skuList = await ExecuteQueryAsync<Model.Interfaces.ISKU>(skuSql.ToString(), Parameters, skuType);


            var skuConfig = new StringBuilder(@"SELECT id AS Id,
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
                WHERE is_active = true AND sku_uid = @UID;");

            Type skuConfigType = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUConfig>().GetType();
            List<Model.Interfaces.ISKUConfig> skuConfigList = await ExecuteQueryAsync<Model.Interfaces.ISKUConfig>(skuConfig.ToString(), Parameters, skuConfigType);

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

            Type skuUomtype = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUUOM>().GetType();
            List<Model.Interfaces.ISKUUOM> skuUomList = await ExecuteQueryAsync<Model.Interfaces.ISKUUOM>(skuUomSql.ToString(), Parameters, skuUomtype);


            var skuAtrributesSql = new StringBuilder(@"SELECT id AS Id,
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


            Type skuAttributestype = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUAttributes>().GetType();


            List<Model.Interfaces.ISKUAttributes> sKUAttributesList = await ExecuteQueryAsync<Model.Interfaces.ISKUAttributes>(skuAtrributesSql.ToString(), Parameters, skuAttributestype);


            var customSKUfielsSql = new StringBuilder(@"SELECT id AS Id,
                                                        uid AS UID,
                                                        created_by AS CreatedBy,
                                                        created_time AS CreatedTime,
                                                        modified_by AS ModifiedBy,
                                                        modified_time AS ModifiedTime,
                                                        server_add_time AS ServerAddTime,
                                                        server_modified_time AS ServerModifiedTime,
                                                        sku_uid AS SKUUID,custom_field AS CustomField 
                                                FROM custom_sku_fields 
                                                WHERE sku_uid = @UID;");

            Type taxSkuMaptype = _serviceProvider.GetRequiredService<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields>().GetType();
            List<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields> CustomSKUFieldsList = await ExecuteQueryAsync<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields>(customSKUfielsSql.ToString(), Parameters, taxSkuMaptype);

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
                        --temp_path AS TempPath,
                        latitude AS Latitude,
                        longitude AS Longitude,
                        created_by_job_position_uid AS CreatedByJobPositionUid,
                        created_by_emp_uid AS CreatedByEmpUid,
                        is_default AS IsDefault
                FROM file_sys
                WHERE linked_item_uid = @UID;");

            Type fileSystype = _serviceProvider.GetRequiredService<Winit.Modules.FileSys.Model.Interfaces.IFileSys>().GetType();
            List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSysList = await ExecuteQueryAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(fileSysSql.ToString(), Parameters, fileSystype);

            return (skuList, skuConfigList, skuUomList, sKUAttributesList, CustomSKUFieldsList, fileSysList);
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
            var skuSql = new StringBuilder("""
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
                                               S.is_third_party AS IsThirdParty, 
                                               S.supplier_org_uid AS SupplierOrgUID,
                                               FS.relative_path || '/' || FS.file_name AS SKUImage,
                                               FSC.relative_path || '/' || FSC.file_name AS CatalogueURL,
                                               '[' || O.code || '] ' || O.name AS L1, 
                                               '[' || DC.code || '] ' || DC.name AS L2,
                                               SAC.value AS L3,
                                               SASC.value AS L4,
                                               SAB.value AS L5,
                                               SASB.value AS L6,
                                               sed.model_code AS modelcode,
                                               sed.year AS year,
                                               sed.type AS type,
                                               sed.product_type AS product_type,
                                               sed.category AS category,
                                               sed.tonnage AS tonnage,
                                               sed.capacity AS capacity,
                                               sed.star_rating AS starrating,
                                               sed.product_category_id AS productcategoryid,
                                               sed.product_category_name AS productcategoryname,
                                               sed.item_series AS itemseries,
                                               sed.hsn_code AS hsncode,
                                               sed.is_available_in_ap_master AS IsAvailableInApMaster
                                           FROM sku S
                                           LEFT JOIN file_sys FS 
                                               ON FS.linked_item_type = 'SKU' 
                                               AND FS.linked_item_uid = S.uid  
                                               AND FS.file_sys_type = 'Image' 
                                               AND FS.is_default = true
                                           LEFT JOIN file_sys FSC 
                                               ON FSC.linked_item_type = 'SKU' 
                                               AND FSC.linked_item_uid = S.uid
                                               AND FSC.file_sys_type = 'Catalogue'
                                           LEFT JOIN org DC 
                                               ON DC.uid = S.supplier_org_uid	
                                           LEFT JOIN org O 
                                               ON O.uid = DC.parent_uid
                                           LEFT JOIN sku_attributes SAC 
                                               ON SAC.sku_uid = S.uid AND SAC.type = 'Category'
                                           LEFT JOIN sku_attributes SASC 
                                               ON SASC.sku_uid = S.uid AND SASC.type = 'Product Type'
                                           LEFT JOIN sku_attributes SAB 
                                               ON SAB.sku_uid = S.uid AND SAB.type = 'Star Rating'
                                           LEFT JOIN sku_attributes SASB 
                                               ON SASB.sku_uid = S.uid AND SASB.type = 'Item Series'
                                           LEFT JOIN sku_ext_data sed 
                                               ON S.uid = sed.sku_uid 
                                           WHERE S.is_active = true
                                           

                                           """);
            Dictionary<string, object?> parameters = new Dictionary<string, object?>();
            if (orgUIDs != null && orgUIDs.Any())
            {
                skuSql.Append($" AND s.org_uid = Any(@ORGUIDs)");
                parameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                skuSql.Append($" AND S.uid = ANY(@SKUUIDs)");
                parameters.Add("SKUUIDs", skuUIDs);
            }
            return await ExecuteQueryAsync<Model.Interfaces.ISKU>(skuSql.ToString(), parameters);

        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs)
    {
        throw new NotImplementedException();
    }
}

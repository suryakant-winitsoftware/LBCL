using Microsoft.Extensions.Configuration;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.DL.Classes;

public class PGSQLSKUV1DL : PGSQLSKUDL
{
    public PGSQLSKUV1DL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async  Task<List<ISKU>> GetSKUsForMaster(List<string> orgUIDs)
    {
        try
        {
            var skuSql = """
                SELECT S.id AS Id, S.uid AS UID, S.created_by AS CreatedBy, S.created_time AS CreatedTime, 
                S.modified_by AS ModifiedBy,S.modified_time AS ModifiedTime, S.server_add_time AS ServerAddTime, 
                S.server_modified_time AS ServerModifiedTime,S.company_uid AS CompanyUID, S.org_uid AS OrgUID, 
                S.code AS Code, S.name AS Name, S.arabic_name AS ArabicName,S.alias_name AS AliasName, 
                S.long_name AS LongName, S.base_uom AS BaseUOM, S.outer_uom AS OuterUOM, S.from_date AS FromDate,
                S.to_date AS ToDate, S.is_stockable AS IsStockable, S.parent_uid AS ParentUID, S.is_active AS IsActive, 
                S.is_third_party As IsThirdParty, S.supplier_org_uid AS SupplierOrgUID,
                FSC.relative_path || '/' || FSC.file_name AS CatalogueURL,
                '[' || O.code || '] ' || O.name AS L1, 
                '[' || DC.code || '] ' || DC.name AS L2,
                '[' || SAC.code || '] ' || SAC.value AS L3,
                '[' || SASC.code || '] ' || SASC.value AS L3,
                '[' || SAB.code || '] ' || SAB.value AS L3,
                '[' || SASB.code || '] ' || SASB.value AS L3
                FROM sku S
                LEFT JOIN file_sys FS ON FS.linked_item_type = 'SKU' AND FS.linked_item_uid = S.uid  
                AND FS.file_sys_type = 'Image' AND FS.is_default = true
                LEFT JOIN file_sys FSC ON FSC.linked_item_type = 'SKU' AND FSC.linked_item_uid = S.uid
                AND FSC.file_sys_type = 'Catalogue'
                LEFT JOIN org O ON O.uid = s.org_uid
                LEFT JOIN org DC ON DC.uid = s.supplier_org_uid	
                LEFT JOIN sku_attributes SAC ON SAC.sku_uid = S.uid AND SAC.type = 'Category'
                LEFT JOIN sku_attributes SASC ON SASC.sku_uid = S.uid AND SASC.type = 'Sub Category'
                LEFT JOIN sku_attributes SAB ON SAB.sku_uid = S.uid AND SAB.type = 'Brand'
                LEFT JOIN sku_attributes SASB ON SASB.sku_uid = S.uid AND SASB.type = 'Sub Brand'
                WHERE S.is_active = true
                """;
            Dictionary<string, object?> parameters = new Dictionary<string, object?>();
            if (orgUIDs != null && orgUIDs.Any())
            {
                skuSql += $" AND s.org_uid = Any(@ORGUIDs)";
                parameters.Add("ORGUIDs", orgUIDs);
            }
            return await ExecuteQueryAsync<Model.Interfaces.ISKU>(skuSql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
}

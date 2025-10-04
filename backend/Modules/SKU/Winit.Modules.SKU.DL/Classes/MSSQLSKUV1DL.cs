using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using Winit.Modules.SKU.Model.Interfaces;
using Exception=System.Exception;

namespace Winit.Modules.SKU.DL.Classes;

public class MSSQLSKUV1DL : MSSQLSKUDL
{
    public MSSQLSKUV1DL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async override Task<List<ISKU>> GetSKUsForMaster(List<string> orgUIDs, List<string> skuUIDs)
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
                                               S.is_third_party As IsThirdParty, 
                                               S.supplier_org_uid AS SupplierOrgUID,
                                               FS.relative_path + '/' + FS.file_name AS SKUImage,
                                               FSC.relative_path + '/' + FSC.file_name AS CatalogueURL,
                                               '[' + O.code + '] ' + O.name AS L1, 
                                               '[' + DC.code + '] ' + DC.name AS L2,
                                                SAC.value AS L3,
                                               SASC.value AS L4,
                                               SAB.value AS L5,
                                               SASB.value AS L6,
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
                                               sed.is_available_in_ap_master as IsAvailableInApMaster
                                           FROM sku S
                                           LEFT JOIN file_sys FS ON FS.linked_item_type = 'SKU' AND FS.linked_item_uid = S.uid  
                                               AND FS.file_sys_type = 'Image' AND FS.is_default = 1
                                           LEFT JOIN file_sys FSC ON FSC.linked_item_type = 'SKU' AND FSC.linked_item_uid = S.uid
                                               AND FSC.file_sys_type = 'Catalogue'
                                           LEFT JOIN org DC ON DC.uid = S.supplier_org_uid	
                                           LEFT JOIN org O ON O.uid = DC.parent_uid
                                           LEFT JOIN sku_attributes SAC ON SAC.sku_uid = S.uid AND SAC.type = 'Category'
                                           LEFT JOIN sku_attributes SASC ON SASC.sku_uid = S.uid AND SASC.type = 'Product Type'
                                           LEFT JOIN sku_attributes SAB ON SAB.sku_uid = S.uid AND SAB.type = 'Star Rating'
                                           LEFT JOIN sku_attributes SASB ON SASB.sku_uid = S.uid AND SASB.type = 'Item Series'
                                           LEFT JOIN sku_ext_data sed ON S.uid = sed.sku_uid 
                                           WHERE S.is_active = 1

                                           """);
            Dictionary<string, object?> parameters = new Dictionary<string, object?>();
            if (orgUIDs != null && orgUIDs.Any())
            {
                skuSql.Append($" AND s.org_uid = Any(@ORGUIDs)");
                parameters.Add("ORGUIDs", orgUIDs);
            }
            if (skuUIDs != null && skuUIDs.Any())
            {
                skuSql.Append($" AND S.uid IN @SKUUIDs");
                parameters.Add("SKUUIDs", skuUIDs);
            }
            return await ExecuteQueryAsync<Model.Interfaces.ISKU>(skuSql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }    
}

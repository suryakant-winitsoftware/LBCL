using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class PGSQLSKUUOMDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISKUUOMDL
    {
        private readonly Winit.Modules.SKU.DL.Interfaces.ISKUPriceDL _sKUPriceDL;
        public PGSQLSKUUOMDL(IServiceProvider serviceProvider, IConfiguration config, ISKUPriceDL sKUPriceDL) : base(serviceProvider, config)
        {
            _sKUPriceDL = sKUPriceDL;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUUOM>> SelectAllSKUUOMDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                    id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    sku_uid AS SKUUID,
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
                FROM sku_uom");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM sku_uom");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUUOM>(filterCriterias, sbFilterCriteria, parameters);

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
                else if (pageNumber > 0 || pageSize > 0)
                {
                    // PostgreSQL requires ORDER BY when using OFFSET/LIMIT
                    sql.Append(" ORDER BY id");
                }

                if (pageSize > 0)
                {
                    // pageNumber is 0-based from API
                    int offset = pageNumber * pageSize;
                    sql.Append($" OFFSET {offset} LIMIT {pageSize}");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUUOM>().GetType();
                IEnumerable<Model.Interfaces.ISKUUOM> skuUOMList = await ExecuteQueryAsync<Model.Interfaces.ISKUUOM>(sql.ToString(), parameters, type);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUUOM>
                {
                    PagedData = skuUOMList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SelectSKUUOMByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"SELECT id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    sku_uid AS SKUUID,
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
                    kgm AS KGM FROM sku_uom WHERE uid= @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUUOM>().GetType();
            Winit.Modules.SKU.Model.Interfaces.ISKUUOM skuUOMDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUUOM>(sql, parameters, type);
            return skuUOMDetails;
        }
        public async Task<int> CreateSKUUOM(Winit.Modules.SKU.Model.Interfaces.ISKUUOM skuUOM)
        {
            int retVal = -1;
            try
            {
                var sql = @"INSERT INTO sku_uom (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                            sku_uid, code, name, label, barcodes, is_base_uom, is_outer_uom, multiplier, length, depth, width, height, volume, 
                            weight, gross_weight, dimension_unit, volume_unit, weight_unit, gross_weight_unit, liter, kgm)
                            Values (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@SKUUID,@Code,@Name
                            ,@Label,@Barcodes,@IsBaseUOM,@IsOuterUOM,@Multiplier,@Length,@Depth,@Width,@Height,@Volume,@Weight,@GrossWeight
                            ,@DimensionUnit,@VolumeUnit,@WeightUnit,@GrossWeightUnit,@Liter,@KGM);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",skuUOM.UID},
                    {"CreatedBy",skuUOM.CreatedBy},
                    {"CreatedTime",skuUOM.CreatedTime},
                    {"ModifiedBy",skuUOM.ModifiedBy},
                    {"ModifiedTime",skuUOM.ModifiedTime},
                    {"ServerAddTime",skuUOM.ServerAddTime},
                   {"ServerModifiedTime",skuUOM.ServerModifiedTime},
                   {"SKUUID",skuUOM.SKUUID},
                   {"Code",skuUOM.Code},
                   {"Name",skuUOM.Name},
                    {"Label",skuUOM.Label},
                    {"Barcodes",skuUOM.Barcodes},
                    {"IsBaseUOM",skuUOM.IsBaseUOM},
                    {"IsOuterUOM",skuUOM.IsOuterUOM},
                   {"Multiplier",skuUOM.Multiplier},
                   {"Length",skuUOM.Length},
                   {"Depth",skuUOM.Depth},
                   {"Width",skuUOM.Width},
                   {"Height",skuUOM.Height},
                   {"Volume",skuUOM.Volume},
                    {"Weight",skuUOM.Weight},
                    {"GrossWeight",skuUOM.GrossWeight},
                    {"DimensionUnit",skuUOM.DimensionUnit},
                    {"VolumeUnit",skuUOM.VolumeUnit},
                    {"WeightUnit",skuUOM.WeightUnit},
                   {"GrossWeightUnit",skuUOM.GrossWeightUnit},
                   {"Liter",skuUOM.Liter},
                   {"KGM",skuUOM.KGM},
                };
                retVal = await ExecuteNonQueryAsync(sql, parameters);
                if (retVal == 1 && (skuUOM.IsBaseUOM == true || skuUOM.IsOuterUOM == true))
                {
                    // Call update sku
                  await  UpdateSKUForBaseAndOuterUOMs(skuUOM);
                    if (skuUOM.IsBaseUOM == true)
                    {
                       await _sKUPriceDL.CreateStandardPriceForSKU(skuUOM.SKUUID);
                    }
                }
                    
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUUOM(Winit.Modules.SKU.Model.Interfaces.ISKUUOM skuUOM)
        {
            var baseUOMExists = await BaseUOMExists(skuUOM.SKUUID, skuUOM.Code, skuUOM.IsBaseUOM);

            var outerUOMExists = await OuterUOMExists(skuUOM.SKUUID, skuUOM.Code, skuUOM.IsOuterUOM);
            if (!baseUOMExists)
            {
                if (baseUOMExists == true)
                {
                    throw new Exception("Base UOM already exists for other UOM.");
                }

                if (outerUOMExists == true)
                {
                    throw new Exception("Outer UOM already exists for other UOM.");
                }
            }



            //else{
            //        if (!baseUOMExists)
            //        {
            //            throw new Exception("Base UOM already exists for other UOM.");
            //        }

            //        if (outerUOMExists)
            //        {
            //            throw new Exception("Outer UOM already exists for other UOM.");
            //        }
            //    }
            try
            {
                var sql = @"UPDATE sku_uom SET 
                    modified_by = @ModifiedBy, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime, 
                    sku_uid = @SKUUID, 
                    code = @Code, 
                    name = @Name, 
                    label = @Label, 
                    barcodes = @Barcodes, 
                    is_base_uom = @IsBaseUOM, 
                    is_outer_uom = @IsOuterUOM, 
                    multiplier = @Multiplier, 
                    length = @Length, 
                    depth = @Depth, 
                    width = @Width, 
                    height = @Height, 
                    volume = @Volume, 
                    weight = @Weight, 
                    gross_weight = @GrossWeight, 
                    dimension_unit = @DimensionUnit, 
                    volume_unit = @VolumeUnit, 
                    weight_unit = @WeightUnit, 
                    gross_weight_unit = @GrossWeightUnit, 
                    liter = @Liter, 
                    kgm = @KGM
                WHERE 
                    uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",skuUOM.UID},
                   {"ModifiedBy",skuUOM.ModifiedBy},
                   {"ModifiedTime",skuUOM.ModifiedTime},
                   {"ServerModifiedTime",skuUOM.ServerModifiedTime},
                   {"SKUUID",skuUOM.SKUUID},
                   {"Code",skuUOM.Code},
                   {"Name",skuUOM.Name},
                   {"Label",skuUOM.Label},
                   {"Barcodes",skuUOM.Barcodes},
                   {"IsBaseUOM",skuUOM.IsBaseUOM},
                   {"IsOuterUOM",skuUOM.IsOuterUOM},
                   {"Multiplier",skuUOM.Multiplier},
                   {"Length",skuUOM.Length},
                   {"Depth",skuUOM.Depth},
                   {"Width",skuUOM.Width},
                   {"Height",skuUOM.Height},
                   {"Volume",skuUOM.Volume},
                   {"Weight",skuUOM.Weight},
                   {"DimensionUnit",skuUOM.DimensionUnit},
                   {"GrossWeight",skuUOM.GrossWeight},
                   {"VolumeUnit",skuUOM.VolumeUnit},
                   {"WeightUnit",skuUOM.WeightUnit},
                   {"GrossWeightUnit",skuUOM.GrossWeightUnit},
                   {"Liter",skuUOM.Liter},
                   {"KGM",skuUOM.KGM},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> BaseUOMExists(string SKUUID, string Code, bool IsBaseUOM)
        {
            var parameters = new Dictionary<string, object>
            {
                {"SKUUID", SKUUID},
                {"Code", Code},
                {"IsBaseUOM", IsBaseUOM },
            };
            var sql = @"SELECT 1
            FROM 
                sku_uom
            WHERE 
                sku_uid = @SKUUID 
                AND code != @Code 
                AND is_base_uom = @IsBaseUOM;";
            var result = await ExecuteScalarAsync<int>(sql, parameters);
            return result == 1;
        }

        private async Task<bool> OuterUOMExists(string SKUUID, string Code, bool IsOuterUOM)
        {
            var parameters = new Dictionary<string, object>
    {
        {"SKUUID", SKUUID},
        {"Code", Code},
        {"IsOuterUOM", IsOuterUOM },
    };
            var sql = @"SELECT 1
            FROM 
                sku_uom
            WHERE 
                sku_uid = @SKUUID 
                AND code != @Code 
                AND is_outer_uom = @IsOuterUOM;";
            var result = await ExecuteScalarAsync<int>(sql, parameters);
            return result == 1;
        }
        public async Task<int> DeleteSKUUOMByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE FROM sku_uom WHERE uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<int> UpdateSKUForBaseAndOuterUOMs(Winit.Modules.SKU.Model.Interfaces.ISKUUOM skuUOM)
        {
            try
            {
                string sql;
                if (skuUOM.IsBaseUOM == true)
                {
                    sql = "update sku set base_uom = @base_uom, server_modified_time = now() where uid = @sku_uid";
                }
                else
                {
                    sql = "update sku set outer_uom = @outer_uom, server_modified_time = now() where uid = @sku_uid";
                }

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"outer_uom",skuUOM.Code},
                    {"base_uom",skuUOM.Code},
                    {"sku_uid",skuUOM.SKUUID},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }


}

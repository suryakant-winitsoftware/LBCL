using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Text;
using Winit.Modules.Tax.Model.Classes;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Tax.DL.Classes;

public class PGSQTaxMasterDL : Base.DL.DBManager.PostgresDBManager, Interfaces.ITaxMasterDL
{
    public PGSQTaxMasterDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }

    public async Task<PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITax>> GetTaxDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(@"SELECT 
                                        id AS ""Id"",
                                        uid AS ""Uid"",
                                        created_by AS ""CreatedBy"",
                                        created_time AS ""CreatedTime"",
                                        modified_by AS ""ModifiedBy"",
                                        modified_time AS ""ModifiedTime"",
                                        server_add_time AS ""ServerAddTime"",
                                        server_modified_time AS ""ServerModifiedTime"",
                                        name AS ""Name"",
                                        legal_name AS ""LegalName"",
                                        applicable_at AS ""ApplicableAt"",
                                        tax_calculation_type AS ""TaxCalculationType"",
                                        base_tax_rate AS ""BaseTaxRate"",
                                        status AS ""Status"",
                                        valid_from AS ""ValidFrom"",
                                        valid_upto AS ""ValidUpto"",
                                        code as ""Code"",
                                        is_tax_on_tax_applicable as ""IsTaxOnTaxApplicable""
                                    FROM 
                                        tax");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS ""Cnt"" FROM tax");

            }
            Dictionary<string, object> parameters = new();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.Tax.Model.Interfaces.ITax>(filterCriterias, sbFilterCriteria, parameters);
                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }
            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ITax>().GetType();

            IEnumerable<Winit.Modules.Tax.Model.Interfaces.ITax> TaxDetails = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITax>(sql.ToString(), parameters, type);
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITax> pagedResponse = new()
            {
                PagedData = TaxDetails,
                TotalCount = totalCount
            };
            return pagedResponse;

        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Winit.Modules.Tax.Model.Interfaces.ITax?> GetTaxByUID(string UID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID",  UID}
        };
        string sql = @"select 
                                    id as ""Id"",
                                    uid as ""UID"",
                                    created_by as ""CreatedBy"",
                                    created_time as ""CreatedTime"",
                                    modified_by as ""ModifiedBy"",
                                    modified_time as ""ModifiedTime"",
                                    server_add_time as ""ServerAddTime"",
                                    server_modified_time as ""ServerModifiedTime"",
                                    name as ""Name"",
                                    legal_name as ""LegalName"",
                                    applicable_at as ""ApplicableAt"",
                                    tax_calculation_type as ""TaxCalculationType"",
                                    base_tax_rate as ""BaseTaxRate"",
                                    status as ""Status"",
                                    valid_from as ""ValidFrom"",
                                    valid_upto as ""ValidUpto"",
                                    code as ""Code"",
                                    is_tax_on_tax_applicable as ""IsTaxOnTaxApplicable""
                                from 
                                    tax 
                                where 
                                    uid = @UID";
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ITax>().GetType();
        return await ExecuteSingleAsync<Winit.Modules.Tax.Model.Interfaces.ITax>(sql, parameters, type);
    }
    public async Task<int> CreateTax(Winit.Modules.Tax.Model.Interfaces.ITax tax)
    {
        try
        {
            string sql = @"INSERT INTO tax 
                            (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            name, legal_name, applicable_at, tax_calculation_type, 
                            base_tax_rate, status, valid_from, valid_upto, code, is_tax_on_tax_applicable) 
                            VALUES 
                            (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, 
                            @ModifiedTime, @ServerAddTime, @ServerModifiedTime,  
                            @Name, @LegalName, @ApplicableAt, @TaxCalculationType, 
                            @BaseTaxRate, @Status, @ValidFrom, @ValidUpto, @Code, @IsTaxOnTaxApplicable) ";
            Dictionary<string, object?> parameters = new()
            {

                { "@UID", tax.UID },
                { "@CreatedBy", tax.CreatedBy },
                { "@CreatedTime", tax.CreatedTime },
                { "@ModifiedBy", tax.ModifiedBy },
                { "@ModifiedTime", tax.ModifiedTime },
                { "@ServerAddTime", tax.ServerAddTime },
                { "@ServerModifiedTime", tax.ServerModifiedTime },
                { "@Name", tax.Name },
                { "@LegalName", tax.LegalName },
                { "@ApplicableAt", tax.ApplicableAt },
                { "@TaxCalculationType", tax.TaxCalculationType },
                { "@BaseTaxRate", tax.BaseTaxRate },
                { "@Status", tax.Status },
                { "@ValidFrom", tax.ValidFrom },
                { "@ValidUpto", tax.ValidUpto },
                { "@Code", tax.Code },
                { "@IsTaxOnTaxApplicable", tax.IsTaxOnTaxApplicable }
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> UpdateTax(Winit.Modules.Tax.Model.Interfaces.ITax tax)
    {
        try
        {
            string sql = @"update tax set 
                                modified_by = @ModifiedBy, 
                                modified_time = @ModifiedTime, 
                                server_modified_time = @ServerModifiedTime, 
                                company_uid = @CompanyUID, 
                                name = @Name, 
                                legal_name = @LegalName, 
                                applicable_at = @ApplicableAt, 
                                tax_calculation_type = @TaxCalculationType, 
                                base_tax_rate = @BaseTaxRate, 
                                status = @Status, 
                                valid_from = @ValidFrom, 
                                valid_upto = @ValidUpto,
                                code = @Code,
                                is_tax_on_tax_applicable = @IsTaxOnTaxApplicable
                            where uid = @UID";
            Dictionary<string, object?> parameters = new()
            {
                { "@UID", tax.UID },
                { "@CreatedBy", tax.CreatedBy },
                { "@CreatedTime", tax.CreatedTime },
                { "@ModifiedBy", tax.ModifiedBy },
                { "@ModifiedTime", tax.ModifiedTime },
                { "@ServerAddTime", tax.ServerAddTime },
                { "@ServerModifiedTime", tax.ServerModifiedTime },
                { "@Name", tax.Name },
                { "@LegalName", tax.LegalName },
                { "@ApplicableAt", tax.ApplicableAt },
                { "@TaxCalculationType", tax.TaxCalculationType },
                { "@BaseTaxRate", tax.BaseTaxRate },
                { "@Status", tax.Status },
                { "@ValidFrom", tax.ValidFrom },
                { "@ValidUpto", tax.ValidUpto },
                { "@Code", tax.Code },
                { "@IsTaxOnTaxApplicable", tax.IsTaxOnTaxApplicable },
             };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteTax(string UID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID" , UID}
        };
        string sql = @"delete from tax where uid = @UID";
        return await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task<(List<Winit.Modules.Tax.Model.Interfaces.ITax>, List<Winit.Modules.Tax.Model.Interfaces.ITaxSlab>)> GetTaxMaster(List<string> OrgUIDs)
    {
        try
        {
            string commaSeparatedorgUIDs = string.Empty;
            if (OrgUIDs != null)
            {
                commaSeparatedorgUIDs = string.Join(",", OrgUIDs);

            }

            Dictionary<string, object> Taxparameters = new()
            {
                {"OrgUIDs", commaSeparatedorgUIDs}
            };
            StringBuilder taxSql = new(@"SELECT T.id AS Id, T.uid AS UID, T.created_by CreatedBy, 
                                        T.created_time AS CreatedTime, T.modified_by ModifiedBy, T.modified_time AS ModifiedTime, 
                                        T.server_add_time AS ServerAddTime, T.server_modified_time AS ServerModifiedTime, 
                                        T.name AS Name, T.legal_name AS LegalName, 
                                        T.applicable_at AS ApplicableAt, T.tax_calculation_type AS TaxCalculationType, 
                                        T.base_tax_rate AS BaseTaxRate, T.status AS Status, 
                                        T.valid_from AS ValidFrom, T.valid_upto AS ValidUpto, T.code AS Code, 
                                        T.is_tax_on_tax_applicable AS IsTaxOnTaxApplicable,
                                        COALESCE(td.dependent_taxes, '') AS DependentTaxes
                                    FROM 
                                    org o
                                    inner join tax_group_taxes tgt on tgt.tax_group_uid = o.tax_group_uid
                                    inner join tax t on t.uid = tgt.tax_uid and t.status = 'Active'
                                    LEFT JOIN (
                                        SELECT
                                            tax_uid,
                                            string_agg(depends_on_tax_uid, ',') AS dependent_taxes
                                        FROM
                                            tax_dependencies
                                        GROUP BY
                                            tax_uid
                                    ) td ON t.uid = td.tax_uid
                                where 
                                    1=1");
            if (!string.IsNullOrEmpty(commaSeparatedorgUIDs))
            {
                _ = taxSql.Append($" AND O.UID = ANY(string_to_array(@ORGUIDs, ','))");
            }

            Type taxType = _serviceProvider.GetRequiredService<Winit.Modules.Tax.Model.Interfaces.ITax>().GetType();
            // Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster = _serviceProvider.CreateInstance<Winit.Modules.Tax.Model.Interfaces.ITaxMaster>();

            List<Winit.Modules.Tax.Model.Interfaces.ITax> taxList = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITax>(taxSql.ToString(), Taxparameters, taxType);

            StringBuilder taxsalbSql = new(@"SELECT 
                                            t.uid AS ""tax_uid"",
                                            ts.uid AS ""tax_slab_uid"",
                                            ts.range_start AS ""range_start"",
                                            ts.range_end AS ""range_end"",
                                            ts.tax_rate AS ""tax_rate""
                                        FROM 
                                            org o 
                                            INNER JOIN tax_group_taxes tgt ON tgt.tax_group_uid = o.tax_group_uid 
                                            INNER JOIN tax t ON t.""uid"" = tgt.tax_uid AND t.""status"" = 'Active' 
                                            LEFT JOIN tax_slab ts ON ts.tax_uid = t.""uid"" AND ts.""status"" = 'Active' 
                                        WHERE 
                                            1 = 1");
            if (!string.IsNullOrEmpty(commaSeparatedorgUIDs))
            {
                _ = taxsalbSql.Append($" AND O.UID = ANY(string_to_array(@ORGUIDs, ','))");
            }

            Dictionary<string, object?> taxslabViewParameters = new();
            Type taxsalbViewType = _serviceProvider.GetRequiredService<Winit.Modules.Tax.Model.Interfaces.ITaxSlab>().GetType();
            List<Winit.Modules.Tax.Model.Interfaces.ITaxSlab> taxsalbViewList = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITaxSlab>(taxsalbSql.ToString(), Taxparameters, taxsalbViewType);




            return (taxList, taxsalbViewList);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> CreateTaxMaster(Winit.Modules.Tax.Model.Classes.TaxMasterDTO taxMasterDTO)
    {
        int count = 0;
        try
        {
            using NpgsqlConnection connection = PostgreConnection();
            await connection.OpenAsync();

            using NpgsqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string taxQuery = @"INSERT INTO tax 
                            (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            name, legal_name, applicable_at, tax_calculation_type, 
                            base_tax_rate, status, valid_from, valid_upto, code, is_tax_on_tax_applicable) 
                            VALUES 
                            (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, 
                            @ModifiedTime, @ServerAddTime, @ServerModifiedTime,  
                            @Name, @LegalName, @ApplicableAt, @TaxCalculationType, 
                            @BaseTaxRate, @Status, @ValidFrom, @ValidUpto, @Code, @IsTaxOnTaxApplicable)";

                Dictionary<string, object?> taxParameters = new()
                {

                    { "@UID",  taxMasterDTO.Tax.UID },
                    { "@CreatedBy",  taxMasterDTO.Tax.CreatedBy },
                    { "@CreatedTime",  taxMasterDTO.Tax.CreatedTime },
                    { "@ModifiedBy",  taxMasterDTO.Tax.ModifiedBy },
                    { "@ModifiedTime",  taxMasterDTO.Tax.ModifiedTime },
                    { "@ServerAddTime",  DateTime.Now },
                    { "@ServerModifiedTime",  DateTime.Now },
                    { "@Name",  taxMasterDTO.Tax.Name },
                    { "@LegalName",  taxMasterDTO.Tax.LegalName },
                    { "@ApplicableAt",  taxMasterDTO.Tax.ApplicableAt },
                    { "@TaxCalculationType", taxMasterDTO.Tax.TaxCalculationType },
                    { "@BaseTaxRate", taxMasterDTO.Tax.BaseTaxRate },
                    { "@Status", taxMasterDTO.Tax.Status },
                    { "@ValidFrom", taxMasterDTO.Tax.ValidFrom },
                    { "@ValidUpto", taxMasterDTO.Tax.ValidUpto },
                    { "@Code", taxMasterDTO.Tax.Code },
                    { "@IsTaxOnTaxApplicable", taxMasterDTO.Tax.IsTaxOnTaxApplicable }
                };

                count += await ExecuteNonQueryAsync(taxQuery, connection, transaction, taxParameters);
                if (count != 1)
                {
                    transaction.Rollback();
                    throw new Exception("Tax Insert failed");
                }

                foreach (TaxSkuMap taxSkuMap in taxMasterDTO.TaxSKUMapList)
                {

                    string taxSkuMapQuery = @"INSERT INTO public.tax_sku_map (
                                             uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                                            sku_uid, tax_uid
                                            )
                                            VALUES (
                                                 @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                                            @ServerModifiedTime, @SKUUID, @TaxUID);";

                    Dictionary<string, object?> taxSkuMapParameters = new()
                    {
                                {"UID",taxSkuMap.UID},
                                {"CreatedBy",taxSkuMap.CreatedBy},
                                {"CreatedTime",taxSkuMap.CreatedTime},
                                {"ModifiedBy",taxSkuMap.ModifiedBy},
                                {"ModifiedTime",taxSkuMap.ModifiedTime},
                                {"ServerAddTime",taxSkuMap.ServerAddTime},
                                {"ServerModifiedTime",DateTime.Now},
                                {"SKUUID",taxSkuMap.SKUUID},
                                {"TaxUID",taxSkuMap.TaxUID},
                            };
                    count += await ExecuteNonQueryAsync(taxSkuMapQuery, connection, transaction, taxSkuMapParameters);
                    if (count < 0)
                    {
                        transaction.Rollback();
                        throw new Exception("TaxSkuMap Table Insert Failed");
                    }
                }
                transaction.Commit();
                int total = count;
                return total;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdateTaxMaster(Winit.Modules.Tax.Model.Classes.TaxMasterDTO taxMasterDTO)
    {
        try
        {
            using NpgsqlConnection connection = PostgreConnection();
            await connection.OpenAsync();

            using NpgsqlTransaction transaction = connection.BeginTransaction();
            try
            {

                string taxSQL = @"UPDATE public.tax SET 
                                                modified_by = @ModifiedBy, 
                                                modified_time = @ModifiedTime, 
                                                server_modified_time = @ServerModifiedTime, 
                                                name = @Name, 
                                                legal_name = @LegalName, 
                                                applicable_at = @ApplicableAt, 
                                                tax_calculation_type = @TaxCalculationType, 
                                                base_tax_rate = @BaseTaxRate, 
                                                status = @Status, 
                                                valid_from = @ValidFrom, 
                                                valid_upto = @ValidUpto,
                                                code = @Code,
                                                is_tax_on_tax_applicable = @IsTaxOnTaxApplicable
                                            WHERE uid = @UID;";
                Dictionary<string, object?> taxparameters = new()
                {
                { "@UID", taxMasterDTO.Tax.UID },
                { "@CreatedBy",  taxMasterDTO.Tax.CreatedBy },
                { "@CreatedTime",  taxMasterDTO.Tax.CreatedTime },
                { "@ModifiedBy",  taxMasterDTO.Tax.ModifiedBy },
                { "@ModifiedTime",  taxMasterDTO.Tax.ModifiedTime },
                { "@ServerAddTime",  taxMasterDTO.Tax.ServerAddTime },
                { "@ServerModifiedTime",  DateTime.Now },
                { "@Name",  taxMasterDTO.Tax.Name },
                { "@LegalName",  taxMasterDTO.Tax.LegalName },
                { "@ApplicableAt",  taxMasterDTO.Tax.ApplicableAt },
                { "@TaxCalculationType",  taxMasterDTO.Tax.TaxCalculationType },
                { "@BaseTaxRate",  taxMasterDTO.Tax.BaseTaxRate },
                { "@Status",  taxMasterDTO.Tax.Status },
                { "@ValidFrom",  taxMasterDTO.Tax.ValidFrom },
                { "@ValidUpto",  taxMasterDTO.Tax.ValidUpto },
                { "@Code",  taxMasterDTO.Tax.Code },
                { "@IsTaxOnTaxApplicable",  taxMasterDTO.Tax.IsTaxOnTaxApplicable },
             };

                int count = await ExecuteNonQueryAsync(taxSQL, connection, transaction, taxparameters);
                if (count < 0)
                {
                    transaction.Rollback();
                    throw new Exception("Tax Update failed");
                }

                if (taxMasterDTO.TaxSKUMapList != null && taxMasterDTO.TaxSKUMapList.Count > 0)
                {
                    foreach (TaxSkuMap taxSkuMap in taxMasterDTO.TaxSKUMapList)
                    {
                        switch (taxSkuMap.ActionType)
                        {
                            case Winit.Shared.Models.Enums.ActionType.Add:
                                count += await InsertTaxSKUMap(connection, transaction, taxSkuMap);
                                break;

                            case Winit.Shared.Models.Enums.ActionType.Update:
                                count += await UpdateTaxSKUMap(connection, transaction, taxSkuMap);
                                break;
                            case Winit.Shared.Models.Enums.ActionType.Delete:
                                count += await DeleteTaxSKUMap(connection, transaction, taxSkuMap);
                                break;
                        }
                    }
                }


                transaction.Commit();
                int total = count;
                return total;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<int> InsertTaxSKUMap(NpgsqlConnection connection, NpgsqlTransaction transaction, Winit.Modules.Tax.Model.Classes.TaxSkuMap taxSkuMap)
    {
        string taxskuMapQuery = @"INSERT INTO tax_sku_map (
                    uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, sku_uid, tax_uid
                )
                VALUES (
                    @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SKUUID, @TaxUID
                );";
        Dictionary<string, object?> taxskuMapListParameters = new()
        {
                                {"UID",taxSkuMap.UID},
                                {"CreatedBy",taxSkuMap.CreatedBy},
                                {"CreatedTime",taxSkuMap.CreatedTime},
                                {"ModifiedBy",taxSkuMap.ModifiedBy},
                                {"ModifiedTime",taxSkuMap.ModifiedTime},
                                {"ServerAddTime",DateTime.Now},
                                {"ServerModifiedTime",DateTime.Now},
                                {"SKUUID",taxSkuMap.SKUUID},
                                {"TaxUID",taxSkuMap.TaxUID},
                    };

        return await ExecuteNonQueryAsync(taxskuMapQuery, connection, transaction, taxskuMapListParameters);
    }

    private async Task<int> UpdateTaxSKUMap(NpgsqlConnection connection, NpgsqlTransaction transaction, Winit.Modules.Tax.Model.Classes.TaxSkuMap taxSkuMap)
    {
        string taxskuMapQuery = @"UPDATE tax_sku_map
                                    SET 
                                        modified_by = @ModifiedBy, 
                                        modified_time = @ModifiedTime, 
                                        server_modified_time = @ServerModifiedTime
                                    WHERE uid = @UID ";
        Dictionary<string, object?> taxskuMapListParameters = new()
        {
               {"UID",taxSkuMap.UID},
               {"ModifiedBy",taxSkuMap.ModifiedBy},
               {"ModifiedTime",taxSkuMap.ModifiedTime},
               {"ServerModifiedTime",DateTime.Now},
        };

        return await ExecuteNonQueryAsync(taxskuMapQuery, connection, transaction, taxskuMapListParameters);
    }

    private async Task<int> DeleteTaxSKUMap(NpgsqlConnection connection, NpgsqlTransaction transaction, Winit.Modules.Tax.Model.Classes.TaxSkuMap taxSkuMap)
    {
        string taxskuMapQuery = @"DELETE FROM tax_sku_map WHERE uid = @UID";

        Dictionary<string, object?> taxskuMapListParameters = new()
        {
               {"UID",taxSkuMap.UID},
        };

        return await ExecuteNonQueryAsync(taxskuMapQuery, connection, transaction, taxskuMapListParameters);
    }

    public async Task<(List<Model.Interfaces.ITax>, List<Model.Interfaces.ITaxSkuMap>)> SelectTaxMasterViewByUID(string UID)
    {
        try
        {
            Dictionary<string, object?> Parameters = new()
            {
                { "UID", UID },
            };
            StringBuilder TaxSql = new(@"SELECT 
                                        id AS ""Id"",
                                        uid AS ""Uid"",
                                        created_by AS ""CreatedBy"",
                                        created_time AS ""CreatedTime"",
                                        modified_by AS ""ModifiedBy"",
                                        modified_time AS ""ModifiedTime"",
                                        server_add_time AS ""ServerAddTime"",
                                        server_modified_time AS ""ServerModifiedTime"",
                                        name AS ""Name"",
                                        legal_name AS ""LegalName"",
                                        applicable_at AS ""ApplicableAt"",
                                        tax_calculation_type AS ""TaxCalculationType"",
                                        base_tax_rate AS ""BaseTaxRate"",
                                        status AS ""Status"",
                                        valid_from AS ""ValidFrom"",
                                        valid_upto AS ""ValidUpto"",
                                        code as ""Code"",
                                        is_tax_on_tax_applicable as ""IsTaxOnTaxApplicable""
                                        FROM tax
                                WHERE uid = @UID");
            Type TaxSqlType = _serviceProvider.GetRequiredService<Model.Interfaces.ITax>().GetType();
            List<Model.Interfaces.ITax> TaxList = await ExecuteQueryAsync<Model.Interfaces.ITax>(TaxSql.ToString(), Parameters, TaxSqlType);
            StringBuilder TaxskuMapSQL = new(@"SELECT
                                                        id AS ""Id"",
                                                        uid AS ""UID"",
                                                        created_by AS ""CreatedBy"",
                                                        created_time AS ""CreatedTime"",
                                                        modified_by AS ""ModifiedBy"",
                                                        modified_time AS ""ModifiedTime"",
                                                        server_add_time AS ""ServerAddTime"",
                                                        server_modified_time AS ""ServerModifiedTime"",
                                                        sku_uid AS ""SKUUID"",
                                                        tax_uid AS ""TaxUID""
                                                    FROM tax_sku_map
                                                    WHERE tax_uid = @UID");
            Type TaxskuMapType = _serviceProvider.GetRequiredService<Model.Interfaces.ITaxSkuMap>().GetType();
            List<Model.Interfaces.ITaxSkuMap> TaxskuMapList = await ExecuteQueryAsync<Model.Interfaces.ITaxSkuMap>(TaxskuMapSQL.ToString(), Parameters, TaxskuMapType);

            return (TaxList, TaxskuMapList);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>> GetTaxGroupDetails(List<SortCriteria> sortCriterias, int pageNumber,
     int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(@"SELECT
                            id AS ""Id"",
                            uid AS ""UID"",
                            created_by AS ""CreatedBy"",
                            created_time AS ""CreatedTime"",
                            modified_by AS ""ModifiedBy"",
                            modified_time AS ""ModifiedTime"",
                            server_add_time AS ""ServerAddTime"",
                            server_modified_time AS ""ServerModifiedTime"",
                            name AS ""Name"",
                            code AS ""Code""
                        FROM tax_group
                        ");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS ""Cnt"" FROM tax_group");

            }
            Dictionary<string, object> parameters = new();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>(filterCriterias, sbFilterCriteria, parameters); ;
                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }
            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ITaxGroup>().GetType();

            IEnumerable<Winit.Modules.Tax.Model.Interfaces.ITaxGroup> TaxGroupDetails = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>(sql.ToString(), parameters, type);
            int totalCount = -1;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITaxGroup> pagedResponse = new()
            {
                PagedData = TaxGroupDetails,
                TotalCount = totalCount
            };
            return pagedResponse;

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Winit.Modules.Tax.Model.Interfaces.ITaxGroup?> GetTaxGroupByUID(string UID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID",  UID}
        };
        string sql = @"SELECT
                            id AS ""Id"",
                            uid AS ""UID"",
                            created_by AS ""CreatedBy"",
                            created_time AS ""CreatedTime"",
                            modified_by AS ""ModifiedBy"",
                            modified_time AS ""ModifiedTime"",
                            server_add_time AS ""ServerAddTime"",
                            server_modified_time AS ""ServerModifiedTime"",
                            name AS ""Name"",
                            code AS ""Code""
                        FROM tax_group
                        WHERE uid = @UID
                        ";
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ITaxGroup>().GetType();

        return await ExecuteSingleAsync<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>(sql, parameters, type);
    }
    public async Task<int> CreateTaxGroupMaster(Winit.Modules.Tax.Model.Classes.TaxGroupMasterDTO taxGroupMasterDTO)
    {
        int count = 0;
        try
        {
            using NpgsqlConnection connection = PostgreConnection();
            await connection.OpenAsync();

            using NpgsqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string taxGroupQuery = @"INSERT INTO tax_group (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                                               server_modified_time,  name,code)
                                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,
                                                @ServerModifiedTime, @Name,@Code)";
                Dictionary<string, object?> taxGroupParameters = new()
                {
                            { "@UID", taxGroupMasterDTO.TaxGroup.UID },
                            { "@CreatedBy", taxGroupMasterDTO.TaxGroup.CreatedBy },
                            { "@CreatedTime", taxGroupMasterDTO.TaxGroup.CreatedTime },
                            { "@ModifiedBy", taxGroupMasterDTO.TaxGroup.ModifiedBy },
                            { "@ModifiedTime", taxGroupMasterDTO.TaxGroup.ModifiedTime },
                            { "@ServerAddTime", taxGroupMasterDTO.TaxGroup.ServerAddTime },
                            { "@ServerModifiedTime", taxGroupMasterDTO.TaxGroup.ServerModifiedTime },
                            { "@Name", taxGroupMasterDTO.TaxGroup.Name },
                            { "@Code", taxGroupMasterDTO.TaxGroup.Code },
                        };

                count += await ExecuteNonQueryAsync(taxGroupQuery, connection, transaction, taxGroupParameters);
                if (count != 1)
                {
                    transaction.Rollback();
                    throw new Exception("Tax Insert failed");
                }

                foreach (TaxGroupTaxes taxGroupTaxes in taxGroupMasterDTO.TaxGroupTaxes)
                {

                    string taxGroupTaxesQuery = @"INSERT INTO tax_group_taxes (uid, created_by, created_time, modified_by, modified_time,
                                            server_add_time, server_modified_time, tax_group_uid, tax_uid)
                                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                                            @ServerAddTime, @ServerModifiedTime, @TaxGroupUID, @TaxUID)";
                    Dictionary<string, object?> taxGroupTaxesParameters = new()
                    {
                                { "@UID", taxGroupTaxes.UID },
                                { "@CreatedBy", taxGroupTaxes.CreatedBy },
                                { "@CreatedTime", taxGroupTaxes.CreatedTime },
                                { "@ModifiedBy", taxGroupTaxes.ModifiedBy },
                                { "@ModifiedTime", taxGroupTaxes.ModifiedTime },
                                { "@ServerAddTime", taxGroupTaxes.ServerAddTime },
                                { "@ServerModifiedTime", taxGroupTaxes.ServerModifiedTime },
                                { "@TaxGroupUID", taxGroupTaxes.TaxGroupUID },
                                { "@TaxUID", taxGroupTaxes.TaxUID },
                            };
                    count += await ExecuteNonQueryAsync(taxGroupTaxesQuery, connection, transaction, taxGroupTaxesParameters);
                    if (count < 0)
                    {
                        transaction.Rollback();
                        throw new Exception("TaxSkuMap Table Insert Failed");
                    }
                }
                transaction.Commit();
                int total = count;
                return total;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> UpdateTaxGroupMaster(Winit.Modules.Tax.Model.Classes.TaxGroupMasterDTO taxGroupMasterDTO)
    {
        int count = 0;
        try
        {
            using NpgsqlConnection connection = PostgreConnection();
            await connection.OpenAsync();

            using NpgsqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string taxGroupQuery = @"update tax_group
                                                    set 
                                                        modified_by = @ModifiedBy, 
                                                        modified_time = @ModifiedTime, 
                                                        server_modified_time = @ServerModifiedTime, 
                                                        name = @Name,
                                                        code = @Code
                                                    where uid = @UID";
                Dictionary<string, object?> taxGroupParameters = new()
                {
                            { "@UID", taxGroupMasterDTO.TaxGroup.UID },
                            { "@ModifiedBy", taxGroupMasterDTO.TaxGroup.ModifiedBy },
                            { "@ModifiedTime", taxGroupMasterDTO.TaxGroup.ModifiedTime },
                            { "@ServerModifiedTime", taxGroupMasterDTO.TaxGroup.ServerModifiedTime },
                            { "@Name", taxGroupMasterDTO.TaxGroup.Name },
                            { "@Code", taxGroupMasterDTO.TaxGroup.Code },
                        };

                count += await ExecuteNonQueryAsync(taxGroupQuery, connection, transaction, taxGroupParameters);
                if (count != 1)
                {
                    transaction.Rollback();
                    throw new Exception("Tax Insert failed");
                }

                foreach (TaxGroupTaxes taxGroupTaxes in taxGroupMasterDTO.TaxGroupTaxes)
                {
                    int Count = -1;
                    switch (taxGroupTaxes.ActionType)
                    {
                        case ActionType.Add:
                            Count = await CreateTaxGroupTaxes(connection, transaction, taxGroupTaxes);
                            break;
                        case ActionType.Update:
                            Count = await UpdateTaxGroupTaxes(connection, transaction, taxGroupTaxes);
                            break;
                        case ActionType.Delete:
                            Count = await DeleteTaxGroupTaxes(connection, transaction, taxGroupTaxes.UID);
                            break;
                    }
                    if (count <= 0)
                    {
                        transaction.Rollback();
                        throw new Exception("TaxGroup Taxes  Table update Failed");
                    }
                }
                transaction.Commit();
                int total = count;
                return total;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<IEnumerable<ITaxSelectionItem>> GetTaxSelectionItems(string taxGroupUID)
    {
        StringBuilder sql = new(@"select case when tgt.tax_group_uid = @TaxGroupUID then cast(1 as bit) else cast(0 as bit) end as IsSelected,
                                        t.uid as TaxUID,
                                        t.name as TaxName,
                                        tgt.*
                                    from tax t
                                    left join tax_group_taxes tgt on tgt.tax_uid = t.uid
                                    where (tgt.tax_group_uid = @TaxGroupUID or tgt.id is null)");

        Dictionary<string, object> parameters = new()
        {
            {"TaxGroupUID",taxGroupUID }
        };

        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ITaxSelectionItem>().GetType();
        IEnumerable<Winit.Modules.Tax.Model.Interfaces.ITaxSelectionItem> TaxSelectionItems = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITaxSelectionItem>(sql.ToString(), parameters, type);
        return TaxSelectionItems;
    }
    public async Task<int> CreateTaxGroupTaxes(NpgsqlConnection connection, NpgsqlTransaction transaction, Winit.Modules.Tax.Model.Interfaces.ITaxGroupTaxes taxGroupTaxes)
    {
        try
        {
            string sql = @"insert into tax_group_taxes (uid, created_by, created_time, modified_by, modified_time, 
                                server_add_time, server_modified_time, tax_group_uid, tax_uid) values (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, 
                                @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @TaxGroupUID, @TaxUID)";
            Dictionary<string, object?> parameters = new()
            {

               { "@UID", taxGroupTaxes.UID },
               { "@CreatedBy", taxGroupTaxes.CreatedBy },
               { "@CreatedTime", taxGroupTaxes.CreatedTime },
               { "@ModifiedBy", taxGroupTaxes.ModifiedBy },
               { "@ModifiedTime", taxGroupTaxes.ModifiedTime },
               { "@ServerAddTime", taxGroupTaxes.ServerAddTime },
               { "@ServerModifiedTime", taxGroupTaxes.ServerModifiedTime },
               { "@TaxGroupUID", taxGroupTaxes.TaxGroupUID },
               { "@TaxUID", taxGroupTaxes.TaxUID },
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<int> UpdateTaxGroupTaxes(NpgsqlConnection connection, NpgsqlTransaction transaction, Winit.Modules.Tax.Model.Interfaces.ITaxGroupTaxes taxGroupTaxes)
    {
        string taxGroupTaxesQuery = @"update tax_group_taxes
                                    set modified_by = @ModifiedBy,
                                        modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,
                                        tax_group_uid = @TaxGroupUID,
                                        tax_uid = @TaxUID
                                    where uid = @UID";
        Dictionary<string, object?> taxGroupTaxesParameters = new()
        {
               { "@UID", taxGroupTaxes.UID },
               { "@ModifiedBy", taxGroupTaxes.ModifiedBy },
               { "@ModifiedTime", taxGroupTaxes.ModifiedTime },
               { "@ServerModifiedTime", taxGroupTaxes.ServerModifiedTime },
               { "@TaxGroupUID", taxGroupTaxes.TaxGroupUID },
               { "@TaxUID", taxGroupTaxes.TaxUID },
        };

        return await ExecuteNonQueryAsync(taxGroupTaxesQuery, connection, transaction, taxGroupTaxesParameters);
    }
    private async Task<int> DeleteTaxGroupTaxes(NpgsqlConnection connection, NpgsqlTransaction transaction, string UID)
    {
        string taxGroupTaxesQuery = @"delete from tax_group_taxes where uid = @UID";

        Dictionary<string, object?> taxGroupTaxesParameters = new()
        {
               {"UID",UID},
        };

        return await ExecuteNonQueryAsync(taxGroupTaxesQuery, connection, transaction, taxGroupTaxesParameters);
    }

}










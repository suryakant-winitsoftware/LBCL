using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;
using Winit.Modules.Tax.Model.Classes;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Tax.DL.Classes
{
    public class MSSQTaxMasterDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.ITaxMasterDL
    {
        public MSSQTaxMasterDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITax>> GetTaxDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new(@"Select * From (SELECT 
                                        id AS Id,
                                        uid AS Uid,
                                        created_by AS CreatedBy,
                                        created_time AS CreatedTime,
                                        modified_by AS ModifiedBy,
                                        modified_time AS ModifiedTime,
                                        server_add_time AS ServerAddTime,
                                        server_modified_time AS ServerModifiedTime,
                                        name AS Name,
                                        legal_name AS LegalName,
                                        applicable_at AS ApplicableAt,
                                        tax_calculation_type AS TaxCalculationType,
                                        base_tax_rate AS BaseTaxRate,
                                        status AS Status,
                                        valid_from AS ValidFrom,
                                        valid_upto AS ValidUpto,
                                        code as Code,
                                        is_tax_on_tax_applicable as IsTaxOnTaxApplicable
                                    FROM 
                                        tax)As SubQuery");
                StringBuilder sqlCount = new();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                        id AS Id,
                                        uid AS Uid,
                                        created_by AS CreatedBy,
                                        created_time AS CreatedTime,
                                        modified_by AS ModifiedBy,
                                        modified_time AS ModifiedTime,
                                        server_add_time AS ServerAddTime,
                                        server_modified_time AS ServerModifiedTime,
                                        name AS Name,
                                        legal_name AS LegalName,
                                        applicable_at AS ApplicableAt,
                                        tax_calculation_type AS TaxCalculationType,
                                        base_tax_rate AS BaseTaxRate,
                                        status AS Status,
                                        valid_from AS ValidFrom,
                                        valid_upto AS ValidUpto,
                                        code as Code,
                                        is_tax_on_tax_applicable as IsTaxOnTaxApplicable
                                    FROM 
                                        tax)As SubQuery");

                }
                Dictionary<string, object> parameters = [];
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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        _ = sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<Winit.Modules.Tax.Model.Interfaces.ITax> TaxDetails = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITax>(sql.ToString(), parameters);
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
                                    id as Id,
                                    uid as UID,
                                    created_by as CreatedBy,
                                    created_time as CreatedTime,
                                    modified_by as ModifiedBy,
                                    modified_time as ModifiedTime,
                                    server_add_time as ServerAddTime,
                                    server_modified_time as ServerModifiedTime,
                                    name as Name,
                                    legal_name as LegalName,
                                    applicable_at as ApplicableAt,
                                    tax_calculation_type as TaxCalculationType,
                                    base_tax_rate as BaseTaxRate,
                                    status as Status,
                                    valid_from as ValidFrom,
                                    valid_upto as ValidUpto,
                                    code as Code,
                                    is_tax_on_tax_applicable as IsTaxOnTaxApplicable
                                from 
                                    tax 
                                where 
                                    uid = @UID";
            return await ExecuteSingleAsync<Winit.Modules.Tax.Model.Interfaces.ITax>(sql, parameters);
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

                return await ExecuteNonQueryAsync(sql, tax);
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

                return await ExecuteNonQueryAsync(sql, tax);
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
                var Taxparameters = new { orgUIDs = OrgUIDs };
                StringBuilder taxSql = new("""
                                        SELECT T.id AS Id, T.uid AS UID, T.created_by CreatedBy, 
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
                                    inner join.tax_group_taxes tgt on tgt.tax_group_uid = o.tax_group_uid
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
                                    1=1
                                """);
                if (Taxparameters != null)
                {
                    _ = taxSql.Append($"AND O.[UID] IN @orgUIDs");
                }

                // Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster = _serviceProvider.CreateInstance<Winit.Modules.Tax.Model.Interfaces.ITaxMaster>();

                List<Winit.Modules.Tax.Model.Interfaces.ITax> taxList = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITax>(taxSql.ToString(), Taxparameters);

                StringBuilder taxsalbSql = new(@"SELECT 
                                            t.uid AS tax_uid,
                                            ts.uid AS tax_slab_uid,
                                            ts.range_start AS range_start,
                                            ts.range_end AS range_end,
                                            ts.tax_rate AS tax_rate
                                        FROM 
                                            org o 
                                            INNER JOIN tax_group_taxes tgt ON tgt.tax_group_uid = o.tax_group_uid 
                                            INNER JOIN tax t ON t.uid = tgt.tax_uid AND t.status = 'Active' 
                                            LEFT JOIN tax_slab ts ON ts.tax_uid = t.uid AND ts.status = 'Active' 
                                        WHERE 
                                            1 = 1");
                if (Taxparameters != null)
                {
                    _ = taxsalbSql.Append($"AND O.[UID] IN @orgUIDs");
                }
                List<Winit.Modules.Tax.Model.Interfaces.ITaxSlab> taxsalbViewList = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITaxSlab>(taxsalbSql.ToString(), Taxparameters);
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
                using SqlConnection connection = CreateConnection();
                await connection.OpenAsync();

                using SqlTransaction transaction = connection.BeginTransaction();
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

                    count += await ExecuteNonQueryAsync(taxQuery, connection, transaction, taxMasterDTO.Tax);
                    if (count != 1)
                    {
                        transaction.Rollback();
                        throw new Exception("Tax Insert failed");
                    }

                    if (taxMasterDTO.TaxSKUMapList != null && taxMasterDTO.TaxSKUMapList.Any())
                    {
                        string taxSkuMapQuery = @"INSERT INTO tax_sku_map (
                                             uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                                            sku_uid, tax_uid
                                            )
                                            VALUES (
                                                 @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                                            @ServerModifiedTime, @SKUUID, @TaxUID);";


                        count += await ExecuteNonQueryAsync(taxSkuMapQuery, connection, transaction, taxMasterDTO.TaxSKUMapList);
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
                using SqlConnection connection = CreateConnection();
                await connection.OpenAsync();

                using SqlTransaction transaction = connection.BeginTransaction();
                try
                {

                    string taxSQL = @"UPDATE tax SET 
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


                    int count = await ExecuteNonQueryAsync(taxSQL, connection, transaction, taxMasterDTO.Tax);
                    if (count < 0)
                    {
                        transaction.Rollback();
                        throw new Exception("Tax Update failed");
                    }

                    if (taxMasterDTO.TaxSKUMapList != null && taxMasterDTO.TaxSKUMapList.Count > 0)
                    {
                        List<string> listOfUIDs = taxMasterDTO.TaxSKUMapList.Select(x => x.UID).ToList();
                        List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.TaxSKUMap, listOfUIDs);
                        List<TaxSkuMap>? newTaxSkuMaps = null;
                        List<TaxSkuMap>? existingTaxSkuMaps = null;
                        if (existingUIDs != null && existingUIDs.Count > 0)
                        {
                            newTaxSkuMaps = taxMasterDTO.TaxSKUMapList.Where(sol => !existingUIDs.Contains(sol.UID)).ToList();
                            existingTaxSkuMaps = taxMasterDTO.TaxSKUMapList.Where(e => existingUIDs.Contains(e.UID)).ToList();
                        }
                        else
                        {
                            newTaxSkuMaps = taxMasterDTO.TaxSKUMapList;
                        }

                        if (existingTaxSkuMaps != null && existingTaxSkuMaps.Any())
                        {
                            count += await UpdateTaxSKUMaps(connection, transaction, existingTaxSkuMaps);
                        }
                        if (newTaxSkuMaps.Any())
                        {
                            count += await InsertTaxSKUMaps(connection, transaction, newTaxSkuMaps);
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

        private async Task<int> InsertTaxSKUMaps(IDbConnection connection, IDbTransaction transaction, List<Winit.Modules.Tax.Model.Classes.TaxSkuMap> taxSkuMaps)
        {
            string taxskuMapQuery = @"INSERT INTO tax_sku_map (
                    uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, sku_uid, tax_uid
                )
                VALUES (
                    @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SKUUID, @TaxUID
                );";
            return await ExecuteNonQueryAsync(taxskuMapQuery, connection, transaction, taxSkuMaps);
        }

        private async Task<int> UpdateTaxSKUMaps(IDbConnection connection, IDbTransaction transaction, List<Winit.Modules.Tax.Model.Classes.TaxSkuMap> taxSkuMaps)
        {
            string taxskuMapQuery = @"UPDATE tax_sku_map
                                    SET 
                                        modified_by = @ModifiedBy, 
                                        modified_time = @ModifiedTime, 
                                        server_modified_time = @ServerModifiedTime
                                    WHERE uid = @UID ";
            return await ExecuteNonQueryAsync(taxskuMapQuery, connection, transaction, taxSkuMaps);
        }

        private async Task<int> DeleteTaxSKUMap(IDbConnection connection, IDbTransaction transaction, Winit.Modules.Tax.Model.Classes.TaxSkuMap taxSkuMap)
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
                                        id AS Id,
                                        uid AS Uid,
                                        created_by AS CreatedBy,
                                        created_time AS CreatedTime,
                                        modified_by AS ModifiedBy,
                                        modified_time AS ModifiedTime,
                                        server_add_time AS ServerAddTime,
                                        server_modified_time AS ServerModifiedTime,
                                        name AS Name,
                                        legal_name AS LegalName,
                                        applicable_at AS ApplicableAt,
                                        tax_calculation_type AS TaxCalculationType,
                                        base_tax_rate AS BaseTaxRate,
                                        status AS Status,
                                        valid_from AS ValidFrom,
                                        valid_upto AS ValidUpto,
                                        code as Code,
                                        is_tax_on_tax_applicable as IsTaxOnTaxApplicable
                                        FROM tax
                                WHERE uid = @UID");
                List<Model.Interfaces.ITax> TaxList = await ExecuteQueryAsync<Model.Interfaces.ITax>(TaxSql.ToString(), Parameters);
                StringBuilder TaxskuMapSQL = new(@"SELECT
                                                        id AS Id,
                                                        uid AS UID,
                                                        created_by AS CreatedBy,
                                                        created_time AS CreatedTime,
                                                        modified_by AS ModifiedBy,
                                                        modified_time AS ModifiedTime,
                                                        server_add_time AS ServerAddTime,
                                                        server_modified_time AS ServerModifiedTime,
                                                        sku_uid AS SKUUID,
                                                        tax_uid AS TaxUID
                                                    FROM tax_sku_map
                                                    WHERE tax_uid = @UID");
                List<Model.Interfaces.ITaxSkuMap> TaxskuMapList = await ExecuteQueryAsync<Model.Interfaces.ITaxSkuMap>(TaxskuMapSQL.ToString(), Parameters);
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
                StringBuilder sql = new(@"Select * From (SELECT
                            id AS Id,
                            uid AS UID,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            name AS Name,
                            code AS Code
                        FROM tax_group)AS SubQuery
                        ");
                StringBuilder sqlCount = new();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                            (SELECT
                            id AS Id,
                            uid AS UID,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            name AS Name,
                            code AS Code
                        FROM tax_group)AS SubQuery");

                }
                Dictionary<string, object> parameters = [];
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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        _ = sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<Winit.Modules.Tax.Model.Interfaces.ITaxGroup> TaxGroupDetails = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>(sql.ToString(), parameters);
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
                            id AS Id,
                            uid AS UID,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            name AS Name,
                            code AS Code
                        FROM tax_group
                        WHERE uid = @UID
                        ";

            return await ExecuteSingleAsync<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>(sql, parameters);
        }
        public async Task<int> CreateTaxGroupMaster(Winit.Modules.Tax.Model.Classes.TaxGroupMasterDTO taxGroupMasterDTO)
        {
            int count = 0;
            try
            {
                using SqlConnection connection = CreateConnection();
                await connection.OpenAsync();

                using SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    string taxGroupQuery = @"INSERT INTO tax_group (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                                               server_modified_time,  name,code)
                                                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,
                                                @ServerModifiedTime, @Name,@Code)";
                    count += await ExecuteNonQueryAsync(taxGroupQuery, connection, transaction, taxGroupMasterDTO.TaxGroup);
                    if (count != 1)
                    {
                        transaction.Rollback();
                        throw new Exception("Tax Insert failed");
                    }
                    string taxGroupTaxesQuery = @"INSERT INTO tax_group_taxes (uid, created_by, created_time, modified_by, modified_time,
                                            server_add_time, server_modified_time, tax_group_uid, tax_uid)
                                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                                            @ServerAddTime, @ServerModifiedTime, @TaxGroupUID, @TaxUID)";

                    count += await ExecuteNonQueryAsync(taxGroupTaxesQuery, connection, transaction, taxGroupMasterDTO.TaxGroupTaxes);
                    if (count < 0)
                    {
                        transaction.Rollback();
                        throw new Exception("TaxSkuMap Table Insert Failed");
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
                using SqlConnection connection = CreateConnection();
                await connection.OpenAsync();

                using SqlTransaction transaction = connection.BeginTransaction();
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
                    count += await ExecuteNonQueryAsync(taxGroupQuery, connection, transaction, taxGroupMasterDTO.TaxGroup);
                    if (taxGroupMasterDTO.TaxGroupTaxes != null && taxGroupMasterDTO.TaxGroupTaxes.Any())
                    {

                        List<TaxGroupTaxes>? newTaxGroupTaxes = null;
                        List<TaxGroupTaxes>? existingTaxGroupTaxes = null;
                        List<TaxGroupTaxes>? deleteTaxGroupTaxes = null;
                        if (taxGroupMasterDTO.TaxGroupTaxes.Any(e => e.ActionType == ActionType.Add))
                        {
                            newTaxGroupTaxes = taxGroupMasterDTO.TaxGroupTaxes.Where(x => x.ActionType == ActionType.Add).ToList();
                        }
                        if (taxGroupMasterDTO.TaxGroupTaxes.Any(e => e.ActionType == ActionType.Update))
                        {
                            existingTaxGroupTaxes = taxGroupMasterDTO.TaxGroupTaxes.Where(x => x.ActionType == ActionType.Update).ToList();
                        }
                        if (taxGroupMasterDTO.TaxGroupTaxes.Any(e => e.ActionType == ActionType.Delete))
                        {
                            deleteTaxGroupTaxes = taxGroupMasterDTO.TaxGroupTaxes.Where(x => x.ActionType == ActionType.Delete).ToList();
                        }
                        if (existingTaxGroupTaxes != null && existingTaxGroupTaxes.Any())
                        {
                            count += await UpdateTaxGroupTaxes(connection, transaction, existingTaxGroupTaxes.Cast<ITaxGroupTaxes>().ToList());
                        }
                        if (newTaxGroupTaxes != null && newTaxGroupTaxes.Any())
                        {
                            count += await CreateTaxGroupTaxes(connection, transaction, newTaxGroupTaxes.Cast<ITaxGroupTaxes>().ToList());
                        }
                        if (deleteTaxGroupTaxes != null && deleteTaxGroupTaxes.Any())
                        {
                            count += await DeleteTaxGroupTaxes(connection, transaction, deleteTaxGroupTaxes.Select(e => e.UID).ToList());
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

            IEnumerable<Winit.Modules.Tax.Model.Interfaces.ITaxSelectionItem> TaxSelectionItems = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITaxSelectionItem>(sql.ToString(), parameters);
            return TaxSelectionItems;
        }
        public async Task<int> CreateTaxGroupTaxes(IDbConnection connection, IDbTransaction transaction, List<Winit.Modules.Tax.Model.Interfaces.ITaxGroupTaxes> taxGroupTaxes)
        {
            int retVal;
            try
            {
                string sql = @"insert into tax_group_taxes (uid, created_by, created_time, modified_by, modified_time, 
                                server_add_time, server_modified_time, tax_group_uid, tax_uid) values (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, 
                                @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @TaxGroupUID, @TaxUID)";

                retVal = await ExecuteNonQueryAsync(sql, connection, transaction, taxGroupTaxes);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> UpdateTaxGroupTaxes(IDbConnection connection, IDbTransaction transaction, List<Winit.Modules.Tax.Model.Interfaces.ITaxGroupTaxes> taxGroupTaxes)
        {
            string taxGroupTaxesQuery = @"update tax_group_taxes
                                    set modified_by = @ModifiedBy,
                                        modified_time = @ModifiedTime,
                                        server_modified_time = @ServerModifiedTime,
                                        tax_group_uid = @TaxGroupUID,
                                        tax_uid = @TaxUID
                                    where uid = @UID";


            return await ExecuteNonQueryAsync(taxGroupTaxesQuery, connection, transaction, taxGroupTaxes);
        }
        private async Task<int> DeleteTaxGroupTaxes(IDbConnection connection, IDbTransaction transaction, List<string> uIDs)
        {
            int retVal;
            try
            {
                string taxGroupTaxesQuery = @"delete from tax_group_taxes where uid In @UIDs";
                var taxGroupTaxesParameters = new { UIDs = uIDs };
                retVal = await ExecuteNonQueryAsync(taxGroupTaxesQuery, connection, transaction, taxGroupTaxesParameters);
            }
            catch
            {
                throw;
            }
            return retVal;

        }
    }
}










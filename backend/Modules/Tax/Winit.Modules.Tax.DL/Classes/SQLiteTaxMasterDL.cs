using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Winit.Modules.Base.Model;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.Tax.Model.Classes;
using Winit.Modules.Tax.DL.Interfaces;
using Nest;

namespace Winit.Modules.Tax.DL.Classes
{
    public class SQLiteTaxMasterDL : Base.DL.DBManager.SqliteDBManager, Interfaces.ITaxMasterDL
    {
        public SQLiteTaxMasterDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }


        public async Task<PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITax>> GetTaxDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                    id AS Id,
                    uid AS Uid,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    company_uid AS CompanyUid,
                    name AS Name,
                    tax_id_name AS TaxIdName,
                    applicable_at AS ApplicableAt,
                    transaction_type AS TransactionType,
                    transaction_sub_type AS TransactionSubType,
                    dependent_tax_uid AS DependentTaxUid,
                    tax_calculation_type AS TaxCalculationType,
                    base_tax_rate AS BaseTaxRate,
                    status AS Status,
                    valid_from AS ValidFrom,
                    valid_upto AS ValidUpto
                FROM 
                    tax WHERE uid = @UID ");
                var sqlCount = new StringBuilder();
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
                                            company_uid AS CompanyUid,
                                            name AS Name,
                                            tax_id_name AS TaxIdName,
                                            applicable_at AS ApplicableAt,
                                            transaction_type AS TransactionType,
                                            transaction_sub_type AS TransactionSubType,
                                            dependent_tax_uid AS DependentTaxUid,
                                            tax_calculation_type AS TaxCalculationType,
                                            base_tax_rate AS BaseTaxRate,
                                            status AS Status,
                                            valid_from AS ValidFrom,
                                            valid_upto AS ValidUpto
                                        FROM 
                                            tax) As SubQuery");
                }
                var parameters = new Dictionary<string, object>();
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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ITax>().GetType();

                IEnumerable<Winit.Modules.Tax.Model.Interfaces.ITax> TaxDetails = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITax>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITax> pagedResponse = new PagedResponse<Winit.Modules.Tax.Model.Interfaces.ITax>
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
        public async Task<Winit.Modules.Tax.Model.Interfaces.ITax> GetTaxByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            company_uid AS CompanyUid,
                            name AS Name,
                            tax_id_name AS TaxIdName,
                            applicable_at AS ApplicableAt,
                            transaction_type AS TransactionType,
                            transaction_sub_type AS TransactionSubType,
                            dependent_tax_uid AS DependentTaxUid,
                            tax_calculation_type AS TaxCalculationType,
                            base_tax_rate AS BaseTaxRate,
                            status AS Status,
                            valid_from AS ValidFrom,
                            valid_upto AS ValidUpto
                        FROM 
                            tax 
                    WHERE UID = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ITax>().GetType();

            Winit.Modules.Tax.Model.Interfaces.ITax taxDetails = await ExecuteSingleAsync<Winit.Modules.Tax.Model.Interfaces.ITax>(sql, parameters, type);
            return taxDetails;
        }
        public async Task<int> CreateTax(Winit.Modules.Tax.Model.Interfaces.ITax tax)
        {
            try
            {
                
                var sql = @"INSERT INTO Tax ( uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, name, 
                        legal_name, applicable_at, tax_calculation_type, base_tax_rate, status, valid_from, valid_upto, Code, ius_tax_on_tax_applicable) 
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                        @Name, @LegalName, @ApplicableAt, @TaxCalculationType, @BaseTaxRate, @Status, @ValidFrom, 
                        @ValidUpto, @Code,@IsTaxOnTaxApplicable )";
                Dictionary<string, object> parameters = new Dictionary<string, object>
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
                var sql = @"UPDATE tax SET 
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
                WHERE 
                    uid = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
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
        public async Task<int> DeleteTax(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = "DELETE  FROM tax WHERE uid = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<(List<Winit.Modules.Tax.Model.Interfaces.ITax>, List<Winit.Modules.Tax.Model.Interfaces.ITaxSlab>)> GetTaxMaster(List<string> OrgUIDs)
        {
            try
            {
                string commaSeparatedorgUIDs = String.Empty;
                if (OrgUIDs != null)
                {
                    commaSeparatedorgUIDs = string.Join(",", OrgUIDs);

                }

                Dictionary<string, object> Taxparameters = new Dictionary<string, object>
                {
                    {"OrgUIDs", commaSeparatedorgUIDs}
                };
                var taxSql = new StringBuilder(@"SELECT 
                                        T.id AS Id, 
                                        T.uid AS UID, 
                                        T.created_by AS CreatedBy, 
                                        T.created_time AS CreatedTime, 
                                        T.modified_by AS ModifiedBy, 
                                        T.modified_time AS ModifiedTime, 
                                        T.server_add_time AS ServerAddTime, 
                                        T.server_modified_time AS ServerModifiedTime, 
                                        T.name AS Name, 
                                        T.legal_name AS LegalName, 
                                        T.applicable_at AS ApplicableAt, 
                                        T.tax_calculation_type AS TaxCalculationType, 
                                        T.base_tax_rate AS BaseTaxRate, 
                                        T.status AS Status, 
                                        T.valid_from AS ValidFrom, 
                                        T.valid_upto AS ValidUpto, 
                                        T.code AS Code, 
                                        T.is_tax_on_tax_applicable AS IsTaxOnTaxApplicable,
                                        IFNULL(td.dependent_taxes, '') AS DependentTaxes
                                    FROM 
                                        org O 
                                    INNER JOIN 
                                        tax_group_taxes TGT ON TGT.tax_group_uid = O.tax_group_uid 
                                    INNER JOIN 
                                        tax T ON T.uid = TGT.tax_uid AND T.status = 'Active' 
                                    LEFT JOIN (
                                        SELECT
                                            tax_uid,
                                            group_concat(depends_on_tax_uid, ',') AS dependent_taxes
                                        FROM
                                            tax_dependencies
                                        GROUP BY
                                            tax_uid
                                    ) td ON t.uid = td.tax_uid
                                    WHERE 
                                        1=1");
                if (!string.IsNullOrEmpty(commaSeparatedorgUIDs))
                {
                    taxSql.Append($" AND O.UID IN ('{commaSeparatedorgUIDs}');");
                }

                Type taxType = _serviceProvider.GetRequiredService<Winit.Modules.Tax.Model.Interfaces.ITax>().GetType();
               // Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster = _serviceProvider.CreateInstance<Winit.Modules.Tax.Model.Interfaces.ITaxMaster>();

                List<Winit.Modules.Tax.Model.Interfaces.ITax> taxList = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITax>(taxSql.ToString(), Taxparameters, taxType);

                var taxsalbSql = new StringBuilder(@"SELECT 
                                    T.uid AS TaxUID,
                                    TS.uid AS TaxSlabUID,
                                    TS.range_start AS RangeStart,
                                    TS.range_end AS RangeEnd,
                                    TS.tax_rate As TaxRate
                                FROM 
                                    org O 
                                INNER JOIN 
                                    tax_group_taxes TGT ON TGT.tax_group_uid = O.tax_group_uid 
                                INNER JOIN 
                                    tax T ON T.uid = TGT.tax_uid AND T.status = 'Active' 
                                INNER JOIN 
                                    tax_slab TS ON TS.tax_uid = T.uid AND TS.status = 'Active' 
                                WHERE 
                                    1=1");

                if (!string.IsNullOrEmpty(commaSeparatedorgUIDs))
                {
                    taxsalbSql.Append($" AND O.UID IN ('{commaSeparatedorgUIDs}');");
                }

                var taxslabViewParameters = new Dictionary<string, object>();
                Type taxsalbViewType = _serviceProvider.GetRequiredService<Winit.Modules.Tax.Model.Interfaces.ITaxSlab>().GetType();
                List<Winit.Modules.Tax.Model.Interfaces.ITaxSlab> taxsalbViewList = await ExecuteQueryAsync<Winit.Modules.Tax.Model.Interfaces.ITaxSlab>(taxsalbSql.ToString(), Taxparameters, taxsalbViewType);

             
              

                return (taxList, taxsalbViewList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> CreateTaxMaster(TaxMasterDTO taxMasterDTO)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateTaxMaster(TaxMasterDTO taxMasterDTO)
        {
            throw new NotImplementedException();
        }

        public Task<(List<ITax>, List<ITaxSkuMap>)> SelectTaxMasterViewByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<ITaxGroup>> GetTaxGroupDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

        public Task<ITaxGroup> GetTaxGroupByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateTaxGroupMaster(TaxGroupMasterDTO taxGroupMasterDTO)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateTaxGroupMaster(TaxGroupMasterDTO taxGroupMasterDTO)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ITaxSelectionItem>> GetTaxSelectionItems(string taxGroupUID)
        {
            throw new NotImplementedException();
        }
    }
}










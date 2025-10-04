using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Currency.DL.Interfaces;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Currency.DL.Classes
{
    public class MSSQLCurrencyDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ICurrencyDL
    {
        public MSSQLCurrencyDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency>> GetCurrencyDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(SELECT 
                                            id AS Id,
                                            uid AS UID,
                                            name AS Name,
                                            symbol AS Symbol,
                                            digits AS Digits,
                                            code AS Code,
                                            fraction_name AS FractionName,
                                            ss AS SS,
                                            created_time AS CreatedTime,
                                            modified_time AS ModifiedTime,
                                            server_add_time AS ServerAddTime,
                                            server_modified_time AS ServerModifiedTime
                                        FROM 
                                            currency)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                            (SELECT 
                                            id AS Id,
                                            uid AS UID,
                                            name AS Name,
                                            symbol AS Symbol,
                                            digits AS Digits,
                                            code AS Code,
                                            fraction_name AS FractionName,
                                            ss AS SS,
                                            created_time AS CreatedTime,
                                            modified_time AS ModifiedTime,
                                            server_add_time AS ServerAddTime,
                                            server_modified_time AS ServerModifiedTime
                                        FROM 
                                            currency)as subquery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Currency.Model.Interfaces.ICurrency>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.Currency.Model.Interfaces.ICurrency> currencyDetails = await ExecuteQueryAsync<Winit.Modules.Currency.Model.Interfaces.ICurrency>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency> pagedResponse = new PagedResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency>
                {
                    PagedData = currencyDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Currency.Model.Interfaces.ICurrency> GetCurrencyById(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT 
                                    id AS Id,
                                    uid AS UID,
                                    name AS Name,
                                    symbol AS Symbol,
                                    digits AS Digits,
                                    code AS Code,
                                    fraction_name AS FractionName,
                                    ss AS SS,
                                    created_time AS CreatedTime,
                                    modified_time AS ModifiedTime,
                                    server_add_time AS ServerAddTime,
                                    server_modified_time AS ServerModifiedTime
                                FROM 
                                   currency WHERE uid = @UID";
            Winit.Modules.Currency.Model.Interfaces.ICurrency? currency = await ExecuteSingleAsync<Winit.Modules.Currency.Model.Interfaces.ICurrency>(sql, parameters);
            return currency;
        }
        public async Task<List<Winit.Modules.Currency.Model.Interfaces.ICurrency>> GetCurrencyListByOrgUID(string OrgUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"OrgUID",  OrgUID}
            };

            var sql = @"SELECT 
                        C.uid AS UID,
                        C.name AS Name,
                        C.symbol AS Symbol,
                        C.digits AS Digits,
                        C.code AS Code,
                        C.fraction_name AS FractionName,
                        OC.is_primary AS IsPrimary 
                    FROM 
                        org_currency OC
                    INNER JOIN 
                        currency C ON C.uid = OC.currency_uid AND OC.org_uid = @OrgUID";
            List<Winit.Modules.Currency.Model.Interfaces.ICurrency> currencyDetails = await ExecuteQueryAsync<Winit.Modules.Currency.Model.Interfaces.ICurrency>(sql, parameters);
            return currencyDetails;
        }
        public async Task<int> CreateCurrency(Winit.Modules.Currency.Model.Interfaces.ICurrency createCurrency)
        {
            try
            {
                var sql = @"INSERT INTO currency (uid, name, symbol, digits, code, fraction_name, ss, created_time, modified_time, 
                            server_add_time, server_modified_time) VALUES (@UID, @Name, @Symbol, @Digits, @Code, @FractionName,
                            @SS, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

                return await ExecuteNonQueryAsync(sql, createCurrency);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateCurrency(Winit.Modules.Currency.Model.Interfaces.ICurrency updateCurrency)
        {
            try
            {
                var sql = @"UPDATE currency SET name = @Name, symbol = @Symbol, digits = @Digits, code = @Code, 
                            fraction_name = @FractionName, ss = @SS,modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime  WHERE uid = @UID;";

                return await ExecuteNonQueryAsync(sql, updateCurrency);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteCurrency(string UID)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM currency WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);

        }
        public async Task<int> DeleteOrgCurrency(string UID)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM org_currency WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);

        }
        Task<ICurrency> ICurrencyDL.GetCurrencyListByOrgUID(string OrgUID)
        {
            throw new NotImplementedException();
        }
        public async Task<int> CreateOrgCurrency(IOrgCurrency orgCurrency)
        {
            try
            {
                var sql = @"INSERT INTO org_currency( uid, org_uid, currency_uid, is_primary, ss, created_time, modified_time, server_add_time, 
                            server_modified_time, created_by, modified_by) 
                            VALUES 
                            (@UID, @OrgUID, @CurrencyUID, @IsPrimary,@SS, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                            @CreatedBy,@ModifiedBy);";
                return await ExecuteNonQueryAsync(sql, orgCurrency);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateOrgCurrency(IOrgCurrency orgCurrency)
        {
            try
            {
                var sql =   @"UPDATE org_currency
                              SET org_uid = @OrgUID,
                                  currency_uid = @CurrencyUID,
                                  is_primary = @IsPrimary,
                                  ss = @SS,
                                  created_time = @CreatedTime,
                                  modified_time = @ModifiedTime,
                                  server_add_time = @ServerAddTime,
                                  server_modified_time = @ServerModifiedTime,
                                  created_by = @CreatedBy,
                                  modified_by = @ModifiedBy
                               WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, orgCurrency);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<IOrgCurrency>> GetOrgCurrencyListByOrgUID(string orgUID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"OrgUID",  orgUID}
            };

                var sql = @"SELECT 
                                id AS Id, 
                                uid AS Uid, 
                                org_uid AS OrgUid, 
                                currency_uid AS CurrencyUid, 
                                is_primary AS IsPrimary, 
                                ss AS Ss, 
                                created_time AS CreatedTime, 
                                modified_time AS ModifiedTime, 
                                server_add_time AS ServerAddTime, 
                                server_modified_time AS ServerModifiedTime, 
                                created_by AS CreatedBy, 
                                modified_by AS ModifiedBy
                            FROM 
                                org_currency WHERE
                                org_uid = @OrgUID";

                List<IOrgCurrency> currencyList = await ExecuteQueryAsync<IOrgCurrency>(sql, parameters);
                return currencyList;
            }
            catch
            {
                throw;
            }
           
        }
        public async Task<IEnumerable<IOrgCurrency>> GetOrgCurrencyListBySelectedOrg(string orgUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                 {"OrgUID",  orgUID}
            };

            var sql = @"SELECT C.UID As UID,  C.Symbol AS Symbol,
                    C.Digits AS Digits,
                    C.fraction_name AS FractionName,
                    OC.org_uid AS OrgUID,
                    OC.currency_uid AS CurrencyUID,
                    OC.is_primary AS IsPrimary,
                    C.round_off_min_limit AS RoundOffMinLimit,
                    C.round_off_max_limit AS RoundOffMaxLimit
                FROM
                    org_currency OC
                INNER JOIN
                    currency C ON C.UID = OC.currency_uid
                WHERE
                    OC.org_uid = @OrgUID";

            IEnumerable<IOrgCurrency> currencyList = await ExecuteQueryAsync<IOrgCurrency>(sql, parameters);
            return currencyList;
        }
    }
}

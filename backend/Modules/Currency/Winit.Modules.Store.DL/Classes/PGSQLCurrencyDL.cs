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
    public class PGSQLCurrencyDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ICurrencyDL
    {
        public PGSQLCurrencyDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency>> GetCurrencyDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
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
                                            public.currency");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM currency");
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
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICurrency>().GetType();
                IEnumerable<Winit.Modules.Currency.Model.Interfaces.ICurrency> currencyDetails = await ExecuteQueryAsync<Winit.Modules.Currency.Model.Interfaces.ICurrency>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
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
                                    public.currency WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICurrency>().GetType();
            Winit.Modules.Currency.Model.Interfaces.ICurrency currencyDetails = await ExecuteSingleAsync<Winit.Modules.Currency.Model.Interfaces.ICurrency>(sql, parameters, type);
            return currencyDetails;
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
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICurrency>().GetType();
            List<Winit.Modules.Currency.Model.Interfaces.ICurrency> currencyDetails = await ExecuteQueryAsync<Winit.Modules.Currency.Model.Interfaces.ICurrency>(sql, parameters, type);
            return currencyDetails;
        }
        public async Task<int> CreateCurrency(Winit.Modules.Currency.Model.Interfaces.ICurrency createCurrency)
        {
            try
            {
                var sql = @"INSERT INTO currency (uid, name, symbol, digits, code, fraction_name, ss, created_time, modified_time, 
                            server_add_time, server_modified_time) VALUES (@UID, @Name, @Symbol, @Digits, @Code, @FractionName,
                            @SS, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID", createCurrency.UID},
                   {"Name", createCurrency.Name},
                   {"Symbol", createCurrency.Symbol},
                   {"Digits", createCurrency.Digits},
                   {"Code", createCurrency.Code},
                   {"FractionName", createCurrency.FractionName},
                   {"SS", createCurrency.SS},
                   {"CreatedTime", createCurrency.CreatedTime},
                   {"ModifiedTime", createCurrency.ModifiedTime},
                   {"ServerAddTime", createCurrency.ServerAddTime},
                   {"ServerModifiedTime", createCurrency.ServerModifiedTime},
             };
                return await ExecuteNonQueryAsync(sql, parameters);

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
                            fraction_name = @FractionName, ss = @SS,modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime 
                            WHERE uid = @UID;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                  {"Name", updateCurrency.Name},
                   {"Symbol", updateCurrency.Symbol},
                   {"Digits", updateCurrency.Digits},
                   {"Code", updateCurrency.Code},
                   {"FractionName", updateCurrency.FractionName},
                   {"SS", updateCurrency.SS},
                   {"ModifiedTime", updateCurrency.ModifiedTime},
                   {"ServerModifiedTime", updateCurrency.ServerModifiedTime},
                   {"UID", updateCurrency.UID},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
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
        public async Task<int> CreateOrgCurrency(IOrgCurrency createOrgCurrency)
        {
            try
            {
                var sql = @"INSERT INTO public.org_currency( uid, org_uid, currency_uid, is_primary, ss, created_time, modified_time, server_add_time, 
                                server_modified_time, created_by, modified_by) 
                            VALUES (@UID, @OrgUID, @CurrencyUID, @IsPrimary,
                            @SS, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,@CreatedBy,@ModifiedBy);";

                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID", createOrgCurrency.UID},
                   {"OrgUID", createOrgCurrency.OrgUID},
                   {"CurrencyUID" , createOrgCurrency.CurrencyUID},
                   {"IsPrimary", createOrgCurrency.IsPrimary},
                   {"Code", createOrgCurrency.Code},
                   {"FractionName", createOrgCurrency.FractionName},
                   {"SS", createOrgCurrency.SS},
                   {"CreatedTime", createOrgCurrency.CreatedTime},
                   {"ModifiedTime", createOrgCurrency.ModifiedTime},
                   {"ServerAddTime", createOrgCurrency.ServerAddTime},
                   {"ServerModifiedTime", createOrgCurrency.ServerModifiedTime},
                   {"CreatedBy", createOrgCurrency.CreatedBy},
                   {"ModifiedBy", createOrgCurrency.ModifiedBy},
             };
                return await ExecuteNonQueryAsync(sql, parameters);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateOrgCurrency(IOrgCurrency createOrgCurrency)
        {
            try
            {
                var sql = @"UPDATE public.org_currency
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

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID", createOrgCurrency.UID},
                    {"OrgUID", createOrgCurrency.OrgUID},
                    {"CurrencyUID", createOrgCurrency.CurrencyUID},
                    {"IsPrimary", createOrgCurrency.IsPrimary},
                    {"Code", createOrgCurrency.Code},
                    {"FractionName", createOrgCurrency.FractionName},
                    {"SS", createOrgCurrency.SS},
                    {"CreatedTime", createOrgCurrency.CreatedTime},
                    {"ModifiedTime", createOrgCurrency.ModifiedTime},
                    {"ServerAddTime", createOrgCurrency.ServerAddTime},
                    {"ServerModifiedTime", createOrgCurrency.ServerModifiedTime},
                    {"CreatedBy", createOrgCurrency.CreatedBy},
                    {"ModifiedBy", createOrgCurrency.ModifiedBy}
                };

                return await ExecuteNonQueryAsync(sql, parameters);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<IOrgCurrency>> GetOrgCurrencyListByOrgUID(string orgUID)
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
                                public.org_currency WHERE
                                org_uid = @OrgUID";

            List<IOrgCurrency> currencyList = await ExecuteQueryAsync<IOrgCurrency>(sql, parameters);
            return currencyList;
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

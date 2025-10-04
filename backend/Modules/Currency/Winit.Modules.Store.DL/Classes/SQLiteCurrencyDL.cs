using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
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
    public class SQLiteCurrencyDL:Winit.Modules.Base.DL.DBManager.SqliteDBManager,ICurrencyDL
    {
        public SQLiteCurrencyDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Currency.Model.Interfaces.ICurrency>> GetCurrencyDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                    id AS Id,
                    uid AS Uid,
                    name AS Name,
                    symbol AS Symbol,
                    digits AS Digits,
                    code AS Code,
                    fraction_name AS FractionName,
                    ss AS Ss,
                    created_time AS CreatedTime,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime
                FROM 
                    currency) As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                    id AS Id,
                    uid AS Uid,
                    name AS Name,
                    symbol AS Symbol,
                    digits AS Digits,
                    code AS Code,
                    fraction_name AS FractionName,
                    ss AS Ss,
                    created_time AS CreatedTime,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime
                FROM 
                    currency) As SubQuery");
                }
                var parameters = new Dictionary<string, object?>();
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
                //if (pageNumber > 0 && pageSize > 0)
                //{
                //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}
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
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID",  UID}
            };

            var sql = @"SELECT 
                    id AS Id,
                    uid AS Uid,
                    name AS Name,
                    symbol AS Symbol,
                    digits AS Digits,
                    code AS Code,
                    fraction_name AS FractionName,
                    ss AS Ss,
                    created_time AS CreatedTime,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime
                FROM 
                    currency WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ICurrency>().GetType();
            Winit.Modules.Currency.Model.Interfaces.ICurrency currencyDetails = await ExecuteSingleAsync<Winit.Modules.Currency.Model.Interfaces.ICurrency>(sql, parameters, type);
            return currencyDetails;
        }
        public async Task<int> CreateCurrency(Winit.Modules.Currency.Model.Interfaces.ICurrency createCurrency)
        {
            try
            {
                var sql = @"INSERT INTO Currency (uid, name, symbol, digits, code, fraction_name, ss, created_time, modified_time, server_add_time, server_modified_time)
                              VALUES (@UID, @Name, @Symbol,@Digits ,@Code,@FractionName,@SS,@CreatedTime,@ModifiedTime,
                    @ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
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
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> UpdateCurrency(Winit.Modules.Currency.Model.Interfaces.ICurrency updateCurrency)
        {
            try
            {
                var sql = @"UPDATE currency SET 
                    name = @Name, 
                    symbol = @Symbol, 
                    digits = @Digits, 
                    code = @Code,
                    fraction_name = @FractionName,
                    ss = @SS,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime
                WHERE
                    uid = @UId";
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
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
                return  await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw ;
            }
        }
        public async Task<int> DeleteCurrency(string UID)
        {

            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID" , UID}
            };
            var sql = "DELETE  FROM currency WHERE uid = @UID";
            return  await ExecuteNonQueryAsync(sql, parameters);

        }
        public async Task<List<IOrgCurrency>> GetOrgCurrencyListByOrgUID(string orgUID)
        {
            throw new NotImplementedException();
        }
        Task<ICurrency> ICurrencyDL.GetCurrencyListByOrgUID(string OrgUID)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<IOrgCurrency>> GetOrgCurrencyListBySelectedOrg(string orgUID)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                 {"OrgUID",  orgUID}
            };

            var sql = @"SELECT C.UID As UID,  C.Symbol AS Symbol,
                    C.Digits AS Digits,
                    C.fraction_name AS FractionName,
                    OC.org_uid AS OrgUID,
                    OC.is_primary AS IsPrimary,
                    C.round_off_min_limit AS RoundOffMinLimit,
                    C.round_off_max_limit AS RoundOffMaxLimit,
                    C.UID as CurrencyUID
                FROM
                    org_currency OC
                INNER JOIN
                    currency C ON C.UID = OC.currency_uid
                WHERE
                    OC.org_uid = @OrgUID";

            IEnumerable<IOrgCurrency> currencyList = await ExecuteQueryAsync<IOrgCurrency>(sql, parameters);
            return currencyList;
        }
        public async Task<int> CreateOrgCurrency(Winit.Modules.Currency.Model.Interfaces.IOrgCurrency createOrgCurrency)
        {
            throw new NotImplementedException();

        }
        public async Task<int> UpdateOrgCurrency(Winit.Modules.Currency.Model.Interfaces.IOrgCurrency createOrgCurrency)
        {
            throw new NotImplementedException();
        }
        public async Task<int> DeleteOrgCurrency(string UID)
        {
            throw new NotImplementedException();
        }
    }
}

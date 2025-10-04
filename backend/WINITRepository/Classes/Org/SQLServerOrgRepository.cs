using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace WINITRepository.Classes.Org
{
    public class SQLServerOrgRepository : Interfaces.Org.IOrgRepository
    {
        private readonly string _connectionString;
        public SQLServerOrgRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SqlServer");
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.Org>> GetOrgDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Org> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Org>(_connectionString);

                var sql = new StringBuilder(@"SELECT org_id,CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,org_code,org_name,is_active FROM ORG ");
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    sql.Append(" WHERE ");
                    dbManager.AppendFilterCriteria(filterCriterias, sql, parameters);
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    dbManager.AppendSortCriteria(sortCriterias, sql);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                IEnumerable<WINITSharedObjects.Models.Org> orgDetails = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return orgDetails;
            }
            catch (Exception ex)
            {
                throw;
            }


        }


        public async Task<WINITSharedObjects.Models.Org> GetOrgByOrgCode(string orgCode)   
            {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Org> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Org>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"orgCode",  orgCode}
            };

            var sql = @"SELECT * FROM Org WHERE org_code = @orgCode";

            WINITSharedObjects.Models.Org orgDetails = await dbManager.ExecuteSingleAsync(sql, parameters);
            return orgDetails;
        }


        public async Task<WINITSharedObjects.Models.Org> CreateOrg(WINITSharedObjects.Models.Org createOrg)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Org> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Org>(_connectionString);
            try
            {
                var sql = "INSERT INTO Org ([CreatedBy], [ModifiedBy],  [org_code],[org_name],[is_active],[CreatedTime],[ModifiedTime],[ServerAddTime],[ServerModifiedTime])" +
                              " VALUES (@CreatedBy,  @ModifiedBy,@org_code ,@org_name,@is_active,@CreatedTime,@ModifiedTime,@ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"CreatedBy", createOrg.CreatedBy},
                   {"ModifiedBy", createOrg.ModifiedBy},
                   {"org_code", createOrg.org_code},
                   {"org_name", createOrg.org_name},
                   {"is_active", createOrg.is_active},
                   {"CreatedTime", createOrg.CreatedTime},
                   {"ModifiedTime", createOrg.ModifiedTime},
                   {"ServerAddTime", createOrg.ServerAddTime},
                   {"ServerModifiedTime", createOrg.ServerModifiedTime},

             };
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return createOrg;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<int> UpdateOrg(WINITSharedObjects.Models.Org updateOrg)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Org> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Org>(_connectionString);

                var sql = "UPDATE Org SET [ModifiedBy] = @ModifiedBy,  [org_name] = @org_name," +
                    " [is_active] = @is_active,[ModifiedTime] = @ModifiedTime,[ServerModifiedTime] = @ServerModifiedTime WHERE [org_code] = @org_code;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                  {"org_code", updateOrg.org_code},
                  {"ModifiedBy", updateOrg.ModifiedBy},
                   {"org_name", updateOrg.org_name},
                   {"is_active", updateOrg.is_active},
                   {"ModifiedTime", updateOrg.ModifiedTime},
                   {"ServerModifiedTime", updateOrg.ServerModifiedTime},
                 };
                var updateDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> DeleteOrg(string orgCode)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.OrgCurrency>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"orgCode" , orgCode}
            };
            var sql = "DELETE  FROM Org WHERE org_code = @orgCode";

            var Details = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return Details;
        }
    }
}

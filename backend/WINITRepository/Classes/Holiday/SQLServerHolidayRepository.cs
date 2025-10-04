using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WINITRepository.Classes.DBManager;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace WINITRepository.Classes.Holiday
{
    public class SQLServerHolidayRepository : Interfaces.Holiday.IHolidayRepository
    {
        private readonly string _connectionString;
        public SQLServerHolidayRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SqlServer");
        }


//        public async Task<DataSet> HolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
//int pageSize, List<FilterCriteria> filterCriterias)
          public async Task<IEnumerable<WINITSharedObjects.Models.HolidayDetails>> HolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias)

        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayDetails> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayDetails>(_connectionString);

                var sql = new StringBuilder(@"SELECT Id, UID, CompanyUID, OrgUID, Name, Description, LocationUID, IsActive, 
                Year, SS, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, ServerModifiedTime FROM HolidayList");
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

                IEnumerable<WINITSharedObjects.Models.HolidayDetails> HolidayList = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);
               // DataSet ds = await dbManager.ExecuteQueryDataSetAsync(sql.ToString(), parameters);

                return HolidayList;
            }
            catch (Exception ex)
            {
                throw;
            }


        }
        public async Task<IEnumerable<WINITSharedObjects.Models.Holiday>> GetHolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday>(_connectionString);

                var sql = new StringBuilder(@"SELECT Id,[UID],HolidayListUID,HolidayDate,TYPE,NAME,IsOptional,YEAR,SS,
CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime FROM Holiday");
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

                IEnumerable<WINITSharedObjects.Models.Holiday> HolidayList = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return HolidayList;
            }
            catch (Exception ex)
            {
                throw;
            }


        }

        public async Task<IEnumerable<WINITSharedObjects.Models.HolidayListRole>> GetHolidayListRoleDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole>(_connectionString);

                var sql = new StringBuilder("SELECT * FROM HolidayListRole");
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

                IEnumerable<WINITSharedObjects.Models.HolidayListRole> HolidayListRole = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);

                return HolidayListRole;
            }
            catch (Exception ex)
            {
                throw;
            }


        }

        public async Task<WINITSharedObjects.Models.Holiday> GetHolidayByOrgUID(string holidayListUID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"holidayListUID",  holidayListUID}
            };

            var sql = @"SELECT * FROM Holiday WHERE holidayListUID = @holidayListUID";

            WINITSharedObjects.Models.Holiday HolidayDetails = await dbManager.ExecuteSingleAsync(sql, parameters);
            return HolidayDetails;
        }


        public async Task<WINITSharedObjects.Models.Holiday> CreateHoliday(WINITSharedObjects.Models.Holiday createHoliday)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday>(_connectionString);
            try
            {
                var sql = "INSERT INTO HOLIDAY ([UID], [HolidayListUID],  [HolidayDate],[Type],[IsOptional],[Year],[CreatedBy],[CreatedTime]," +
                    "[ModifiedBy],[ModifiedTime],[ServerAddTime],[ServerModifiedTime])" +
                              " VALUES (@UID,  @HolidayListUID,@HolidayDate ,@Type,@IsOptional,@Year,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime," +
                              "@ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"UID", createHoliday.UID},
                   {"HolidayListUID", createHoliday.HolidayListUID},
                   {"HolidayDate", createHoliday.HolidayDate},
                   {"Type", createHoliday.Type},
                   {"Name", createHoliday.Name},
                   {"IsOptional", createHoliday.IsOptional},
                   {"Year", createHoliday.Year},
                   {"SS", createHoliday.SS},
                   {"CreatedBy", createHoliday.CreatedBy},
                   {"CreatedTime", createHoliday.CreatedTime},
                   {"ModifiedBy", createHoliday.ModifiedBy},
                   {"ModifiedTime", createHoliday.ModifiedTime},
                   {"ServerAddTime", createHoliday.ServerAddTime},
                   {"ServerModifiedTime", createHoliday.ServerModifiedTime},

             };
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return createHoliday;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<int> UpdateHoliday(WINITSharedObjects.Models.Holiday updateHoliday)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday>(_connectionString);

                var sql = "UPDATE Holiday SET   [HolidayDate] = @HolidayDate," +
                    " [Type] = @Type,[Name] = @Name,[IsOptional] = @IsOptional,[Year] = @Year,[SS] = @SS,[ModifiedBy] = @ModifiedBy," +
                    "[ModifiedTime]=@ModifiedTime,[ServerModifiedTime]=@ServerModifiedTime WHERE [HolidayListUID] = @HolidayListUID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                //  {"UID", updateHoliday.UID},
                   {"HolidayDate", updateHoliday.HolidayDate},
                   {"Type", updateHoliday.Type},
                   {"Name", updateHoliday.Name},
                   {"IsOptional", updateHoliday.IsOptional},
                   {"Year", updateHoliday.Year},
                   {"SS", updateHoliday.SS},
                   {"ModifiedBy", updateHoliday.ModifiedBy},
                   {"ModifiedTime", updateHoliday.ModifiedTime},
                   {"ServerModifiedTime", updateHoliday.ServerModifiedTime},
                   {"HolidayListUID", updateHoliday.HolidayListUID},
                 };
                var updateDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> DeleteHoliday(string holidayListUID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"holidayListUID" , holidayListUID}
            };
            var sql = "DELETE  FROM Holiday WHERE holidayListUID = @holidayListUID";

            var Details = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return Details;
        }
        //HOLIDAYLISTROLE


        public async Task<WINITSharedObjects.Models.HolidayListRole> GetHolidayListRoleByHolidayListUID(string holidayListUID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"holidayListUID",  holidayListUID}
            };

            var sql = @"SELECT * FROM HolidayListRole WHERE holidayListUID = @holidayListUID";

            WINITSharedObjects.Models.HolidayListRole HolidayListRoleDetails = await dbManager.ExecuteSingleAsync(sql, parameters);
            return HolidayListRoleDetails;
        }


        public async Task<WINITSharedObjects.Models.HolidayListRole> CreateHolidayListRole(WINITSharedObjects.Models.HolidayListRole CreateHolidayListRole)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole>(_connectionString);
            try
            {
                var sql = "INSERT INTO HolidayListRole ([UID], [HolidayListUID], [UserRoleUID],[SS],[CreatedTime]," +
                    "[ModifiedTime],[ServerAddTime],[ServerModifiedTime]) VALUES (@UID,  @HolidayListUID,@UserRoleUID,@SS,@CreatedTime,@ModifiedTime," +
                              "@ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"UID", CreateHolidayListRole.UID},
                   {"HolidayListUID", CreateHolidayListRole.HolidayListUID},
                   {"UserRoleUID", CreateHolidayListRole.UserRoleUID},
                   {"SS", CreateHolidayListRole.SS},
                   {"CreatedTime", CreateHolidayListRole.CreatedTime},
                   {"ModifiedTime", CreateHolidayListRole.ModifiedTime},
                   {"ServerAddTime", CreateHolidayListRole.ServerAddTime},
                   {"ServerModifiedTime", CreateHolidayListRole.ServerModifiedTime},

             };
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return CreateHolidayListRole;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<int> UpdateHolidayListRole(WINITSharedObjects.Models.HolidayListRole updateHolidayListRole)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole>(_connectionString);

                var sql = "UPDATE HolidayListRole SET  [UserRoleUID] = @UserRoleUID,[SS] = @SS,[ModifiedTime] = @ModifiedTime,[ServerModifiedTime]=@ServerModifiedTime WHERE [HolidayListUID] = @HolidayListUID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   // {"UID", updateHolidayListRole.UID},
                     {"HolidayListUID", updateHolidayListRole.HolidayListUID},
                   {"UserRoleUID", updateHolidayListRole.UserRoleUID},
                   {"SS", updateHolidayListRole.SS},
                   {"ModifiedTime", updateHolidayListRole.ModifiedTime},
                   {"ServerModifiedTime", updateHolidayListRole.ServerModifiedTime},
                  
                 };
                var updateDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> DeleteHolidayListRole(string holidayListUID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayListRole>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"holidayListUID" , holidayListUID}
            };
            var sql = "DELETE  FROM HolidayListRole WHERE holidayListUID = @holidayListUID";

            var Details = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return Details;
        }






        //HOLIDAYLIST

        public async Task<WINITSharedObjects.Models.HolidayList> GetHolidayListByHolidayListUID(string uID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayList> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayList>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"uID",  uID}
            };

            var sql = @"SELECT * FROM HolidayList WHERE uID = @uID";

            WINITSharedObjects.Models.HolidayList HolidayListDetails = await dbManager.ExecuteSingleAsync(sql, parameters);
            return HolidayListDetails;
        }


        public async Task<WINITSharedObjects.Models.HolidayList> CreateHolidayList(WINITSharedObjects.Models.HolidayList createHolidayList)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayList> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayList>(_connectionString);
            try
            {
                var sql = "INSERT INTO HolidayList ([UID], [CompanyUID], [OrgUID],[Name]," +
                    "[Description],[LocationUID],[IsActive],[Year],[SS],[CreatedBy],[CreatedTime],[ModifiedBy],[ModifiedTime],[ServerAddTime],[ServerModifiedTime]) " +
                    "VALUES (@UID,  @CompanyUID,@OrgUID,@Name,@Description,@LocationUID,@IsActive,@Year,@SS,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime," +
                              "@ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"UID", createHolidayList ?.UID},
                   {"CompanyUID", createHolidayList ?.CompanyUID},
                   {"OrgUID", createHolidayList ?.OrgUID},
                   {"Name", createHolidayList ?.Name},
                   {"Description", createHolidayList ?.Description},
                   {"LocationUID", createHolidayList ?.LocationUID},
                   {"IsActive", createHolidayList ?.IsActive},
                   {"Year", createHolidayList ?.Year},
                   {"SS", createHolidayList ?.SS},
                   {"CreatedBy", createHolidayList ?.CreatedBy},
                   {"CreatedTime", createHolidayList ?.CreatedTime},
                   {"ModifiedBy", createHolidayList ?.ModifiedBy},
                   {"ModifiedTime", createHolidayList ?.ModifiedTime},
                   {"ServerAddTime", createHolidayList ?.ServerAddTime},
                   {"ServerModifiedTime", createHolidayList ?.ServerModifiedTime},

             };
                await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return createHolidayList;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        public async Task<int> UpdateHolidayList(WINITSharedObjects.Models.HolidayList updateHolidayList)
        {
            try
            {
                DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayList> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayList>(_connectionString);

                var sql = "UPDATE HolidayList SET  [CompanyUID] = @CompanyUID,[Name] = @Name,[Description] = @Description,[LocationUID]=@LocationUID,[IsActive]=@IsActive,[Year]=@Year,[SS]=@SS," +
                    "[ModifiedBy]=@ModifiedBy,[ModifiedTime]=@ModifiedTime,[ServerModifiedTime]=@ServerModifiedTime WHERE [UID] = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", updateHolidayList ?.UID},
                   {"CompanyUID", updateHolidayList ?.CompanyUID},
                  // {"OrgUID", updateHolidayList ?.OrgUID},
                   {"Name", updateHolidayList ?.Name},
                   {"Description", updateHolidayList ?.Description},
                   {"LocationUID", updateHolidayList ?.LocationUID},
                   {"IsActive", updateHolidayList ?.IsActive},
                   {"Year", updateHolidayList ?.Year},
                   {"SS", updateHolidayList ?.SS},
                   {"ModifiedBy", updateHolidayList ?.ModifiedBy},
                   {"ModifiedTime", updateHolidayList ?.ModifiedTime},
                   {"ServerModifiedTime", updateHolidayList ?.ServerModifiedTime},

                 };
                var updateDetails = await dbManager.ExecuteNonQueryAsync(sql, parameters);

                return updateDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> DeleteHolidayList(string uID)
        {
            DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayList> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.HolidayList>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"uID" , uID}
            };
            var sql = "DELETE  FROM HolidayList WHERE uID = @uID";

            var Details = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return Details;
        }


        //public async Task<IEnumerable<WINITSharedObjects.Models.Holiday>> GetHolidayDetails(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
        //     int pageSize, List<FilterCriteria> filterCriterias)

        //{
        //    try
        //    {
        //        DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday> dbManager = new DBManager.SqlServerDBManager<WINITSharedObjects.Models.Holiday>(_connectionString);

        //        //var sql = new StringBuilder("SELECT * FROM Holiday");
        //        var sql = new StringBuilder("SELECT * FROM Holiday H JOIN HolidayListRole HLR ON H.HolidayListUID=HLR.HolidayListUID");
        //        var parameters = new Dictionary<string, object>();

        //        if (filterCriterias != null && filterCriterias.Count > 0)
        //        {
        //            sql.Append(" WHERE ");
        //            dbManager.AppendFilterCriteria(filterCriterias, sql, parameters);
        //        }

        //        if (sortCriterias != null && sortCriterias.Count > 0)
        //        {
        //            sql.Append(" ORDER BY ");
        //            dbManager.AppendSortCriteria(sortCriterias, sql);
        //        }

        //        if (pageNumber > 0 && pageSize > 0)
        //        {
        //            sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
        //        }

        //        IEnumerable<WINITSharedObjects.Models.Holiday> HolidayDetails = await dbManager.ExecuteQueryAsync(sql.ToString(), parameters);
        //        DataSet ds = await dbManager.ExecuteQueryDataSetAsync(sql.ToString(), parameters);



        //        List < WINITSharedObjects.Models.Holiday> holidayList = new List<WINITSharedObjects.Models.Holiday>();

        //        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows != null)
        //        {
        //            foreach (DataRow row in ds.Tables[0].Rows)
        //            {
        //                WINITSharedObjects.Models.Holiday holiday = new WINITSharedObjects.Models.Holiday
        //                {
        //                    Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
        //                    UID = row["UID"] != DBNull.Value ? row["UID"].ToString() : string.Empty,
        //                    HolidayListUID = row["HolidayListUID"] != DBNull.Value ? row["HolidayListUID"].ToString() : string.Empty,
        //                    HolidayDate = row["HolidayDate"] != DBNull.Value ? Convert.ToDateTime(row["HolidayDate"]) : DateTime.MinValue,
        //                    Type = row["Type"] != DBNull.Value ? row["Type"].ToString() : string.Empty,
        //                    Name = row["Name"] != DBNull.Value ? row["Name"].ToString() : string.Empty,
        //                    IsOptional = row["IsOptional"] != DBNull.Value && Convert.ToBoolean(row["IsOptional"]),
        //                    Year = row["Year"] != DBNull.Value ? Convert.ToInt32(row["Year"]) : 0,
        //                    SS = row["SS"] != DBNull.Value ? Convert.ToInt32(row["SS"]) : 0,
        //                    CreatedBy = row["CreatedBy"] != DBNull.Value ? row["CreatedBy"].ToString() : string.Empty,
        //                    CreatedTime = row["CreatedTime"] != DBNull.Value ? Convert.ToDateTime(row["CreatedTime"]) : DateTime.MinValue,
        //                    ModifiedBy = row["ModifiedBy"] != DBNull.Value ? row["ModifiedBy"].ToString() : string.Empty,
        //                    ModifiedTime = row["ModifiedTime"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedTime"]) : DateTime.MinValue,
        //                    ServerAddTime = row["ServerAddTime"] != DBNull.Value ? Convert.ToDateTime(row["ServerAddTime"]) : DateTime.MinValue,
        //                    ServerModifiedTime = row["ServerModifiedTime"] != DBNull.Value ? Convert.ToDateTime(row["ServerModifiedTime"]) : DateTime.MinValue
        //                };


        //                holiday.HolidayListRoles = new List<HolidayListRole>();

        //                foreach (DataRow childRow in row.GetChildRows("HolidayListRole"))
        //                {
        //                    HolidayListRole holidayListRole = new HolidayListRole
        //                    {
        //                        Id = Convert.ToInt32(childRow["Id"]),
        //                        UID = childRow["UID"].ToString(),
        //                        HolidayListUID = childRow["HolidayListUID"].ToString(),
        //                        UserRoleUID = childRow["UserRoleUID"].ToString(),
        //                        SS = Convert.ToInt32(childRow["SS"]),
        //                        CreatedTime = Convert.ToDateTime(childRow["CreatedTime"]),
        //                        ModifiedTime = Convert.ToDateTime(childRow["ModifiedTime"]),
        //                        ServerAddTime = Convert.ToDateTime(childRow["ServerAddTime"]),
        //                        ServerModifiedTime = Convert.ToDateTime(childRow["ServerModifiedTime"])
        //                    };

        //                    holiday.HolidayListRoles.Add(holidayListRole);
        //                }

        //                holidayList.Add(holiday);
        //            }
        //        }
        //        return holidayList;
        //    }


        //    catch (Exception ex)
        //    {
        //        throw;
        //    }




        //}
    }
}

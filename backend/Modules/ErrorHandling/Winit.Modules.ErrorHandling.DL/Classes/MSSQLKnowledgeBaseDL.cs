using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.DL.Interfaces;
using Winit.Modules.ErrorHandling.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using System.Data;
using Winit.Shared.CommonUtilities.Extensions;
using System.Reflection.Metadata.Ecma335;
using Winit.Shared.Models.Constants;
namespace Winit.Modules.ErrorHandling.DL.Classes
{
    public class MSSQLKnowledgeBaseDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IKnowledgeBaseDL
    {
        public MSSQLKnowledgeBaseDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<IErrorDetail> GetErrorDetailAsync(string errorCode, string languageCode)
        {
            string sql = @"
            SELECT
	            ed.uid as UID,
                ed.error_code AS ErrorCode,
                ed.severity AS Severity,
                ed.category AS Category,
                ed.platform AS Platform,
                ed.module AS Module,
                ed.sub_module AS SubModule,
                edl.short_description AS ShortDescription,
                edl.language_code AS LanguageCode,
                edl.description AS Description,
                edl.cause AS Cause,
                edl.resolution AS Resolution,
	            edl.uid AS DetailUID,
	            ed.ss AS SS,
	            ed.created_time AS CreatedTime,
	            ed.modified_time AS ModifiedTime,
	            ed.server_add_time AS ServerAddTime,
	            ed.server_modified_time as ServerModifiedTime
            FROM error_details ed
            INNER JOIN error_details_localization edl ON ed.error_code = edl.error_code
            WHERE
                ed.error_code = @ErrorCode
                AND edl.language_code = @LanguageCode";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"ErrorCode",  errorCode},
                {"LanguageCode",  languageCode}
            };
            IErrorDetail errorDetail = await ExecuteSingleAsync<IErrorDetail>(sql, parameters);
            return errorDetail;
        }
        public async Task<List<IErrorDetail>> GetErrorDetailsAsync()
        {
            string sql = @"
            SELECT
	            ed.uid as UID,
                ed.error_code AS ErrorCode,
                ed.severity AS Severity,
                ed.category AS Category,
                ed.platform AS Platform,
                ed.module AS Module,
                ed.sub_module AS SubModule,
                edl.short_description AS ShortDescription,
                edl.language_code AS LanguageCode,
                edl.description AS Description,
                edl.cause AS Cause,
                edl.resolution AS Resolution,
	            edl.uid AS DetailUID,
	            ed.ss AS SS,
	            ed.created_time AS CreatedTime,
	            ed.modified_time AS ModifiedTime,
	            ed.server_add_time AS ServerAddTime,
	            ed.server_modified_time as ServerModifiedTime
            FROM error_details ed
            INNER JOIN error_details_localization edl ON ed.error_code = edl.error_code
            ";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };
            List<IErrorDetail> errorDetail = await ExecuteQueryAsync<IErrorDetail>(sql, parameters);
            return errorDetail;
        }
        public async Task<PagedResponse<Model.Interfaces.IErrorDetailModel>> GetErrorDetails(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from(select error_code as errorcode,severity,category,platform,module,
                                            sub_module as SubModule,uid,ss,created_time as createdtime,
                                            modified_time as modifiedtime,server_add_time as serveraddtime,
                                            server_modified_time as servermodifiedtime from error_details)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select error_code as errorcode,severity,category,platform,module,
                                            sub_module as SubModule,uid,ss,created_time as createdtime,
                                            modified_time as modifiedtime,server_add_time as serveraddtime,
                                            server_modified_time as servermodifiedtime from error_details)as subquery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IErrorDetailModel>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<IErrorDetailModel> errorDetails = await ExecuteQueryAsync<IErrorDetailModel>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<IErrorDetailModel> pagedResponse = new PagedResponse<IErrorDetailModel>
                {
                    PagedData = errorDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IErrorDetailModel> GetErrorDetailsByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"select error_code as ErrorCode,severity,category,platform,module,sub_module as SubModule,uid,ss,
                      created_time as CreatedTime, modified_time ModifiedTime,server_add_time as ServerAddTime,
                server_modified_time as ServerModifiedTime from error_details where uid = @uid";
                  return await ExecuteSingleAsync<IErrorDetailModel>(sql, parameters);
        }

        public async Task<int> CreateErrorDetails(IErrorDetailModel errorDetail)
        {
            try
            {
                var sql = @"insert into error_details (
                           error_code, severity, category, platform, module, sub_module,uid,
                            ss, created_time, modified_time, server_add_time, server_modified_time) 
                        VALUES (
                            @ErrorCode, @Severity, @Category, @Platform,@Module, @SubModule, @UID, @SS, 
                            @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
                return await ExecuteNonQueryAsync(sql, errorDetail);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<int> UpdateErrorDetails(IErrorDetailModel errorDetail)
        {
            try
            {
                var sql = @"update error_details set 
                            severity = @Severity, 
                            category = @Category, 
                            platform = @Platform, 
                            sub_module = @SubModule, 
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime 
                             WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, errorDetail);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<Model.Interfaces.IErrorDescriptionDetails> GetErrorDescriptionDetailsByErroCode(string errorCode)
        {
            IErrorDescriptionDetails result = null;
            Type errorDetailModelType = _serviceProvider.GetRequiredService<IErrorDetailModel>().GetType();
            Type errorDetailsLocalizationType = _serviceProvider.GetRequiredService<IErrorDetailsLocalization>().GetType();
            try
            {
                var sql = @"select error_code as ErrorCode,severity as Severity,category as Category,platform as Platform,
                            module as Module,sub_module as SubModule,uid as UID,ss as SS,created_time as CreatedTime,modified_time as 
                           ModifiedTime,server_add_time as ServerAddTime,server_modified_time as ServerModifiedTime 
                           from error_details where error_code = @errorCode;

                                select
                                error_code as ErrorCode,
                                language_code as LanguageCode,
                                short_description as ShortDescription,
   	                            description as Description,
   	                            cause as Cause,
                                resolution Resolution,
   	                            uid as UID,
   	                            ss as SS,
   	                            created_time as CreatedTime,
   	                            modified_time as ModifiedTime,
   	                            server_add_time as ServerAddTime,
                                server_modified_time as ServerModifiedTime
                                from
                                    error_details_localization
                            
                                WHERE error_code = @errorCode;";

                var parameters = new Dictionary<string, object>()
                 {
                 { "errorCode", errorCode },
                 };
                DataSet ds = await ExecuteQueryDataSetAsync(sql, parameters);
                if (ds != null && ds.Tables.Count == 2)
                {
                    DataTable dataTable0 = ds.Tables[0];
                    DataTable dataTable1 = ds.Tables[1];
                    result = _serviceProvider.CreateInstance<IErrorDescriptionDetails>();
                    if (dataTable0.Rows.Count > 0)
                    {
                        result.errorDetail = new ErrorDetailModel();
                        foreach (DataRow row in dataTable0.Rows)
                        {
                            IErrorDetailModel errorDetail = ConvertDataTableToObject<IErrorDetailModel>(row, null, errorDetailModelType);
                            result.errorDetail = errorDetail;
                        }
                    }
                    if (dataTable1.Rows.Count > 0)
                    {
                        result.errorDetailsLocalizationList = new List<IErrorDetailsLocalization>();

                        foreach (DataRow row in dataTable1.Rows)
                        {
                            IErrorDetailsLocalization errorDetailsLocalization = ConvertDataTableToObject<IErrorDetailsLocalization>(row, null, errorDetailsLocalizationType);
                            result.errorDetailsLocalizationList.Add(errorDetailsLocalization);
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<IEnumerable<Model.Interfaces.IErrorDetailsLocalization>> GetErrorDetailsLocalizationByErrorCode(List<string> uidList)
        {
            try
            {
                var sql = new StringBuilder(@"select
                                error_code as errorcode,
                                language_code as languagecode,
                                short_description as shortdescription,
                            	description,
                            	cause,
                                resolution,
                            	uid,
                            	ss,
                            	created_time as createdtime,
                            	modified_time as modifiedtime,
                            	server_add_time as serveraddtime,
                                server_modified_time as servermodifiedtime
                            from
                                error_details_localization
                            
                            WHERE uid In(@commaSeparatedList)");
                var parameters = new { UIDs = uidList };
                IEnumerable<IErrorDetailsLocalization> errorDetails = await ExecuteQueryAsync<IErrorDetailsLocalization>(sql.ToString(), parameters);
                return errorDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CUDErrorDetailsLocalization(List<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetailsLocalization> errorDetailsLocalizations)
        {
            int count = 0;
            if (errorDetailsLocalizations == null || errorDetailsLocalizations.Count == 0)
            {
                return count;
            }
            List<string> uidList = errorDetailsLocalizations.Select(po => po.UID).ToList();
            List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.ErrorDetailsLocalization, uidList);
            List<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetailsLocalization>? newErrorDetailsLocalizations = null;
            List<Winit.Modules.ErrorHandling.Model.Classes.ErrorDetailsLocalization>? existingErrorDetailsLocalizations = null;
            if (existingUIDs != null && existingUIDs.Count > 0)
            {
                newErrorDetailsLocalizations = errorDetailsLocalizations.Where(edl => !existingUIDs.Contains(edl.UID)).ToList();
                existingErrorDetailsLocalizations = errorDetailsLocalizations.Where(edl => existingUIDs.Contains(edl.UID)).ToList();
            }
            else
            {
                newErrorDetailsLocalizations = errorDetailsLocalizations;
            }
            if (existingErrorDetailsLocalizations != null && existingErrorDetailsLocalizations.Any())
            {
                count += await UpdateErrorDetailsLocalization(existingErrorDetailsLocalizations);
            }
            if (newErrorDetailsLocalizations != null && newErrorDetailsLocalizations.Any())
            {
                count += await CreateErrorDetailsLocalization(newErrorDetailsLocalizations);
            }
            return count;
        }
        public async Task<int> UpdateErrorDetailsLocalization(List<ErrorDetailsLocalization> errorDetailsLocalizations)
        {
            var result = -1;
            try
            {
                var sql = @"update error_details_localization set 
                            language_code = @LanguageCode, 
                            short_description = @ShortDescription, 
                            description = @Description, 
                            cause = @Cause, 
                            resolution = @Resolution, 
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime 
                             WHERE uid = @UID";
                result = await ExecuteNonQueryAsync(sql, errorDetailsLocalizations);
                return result;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateErrorDetailsLocalization(List<ErrorDetailsLocalization> errorDetailsLocalizations)
        {
            var result = -1;
            try
            {
                var sql = @"insert into error_details_localization (
                           error_code, language_code, short_description, description, cause, resolution,uid,
                            ss, created_time, modified_time, server_add_time, server_modified_time) 
                        VALUES (
                            @ErrorCode, @LanguageCode, @ShortDescription, @Description, @Cause,@Resolution, @UID, @SS, 
                            @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
                result = await ExecuteNonQueryAsync(sql, errorDetailsLocalizations);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IErrorDetailsLocalization?> GetErrorDetailsLocalizationbyUID(string errorDetailsLocalizationUID)
        {
            try
            {
                var sql = @" Select error_code as errorcode,
                                language_code as languagecode,
                                short_description as shortdescription,
                            	description,
                            	cause,
                                resolution,
                            	uid,
                            	ss,
                            	created_time as createdtime,
                            	modified_time as modifiedtime,
                            	server_add_time as serveraddtime,
                                server_modified_time as servermodifiedtime from error_details_localization where uid  = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                 {
                       {"UID", errorDetailsLocalizationUID},
                 };
                return await ExecuteSingleAsync<IErrorDetailsLocalization>(sql, parameters);

            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.DL.Classes
{
    public class PGSQLLocationTemplateDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ILocationTemplateDL
    {
        IServiceProvider _serviceProvider;
        public PGSQLLocationTemplateDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task<List<ILocationTemplate>> SelectAllLocationTemplates()
        {
            string sql = @"SELECT id, uid, created_by, created_time, 
                              modified_by, modified_time, server_add_time, 
                              server_modified_time, template_code, template_name, 
                              is_active, location_template_data 
                       FROM location_template";
            var data = await ExecuteQueryAsync<ILocationTemplate>(sql);
            return data;
        }
        public async Task<int> CreateLocationTemplate(ILocationTemplate locationTemplate)
        {
            string sql = @"INSERT INTO location_template(
                                        id, uid, created_by, created_time, modified_by, modified_time,
                                        server_add_time, server_modified_time, template_code, template_name,
                                        is_active, location_template_data)
                                    VALUES (
                                        @Id, @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                                        @ServerAddTime, @ServerModifiedTime, @TemplateCode, @TemplateName,
                                @IsActive, @LocationTemplateData::json)";
            string locationTemplateDataJson = null;
            if (locationTemplate.LocationTemplateData != null)
            {
                locationTemplateDataJson = JsonConvert.SerializeObject(locationTemplate.LocationTemplateData);
            }
            locationTemplate.LocationTemplateData = locationTemplateDataJson;
            return await ExecuteNonQueryAsync(sql, locationTemplate);
        }
        public async Task<int> UpdateLocationTemplate(ILocationTemplate locationTemplate)
        {
            string sql = @"UPDATE location_template
                   SET 
                       modified_by = @ModifiedBy,
                       modified_time = @ModifiedTime,
                       server_modified_time = @ServerModifiedTime,
                       template_code = @TemplateCode,
                       template_name = @TemplateName,
                       is_active = @IsActive,
                       location_template_data = @LocationTemplateData::json
                   WHERE uid = @Uid";
            string locationTemplateDataJson = JsonConvert.SerializeObject(locationTemplate.LocationTemplateData);
            locationTemplate.LocationTemplateData = locationTemplateDataJson;
            return await ExecuteNonQueryAsync(sql, locationTemplate);
        }
        public async Task<List<ILocationTemplateLine>> SelectAllLocationTemplatesLineBytemplateUID(string templateUID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"TemplateUID",  templateUID}
                };
            string sql = @"SELECT 
                                    ltl.id AS Id,
                                    ltl.uid AS Uid,
                                    ltl.created_by AS CreatedBy,
                                    ltl.created_time AS CreatedTime,
                                    ltl.modified_by AS ModifiedBy,
                                    ltl.modified_time AS ModifiedTime,
                                    ltl.server_add_time AS ServerAddTime,
                                    ltl.server_modified_time AS ServerModifiedTime,
                                    ltl.location_template_uid AS LocationTemplateUid,
                                    ltl.location_type_uid AS LocationTypeUid,
									lt.name as Type,
                                    ltl.location_uid AS LocationUid,
									l.name as value,
                                    ltl.is_excluded AS IsExcluded
                                FROM 
                                    location_template_line ltl
									Left join location_type lt on ltl.location_type_uid=lt.uid
									Left join location l on ltl.location_uid=l.uid	 where ltl.location_template_uid=@TemplateUID";

            return await ExecuteQueryAsync<ILocationTemplateLine>(sql, parameters);

        }


        #region CUDLocationTemplateAndLine
        public async Task<int> CUDLocationMappingAndLine(LocationTemplateMaster locationTemplateMaster)
        {
            List<ILocationTemplateLine> locationTemplateLines = null;
            if (locationTemplateMaster.LocationMappingLineList != null)
            {
                locationTemplateLines = locationTemplateMaster.LocationMappingLineList.ToList<ILocationTemplateLine>();
            }
            int count = -1;
            try
            {
                if (locationTemplateMaster.LocationTemplate != null)
                {
                    count = await CUDLocationTemplate(locationTemplateMaster.LocationTemplate);
                }
                if (locationTemplateMaster.LocationMappingLineList != null && locationTemplateMaster.LocationMappingLineList.Any())
                {
                    count = await CUDLocationTemplateLine(locationTemplateLines);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        #endregion
        #region CUDLocationTemplate
        public async Task<int> CUDLocationTemplate(Winit.Modules.Location.Model.Interfaces.ILocationTemplate locationTemplate)
        {
            int count = -1;
            try
            {
                var existingRec = await SelectLocationTemplateByUID(locationTemplate.UID);
                if (existingRec == null)
                {
                    count = await CreateLocationTemplate(locationTemplate);
                }
                else
                {
                    count = await UpdateLocationTemplate(locationTemplate);
                }
            }
            catch
            {
                throw;
            }

            return count;
        }
        private async Task<string> SelectLocationTemplateByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",  UID}
                };
            var sql = @"SELECT  uid FROM location_template WHERE uid = @UID;";
            return await ExecuteSingleAsync<string>(sql, parameters);

        }
        #endregion
        #region CUDLocationTemplateLine
        public async Task<int> CUDLocationTemplateLine(List<Winit.Modules.Location.Model.Interfaces.ILocationTemplateLine> locationTemplateLines)
        {
            int count = -1;
            if (locationTemplateLines == null || locationTemplateLines.Count == 0)
            {
                return count;
            }

            List<string> uidList = locationTemplateLines.Select(ltl => ltl.UID).ToList();

            try
            {
                List<string> existingUIDs = await CheckLocationTemplateLineByUIDs(uidList);
                List<Winit.Modules.Location.Model.Interfaces.ILocationTemplateLine> newLocationTemplateLines = locationTemplateLines.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                List<Winit.Modules.Location.Model.Interfaces.ILocationTemplateLine> existingLocationTemplateLines = locationTemplateLines.Where(e => existingUIDs.Contains(e.UID)).ToList();
                if (existingLocationTemplateLines.Any())
                {
                    count = await UpdateLoactionTemplateLine(existingLocationTemplateLines);
                }
                if (newLocationTemplateLines.Any())
                {
                    count = await InsertLoactionTemplateLine(newLocationTemplateLines);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        private async Task<List<string>> CheckLocationTemplateLineByUIDs(List<string> UIDs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UIDs",  UIDs}
                };
            var sql = @"SELECT uid FROM  location_template_line  WHERE  UID = ANY(@UIDs) ";
            return await ExecuteQueryAsync<string>(sql, parameters);
        }
        private async Task<int> InsertLoactionTemplateLine(List<ILocationTemplateLine> locationTemplateLines)
        {
            int count = -1;
            try
            {
                string sql = @"INSERT INTO location_template_line(
                                 uid, created_by, created_time, modified_by, modified_time,
                                server_add_time, server_modified_time, location_template_uid, location_type_uid,
                                location_uid, is_excluded)
                            VALUES (
                                 @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                                @ServerAddTime, @ServerModifiedTime, @LocationTemplateUid, @LocationTypeUid,
                                @LocationUid, @IsExcluded)";
                count = await ExecuteNonQueryAsync(sql, locationTemplateLines);
            }
            catch (Exception)
            {
                throw;
            }

            return count;
        }
        private async Task<int> UpdateLoactionTemplateLine(List<ILocationTemplateLine> locationTemplateLines)
        {
            int count = -1;
            try
            {
                string sql = @"UPDATE location_template_line
                    SET
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        location_template_uid = @LocationTemplateUid,
                        location_type_uid = @LocationTypeUid,
                        location_uid = @LocationUid,
                        is_excluded = @IsExcluded
                    WHERE
                        uid = @Uid;";

                count = await ExecuteNonQueryAsync(sql, locationTemplateLines);

            }
            catch (Exception)
            {
                throw;
            }

            return count;
        }
        #endregion
        public async Task<int> DeleteLocationTemplateLines(List<string> uIDs)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                 {
                       {"UIDs",  uIDs}
                 };
                var sql = @"
                    DELETE FROM location_template_line
                    WHERE uid = ANY(@UIDs); ";
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }
        }




        #region unusedmethod
        public async Task<int> CreateTemplateLine(List<LocationTemplateLine> templateLines)
        {
            string sql = @"INSERT INTO location_template_line(
                            id, uid, created_by, created_time, modified_by, modified_time,
                            server_add_time, server_modified_time, location_template_uid, location_type_uid,
                            location_uid, is_excluded)
                        VALUES (
                            @Id, @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                            @ServerAddTime, @ServerModifiedTime, @LocationTemplateUid, @LocationTypeUid,
                            @LocationUid, @IsExcluded)";

            int count = 0;
            foreach (var locationTemplateLine in templateLines)
            {
                var parameters = new Dictionary<string, object>()
                    {
                        { "@Id", locationTemplateLine.Id },
                        { "@Uid", locationTemplateLine.UID },
                        { "@CreatedBy", locationTemplateLine.CreatedBy },
                        { "@CreatedTime", locationTemplateLine.CreatedTime },
                        { "@ModifiedBy", locationTemplateLine.ModifiedBy },
                        { "@ModifiedTime", locationTemplateLine.ModifiedTime },
                        { "@ServerAddTime", locationTemplateLine.ServerAddTime },
                        { "@ServerModifiedTime", locationTemplateLine.ServerModifiedTime },
                        { "@LocationTemplateUid", locationTemplateLine.LocationTemplateUID },
                        { "@LocationTypeUid", locationTemplateLine.LocationTypeUID },
                        { "@LocationUid", locationTemplateLine.LocationUID },
                        { "@IsExcluded", locationTemplateLine.IsExcluded }
                };
                count += await ExecuteNonQueryAsync(sql, parameters);
            }
            return count;
        }
        #endregion

    }
}


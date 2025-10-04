using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Classes
{
    public class PGSQLLocationDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ILocationDL
    {
        public PGSQLLocationDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocation>> SelectAllLocationDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select 
                                                    * from (select 
                                                    l.id as Id, 
                                                    l.uid as UID, 
                                                    l.created_by as CreatedBy, 
                                                    l.created_time as CreatedTime, 
                                                    l.modified_by as ModifiedBy, 
                                                    l.modified_time as ModifiedTime, 
                                                    l.server_add_time as ServerAddTime, 
                                                    l.server_modified_time as ServerModifiedTime, 
                                                    l.company_uid as CompanyUID, 
                                                    l.location_type_uid as LocationTypeUID, 
                                                    l.code as Code, 
                                                    l.name as Name, 
                                                    l.parent_uid as ParentUID, 
                                                    l.item_level as ItemLevel ,
                                                     CASE 
                                                            WHEN EXISTS (SELECT 1 FROM location l1 WHERE l1.parent_uid = l.uid) 
                                                            THEN 1 ELSE 0 
                                                        END AS HasChild,
                                                    lt.name as locationtypename,
                                                    lt.code as locationtypecode
                                                from 
                                                    location l INNER JOIN location_type lt ON l.location_type_uid = lt.uid) 
                                                as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("""
                            select 
                            count(*) from (select 
                            l.id as Id, 
                            l.uid as UID, 
                            l.created_by as CreatedBy, 
                            l.created_time as CreatedTime, 
                            l.modified_by as ModifiedBy, 
                            l.modified_time as ModifiedTime, 
                            l.server_add_time as ServerAddTime, 
                            l.server_modified_time as ServerModifiedTime, 
                            l.company_uid as CompanyUID, 
                            l.location_type_uid as LocationTypeUID, 
                            l.code as Code, 
                            l.name as Name, 
                            l.parent_uid as ParentUID, 
                            l.item_level as ItemLevel ,
                                CASE 
                                    WHEN EXISTS (SELECT 1 FROM location l1 WHERE l1.parent_uid = l.uid) 
                                    THEN 1 ELSE 0 
                                END AS HasChild,
                            lt.name as locationtypename,
                            lt.code as locationtypecode
                        from 
                            location l INNER JOIN location_type lt ON l.location_type_uid = lt.uid) as SubQuery
                        """);
                }
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    FilterCriteria? filterCriteria = filterCriterias.Find(e => e.Name == "locationhierarchy_level_code_name");
                    if (filterCriteria != null)
                    {
                        sbFilterCriteria.Append(" (CONVERT(varchar,code,120) Like '%' + @code + '%' OR CONVERT(varchar,name,120) LIKE '%' + @name + '%') ");
                        parameters.Add("code", filterCriteria.Value);
                        parameters.Add("name", filterCriteria.Value);
                        filterCriterias.Remove(filterCriteria);
                        if (filterCriterias.Any()) sbFilterCriteria.Append(" AND ");
                    }
                    AppendFilterCriteria<Winit.Modules.Location.Model.Interfaces.ILocation>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.Location.Model.Interfaces.ILocation> locationDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ILocation>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocation> pagedResponse = new PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocation>
                {
                    PagedData = locationDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.ILocation?> GetLocationByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"select 
                                id as Id, 
                                uid as UID, 
                                created_by as CreatedBy, 
                                created_time as CreatedTime, 
                                modified_by as ModifiedBy, 
                                modified_time as ModifiedTime, 
                                server_add_time as ServerAddTime, 
                                server_modified_time as ServerModifiedTime, 
                                company_uid as CompanyUID, 
                                location_type_uid as LocationTypeUID, 
                                code as Code, 
                                name as Name, 
                                parent_uid as ParentUID, 
                                item_level as ItemLevel 
                            from 
                                public.location
                             WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ILocation>().GetType();

           return await ExecuteSingleAsync<Winit.Modules.Location.Model.Interfaces.ILocation>(sql, parameters, type);
        }

        public async Task<List<Winit.Modules.Location.Model.Interfaces.ILocation>> GetLocationByType(List<string> locationTypes)
        {
            //var upperCaseLocationTypeList = locationTypes.Select(t => t.ToUpper()).ToList();
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Type",  locationTypes.ToArray()}
            };
            var sql = @"select 
                                L.id as Id, 
                                L.uid as UID, 
                                L.created_by as CreatedBy, 
                                L.created_time as CreatedTime, 
                                L.modified_by as ModifiedBy, 
                                L.modified_time as ModifiedTime, 
                                L.server_add_time as ServerAddTime, 
                                L.server_modified_time as ServerModifiedTime, 
                                L.company_uid as CompanyUID, 
                                L.location_type_uid as LocationTypeUID, 
                                L.code as Code, 
                                L.name as Name, 
                                L.parent_uid as ParentUID, 
                                L.item_level as ItemLevel 
                            from 
                                public.location L
                            INNER JOIN public.location_type LT ON LT.uid = L.location_type_uid
                            AND LT.Code = ANY(@Type)";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ILocation>().GetType();
            List<Winit.Modules.Location.Model.Interfaces.ILocation> locationList = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ILocation>(sql, parameters, type);
            return locationList;
        }
        public async Task<int> CreateLocation(Winit.Modules.Location.Model.Interfaces.ILocation createLocation)
        {
            try
            {
                var sql = @"INSERT INTO location (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, company_uid, location_type_uid, code, name, parent_uid, item_level)
                    VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @CompanyUID, @LocationTypeUID, @Code, @Name, @ParentUID, @ItemLevel);";


                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID", createLocation.UID},
                   {"CreatedBy", createLocation.CreatedBy},
                   {"ModifiedBy", createLocation.ModifiedBy},
                   {"CreatedTime", createLocation.CreatedTime},
                   {"CompanyUID", createLocation.CompanyUID},
                   {"LocationTypeUID", createLocation.LocationTypeUID},
                   {"Code", createLocation.Code},
                   {"Name", createLocation.Name},
                   {"ParentUID", createLocation.ParentUID},
                   {"ModifiedTime", createLocation.ModifiedTime},
                   {"ServerAddTime", createLocation.ServerAddTime},
                   {"ServerModifiedTime", createLocation.ServerModifiedTime},
                   {"ItemLevel", createLocation.ItemLevel},
             };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public async Task<int> UpdateLocationDetails(Winit.Modules.Location.Model.Interfaces.ILocation updateLocation)
        {
            try
            {
                var sql = @"UPDATE location 
                        SET created_by = @CreatedBy, 
                            created_time = @CreatedTime, 
                            modified_by = @ModifiedBy, 
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime, 
                            company_uid = @CompanyUID, 
                            location_type_uid = @LocationTypeUID, 
                            code = @Code, 
                            name = @Name, 
                            parent_uid = @ParentUID, 
                            item_level = @ItemLevel 
                        WHERE uid = @UID;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"UID", updateLocation.UID},
                   {"CreatedBy", updateLocation.CreatedBy},
                   {"ModifiedBy", updateLocation.ModifiedBy},
                   {"CreatedTime", updateLocation.CreatedTime},
                   {"CompanyUID", updateLocation.CompanyUID},
                   {"LocationTypeUID", updateLocation.LocationTypeUID},
                   {"Code", updateLocation.Code},
                   {"Name", updateLocation.Name},
                   {"ParentUID", updateLocation.ParentUID},
                   {"ModifiedTime", updateLocation.ModifiedTime},
                   {"ServerModifiedTime", updateLocation.ServerModifiedTime},
                   {"ItemLevel", updateLocation.ItemLevel},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> DeleteLocationDetails(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM Location WHERE UID = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);

        }
        public async Task<List<Winit.Modules.Location.Model.Interfaces.ILocation>> GetCityandLoaclityByUIDs(List<string> UIDs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs",  UIDs}
            };
            var sql = @"SELECT 
                        L.id as Id, 
                        L.uid as UID, 
                        L.created_by as CreatedBy, 
                        L.created_time as CreatedTime, 
                        L.modified_by as ModifiedBy, 
                        L.modified_time as ModifiedTime, 
                        L.server_add_time as ServerAddTime, 
                        L.server_modified_time as ServerModifiedTime, 
                        L.company_uid as CompanyUID, 
                        L.location_type_uid as LocationTypeUID, 
                        L.code as Code, 
                        L.name as Name, 
                        L.parent_uid as ParentUID, 
                        L.item_level as ItemLevel 
                    FROM 
                        location L
                    INNER JOIN 
                        location_type LT 
                    ON 
                        LT.uid = L.location_type_uid 
                    WHERE 
                        (LT.Code = 'City' AND L.parent_uid IN (@UIDs)) 
                        OR 
                        (LT.Code = 'Locality' AND L.parent_uid IN (@UIDs))";
            List<Winit.Modules.Location.Model.Interfaces.ILocation> locationList = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ILocation>(sql, parameters);
            return locationList;
        }

        Task<ILocation?> ILocationDL.GetCountryAndRegionByState(string UID, string Type)
        {
            throw new NotImplementedException();
        }
    }
}

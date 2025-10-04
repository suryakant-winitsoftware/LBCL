using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Classes
{
    public class MSSQLCityBranchMappingDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ICityBranchMappingDL
    {
        public MSSQLCityBranchMappingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ICityBranch>> SelectCityBranchDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from 
                                            (SELECT L.Id As Id, L.uid, '[' + LS.code + '] ' + LS.name AS StateCodeName, 
                                            '[' + L.code + '] ' + L.name AS CityCodeName,
                                            COALESCE('[ ' + CBML.code + '] ' + CBML.name, 'N/A') AS BranchCodeName
                                            FROM  location L
                                            INNER JOIN 
                                            location_type LT ON LT.uid = L.location_type_uid AND LT.code = 'City'
                                            INNER JOIN 
                                            location LS ON LS.uid = L.parent_uid
                                            LEFT JOIN 
                                            city_branch_mapping CBM ON CBM.city_location_uid = L.uid
                                            LEFT JOIN 
                                            location CBML ON CBML.uid = CBM.branch_location_uid
                                            ) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                            (SELECT  L.Id As Id, L.uid, '[' + LS.code + '] ' + LS.name AS StateCodeName, 
                                            '[' + L.code + '] ' + L.name AS CityCodeName,
                                            COALESCE('[ ' + CBML.code + '] ' + CBML.name, 'N/A') AS BranchCodeName
                                            FROM  location L
                                            INNER JOIN 
                                            location_type LT ON LT.uid = L.location_type_uid AND LT.code = 'City'
                                            INNER JOIN 
                                            location LS ON LS.uid = L.parent_uid
                                            LEFT JOIN 
                                            city_branch_mapping CBM ON CBM.city_location_uid = L.uid
                                            LEFT JOIN 
                                            location CBML ON CBML.uid = CBM.branch_location_uid
                                             ) as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Location.Model.Interfaces.ICityBranch>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.Location.Model.Interfaces.ICityBranch> cityBranchDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ICityBranch>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ICityBranch> pagedResponse = new PagedResponse<Winit.Modules.Location.Model.Interfaces.ICityBranch>
                {
                    PagedData = cityBranchDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<ISelectionItem>> SelectBranchDetails()
        {
            try
            {
                var sql = new StringBuilder(@" 
                                            select l.uid as UID, l.name as Label,l.name  as code  from location l join location_type lt on lt.uid=l.location_type_uid
                                            where lt.name='Branch'");
                List<ISelectionItem> branchDetails = await ExecuteQueryAsync<ISelectionItem>(sql.ToString());
                return branchDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateCityBranchMapping(List<Winit.Modules.Location.Model.Interfaces.ICityBranchMapping> cityBranchMappings)
        {
            try
            {
                var sql = @"INSERT INTO city_branch_mapping (uid,created_by,created_time,modified_by,modified_time,server_add_time
                            ,server_modified_time,ss,city_location_uid,branch_location_uid)
                            VALUES 
                            (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,@SS, 
                            @CityLocationUID, @BranchLocationUID);";
                return await ExecuteNonQueryAsync(sql, cityBranchMappings);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


    }
}

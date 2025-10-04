using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Vehicle.DL.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Vehicle.DL.Classes
{
    public class SQLiteVehicleDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IVehicleDL
    {
        public SQLiteVehicleDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>> SelectAllVehicleDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired,string OrgUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                                    id AS VehicleId,
                                                    uid AS VehicleUID,
                                                    created_by AS VehicleCreatedBy,
                                                    created_time AS VehicleCreatedTime,
                                                    modified_by AS VehicleModifiedBy,
                                                    modified_time AS VehicleModifiedTime,
                                                    server_add_time AS VehicleServerAddTime,
                                                    server_modified_time AS VehicleServerModifiedTime,
                                                    company_uid AS VehicleCompanyUID,
                                                    org_uid AS VehicleOrgUID,
                                                    vehicle_no AS VehicleNo,
                                                    registration_no AS VehicleRegistrationNo,
                                                    model AS VehicleModel,
                                                    type AS VehicleType,
                                                    is_active AS VehicleIsActive,
                                                    truck_si_date AS VehicleTruckSiDate,
                                                    road_tax_expiry_date AS VehicleRoadTaxExpiryDate,
                                                    inspection_date AS VehicleInspectionDate
                                                FROM 
                                                    vehicle) As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                id AS VehicleId,
                                                uid AS VehicleUID,
                                                created_by AS VehicleCreatedBy,
                                                created_time AS VehicleCreatedTime,
                                                modified_by AS VehicleModifiedBy,
                                                modified_time AS VehicleModifiedTime,
                                                server_add_time AS VehicleServerAddTime,
                                                server_modified_time AS VehicleServerModifiedTime,
                                                company_uid AS VehicleCompanyUID,
                                                org_uid AS VehicleOrgUID,
                                                vehicle_no AS VehicleNo,
                                                registration_no AS VehicleRegistrationNo,
                                                model AS VehicleModel,
                                                type AS VehicleType,
                                                is_active AS VehicleIsActive,
                                                truck_si_date AS VehicleTruckSiDate,
                                                road_tax_expiry_date AS VehicleRoadTaxExpiryDate,
                                                inspection_date AS VehicleInspectionDate
                                            FROM 
                                                vehicle) As SubQuery");
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
                //if (pageNumber > 0 && pageSize > 0)
                //{
                //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IVehicle>().GetType();

                IEnumerable<Winit.Modules.Vehicle.Model.Interfaces.IVehicle> VehicleDetails = await ExecuteQueryAsync<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle> pagedResponse = new PagedResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>
                {
                    PagedData = VehicleDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Vehicle.Model.Interfaces.IVehicle> GetVehicleByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT 
                            id AS VehicleId,
                            uid AS VehicleUID,
                            created_by AS VehicleCreatedBy,
                            created_time AS VehicleCreatedTime,
                            modified_by AS VehicleModifiedBy,
                            modified_time AS VehicleModifiedTime,
                            server_add_time AS VehicleServerAddTime,
                            server_modified_time AS VehicleServerModifiedTime,
                            company_uid AS VehicleCompanyUID,
                            org_uid AS VehicleOrgUID,
                            vehicle_no AS VehicleNo,
                            registration_no AS VehicleRegistrationNo,
                            model AS VehicleModel,
                            type AS VehicleType,
                            is_active AS VehicleIsActive,
                            truck_si_date AS VehicleTruckSiDate,
                            road_tax_expiry_date AS VehicleRoadTaxExpiryDate,
                            inspection_date AS VehicleInspectionDate
                        FROM 
                            vehicle WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IVehicle>().GetType();
            Winit.Modules.Vehicle.Model.Interfaces.IVehicle VehicleDetails = await ExecuteSingleAsync<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>(sql, parameters, type);
            return VehicleDetails;
        }
        public async Task<int> CreateVehicle(Winit.Modules.Vehicle.Model.Interfaces.IVehicle createVehicle)
        {
            try
            {
                var sql = @"INSERT INTO Vehicle ( uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, company_uid, org_uid, vehicle_no, registration_no, model, type, is_active, truck_si_date, road_tax_expiry_date, inspection_date) " +
                          @"VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @CompanyUID, @OrgUID, 
                          @VehicleNo, @RegistrationNo,@Model,@Type,@IsActive,@TruckSIDate,@RoadTaxExpiryDate,@InspectionDate)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", createVehicle.UID},
                   {"CreatedBy", createVehicle.CreatedBy},
                   {"ModifiedBy", createVehicle.ModifiedBy},
                   {"CreatedTime", createVehicle.CreatedTime},
                   {"CompanyUID", createVehicle.CompanyUID},
                   {"OrgUID", createVehicle.OrgUID},
                   {"VehicleNo", createVehicle.VehicleNo},
                   {"RegistrationNo",createVehicle.RegistrationNo },
                   {"Model",createVehicle.Model },
                   {"Type",createVehicle.Type },
                   {"IsActive",createVehicle.IsActive },
                   {"TruckSIDate",createVehicle.TruckSIDate },
                   {"RoadTaxExpiryDate",createVehicle.RoadTaxExpiryDate },
                   {"InspectionDate",createVehicle.InspectionDate },
                   {"ModifiedTime", createVehicle.ModifiedTime},
                   {"ServerAddTime", createVehicle.ServerAddTime},
                   {"ServerModifiedTime", createVehicle.ServerModifiedTime},
                };
               return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateVehicleDetails(Winit.Modules.Vehicle.Model.Interfaces.IVehicle updateVehicle)
        {
            try
            {
                var sql = @"UPDATE Vehicle SET 
                    created_by = @CreatedBy, 
                    created_time = @CreatedTime, 
                    modified_by = @ModifiedBy, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime, 
                    company_uid = @CompanyUID, 
                    org_uid = @OrgUID, 
                    vehicle_no = @VehicleNo, 
                    registration_no = @RegistrationNo,
                    model = @Model, 
                    type = @Type, 
                    is_active = @IsActive, 
                    truck_si_date = @TruckSIDate, 
                    road_tax_expiry_date = @RoadTaxExpiryDate, 
                    inspection_date = @InspectionDate 
                WHERE 
                    uid = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"UID", updateVehicle.UID},
                   {"CreatedBy", updateVehicle.CreatedBy},
                   {"ModifiedBy", updateVehicle.ModifiedBy},
                   {"CreatedTime", updateVehicle.CreatedTime},
                   {"CompanyUID", updateVehicle.CompanyUID},
                   {"OrgUID", updateVehicle.OrgUID},
                   {"VehicleNo", updateVehicle.VehicleNo},
                   {"RegistrationNo",updateVehicle.RegistrationNo },
                   {"Model",updateVehicle.Model },
                   {"Type",updateVehicle.Type },
                   {"IsActive",updateVehicle.IsActive },
                   {"TruckSIDate",updateVehicle.TruckSIDate },
                   {"RoadTaxExpiryDate",updateVehicle.RoadTaxExpiryDate },
                   {"InspectionDate",updateVehicle.InspectionDate },
                   {"ModifiedTime", updateVehicle.ModifiedTime},
                   {"ServerModifiedTime", updateVehicle.ServerModifiedTime},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteVehicleDetails(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM vehicle WHERE uid  = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public Task<List<IVehicleStatus>> GetAllVehicleStatusDetailsByEmpUID(string empUID)
        {
            throw new NotImplementedException();
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Vehicle.DL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Vehicle.DL.Classes
{
    public class PGSQLVehicleDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IVehicleDL
    {
        public PGSQLVehicleDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>> SelectAllVehicleDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string OrgUID)
        {
            try
            {
                var sql = new StringBuilder(@" select * from (select
                                            id as Id,
                                            uid as UID,
                                            created_by as CreatedBy,
                                            created_time as CreatedTime,
                                            modified_by as ModifiedBy,
                                            modified_time as ModifiedTime,
                                            server_add_time as ServerAddTime,
                                            server_modified_time as ServerModifiedTime,
                                            company_uid as CompanyUid,
                                            org_uid as OrgUid,
                                            vehicle_no as VehicleNo,
                                            registration_no as RegistrationNo,
                                            model as Model,
                                            type as Type,
                                            is_active as IsActive,
                                            truck_si_date as TruckSiDate,
                                            road_tax_expiry_date as RoadTaxExpiryDate,
                                            inspection_date as InspectionDate,
                                            weight_limit as WeightLimit,
                                            capacity as Capacity,
                                            loading_capacity as LoadingCapacity,
                                            warehouse_code as WarehouseCode,
                                            location_code as LocationCode,
                                            territory_uid as TerritoryUID
                                        from
                                            vehicle
                                        where
                                            org_uid = @orguid)as subquery
                                        ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"select
                                                    count(1) as Cnt
                                                from (
                                                 select
                                            id as Id,
                                            uid as UID,
                                            created_by as CreatedBy,
                                            created_time as CreatedTime,
                                            modified_by as ModifiedBy,
                                            modified_time as ModifiedTime,
                                            server_add_time as ServerAddTime,
                                            server_modified_time as ServerModifiedTime,
                                            company_uid as CompanyUid,
                                            org_uid as OrgUid,
                                            vehicle_no as VehicleNo,
                                            registration_no as RegistrationNo,
                                            model as Model,
                                            type as Type,
                                            is_active as IsActive,
                                            truck_si_date as TruckSiDate,
                                            road_tax_expiry_date as RoadTaxExpiryDate,
                                            inspection_date as InspectionDate,
                                            weight_limit as WeightLimit,
                                            capacity as Capacity,
                                            loading_capacity as LoadingCapacity,
                                            warehouse_code as WarehouseCode,
                                            location_code as LocationCode,
                                            territory_uid as TerritoryUID
                                        from
                                            vehicle
                                        where
                                            org_uid = @orguid )as subquery
                                                ");
                }
                var parameters = new Dictionary<string, object>()
                {
                    {"OrgUID",OrgUID}
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" where ");
                    AppendFilterCriteria<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>(filterCriterias, sbFilterCriteria, parameters); ;
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
            var sql = @"select 
                        id as Id, 
                        uid as UID, 
                        created_by as CreatedBy,
                        created_time as CreatedTime,
                        modified_by as ModifiedBy,
                        modified_time as ModifiedTime,
                        server_add_time as ServerAddTime,
                        server_modified_time as ServerModifiedTime,
                        company_uid as CompanyUid,
                        org_uid as OrgUid,
                        vehicle_no as VehicleNo,
                        registration_no as RegistrationNo,
                        model as Model,
                        type as Type,
                        is_active as IsActive,
                        truck_si_date as TruckSiDate,
                        road_tax_expiry_date as RoadTaxExpiryDate,
                        inspection_date as InspectionDate,
                        weight_limit as WeightLimit,
                        capacity as Capacity,
                        loading_capacity as LoadingCapacity,
                        warehouse_code as WarehouseCode,
                        location_code as LocationCode,
                        territory_uid as TerritoryUID
                    from
                        vehicle
                    where
                        uid = @uid
                    ";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IVehicle>().GetType();
            Winit.Modules.Vehicle.Model.Interfaces.IVehicle VehicleDetails = await ExecuteSingleAsync<Winit.Modules.Vehicle.Model.Interfaces.IVehicle>(sql, parameters, type);
            return VehicleDetails;
        }
        public async Task<int> CreateVehicle(Winit.Modules.Vehicle.Model.Interfaces.IVehicle createVehicle)
        {
            int retVal = -1;
            try
            {
                var sql = @"insert into vehicle (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                              server_modified_time, company_uid, org_uid, vehicle_no, registration_no, model, type, is_active,
                              truck_si_date, road_tax_expiry_date, inspection_date, weight_limit, capacity, loading_capacity,
                              warehouse_code, location_code, territory_uid)
                              values (@uid, @createdby, @createdtime, @modifiedby, @modifiedtime, @serveraddtime,
                               @servermodifiedtime, @companyuid, @orguid, @vehicleno, @registrationno, @model, @type, @isactive,
                               @trucksidate, @roadtaxexpirydate, @inspectiondate, @weightlimit, @capacity, @loadingcapacity,
                               @warehousecode, @locationcode, @territoryuid)";
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
                   {"WeightLimit",createVehicle.WeightLimit },
                   {"Capacity",createVehicle.Capacity },
                   {"LoadingCapacity",createVehicle.LoadingCapacity },
                   {"WarehouseCode",createVehicle.WarehouseCode },
                   {"LocationCode",createVehicle.LocationCode },
                   {"TerritoryUID",createVehicle.TerritoryUID },
                   {"ModifiedTime", createVehicle.ModifiedTime},
                   {"ServerAddTime", createVehicle.ServerAddTime},
                   {"ServerModifiedTime", createVehicle.ServerModifiedTime},
                };
                retVal = await ExecuteNonQueryAsync(sql, parameters);
                if (retVal == 1)
                {
                    await CreateOrg_ForVehicle(createVehicle);
                }
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<int> CreateOrg_ForVehicle(Winit.Modules.Vehicle.Model.Interfaces.IVehicle vehicle)
        {
            var status = vehicle.IsActive ? "Active" : "Inactive";
            try
            {

                var sql = @"INSERT INTO org (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, code, name, is_active, org_type_uid, parent_uid, status)
                            VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                            @Code, @Name, @IsActive, @OrgTypeUID, @ParentUID, @Status);";
                Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                   {"UID", vehicle.UID},
                   {"CreatedBy", vehicle.CreatedBy},
                   {"ModifiedBy", vehicle.ModifiedBy},
                   {"CreatedTime", vehicle.CreatedTime},
                   {"ModifiedTime", vehicle.ModifiedTime},
                   {"ServerAddTime", vehicle.ServerAddTime},
                   {"ServerModifiedTime", vehicle.ServerModifiedTime},
                   {"Code", vehicle.VehicleNo},
                   {"Name", vehicle.RegistrationNo},
                    {"IsActive", vehicle.IsActive},
                   {"OrgTypeUID", "VWH"},
                   {"ParentUID", vehicle.OrgUID},
                   {"Status", status},


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
            int retVal = -1;
            try
            {
                var sql = @"update vehicle
                            set created_by = @CreatedBy,
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
                                inspection_date = @InspectionDate,
                                weight_limit = @WeightLimit,
                                capacity = @Capacity,
                                loading_capacity = @LoadingCapacity,
                                warehouse_code = @WarehouseCode,
                                location_code = @LocationCode,
                                territory_uid = @TerritoryUID
                            where uid = @UID";

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
                   {"WeightLimit",updateVehicle.WeightLimit },
                   {"Capacity",updateVehicle.Capacity },
                   {"LoadingCapacity",updateVehicle.LoadingCapacity },
                   {"WarehouseCode",updateVehicle.WarehouseCode },
                   {"LocationCode",updateVehicle.LocationCode },
                   {"TerritoryUID",updateVehicle.TerritoryUID },
                   {"ModifiedTime", updateVehicle.ModifiedTime},
                   {"ServerModifiedTime", updateVehicle.ServerModifiedTime},
                 };
                retVal = await ExecuteNonQueryAsync(sql, parameters);
                if (retVal == 1)
                {
                    await UpdateOrg(updateVehicle);
                }
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<int> UpdateOrg(Winit.Modules.Vehicle.Model.Interfaces.IVehicle vehicle)
        {
            var status = vehicle.IsActive ? "Active" : "Inactive";

            try
            {
                var sql = @"UPDATE org 
                                    SET 
                                        modified_by = @ModifiedBy, 
                                        name = @Name, 
                                        is_active = @IsActive, 
                                        modified_time = @ModifiedTime, 
                                        server_modified_time = @ServerModifiedTime, 
                                        code = @Code,
                                        status = @Status 
                                    WHERE 
                                        uid = @UID;";

                Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                   {"ModifiedBy", vehicle.ModifiedBy},
                   {"ModifiedTime", vehicle.ModifiedTime},
                   {"ServerModifiedTime", vehicle.ServerModifiedTime},
                   {"Code", vehicle.VehicleNo},
                   {"IsActive", vehicle.IsActive},
                   {"Status", status},
                   {"UID", vehicle.UID},
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
            var sql = @"delete from vehicle where uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<List<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus>> GetAllVehicleStatusDetailsByEmpUID
            (string jobPositionUID)
        {
            // For now, return an empty list to bypass the error during login
            // TODO: Fix the query to properly handle beat_history and store_history tables
            return new List<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus>();
            
            /* Original query commented out - needs fixing
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"jobPositionUID",  jobPositionUID},
                {"visitDate", DateTime.Now.Date}
            };
            var sql = @"SELECT DISTINCT V.uid , V.vehicle_no AS VehicleNo, 
                        V.registration_no AS RegistrationCode
                        FROM route R
                        INNER JOIN (
			                        SELECT DISTINCT BH.route_uid 
			                        FROM 
			                        route_user RU 
			                        INNER JOIN beat_history BH ON BH.route_uid = RU.route_uid 
			                        AND RU.job_position_uid = @jobPositionUID 
			                        AND BH.visit_date::date = @visitDate
			                        INNER JOIN store_history SH ON SH.beat_history_uid = BH.uid AND SH.is_planned = true
			                        INNER JOIN store S ON S.uid = SH.store_uid AND S.number != '99999'
                        ) T ON T.route_uid = R.uid
                        INNER JOIN vehicle V ON V.uid = R.vehicle_uid
                    ";
            return await ExecuteQueryAsync<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus>(sql, parameters);
            */
        }
    }
}

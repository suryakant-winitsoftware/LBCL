using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ServiceAndCallRegistration.DL.Interfaces;
using Winit.Modules.ServiceAndCallRegistration.Model.Classes;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ServiceAndCallRegistration.DL.Classes
{
    public class PGSQLServiceAndCallRegistrationDL : Base.DL.DBManager.PostgresDBManager, IServiceAndCallRegistrationDL
    {
        public PGSQLServiceAndCallRegistrationDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }



        public async Task<PagedResponse<ICallRegistration>> GetCallRegistrations(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string jobPositionUID)
        {
            try
            {
                var sql = new StringBuilder("""
                                            SELECT * from (SELECT 
                                                r.id AS Id,
                                                r.uid AS Uid,
                                                r.created_by AS CreatedBy,
                                                r.created_time AS CreatedTime,
                                                r.modified_by AS ModifiedBy,
                                                r.modified_time AS ModifiedTime,
                                                r.server_add_time AS ServerAddTime,
                                                r.server_modified_time AS ServerModifiedTime,
                                                r.ss AS Ss,
                                                r.customer_type AS CustomerType,
                                                r.product_category_name AS ProductCategoryCode,
                                                r.brand_code AS BrandCode,
                                                r.service_type_code AS ServiceType,
                                                r.warranty_status AS WarrantyStatus,
                                                r.customer_name AS CustomerName,
                                                r.contact_person AS ContactPerson,
                                                r.mobile_no AS MobileNumber,
                                                r.pin_code AS Pincode,
                                                r.email AS EmailID,
                                                r.purchase_order_date AS PurchaseDate,
                                                r.reseller_name AS ResellerName,
                                                r.remarks AS Remarks,
                                                r.address AS Address,
                                                r.service_call_number AS ServiceCallNo,r.org_uid as  OrgUID 
                                            FROM 
                                                call_registration r
                                            	inner join my_orgs mo on mo.job_position_uid=@JobPositionUID and mo.org_uid=r.org_uid ) as subquery
                                            
                                            """);
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("""
                                                SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                    r.id AS Id,
                                                    r.uid AS Uid,
                                                    r.created_by AS CreatedBy,
                                                    r.created_time AS CreatedTime,
                                                    r.modified_by AS ModifiedBy,
                                                    r.modified_time AS ModifiedTime,
                                                    r.server_add_time AS ServerAddTime,
                                                    r.server_modified_time AS ServerModifiedTime,
                                                    r.ss AS Ss,
                                                    r.customer_type AS CustomerType,
                                                    r.product_category_name AS ProductCategoryCode,
                                                    r.brand_code AS BrandCode,
                                                    r.service_type_code AS ServiceType,
                                                    r.warranty_status AS WarrantyStatus,
                                                    r.customer_name AS CustomerName,
                                                    r.contact_person AS ContactPerson,
                                                    r.mobile_no AS MobileNumber,
                                                    r.pin_code AS Pincode,
                                                    r.email AS EmailID,
                                                    r.purchase_order_date AS PurchaseDate,
                                                    r.reseller_name AS ResellerName,
                                                    r.remarks AS Remarks,
                                                    r.address AS Address,
                                                    r.service_call_number AS ServiceCallNo,r.org_uid as  OrgUID 
                                                FROM 
                                                    call_registration r
                                                	inner join my_orgs mo on mo.job_position_uid=@JobPositionUID and mo.org_uid=r.org_uid ) as sub_query
                                                """);
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "JobPositionUID", jobPositionUID  }
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration> InventoryItems = await ExecuteQueryAsync<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration> pagedResponse = new PagedResponse<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration>
                {
                    PagedData = InventoryItems,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<ICallRegistration> GetCallRegistrationItemDetailsByCallID(string serviceCallNumber)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                        {"ServiceCallNo",  serviceCallNumber}
                    };
                var sql = """
                                            SELECT 
                                                r.id AS Id,
                                                r.uid AS Uid,
                                                r.created_by AS CreatedBy,
                                                r.created_time AS CreatedTime,
                                                r.modified_by AS ModifiedBy,
                                                r.modified_time AS ModifiedTime,
                                                r.server_add_time AS ServerAddTime,
                                                r.server_modified_time AS ServerModifiedTime,
                                                r.ss AS Ss,
                                                r.customer_type AS CustomerType,
                                                r.product_category_name AS ProductCategoryCode,
                                                r.brand_code AS BrandCode,
                                                r.service_type_code AS ServiceType,
                                                r.warranty_status AS WarrantyStatus,
                                                r.customer_name AS CustomerName,
                                                r.contact_person AS ContactPerson,
                                                r.mobile_no AS MobileNumber,
                                                r.pin_code AS Pincode,
                                                r.email AS EmailID,
                                                r.purchase_order_date AS PurchaseDate,
                                                r.reseller_name AS ResellerName,
                                                r.remarks AS Remarks,
                                                r.address AS Address,
                                                r.service_call_number AS ServiceCallNo,r.org_uid as  OrgUID 
                                            FROM 
                                                call_registration r 
                                            WHERE r.service_call_number = @ServiceCallNo
                                            
                                            """;
                Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration? callRegistrationItemDetails = await ExecuteSingleAsync<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration>(sql, parameters);
                return callRegistrationItemDetails;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<int> SaveCallRegistrationDetails(CallRegistration callRegistrationDetails)
        {
            try
            {
                var sql = """
                        INSERT INTO call_registration
                        (
                            uid, 
                            created_by, 
                            created_time, 
                            modified_by, 
                            modified_time, 
                            server_add_time, 
                            server_modified_time, 
                            ss, 
                            customer_type, 
                            product_category_name, 
                            brand_code, 
                            service_type_code, 
                            warranty_status, 
                            customer_name, 
                            contact_person, 
                            mobile_no, 
                            pin_code, 
                            email, 
                            purchase_order_date, 
                            reseller_name, 
                            remarks, 
                            address, 
                            service_call_number,org_uid
                        )
                        VALUES
                        (
                            @UID, 
                            @CreatedBy, 
                            @CreatedTime, 
                            @ModifiedBy, 
                            @ModifiedTime, 
                            @ServerAddTime, 
                            @ServerModifiedTime, 
                            @SS, 
                            @CustomerType, 
                            @ProductCategoryCode, 
                            @BrandCode, 
                            @ServiceType, 
                            @WarrantyStatus, 
                            @CustomerName, 
                            @ContactPerson, 
                            @MobileNumber, 
                            @Pincode, 
                            @EmailID, 
                            @PurchaseDate, 
                            @ResellerName, 
                            @Remarks, 
                            @Address, 
                            @ServiceCallNo,@OrgUID
                        );
                        """;

                return await ExecuteNonQueryAsync(sql, callRegistrationDetails);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

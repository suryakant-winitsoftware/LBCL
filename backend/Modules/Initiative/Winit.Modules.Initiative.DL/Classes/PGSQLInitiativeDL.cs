using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Winit.Modules.Initiative.DL.Interfaces;
using Winit.Modules.Initiative.Model.Classes;
using Winit.Modules.Initiative.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Initiative.DL.Classes
{
    public class PGSQLInitiativeDL : Base.DL.DBManager.PostgresDBManager, IInitiativeDL
    {
        public PGSQLInitiativeDL(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider, configuration)
        {
        }

        #region Initiative CRUD Operations

        public async Task<IInitiative> GetInitiativeByIdAsync(int initiativeId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        i.*,
                        am.allocation_no, am.allocation_name, am.available_allocation_amount, am.total_allocation_amount
                    FROM initiatives i
                    LEFT JOIN allocation_master am ON i.allocation_no = am.allocation_no
                    WHERE i.initiative_id = @InitiativeId";

                var parameters = new { InitiativeId = initiativeId };
                
                using var connection = CreateConnection();
                var initiatives = await connection.QueryAsync<Model.Classes.Initiative, AllocationMaster, Model.Classes.Initiative>(
                    sql,
                    (initiative, allocation) =>
                    {
                        initiative.AllocationMaster = allocation;
                        return initiative;
                    },
                    parameters,
                    splitOn: "allocation_no"
                );

                var result = initiatives.FirstOrDefault();
                
                if (result != null)
                {
                    // Load customers
                    result.InitiativeCustomers = (await GetInitiativeCustomersAsync(initiativeId)).Cast<InitiativeCustomer>().ToList();
                    
                    // Load products
                    result.InitiativeProducts = (await GetInitiativeProductsAsync(initiativeId)).Cast<InitiativeProduct>().ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting initiative by ID: {ex.Message}", ex);
            }
        }

        public async Task<IInitiative> GetInitiativeByContractCodeAsync(string contractCode)
        {
            try
            {
                var sql = @"
                    SELECT * FROM initiatives 
                    WHERE contract_code = @ContractCode";

                var parameters = new { ContractCode = contractCode };
                
                using var connection = CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<Model.Classes.Initiative>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting initiative by contract code: {ex.Message}", ex);
            }
        }

        public async Task<List<IInitiative>> GetAllInitiativesAsync(InitiativeSearchRequest searchRequest)
        {
            try
            {
                var sql = @"
                    SELECT i.*, am.allocation_name, am.available_allocation_amount
                    FROM initiatives i
                    LEFT JOIN allocation_master am ON i.allocation_no = am.allocation_no
                    WHERE 1=1";

                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(searchRequest.SearchText))
                {
                    sql += " AND (i.name ILIKE @SearchText OR i.contract_code ILIKE @SearchText)";
                    parameters.Add("SearchText", $"%{searchRequest.SearchText}%");
                }

                if (!string.IsNullOrEmpty(searchRequest.SalesOrgCode))
                {
                    sql += " AND i.sales_org_code = @SalesOrgCode";
                    parameters.Add("SalesOrgCode", searchRequest.SalesOrgCode);
                }

                if (!string.IsNullOrEmpty(searchRequest.Brand))
                {
                    sql += " AND i.brand = @Brand";
                    parameters.Add("Brand", searchRequest.Brand);
                }

                if (!string.IsNullOrEmpty(searchRequest.Status))
                {
                    sql += " AND i.status = @Status";
                    parameters.Add("Status", searchRequest.Status);
                }

                if (searchRequest.StartDateFrom.HasValue)
                {
                    sql += " AND i.start_date >= @StartDateFrom";
                    parameters.Add("StartDateFrom", searchRequest.StartDateFrom.Value);
                }

                if (searchRequest.StartDateTo.HasValue)
                {
                    sql += " AND i.start_date <= @StartDateTo";
                    parameters.Add("StartDateTo", searchRequest.StartDateTo.Value);
                }

                sql += " ORDER BY i.created_on DESC";
                sql += $" LIMIT {searchRequest.PageSize} OFFSET {(searchRequest.PageNumber - 1) * searchRequest.PageSize}";

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<Model.Classes.Initiative>(sql, parameters);
                return result.Cast<IInitiative>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting all initiatives: {ex.Message}", ex);
            }
        }

        public async Task<int> GetInitiativeCountAsync(InitiativeSearchRequest searchRequest)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*) 
                    FROM initiatives i
                    WHERE 1=1";

                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(searchRequest.SearchText))
                {
                    sql += " AND (i.name ILIKE @SearchText OR i.contract_code ILIKE @SearchText)";
                    parameters.Add("SearchText", $"%{searchRequest.SearchText}%");
                }

                if (!string.IsNullOrEmpty(searchRequest.SalesOrgCode))
                {
                    sql += " AND i.sales_org_code = @SalesOrgCode";
                    parameters.Add("SalesOrgCode", searchRequest.SalesOrgCode);
                }

                if (!string.IsNullOrEmpty(searchRequest.Brand))
                {
                    sql += " AND i.brand = @Brand";
                    parameters.Add("Brand", searchRequest.Brand);
                }

                if (!string.IsNullOrEmpty(searchRequest.Status))
                {
                    sql += " AND i.status = @Status";
                    parameters.Add("Status", searchRequest.Status);
                }

                using var connection = CreateConnection();
                return await connection.QuerySingleAsync<int>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting initiative count: {ex.Message}", ex);
            }
        }

        public async Task<int> InsertInitiativeAsync(IInitiative initiative)
        {
            try
            {
                var sql = @"
                    INSERT INTO initiatives (
                        allocation_no, name, description, sales_org_code, brand,
                        contract_amount, activity_type, display_type, display_location,
                        customer_type, customer_group, posm_file, default_image, email_attachment,
                        start_date, end_date, status, is_active, created_by, created_on
                    ) VALUES (
                        @AllocationNo, @Name, @Description, @SalesOrgCode, @Brand,
                        @ContractAmount, @ActivityType, @DisplayType, @DisplayLocation,
                        @CustomerType::customer_type_enum, @CustomerGroup, @PosmFile, @DefaultImage, @EmailAttachment,
                        @StartDate, @EndDate, 'draft', true, @CreatedBy, CURRENT_TIMESTAMP
                    )
                    RETURNING initiative_id";

                using var connection = CreateConnection();
                var initiativeId = await connection.QuerySingleAsync<int>(sql, initiative);
                
                // Update allocation consumed amount
                await UpdateAllocationConsumedAmountAsync(initiative.AllocationNo);
                
                return initiativeId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting initiative: {ex.Message}", ex);
            }
        }

        public async Task<int> UpdateInitiativeAsync(IInitiative initiative)
        {
            try
            {
                var sql = @"
                    UPDATE initiatives SET
                        allocation_no = @AllocationNo,
                        name = @Name,
                        description = @Description,
                        sales_org_code = @SalesOrgCode,
                        brand = @Brand,
                        contract_amount = @ContractAmount,
                        activity_type = @ActivityType,
                        display_type = @DisplayType,
                        display_location = @DisplayLocation,
                        customer_type = @CustomerType::customer_type_enum,
                        customer_group = @CustomerGroup,
                        posm_file = @PosmFile,
                        default_image = @DefaultImage,
                        email_attachment = @EmailAttachment,
                        start_date = @StartDate,
                        end_date = @EndDate,
                        modified_by = @ModifiedBy,
                        modified_on = CURRENT_TIMESTAMP
                    WHERE initiative_id = @InitiativeId
                    AND status = 'draft'"; // Only allow updates to draft initiatives

                using var connection = CreateConnection();
                var rowsAffected = await connection.ExecuteAsync(sql, initiative);
                
                if (rowsAffected > 0)
                {
                    await UpdateAllocationConsumedAmountAsync(initiative.AllocationNo);
                }
                
                return rowsAffected;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating initiative: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteInitiativeAsync(int initiativeId)
        {
            try
            {
                // Get allocation to update consumed amount
                var getInitiativeSql = "SELECT allocation_no FROM initiatives WHERE initiative_id = @InitiativeId";
                
                using var connection = CreateConnection();
                var allocationNo = await connection.QuerySingleOrDefaultAsync<string>(getInitiativeSql, new { InitiativeId = initiativeId });
                
                var sql = @"
                    DELETE FROM initiatives 
                    WHERE initiative_id = @InitiativeId 
                    AND status = 'draft'"; // Only allow deletion of draft initiatives

                var rowsAffected = await connection.ExecuteAsync(sql, new { InitiativeId = initiativeId });
                
                if (rowsAffected > 0 && !string.IsNullOrEmpty(allocationNo))
                {
                    await UpdateAllocationConsumedAmountAsync(allocationNo);
                }
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting initiative: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateContractCodeAsync(int initiativeId)
        {
            try
            {
                var contractCode = $"CNT-{initiativeId}-{DateTime.Now:yyyyMMddHHmmss}";
                
                var sql = @"
                    UPDATE initiatives 
                    SET contract_code = @ContractCode 
                    WHERE initiative_id = @InitiativeId";

                using var connection = CreateConnection();
                await connection.ExecuteAsync(sql, new { ContractCode = contractCode, InitiativeId = initiativeId });
                
                return contractCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating contract code: {ex.Message}", ex);
            }
        }

        public async Task<bool> SubmitInitiativeAsync(int initiativeId, string userCode)
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Generate contract code if not exists
                    var checkSql = "SELECT contract_code FROM initiatives WHERE initiative_id = @InitiativeId";
                    var existingCode = await connection.QuerySingleOrDefaultAsync<string>(checkSql, new { InitiativeId = initiativeId }, transaction);
                    
                    if (string.IsNullOrEmpty(existingCode))
                    {
                        var contractCode = await GenerateContractCodeAsync(initiativeId);
                    }

                    // Update status to submitted
                    var sql = @"
                        UPDATE initiatives 
                        SET status = 'submitted',
                            modified_by = @UserCode,
                            modified_on = CURRENT_TIMESTAMP
                        WHERE initiative_id = @InitiativeId
                        AND status = 'draft'";

                    var rowsAffected = await connection.ExecuteAsync(sql, new { InitiativeId = initiativeId, UserCode = userCode }, transaction);
                    
                    transaction.Commit();
                    return rowsAffected > 0;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error submitting initiative: {ex.Message}", ex);
            }
        }

        public async Task<bool> CancelInitiativeAsync(int initiativeId, string cancelReason, string userCode)
        {
            try
            {
                var sql = @"
                    UPDATE initiatives 
                    SET status = 'cancelled',
                        cancel_reason = @CancelReason,
                        modified_by = @UserCode,
                        modified_on = CURRENT_TIMESTAMP
                    WHERE initiative_id = @InitiativeId
                    AND status IN ('draft', 'submitted')";

                using var connection = CreateConnection();
                var rowsAffected = await connection.ExecuteAsync(sql, new 
                { 
                    InitiativeId = initiativeId, 
                    CancelReason = cancelReason, 
                    UserCode = userCode 
                });
                
                if (rowsAffected > 0)
                {
                    // Get allocation to update consumed amount
                    var getAllocationSql = "SELECT allocation_no FROM initiatives WHERE initiative_id = @InitiativeId";
                    var allocationNo = await connection.QuerySingleOrDefaultAsync<string>(getAllocationSql, new { InitiativeId = initiativeId });
                    
                    if (!string.IsNullOrEmpty(allocationNo))
                    {
                        await UpdateAllocationConsumedAmountAsync(allocationNo);
                    }
                }
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error cancelling initiative: {ex.Message}", ex);
            }
        }

        #endregion

        #region Initiative Customer Operations

        public async Task<List<IInitiativeCustomer>> GetInitiativeCustomersAsync(int initiativeId)
        {
            try
            {
                var sql = @"
                    SELECT * FROM initiative_customers 
                    WHERE initiative_id = @InitiativeId
                    ORDER BY customer_name";

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<InitiativeCustomer>(sql, new { InitiativeId = initiativeId });
                return result.Cast<IInitiativeCustomer>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting initiative customers: {ex.Message}", ex);
            }
        }

        public async Task<bool> InsertInitiativeCustomersAsync(int initiativeId, List<IInitiativeCustomer> customers)
        {
            try
            {
                // Delete existing customers first
                var deleteSql = "DELETE FROM initiative_customers WHERE initiative_id = @InitiativeId";
                
                using var connection = CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    await connection.ExecuteAsync(deleteSql, new { InitiativeId = initiativeId }, transaction);

                    if (customers != null && customers.Any())
                    {
                        var insertSql = @"
                            INSERT INTO initiative_customers (
                                initiative_id, customer_code, customer_name, 
                                display_type, display_location, execution_status
                            ) VALUES (
                                @InitiativeId, @CustomerCode, @CustomerName,
                                @DisplayType, @DisplayLocation, 'pending'
                            )";

                        foreach (var customer in customers)
                        {
                            customer.InitiativeId = initiativeId;
                        }

                        await connection.ExecuteAsync(insertSql, customers, transaction);
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting initiative customers: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteInitiativeCustomersAsync(int initiativeId, List<string> customerCodes)
        {
            try
            {
                var sql = @"
                    DELETE FROM initiative_customers 
                    WHERE initiative_id = @InitiativeId 
                    AND customer_code = ANY(@CustomerCodes)";

                using var connection = CreateConnection();
                var rowsAffected = await connection.ExecuteAsync(sql, new 
                { 
                    InitiativeId = initiativeId, 
                    CustomerCodes = customerCodes.ToArray() 
                });
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting initiative customers: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateInitiativeCustomerAsync(IInitiativeCustomer customer)
        {
            try
            {
                var sql = @"
                    UPDATE initiative_customers SET
                        display_type = @DisplayType,
                        display_location = @DisplayLocation,
                        modified_on = CURRENT_TIMESTAMP
                    WHERE initiative_customer_id = @InitiativeCustomerId";

                using var connection = CreateConnection();
                var rowsAffected = await connection.ExecuteAsync(sql, customer);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating initiative customer: {ex.Message}", ex);
            }
        }

        #endregion

        #region Initiative Product Operations

        public async Task<List<IInitiativeProduct>> GetInitiativeProductsAsync(int initiativeId)
        {
            try
            {
                var sql = @"
                    SELECT * FROM initiative_products 
                    WHERE initiative_id = @InitiativeId
                    ORDER BY item_name";

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<InitiativeProduct>(sql, new { InitiativeId = initiativeId });
                return result.Cast<IInitiativeProduct>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting initiative products: {ex.Message}", ex);
            }
        }

        public async Task<bool> InsertInitiativeProductsAsync(int initiativeId, List<IInitiativeProduct> products)
        {
            try
            {
                // Delete existing products first
                var deleteSql = "DELETE FROM initiative_products WHERE initiative_id = @InitiativeId";
                
                using var connection = CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    await connection.ExecuteAsync(deleteSql, new { InitiativeId = initiativeId }, transaction);

                    if (products != null && products.Any())
                    {
                        var insertSql = @"
                            INSERT INTO initiative_products (
                                initiative_id, item_code, item_name, barcode, ptt_price
                            ) VALUES (
                                @InitiativeId, @ItemCode, @ItemName, @Barcode, @PttPrice
                            )";

                        foreach (var product in products)
                        {
                            product.InitiativeId = initiativeId;
                        }

                        await connection.ExecuteAsync(insertSql, products, transaction);
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting initiative products: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteInitiativeProductsAsync(int initiativeId, List<string> itemCodes)
        {
            try
            {
                var sql = @"
                    DELETE FROM initiative_products 
                    WHERE initiative_id = @InitiativeId 
                    AND item_code = ANY(@ItemCodes)";

                using var connection = CreateConnection();
                var rowsAffected = await connection.ExecuteAsync(sql, new 
                { 
                    InitiativeId = initiativeId, 
                    ItemCodes = itemCodes.ToArray() 
                });
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting initiative products: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateInitiativeProductAsync(IInitiativeProduct product)
        {
            try
            {
                var sql = @"
                    UPDATE initiative_products SET
                        ptt_price = @PttPrice,
                        modified_on = CURRENT_TIMESTAMP
                    WHERE initiative_product_id = @InitiativeProductId";

                using var connection = CreateConnection();
                var rowsAffected = await connection.ExecuteAsync(sql, product);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating initiative product: {ex.Message}", ex);
            }
        }

        #endregion

        #region Allocation Operations

        public async Task<IAllocationMaster> GetAllocationByIdAsync(string allocationNo)
        {
            try
            {
                var sql = @"
                    SELECT *,
                        CASE WHEN end_date >= CURRENT_DATE 
                        THEN CAST(end_date - CURRENT_DATE AS INTEGER)
                        ELSE 0 END as days_left
                    FROM allocation_master 
                    WHERE allocation_no = @AllocationNo";

                using var connection = CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<AllocationMaster>(sql, new { AllocationNo = allocationNo });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting allocation by ID: {ex.Message}", ex);
            }
        }

        public async Task<List<IAllocationMaster>> GetAllocationsAsync(string salesOrgCode, string brand, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var sql = @"
                    SELECT *,
                        CASE WHEN end_date >= CURRENT_DATE 
                        THEN CAST(end_date - CURRENT_DATE AS INTEGER)
                        ELSE 0 END as days_left
                    FROM allocation_master 
                    WHERE is_active = true";

                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(salesOrgCode))
                {
                    sql += " AND sales_org_code = @SalesOrgCode";
                    parameters.Add("SalesOrgCode", salesOrgCode);
                }

                if (!string.IsNullOrEmpty(brand))
                {
                    sql += " AND brand = @Brand";
                    parameters.Add("Brand", brand);
                }

                if (startDate.HasValue)
                {
                    sql += " AND start_date >= @StartDate";
                    parameters.Add("StartDate", startDate.Value);
                }

                if (endDate.HasValue)
                {
                    sql += " AND end_date <= @EndDate";
                    parameters.Add("EndDate", endDate.Value);
                }

                sql += " ORDER BY allocation_name";

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<AllocationMaster>(sql, parameters);
                return result.Cast<IAllocationMaster>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting allocations: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAllocationConsumedAmountAsync(string allocationNo)
        {
            try
            {
                var sql = @"
                    UPDATE allocation_master 
                    SET consumed_amount = (
                        SELECT COALESCE(SUM(contract_amount), 0)
                        FROM initiatives 
                        WHERE allocation_no = @AllocationNo 
                        AND status NOT IN ('cancelled', 'draft')
                    ),
                    available_allocation_amount = total_allocation_amount - (
                        SELECT COALESCE(SUM(contract_amount), 0)
                        FROM initiatives 
                        WHERE allocation_no = @AllocationNo 
                        AND status NOT IN ('cancelled', 'draft')
                    )
                    WHERE allocation_no = @AllocationNo";

                using var connection = CreateConnection();
                var rowsAffected = await connection.ExecuteAsync(sql, new { AllocationNo = allocationNo });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating allocation consumed amount: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetAvailableAllocationAmountAsync(string allocationNo)
        {
            try
            {
                var sql = @"
                    SELECT available_allocation_amount 
                    FROM allocation_master 
                    WHERE allocation_no = @AllocationNo";

                using var connection = CreateConnection();
                return await connection.QuerySingleOrDefaultAsync<decimal>(sql, new { AllocationNo = allocationNo });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting available allocation amount: {ex.Message}", ex);
            }
        }

        #endregion

        #region Validation Operations

        public async Task<bool> ValidateContractAmountAsync(string allocationNo, decimal contractAmount, int? initiativeId = null)
        {
            try
            {
                var sql = @"
                    SELECT 
                        am.available_allocation_amount + COALESCE(i.contract_amount, 0) as total_available
                    FROM allocation_master am
                    LEFT JOIN initiatives i ON i.initiative_id = @InitiativeId
                    WHERE am.allocation_no = @AllocationNo";

                using var connection = CreateConnection();
                var totalAvailable = await connection.QuerySingleOrDefaultAsync<decimal>(sql, new 
                { 
                    AllocationNo = allocationNo, 
                    InitiativeId = initiativeId 
                });

                return contractAmount <= totalAvailable;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating contract amount: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsInitiativeNameUniqueAsync(string name, int? initiativeId = null)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*) 
                    FROM initiatives 
                    WHERE LOWER(name) = LOWER(@Name)";

                var parameters = new DynamicParameters();
                parameters.Add("Name", name);

                if (initiativeId.HasValue)
                {
                    sql += " AND initiative_id != @InitiativeId";
                    parameters.Add("InitiativeId", initiativeId.Value);
                }

                using var connection = CreateConnection();
                var count = await connection.QuerySingleAsync<int>(sql, parameters);
                return count == 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking initiative name uniqueness: {ex.Message}", ex);
            }
        }

        public async Task<bool> CanEditInitiativeAsync(int initiativeId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*) 
                    FROM initiatives 
                    WHERE initiative_id = @InitiativeId 
                    AND status = 'draft'";

                using var connection = CreateConnection();
                var count = await connection.QuerySingleAsync<int>(sql, new { InitiativeId = initiativeId });
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if initiative can be edited: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
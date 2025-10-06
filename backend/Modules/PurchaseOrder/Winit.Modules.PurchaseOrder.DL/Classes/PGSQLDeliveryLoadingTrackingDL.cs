using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using Dapper;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class PGSQLDeliveryLoadingTrackingDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IDeliveryLoadingTrackingDL
{
    public PGSQLDeliveryLoadingTrackingDL(IServiceProvider serviceProvider, IConfiguration config)
        : base(serviceProvider, config)
    {
    }

    public async Task<List<IDeliveryLoadingTracking>> GetByStatusAsync(string status)
    {
        try
        {
            string sql = @"
                SELECT
                    dlt.""UID""::text as ""UID"",
                    dlt.""PurchaseOrderUID""::text as ""PurchaseOrderUID"",
                    dlt.""VehicleUID"",
                    dlt.""DriverEmployeeUID"",
                    dlt.""ForkLiftOperatorUID"",
                    dlt.""SecurityOfficerUID"",
                    dlt.""ArrivalTime"",
                    dlt.""LoadingStartTime"",
                    dlt.""LoadingEndTime"",
                    dlt.""DepartureTime"",
                    dlt.""LogisticsSignature"",
                    dlt.""DriverSignature"",
                    dlt.""Notes"",
                    dlt.""DeliveryNoteFilePath"",
                    dlt.""DeliveryNoteNumber"",
                    dlt.""IsActive"",
                    dlt.""CreatedDate"",
                    dlt.""CreatedBy""::text as ""CreatedBy"",
                    dlt.""ModifiedDate"",
                    dlt.""ModifiedBy""::text as ""ModifiedBy"",
                    poh.order_number,
                    poh.order_date,
                    poh.warehouse_uid,
                    poh.status,
                    org.name as OrgName
                FROM public.""DeliveryLoadingTracking"" dlt
                INNER JOIN public.purchase_order_header poh ON dlt.""PurchaseOrderUID""::text = poh.uid
                LEFT JOIN public.org org ON poh.org_uid = org.uid
                WHERE poh.status = @Status
                AND dlt.""IsActive"" = true
                ORDER BY dlt.""CreatedDate"" DESC";

            var parameters = new { Status = status };

            using var connection = new NpgsqlConnection(_connectionString);
            var results = await connection.QueryAsync<DeliveryLoadingTracking>(sql, parameters);
            return results.Cast<IDeliveryLoadingTracking>().ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetByStatusAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<IDeliveryLoadingTracking?> GetByPurchaseOrderUIDAsync(string purchaseOrderUID)
    {
        try
        {
            string sql = @"
                SELECT
                    dlt.""UID""::text as ""UID"",
                    dlt.""PurchaseOrderUID""::text as ""PurchaseOrderUID"",
                    dlt.""VehicleUID"",
                    dlt.""DriverEmployeeUID"",
                    dlt.""ForkLiftOperatorUID"",
                    dlt.""SecurityOfficerUID"",
                    dlt.""ArrivalTime"",
                    dlt.""LoadingStartTime"",
                    dlt.""LoadingEndTime"",
                    dlt.""DepartureTime"",
                    dlt.""LogisticsSignature"",
                    dlt.""DriverSignature"",
                    dlt.""Notes"",
                    dlt.""DeliveryNoteFilePath"",
                    dlt.""DeliveryNoteNumber"",
                    dlt.""IsActive"",
                    dlt.""CreatedDate"",
                    dlt.""CreatedBy""::text as ""CreatedBy"",
                    dlt.""ModifiedDate"",
                    dlt.""ModifiedBy""::text as ""ModifiedBy"",
                    poh.order_number,
                    poh.order_date,
                    poh.warehouse_uid,
                    poh.status,
                    org.name as OrgName
                FROM public.""DeliveryLoadingTracking"" dlt
                INNER JOIN public.purchase_order_header poh ON dlt.""PurchaseOrderUID""::text = poh.uid
                LEFT JOIN public.org org ON poh.org_uid = org.uid
                WHERE dlt.""PurchaseOrderUID""::text = @PurchaseOrderUID
                AND dlt.""IsActive"" = true
                ORDER BY dlt.""CreatedDate"" DESC
                LIMIT 1";

            var parameters = new
            {
                PurchaseOrderUID = purchaseOrderUID
            };

            // Use direct Dapper query with concrete class to avoid interface mapping issues
            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<DeliveryLoadingTracking>(sql, parameters);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetByPurchaseOrderUIDAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> SaveDeliveryLoadingTrackingAsync(IDeliveryLoadingTracking deliveryLoadingTracking)
    {
        try
        {
            string sql = @"
                INSERT INTO public.""DeliveryLoadingTracking""
                (
                    ""UID"",
                    ""PurchaseOrderUID"",
                    ""VehicleUID"",
                    ""DriverEmployeeUID"",
                    ""ForkLiftOperatorUID"",
                    ""SecurityOfficerUID"",
                    ""ArrivalTime"",
                    ""LoadingStartTime"",
                    ""LoadingEndTime"",
                    ""DepartureTime"",
                    ""LogisticsSignature"",
                    ""DriverSignature"",
                    ""Notes"",
                    ""DeliveryNoteFilePath"",
                    ""DeliveryNoteNumber"",
                    ""IsActive"",
                    ""CreatedDate"",
                    ""CreatedBy""
                )
                VALUES
                (
                    @UID,
                    @PurchaseOrderUID,
                    @VehicleUID,
                    @DriverEmployeeUID,
                    @ForkLiftOperatorUID,
                    @SecurityOfficerUID,
                    @ArrivalTime,
                    @LoadingStartTime,
                    @LoadingEndTime,
                    @DepartureTime,
                    @LogisticsSignature,
                    @DriverSignature,
                    @Notes,
                    @DeliveryNoteFilePath,
                    @DeliveryNoteNumber,
                    @IsActive,
                    @CreatedDate,
                    @CreatedBy
                )";

            var uid = string.IsNullOrEmpty(deliveryLoadingTracking.UID) ? Guid.NewGuid() : Guid.Parse(deliveryLoadingTracking.UID);

            var parameters = new
            {
                UID = uid,
                PurchaseOrderUID = Guid.Parse(deliveryLoadingTracking.PurchaseOrderUID),
                VehicleUID = string.IsNullOrWhiteSpace(deliveryLoadingTracking.VehicleUID) ? null : deliveryLoadingTracking.VehicleUID,
                DriverEmployeeUID = string.IsNullOrWhiteSpace(deliveryLoadingTracking.DriverEmployeeUID) ? null : deliveryLoadingTracking.DriverEmployeeUID,
                ForkLiftOperatorUID = string.IsNullOrWhiteSpace(deliveryLoadingTracking.ForkLiftOperatorUID) ? null : deliveryLoadingTracking.ForkLiftOperatorUID,
                SecurityOfficerUID = string.IsNullOrWhiteSpace(deliveryLoadingTracking.SecurityOfficerUID) ? null : deliveryLoadingTracking.SecurityOfficerUID,
                ArrivalTime = deliveryLoadingTracking.ArrivalTime,
                LoadingStartTime = deliveryLoadingTracking.LoadingStartTime,
                LoadingEndTime = deliveryLoadingTracking.LoadingEndTime,
                DepartureTime = deliveryLoadingTracking.DepartureTime,
                LogisticsSignature = deliveryLoadingTracking.LogisticsSignature,
                DriverSignature = deliveryLoadingTracking.DriverSignature,
                Notes = deliveryLoadingTracking.Notes,
                DeliveryNoteFilePath = deliveryLoadingTracking.DeliveryNoteFilePath,
                DeliveryNoteNumber = deliveryLoadingTracking.DeliveryNoteNumber,
                IsActive = deliveryLoadingTracking.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = string.IsNullOrWhiteSpace(deliveryLoadingTracking.CreatedBy) ? null : deliveryLoadingTracking.CreatedBy
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveDeliveryLoadingTrackingAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateDeliveryLoadingTrackingAsync(IDeliveryLoadingTracking deliveryLoadingTracking)
    {
        try
        {
            string sql = @"
                UPDATE public.""DeliveryLoadingTracking""
                SET
                    ""VehicleUID"" = @VehicleUID,
                    ""DriverEmployeeUID"" = @DriverEmployeeUID,
                    ""ForkLiftOperatorUID"" = @ForkLiftOperatorUID,
                    ""SecurityOfficerUID"" = @SecurityOfficerUID,
                    ""ArrivalTime"" = @ArrivalTime,
                    ""LoadingStartTime"" = @LoadingStartTime,
                    ""LoadingEndTime"" = @LoadingEndTime,
                    ""DepartureTime"" = @DepartureTime,
                    ""LogisticsSignature"" = @LogisticsSignature,
                    ""DriverSignature"" = @DriverSignature,
                    ""Notes"" = @Notes,
                    ""DeliveryNoteFilePath"" = @DeliveryNoteFilePath,
                    ""DeliveryNoteNumber"" = @DeliveryNoteNumber,
                    ""ModifiedDate"" = @ModifiedDate,
                    ""ModifiedBy"" = @ModifiedBy
                WHERE ""UID"" = @UID";

            var parameters = new
            {
                UID = Guid.Parse(deliveryLoadingTracking.UID),
                VehicleUID = string.IsNullOrWhiteSpace(deliveryLoadingTracking.VehicleUID) ? null : deliveryLoadingTracking.VehicleUID,
                DriverEmployeeUID = string.IsNullOrWhiteSpace(deliveryLoadingTracking.DriverEmployeeUID) ? null : deliveryLoadingTracking.DriverEmployeeUID,
                ForkLiftOperatorUID = string.IsNullOrWhiteSpace(deliveryLoadingTracking.ForkLiftOperatorUID) ? null : deliveryLoadingTracking.ForkLiftOperatorUID,
                SecurityOfficerUID = string.IsNullOrWhiteSpace(deliveryLoadingTracking.SecurityOfficerUID) ? null : deliveryLoadingTracking.SecurityOfficerUID,
                ArrivalTime = deliveryLoadingTracking.ArrivalTime,
                LoadingStartTime = deliveryLoadingTracking.LoadingStartTime,
                LoadingEndTime = deliveryLoadingTracking.LoadingEndTime,
                DepartureTime = deliveryLoadingTracking.DepartureTime,
                LogisticsSignature = deliveryLoadingTracking.LogisticsSignature,
                DriverSignature = deliveryLoadingTracking.DriverSignature,
                Notes = deliveryLoadingTracking.Notes,
                DeliveryNoteFilePath = deliveryLoadingTracking.DeliveryNoteFilePath,
                DeliveryNoteNumber = deliveryLoadingTracking.DeliveryNoteNumber,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = string.IsNullOrWhiteSpace(deliveryLoadingTracking.ModifiedBy) ? null : deliveryLoadingTracking.ModifiedBy
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateDeliveryLoadingTrackingAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdatePurchaseOrderStatusAsync(string purchaseOrderUID, string status)
    {
        try
        {
            string sql = @"
                UPDATE purchase_order_header
                SET status = @Status
                WHERE uid = @UID";

            var parameters = new
            {
                UID = purchaseOrderUID,
                Status = status
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdatePurchaseOrderStatusAsync: {ex.Message}");
            throw;
        }
    }
}

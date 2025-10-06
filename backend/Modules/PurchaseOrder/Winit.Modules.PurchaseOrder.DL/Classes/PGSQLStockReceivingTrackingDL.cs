using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using Dapper;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class PGSQLStockReceivingTrackingDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IStockReceivingTrackingDL
{
    public PGSQLStockReceivingTrackingDL(IServiceProvider serviceProvider, IConfiguration config)
        : base(serviceProvider, config)
    {
    }

    public async Task<IStockReceivingTracking?> GetByPurchaseOrderUIDAsync(string purchaseOrderUID)
    {
        try
        {
            string sql = @"
                SELECT
                    ""UID""::text as ""UID"",
                    ""PurchaseOrderUID""::text as ""PurchaseOrderUID"",
                    ""ReceiverName"",
                    ""ReceiverEmployeeCode"",
                    ""ForkLiftOperatorUID"",
                    ""LoadEmptyStockEmployeeUID"",
                    ""GetpassEmployeeUID"",
                    ""ArrivalTime"",
                    ""UnloadingStartTime"",
                    ""UnloadingEndTime"",
                    ""LoadEmptyStockTime"",
                    ""GetpassTime"",
                    ""PhysicalCountStartTime"",
                    ""PhysicalCountEndTime"",
                    ""ReceiverSignature"",
                    ""Notes"",
                    ""IsActive"",
                    ""CreatedDate"",
                    ""CreatedBy""::text as ""CreatedBy"",
                    ""ModifiedDate"",
                    ""ModifiedBy""::text as ""ModifiedBy""
                FROM public.""StockReceivingTracking""
                WHERE ""PurchaseOrderUID"" = @PurchaseOrderUID
                AND ""IsActive"" = true
                ORDER BY ""CreatedDate"" DESC
                LIMIT 1";

            var parameters = new
            {
                PurchaseOrderUID = purchaseOrderUID
            };

            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<StockReceivingTracking>(sql, parameters);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetByPurchaseOrderUIDAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<IStockReceivingTracking>> GetAllAsync()
    {
        try
        {
            string sql = @"
                SELECT
                    srt.""UID""::text as ""UID"",
                    srt.""PurchaseOrderUID""::text as ""PurchaseOrderUID"",
                    srt.""ReceiverName"",
                    srt.""ReceiverEmployeeCode"",
                    srt.""ForkLiftOperatorUID"",
                    srt.""LoadEmptyStockEmployeeUID"",
                    srt.""GetpassEmployeeUID"",
                    srt.""ArrivalTime"",
                    srt.""UnloadingStartTime"",
                    srt.""UnloadingEndTime"",
                    srt.""LoadEmptyStockTime"",
                    srt.""GetpassTime"",
                    srt.""PhysicalCountStartTime"",
                    srt.""PhysicalCountEndTime"",
                    srt.""ReceiverSignature"",
                    srt.""Notes"",
                    srt.""IsActive"",
                    srt.""CreatedDate"",
                    srt.""CreatedBy""::text as ""CreatedBy"",
                    srt.""ModifiedDate"",
                    srt.""ModifiedBy""::text as ""ModifiedBy"",
                    dlt.""DeliveryNoteNumber"" as ""DeliveryNoteNumber"",
                    poh.order_number,
                    poh.order_date
                FROM public.""StockReceivingTracking"" srt
                LEFT JOIN public.""DeliveryLoadingTracking"" dlt ON dlt.""PurchaseOrderUID""::text = srt.""PurchaseOrderUID""
                LEFT JOIN purchase_order_header poh ON poh.uid::text = srt.""PurchaseOrderUID""
                WHERE srt.""IsActive"" = true
                ORDER BY srt.""CreatedDate"" DESC";

            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryAsync<StockReceivingTracking>(sql);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> SaveStockReceivingTrackingAsync(IStockReceivingTracking stockReceivingTracking)
    {
        try
        {
            string sql = @"
                INSERT INTO public.""StockReceivingTracking""
                (
                    ""UID"",
                    ""PurchaseOrderUID"",
                    ""ReceiverName"",
                    ""ReceiverEmployeeCode"",
                    ""ForkLiftOperatorUID"",
                    ""LoadEmptyStockEmployeeUID"",
                    ""GetpassEmployeeUID"",
                    ""ArrivalTime"",
                    ""UnloadingStartTime"",
                    ""UnloadingEndTime"",
                    ""LoadEmptyStockTime"",
                    ""GetpassTime"",
                    ""PhysicalCountStartTime"",
                    ""PhysicalCountEndTime"",
                    ""ReceiverSignature"",
                    ""Notes"",
                    ""IsActive"",
                    ""CreatedDate"",
                    ""CreatedBy""
                )
                VALUES
                (
                    @UID,
                    @PurchaseOrderUID,
                    @ReceiverName,
                    @ReceiverEmployeeCode,
                    @ForkLiftOperatorUID,
                    @LoadEmptyStockEmployeeUID,
                    @GetpassEmployeeUID,
                    @ArrivalTime,
                    @UnloadingStartTime,
                    @UnloadingEndTime,
                    @LoadEmptyStockTime,
                    @GetpassTime,
                    @PhysicalCountStartTime,
                    @PhysicalCountEndTime,
                    @ReceiverSignature,
                    @Notes,
                    @IsActive,
                    @CreatedDate,
                    @CreatedBy
                )";

            var uid = string.IsNullOrEmpty(stockReceivingTracking.UID) ? Guid.NewGuid() : Guid.Parse(stockReceivingTracking.UID);

            var parameters = new
            {
                UID = uid,
                PurchaseOrderUID = stockReceivingTracking.PurchaseOrderUID,
                ReceiverName = stockReceivingTracking.ReceiverName,
                ReceiverEmployeeCode = stockReceivingTracking.ReceiverEmployeeCode,
                ForkLiftOperatorUID = stockReceivingTracking.ForkLiftOperatorUID,
                LoadEmptyStockEmployeeUID = stockReceivingTracking.LoadEmptyStockEmployeeUID,
                GetpassEmployeeUID = stockReceivingTracking.GetpassEmployeeUID,
                ArrivalTime = stockReceivingTracking.ArrivalTime,
                UnloadingStartTime = stockReceivingTracking.UnloadingStartTime,
                UnloadingEndTime = stockReceivingTracking.UnloadingEndTime,
                LoadEmptyStockTime = stockReceivingTracking.LoadEmptyStockTime,
                GetpassTime = stockReceivingTracking.GetpassTime,
                PhysicalCountStartTime = stockReceivingTracking.PhysicalCountStartTime,
                PhysicalCountEndTime = stockReceivingTracking.PhysicalCountEndTime,
                ReceiverSignature = stockReceivingTracking.ReceiverSignature,
                Notes = stockReceivingTracking.Notes,
                IsActive = stockReceivingTracking.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = stockReceivingTracking.CreatedBy
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveStockReceivingTrackingAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateStockReceivingTrackingAsync(IStockReceivingTracking stockReceivingTracking)
    {
        try
        {
            string sql = @"
                UPDATE public.""StockReceivingTracking""
                SET
                    ""ReceiverName"" = @ReceiverName,
                    ""ReceiverEmployeeCode"" = @ReceiverEmployeeCode,
                    ""ForkLiftOperatorUID"" = @ForkLiftOperatorUID,
                    ""LoadEmptyStockEmployeeUID"" = @LoadEmptyStockEmployeeUID,
                    ""GetpassEmployeeUID"" = @GetpassEmployeeUID,
                    ""ArrivalTime"" = @ArrivalTime,
                    ""UnloadingStartTime"" = @UnloadingStartTime,
                    ""UnloadingEndTime"" = @UnloadingEndTime,
                    ""LoadEmptyStockTime"" = @LoadEmptyStockTime,
                    ""GetpassTime"" = @GetpassTime,
                    ""PhysicalCountStartTime"" = @PhysicalCountStartTime,
                    ""PhysicalCountEndTime"" = @PhysicalCountEndTime,
                    ""ReceiverSignature"" = @ReceiverSignature,
                    ""Notes"" = @Notes,
                    ""ModifiedDate"" = @ModifiedDate,
                    ""ModifiedBy"" = @ModifiedBy
                WHERE ""UID"" = @UID";

            var parameters = new
            {
                UID = Guid.Parse(stockReceivingTracking.UID),
                ReceiverName = stockReceivingTracking.ReceiverName,
                ReceiverEmployeeCode = stockReceivingTracking.ReceiverEmployeeCode,
                ForkLiftOperatorUID = stockReceivingTracking.ForkLiftOperatorUID,
                LoadEmptyStockEmployeeUID = stockReceivingTracking.LoadEmptyStockEmployeeUID,
                GetpassEmployeeUID = stockReceivingTracking.GetpassEmployeeUID,
                ArrivalTime = stockReceivingTracking.ArrivalTime,
                UnloadingStartTime = stockReceivingTracking.UnloadingStartTime,
                UnloadingEndTime = stockReceivingTracking.UnloadingEndTime,
                LoadEmptyStockTime = stockReceivingTracking.LoadEmptyStockTime,
                GetpassTime = stockReceivingTracking.GetpassTime,
                PhysicalCountStartTime = stockReceivingTracking.PhysicalCountStartTime,
                PhysicalCountEndTime = stockReceivingTracking.PhysicalCountEndTime,
                ReceiverSignature = stockReceivingTracking.ReceiverSignature,
                Notes = stockReceivingTracking.Notes,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = stockReceivingTracking.ModifiedBy
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateStockReceivingTrackingAsync: {ex.Message}");
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

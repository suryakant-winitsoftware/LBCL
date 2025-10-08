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

    public async Task<IStockReceivingTracking?> GetByWHStockRequestUIDAsync(string whStockRequestUID)
    {
        try
        {
            string sql = @"
                SELECT
                    ""UID""::text as ""UID"",
                    ""WHStockRequestUID"" as ""WHStockRequestUID"",
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
                    ""Status"",
                    ""IsActive"",
                    ""CreatedDate"",
                    ""CreatedBy""::text as ""CreatedBy"",
                    ""ModifiedDate"",
                    ""ModifiedBy""::text as ""ModifiedBy""
                FROM public.stock_receiving_tracking
                WHERE ""WHStockRequestUID"" = @WHStockRequestUID
                AND ""IsActive"" = true
                ORDER BY ""CreatedDate"" DESC
                LIMIT 1";

            var parameters = new
            {
                WHStockRequestUID = whStockRequestUID
            };

            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<StockReceivingTracking>(sql, parameters);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetByWHStockRequestUIDAsync: {ex.Message}");
            throw;
        }
    }

    // Keep old method for backward compatibility
    public async Task<IStockReceivingTracking?> GetByPurchaseOrderUIDAsync(string purchaseOrderUID)
    {
        return await GetByWHStockRequestUIDAsync(purchaseOrderUID);
    }

    public async Task<IEnumerable<IStockReceivingTracking>> GetAllAsync()
    {
        try
        {
            string sql = @"
                SELECT
                    srt.""UID""::text as ""UID"",
                    srt.""WHStockRequestUID"" as ""WHStockRequestUID"",
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
                    srt.""Status"",
                    srt.""IsActive"",
                    srt.""CreatedDate"",
                    srt.""CreatedBy""::text as ""CreatedBy"",
                    srt.""ModifiedDate"",
                    srt.""ModifiedBy""::text as ""ModifiedBy"",
                    dlt.""DeliveryNoteNumber"" as ""DeliveryNoteNumber"",
                    wsr.code as request_code,
                    wsr.created_time
                FROM public.stock_receiving_tracking srt
                LEFT JOIN public.""DeliveryLoadingTracking"" dlt ON dlt.""WHStockRequestUID"" = srt.""WHStockRequestUID""
                LEFT JOIN wh_stock_request wsr ON wsr.uid = srt.""WHStockRequestUID""
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
                INSERT INTO public.stock_receiving_tracking
                (
                    ""UID"",
                    ""WHStockRequestUID"",
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
                    ""Status"",
                    ""IsActive"",
                    ""CreatedDate"",
                    ""CreatedBy""
                )
                VALUES
                (
                    @UID,
                    @WHStockRequestUID,
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
                    @Status,
                    @IsActive,
                    @CreatedDate,
                    @CreatedBy
                )";

            var uid = string.IsNullOrEmpty(stockReceivingTracking.UID) ? Guid.NewGuid() : Guid.Parse(stockReceivingTracking.UID);

            var parameters = new
            {
                UID = uid,
                WHStockRequestUID = stockReceivingTracking.WHStockRequestUID,
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
                Status = string.IsNullOrWhiteSpace(stockReceivingTracking.Status) ? "PENDING" : stockReceivingTracking.Status,
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
                UPDATE public.stock_receiving_tracking
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
                    ""Status"" = @Status,
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
                Status = string.IsNullOrWhiteSpace(stockReceivingTracking.Status) ? "PENDING" : stockReceivingTracking.Status,
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

    public async Task<bool> UpdateWHStockRequestStatusAsync(string whStockRequestUID, string status)
    {
        try
        {
            string sql = @"
                UPDATE wh_stock_request
                SET status = @Status
                WHERE uid = @UID";

            var parameters = new
            {
                UID = whStockRequestUID,
                Status = status
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateWHStockRequestStatusAsync: {ex.Message}");
            throw;
        }
    }

    // Keep old method for backward compatibility
    public async Task<bool> UpdatePurchaseOrderStatusAsync(string purchaseOrderUID, string status)
    {
        return await UpdateWHStockRequestStatusAsync(purchaseOrderUID, status);
    }
}

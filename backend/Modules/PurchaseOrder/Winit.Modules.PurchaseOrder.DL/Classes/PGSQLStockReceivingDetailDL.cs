using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using Dapper;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class PGSQLStockReceivingDetailDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IStockReceivingDetailDL
{
    public PGSQLStockReceivingDetailDL(IServiceProvider serviceProvider, IConfiguration config)
        : base(serviceProvider, config)
    {
    }

    public async Task<IEnumerable<IStockReceivingDetail>> GetByWHStockRequestUIDAsync(string whStockRequestUID)
    {
        try
        {
            string sql = @"
                SELECT
                    ""UID""::text as ""UID"",
                    ""WHStockRequestUID"",
                    ""WHStockRequestLineUID""::text as ""WHStockRequestLineUID"",
                    ""SKUCode"",
                    ""SKUName"",
                    ""OrderedQty"",
                    ""ReceivedQty"",
                    ""AdjustmentReason"",
                    ""AdjustmentQty"",
                    ""ImageURL"",
                    ""IsActive"",
                    ""CreatedDate"",
                    ""CreatedBy""::text as ""CreatedBy"",
                    ""ModifiedDate"",
                    ""ModifiedBy""::text as ""ModifiedBy""
                FROM public.""StockReceivingDetail""
                WHERE ""WHStockRequestUID"" = @WHStockRequestUID
                AND ""IsActive"" = true
                ORDER BY ""CreatedDate"" ASC";

            var parameters = new
            {
                WHStockRequestUID = whStockRequestUID
            };

            using var connection = new NpgsqlConnection(_connectionString);
            var result = await connection.QueryAsync<StockReceivingDetail>(sql, parameters);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetByWHStockRequestUIDAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> SaveStockReceivingDetailsAsync(IEnumerable<IStockReceivingDetail> details)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                foreach (var detail in details)
                {
                    string sql = @"
                        INSERT INTO public.""StockReceivingDetail""
                        (
                            ""UID"",
                            ""WHStockRequestUID"",
                            ""WHStockRequestLineUID"",
                            ""SKUCode"",
                            ""SKUName"",
                            ""OrderedQty"",
                            ""ReceivedQty"",
                            ""AdjustmentReason"",
                            ""AdjustmentQty"",
                            ""ImageURL"",
                            ""IsActive"",
                            ""CreatedDate"",
                            ""CreatedBy""
                        )
                        VALUES
                        (
                            @UID,
                            @WHStockRequestUID,
                            @WHStockRequestLineUID,
                            @SKUCode,
                            @SKUName,
                            @OrderedQty,
                            @ReceivedQty,
                            @AdjustmentReason,
                            @AdjustmentQty,
                            @ImageURL,
                            @IsActive,
                            @CreatedDate,
                            @CreatedBy
                        )";

                    var uid = string.IsNullOrEmpty(detail.UID) ? Guid.NewGuid() : Guid.Parse(detail.UID);

                    var parameters = new
                    {
                        UID = uid,
                        WHStockRequestUID = detail.WHStockRequestUID,
                        WHStockRequestLineUID = detail.WHStockRequestLineUID,
                        SKUCode = detail.SKUCode,
                        SKUName = detail.SKUName,
                        OrderedQty = detail.OrderedQty,
                        ReceivedQty = detail.ReceivedQty,
                        AdjustmentReason = detail.AdjustmentReason,
                        AdjustmentQty = detail.AdjustmentQty,
                        ImageURL = detail.ImageURL,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = detail.CreatedBy
                    };

                    await connection.ExecuteAsync(sql, parameters, transaction);
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SaveStockReceivingDetailsAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteByWHStockRequestUIDAsync(string whStockRequestUID)
    {
        try
        {
            string sql = @"
                UPDATE public.""StockReceivingDetail""
                SET ""IsActive"" = false,
                    ""ModifiedDate"" = @ModifiedDate
                WHERE ""WHStockRequestUID"" = @WHStockRequestUID";

            var parameters = new
            {
                WHStockRequestUID = whStockRequestUID,
                ModifiedDate = DateTime.UtcNow
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DeleteByWHStockRequestUIDAsync: {ex.Message}");
            throw;
        }
    }
}

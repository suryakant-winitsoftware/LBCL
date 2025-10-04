using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Winit.Modules.Target.Model.Classes;
using Winit.Modules.Target.Model.Interfaces;
using Winit.Modules.Target.DL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Winit.Modules.Target.DL.Classes
{
    public class PGSQLTargetDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ITargetDL
    {
        public PGSQLTargetDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<IEnumerable<ITarget>> GetAllTargetsAsync(TargetFilter filter)
        {
            var query = @"
                SELECT id, user_linked_type as UserLinkedType, user_linked_uid as UserLinkedUid,
                       customer_linked_type as CustomerLinkedType,
                       customer_linked_uid as CustomerLinkedUid, item_linked_item_type as ItemLinkedItemType,
                       item_linked_item_uid as ItemLinkedItemUid, target_month as TargetMonth,
                       target_year as TargetYear, target_amount as TargetAmount,
                       status as Status, notes as Notes,
                       created_time as CreatedTime, created_by as CreatedBy,
                       modified_time as ModifiedTime, modified_by as ModifiedBy
                FROM targets
                WHERE 1=1";

            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(filter.UserLinkedType))
            {
                query += " AND user_linked_type = @UserLinkedType";
                parameters.Add("@UserLinkedType", filter.UserLinkedType);
            }

            if (!string.IsNullOrEmpty(filter.UserLinkedUid))
            {
                query += " AND user_linked_uid = @UserLinkedUid";
                parameters.Add("@UserLinkedUid", filter.UserLinkedUid);
            }

            if (!string.IsNullOrEmpty(filter.CustomerLinkedType))
            {
                query += " AND customer_linked_type = @CustomerLinkedType";
                parameters.Add("@CustomerLinkedType", filter.CustomerLinkedType);
            }

            if (!string.IsNullOrEmpty(filter.CustomerLinkedUid))
            {
                query += " AND customer_linked_uid = @CustomerLinkedUid";
                parameters.Add("@CustomerLinkedUid", filter.CustomerLinkedUid);
            }

            if (!string.IsNullOrEmpty(filter.ItemLinkedItemType))
            {
                query += " AND item_linked_item_type = @ItemLinkedItemType";
                parameters.Add("@ItemLinkedItemType", filter.ItemLinkedItemType);
            }

            if (filter.TargetMonth.HasValue)
            {
                query += " AND target_month = @TargetMonth";
                parameters.Add("@TargetMonth", filter.TargetMonth.Value);
            }

            if (filter.TargetYear.HasValue)
            {
                query += " AND target_year = @TargetYear";
                parameters.Add("@TargetYear", filter.TargetYear.Value);
            }

            query += " ORDER BY id DESC";

            var result = await ExecuteQueryAsync<Winit.Modules.Target.Model.Classes.Target>(query, parameters);
            return result.Cast<ITarget>();
        }

        public async Task<ITarget> GetTargetByIdAsync(long id)
        {
            var query = @"
                SELECT id, user_linked_type as UserLinkedType, user_linked_uid as UserLinkedUid,
                       customer_linked_type as CustomerLinkedType,
                       customer_linked_uid as CustomerLinkedUid, item_linked_item_type as ItemLinkedItemType,
                       item_linked_item_uid as ItemLinkedItemUid, target_month as TargetMonth,
                       target_year as TargetYear, target_amount as TargetAmount,
                       status as Status, notes as Notes,
                       created_time as CreatedTime, created_by as CreatedBy,
                       modified_time as ModifiedTime, modified_by as ModifiedBy
                FROM targets
                WHERE id = @Id";

            var parameters = new Dictionary<string, object> { { "@Id", id } };
            var result = await ExecuteQueryAsync<Winit.Modules.Target.Model.Classes.Target>(query, parameters);
            return result?.FirstOrDefault();
        }

        public async Task<ITarget> CreateTargetAsync(ITarget target)
        {
            var query = @"
                INSERT INTO targets (user_linked_type, user_linked_uid,
                                   customer_linked_type, customer_linked_uid,
                                   item_linked_item_type, item_linked_item_uid,
                                   target_month, target_year, target_amount,
                                   status, notes,
                                   created_time, created_by, modified_time, modified_by)
                VALUES (@UserLinkedType, @UserLinkedUid,
                        @CustomerLinkedType, @CustomerLinkedUid,
                        @ItemLinkedItemType, @ItemLinkedItemUid,
                        @TargetMonth, @TargetYear, @TargetAmount,
                        @Status, @Notes,
                        @CreatedTime, @CreatedBy, @ModifiedTime, @ModifiedBy)
                RETURNING id";

            var parameters = new Dictionary<string, object>
            {
                { "@UserLinkedType", target.UserLinkedType },
                { "@UserLinkedUid", target.UserLinkedUid },
                { "@CustomerLinkedType", target.CustomerLinkedType ?? (object)DBNull.Value },
                { "@CustomerLinkedUid", target.CustomerLinkedUid ?? (object)DBNull.Value },
                { "@ItemLinkedItemType", target.ItemLinkedItemType ?? (object)DBNull.Value },
                { "@ItemLinkedItemUid", target.ItemLinkedItemUid ?? (object)DBNull.Value },
                { "@TargetMonth", target.TargetMonth },
                { "@TargetYear", target.TargetYear },
                { "@TargetAmount", target.TargetAmount },
                { "@Status", target.Status ?? (object)DBNull.Value },
                { "@Notes", target.Notes ?? (object)DBNull.Value },
                { "@CreatedTime", target.CreatedTime ?? (object)DBNull.Value },
                { "@CreatedBy", target.CreatedBy ?? (object)DBNull.Value },
                { "@ModifiedTime", target.ModifiedTime ?? (object)DBNull.Value },
                { "@ModifiedBy", target.ModifiedBy ?? (object)DBNull.Value }
            };

            var result = await ExecuteScalarAsync<long>(query, parameters);
            target.Id = result;
            return target;
        }

        public async Task<ITarget> UpdateTargetAsync(ITarget target)
        {
            var query = @"
                UPDATE targets 
                SET user_linked_type = @UserLinkedType,
                    user_linked_uid = @UserLinkedUid,
                    customer_linked_type = @CustomerLinkedType,
                    customer_linked_uid = @CustomerLinkedUid,
                    item_linked_item_type = @ItemLinkedItemType,
                    item_linked_item_uid = @ItemLinkedItemUid,
                    target_month = @TargetMonth,
                    target_year = @TargetYear,
                    target_amount = @TargetAmount,
                    status = @Status,
                    notes = @Notes,
                    modified_time = @ModifiedTime,
                    modified_by = @ModifiedBy
                WHERE id = @Id";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", target.Id },
                { "@UserLinkedType", target.UserLinkedType },
                { "@UserLinkedUid", target.UserLinkedUid },
                { "@CustomerLinkedType", target.CustomerLinkedType ?? (object)DBNull.Value },
                { "@CustomerLinkedUid", target.CustomerLinkedUid ?? (object)DBNull.Value },
                { "@ItemLinkedItemType", target.ItemLinkedItemType ?? (object)DBNull.Value },
                { "@ItemLinkedItemUid", target.ItemLinkedItemUid ?? (object)DBNull.Value },
                { "@TargetMonth", target.TargetMonth },
                { "@TargetYear", target.TargetYear },
                { "@TargetAmount", target.TargetAmount },
                { "@Status", target.Status ?? (object)DBNull.Value },
                { "@Notes", target.Notes ?? (object)DBNull.Value },
                { "@ModifiedTime", target.ModifiedTime ?? (object)DBNull.Value },
                { "@ModifiedBy", target.ModifiedBy ?? (object)DBNull.Value }
            };

            await ExecuteNonQueryAsync(query, parameters);
            return target;
        }

        public async Task<bool> DeleteTargetAsync(long id)
        {
            var query = "DELETE FROM targets WHERE id = @Id";
            var parameters = new Dictionary<string, object> { { "@Id", id } };
            var affected = await ExecuteNonQueryAsync(query, parameters);
            return affected > 0;
        }

        public async Task<int> BulkInsertTargetsAsync(IEnumerable<ITarget> targets)
        {
            var query = @"
                INSERT INTO targets (user_linked_type, user_linked_uid, customer_linked_type, customer_linked_uid,
                                   item_linked_item_type, item_linked_item_uid,
                                   target_month, target_year, target_amount,
                                   status, notes,
                                   created_time, created_by, modified_time, modified_by)
                VALUES (@UserLinkedType, @UserLinkedUid, @CustomerLinkedType, @CustomerLinkedUid,
                        @ItemLinkedItemType, @ItemLinkedItemUid,
                        @TargetMonth, @TargetYear, @TargetAmount,
                        @Status, @Notes,
                        @CreatedTime, @CreatedBy, @ModifiedTime, @ModifiedBy)";

            var result = 0;
            foreach (var target in targets)
            {
                // Use anonymous object for Dapper parameters - it handles nulls better
                var parameters = new 
                {
                    UserLinkedType = target.UserLinkedType,
                    UserLinkedUid = target.UserLinkedUid,
                    CustomerLinkedType = target.CustomerLinkedType,
                    CustomerLinkedUid = target.CustomerLinkedUid,
                    ItemLinkedItemType = target.ItemLinkedItemType,
                    ItemLinkedItemUid = target.ItemLinkedItemUid,
                    TargetMonth = target.TargetMonth,
                    TargetYear = target.TargetYear,
                    TargetAmount = target.TargetAmount,
                    Status = target.Status,
                    Notes = target.Notes,
                    CreatedTime = target.CreatedTime ?? DateTime.Now,
                    CreatedBy = target.CreatedBy ?? "SYSTEM",
                    ModifiedTime = target.ModifiedTime ?? DateTime.Now,
                    ModifiedBy = target.ModifiedBy ?? "SYSTEM"
                };
                await ExecuteNonQueryAsync(query, parameters);
                result++;
            }
            return result;
        }

        public async Task<IEnumerable<TargetSummary>> GetTargetSummaryAsync(string userLinkedUid, int year, int month)
        {
            var query = @"
                SELECT 
                    user_linked_uid as UserLinkedUid,
                    customer_linked_type as CustomerLinkedType,
                    customer_linked_uid as CustomerLinkedUid,
                    target_month as TargetMonth,
                    target_year as TargetYear,
                    SUM(target_amount) as TotalTarget,
                    SUM(CASE WHEN item_linked_item_type = 'Cosmetics' THEN target_amount ELSE 0 END) as CosmeticsTarget,
                    SUM(CASE WHEN item_linked_item_type = 'FMCG Non-Food' THEN target_amount ELSE 0 END) as FMCGNonFoodTarget,
                    SUM(CASE WHEN item_linked_item_type = 'FMCG Food' THEN target_amount ELSE 0 END) as FMCGFoodTarget
                FROM targets
                WHERE user_linked_uid = @UserLinkedUid
                    AND target_year = @Year
                    AND target_month = @Month
                GROUP BY user_linked_uid, customer_linked_type, customer_linked_uid, target_month, target_year";

            var parameters = new Dictionary<string, object>
            {
                { "@UserLinkedUid", userLinkedUid },
                { "@Year", year },
                { "@Month", month }
            };
            
            return await ExecuteQueryAsync<TargetSummary>(query, parameters);
        }

        public async Task<bool> DeleteTargetsByFilterAsync(string userLinkedUid, string? customerUid, int year, int month)
        {
            var query = @"
                DELETE FROM targets 
                WHERE user_linked_uid = @UserLinkedUid 
                    AND customer_linked_uid = @CustomerUid
                    AND target_year = @Year 
                    AND target_month = @Month";

            var parameters = new Dictionary<string, object>
            {
                { "@UserLinkedUid", userLinkedUid },
                { "@CustomerUid", customerUid ?? (object)DBNull.Value },
                { "@Year", year },
                { "@Month", month }
            };
            
            var affected = await ExecuteNonQueryAsync(query, parameters);
            
            return affected > 0;
        }

        public async Task<IEnumerable<ITarget>> GetPagedTargetsAsync(TargetFilter filter)
        {
            var query = @"
                SELECT id, user_linked_type as UserLinkedType, user_linked_uid as UserLinkedUid,
                       customer_linked_type as CustomerLinkedType,
                       customer_linked_uid as CustomerLinkedUid, item_linked_item_type as ItemLinkedItemType,
                       item_linked_item_uid as ItemLinkedItemUid, target_month as TargetMonth,
                       target_year as TargetYear, target_amount as TargetAmount,
                       status as Status, notes as Notes,
                       created_time as CreatedTime, created_by as CreatedBy,
                       modified_time as ModifiedTime, modified_by as ModifiedBy
                FROM targets
                WHERE 1=1";

            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(filter.UserLinkedType))
            {
                query += " AND user_linked_type = @UserLinkedType";
                parameters.Add("@UserLinkedType", filter.UserLinkedType);
            }

            if (!string.IsNullOrEmpty(filter.UserLinkedUid))
            {
                query += " AND user_linked_uid = @UserLinkedUid";
                parameters.Add("@UserLinkedUid", filter.UserLinkedUid);
            }

            if (!string.IsNullOrEmpty(filter.CustomerLinkedType))
            {
                query += " AND customer_linked_type = @CustomerLinkedType";
                parameters.Add("@CustomerLinkedType", filter.CustomerLinkedType);
            }

            if (!string.IsNullOrEmpty(filter.CustomerLinkedUid))
            {
                query += " AND customer_linked_uid = @CustomerLinkedUid";
                parameters.Add("@CustomerLinkedUid", filter.CustomerLinkedUid);
            }

            if (filter.TargetYear.HasValue)
            {
                query += " AND target_year = @TargetYear";
                parameters.Add("@TargetYear", filter.TargetYear.Value);
            }

            if (filter.TargetMonth.HasValue)
            {
                query += " AND target_month = @TargetMonth";
                parameters.Add("@TargetMonth", filter.TargetMonth.Value);
            }

            query += " ORDER BY id DESC";
            
            var offset = (filter.PageNumber - 1) * filter.PageSize;
            query += " LIMIT @PageSize OFFSET @Offset";
            parameters.Add("@PageSize", filter.PageSize);
            parameters.Add("@Offset", offset);

            var result = await ExecuteQueryAsync<Winit.Modules.Target.Model.Classes.Target>(query, parameters);
            return result.Cast<ITarget>();
        }

        public async Task<int> GetTargetsCountAsync(TargetFilter filter)
        {
            var query = "SELECT COUNT(*) FROM targets WHERE 1=1";
            var parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(filter.UserLinkedType))
            {
                query += " AND user_linked_type = @UserLinkedType";
                parameters.Add("@UserLinkedType", filter.UserLinkedType);
            }

            if (!string.IsNullOrEmpty(filter.UserLinkedUid))
            {
                query += " AND user_linked_uid = @UserLinkedUid";
                parameters.Add("@UserLinkedUid", filter.UserLinkedUid);
            }

            if (!string.IsNullOrEmpty(filter.CustomerLinkedType))
            {
                query += " AND customer_linked_type = @CustomerLinkedType";
                parameters.Add("@CustomerLinkedType", filter.CustomerLinkedType);
            }

            if (!string.IsNullOrEmpty(filter.CustomerLinkedUid))
            {
                query += " AND customer_linked_uid = @CustomerLinkedUid";
                parameters.Add("@CustomerLinkedUid", filter.CustomerLinkedUid);
            }

            if (filter.TargetYear.HasValue)
            {
                query += " AND target_year = @TargetYear";
                parameters.Add("@TargetYear", filter.TargetYear.Value);
            }

            if (filter.TargetMonth.HasValue)
            {
                query += " AND target_month = @TargetMonth";
                parameters.Add("@TargetMonth", filter.TargetMonth.Value);
            }

            return await ExecuteScalarAsync<int>(query, parameters);
        }
    }
}
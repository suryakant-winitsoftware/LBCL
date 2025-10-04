using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.DL.Classes
{
    public class PGSQLProductConfigDL : Base.DL.DBManager.PostgresDBManager, Interfaces.IProductConfigDL
    {
        public PGSQLProductConfigDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductConfig>> SelectProductConfigAll(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM ""ProductConfig""");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM ""ProductConfig""");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sql.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Product.Model.Interfaces.IProductConfig>(filterCriterias, sql, parameters);
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
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IProductConfig>().GetType();
                IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig> ProductConfigDetails = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductConfig>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductConfig> pagedResponse = new PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductConfig>
                {
                    PagedData = ProductConfigDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Product.Model.Interfaces.IProductConfig> SelectProductConfigByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT * FROM ""ProductConfig"" WHERE ""UID"" = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IProductConfig>().GetType();
            Winit.Modules.Product.Model.Interfaces.IProductConfig ProductConfigDetails = await ExecuteSingleAsync<Winit.Modules.Product.Model.Interfaces.IProductConfig>(sql, parameters, type);
            return ProductConfigDetails;
        }
        public async Task<int> CreateProductConfig(Model.Interfaces.IProductConfig createProductConfig)
        {
            var sql = @"INSERT INTO ""ProductConfig""(""UID"",""CreatedBy"",""CreatedTime"",""ModifiedBy"",""ModifiedTime"",""ServerAddTime"",""ServerModifiedTime"",
                      ""ProductCode"",""DistributionChannelOrgUID"",""CanBuy"",""CanSell"",""BuyingUOM"",""SellingUOM"",""IsActive"")
                    VALUES(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@ProductCode,@DistributionChannelOrgUID,
                     @CanBuy,@CanSell,@BuyingUOM,@SellingUOM,@IsActive);";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID", createProductConfig.UID},
                   {"CreatedBy", createProductConfig.CreatedBy},
                   {"CreatedTime", createProductConfig.CreatedTime},
                   {"ModifiedBy", createProductConfig.ModifiedBy},
                   {"ModifiedTime", createProductConfig.ModifiedTime},
                   {"ServerAddTime", createProductConfig.ServerAddTime},
                   {"ServerModifiedTime", createProductConfig.ServerModifiedTime},
                   {"ProductCode", createProductConfig.ProductCode},
                   {"DistributionChannelOrgUID", createProductConfig.DistributionChannelOrgUID},
                   {"CanBuy", createProductConfig.CanBuy},
                   {"CanSell", createProductConfig.CanSell},
                   {"BuyingUOM", createProductConfig.BuyingUOM},
                   {"SellingUOM", createProductConfig.SellingUOM},
                   {"IsActive", createProductConfig.IsActive},
             };
            return await ExecuteNonQueryAsync(sql, parameters);

        }
        public async Task<int> UpdateProductConfig(Model.Interfaces.IProductConfig updateProductConfig)
        {
            try
            {
                var sql = @"UPDATE ""ProductConfig"" SET ""ModifiedBy"" = @ModifiedBy, ""ModifiedTime"" = @ModifiedTime, ""ServerModifiedTime""= @ServerModifiedTime,
                    ""CanBuy"" = @CanBuy,""CanSell"" = @CanSell, ""BuyingUOM"" = @BuyingUOM,""SellingUOM"" = @SellingUOM, ""IsActive"" = @IsActive WHERE ""UID"" = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", updateProductConfig.UID},
                   {"ModifiedBy", updateProductConfig.ModifiedBy},
                   {"ModifiedTime", updateProductConfig.ModifiedTime},
                   {"ServerModifiedTime", updateProductConfig.ServerModifiedTime},
                   {"CanBuy", updateProductConfig.CanBuy},
                   {"CanSell", updateProductConfig.CanSell},
                   {"BuyingUOM", updateProductConfig.BuyingUOM},
                   {"SellingUOM", updateProductConfig.SellingUOM},
                   {"IsActive", updateProductConfig.IsActive},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteProductConfig(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  FROM ""ProductConfig"" WHERE ""UID"" = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            
            Dictionary<string, object> parameters = new Dictionary<string, object>();


            if (!string.IsNullOrEmpty(ProductCode))
            {
                parameters.Add("ProductCode", ProductCode);
            }
            else
            {
                parameters.Add("ProductCode", DBNull.Value);
            }

            if (CreatedTime != default(DateTime))
            {
                parameters.Add("CreatedTime", CreatedTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                parameters.Add("CreatedTime", DBNull.Value);
            }

            if (ModifiedTime != default(DateTime))
            {
                parameters.Add("ModifiedTime", ModifiedTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                parameters.Add("ModifiedTime", DBNull.Value);
            }

            //var sql = @"SELECT * FROM ProductConfig WHERE (ProductCode LIKE '%' || @ProductCode || '%' OR CreatedTime LIKE '%' || @CreatedTime || '%'  OR ModifiedTime LIKE '%' || @ModifiedTime || '%' )";


            var sql = "SELECT * FROM ProductConfig WHERE (ProductCode LIKE '%' + @ProductCode + '%' OR CreatedTime LIKE '%' + @CreatedTime + '%' OR ModifiedTime LIKE '%' + @ModifiedTime + '%' )";
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig> ProductConfiglist = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductConfig>(sql, parameters);
            return await Task.FromResult(ProductConfiglist);
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig>> GetProductConfigPaged(int pageNumber, int pageSize)
        {
            
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "SELECT * FROM ProductConfig ORDER BY (SELECT NULL) OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig> ProductConfigList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductConfig>(sql, parameters);
            return await Task.FromResult(ProductConfigList);

        }


        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig>> GetProductConfigSorted(List<SortCriteria> sortCriterias)
        {
          
            try
            {
                var sql = new StringBuilder("SELECT * FROM ProductConfig ORDER BY ");
                var sortParameters = new Dictionary<string, object>();

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    for (var i = 0; i < sortCriterias.Count; i++)
                    {
                        var columnName = $"{sortCriterias[i].SortParameter}";
                        //    var columnName = $"{i}";

                        sql.Append($"{columnName} {(sortCriterias[i].Direction == SortDirection.Asc ? "ASC" : "DESC")}");

                        if (i < sortCriterias.Count - 1)
                        {
                            sql.Append(", ");
                        }
                    }
                }

                IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfig> ProductConfigList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductConfig>(sql.ToString(), sortParameters);

                return ProductConfigList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}









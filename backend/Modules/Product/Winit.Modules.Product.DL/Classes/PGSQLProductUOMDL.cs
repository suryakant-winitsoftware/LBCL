using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.DL.Interfaces;
using Winit.Modules.Product.Model.Classes;
using Winit.Modules.Product.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.DL.Classes
{
    public class PGSQLProductUOMDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IProductUOMDL
    {
        public PGSQLProductUOMDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> SelectProductUOMAll()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };
            var sql = @"SELECT * FROM ""ProductUOM""";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Product.Model.Interfaces.IProductUOM>().GetType();
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM> ProductUOMList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductUOM>(sql, parameters, type);
            return await Task.FromResult(ProductUOMList);
        }

        public async Task<Winit.Modules.Product.Model.Interfaces.IProductUOM> GetProductUOMByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT * FROM ""ProductUOM"" WHERE ""UID"" = @UID";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Product.Model.Interfaces.IProductUOM>().GetType();
            Winit.Modules.Product.Model.Interfaces.IProductUOM ProductUOMList = await ExecuteSingleAsync<Winit.Modules.Product.Model.Interfaces.IProductUOM>(sql, parameters, type);
            return await Task.FromResult(ProductUOMList);
        }

        public async Task<int> CreateProductUOM(Winit.Modules.Product.Model.Interfaces.IProductUOM productUOM)
        {
            var sql = @"INSERT INTO ""ProductUOM"" (""UID"", ""CreatedBy"",""CreatedTime"",""ModifiedBy"",""ModifiedTime"",""ServerAddTime"",""ServerModifiedTime"",""ProductCode"",
                ""Code"",""Name"",""Label"",""BarCode"",""IsBaseUOM"",""IsOuterUOM"",""Multiplier"") VALUES (@UID, @CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,
                @ServerAddTime ,@ServerModifiedTime ,@ProductCode ,@Code ,@Name ,@Label ,@BarCode ,@IsBaseUOM ,@IsOuterUOM ,@Multiplier)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID",productUOM.UID},
                   {"CreatedBy",productUOM.CreatedBy},
                   {"CreatedTime",productUOM.CreatedTime},
                   {"ModifiedBy",productUOM.ModifiedBy},
                   {"ModifiedTime",productUOM.ModifiedTime},
                   {"ServerAddTime",productUOM.ServerAddTime},
                   {"ServerModifiedTime",productUOM.ServerModifiedTime},
                   {"ProductCode",productUOM.ProductCode},
                   {"Code",productUOM.Code},
                   {"Name",productUOM.Name},
                   {"Label",productUOM.Label},
                   {"BarCode",productUOM.BarCode},
                   {"IsBaseUOM",productUOM.IsBaseUOM},
                   {"IsOuterUOM",productUOM.IsOuterUOM},
                   {"Multiplier",productUOM.Multiplier}
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<int> UpdateProductUOM(Winit.Modules.Product.Model.Interfaces.IProductUOM productUOM)
        {
            try
            {
                var sql = @"UPDATE ""ProductUOM"" SET 
                          ""ModifiedBy""=@ModifiedBy
                          ,""ModifiedTime""	=@ModifiedTime
                          ,""ServerModifiedTime""=@ServerModifiedTime
                          ,""ProductCode""=@ProductCode
                          ,""Code""	=@Code
                          ,""Name""	=@Name
                          ,""Label""=@Label
                          ,""BarCode""=@BarCode
                          ,""IsBaseUOM""=@IsBaseUOM
                          ,""IsOuterUOM""=@IsOuterUOM
                          ,""Multiplier""=@Multiplier
                          WHERE ""UID""=@UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"ModifiedBy",productUOM.ModifiedBy},
                   {"ModifiedTime",productUOM.ModifiedTime},
                   {"ServerModifiedTime",productUOM.ServerModifiedTime},
                   {"ProductCode",productUOM.ProductCode},
                   {"Code",productUOM.Code},
                   {"Name",productUOM.Name},
                   {"Label",productUOM.Label},
                   {"BarCode",productUOM.BarCode},
                   {"IsBaseUOM",productUOM.IsBaseUOM},
                   {"IsOuterUOM",productUOM.IsOuterUOM},
                   {"Multiplier",productUOM.Multiplier},
                   {"UID",productUOM.UID}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteProductUOM(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  FROM ""ProductUOM"" WHERE ""UID"" = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> GetProductUOMFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
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
            var sql = @"SELECT * FROM ProductUOM WHERE (""ProductCode"" LIKE '%' || @ProductCode || '%' OR ""CreatedTime"" LIKE '%' || @CreatedTime || '%'  OR ""ModifiedTime"" LIKE '%' || @ModifiedTime || '%' )";
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM> ProductUOMlist = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductUOM>(sql, parameters);
            return await Task.FromResult(ProductUOMlist);
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> GetProductUOMPaged(int pageNumber, int pageSize)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "SELECT * FROM ProductUOM OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM> ProductUOMList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductUOM>(sql, parameters);
            return await Task.FromResult(ProductUOMList);
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM>> GetProductUOMSorted(List<SortCriteria> sortCriterias)
        {
            var sql = new StringBuilder("SELECT * FROM ProductUOM ORDER BY ");
            var sortParameters = new Dictionary<string, object>();
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                for (var i = 0; i < sortCriterias.Count; i++)
                {
                    var paramName = $"{i}";
                    sql.Append($"@{paramName} {(sortCriterias[i].Direction == SortDirection.Asc ? "ASC" : "DESC")}");
                    if (i < sortCriterias.Count - 1)
                    {
                        sql.Append(", ");
                    }
                    sortParameters.Add(paramName, sortCriterias[i].SortParameter);
                }
            }
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductUOM> ProductUOMList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductUOM>(sql.ToString(), sortParameters);
            return ProductUOMList;
        }

        public async Task<PagedResponse<IProductUOM>> SelectAllProductUOM(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM ""ProductUOM""");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM ""ProductUOM""");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IProductUOM>(filterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    // If count required then add filters to count
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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IProductUOM>().GetType();
                IEnumerable<Model.Interfaces.IProductUOM> productUOMs = await ExecuteQueryAsync<Model.Interfaces.IProductUOM>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductUOM> pagedResponse = new PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductUOM>
                {
                    PagedData = productUOMs,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

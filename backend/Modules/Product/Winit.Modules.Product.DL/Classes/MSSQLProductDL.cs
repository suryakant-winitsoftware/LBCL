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
using Winit.Modules.Product.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.DL.Classes
{
    public class MSSQLProductDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IProductDL
    {
        public MSSQLProductDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProduct>> SelectProductsAll(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT * FROM Product");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM Product");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Product.Model.Interfaces.IProduct>(filterCriterias, sbFilterCriteria, parameters);

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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IProduct>().GetType();
                IEnumerable<Model.Interfaces.IProduct> products = await ExecuteQueryAsync<Model.Interfaces.IProduct>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProduct> pagedResponse = new PagedResponse<Winit.Modules.Product.Model.Interfaces.IProduct>
                {
                    PagedData = products,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Product.Model.Interfaces.IProduct> SelectProductByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT * FROM Product WHERE UID = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IProduct>().GetType();
            Winit.Modules.Product.Model.Interfaces.IProduct productList = await ExecuteSingleAsync<Winit.Modules.Product.Model.Interfaces.IProduct>(sql, parameters, type);
            return productList;
        }
        public async Task<int> CreateProduct(Model.Interfaces.IProduct createproduct)
        {
            var sql = @"INSERT INTO PRODUCT (UID,CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,OrgUID,ProductCode,
                      ProductName,ProductAliasName,LongName,DisplayName,FromDate,ToDate,IsActive,BaseUOM)
                      VALUES(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,
                      @OrgUID,@ProductCode,@ProductName,@ProductAliasName,@LongName,@DisplayName,@FromDate,@ToDate,@IsActive,@BaseUOM);";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID", createproduct.UID},
                   {"CreatedBy", createproduct.CreatedBy},
                   {"CreatedTime", createproduct.CreatedTime},
                   {"ModifiedBy", createproduct.ModifiedBy},
                   {"ModifiedTime", createproduct.ModifiedTime},
                   {"ServerAddTime", createproduct.ServerAddTime},
                   {"ServerModifiedTime", createproduct.ServerModifiedTime},
                   {"OrgUID", createproduct.OrgUID},
                   {"ProductCode", createproduct.ProductCode},
                   {"ProductName", createproduct.ProductName},
                   {"LongName", createproduct.LongName},
                   {"DisplayName", createproduct.DisplayName},
                   {"FromDate", createproduct.FromDate},
                   {"ToDate", createproduct.ToDate},
                   {"IsActive", createproduct.IsActive},
                   {"BaseUOM", createproduct.BaseUOM},
             };
            return await ExecuteNonQueryAsync(sql, parameters);

        }
        public async Task<int> UpdateProduct(Model.Interfaces.IProduct updateProduct)
        {
            try
            {
                var sql = @"UPDATE Product SET ModifiedBy = @ModifiedBy, ModifiedTime = @ModifiedTime, ServerModifiedTime= @ServerModifiedTime,
                    LongName = @LongName,DisplayName = @DisplayName, FromDate = @FromDate,ToDate = @ToDate, IsActive = @IsActive, BaseUOM = @BaseUOM WHERE UID = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"ModifiedBy", updateProduct.ModifiedBy},
                   {"ModifiedTime", updateProduct.ModifiedTime},
                   {"ServerModifiedTime", updateProduct.ServerModifiedTime},
                   {"LongName", updateProduct.LongName},
                   {"DisplayName", updateProduct.DisplayName},
                   {"FromDate", updateProduct.FromDate},
                   {"ToDate", updateProduct.ToDate},
                   {"IsActive", updateProduct.IsActive},
                   {"BaseUOM", updateProduct.BaseUOM},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteProduct(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = "DELETE  FROM Product WHERE UID = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<Model.Interfaces.IProduct>> GetProductsFiltered(string product_code, string product_name, DateTime CreatedTime, DateTime ModifiedTime)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(product_code))
            {
                parameters.Add("product_code", product_code);
            }
            else
            {
                parameters.Add("product_code", DBNull.Value);
            }
            if (!string.IsNullOrEmpty(product_name))
            {
                parameters.Add("product_name", product_name);
            }
            else
            {
                parameters.Add("product_name", DBNull.Value);
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

            var sql = "SELECT * FROM Product WHERE (product_code LIKE '%' + @product_code + '%' OR product_name LIKE '%' + @product_name + '%' " +
                "OR CreatedTime LIKE '%' + @CreatedTime + '%' OR ModifiedTime LIKE '%' + @ModifiedTime + '%' )";

            IEnumerable<Model.Interfaces.IProduct> ProductsList = await ExecuteQueryAsync<Model.Interfaces.IProduct>(sql, parameters);
            return await Task.FromResult(ProductsList);
        }
        public async Task<IEnumerable<Model.Interfaces.IProduct>> GetProductsPaged(int pageNumber, int pageSize)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "SELECT * FROM Product ORDER BY (SELECT NULL) OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<Model.Interfaces.IProduct> ProductsList = await ExecuteQueryAsync<Model.Interfaces.IProduct>(sql, parameters);
            return await Task.FromResult(ProductsList);
        }
        public async Task<IEnumerable<Model.Interfaces.IProduct>> GetProductsSorted(List<SortCriteria> sortCriterias)
        {
            var sql = new StringBuilder("SELECT * FROM Product ORDER BY ");
            var sortParameters = new Dictionary<string, object>();
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                for (var i = 0; i < sortCriterias.Count; i++)
                {
                    var paramName = $"{sortCriterias[i].SortParameter}";
                    sql.Append($"{paramName} {(sortCriterias[i].Direction == SortDirection.Asc ? "ASC" : "DESC")}");

                    if (i < sortCriterias.Count - 1)
                    {
                        sql.Append(", ");
                    }
                    sortParameters.Add(paramName, sortCriterias[i].SortParameter);
                }
            }

            IEnumerable<Model.Interfaces.IProduct> ProductsList = await ExecuteQueryAsync<Model.Interfaces.IProduct>(sql.ToString(), sortParameters);

            return ProductsList;
        }

        public async Task<IEnumerable<IProduct>> SelectAllProduct(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM Product");
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    sql.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Product.Model.Interfaces.IProduct>(filterCriterias, sql, parameters);
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
                IEnumerable<Model.Interfaces.IProduct> productList = (IEnumerable<IProduct>)await ExecuteQueryAsync<Model.Interfaces.IProduct>(sql.ToString(), parameters);
                return productList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}









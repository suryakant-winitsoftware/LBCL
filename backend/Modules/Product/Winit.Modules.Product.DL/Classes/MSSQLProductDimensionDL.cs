using Microsoft.Data.SqlClient;
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
    public class MSSQLProductDimensionDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IProductDimensionDL
    {
        public MSSQLProductDimensionDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> SelectProductDimensionAll()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };
            var sql = @"SELECT * FROM ProductDimension";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Product.Model.Interfaces.IProductDimension>().GetType();
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension> ProductDimensionList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductDimension>(sql, parameters, type);
            return await Task.FromResult(ProductDimensionList);
        }
        public async Task<Winit.Modules.Product.Model.Interfaces.IProductDimension> GetProductDimensionByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT * FROM ProductDimension WHERE UID = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IProductDimension>().GetType();
            Winit.Modules.Product.Model.Interfaces.IProductDimension ProductDimensionDetails = await ExecuteSingleAsync<Winit.Modules.Product.Model.Interfaces.IProductDimension>(sql, parameters, type);
            return ProductDimensionDetails;
        }

        public async Task<int> CreateProductDimension(Winit.Modules.Product.Model.Interfaces.IProductDimension productDimension)
        {
            var sql = @"INSERT INTO ProductDimension (UID,CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,Code,
                    Description,ParentUID) VALUES (@UID,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@Code ,
                        @Description ,@ParentUID)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"CreatedBy",productDimension.CreatedBy},
                   {"CreatedTime",productDimension.CreatedTime},
                   {"ModifiedBy",productDimension.ModifiedBy},
                   {"ModifiedTime",productDimension.ModifiedTime},
                   {"ServerAddTime",productDimension.ServerAddTime},
                   {"ServerModifiedTime",productDimension.ServerModifiedTime},
                   {"Code",productDimension.Code},
                   {"Description",productDimension.Description},
                   {"ParentUID",productDimension.ParentUID},
                   {"UID",productDimension.UID}
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<int> UpdateProductDimension(Winit.Modules.Product.Model.Interfaces.IProductDimension productDimension)
        {
            try
            {
                var sql = @"UPDATE ProductDimension SET 
                        ModifiedBy	=@ModifiedBy
                        ,ModifiedTime=@ModifiedTime
                        ,ServerModifiedTime=@ServerModifiedTime
                        ,Code=@Code
                        ,Description=@Description
                        ,ParentUID=@ParentUID
                         WHERE UID=@UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",productDimension.UID},
                    {"ModifiedBy",productDimension.ModifiedBy},
                    {"ModifiedTime",productDimension.ModifiedTime},
                    {"ServerModifiedTime",productDimension.ServerModifiedTime},
                    {"Code",productDimension.Code},
                    {"Description",productDimension.Description},
                    {"ParentUID",productDimension.ParentUID},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteProductDimension(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  FROM ProductDimension WHERE UID = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> GetProductDimensionFiltered(string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(product_dimension_code))
            {
                parameters.Add("product_dimension_code", product_dimension_code);
            }
            else
            {
                parameters.Add("product_dimension_code", DBNull.Value);
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

            var sql = @"SELECT * FROM ProductDimension WHERE (product_dimension_code LIKE '%' || @product_dimension_code || '%' OR CreatedTime LIKE '%' || @CreatedTime || '%'  OR ModifiedTime LIKE '%' || @ModifiedTime || '%' )";
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension> ProductDimensionlist = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductDimension>(sql, parameters);
            return await Task.FromResult(ProductDimensionlist);
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> GetProductDimensionPaged(int pageNumber, int pageSize)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = @"SELECT * FROM ProductDimension OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension> ProductDimensionList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductDimension>(sql, parameters);
            return await Task.FromResult(ProductDimensionList);
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension>> GetProductDimensionSorted(List<SortCriteria> sortCriterias)
        {
            var sql = new StringBuilder(@"SELECT * FROM ProductDimension ORDER BY ");
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
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension> ProductDimensionList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductDimension>(sql.ToString(), sortParameters);
            return ProductDimensionList;
        }

        public async Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductDimension>> SelectAllProductDimension(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM ProductDimension");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM ProductDimension");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sql.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Product.Model.Interfaces.IProductDimension>(filterCriterias, sql, parameters);
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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IProductDimension>().GetType();
                IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductDimension> ProductDimensionDetails = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductDimension>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductDimension> pagedResponse = new PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductDimension>
                {
                    PagedData = ProductDimensionDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}

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
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.DL.Classes
{
    public class MSSQLProductTypeDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IProductTypeDL
    {
        public MSSQLProductTypeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductType>> SelectProductTypeAll(List<SortCriteria> sortCriterias, int pageNumber,
   int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT * FROM ProductType");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM ProductType");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Product.Model.Interfaces.IProductType>(filterCriterias, sbFilterCriteria, parameters);

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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IProductType>().GetType();
                IEnumerable<Model.Interfaces.IProductType> productTypes = await ExecuteQueryAsync<Model.Interfaces.IProductType>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductType> pagedResponse = new PagedResponse<Winit.Modules.Product.Model.Interfaces.IProductType>
                {
                    PagedData = productTypes,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Product.Model.Interfaces.IProductType> GetProductTypeByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT * FROM ProductType WHERE UID = @UID";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.Product.Model.Interfaces.IProductType>().GetType();
            Winit.Modules.Product.Model.Interfaces.IProductType ProductTypeList = await ExecuteSingleAsync<Winit.Modules.Product.Model.Interfaces.IProductType>(sql, parameters, type);
            return ProductTypeList;
        }

        public async Task<int> CreateProductType(Winit.Modules.Product.Model.Interfaces.IProductType productType)
        {

            var sql = @"INSERT INTO ProductType (UID,CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,ProductTypeName,ProductTypeCode,ProductTypeDescription,
            ParentProductTypeUID) VALUES (@UID ,@CreatedBy ,@CreatedTime ,@ModifiedBy ,@ModifiedTime ,@ServerAddTime ,@ServerModifiedTime ,@ProductTypeName ,@ProductTypeCode ,@ProductTypeDescription ,@ParentProductTypeUID)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",productType.UID},
                {"CreatedBy",productType.CreatedBy},
                {"CreatedTime",productType.CreatedTime},
                {"ModifiedBy",productType.ModifiedBy},
                {"ModifiedTime",productType.ModifiedTime},
                {"ServerAddTime",productType.ServerAddTime},
                {"ServerModifiedTime",productType.ServerModifiedTime},
                {"ProductTypeName",productType.ProductTypeName},
                {"ProductTypeCode",productType.ProductTypeCode},
                {"ProductTypeDescription",productType.ProductTypeDescription},
                {"ParentProductTypeUID",productType.ParentProductTypeUID},
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<int> UpdateProductType(Winit.Modules.Product.Model.Interfaces.IProductType productType)
        {
            try
            {
                var sql = @"UPDATE ProductType SET 
                          ModifiedBy=@ModifiedBy
                          ,ModifiedTime	=@ModifiedTime
                          ,ServerModifiedTime=@ServerModifiedTime
                          ,ProductTypeName=@ProductTypeName
                          ,ProductTypeCode=@ProductTypeCode
                          ,ProductTypeDescription=@ProductTypeDescription
                          ,ParentProductTypeUID=@ParentProductTypeUID
                          WHERE  UID=@UID ;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                     {"UID",productType.UID},
                     {"ModifiedBy",productType.ModifiedBy},
                     {"ModifiedTime",productType.ModifiedTime},
                     {"ServerModifiedTime",productType.ServerModifiedTime},
                     {"ProductTypeName",productType.ProductTypeName},
                     {"ProductTypeCode",productType.ProductTypeCode},
                     {"ProductTypeDescription",productType.ProductTypeDescription},
                     {"ParentProductTypeUID",productType.ParentProductTypeUID}
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<int> DeleteProductType(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  FROM ProductType WHERE UID = @UID";
            var ProductTypeList = await ExecuteNonQueryAsync(sql, parameters);
            return ProductTypeList;
        }
        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType>> GetProductTypeFiltered(string product_type_code, DateTime CreatedTime, DateTime ModifiedTime)
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>();


            if (!string.IsNullOrEmpty(product_type_code))
            {
                parameters.Add("product_type_code", product_type_code);
            }
            else
            {
                parameters.Add("product_type_code", DBNull.Value);
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

            var sql = @"SELECT * FROM ProductType WHERE (product_type_code LIKE '%' || @product_type_code || '%' OR CreatedTime LIKE '%' || @CreatedTime || '%' OR ModifiedTime LIKE '%' || @ModifiedTime || '%')";

            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType> ProductTypelist = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductType>(sql, parameters);
            return await Task.FromResult(ProductTypelist);
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType>> GetProductTypePaged(int pageNumber, int pageSize)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = @"SELECT * FROM ProductType ORDER BY (SELECT NULL) OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType> ProductTypeList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductType>(sql, parameters);
            return await Task.FromResult(ProductTypeList);
        }


        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType>> GetProductTypeSorted(List<SortCriteria> sortCriterias)
        {

            var sql = new StringBuilder(@"SELECT * FROM ProductType ORDER BY ");
            var sortParameters = new Dictionary<string, object>();
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                for (var i = 0; i < sortCriterias.Count; i++)
                {


                    var paramName = $"{sortCriterias[i].SortParameter}";
                    //    var columnName = $"{i}";
                    sql.Append($"{paramName} {(sortCriterias[i].Direction == SortDirection.Asc ? "ASC" : "DESC")}");

                    if (i < sortCriterias.Count - 1)
                    {
                        sql.Append(", ");
                    }

                    sortParameters.Add(paramName, sortCriterias[i].SortParameter);
                }
            }

            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductType> ProductTypeList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductType>(sql.ToString(), sortParameters);

            return ProductTypeList;
        }
    }
}

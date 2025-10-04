using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using WINITRepository.Interfaces.Customers;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace WINITRepository.Classes.Proucts
{
    public class PostgreSQLProductsRepository : Interfaces.Products.IProductRepository
    {
        private readonly string _connectionString;
        public PostgreSQLProductsRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("PostgreSQL");
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsAll()
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Product> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Product>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };

            var sql = "SELECT * FROM Product";

            IEnumerable<WINITSharedObjects.Models.Product> ProductsList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductsList);
        }
        public async Task<WINITSharedObjects.Models.Product> GetProductByProductCode(string productCode)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Product> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Product>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"productCode",  productCode}
            };

            
            var sql = @"SELECT * FROM Product WHERE ""product_code"" = @productCode";

            WINITSharedObjects.Models.Product Product = await dbManager.ExecuteSingleAsync(sql, parameters);
            return await Task.FromResult(Product);
        }

        public async Task<WINITSharedObjects.Models.Product> CreateProduct(WINITSharedObjects.Models.Product Product)
        {
            DBManager.PostgresDBManager<Product> dbManager = new DBManager.PostgresDBManager<Product>(_connectionString);
        
            var sql = @"INSERT INTO Product (""CreatedBy"", ""CreatedTime"", ""ModifiedBy"",""ServerAddTime"",""ServerModifiedTime"",""org_code"",""product_code""
 ,""product_name"",""is_active"",""BaseUOM"") VALUES (@CreatedBy, @CreatedTime, @ModifiedBy,@ServerAddTime,@ServerModifiedTime,@org_code,@product_code,@product_name,@is_active,@BaseUOM)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"CreatedBy", Product.CreatedBy},
                   {"CreatedTime", Product.CreatedTime},
                   {"ModifiedBy", Product.ModifiedBy},
                   {"ServerAddTime", Product.ServerAddTime},
                   {"ServerModifiedTime", Product.ServerModifiedTime},
                   {"org_code", Product.org_code},
                   {"product_code", Product.product_code},
                   {"product_name", Product.product_name},
                   {"is_active", Product.is_active},
                   {"BaseUOM", Product.BaseUOM},



             };
            await dbManager.ExecuteNonQueryAsync(sql, parameters);
           
            return Product;
        }

       
        public async Task<int> UpdateProduct( Product UpdateProduct)
        {
            try
            {
                DBManager.PostgresDBManager<Product> dbManager = new DBManager.PostgresDBManager<Product>(_connectionString);
               
                var sql = @"UPDATE Product SET ""ModifiedBy""=@ModifiedBy,""ModifiedTime""=@ModifiedTime,""Price""=@Price,""ServerModifiedTime""=@ServerModifiedTime,
                       ""org_code""=@org_code, ""product_name""=@product_name ,""is_active""=@is_active ,""BaseUOM""=@BaseUOM  WHERE ""ProductId""=@ProductId";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"ModifiedBy", UpdateProduct.ModifiedBy},
                   {"ModifiedTime", UpdateProduct.ModifiedTime},
                   {"ServerModifiedTime", UpdateProduct.ServerModifiedTime},
                   {"org_code", UpdateProduct.org_code},
                   {"product_name", UpdateProduct.product_name},
                   {"is_active", UpdateProduct.is_active},
                   {"BaseUOM", UpdateProduct.BaseUOM},
                   {"product_code", UpdateProduct.product_code},
                 };
                var updateProducts = await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return updateProducts;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        
        }
        public async Task<int> DeleteProduct(string productCode)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Product> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Product>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"product_code",  productCode}
            };
           
            var sql = @"DELETE  FROM Product WHERE ""product_code"" = @productCode";

            var products = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return products;
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsFiltered(string product_code, String product_name,DateTime CreatedTime,DateTime ModifiedTime)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Product> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Product>(_connectionString);

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

            var sql = @"SELECT * FROM Product WHERE (""product_code"" LIKE '%' || @product_code || '%' OR 
            ""product_name"" LIKE '%' || @product_name || '%' OR ""CreatedTime"" LIKE '%' || @CreatedTime || '%'  OR ""ModifiedTime"" LIKE '%' || @ModifiedTime || '%' )";

            IEnumerable<WINITSharedObjects.Models.Product> productslist = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(productslist);
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsPaged(int pageNumber, int pageSize)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Product> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Product>(_connectionString);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = @"SELECT * FROM Product OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<WINITSharedObjects.Models.Product> productsList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(productsList);
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Product> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Product>(_connectionString);

            var sql = new StringBuilder("SELECT * FROM Product ORDER BY ");
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

            IEnumerable<WINITSharedObjects.Models.Product> productList = await dbManager.ExecuteQueryAsync(sql.ToString(), sortParameters);

            return productList;
        }

        //ProductConfig All EndPoints

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigAll()
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };

            var sql = "SELECT * FROM ProductConfig";

            IEnumerable<WINITSharedObjects.Models.ProductConfig> ProductConfigList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductConfigList);
        }
        public async Task<WINITSharedObjects.Models.ProductConfig> GetProductConfigByskuConfigId(int skuConfigId)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"skuConfigId",  skuConfigId}
            };


            var sql = @"SELECT * FROM ProductConfig WHERE ""SKUConfigId"" = @skuConfigId";

            WINITSharedObjects.Models.ProductConfig ProductConfigList = await dbManager.ExecuteSingleAsync(sql, parameters);
            return await Task.FromResult(ProductConfigList);
        }



        public async Task<WINITSharedObjects.Models.ProductConfig> CreateProductConfig(WINITSharedObjects.Models.ProductConfig CreateProductConfig)
        {
            DBManager.PostgresDBManager<ProductConfig> dbManager = new DBManager.PostgresDBManager<ProductConfig>(_connectionString);

            var sql = @"INSERT INTO ProductConfig (""CreatedBy"", ""CreatedTime"", ""ModifiedBy"",""ModifiedTime"",""ServerAddTime"",""ServerModifiedTime"",""ProductCode"",""DistributionChannelOrgCode""
 ,""CanBuy"",""CanSell"",""BuyingUOM"",""SellingUOM"",""IsActive"") VALUES (@CreatedBy, @CreatedTime, @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@ProductCode,@DistributionChannelOrgCode,@CanBuy,@CanSell,@BuyingUOM,@SellingUOM,@IsActive)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"CreatedBy", CreateProductConfig.CreatedBy},
                   {"CreatedTime", CreateProductConfig.CreatedTime},
                   {"ModifiedBy", CreateProductConfig.ModifiedBy},
                   {"ModifiedTime", CreateProductConfig.ModifiedTime},
                   {"ServerAddTime", CreateProductConfig.ServerAddTime},
                   {"ServerModifiedTime", CreateProductConfig.ServerModifiedTime},
                   {"ProductCode", CreateProductConfig.ProductCode},
                   {"DistributionChannelOrgCode", CreateProductConfig.DistributionChannelOrgCode},
                   {"CanBuy", CreateProductConfig.CanBuy},
                   {"CanSell", CreateProductConfig.CanSell},
                   {"BuyingUOM", CreateProductConfig.BuyingUOM},
                   {"SellingUOM", CreateProductConfig.SellingUOM},
                   {"IsActive", CreateProductConfig.IsActive},

             };
            await dbManager.ExecuteNonQueryAsync(sql, parameters);

            return CreateProductConfig;
        }
        public async Task<int> UpdateProductConfig( ProductConfig UpdateProductConfig)
        {
            try
            {
                DBManager.PostgresDBManager<ProductConfig> dbManager = new DBManager.PostgresDBManager<ProductConfig>(_connectionString);

                var sql = @"UPDATE ProductConfig SET ""ModifiedBy""=@ModifiedBy,""ModifiedTime""=@ModifiedTime,""ProductCode""=@ProductCode,""ServerModifiedTime""=@ServerModifiedTime,
                       ""DistributionChannelOrgCode""=@DistributionChannelOrgCode, ""CanBuy""=@CanBuy ,""BuyingUOM""=@BuyingUOM ,""SellingUOM""=@SellingUOM,""SellingUOM""=@SellingUOM
,""IsActive""=@IsActive WHERE ""SKUConfigId""=@SKUConfigId";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"ModifiedBy", UpdateProductConfig.ModifiedBy},
                   {"ModifiedTime", UpdateProductConfig.ModifiedTime},
                   {"ServerModifiedTime", UpdateProductConfig.ServerModifiedTime},
                   {"ProductCode", UpdateProductConfig.ProductCode},
                   {"DistributionChannelOrgCode", UpdateProductConfig.DistributionChannelOrgCode},
                   {"CanBuy", UpdateProductConfig.CanBuy},
                   {"CanSell", UpdateProductConfig.CanSell},
                   {"BuyingUOM", UpdateProductConfig.BuyingUOM},
                   {"SellingUOM", UpdateProductConfig.SellingUOM},
                   {"IsActive", UpdateProductConfig.IsActive},
                   {"SKUConfigId", UpdateProductConfig.SKUConfigId},
                 };
                var updateProductConfig = await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return updateProductConfig;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<int> DeleteProductConfig(int skuConfigId)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"skuConfigId",  skuConfigId}
            };

            var sql = @"DELETE  FROM ProductConfig WHERE ""SKUConfigId"" = @skuConfigId";

            var ProductConfigList = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return ProductConfigList;
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig>(_connectionString);

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

            var sql = @"SELECT * FROM ProductConfiglist WHERE (""ProductCode"" LIKE '%' || @ProductCode || '%' OR ""CreatedTime"" LIKE '%' || @CreatedTime || '%'  OR ""ModifiedTime"" LIKE '%' || @ModifiedTime || '%' )";

            IEnumerable<WINITSharedObjects.Models.ProductConfig> ProductConfiglist = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductConfiglist);
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigPaged(int pageNumber, int pageSize)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig>(_connectionString);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = @"SELECT * FROM ProductConfig OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<WINITSharedObjects.Models.ProductConfig> ProductConfigList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductConfigList);
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductConfig>(_connectionString);

            var sql = new StringBuilder("SELECT * FROM ProductConfig ORDER BY ");
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

            IEnumerable<WINITSharedObjects.Models.ProductConfig> ProductConfigList = await dbManager.ExecuteQueryAsync(sql.ToString(), sortParameters);

            return ProductConfigList;
        }

        //ProductUOM All EndPoints


        public async Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMAll()
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };

            var sql = "SELECT * FROM ProductUOM";

            IEnumerable<WINITSharedObjects.Models.ProductUOM> ProductUOMList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductUOMList);
        }

        public async Task<WINITSharedObjects.Models.ProductUOM> GetProductUOMByProductUomId(int productUomId)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"productUomId",  productUomId}
            };


            var sql = @"SELECT * FROM ProductUOM WHERE ""productUomId"" = @productUomId";

            WINITSharedObjects.Models.ProductUOM ProductUOMList = await dbManager.ExecuteSingleAsync(sql, parameters);
            return await Task.FromResult(ProductUOMList);
        }

        public async Task<WINITSharedObjects.Models.ProductUOM> CreateProductUOM(WINITSharedObjects.Models.ProductUOM CreateProductUOM)
        {
            DBManager.PostgresDBManager<ProductUOM> dbManager = new DBManager.PostgresDBManager<ProductUOM>(_connectionString);

            var sql = @"INSERT INTO ProductUOM (""CreatedBy"", ""CreatedTime"", ""ModifiedBy"",""ModifiedTime"",""ServerAddTime"",""ServerModifiedTime"",""ProductCode"",""Code""
 ,""Name"",""Label"",""BarCode"",""IsBaseUOM"",""IsOuterUOM"",""Multiplier"") VALUES (@CreatedBy, @CreatedTime, @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@ProductCode,@Code,@Name,@Label,@BarCode,@IsBaseUOM,@IsOuterUOM,@Multiplier)";


           
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"CreatedBy", CreateProductUOM.CreatedBy},
                   {"CreatedTime", CreateProductUOM.CreatedTime},
                   {"ModifiedBy", CreateProductUOM.ModifiedBy},
                   {"ModifiedTime", CreateProductUOM.ModifiedTime},
                   {"ServerAddTime", CreateProductUOM.ServerAddTime},
                   {"ServerModifiedTime", CreateProductUOM.ServerModifiedTime},
                   {"ProductCode", CreateProductUOM.ProductCode},
                   {"Code", CreateProductUOM.Code},
                   {"Name", CreateProductUOM.Name},
                   {"Label", CreateProductUOM.Label},
                   {"BarCode", CreateProductUOM.BarCode},
                   {"IsBaseUOM", CreateProductUOM.IsBaseUOM},
                   {"IsOuterUOM", CreateProductUOM.IsOuterUOM},
                   {"Multiplier", CreateProductUOM.Multiplier},

             };
            await dbManager.ExecuteNonQueryAsync(sql, parameters);

            return CreateProductUOM;
        }

        public async Task<int> UpdateProductUOM( ProductUOM UpdateProductUOM)
        {
            try
            {
                DBManager.PostgresDBManager<ProductUOM> dbManager = new DBManager.PostgresDBManager<ProductUOM>(_connectionString);

               

                var sql = @"UPDATE ProductUOM SET ""ModifiedBy""=@ModifiedBy,""ModifiedTime""=@ModifiedTime,""ProductCode""=@ProductCode,""ServerModifiedTime""=@ServerModifiedTime,
                       ""Code""=@Code, ""Name""=@Name ,""Label""=@Label ,""BarCode""=@BarCode,""IsBaseUOM""=@IsBaseUOM
,""IsOuterUOM""=@IsOuterUOM,,""Multiplier""=@Multiplier WHERE ""ProductUOMId""=@ProductUOMId";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"ModifiedBy", UpdateProductUOM.ModifiedBy},
                   {"ModifiedTime", UpdateProductUOM.ModifiedTime},
                   {"ServerModifiedTime", UpdateProductUOM.ServerModifiedTime},
                   {"ProductCode", UpdateProductUOM.ProductCode},
                   {"Code", UpdateProductUOM.Code},
                   {"Name", UpdateProductUOM.Name},
                   {"Label", UpdateProductUOM.Label},
                   {"BarCode", UpdateProductUOM.BarCode},
                   {"IsBaseUOM", UpdateProductUOM.IsBaseUOM},
                   {"IsOuterUOM", UpdateProductUOM.IsOuterUOM},
                   {"Multiplier", UpdateProductUOM.Multiplier},
                   {"ProductUOMId", UpdateProductUOM.ProductUOMId},
                 };
                var updateProductUOM = await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return updateProductUOM;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> DeleteProductUOM(int productUomId)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"productUomId",  productUomId}
            };

            var sql = @"DELETE  FROM ProductUOM WHERE ""productUomId"" = @productUomId";

            var ProductUOMList = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return ProductUOMList;
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM>(_connectionString);

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
            IEnumerable<WINITSharedObjects.Models.ProductUOM> ProductUOMlist = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductUOMlist);
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMPaged(int pageNumber, int pageSize)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM>(_connectionString);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "SELECT * FROM ProductUOM OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<WINITSharedObjects.Models.ProductUOM> ProductUOMList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductUOMList);
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductUOM>(_connectionString);

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

            IEnumerable<WINITSharedObjects.Models.ProductUOM> ProductUOMList = await dbManager.ExecuteQueryAsync(sql.ToString(), sortParameters);

            return ProductUOMList;
        }

        //GetProductMaster

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductAttributes>> GetProductAttributes()
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductAttributes> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductAttributes>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };

            var sql = "SELECT * FROM ProductAttributes ";

            IEnumerable<WINITSharedObjects.Models.ProductAttributes> ProductAttributesList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductAttributesList);
        }

        public async Task<WINITSharedObjects.Models.ProductDimensionBridge> CreateProductDimensionBridge(WINITSharedObjects.Models.ProductDimensionBridge CreateProductDimensionBridge)
        {
            DBManager.PostgresDBManager<ProductDimensionBridge> dbManager = new DBManager.PostgresDBManager<ProductDimensionBridge>(_connectionString);

            var sql = "INSERT INTO ProductDimensionBridge (CreatedBy, CreatedTime, ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,product_code,product_dimension_id ) VALUES " +
                "(@CreatedBy, @CreatedTime, @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@product_code,@product_dimension_id)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"CreatedBy", CreateProductDimensionBridge.CreatedBy},
                   {"CreatedTime", CreateProductDimensionBridge.CreatedTime},
                   {"ModifiedBy", CreateProductDimensionBridge.ModifiedBy},
                   {"ModifiedTime", CreateProductDimensionBridge.ModifiedTime},
                   {"ServerAddTime", CreateProductDimensionBridge.ServerAddTime},
                   {"ServerModifiedTime", CreateProductDimensionBridge.ServerModifiedTime},
                   {"product_code", CreateProductDimensionBridge.product_code},
                   {"product_dimension_id", CreateProductDimensionBridge.product_dimension_id},


             };
            await dbManager.ExecuteNonQueryAsync(sql, parameters);

            return CreateProductDimensionBridge;
        }
        public async Task<int> DeleteProductDimensionBridge(int product_dimension_bridge_id)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimensionBridge> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimensionBridge>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"product_dimension_bridge_id",  product_dimension_bridge_id}
            };

            var sql = "DELETE  FROM ProductDimensionBridge WHERE product_dimension_bridge_id = @product_dimension_bridge_id";

            var ProductDimensionBridgeList = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return ProductDimensionBridgeList;
        }

        //ProductTypeBridge 

        public async Task<WINITSharedObjects.Models.ProductTypeBridge> CreateProductTypeBridge(WINITSharedObjects.Models.ProductTypeBridge CreateProductTypeBridge)
        {
            DBManager.PostgresDBManager<ProductTypeBridge> dbManager = new DBManager.PostgresDBManager<ProductTypeBridge>(_connectionString);

            var sql = "INSERT INTO ProductTypeBridge (CreatedBy, CreatedTime, ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,product_code,product_type_id) " +
                "VALUES (@CreatedBy, @CreatedTime, @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@product_code," +
                "@product_type_id)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"CreatedBy", CreateProductTypeBridge.CreatedBy},
                   {"CreatedTime", CreateProductTypeBridge.CreatedTime},
                   {"ModifiedBy", CreateProductTypeBridge.ModifiedBy},
                   {"ModifiedTime", CreateProductTypeBridge.ModifiedTime},
                   {"ServerAddTime", CreateProductTypeBridge.ServerAddTime},
                   {"ServerModifiedTime", CreateProductTypeBridge.ServerModifiedTime},
                   {"product_code", CreateProductTypeBridge.product_code},
                   {"product_type_id", CreateProductTypeBridge.product_type_id},


             };
            await dbManager.ExecuteNonQueryAsync(sql, parameters);

            return CreateProductTypeBridge;
        }
        public async Task<int> DeleteProductTypeBridge(int product_type_bridge_id)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductTypeBridge> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductTypeBridge>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"product_type_bridge_id",  product_type_bridge_id}
            };

            var sql = "DELETE  FROM ProductTypeBridge WHERE product_type_bridge_id = @product_type_bridge_id";

            var ProductDimensionBridgeList = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return ProductDimensionBridgeList;
        }


        //ProductType All EndPoints

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeAll()
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };

            var sql = "SELECT * FROM ProductType";

            IEnumerable<WINITSharedObjects.Models.ProductType> ProductTypeList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductTypeList);
        }
        public async Task<WINITSharedObjects.Models.ProductType> GetProductTypeByProductTypeId(int productTypeId)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"productTypeId",  productTypeId}
            };


            var sql = @"SELECT * FROM ProductType WHERE product_type_id = @productTypeId";

            WINITSharedObjects.Models.ProductType ProductTypeList = await dbManager.ExecuteSingleAsync(sql, parameters);
            return await Task.FromResult(ProductTypeList);
        }

        public async Task<WINITSharedObjects.Models.ProductType> CreateProductType(WINITSharedObjects.Models.ProductType CreateProductType)
        {
            DBManager.PostgresDBManager<ProductType> dbManager = new DBManager.PostgresDBManager<ProductType>(_connectionString);

            var sql = "INSERT INTO ProductType (CreatedBy, CreatedTime, ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,product_group_type,product_type_code" +
                ",product_type_description,parent_product_type_id) VALUES (@CreatedBy, @CreatedTime, @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@product_group_type," +
                "@product_type_code,@product_type_description,@parent_product_type_id)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"CreatedBy", CreateProductType.CreatedBy},
                   {"CreatedTime", CreateProductType.CreatedTime},
                   {"ModifiedBy", CreateProductType.ModifiedBy},
                   {"ModifiedTime", CreateProductType.ModifiedTime},
                   {"ServerAddTime", CreateProductType.ServerAddTime},
                   {"ServerModifiedTime", CreateProductType.ServerModifiedTime},
                   {"product_group_type", CreateProductType.product_group_type},
                   {"product_type_code", CreateProductType.product_type_code},
                   {"product_type_description", CreateProductType.product_type_description},
                   {"parent_product_type_id", CreateProductType.parent_product_type_id},


             };
            await dbManager.ExecuteNonQueryAsync(sql, parameters);

            return CreateProductType;
        }

        public async Task<int> UpdateProductType( ProductType UpdateProductType)
        {
            try
            {
                DBManager.PostgresDBManager<ProductType> dbManager = new DBManager.PostgresDBManager<ProductType>(_connectionString);

                var sql = "UPDATE ProductType SET ModifiedBy=@ModifiedBy,ModifiedTime=@ModifiedTime,product_group_type=@product_group_type,ServerModifiedTime=@ServerModifiedTime," +
                    "product_type_code=@product_type_code, product_type_description=@product_type_description ,parent_product_type_id=@parent_product_type_id  WHERE product_type_id=@product_type_id";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"ModifiedBy", UpdateProductType.ModifiedBy},
                   {"ModifiedTime", UpdateProductType.ModifiedTime},
                   {"ServerModifiedTime", UpdateProductType.ServerModifiedTime},
                   {"product_group_type", UpdateProductType.product_group_type},
                   {"product_type_code", UpdateProductType.product_type_code},
                   {"product_type_description", UpdateProductType.product_type_description},
                   {"parent_product_type_id", UpdateProductType.parent_product_type_id},

                   {"product_type_id", UpdateProductType.product_type_id},
                 };
                var updateProductType = await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return updateProductType;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public async Task<int> DeleteProductType(int productTypeId)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"productTypeId",  productTypeId}
            };

            var sql = "DELETE  FROM ProductType WHERE product_type_id = @productTypeId";

            var ProductTypeList = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return ProductTypeList;
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeFiltered(string product_type_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType>(_connectionString);

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

            var sql = @"SELECT * FROM ProductType WHERE (product_type_code LIKE '%' || @product_type_code || '%' OR CreatedTime LIKE '%' || @CreatedTime || '%'  OR ModifiedTime LIKE '%' || @ModifiedTime || '%' )";

            IEnumerable<WINITSharedObjects.Models.ProductType> ProductTypelist = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductTypelist);
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypePaged(int pageNumber, int pageSize)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType>(_connectionString);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "SELECT * FROM ProductType OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<WINITSharedObjects.Models.ProductType> ProductTypeList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductTypeList);
        }


        public async Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductType>(_connectionString);

            var sql = new StringBuilder("SELECT * FROM ProductType ORDER BY ");
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

            IEnumerable<WINITSharedObjects.Models.ProductType> ProductTypeList = await dbManager.ExecuteQueryAsync(sql.ToString(), sortParameters);

            return ProductTypeList;
        }

        //ProductDimension




        public async Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionAll()
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };

            var sql = "SELECT * FROM ProductDimension";

            IEnumerable<WINITSharedObjects.Models.ProductDimension> ProductDimensionList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductDimensionList);
        }
        public async Task<WINITSharedObjects.Models.ProductDimension> GetProductDimensionByProductDimensionId(int productDimensionId)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"productDimensionId",  productDimensionId}
            };


            var sql = @"SELECT * FROM ProductDimension WHERE product_dimension_id = @productDimensionId";

            WINITSharedObjects.Models.ProductDimension ProductDimensionList = await dbManager.ExecuteSingleAsync(sql, parameters);
            return await Task.FromResult(ProductDimensionList);
        }


        public async Task<WINITSharedObjects.Models.ProductDimension> CreateProductDimensionList(WINITSharedObjects.Models.ProductDimension CreateProductDimension)
        {
            DBManager.PostgresDBManager<ProductDimension> dbManager = new DBManager.PostgresDBManager<ProductDimension>(_connectionString);

            var sql = "INSERT INTO ProductDimension (CreatedBy, CreatedTime, ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,product_dimension_code,product_dimension_description" +
                ",parent_product_dimension_id) VALUES (@CreatedBy, @CreatedTime, @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@product_dimension_code," +
                "@product_dimension_description,@parent_product_dimension_id)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"CreatedBy", CreateProductDimension.CreatedBy},
                   {"CreatedTime", CreateProductDimension.CreatedTime},
                   {"ModifiedTime", CreateProductDimension.ModifiedTime},
                   {"ModifiedBy", CreateProductDimension.ModifiedBy},
                   {"ServerAddTime", CreateProductDimension.ServerAddTime},
                   {"ServerModifiedTime", CreateProductDimension.ServerModifiedTime},
                   {"product_dimension_code", CreateProductDimension.product_dimension_code},
                   {"product_dimension_description", CreateProductDimension.product_dimension_description},
                   {"parent_product_dimension_id", CreateProductDimension.parent_product_dimension_id},


             };
            await dbManager.ExecuteNonQueryAsync(sql, parameters);

            return CreateProductDimension;
        }

        public async Task<int> UpdateProductDimension( ProductDimension UpdateProductDimension)
        {
            try
            {
                DBManager.PostgresDBManager<ProductDimension> dbManager = new DBManager.PostgresDBManager<ProductDimension>(_connectionString);

                var sql = "UPDATE ProductDimension SET ModifiedBy=@ModifiedBy,ModifiedTime=@ModifiedTime,product_group_type=@product_group_type,ServerModifiedTime=@ServerModifiedTime," +
                    "product_dimension_code=@product_dimension_code, product_dimension_description=@product_dimension_description ,parent_product_dimension_id=@parent_product_dimension_id  WHERE product_dimension_id=@product_dimension_id";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"ModifiedBy", UpdateProductDimension.ModifiedBy},
                   {"ModifiedTime", UpdateProductDimension.ModifiedTime},
                   {"ServerModifiedTime", UpdateProductDimension.ServerModifiedTime},
                   {"product_dimension_code", UpdateProductDimension.product_dimension_code},
                   {"product_dimension_description", UpdateProductDimension.product_dimension_description},
                   {"parent_product_dimension_id", UpdateProductDimension.parent_product_dimension_id},

                   {"product_dimension_id", UpdateProductDimension.product_dimension_id},
                 };
                var updateProductDimension = await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return updateProductDimension;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public async Task<int> DeleteProductDimension(int productDimensionId)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"productDimensionId",  productDimensionId}
            };

            var sql = "DELETE  FROM ProductDimension WHERE product_dimension_id = @productDimensionId";

            var ProductDimensionList = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return ProductDimensionList;
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionFiltered(string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension>(_connectionString);

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

            IEnumerable<WINITSharedObjects.Models.ProductDimension> ProductDimensionlist = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductDimensionlist);
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionPaged(int pageNumber, int pageSize)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension>(_connectionString);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "SELECT * FROM ProductDimension OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<WINITSharedObjects.Models.ProductDimension> ProductDimensionList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(ProductDimensionList);
        }


        public async Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.ProductDimension>(_connectionString);

            var sql = new StringBuilder("SELECT * FROM ProductDimension ORDER BY ");
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

            IEnumerable<WINITSharedObjects.Models.ProductDimension> ProductDimensionList = await dbManager.ExecuteQueryAsync(sql.ToString(), sortParameters);

            return ProductDimensionList;
        }

        public Task<IEnumerable<Product>> AllGetProduct(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias)
        {
            throw new NotImplementedException();
        }
    }
}









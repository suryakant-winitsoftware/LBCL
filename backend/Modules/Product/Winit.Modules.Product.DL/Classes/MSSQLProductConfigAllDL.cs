using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.DL.Classes
{
    public class MSSQLProductConfigAllDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IProductConfigAllDL
    {
        public MSSQLProductConfigAllDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>> GetProductConfigAll()
        {

            try
            {
               
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                };
                var sql = "SELECT * FROM ProductConfig";

                IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll> ProductConfigList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>(sql, parameters);
                return await Task.FromResult(ProductConfigList);
            }
            catch (Exception)
            {
                throw;
            }


        }
        public async Task<Winit.Modules.Product.Model.Interfaces.IProductConfigAll> GetProductConfigByskuConfigId(int skuConfigId)
        {

            try
            {
               

                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"SKUConfigId",  skuConfigId}
            };


                var sql = @"SELECT * FROM ProductConfig WHERE SKUConfigId = @skuConfigId";

                Winit.Modules.Product.Model.Interfaces.IProductConfigAll ProductConfigList = await ExecuteSingleAsync<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>(sql, parameters);
                return await Task.FromResult(ProductConfigList);
            }
            catch (Exception)
            {
                throw;
            }

        }



        public async Task<Winit.Modules.Product.Model.Interfaces.IProductConfigAll> CreateProductConfig(Winit.Modules.Product.Model.Interfaces.IProductConfigAll CreateProductConfig)
        {
            

            var sql = "INSERT INTO ProductConfig (CreatedBy, CreatedTime, ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,ProductCode,DistributionChannelOrgCode" +
                ",CanBuy,CanSell,BuyingUOM,SellingUOM,IsActive) VALUES (@CreatedBy, @CreatedTime, @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@ProductCode,@DistributionChannelOrgCode,@CanBuy,@CanSell,@BuyingUOM,@SellingUOM,@IsActive)";
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
            await ExecuteNonQueryAsync(sql, parameters);

            return CreateProductConfig;
        }
        public async Task<int> UpdateProductConfig(Winit.Modules.Product.Model.Interfaces.IProductConfigAll UpdateProductConfig)
        {
            try
            {
            

                var sql = "UPDATE ProductConfig SET ModifiedBy=@ModifiedBy,ModifiedTime=@ModifiedTime,ProductCode=@ProductCode,ServerModifiedTime=@ServerModifiedTime," +
                    "DistributionChannelOrgCode=@DistributionChannelOrgCode, CanBuy=@CanBuy ,BuyingUOM=@BuyingUOM ,SellingUOM=@SellingUOM," +
                    "IsActive=@IsActive WHERE SKUConfigId=@SKUConfigId";
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
                var updateProductConfig = await ExecuteNonQueryAsync(sql, parameters);
                return updateProductConfig;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<int> DeleteProductConfig(int skuConfigId)
        {
           

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"skuConfigId",  skuConfigId}
            };

            var sql = "DELETE  FROM ProductConfig WHERE SKUConfigId = @skuConfigId";

            var ProductConfigList = await ExecuteNonQueryAsync(sql, parameters);
            return ProductConfigList;
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
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
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll> ProductConfiglist = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>(sql, parameters);
            return await Task.FromResult(ProductConfiglist);
        }

        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>> GetProductConfigPaged(int pageNumber, int pageSize)
        {
            
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = "SELECT * FROM ProductConfig ORDER BY (SELECT NULL) OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll> ProductConfigList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>(sql, parameters);
            return await Task.FromResult(ProductConfigList);

        }


        public async Task<IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>> GetProductConfigSorted(List<SortCriteria> sortCriterias)
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

                IEnumerable<Winit.Modules.Product.Model.Interfaces.IProductConfigAll> ProductConfigList = await ExecuteQueryAsync<Winit.Modules.Product.Model.Interfaces.IProductConfigAll>(sql.ToString(), sortParameters);

                return ProductConfigList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}









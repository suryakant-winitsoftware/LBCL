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
using Winit.Modules.Product.Model.Classes;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Product.DL.Classes
{
    public class MSSQLProductDimensionBridgeDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IProductDimensionBridgeDL
    {
        public MSSQLProductDimensionBridgeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> CreateProductDimensionBridge(Winit.Modules.Product.Model.Interfaces.IProductDimensionBridge CreateProductDimensionBridge)
        {
       
            try
            {
                var sql = @"INSERT INTO ProductDimensionBridge (UID, CreatedBy, CreatedTime, ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,ProductCode,ProductDimensionUID) VALUES
              (@UID,@CreatedBy, @CreatedTime, @ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@ProductCode,@ProductDimensionUID)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"CreatedBy", CreateProductDimensionBridge.CreatedBy},
                   {"CreatedTime", CreateProductDimensionBridge.CreatedTime},
                   {"ModifiedBy", CreateProductDimensionBridge.ModifiedBy},
                   {"ModifiedTime", CreateProductDimensionBridge.ModifiedTime},
                   {"ServerAddTime", CreateProductDimensionBridge.ServerAddTime},
                   {"ServerModifiedTime", CreateProductDimensionBridge.ServerModifiedTime},
                   {"ProductCode", CreateProductDimensionBridge.ProductCode},
                   {"ProductDimensionUID", CreateProductDimensionBridge.ProductDimensionUID},
                   {"UID", CreateProductDimensionBridge.UID}
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteProductDimensionBridge(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = "DELETE  FROM ProductDimensionBridge WHERE UID = @UID";
            var ProductDimensionBridgeList = await ExecuteNonQueryAsync(sql, parameters);
            return ProductDimensionBridgeList;
        }
    }
}









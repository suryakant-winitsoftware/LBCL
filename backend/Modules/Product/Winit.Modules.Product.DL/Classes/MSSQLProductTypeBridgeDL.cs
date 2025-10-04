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
    public class MSSQLProductTypeBridgeDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IProductTypeBridgeDL
    {
        public MSSQLProductTypeBridgeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> CreateProductTypeBridge(Model.Interfaces.IProductTypeBridge createProductTypeBridge)
        {
            var sql = @"INSERT INTO ProductTypeBridge (UID,CreatedBy,CreatedTime,ModifiedBy,ModifiedTime,ServerAddTime,ServerModifiedTime,ProductCode,ProductTypeUID)
                      VALUES(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@ProductCode,@ProductTypeUID);";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                   {"UID", createProductTypeBridge.UID},
                   {"CreatedBy", createProductTypeBridge.CreatedBy},
                   {"CreatedTime", createProductTypeBridge.CreatedTime},
                   {"ModifiedBy", createProductTypeBridge.ModifiedBy},
                   {"ModifiedTime", createProductTypeBridge.ModifiedTime},
                   {"ServerAddTime", createProductTypeBridge.ServerAddTime},
                   {"ServerModifiedTime", createProductTypeBridge.ServerModifiedTime},
                   {"ProductCode", createProductTypeBridge.ProductCode},
                   {"ProductTypeUID", createProductTypeBridge.ProductTypeUID},
             };
            return await ExecuteNonQueryAsync(sql, parameters);

        }
        public async Task<int> DeleteProductTypeBridgeByUID(String UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = "DELETE  FROM ProductTypeBridge WHERE UID = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
            
        }
    }
}









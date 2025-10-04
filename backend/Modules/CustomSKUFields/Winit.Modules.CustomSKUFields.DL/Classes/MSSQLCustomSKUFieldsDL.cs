using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Winit.Modules.Base.Model;
using Winit.Modules.CustomSKUField.DL.Interfaces;
using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CustomSKUField.DL.Classes
{
    public class MSSQLCustomSKUFieldsDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ICustomSKUFieldsDL
    {
       protected Winit.Shared.CommonUtilities.Common.CommonFunctions _commonFunctions;
        private readonly ApiSettings _apiSettings;
        public MSSQLCustomSKUFieldsDL(IServiceProvider serviceProvider, IConfiguration config, IOptions<ApiSettings> apiSettings, Shared.CommonUtilities.Common.CommonFunctions commonFunctions) : base(serviceProvider, config)
        {
            _apiSettings = apiSettings.Value;
            _commonFunctions = commonFunctions;
        }
        public async Task<int> CreateCustomSKUField(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField)
        {
            try
            {
                var json = JsonConvert.SerializeObject(customSKUField.CustomField);
                int retVal = -1;
                var sql = @"
                            INSERT INTO custom_sku_fields (
                            uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, sku_uid, custom_field
                        ) 
                        VALUES (
                            @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,
                            @ServerModifiedTime, @SKUUID, @CustomField
                        );";
                     Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@UID", Guid.NewGuid() },
                    { "@CreatedBy", customSKUField.CreatedBy },
                    { "@CreatedTime", customSKUField.CreatedTime },
                    { "@ModifiedBy", customSKUField.ModifiedBy },
                    { "@ModifiedTime", customSKUField.ModifiedTime },
                    { "@ServerAddTime", customSKUField.ServerAddTime },
                    { "@ServerModifiedTime", customSKUField.ServerModifiedTime },
                    { "@SKUUID", customSKUField.SKUUID },
                    { "@CustomField", json },
                };

                retVal= await ExecuteNonQueryAsync(sql, parameters);

                return retVal;

                }
                catch (Exception)
                {
                    throw;
                }
        }
        public async Task<int> UpdateCustomSKUField(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField)
        {
            try
            {
                var json = JsonConvert.SerializeObject(customSKUField.CustomField);
                var sql = @"UPDATE custom_sku_fields SET 
                                modified_by = @ModifiedBy, 
                                modified_time = @ModifiedTime, 
                                server_modified_time = @ServerModifiedTime, 
                                custom_field = @CustomField::jsonb
                                 WHERE 
                                sku_uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@UID", customSKUField.SKUUID },
                    { "@ModifiedBy", customSKUField.ModifiedBy },
                    { "@ModifiedTime", customSKUField.ModifiedTime },
                    { "@ServerModifiedTime", customSKUField.ServerModifiedTime },
                    { "@CustomField", json },
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CUDCustomSKUFields(Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField customSKUField)
        {
            try
            {
                string? isExists = await CheckIfUIDExistsInDB(DbTableName.CustomSKUFields, customSKUField.UID);
                if (!string.IsNullOrEmpty(isExists))
                {
                    return await UpdateCustomSKUField(customSKUField);
                }
                else
                {
                    return await CreateCustomSKUField(customSKUField);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<Winit.Modules.CustomSKUField.Model.Classes.CustomField>> SelectAllCustomFieldsDetails(string SKUUID)
        {
            try
            {
                IEnumerable<Model.Classes.CustomField> jsonCustomSKUFieldsList = _commonFunctions.ReadJsonFile<Model.Classes.CustomField>(_apiSettings.SKUCustomFields);
                var sql = new StringBuilder(@"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            sku_uid AS SkuUid,
                            custom_field AS CustomField
                        FROM 
                            custom_sku_fields
                         WHERE sku_uid=@SKUUID");
                Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            {"SKUUID", SKUUID }
        };

                IEnumerable<Model.Interfaces.ICustomSKUFields> lstActualCustomFields = await ExecuteQueryAsync<Model.Interfaces.ICustomSKUFields>(sql.ToString(), parameters);

                if (lstActualCustomFields != null && lstActualCustomFields.Any())
                {
                    foreach (CustomSKUFields customField in lstActualCustomFields)
                    {
                        List<CustomField> dbData = JsonConvert.DeserializeObject<List<CustomField>>(customField.CustomField);

                        foreach (CustomField dbItem in dbData)
                        {
                            CustomField rawCustomField = jsonCustomSKUFieldsList.FirstOrDefault(e => e.UID == dbItem.UID);

                            if (rawCustomField != null)
                            {
                                rawCustomField.Value = dbItem.Value;
                            }
                        }
                    }
                }

                return jsonCustomSKUFieldsList;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.DL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Auth.DL.Classes;

public  class SQLiteCommonDataDL:Winit.Modules.Base.DL.DBManager.SqliteDBManager, ICommonDataDL
{
    public SQLiteCommonDataDL(IServiceProvider serviceProvider) : base(serviceProvider)
    {

    }
    public async Task<List<SKUGroupSelectionItem>> GetAllSKUAttibutes(List<string> OrgUIDs)
    {
        try
        {
            var sql = new StringBuilder(@"SELECT 
                    sgt.code AS Code,
                    sgt.name AS Label,
                    CAST(sgt.item_level AS TEXT) AS Uid,
                    sgtp.code AS ParentCode,
                    sgt.available_for_filter AS AvailableForFilter 
                FROM  
                    sku_group_type sgt
                LEFT JOIN 
                    sku_group_type sgtp ON sgt.parent_uid = sgtp.uid 
                WHERE 
                    sgt.org_uid IN @OrgUIDs;
                ");
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"OrgUIDs",OrgUIDs }
            };
            return
                await ExecuteQueryAsync<SKUGroupSelectionItem>(sql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<SKUGroupSelectionItem>> GetAttributeTypes(List<string> orgUIDs)
    {
        try
        {
            var sql = new StringBuilder(@"SELECT DISTINCT 
                            sgt.code AS Uid, 
                            sg.code, 
                            sg.name AS Label, 
                            sgp.code AS ParentCode,
                            fs.relative_path || '/' || fs.file_name AS ExtData
                        FROM 
                            sku_group_type sgt
                        INNER JOIN 
                            sku_group sg ON sg.sku_group_type_uid = sgt.uid
                        LEFT JOIN 
                            sku_group sgp ON sgp.uid = sg.parent_uid
                        LEFT JOIN 
                            file_sys fs ON fs.linked_item_type = 'SKUGroup' AND fs.linked_item_uid = sg.uid
                        WHERE 
                            sgt.org_uid IN @OrgUIDs");
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"OrgUIDs",orgUIDs }
            };
            return
                await ExecuteQueryAsync<SKUGroupSelectionItem>(sql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
}

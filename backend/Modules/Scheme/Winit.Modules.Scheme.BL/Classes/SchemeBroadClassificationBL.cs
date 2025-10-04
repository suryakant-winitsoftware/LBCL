using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes;

public class SchemeBroadClassificationBL : ISchemeBroadClassificationBL
{
    ISchemeBroadClassificationDL _schemeBroadClassificationDl { get; }
    public SchemeBroadClassificationBL(ISchemeBroadClassificationDL schemeBroadClassificationDl)
    {
        _schemeBroadClassificationDl = schemeBroadClassificationDl;
    }


    public async Task<List<ISchemeBroadClassification>> GetSchemeBroadClassificationByLinkedItemUID(string linkedItemUID)
    {
        return await _schemeBroadClassificationDl.GetSchemeBroadClassificationByLinkedItemUID(linkedItemUID);
    }
    public async Task<int> CreateSchemeBroadClassifications(List<ISchemeBroadClassification> schemeBroadClassifications)
    {
        return await _schemeBroadClassificationDl.CreateSchemeBroadClassifications(schemeBroadClassifications);
    }
    public async Task<int> CDBroadClassification(List<ISchemeBroadClassification> schemeBroadClassifications, string linkedItemUID)
    {
        int cnt = 0;
        var exst = await _schemeBroadClassificationDl.GetSchemeBroadClassificationByLinkedItemUID(linkedItemUID);
        if (exst != null && exst.Count > 0)
        {
            List<ISchemeBroadClassification> insertList = [];
            List<string> deleteList = new List<string>();
            if (schemeBroadClassifications != null && schemeBroadClassifications.Count > 0)
            {
                schemeBroadClassifications.ForEach(p =>
                {
                    if (!exst.Any(q => q.LinkedItemUID.Equals(p.LinkedItemUID) && q.BroadClassificationCode.Equals(p.BroadClassificationCode)))
                    {
                        insertList.Add(p);
                    }
                });
                exst.ForEach(p =>
                {
                    if (!schemeBroadClassifications.Any(q => q.LinkedItemUID.Equals(p.LinkedItemUID) && q.BroadClassificationCode.Equals(p.BroadClassificationCode)))
                    {
                        deleteList.Add(p.UID);
                    }
                });

                // insertList.AddRange(sellInDTO.SchemeBroadClassifications.Except(exst));

                if (insertList.Count > 0)
                    cnt += await _schemeBroadClassificationDl.CreateSchemeBroadClassifications(insertList);
                if (deleteList.Count > 0)
                    cnt += await _schemeBroadClassificationDl.DeleteSchemeBroadClassifications(deleteList);
            }
            else
            {
                deleteList.AddRange(exst.Select(p => p.UID));

                if (deleteList.Count > 0)
                    cnt += await _schemeBroadClassificationDl.DeleteSchemeBroadClassifications(deleteList);
            }

        }
        else if (schemeBroadClassifications != null && schemeBroadClassifications.Count > 0)
        {
            cnt += await _schemeBroadClassificationDl.CreateSchemeBroadClassifications(schemeBroadClassifications);
        }
        return cnt;
    }
}

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;


using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.BL.Classes
{
    public class SchemeBranchBL : ISchemeBranchBL
    {
        ISchemeBranchDL _schemeBranchDL { get; }
        public SchemeBranchBL(ISchemeBranchDL schemeBranchDL)
        {
            _schemeBranchDL = schemeBranchDL;
        }


        public async Task<List<ISchemeBranch>> GetSchemeBranchesByLinkedItemUID(string linkedItemUID)
        {
            return await _schemeBranchDL.GetSchemeBranchesByLinkedItemUID(linkedItemUID);
        }
        public async Task<int> CreateSchemeBranches(List<ISchemeBranch> schemeBranches)
        {

            return await _schemeBranchDL.CreateSchemeBranches(schemeBranches);

        }
        public async Task<int> CDBranches(List<ISchemeBranch> schemeBranches, string linkedItemUID)
        {
            int cnt = 0;
            var exstBranches = await _schemeBranchDL.GetSchemeBranchesByLinkedItemUID(linkedItemUID);
            if (exstBranches != null && exstBranches.Count > 0)
            {
                List<ISchemeBranch> insertList = [];
                List<string> deleteList = new List<string>();
                if (schemeBranches != null && schemeBranches.Count > 0)
                {
                    schemeBranches.ForEach(p =>
                    {
                        if (!exstBranches.Any(q => q.LinkedItemUID.Equals(p.LinkedItemUID) && q.BranchCode.Equals(p.BranchCode)))
                        {
                            insertList.Add(p);
                        }
                    });
                    exstBranches.ForEach(p =>
                    {
                        if (!schemeBranches.Any(q => q.LinkedItemUID.Equals(p.LinkedItemUID) && q.BranchCode.Equals(p.BranchCode)))
                        {
                            deleteList.Add(p.UID);
                        }
                    });

                    if (insertList.Count > 0)
                        cnt += await _schemeBranchDL.CreateSchemeBranches(insertList);

                    if (deleteList.Count > 0)
                        cnt += await _schemeBranchDL.DeleteSchemeBranches(deleteList);
                }
                else
                {
                    deleteList.AddRange(exstBranches.Select(p => p.UID));

                    if (deleteList.Count > 0)
                        cnt += await _schemeBranchDL.DeleteSchemeBranches(deleteList);
                }

            }
            else if (schemeBranches != null && schemeBranches.Count > 0)
            {
                cnt += await _schemeBranchDL.CreateSchemeBranches(schemeBranches);
            }
            return cnt;
        }
    }
}

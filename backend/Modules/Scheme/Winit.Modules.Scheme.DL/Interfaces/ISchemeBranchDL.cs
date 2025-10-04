using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface ISchemeBranchDL
    {
        Task<List<ISchemeBranch>> GetSchemeBranchesByLinkedItemUID(string linkedItemUID);
        Task<int> CreateSchemeBranches(List<ISchemeBranch> schemeBranches);
        Task<int> DeleteSchemeBranches(List<string> uids);
    }
}

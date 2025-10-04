using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISchemeBranchBL
    {
        Task<List<ISchemeBranch>> GetSchemeBranchesByLinkedItemUID(string linkedItemUID);
        Task<int> CreateSchemeBranches(List<ISchemeBranch> schemeBranches);
        Task<int> CDBranches(List<ISchemeBranch> schemeBranches, string linkedItemUID);
    }
}

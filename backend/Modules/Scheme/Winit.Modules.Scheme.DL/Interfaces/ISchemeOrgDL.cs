
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface ISchemeOrgDL
    {
        Task<List<ISchemeOrg>> GetSchemeOrgByLinkedItemUID(string linkedItemUID);
        Task<int> CreateSchemeOrgs(List<ISchemeOrg> schemeBranches);
        Task<int> DeleteSchemeOrgs(List<string> uids);
    }
}

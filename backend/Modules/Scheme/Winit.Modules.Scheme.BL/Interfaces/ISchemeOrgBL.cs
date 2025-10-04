
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISchemeOrgBL
    {
        Task<List<ISchemeOrg>> GetSchemeOrgByLinkedItemUID(string linkedItemUID);
        Task<int> CreateSchemeOrgs(List<ISchemeOrg> schemeBranches);
        Task<int> CDOrgs(List<ISchemeOrg> schemeOrgs, string linkedItemUID);
    }
}

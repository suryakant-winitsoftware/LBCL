using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Scheme.DL.Interfaces
{
    public interface ISchemeBroadClassificationDL
    {
        Task<List<ISchemeBroadClassification>> GetSchemeBroadClassificationByLinkedItemUID(string linkedItemUID);
        Task<int> CreateSchemeBroadClassifications(List<ISchemeBroadClassification> schemeBranches);
        Task<int> DeleteSchemeBroadClassifications(List<string> uids);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerBL.Interfaces
{
    public interface IEntityScriptBL
    {
        Task<SyncManagerModel.Interfaces.IEntityScript> GetEntityScriptDetailsByEntity(string Entity);
    }
}

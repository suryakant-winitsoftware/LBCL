using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Interfaces
{
    public interface ITaxMasterDL
    {
        Task<List<SyncManagerModel.Interfaces.ITaxMaster>> GetTaxMasterDetails(string sql);

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Interfaces
{
    public interface IPriceLadderingDL
    {
        Task<List<SyncManagerModel.Interfaces.IPriceLaddering>> GetPriceLadderingDetails(string sql);

    }
}

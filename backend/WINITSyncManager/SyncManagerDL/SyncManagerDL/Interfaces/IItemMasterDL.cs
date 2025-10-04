using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SyncManagerDL.Interfaces
{
    public interface IItemMasterDL
    {
        Task<List<SyncManagerModel.Interfaces.IItemMaster>> GetItemMasterDetails(string sql);
    }
}

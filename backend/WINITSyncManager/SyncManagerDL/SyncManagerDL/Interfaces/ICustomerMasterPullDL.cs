using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Interfaces
{
    public interface ICustomerMasterPullDL
    {
        Task<List<SyncManagerModel.Interfaces.ICustomerMasterPull>> GetCustomerMasterPullDetails(string sql);
    }
}

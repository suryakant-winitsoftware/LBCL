using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Interfaces
{
    public interface ICustomerCreditLimitDL
    {
        Task<List<SyncManagerModel.Interfaces.ICustomerCreditLimit>> GetCustomerCreditLimitDetails(string sql);

    }
}

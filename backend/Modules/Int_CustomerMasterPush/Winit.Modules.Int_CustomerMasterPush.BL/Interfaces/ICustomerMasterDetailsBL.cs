using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.Int_CustomerMasterPush.Model.Interfaces;

namespace Winit.Modules.Int_CustomerMasterPush.BL.Interfaces
{
    public interface ICustomerMasterDetailsBL
    {
        Task<List<Model.Interfaces.ICustomerMasterDetails>> GetCustomerMasterPushDetails(IEntityDetails entityDetails);
        Task<int> InsertCustomerdetailsIntoOracleStaging(List<ICustomerMasterDetails> customers);
    }
}

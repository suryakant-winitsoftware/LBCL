using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.Int_CustomerMasterPush.Model.Interfaces;


namespace Winit.Modules.Int_CustomerMasterPush.DL.Interfaces
{
    public interface ICustomerMasterDetailsDL
    {
        Task<List<Model.Interfaces.ICustomerMasterDetails>> GetCustomerMasterPushDetails(IEntityDetails entityDetails);
        Task<int> InsertCustomerdetailsIntoOracleStaging(List<ICustomerMasterDetails> customers);
        
    }
}

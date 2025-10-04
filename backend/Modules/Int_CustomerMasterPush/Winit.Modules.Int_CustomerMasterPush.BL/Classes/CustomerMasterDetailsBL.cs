using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_CommonMethods.BL.Interfaces;
using Winit.Modules.Int_CommonMethods.Model.Classes;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.Int_CustomerMasterPush.BL.Interfaces;
using Winit.Modules.Int_CustomerMasterPush.DL.Interfaces;
using Winit.Modules.Int_CustomerMasterPush.Model.Interfaces;
using static Dapper.SqlMapper;

namespace Winit.Modules.Int_CustomerMasterPush.BL.Classes
{
    public class CustomerMasterDetailsBL : ICustomerMasterDetailsBL
    {
        private readonly ICustomerMasterDetailsDL _mssqlCustomerMasterDL;
        private readonly ICustomerMasterDetailsDL _oracleCustomerMasterDL;
        private readonly Iint_CommonMethodsBL _commonMethodsBL;
        public CustomerMasterDetailsBL(Func<string, ICustomerMasterDetailsDL> customerMaster,  Iint_CommonMethodsBL iint_CommonMethodsBL)
        {
            _mssqlCustomerMasterDL = customerMaster(Shared.Models.Constants.ConnectionStringName.SqlServer);
            _oracleCustomerMasterDL = customerMaster(Shared.Models.Constants.ConnectionStringName.OracleServer);
            _commonMethodsBL = iint_CommonMethodsBL;
        }
        public async Task<List<Model.Interfaces.ICustomerMasterDetails>> GetCustomerMasterPushDetails(IEntityDetails entityDetails)
        {
            return await _mssqlCustomerMasterDL.GetCustomerMasterPushDetails(entityDetails);
        }
        public async Task<int> InsertCustomerdetailsIntoOracleStaging(List<ICustomerMasterDetails> customers)
        {
            return await _oracleCustomerMasterDL.InsertCustomerdetailsIntoOracleStaging(customers);
        }
        
    }
}

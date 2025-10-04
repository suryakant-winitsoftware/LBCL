using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncManagerDL.Base.DBManager;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.Int_CustomerMasterPush.DL.Interfaces;
using Winit.Modules.Int_CustomerMasterPush.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Int_CustomerMasterPush.DL.Classes
{
    public class OracleCustomerMasterDetailsDL : OracleServerDBManager, ICustomerMasterDetailsDL
    {
        public OracleCustomerMasterDetailsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> InsertCustomerdetailsIntoOracleStaging(List<ICustomerMasterDetails> customers)
        {
            try
            {
                var sql = new StringBuilder($@"insert into  {Int_OracleTableNames.CustomerMaster}(customer_name,sales_office,classification,address1,address2,pincode,city,state,country,first_name,phn_no,mobile
              ,email,pan_no_gst_no,warehouse,purpose,primary,legal_name,address_key,oracle_customer_code,oracle_location_code,read_from_oracle,inserted_on)
                values( :CustomerName,:SalesOffice,:Classification,:Address1,:Address2,:Pincode,:City,:State,:Country,:FirstName,:PhnNo,:Mobile
              ,:Email,:PanNoGstNo,:Warehouse,:Purpose,:PrimaryCustomer,:LegalName,:AddressKey,:OracleCustomerCode,:OracleLocationCode,:ReadFromOracle,:InsertedOn)");

                return await ExecuteNonQueryAsync(sql.ToString(), customers);

            }
            catch { throw; }
        }
        public Task<List<ICustomerMasterDetails>> GetCustomerMasterPushDetails(IEntityDetails entityDetails)
        {
            throw new NotImplementedException();
        }
    }
}

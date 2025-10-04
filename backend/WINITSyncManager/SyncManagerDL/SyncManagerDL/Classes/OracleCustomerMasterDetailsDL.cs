using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using Winit.Shared.Models.Constants;

namespace SyncManagerDL.Classes
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
                //  var sql = new StringBuilder($@"insert into  {Int_OracleTableNames.CustomerMaster}(customer_name,sales_office,classification,address1,address2,pincode,city,state,country,first_name,phn_no,mobile
                //,email,pan_no_gst_no,warehouse,purpose,primary,legal_name,address_key,oracle_customer_code,oracle_location_code,read_from_oracle,inserted_on)
                //  values( :CustomerName,:SalesOffice,:Classification,:Address1,:Address2,:Pincode,:City,:State,:Country,:FirstName,:PhnNo,:Mobile
                //,:Email,:PanNoGstNo,:Warehouse,:Purpose,:PrimaryCustomer,:LegalName,:AddressKey,:OracleCustomerCode,:OracleLocationCode,:ReadFromOracle,:InsertedOn)");
                var sql = new StringBuilder($@"insert into  {Int_OracleTableNames.CustomerMaster}(customer_name,sales_office,classification,address1,address2,pincode,city,state,country,first_name,phn_no,mobile
              ,email,pan_no_gst_no,warehouse,purpose,primary,legal_name,address_key,oracle_customer_code,oracle_location_code,Site_Number,read_from_oracle)
                values( :CustomerName,:SalesOffice,:Classification,:Address1,:Address2,:Pincode,:City,:State,:Country,:FirstName,:PhnNo,:Mobile
              ,:Email,:PanNoGstNo,:Warehouse,:Purpose,:PrimaryCustomer,:LegalName,:AddressKey,:OracleCustomerCode,:OracleLocationCode,:Site_Number,:ReadFromOracle)");

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

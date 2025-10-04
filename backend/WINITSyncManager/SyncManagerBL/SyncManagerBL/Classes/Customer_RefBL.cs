using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class Customer_RefBL : ICustomer_RefBL
    {
        private readonly ICustomer_RefStagingDL customer_refStagingDL;
        private readonly ICustomer_RefDL customer_RefDL;
        public Customer_RefBL(ICustomer_RefStagingDL customer_refStagingDL, ICustomer_RefDL customer_RefDL)
        {
            this.customer_refStagingDL = customer_refStagingDL;
            this.customer_RefDL = customer_RefDL;
        }

        public async Task<List<ICustomer_Ref>> GetCustomerReferenceDetails(string sql)
        {
            return await customer_RefDL.GetCustomerReferenceDetails(sql);
        }

        public async Task<int> InsertCustomerReferenceDataIntoMonthTable(List<ICustomer_Ref> customer_Refs, IEntityDetails entityDetails)
        {
            return await customer_refStagingDL.InsertCustomerReferenceDataIntoMonthTable(customer_Refs, entityDetails);
        }
    }
}

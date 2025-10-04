using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface ICustomer_RefBL
    {
        Task<List<ICustomer_Ref>> GetCustomerReferenceDetails(string sql);
        Task<int> InsertCustomerReferenceDataIntoMonthTable(List<ICustomer_Ref> customer_Refs, IEntityDetails entityDetails);

    }
}

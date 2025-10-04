using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface ICustomer_RefStagingDL
    {
        Task<int> InsertCustomerReferenceDataIntoMonthTable(List<ICustomer_Ref> customer_Refs, IEntityDetails entityDetails);
    }
}

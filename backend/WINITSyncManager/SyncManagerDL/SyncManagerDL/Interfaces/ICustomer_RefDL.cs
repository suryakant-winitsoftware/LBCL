namespace SyncManagerDL.Interfaces
{
    public interface ICustomer_RefDL
    {
        Task<List<SyncManagerModel.Interfaces.ICustomer_Ref>> GetCustomerReferenceDetails(string sql);

    }
}

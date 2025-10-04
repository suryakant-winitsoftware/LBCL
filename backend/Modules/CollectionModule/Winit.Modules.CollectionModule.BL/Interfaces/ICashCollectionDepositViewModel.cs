using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface ICashCollectionDepositViewModel
    {
         string RequestNumber { get; set; } 
        List<IFileSys> FileSysList { get; set; }
        List<IAccCollection> AccCollections { get; set; }
        IAccCollectionDeposit CashCollectionDeposit { get; set; }
        Task<List<IAccCollection>> GetReceipts();
        Task<List<IAccCollection>> ViewReceipts(string RequestNo);
        Task<List<IAccCollectionDeposit>> GetRequestReceipts(string Status);
        Task<bool> CreateCashDepositRequest();
        Task<string> ApproveOrRejectDepositRequest(IAccCollectionDeposit accCollectionDeposit, string Status);
        Task PopulateViewModel();

    }
}

using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.CashCollectionDeposit
{
    public abstract class CashCollectionDepositBaseViewModel : ICashCollectionDepositViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public List<IFileSys> FileSysList { get; set; }
        public List<IAccCollection> AccCollections { get; set; }
        public IAccCollectionDeposit CashCollectionDeposit { get; set; } = new AccCollectionDeposit();
        public string RequestNumber { get; set; } 
        public CashCollectionDepositBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appUser = appUser;
            _appConfig = appConfig;
          
        }
        public async Task PopulateViewModel()
        {
            RequestNumber = _appUser.Emp.Code + "" + DateTime.Now.ToString("ddMMyyHHmmss");
        }

        public async Task<List<IAccCollection>> GetReceiptRecords()
        {
            try
            {
                return await GetReceipts();
            }
            catch(Exception ex)
            {
                throw new();
            }
        }

        public abstract Task<List<IAccCollection>> GetReceipts();
        public abstract Task<List<IAccCollection>> ViewReceipts(string RequestNo);
        public abstract Task<List<IAccCollectionDeposit>> GetRequestReceipts(string Status);
        public abstract Task<bool> CreateCashDepositRequest();
        public abstract Task<string> ApproveOrRejectDepositRequest(IAccCollectionDeposit accCollectionDeposit, string Status);
    }
}

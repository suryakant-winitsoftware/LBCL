using Newtonsoft.Json;
using System.Linq;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITMobile.Data;

namespace Winit.Modules.CollectionModule.BL.Classes.CashCollectionDeposit
{
    public class CashCollectionDepositAppViewModel : CashCollectionDepositBaseViewModel
    {
        protected IServiceProvider _serviceProvider { get; set; }
        protected ApiService _apiService { get; set; }
        protected IAppConfig _appConfigs { get; set; }
        protected IAppUser _appUser { get; set; }
        protected readonly ICollectionModuleBL _collectionModuleBL;
        protected IFileSysBL _fileSysBL { get; set; }
        public CashCollectionDepositAppViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ICollectionModuleBL collectionModuleBL, ApiService apiService, IFileSysBL fileSys) : base(serviceProvider, appConfig, appUser)
        {
            this._collectionModuleBL = collectionModuleBL;
            _serviceProvider = serviceProvider;
            _appConfigs = appConfig;
            _apiService = apiService;
            _appUser = appUser;
            _fileSysBL = fileSys;
        }

        public override async Task<List<IAccCollection>> GetReceipts()
        {
            try
            {
                return await _collectionModuleBL.GetReceipts();
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public override async Task<List<IAccCollection>> ViewReceipts(string RequestNo)
        {
            try
            {
                IAccCollectionAndDeposit accCollectionAndDeposits = new AccCollectionAndDeposit();
                accCollectionAndDeposits = await _collectionModuleBL.ViewReceipts(RequestNo);
                CashCollectionDeposit = accCollectionAndDeposits.accCollectionDeposits.First();
                return accCollectionAndDeposits.accCollections;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public override async Task<List<IAccCollectionDeposit>> GetRequestReceipts(string Status)
        {
            try
            {
                return await _collectionModuleBL.GetRequestReceipts(Status);
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public override async Task<bool> CreateCashDepositRequest()
        {
            try
            {
                 return await _collectionModuleBL.CreateCashDepositRequest(CashCollectionDeposit);
            }
            catch(Exception ex)
            {
                throw new Exception();
            }
        }

        public override async Task<string> ApproveOrRejectDepositRequest(IAccCollectionDeposit accCollectionDeposit, string Status)
        {
            bool result = await _collectionModuleBL.ApproveOrRejectDepositRequest(accCollectionDeposit, Status);
            if(result)
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }
        
    }
}

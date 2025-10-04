using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using Winit.Modules.ErrorHandling.Model.Classes;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ErrorHandling.BL.Classes
{
    public abstract class AddEditMaintainErrorDescriptionBaseViewModel : IAddEditMaintainErrorDescriptionViewModel
    {
        public bool IsEditErrorDescription { get; set; }
        public IErrorDetail ErrorDescriptionDetail { get; set; }
        private List<string> _propertiesToSearch = new List<string>();
        private readonly IAppUser _appUser;
        public IErrorDescriptionDetails ViewErrorDescriptionDetails { get; set; }
        public IErrorDetailsLocalization ErrorDetailsLocalization { get; set; }

        public string ErrorDescriptionCode {  get; set; }
        public AddEditMaintainErrorDescriptionBaseViewModel(IServiceProvider serviceProvider,
              IFilterHelper filter,
              ISortHelper sorter,
              IListHelper listHelper,
              IAppUser appUser,
              IAppSetting appSetting,
              IDataManager dataManager)
        {
            _appUser = appUser;
            ErrorDescriptionDetail = new ErrorDetail();

            ErrorDetailsLocalization = new ErrorDetailsLocalization();
            // Property set for Search
            _propertiesToSearch.Add("Code");
            _propertiesToSearch.Add("Name");
        }
        public virtual async Task PopulateErrorDescriptionViewModel(string UID)
        {
            if (IsEditErrorDescription)
            {
                ErrorDetailsLocalization = await GetErrorDescriptionDetailsByUID(UID);
            }
        }

        public async Task<bool> SaveOrUpdate()
        {
            if(string.IsNullOrEmpty(ErrorDetailsLocalization.UID))
            {
                ErrorDetailsLocalization.ErrorCode = ErrorDescriptionCode;
                AddCreateFields(ErrorDetailsLocalization, true);
            }
            return await CUDErrorDescriptionDetails(ErrorDetailsLocalization);
        }
        public async Task<bool> Update()
        {
            AddUpdateFields(ErrorDetailsLocalization);
            return await UpdateErrorDescriptionDetails(ErrorDetailsLocalization);
        }
        protected abstract Task<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetailsLocalization?> GetErrorDescriptionDetailsByUID(string errorUID);
        protected abstract Task<bool> CUDErrorDescriptionDetails(Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetailsLocalization errorDetail);
        protected abstract Task<bool> UpdateErrorDescriptionDetails(Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetailsLocalization errorDetail);

        #region Utils
        private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired)
        {
            baseModel.CreatedBy = _appUser?.Emp?.UID ?? "WINIT";
            baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT";
            baseModel.CreatedTime = DateTime.Now;
            baseModel.ModifiedTime = DateTime.Now;
            if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
        }
        private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
        {
            baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT"; //_appUser.Emp.UID;
            baseModel.ModifiedTime = DateTime.Now;
        }
        #endregion
    }
}

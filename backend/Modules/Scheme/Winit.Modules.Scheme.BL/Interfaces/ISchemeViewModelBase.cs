using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISchemeViewModelBase: ISchemeApprovalEngineBaseViewModel
    {
        List<ISKUV1> SKUV1s { get; set; }
        List<ISKUV1> SelectedSKUs { get; set; }
        List<IFileSys> FileSysList { get; set; }
        List<IFileSys> ModifiedFileSysList { get; set; }
        List<ISelectionItem> BranchDDL { get; set; }
        List<ISelectionItem> BroadClassificationDDL { get; set; }
        bool IsDisable {  get; set; }
        void OnFilesUpload(List<IFileSys> fileSys);
        IAddProductPopUpDataHelper _addProductPopUpDataHelper { get; }
        bool IsIntialize { get; set; }
        bool IsNew { get; set; }
        bool IsViewMode { get; set; }
        decimal? Branch_P2Amount { get; set; }
        decimal? HO_P3Amount { get; set; }
        decimal? HO_S_Amount { get; set; }
        decimal CreditLimit { get; set; }
        decimal AvailableLimit { get; set; }
        decimal CurrentOutStanding { get; set; }
        public IStore? SelectedChannelPartner { get; set; }
        List<IWallet> Wallets { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> ChannelPartner { get; set; }
        Task GetAllChannelPartner();
        Task OnChannelpartnerSelected(DropDownEvent dropDownEvent);
        Task GetSKUsAndPriceList_OnChannelpartnerSelection(DropDownEvent dropDownEvent);
        Task<ApiResponse<string>> UploadFiles(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> files);
        Task<bool> ValidateContributions(decimal? contribution1, decimal? contribution2, decimal? contribution3);
        void SetChannelPartnerSelectedonEditMode(string channelPartnerUID);
        bool IsNegativeValue(string inPut, out decimal val);

        void OnBroadClassificationSelected(DropDownEvent dropDownEvent);
        void OnChannelpartnerSelectedUI(DropDownEvent dropDownEvent);
        void OnBranchSelected(DropDownEvent dropDownEvent);
        List<IStore> AllCustomersByFilters();
    }
}

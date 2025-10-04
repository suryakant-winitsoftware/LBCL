using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using System.Resources;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.StoreDocument.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using WinIt.BreadCrum.Classes;


namespace WinIt.Pages.SalesManagement.Distributor
{
    public partial class AddDistributor
    {
        string FilePath = string.Empty;
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            toggle.IsInformation = true;
            SetColumnHeaders();
            await _distributorBaseViewModel.PopulateViewModel();
            await SetHeaderName();
            await base.OnInitializedAsync();
        }

        Winit.UIComponents.Common.FileUploader.FileUploader fileUploader;

        [CascadingParameter]
        public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.HeaderText = _distributorBaseViewModel.IsNewDistributor ? @Localizer["add_new_distributor"] : @Localizer["view/edit_distributor"];
            _IDataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = @Localizer["maintaindistributor"], IsClickable = true, URL = "maintaindistributor" });
            _IDataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 2, Text = _distributorBaseViewModel.IsNewDistributor ? @Localizer["add_new_distributor"] : @Localizer["view/edit_distributor"], IsClickable = false, URL = "maintaindistributor" });
            await CallbackService.InvokeAsync(_IDataService);
        }

        void EditDocs(Winit.Modules.StoreDocument.Model.Classes.StoreDocument StoreDocument)
        {

            _distributorBaseViewModel.EditDocs(StoreDocument);
            StateHasChanged();
        }
        async void ChangeDefaultContact()
        {
            //var isCon=_distributorBaseViewModel.ContactsList.Count > 0 && !_distributorBaseViewModel._Contact.IsDefault ? await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure you want to change the defaul contact"):true;
            //if (isCon)
            //{
            foreach (Contact contact in _distributorBaseViewModel.DistributorMasterView.Contacts)
            {
                contact.IsDefault = false;
            }
            //}
            _distributorBaseViewModel._Contact.IsDefault = !_distributorBaseViewModel._Contact.IsDefault;
            StateHasChanged();
        }
        void ShowDocuments()
        {
            FilePath = FileSysTemplateControles.GetStoreDocumentFolderPath(_distributorBaseViewModel.Distributor.Code);
            toggle.IsDocument = true;
            _distributorBaseViewModel.IsNewDoc = true;

        }
        void AddDocument()
        {

            _loadingService.ShowLoading();
            _distributorBaseViewModel.AddDocument();
            _loadingService.HideLoading();
            StateHasChanged();
        }

        async void Save()
        {
            _loadingService.ShowLoading();
            var isSaved = await _distributorBaseViewModel.Save();
            if (isSaved.Item1)
            {
                if (isSaved.Item2)
                {
                    ApiResponse<string> resp = await fileUploader.MoveFiles();
                    if (resp.IsSuccess)
                    {
                        bool isdone = await _distributorBaseViewModel.CreateStoreImage();
                        if (isdone)
                        {
                            _tost.Add("Success", "Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                            // await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["saved_successfully"]);
                            _navigationManager.NavigateTo("maintaindistributor");
                        }
                    }
                }
                else
                {
                    _navigationManager.NavigateTo("maintaindistributor");
                    _tost.Add("Success", "Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                }
            }
            _loadingService.HideLoading();
        }
        public List<DataGridColumn> Columns { get; set; }
        protected void SetColumnHeaders()
        {
            Columns = new List<DataGridColumn>
            {
                 new DataGridColumn { Header = "Code", GetValue = s => ((OrgCurrency)s).Code, IsSortable = false, SortField = "Code" },
                 new DataGridColumn { Header = "Name", GetValue = s => ((OrgCurrency)s).Name, IsSortable = false, SortField = "Name" },
                 new DataGridColumn { Header = "Symbol", GetValue = s =>((OrgCurrency)s).Symbol },
                 new DataGridColumn
                 {
                     Header = "Is Primary",
                     IsButtonColumn = true,
                     ButtonActions = new List<ButtonAction>
                     {
                         new ButtonAction
                         {
                             ButtonType=ButtonTypes.CheckBox,
                             GetValue = s => ((OrgCurrency)s).IsPrimary,
                             Text="Distributor Admin",
                             Action = async item =>await _distributorBaseViewModel.ChangePrimaryCurrency((IOrgCurrency)item)
                         },
                     }
                 },
                new DataGridColumn
                 {
                     Header = "Action",
                     IsButtonColumn = true,
                     ButtonActions = new List<ButtonAction>
                     {
                         new ButtonAction
                         {
                             ButtonType=ButtonTypes.Text,
                             Text="delete",
                             Action = async item =>await _distributorBaseViewModel.DeleteCurrency((IOrgCurrency)item)
                         },
                     }
                 }
           };
        }

    }
}

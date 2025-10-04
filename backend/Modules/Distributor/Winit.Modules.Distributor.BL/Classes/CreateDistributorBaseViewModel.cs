using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.Distributor.BL.Interfaces;
using Winit.Modules.Distributor.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.StoreDocument.Model.Classes;
using Winit.Modules.StoreDocument.Model.Interfaces;
using Winit.Modules.Tax.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.UIComponents.SnackBar;
using Winit.UIModels.Common;
using Winit.UIModels.Common.GST;

namespace Winit.Modules.Distributor.BL.Classes
{
    public class CreateDistributorBaseViewModel : ICreateDistributorBaseViewModel
    {
        IAppUser _iAppUser { get; set; }
        public CreateDistributorBaseViewModel(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs,
            CommonFunctions commonFunctions, NavigationManager navigationManager,
            Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService, IAppUser iAppUser,
            Winit.UIComponents.Common.Services.ILoadingService loadingService, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService, IToast toast)
        {
            _apiService = apiService;
            _appConfigs = appConfigs;
            _commonFunctions = commonFunctions;
            _navigationManager = navigationManager;
            _dataManager = dataManager;
            _alertService = alertService;
            _iAppUser = iAppUser;
            _loadingService = loadingService;
            _localizer = Localizer;
            _languageService = languageService;
            _toast = toast;

            _Contact = new();
            FileSysList = new();
            _StoreDocument = new()
            {
                UID = Guid.NewGuid().ToString(),
            };
        }
        IToast _toast;
        private readonly ILanguageService _languageService;
        private IStringLocalizer<LanguageKeys> _localizer;
        Winit.UIComponents.Common.Services.ILoadingService _loadingService;
        public ApiService _apiService { get; set; }
        public Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
        public CommonFunctions _commonFunctions { get; set; }
        public NavigationManager _navigationManager { get; set; }
        public Common.Model.Interfaces.IDataManager _dataManager { get; set; }
        public IAlertService _alertService { get; set; }
        public bool IsStatusVisible { get; set; }
        public string StatusLable { get; set; } = "Select Status";
        public bool IsShowPopUp { get; set; }
        public string FilePath { get; set; }
        public Model.Classes.Distributor Distributor { get; set; }
        public DistributorMasterView DistributorMasterView { get; set; }
        public Contact.Model.Classes.Contact _Contact { get; set; } = new();
        public Winit.Modules.StoreDocument.Model.Classes.StoreDocument _StoreDocument { get; set; }
        public List<ISelectionItem> DocumentList { get; set; } = new();

        public GSTINDetailsModel GSTINDetails { get; set; }
        public string? DocumentLabel { get; set; } = "Select Document Type";
        public bool IsDocumentVisible { get; set; }
        public List<DataGridColumn> Columns { get; set; }
        public List<IFileSys> DisplayFileSysList { get; set; }
        List<IFileSys> FileSysList;
        List<IFileSys> modifiedFileSysList;
        public List<ISelectionItem> StatusList { get; set; } = new() {
            new SelectionItem() { UID="1",Label="Active",Code="Active"},
            new SelectionItem() { UID = "0", Label = "In Active", Code = "InActive" },
            new SelectionItem() { UID="-1",Label="Blocked",Code="Blocked"}
        };
        public List<ISelectionItem> TaxGroupList { get; set; }
        public bool IsTaxGroupVisible { get; set; }
        public string TaxGroupLabel { get; set; } = "Select Tax Group";
        public List<ISelectionItem> CurrencyList { get; set; } = new List<ISelectionItem>();
        public List<Currency.Model.Classes.Currency> BackUpCurrencyList { get; set; }
        public bool IsCurrencyVisible { get; set; }
        public string CurrencyLabel { get; set; } = "Select Currency";
        public bool IsLoad { get; set; }
        public bool IsNewDistributor { get; set; }
        string pagetype = string.Empty;

        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        public async Task PopulateViewModel()
        {
            _loadingService.ShowLoading();
            LoadResources(null, _languageService.SelectedCulture);
            IsLoad = false;
            Distributor = new();
            await Task.WhenAll(
             GetTaxGroup(),
             PopulateCurrencyList(),
             getListItemsAsync());
            IsNewDistributor = PageType.New.Equals(_commonFunctions.GetParameterValueFromURL(PageType.Page));
            if (IsNewDistributor)
            {
                DistributorMasterView = new()
                {
                    Org = new(),
                    Store = new(),
                    StoreAdditionalInfo = new(),
                    StoreCredit = new(),
                    Contacts = new(),
                    Documents = new(),
                    Address = new(),
                    OrgCurrencyList = new(),
                };
            }
            else
            {
                string distributorOrgUID = _commonFunctions.GetParameterValueFromURL("UID");
                await GetDataFromAPIAsync(distributorOrgUID);
                if (DistributorMasterView.Documents?.Count > 0)
                {
                    await GetStoreImages(distributorOrgUID);
                }
            }
            SetColumnHeaders();
            IsLoad = true;
            _loadingService.HideLoading();
        }

        #region Getting Data and Populating in Required Object for View Or Edit
        protected async Task GetStoreImages(string DistributorUID)
        {

            PagingRequest request = new()
            {
                FilterCriterias = new()
                {
                    new FilterCriteria("LinkedItemUID",DistributorMasterView.Documents?.Select(p=>p.UID)?.ToArray(),FilterType.In),
                }
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}FileSys/SelectAllFileSysDetails", HttpMethod.Post, request);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                PagedResponse<FileSys.Model.Classes.FileSys>? response = (JsonConvert.DeserializeObject<PagedResponse<FileSys.Model.Classes.FileSys>>(_commonFunctions.GetDataFromResponse(apiResponse.Data)));
                if (response != null && response.PagedData != null)
                {
                    FileSysList = response.PagedData.ToList<IFileSys>();
                }
                //DisplayFileSysList = FileSysList;
            }
        }
        private async Task PopulateCurrencyList()
        {
            PagingRequest pagingRequest = new PagingRequest()
            {
                IsCountRequired = true,
                PageNumber = 1,
                PageSize = 100,
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Currency/GetCurrencyDetails", HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                PagedResponse<Currency.Model.Classes.Currency>? response = JsonConvert.DeserializeObject<PagedResponse<Currency.Model.Classes.Currency>>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                if (response?.PagedData != null)
                {
                    CurrencyList = new();
                    BackUpCurrencyList = new();
                    foreach (Currency.Model.Classes.Currency currency in response.PagedData)
                    {
                        BackUpCurrencyList.Add(currency);
                        CurrencyList.Add(new SelectionItem()
                        {
                            UID = currency.UID,
                            Code = currency.Code,
                            Label = $"[{currency.Symbol}]{currency.Name}"
                        });
                    }
                }
            }
        }
        public void GetFilesysList(List<IFileSys>? fileSys)
        {
            if (fileSys != null)
            {
                FileSysList?.AddRange(fileSys);
                this.modifiedFileSysList = fileSys;
            }
        }
        protected async Task getListItemsAsync()
        {
            DocumentList = new();
            try
            {
                ListItemRequest request = new()
                {
                    Codes = new()
                {
                  "DocumentType",
                },
                    isCountRequired = true
                };
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                 $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                 HttpMethod.Post, request);
                if (apiResponse.Data != null)
                {
                    var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>(data);
                    if (pagedResponse != null)
                    {
                        try
                        {

                            if (pagedResponse.TotalCount > 0)
                            {

                                foreach (var item in pagedResponse.PagedData)
                                {
                                    DocumentList.Add(new SelectionItem() { UID = item.UID, Code = item.Code, Label = item.Name });
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task GetTaxGroup()
        {
            TaxGroupList = new();
            PagingRequest pagingRequest = new PagingRequest()
            {
                IsCountRequired = true,
                PageNumber = 1,
                PageSize = 100,
            };
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Tax/GetTaxGroupDetails", HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                PagedResponse<TaxGroup>? response = JsonConvert.DeserializeObject<PagedResponse<TaxGroup>>(_commonFunctions.GetDataFromResponse(apiResponse.Data));
                if (response?.PagedData != null)
                {
                    foreach (TaxGroup item in response.PagedData)
                    {
                        ISelectionItem? selectionItem = new SelectionItem() { UID = item.UID, Code = item.Code, Label = item.Name };
                        //if (selectionItem?.UID == Distributor?.TaxGroupUID)
                        //{
                        //    TaxGroupLabel = selectionItem?.Label;
                        //}
                        TaxGroupList?.Add(selectionItem);
                    }
                }
            }
        }
        private async Task GetDataFromAPIAsync(string distributorOrgUID)
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Distributor/GetDistributorDetailsByUID?UID={distributorOrgUID}",
                    HttpMethod.Get);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<DistributorMasterView> response = JsonConvert.DeserializeObject<ApiResponse<DistributorMasterView>>(apiResponse.Data);
                    if (response != null)
                    {

                        if (response.Data != null)
                        {
                            DistributorMasterView = response.Data;
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    //  _totalItems = 0;
                }
            }
            catch (Exception ex)
            {
                // _totalItems = 0;
            }
            finally
            {
                SetEditMode();
            }
        }
        private void SetEditMode()
        {
            if (DistributorMasterView != null)
            {
                if (DistributorMasterView.Org != null)
                {
                    Distributor.Code = DistributorMasterView.Org.Code;
                    Distributor.Status = DistributorMasterView.Org.Status;
                    StatusLable = DistributorMasterView.Org.Status;
                    Distributor.SequenceCode = DistributorMasterView.Org.SeqCode;
                    Distributor.TaxGroupUID = DistributorMasterView.Org.TaxGroupUID;
                    TaxGroupLabel = TaxGroupList?.Find(p => p.Code == DistributorMasterView.Org.TaxGroupUID)?.Label;
                }
                if (DistributorMasterView.Store != null)
                {
                    Distributor.Name = DistributorMasterView.Store.Name;
                    // Distributor.OpenAccountDate = DistributorMasterView.StoreAdditionalInfo.CustomerStartDate;
                    //StoreUID = DistributorMasterView.StoreAdditionalInfo.StoreUID;
                }
                if (DistributorMasterView.StoreAdditionalInfo != null)
                {
                    Distributor.OpenAccountDate = DistributorMasterView.StoreAdditionalInfo.CustomerStartDate;
                    //StoreUID = DistributorMasterView.StoreAdditionalInfo.StoreUID;
                }
                if (DistributorMasterView.Contacts == null)
                {
                    DistributorMasterView.Contacts = new();
                }
                if (DistributorMasterView.Documents != null)
                {
                    for (int i = 0; i < DistributorMasterView.Documents.Count; i++)
                    {
                        DistributorMasterView.Documents[i].DocumentLabel = DocumentList.Find(p => p.Code == DistributorMasterView.Documents[i].DocumentType)?.Label;
                    }
                }
                if (DistributorMasterView.Address == null)
                {
                    DistributorMasterView.Address = new();
                }
                if (DistributorMasterView.OrgCurrencyList != null && DistributorMasterView.OrgCurrencyList.Count > 0)
                {
                    foreach (var item in DistributorMasterView.OrgCurrencyList)
                    {
                        var currency = BackUpCurrencyList.Find(p => p.UID == item.CurrencyUID);
                        item.Code = currency?.Code;
                        item.Name = currency?.Name;
                    }
                }
                else
                {
                    DistributorMasterView.OrgCurrencyList = new();
                }
            }
            else
            {
                DistributorMasterView = new()
                {
                    Org = new(),
                    Store = new(),
                    StoreAdditionalInfo = new(),
                    StoreCredit = new(),
                    Contacts = new(),
                    Documents = new(),
                    Address = new(),
                    OrgCurrencyList = new(),
                };
            }
        }
        #endregion

        #region UI Logic
        public void OnDropDownSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null)
            {
                ISelectionItem? selectionItem = dropDownEvent.SelectionItems?.FirstOrDefault<ISelectionItem>();
                if (selectionItem != null)
                {
                    _StoreDocument.DocumentType = selectionItem.Code;
                    DocumentLabel = selectionItem.Label;
                }
            }
            else
            {
                _StoreDocument.DocumentType = string.Empty;
                DocumentLabel = @_localizer["select_document_type"];
            }
            IsDocumentVisible = false;
        }
        public async Task ChangePrimaryCurrency(IOrgCurrency orgCurrency)
        {
            if (orgCurrency.IsPrimary)
            {
                bool isConfirm = await _alertService.ShowConfirmationReturnType(@_localizer["confirm"], @_localizer["are_you_sure_you_want_to_remove_this_as_primary"], @_localizer["yes"], @_localizer["no"]);
                if (!isConfirm)
                {
                    return;
                }
            }
            foreach (OrgCurrency currency in DistributorMasterView.OrgCurrencyList)
            {
                if (orgCurrency.CurrencyUID == currency.CurrencyUID)
                {
                    currency.IsPrimary = !currency.IsPrimary;
                }
                else
                {
                    currency.IsPrimary = false;
                }
            }
        }
        public void OnStatuseSelected(DropDownEvent dropDownEvent)
        {
            IsStatusVisible = false;
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null)
                {
                    ISelectionItem? selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                    if (selectionItem != null)
                    {
                        StatusLable = selectionItem.Label;
                        Distributor.Status = selectionItem.Code;
                    }
                    else
                    {
                        StatusLable = @_localizer["select_status"];
                        Distributor.Status = string.Empty;
                    }
                }
                else
                {
                    Distributor.Status = string.Empty;
                    StatusLable = @_localizer["select_status"];
                }
            }
            //else
            //{
            //    Distributor.Status = string.Empty;
            //    StatusLable = @_localizer["select_status"];
            //}
        }
        public void OnTaxGroupSelected(DropDownEvent dropDownEvent)
        {
            IsTaxGroupVisible = false;
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null)
                {
                    ISelectionItem? selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                    if (selectionItem != null)
                    {
                        TaxGroupLabel = selectionItem.Label;
                        Distributor.TaxGroupUID = selectionItem.Code;
                    }
                    else
                    {
                        TaxGroupLabel = @_localizer["select_tax_group"];
                        Distributor.TaxGroupUID = string.Empty;
                    }
                }
                else
                {
                    TaxGroupLabel = @_localizer["select_tax_group"];
                    Distributor.TaxGroupUID = string.Empty;
                }
            }
            //else
            //{
            //    TaxGroupLabel = @_localizer["select_tax_group"];
            //    Distributor.TaxGroupUID = string.Empty;
            //}
        }
        List<ISelectionItem> SelectedCurrency { get; set; }
        public void AddCurrencies()
        {
            if (SelectedCurrency != null && SelectedCurrency.Any())
            {
                foreach (ISelectionItem selectionItem in SelectedCurrency)
                {
                    ICurrency currency = _iAppUser?.OrgCurrencyList?.Find(p => p.UID == selectionItem?.UID);
                    if (currency != null)
                    {
                        //  SelectedCurrencies.Add(currency);
                    }
                }
            }
        }
        public void OnCurrencySelected(DropDownEvent dropDownEvent)
        {
            IsCurrencyVisible = false;
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null)
                {
                    if (dropDownEvent.SelectionItems.Count > 0)
                    {
                        foreach (ISelectionItem selectionItem in dropDownEvent.SelectionItems)
                        {
                            bool isExist = DistributorMasterView.OrgCurrencyList.Any(p => p.CurrencyUID == selectionItem.UID);
                            if (isExist)
                            {

                            }
                            else
                            {
                                Currency.Model.Classes.Currency? currency = BackUpCurrencyList.Find(p => p.UID == selectionItem.UID);
                                DistributorMasterView.OrgCurrencyList.Add(new OrgCurrency()
                                {
                                    UID = Guid.NewGuid().ToString(),
                                    Code = selectionItem.Code,
                                    CurrencyUID = selectionItem.UID,
                                    Name = currency?.Name,
                                    Symbol = currency?.Symbol,
                                    OrgUID = Distributor.Code,
                                });
                            }
                        }
                    }
                }
                else
                {
                    CurrencyLabel = @_localizer["select_currency"];
                }
            }
            //else
            //{
            //    CurrencyLabel = @_localizer["select_currency"];
            //}
        }
        public void EditContact(Contact.Model.Classes.Contact contact)
        {
            _Contact = contact;
            IsShowPopUp = true;
        }
        public void AddContact()
        {
            if (string.IsNullOrEmpty(_Contact.UID))
            {
                _Contact.UID = Guid.NewGuid().ToString();
                _Contact.LinkedItemUID = Distributor.Code;
                _Contact.CreatedBy = _iAppUser.Emp.UID;
                _Contact.CreatedTime = DateTime.Now;
                DistributorMasterView.Contacts.Add(_Contact);
            }
            if (CommonFunctions.IsValidEmail(_Contact.Email))
            {
                _toast.Add("Error", "Please enter valid email");
            }
            _Contact.ModifiedBy = _iAppUser.Emp.UID;
            _Contact.ModifiedTime = DateTime.Now;
            _Contact = new();
            IsShowPopUp = false;
        }
        public void CloseContactTab()
        {
            IsShowPopUp = false;
            _Contact = new();
        }
        public bool IsNewDoc { get; set; } = true;
        public void EditDocs(Winit.Modules.StoreDocument.Model.Classes.StoreDocument StoreDocument)
        {
            IsNewDoc = false;
            DisplayFileSysList = FileSysList.FindAll(p => p.LinkedItemUID == StoreDocument?.UID);
            DocumentLabel = DocumentList?.Find(p => p.Code == StoreDocument?.DocumentType)?.Label;
            if (DocumentList != null)
            {
                foreach (var doc in DocumentList)
                {
                    if (doc.Code == StoreDocument?.DocumentType)
                    {
                        doc.IsSelected = true;
                    }
                    else { doc.IsSelected = false; }
                }
            }

            _StoreDocument = StoreDocument;
        }
        public void AddDocument()
        {
            _StoreDocument.ModifiedTime = DateTime.Now;
            _StoreDocument.ModifiedBy = _iAppUser.Emp.UID;
            bool isExist = DistributorMasterView.Documents.Any(p =>
            _StoreDocument.DocumentType == p.DocumentType && p.DocumentNo == _StoreDocument.DocumentNo);
            if (IsNewDoc && !isExist)
            {
                _StoreDocument.StoreUID = Distributor.Code;
                _StoreDocument.CreatedBy = _iAppUser.Emp.UID;
                _StoreDocument.CreatedTime = DateTime.Now;
                DistributorMasterView?.Documents?.Add(_StoreDocument);
            }

            _StoreDocument = new()
            {
                UID = Guid.NewGuid().ToString(),
            };
            DocumentLabel = @_localizer["select_document_type"];
            DisplayFileSysList = [];
            IsNewDoc = true;
        }
        protected void CreateDocumentsInstance()
        {

        }
        protected void SetColumnHeaders()
        {
            Columns = new List<DataGridColumn>
            {
                 new DataGridColumn { Header = @_localizer["code"] , GetValue = s => ((OrgCurrency)s).Code, IsSortable = false, SortField = "Code" },
                 new DataGridColumn { Header = @_localizer["name"] , GetValue = s => ((OrgCurrency)s).Name, IsSortable = false, SortField = "Name" },
                 new DataGridColumn { Header = @_localizer["symbol"] , GetValue = s =>((OrgCurrency)s).Symbol },
                 new DataGridColumn
                 {
                     Header = @_localizer["is_primary"] ,
                     IsButtonColumn = true,
                     ButtonActions = new List<ButtonAction>
                     {
                         new ButtonAction
                         {
                             ButtonType=ButtonTypes.CheckBox,
                             GetValue = s => ((OrgCurrency)s).IsPrimary,
                             Text=@_localizer["distributor_admin"],
                             Action = async item =>await ChangePrimaryCurrency((IOrgCurrency)item)
                         },
                     }
                 },
                new DataGridColumn
                 {
                     Header = @_localizer["action"],
                     IsButtonColumn = true,
                     ButtonActions = new List<ButtonAction>
                     {
                         new ButtonAction
                         {
                             ButtonType=ButtonTypes.Text,
                             Text=@_localizer["delete"],
                             Action = async item =>await DeleteCurrency((IOrgCurrency)item)
                         },
                     }
                 }
           };
        }
        #endregion

        #region Save 
        protected Validation Validation()
        {
            string message = string.Empty;
            bool isValidated = true;
            if (string.IsNullOrEmpty(Distributor.Code))
            {
                message += @_localizer["code"] + " " + ",";
                isValidated = false;
            }
            if (string.IsNullOrEmpty(Distributor.Name))
            {
                message += @_localizer["name"] + " " + ",";
                isValidated = false;
            }
            if (string.IsNullOrEmpty(Distributor.SequenceCode))
            {
                message += @_localizer["sequence_code"] + " " + ",";
                isValidated = false;
            }
            if (!isValidated)
            {
                message = message.Substring(0, message.Length - 2);
            }
            if (isValidated)
            {
                message += @_localizer["select_default_:"];
                bool isContactval = DistributorMasterView.Contacts.Any(p => p.IsDefault);
                if (!isContactval)
                {
                    message += @_localizer["contact"] + " ";
                    isValidated = false;
                }
                if (!isValidated)
                {
                    message += " " + @_localizer["and"] + " ";
                }
                if (!DistributorMasterView.OrgCurrencyList.Any(p => p.IsPrimary))
                {
                    message += @_localizer["currency"];
                    isValidated = false;
                }
            }
            return new Validation(isValidated, message);
        }

        public async Task<(bool, bool)> Save()
        {
            Validation validation = Validation();
            _loadingService.ShowLoading();
            if (validation.IsValidated)
            {
                SaveOrg();
                SaveAddress();
                SaveStore();
                SaveStoreAdditionalInfo();
                SaveStoreCredit();
                // DistributorMasterView.Contacts = ContactsList;
                //DistributorMasterView.Documents = StoreDocumentList;
                // DistributorMasterView.OrgCurrencyList = SelectedCurrencyList;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Distributor/CreateDistributor", HttpMethod.Post, DistributorMasterView);
                if (apiResponse != null)
                {
                    if (apiResponse.IsSuccess)
                    {
                        return (true, FileSysList != null ? FileSysList.Any(p => p.Id <= 0) : false);
                        if (FileSysList != null && FileSysList.Count > 0)
                        {

                            // return await CreateStoreImage(FileSysList.Where(p => p.Id <= 0).ToList());
                        }
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert(@_localizer["error"], apiResponse.ErrorMessage);
                    }
                }
            }
            else
            {
                await _alertService.ShowErrorAlert(@_localizer["error"], $"{@_localizer["the_following_field(s)_have_invalid_values_:"]}{validation.ErrorMessage}");
            }
            return (false, false);
        }
        public async Task<bool> CreateStoreImage()
        {
            dynamic obj = FileSysList.Where(p => p.Id <= 0);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}FileSys/CreateFileSysForBulk", HttpMethod.Post, obj);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                return true;
                //ApiResponse<string>? response = JsonConvert.DeserializeObject<PagedResponse<string>>(apiResponse.Data);
            }
            return false;
        }
        protected void SaveAddress()
        {
            if (string.IsNullOrEmpty(DistributorMasterView.Address.UID))
            {
                DistributorMasterView.Address.CreatedBy = _iAppUser.Emp.UID;
                DistributorMasterView.Address.CreatedTime = DateTime.Now;
                DistributorMasterView.Address.LinkedItemUID = Distributor.Code;
                DistributorMasterView.Address.UID = Guid.NewGuid().ToString();
                DistributorMasterView.Address.Type = "FR";
            }
            DistributorMasterView.Address.ModifiedTime = DateTime.Now;
            DistributorMasterView.Address.ModifiedBy = _iAppUser.Emp.UID;
            DistributorMasterView.Address.ModifiedBy = _iAppUser.Emp.UID;
        }
        protected void SaveOrg()
        {
            if (IsNewDistributor)
            {
                DistributorMasterView.Org.UID = Distributor.Code;
                DistributorMasterView.Org.OrgTypeUID = "FR";
                DistributorMasterView.Org.ParentUID = _iAppUser.SelectedJobPosition.OrgUID;
                DistributorMasterView.Org.CreatedBy = _iAppUser.Emp.UID;
                DistributorMasterView.Org.CreatedTime = DateTime.Now;
                DistributorMasterView.Org.ServerAddTime = DateTime.Now;
                DistributorMasterView.Org.CompanyUID = _iAppUser.Emp.CompanyUID;
            }
            DistributorMasterView.Org.ModifiedBy = _iAppUser.Emp.UID;
            DistributorMasterView.Org.ModifiedTime = DateTime.Now;
            DistributorMasterView.Org.Code = Distributor.Code;
            DistributorMasterView.Org.Name = Distributor.Name;
            DistributorMasterView.Org.SeqCode = Distributor.SequenceCode;
            DistributorMasterView.Org.Status = Distributor.Status;
            DistributorMasterView.Org.TaxGroupUID = Distributor.TaxGroupUID;
        }
        protected void SaveStore()
        {
            if (IsNewDistributor)
            {
                DistributorMasterView.Store.UID = Distributor.Code;
                DistributorMasterView.Store.CompanyUID = _iAppUser.Emp.CompanyUID;
                DistributorMasterView.Store.Type = "FR";
                DistributorMasterView.Store.CreatedBy = _iAppUser.Emp.UID;
                DistributorMasterView.Store.ModifiedBy = _iAppUser.Emp.UID;
                DistributorMasterView.Store.CreatedTime = DateTime.Now;
                DistributorMasterView.Store.ServerAddTime = DateTime.Now;
                DistributorMasterView.Store.IsActive = true;
                DistributorMasterView.Store.FranchiseeOrgUID = Distributor.Code;
            }
            DistributorMasterView.Store.ModifiedTime = DateTime.Now;
            DistributorMasterView.Store.Code = Distributor.Code;
            DistributorMasterView.Store.Name = Distributor.Name;
            DistributorMasterView.Store.Number = Distributor.Code;
        }
        protected void SaveStoreAdditionalInfo()
        {
            if (IsNewDistributor && string.IsNullOrEmpty(DistributorMasterView.StoreAdditionalInfo.UID))
            {
                DistributorMasterView.StoreAdditionalInfo.UID = Guid.NewGuid().ToString();
                DistributorMasterView.StoreAdditionalInfo.StoreUID = Distributor.Code;
                DistributorMasterView.StoreAdditionalInfo.CreatedBy = _iAppUser.Emp.UID;
                DistributorMasterView.StoreAdditionalInfo.CreatedTime = DateTime.Now;
                DistributorMasterView.StoreAdditionalInfo.ServerAddTime = DateTime.Now;
            }
            DistributorMasterView.StoreAdditionalInfo.CustomerStartDate = Distributor.OpenAccountDate;
            DistributorMasterView.StoreAdditionalInfo.ModifiedTime = DateTime.Now;
            DistributorMasterView.StoreAdditionalInfo.ModifiedBy = _iAppUser.Emp.UID;
        }
        protected void SaveStoreCredit()
        {
            if (IsNewDistributor && string.IsNullOrEmpty(DistributorMasterView.StoreCredit.UID))
            {
                DistributorMasterView.StoreCredit.UID = Guid.NewGuid().ToString();
                DistributorMasterView.StoreCredit.StoreUID = Distributor.Code;
                DistributorMasterView.StoreCredit.CreatedBy = _iAppUser.Emp.UID;
                DistributorMasterView.StoreCredit.ModifiedBy = _iAppUser.Emp.UID;
                DistributorMasterView.StoreCredit.CreatedTime = DateTime.Now;
                DistributorMasterView.StoreCredit.ModifiedTime = DateTime.Now;
                DistributorMasterView.StoreCredit.ServerAddTime = DateTime.Now;
            }
        }
        #endregion

        public async Task DeleteCurrency(IOrgCurrency orgCurrency)
        {
            bool isConfirm = await _alertService.ShowConfirmationReturnType(@_localizer["confirm"], @_localizer["are_you_sure_you_want_to_delete(this_will_be_deleted_immediately)"], @_localizer["yes"], @_localizer["no"]);
            if (isConfirm)
            {
                if (orgCurrency.Id == 0)
                {
                    DistributorMasterView.OrgCurrencyList = DistributorMasterView.OrgCurrencyList.FindAll(p => p.CurrencyUID == orgCurrency.CurrencyUID);
                    _toast.Add("Success", "Deleted from grid");
                    return;
                }
            }
            else
            {
                return;
            }
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Currency/DeleteOrgCurrency?UID={orgCurrency.UID}", HttpMethod.Delete);
            if (apiResponse != null)
            {
                if (apiResponse.IsSuccess)
                {
                    _toast.Add(@_localizer["success"], @_localizer["deleted_successfully"]);
                    DistributorMasterView.OrgCurrencyList = DistributorMasterView.OrgCurrencyList.FindAll(p => p.CurrencyUID == orgCurrency.CurrencyUID);
                }
                else
                {
                    await _alertService.ShowErrorAlert(@_localizer["erroe"], apiResponse.ErrorMessage);
                }
            }
        }


        #region  Calling GstNumValidationAPI
        public async Task<bool> GetGstDetails(string gstNumber)
        {
            try
            {
                GSTINDetails = new GSTINDetailsModel();
                var gstNumDetails = await _apiService.FetchDataAsync(
               $"{_appConfigs.ApiBaseUrl}GST/GstinSearch", HttpMethod.Post, gstNumber);
                if (gstNumDetails.IsSuccess)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(gstNumDetails.Data);
                    //GstNumDetails fetchedapiData = JsonConvert.DeserializeObject<GstNumDetails>(data);
                    GSTINDetails = ConvertSearchGSTINJsonToModel(gstNumDetails.Data);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }
        private GSTINDetailsModel ConvertSearchGSTINJsonToModel(string jsonString)
        {
            var jsonData = JObject.Parse(jsonString);

            // Access the "Data" object from the parsed JObject
            var data = jsonData["Data"];

            // Map properties from JSON to your model
            return new GSTINDetailsModel
            {
                StateJurisdictionCode = data["stjCd"]?.ToString(),
                LegalName = data["lgnm"]?.ToString(),
                StateJurisdiction = data["stj"]?.ToString(),
                Duty = data["dty"]?.ToString(),
                CancellationDate = data["cxdt"]?.ToString(),
                GSTIN = data["gstin"]?.ToString(),
                NatureOfBusinessActivity = data["nba"] != null ? string.Join(", ", jsonData["nba"]) : string.Empty,
                LastUpdate = data["lstupdt"]?.ToString(),
                RegistrationDate = data["rgdt"]?.ToString(),
                ConstitutionOfBusiness = data["ctb"]?.ToString(),
                TradeName = data["tradeNam"]?.ToString(),
                Status = data["sts"]?.ToString(),
                CentralJurisdictionCode = data["ctjCd"]?.ToString(),
                CentralJurisdiction = data["ctj"]?.ToString(),
                AR_ADR_BuildingName = data["adadr"]?[0]?["addr"]?["bnm"]?.ToString(),
                AR_ADR_Street = data["adadr"]?[0]?["addr"]?["st"]?.ToString(),
                AR_ADR_Location = data["adadr"]?[0]?["addr"]?["loc"]?.ToString(),
                AR_ADR_DoorNo = data["adadr"]?[0]?["addr"]?["bno"]?.ToString(),
                AR_ADR_State = data["adadr"]?[0]?["addr"]?["stcd"]?.ToString(),
                AR_ADR_FloorNo = data["adadr"]?[0]?["addr"]?["flno"]?.ToString(),
                AR_ADR_Latitude = data["adadr"]?[0]?["addr"]?["lt"]?.ToObject<double>() ?? 0,
                AR_ADR_Longitude = data["adadr"]?[0]?["addr"]?["lg"]?.ToObject<double>() ?? 0,
                AR_ADR_Pincode = data["adadr"]?[0]?["addr"]?["pncd"]?.ToString(),
                AR_NatureOfBusiness = data["adadr"]?[0]?["ntr"] != null ? string.Join(", ", jsonData["adadr"][0]["ntr"]) : string.Empty,
                PR_ADDR_BuildingName = data["pradr"]?["addr"]?["bnm"]?.ToString(),
                PR_ADR_Street = data["pradr"]?["addr"]?["st"]?.ToString(),
                PR_ADR_Location = data["pradr"]?["addr"]?["loc"]?.ToString(),
                PR_ADR_DoorNo = data["pradr"]?["addr"]?["bno"]?.ToString(),
                PR_ADR_State = data["pradr"]?["addr"]?["stcd"]?.ToString(),
                PR_ADR_FloorNo = data["pradr"]?["addr"]?["flno"]?.ToString(),
                PR_ADR_Latitude = data["pradr"]?["addr"]?["lt"]?.ToObject<double>() ?? 0,
                PR_ADR_Longitude = data["pradr"]?["addr"]?["lg"]?.ToObject<double>() ?? 0,
                PR_ADR_Pincode = data["pradr"]?["addr"]?["pncd"]?.ToString(),
                PR_NatureOfBusiness = data["pradr"]?["ntr"] != null ? string.Join(", ", jsonData["pradr"]["ntr"]) : string.Empty,
            };
        }
        #endregion
    }
}

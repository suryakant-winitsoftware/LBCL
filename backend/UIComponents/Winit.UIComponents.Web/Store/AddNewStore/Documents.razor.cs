using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.StoreDocument.Model.Classes;
using Winit.Modules.StoreDocument.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.UIModels.Web.Store;

namespace Winit.UIComponents.Web.Store.AddNewStore
{
    public partial class Documents : ComponentBase
    {
        [Parameter] public List<IStoreDocument> StoreDocuments { get; set; } = new List<IStoreDocument>();
        [Parameter]
        public IStoreDocument StoreDocument { get; set; } = new Winit.Modules.StoreDocument.Model.Classes.StoreDocument()
        {
            UID = Guid.NewGuid().ToString(),
            IsNewDoc = true,
            DocumentLabel = DocumentLabel
        };
        [Parameter] public List<ListItem> ListItems { get; set; }
        [Parameter] public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> FileSysList { get; set; } = new List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>();

        private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader { get; set; }
        bool isLoad { get; set; }
        [Parameter] public string StoreUID { get; set; }
        [Parameter] public bool isNewStore { get; set; }
        [Parameter] public EventCallback<string> Response { get; set; }

        HttpClient http = new HttpClient();
        List<ISelectionItem> DocumentList { get; set; }

        const string DocumentLabel = "Select Document Type";
        bool IsDocumentVisible { get; set; }
        bool showHideFileUploader;
        string FilePath { get; set; }
        IList<Winit.Modules.FileSys.Model.Interfaces.IFileSys> modifiedFileSysList { get; set; }
        List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>? DisplayFileSysList { get; set; } = new();
        List<DataGridColumn> dataGridColumns;
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            showHideFileUploader = true;
            if (!string.IsNullOrEmpty(StoreUID))
            {
                FilePath = FileSysTemplateControles.GetStoreDocumentFolderPath(StoreUID);
            }
            _loadingService.ShowLoading();
            StoreDocument.ValidFrom = DateTime.Now;
            StoreDocument.ValidUpTo = DateTime.Now;
            DocumentList = new();
            foreach (ListItem item in ListItems)
            {
                if (item.ListHeaderUID == "DocumentType")
                {
                    DocumentList.Add(new SelectionItem() { UID = item.UID, Code = item.Code, Label = item.Name });
                }
            }
            await GetStoreDocuments();
            if (StoreDocuments != null && StoreDocuments.Count > 0)
            {

            }
            dataGridColumns = new List<DataGridColumn>()
            {
                new DataGridColumn(){Header="Document Type",GetValue=s=>((StoreDocument)s).DocumentLabel},
                new DataGridColumn(){Header="Document Number",GetValue=s=>((StoreDocument)s).DocumentNo},
                new DataGridColumn(){Header="Valid From",GetValue=s=>CommonFunctions.GetDateTimeInFormat(((StoreDocument)s).ValidFrom)},
                new DataGridColumn(){Header="Valid Upto",GetValue=s=>CommonFunctions.GetDateTimeInFormat(((StoreDocument)s).ValidUpTo)},
                new DataGridColumn()
                {
                    Header="Action", IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>()
                    {
                        new ButtonAction()
                        {
                            ButtonType=ButtonTypes.Text,
                            Text="View/Edit",
                            Action=item=>EditDocs((IStoreDocument)item)
                        }
                    }
                }
            };
            isLoad = true;
            _loadingService.HideLoading();
        }
       
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        public bool isEdited { get; set; } = false;
        //public bool IsComponentEdited()
        //{
        //    return isEdited;
        //}
        //private void ChangeIsEditedOrNot(ChangeEventArgs e)
        //{

        //    isEdited = true;
        //}
        //public async Task Update()
        //{
        //    StoreDocument.ModifiedTime = DateTime.Now;
        //    StoreDocument.ServerModifiedTime = DateTime.Now;
        //    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
        //     $"{_appConfigs.ApiBaseUrl}StoreDocument/UpdateStoreDocumentDetails",
        //     HttpMethod.Put, StoreDocument);

        //    if (apiResponse.StatusCode == 200)
        //    {
        //        isEdited = false;
        //        await _alertService.ShowSuccessAlert("Success", $"Updated Successfully");
        //        StoreDocument = new Winit.Modules.StoreDocument.Model.Classes.StoreDocument() { UID = Guid.NewGuid().ToString(), IsNewDoc = true };
        //    }
        //    else
        //    {
        //        await _alertService.ShowErrorAlert("Error", $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}");

        //    }
        //}
        protected async Task SaveOrUpdate()
        {
            _loadingService.ShowLoading();
            if (modifiedFileSysList != null && modifiedFileSysList.Any())
            {
                Winit.Shared.Models.Common.ApiResponse<string> apiResponse1 = await fileUploader.MoveFiles();
                if (apiResponse1.IsSuccess)
                {
                    bool isSaved = await SaveFileSysDataFromAPIAsync();
                    if (isSaved)
                    {
                        DisplayFileSysList = new();
                    }
                }
            }
            HttpMethod httpMethod = HttpMethod.Put;
            string endPointMethodName = "UpdateStoreDocumentDetails";
            if (StoreDocument.IsNewDoc)
            {
                endPointMethodName = "CreateStoreDocumentDetails";
                StoreDocument.CreatedTime = DateTime.Now;
                StoreDocument.StoreUID = StoreUID;
                StoreDocument.CreatedBy = _iAppUser.Emp.UID;
                httpMethod = HttpMethod.Post;
                //await save();
            }
            StoreDocument.ModifiedBy = _iAppUser.Emp.UID;
            StoreDocument.ModifiedTime = DateTime.Now;

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
           $"{_appConfigs.ApiBaseUrl}StoreDocument/{endPointMethodName}",
            httpMethod, StoreDocument);

            if (apiResponse.StatusCode == 200)
            {
                isEdited = false;
                await _alertService.ShowSuccessAlert("Success", StoreDocument.IsNewDoc ? "Saved Successfully" : "Updated Successfully");
                if (StoreDocument.IsNewDoc)
                {
                    StoreDocument.DocumentLabel = DocumentLabel;
                    StoreDocuments.Add(StoreDocument);
                }
                StoreDocument = new Winit.Modules.StoreDocument.Model.Classes.StoreDocument() { UID = Guid.NewGuid().ToString(), IsNewDoc = true, DocumentLabel = DocumentLabel };

            }
            else
            {
                await _alertService.ShowErrorAlert("Success", $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}");

            }
            StateHasChanged();
            _loadingService.HideLoading();
        }
        private async Task<bool> SaveFileSysDataFromAPIAsync()
        {
            try
            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}FileSys/CreateFileSysForBulk", HttpMethod.Post, modifiedFileSysList);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    if (FileSysList != null)
                    {
                        FileSysList.AddRange(modifiedFileSysList);
                    }
                    modifiedFileSysList = null;
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);

                    // Assuming CommonUIDResponse is a class with a property 'UID'
                    //List<CommonUIDResponse> commonUIDResponses = JsonConvert.DeserializeObject<List<CommonUIDResponse>>(data);
                    return apiResponse.IsSuccess;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false; // Or handle the case when no response is received
        }
        //protected async Task save()
        //{
        //    StoreDocument.StoreUID = StoreUID;
        //    StoreDocument.CreatedBy = _iAppUser.Emp.CreatedBy;
        //    StoreDocument.ModifiedBy = _iAppUser.Emp.CreatedBy;
        //    StoreDocument.CreatedTime = DateTime.Now;
        //    StoreDocument.ModifiedTime = DateTime.Now;
        //    StoreDocument.ServerAddTime = DateTime.Now;
        //    StoreDocument.ServerModifiedTime = DateTime.Now;

        //    ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
        //   $"{_appConfigs.ApiBaseUrl}StoreDocument/CreateStoreDocumentDetails",
        //    HttpMethod.Post, StoreDocument);

        //    if (apiResponse.StatusCode == 200)
        //    {
        //        isEdited = false;
        //        await _alertService.ShowSuccessAlert("Success", "Saved Successfully");
        //        StoreDocuments.Add(StoreDocument);
        //        StoreDocument = new Winit.Modules.StoreDocument.Model.Classes.StoreDocument() { UID = Guid.NewGuid().ToString(), IsNewDoc = true };
        //    }
        //    else
        //    {
        //        await _alertService.ShowErrorAlert("Success", $"[{apiResponse.StatusCode}]{apiResponse.ErrorMessage}");
        //    }
        //}
        public async Task GetStoreDocuments()
        {
            PagingRequest paging = new PagingRequest();
            paging.FilterCriterias = new List<FilterCriteria>()
            {
                new FilterCriteria("StoreUID", StoreUID, FilterType.Equal)
            };
            paging.IsCountRequired = true;

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}StoreDocument/SelectAllStoreDocumentDetails",
              HttpMethod.Post, paging);
            if (apiResponse.Data != null)
            {
                var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                PagedResponse<Winit.Modules.StoreDocument.Model.Classes.StoreDocument>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.StoreDocument.Model.Classes.StoreDocument>>(data);
                if (pagedResponse != null)
                {
                    try
                    {
                        if (pagedResponse.TotalCount > 0)
                        {
                            foreach (IStoreDocument doc in pagedResponse.PagedData)
                            {
                                doc.DocumentLabel = DocumentList?.Find(p => p.UID == doc.DocumentType)?.Label;
                                StoreDocuments.Add(doc);
                            }
                            //StoreDocuments = pagedResponse.PagedData.ToList<IStoreDocument>();
                            await GetFileSys();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }

        }
        public async Task GetFileSys()
        {
            PagingRequest paging = new PagingRequest();
            paging.FilterCriterias = new List<FilterCriteria>()
            {
                new FilterCriteria("LinkedItemUID",StoreDocuments.Select(p=>p.UID).ToArray(), FilterType.In)
            };
            paging.IsCountRequired = true;

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
              $"{_appConfigs.ApiBaseUrl}FileSys/SelectAllFileSysDetails",
              HttpMethod.Post, paging);
            if (apiResponse.Data != null)
            {
                var data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                PagedResponse<FileSys>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<FileSys>>(data);
                if (pagedResponse != null)
                {
                    try
                    {
                        if (pagedResponse.TotalCount > 0)
                        {
                            FileSysList = pagedResponse.PagedData.ToList<IFileSys>();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }

        }

        protected async Task GetFilesysList(IList<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys)
        {
            modifiedFileSysList = fileSys;
        }
        protected void AfterDeleteImage(string str)
        {

        }
        protected void OnDropDownSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null)
            {
                ISelectionItem? selectionItem = dropDownEvent.SelectionItems?.FirstOrDefault<ISelectionItem>();
                if (selectionItem != null)
                {
                    StoreDocument.DocumentType = selectionItem.UID;
                    StoreDocument.DocumentLabel = selectionItem.Label;
                }
            }
            else
            {
                StoreDocument.DocumentType = string.Empty;
                StoreDocument.DocumentLabel = "Select Document Type";
            }
            IsDocumentVisible = false;
        }
        private void EditDocs(IStoreDocument model)
        {
            _loadingService.ShowLoading();
            showHideFileUploader = false;
            StoreDocument = model;
            StoreDocument.DocumentLabel = model.DocumentLabel;
            DisplayFileSysList = FileSysList?.Where(p => p.LinkedItemUID == model.UID).ToList();
            showHideFileUploader = true;
            _loadingService.HideLoading();
            StateHasChanged();

        }
    }
}

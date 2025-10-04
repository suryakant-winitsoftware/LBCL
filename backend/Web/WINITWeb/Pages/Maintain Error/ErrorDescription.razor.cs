using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Shared.Models.Common;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Maintain_Error
{
    public partial class ErrorDescription
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

        public string SKUUID { get; set; }
        public string SKUUID2 { get; set; }
        string sku;
        public List<DataGridColumn> DataGridColumns { get; set; }

        private bool OnItemClicked { get; set; }
        public bool IsInitialized { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await SetHeaderName();
            await GenerateDescriptionGridColumns();
            // _addEditMaintainWarehouseViewModel.ParentUID = "FR001";
            SKUUID = _commonFunctions.GetParameterValueFromURL("ErrorUID");
            SKUUID2 = _commonFunctions.GetParameterValueFromURL("ErrorUID2");
            LoadResources(null, _languageService.SelectedCulture);
            if (!string.IsNullOrEmpty(SKUUID))
            {
                sku = SKUUID;
            }
            else if (!string.IsNullOrEmpty(SKUUID2))
            {
                sku = SKUUID2;
            }

            if (!string.IsNullOrEmpty(sku))
            {
                await _ErrorDescriptionViewModel.PopulateErrorDescriptionDetailsByUID(sku);
                StateHasChanged();
            }

            OnItemClicked = true;
            IsInitialized = true;
        }
       
        public async Task GenerateDescriptionGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

                new DataGridColumn { Header = "Language Code", GetValue = s => ((IErrorDetailsLocalization)s)?.LanguageCode ?? "N/A" },
                new DataGridColumn { Header = "Cause", GetValue = s => ((IErrorDetailsLocalization)s)?.Cause ?? "N/A" },
                new DataGridColumn { Header = "Resolution", GetValue = s => ((IErrorDetailsLocalization)s)?.Resolution ?? "N/A" },
                new DataGridColumn { Header = "Short Description", GetValue = s => ((IErrorDetailsLocalization)s)?.ShortDescription ?? "N/A" },
                new DataGridColumn { Header = "Description", GetValue = s => ((IErrorDetailsLocalization)s)?.Description ?? "N/A" },
                new DataGridColumn
                {
                    Header = "Actions",
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Text,
                            Text = "Edit",
                            Action = item => OnEditClick((IErrorDetailsLocalization)item)
                        },

                    }
                }
             };
        }

        private void OnEditClick(IErrorDetailsLocalization item)
        {
            _navigationManager.NavigateTo($"AddEditMaintainErrorDescription?ErrorUID={item.UID}");
        }

        private void AddNewErrorDescription()
        {
            _navigationManager.NavigateTo($"AddEditMaintainErrorDescription?ErrorCode={_ErrorDescriptionViewModel.ErrorDescriptionDetails?.errorDetail?.ErrorCode}", forceLoad: false);
        }

        private async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_error_details"], IsClickable = true, URL = "ViewErrorDetails" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["error_description"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["error_description"];
            await CallbackService.InvokeAsync(_IDataService);
        }
    }
}

using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using System.Resources;
using System.Runtime.InteropServices;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIComponents.Common.Language;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;
using Winit.Modules.User.BL.Interfaces;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WinIt.Pages.View_Reasons
{
    public partial class ViewReasons
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsLoaded { get; set; }
        public bool IsEditPopUp { get; set; }
        public bool IsViewPopUp { get; set; }
        public bool IsBackBtnPopUp { get; set; }
        private bool IsDeleteBtnPopUp { get; set; }
        private bool IsAddPopUp { get; set; }
        public IListItem? SelectedListItem { get; set; }
        public string UID { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        public string validationMessage;
        public string ListItemHeaderUID = "";
        public bool ISEditablelListHeaders { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Reasons",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="View Reasons"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            try
            {
                LoadResources(null, _languageService.SelectedCulture);
                _loadingService.ShowLoading();
                _viewReasonsViewModel.PageSize = 10;
                await Task.Delay(TimeSpan.FromSeconds(2));
                _loadingService.HideLoading();
                //Showing Data above Add Button
                _viewReasonsViewModel.listItem = _serviceProvider.CreateInstance<IListItem>();
                _viewReasonsViewModel.listItem.ListHeaderUID = ListItemHeaderUID;//
                IsLoaded = true;
                GenerateGridColumns();
                await _viewReasonsViewModel.PopulateViewModel();
                //await SetHeaderName();
                if (_viewReasonsViewModel.ListHeaders != null)
                {
                    await _viewReasonsViewModel.PopulateListItemData(_viewReasonsViewModel.ListHeaders[0].UID);
                    ListItemHeaderUID = _viewReasonsViewModel.ListHeaders[0].UID;
                    ISEditablelListHeaders = _viewReasonsViewModel.ListHeaders[0].IsEditable;
                }
                _loadingService.HideLoading();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
        private void ValidateSequenceNumber(ChangeEventArgs e)
        {
            var input = e.Value?.ToString() ?? string.Empty; // Safely handle null input
            validationMessage = string.Empty; // Clear previous messages

            // Check if the input is empty (user cleared the input)
            if (string.IsNullOrWhiteSpace(input))
            {
                _viewReasonsViewModel.listItem.SerialNo =null; // Clear the SerialNo value
                return; // Exit early as no further validation is needed
            }

            // Check if the input matches the allowed format (alphanumeric and up to 11 characters)
            if (Regex.IsMatch(input, @"^[a-zA-Z0-9]{0,11}$"))
            {
                if (int.TryParse(input, out var serialNo) && serialNo > 0)
                {
                    _viewReasonsViewModel.listItem.SerialNo = serialNo; // Valid serial number
                }
                else
                {
                    validationMessage +="Invalid sequence number.";
                }
            }
            else
            {
                validationMessage += "Invalid input format. Please enter up to 11 alphanumeric characters.";
            }
        }


        protected override void OnInitialized()
        {

            base.OnInitialized();
        }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_reasons"], IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["view_reasons"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["code"], GetValue = s => ((IListItem)s)?.Code ?? "N/A",IsSortable = false, SortField = "Code" },
                new DataGridColumn { Header = @Localizer["description"], GetValue = s => ((IListItem)s)?.Name ?? "N/A",IsSortable = false, SortField = "Name" },
                new DataGridColumn { Header =  @Localizer["sequence_no"], GetValue = s => ((IListItem)s)?.SerialNo.ToString() ?? "N/A",IsSortable = false, SortField = "SerialNo"},
                new DataGridColumn { Header =  @Localizer["is_editable"], GetValue = s => ((IListItem)s)?.IsEditable == true ? "Yes" : "No",IsSortable = false, SortField = "IsEditable" },
             new DataGridColumn
            {
                Header = @Localizer["actions"],
                IsButtonColumn = true,

                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType=ButtonTypes.Image,
                       URL="https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                        Action = item => OnEditClick((IListItem)item),
                       IsVisible=_viewReasonsViewModel.listItem?.IsEditable == true|| _viewReasonsViewModel.listItem?.IsEditable != null ?true: false

                    },
                    new ButtonAction
                    {
                        ButtonType=ButtonTypes.Image,
                        URL = "https://qa-fonterra.winitsoftware.com/assets/Images/delete.png",
                        Action = item => OnDeleteClick((IListItem)item),
                        IsVisible=_viewReasonsViewModel.listItem?.IsEditable == true || _viewReasonsViewModel.listItem?.IsEditable != null?true: false
                    }
                }
             }
             };
        }
        private async Task SaveUpdateReason()
        {

            validationMessage = null;
            ValidateSequenceNumber(new ChangeEventArgs { Value = _viewReasonsViewModel.listItem.SerialNo.ToString() });

            // Check if there is a validation message after validating the serial number
            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return; // Exit early if there are validation errors
            }
            if (string.IsNullOrWhiteSpace(_viewReasonsViewModel.listItem.Code) ||
                    string.IsNullOrWhiteSpace(_viewReasonsViewModel.listItem.Name) ||
                    string.IsNullOrWhiteSpace(_viewReasonsViewModel.listItem.SerialNo.ToString()))
            {
                validationMessage = "The following field(s) have invalid value(s):";
                if (string.IsNullOrWhiteSpace(_viewReasonsViewModel.listItem.Code))
                {
                    validationMessage += "Code, ";
                }
                if (string.IsNullOrWhiteSpace(_viewReasonsViewModel.listItem.Name))
                {
                    validationMessage += "Description, ";
                }

                if (string.IsNullOrWhiteSpace(_viewReasonsViewModel.listItem.SerialNo.ToString()) || _viewReasonsViewModel.listItem.SerialNo == 0 || _viewReasonsViewModel.listItem.SerialNo <= 0)
                {
                    validationMessage +="Sequenceno,";
                }
               
                validationMessage = validationMessage.TrimEnd(' ', ',');

            }
            else
            {
                var name = _viewReasonsViewModel.ListHeaders?.FirstOrDefault()?.Name;
                if (IsAddPopUp)
                {
                    bool userExists = await UserExistsAsync(
                      _viewReasonsViewModel.listItem.Code);

                    if (userExists)
                    {
                        validationMessage = "An employee with the same Code already exists.";
                        return; // Exit early if user already exists
                    }
                    await _viewReasonsViewModel.SaveUpdateReasons(_viewReasonsViewModel.listItem, true);
                    await Task.Delay(500);
                    await _viewReasonsViewModel.PopulateListItemData(_viewReasonsViewModel.ListHeaders?.FirstOrDefault().Code);
                    IsEditPopUp = false;
                    IsAddPopUp = false;
                    StateHasChanged();

                    if (name != null)
                    {
                        _tost.Add(@Localizer["reasons"], $"{name} created successfully .", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                }
                else
                {
                    await _viewReasonsViewModel.SaveUpdateReasons(_viewReasonsViewModel.listItem, false);
                    await Task.Delay(500);
                    await _viewReasonsViewModel.PopulateListItemData(_viewReasonsViewModel.ListHeaders?.FirstOrDefault().Code);
                    IsEditPopUp = false;
                    IsAddPopUp = false;
                    if (name != null)
                    {
                        _tost.Add(@Localizer["reasons"], $"{name} updated successfully .", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    }
                }

            }
        }
        private async Task<bool> UserExistsAsync(string code)
        {
            // Implement the logic to check if the user exists in the database
            // This could involve a call to a service or repository that queries the user data
            return await _viewReasonsViewModel.CheckUserExistsAsync(code);
        }
        public async Task AddNewReasons()
        {
            IsAddPopUp = true;
            validationMessage = string.Empty;
            _viewReasonsViewModel.listItem = _serviceProvider.CreateInstance<IListItem>();
            _viewReasonsViewModel.listItem.ListHeaderUID = ListItemHeaderUID;
        }
        public async Task OnEditClick(IListItem listitem)
        {
            UID = listitem.UID;
            if (UID != null)
            {
                IsEditPopUp = true;
                await _viewReasonsViewModel.PopulatetViewReasonsforEditDetailsData(UID);
            }
            StateHasChanged();
        }
        public async Task OnViewClick(IListItem listitem)
        {
            IsViewPopUp = true;
            await OnEditClick(listitem);
        }
        public async Task OnDeleteClick(IListItem listitem)
        {
            SelectedListItem = listitem;
            bool result = await _AlertMessgae.ShowConfirmationReturnType(@Localizer["delete"], "Are you sure you want to delete this item ?", @Localizer["yes"], @Localizer["no"]);
            if (result)
            {
              string s =  await _viewReasonsViewModel.DeleteViewReasonItem(SelectedListItem?.UID);
                if (s.Contains("Failed"))
                {
                    await _AlertMessgae.ShowErrorAlert(@Localizer["failed"], s);
                }
                else
                {
                    await _AlertMessgae.ShowSuccessAlert("Successfull", s);
                    await _viewReasonsViewModel.PopulateListItemData(_viewReasonsViewModel.ListHeaders?.FirstOrDefault().Code);
                    StateHasChanged();
                }
            }
            // IsDeleteBtnPopUp = true;
            StateHasChanged();
        }

        //public async Task OnOkFromDeleteBTnPopUpClick()
        //{
        //    IsDeleteBtnPopUp = false;
        //    string s = await _viewReasonsViewModel.DeleteViewReasonItem(SelectedListItem?.UID);
        //    if (s.Contains("Failed"))
        //    {
        //        await _AlertMessgae.ShowErrorAlert("Failed", s);
        //    }
        //    else
        //    {
        //         await _AlertMessgae.ShowSuccessAlert("Success", s);
        //        //await _viewReasonsViewModel.PopulateListItemData(_viewReasonsViewModel.listItem.ListHeaderUID);

        //        StateHasChanged();
        //    }
        //}

        public async Task ViewEditQuantity(IListHeader item)
        {

        }
        public int pagenumber { get; set; }
        private async Task GetListItemData(IListHeader item)
        {
            _viewReasonsViewModel.PageNumber = 1;
            await _viewReasonsViewModel.PopulateListItemData(item.Code);
            ListItemHeaderUID = item.UID;
            ISEditablelListHeaders = item.IsEditable;
            //GenerateGridColumns();
            _viewReasonsViewModel.ListHeaders.Remove(item);
            // Insert the clicked item at the beginning of the list
            _viewReasonsViewModel.ListHeaders.Insert(0, item);
            _navigationManager.NavigateTo("ViewReasons");
        }
        private async Task OnCancelFromBackBTnPopUpClick()
        {
            IsEditPopUp = false;
           // IsBackBtnPopUp = true;
            StateHasChanged();
            IsAddPopUp = false;
        }
        private async Task OnBackFromUpdateBTnPopUpClick()
        {
            //IsBackBtnPopUp = false;
             //IsEditPopUp = true;
            IsAddPopUp = true;

        }
        private async Task OnOkFromUpdateBTnPopUpClick()
        {
            IsBackBtnPopUp = false;
            IsAddPopUp = false;
            IsEditPopUp = false;
            IsViewPopUp = false;
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _viewReasonsViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }

    }
}

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITMobile.Pages.Base;
using WINITMobile.Data;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Events;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Shared.Models.Constants;
using WINITMobile.Models.TopBar;
using Winit.UIComponents.Common.CustomControls;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Modules.WHStock.BL.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.UIModels.Common;


namespace WINITMobile.Pages.LoadRequestM
{
    public partial class AddEditLoadRequest
    {
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }

        public string uid;
        [Parameter]
        public string RequestLoadType { get; set; }
        private string Heading => RequestLoadType == "Load" ? "Create Load Request" : "View Unload Request";
        public int VanQty = 0;
        private string RequestedDate { get; set; } 
        private DateTime currentDate = DateTime.Now;
        private WinitTextBox wtbSearch;
        private bool CheckAllRows { get; set; } = false;
        int count = 1;
        public string Toplabel = "Add Load Request";
        public bool IsPreviewBtnClicked = false;
        private Winit.UIComponents.Mobile.DialogBox.AddSKU AddSKURef;
        public bool IsSKUDialogBoxOpen = false;
        private decimal totQty;
        protected override void OnInitialized()
        {
            WINITMobile.Data.SideBarService _sidebar = new WINITMobile.Data.SideBarService();
            //  _sidebar.IsBackRestricted = true; //NeedToAsk
            IsPreviewBtnClicked = false;

        }
       
        protected override async Task OnInitializedAsync()
        {
           
            _loadingService.ShowLoading(@Localizer["loading_load_request"]);
            await Task.Run(async () =>
            {

                try
                {                                      
                 await SetAddEditState();
                 RequestedDate  = DateTime.Now.ToString("dd/MM/yyyy");
                }

                catch (Exception ex)
                {

                    Console.WriteLine($"Error loading data: {ex.Message}");
                }
                
                InvokeAsync(() =>
                {
                     SetTopBar();
                    _loadingService.HideLoading();
                   
                    StateHasChanged(); // Ensure UI reflects changes
                });
                
            });

            LoadResources(null, _languageService.SelectedCulture);

        }
        private void AfterBarcodeScanned(string scannedText)
        {
            wtbSearch.UpdateValue(scannedText);
        }
        public async Task SetAddEditState()
        {
            // later it will move to baseview model (populate methode or initial methode)
            if (_AddEditLoadRequest is Winit.Modules.WHStock.BL.Classes.AddEditLoadRequestBaseViewModel viewmodel)
            {   
                if (_appUser?.SelectedRoute != null)
                {
                    viewmodel.SelectedRoute = _appUser.SelectedRoute;
                }
            }
            await FetchFromQuerySting();
            if (uid == null)
            {
                await ResetVariables();
            }
            SetVariables();
        }
        public void Dispose()
        {
        }
        public async Task ResetVariables()
        {
            if (_AddEditLoadRequest.SkuList == null)
            {
                await _AddEditLoadRequest.GetSKUMasterData();
            }
            _AddEditLoadRequest.SelectedRouteCode = _appUser.SelectedRoute.Code;
            _AddEditLoadRequest.SelectedRouteCodeDate = "";
            //_AddEditLoadRequest.SelectedRouteUID = "RouteUID1";
            _AddEditLoadRequest.SelectedRouteUID = _appUser.SelectedRoute.UID;
            _AddEditLoadRequest.WHStockRequestItemview = null;
            _AddEditLoadRequest.DisplayWHStockRequestLineItemview = null;
            _AddEditLoadRequest.OrgUID = _appUser.SelectedJobPosition.OrgUID;
            IsSKUDialogBoxOpen = true;
        }
        public async Task FetchFromQuerySting()
        {
            var uri = new Uri(NavManager.Uri);
            var query = uri.Query;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                uid = queryParams.Get("UID");
                _AddEditLoadRequest.RequestType = queryParams.Get("RequestType");

                if (uid != null)
                {
                    await _AddEditLoadRequest.PopulateViewModel(uid);
                }
                Toplabel = _AddEditLoadRequest.WHStockRequestItemview?.Status == StockRequestStatus.Processed ? "Collect Load Request" : "View Load Requested";
                RequestedDate = _AddEditLoadRequest.WHStockRequestItemview?.RequiredByDate.ToString("dd/MM/yyyy");
                if (_AddEditLoadRequest.WHStockRequestItemview?.Status == StockRequestStatus.Processed)
                { IsPreviewBtnClicked = true; }
            }


        }
        public void OnRequiredByDateChange(CalenderWrappedData calenderWrappedData)
        {
            RequestedDate = calenderWrappedData.SelectedValue;
            _AddEditLoadRequest.RequiredByDate = CommonFunctions.GetDate(RequestedDate);
        }
        protected async Task OpenDialogueSku()
        {
            IsSKUDialogBoxOpen = !IsSKUDialogBoxOpen;
            await Task.CompletedTask;
        }
        public void  SetVariables()
        {
            if (_AddEditLoadRequest.WHStockRequestItemview?.Status == StockRequestStatus.Draft || uid == null)
            {
                if (_AddEditLoadRequest.WHStockRequestItemview?.RequestType == RequestType.Load.ToString() || _AddEditLoadRequest.RequestType == RequestType.Load.ToString())
                {
                    Toplabel = @Localizer["add_load_request"];
                }
                else
                { Toplabel = @Localizer["add_unload_request"]; }

                CalculateTotQty();
            }
        }
        public void CalculateTotQty()
        {
            totQty = 0;
            if (_AddEditLoadRequest.DisplayWHStockRequestLineItemview != null)
            {
                foreach (var item in _AddEditLoadRequest.DisplayWHStockRequestLineItemview)
                {
                    totQty = totQty + (item.RequestedQty1 * 1) + (item.RequestedQty2 * item.UOM2CNF);
                }
            }
        }
        async Task SetTopBar()
        {
            MainButtons buttons = new MainButtons()
            {


                //TopLabel = Toplabel,
                //BottomLabel = $"",
                //UIButton1 = new Models.TopBar.Buttons
                //{
                //    Action = PreviewBtnClicked,
                //    ButtonText = "PREVIEW",
                //    ButtonType = Models.TopBar.ButtonType.Text,
                //    IsVisible = (_AddEditLoadRequest.WHStockRequestItemview?.Status == StockRequestStatus.Draft || uid == null) && IsPreviewBtnClicked == false ? true : false

                //},
                

                UIButton2 = new Models.TopBar.Buttons
                {
                    Action = _AddEditLoadRequest.WHStockRequestItemview?.Status == StockRequestStatus.Processed ? UpdateLoadRequestAsCollect : UpdateLoadRequestAsDraft,
                    ButtonText = _AddEditLoadRequest.WHStockRequestItemview?.Status == StockRequestStatus.Processed ? "COLLECT" : "DRAFT",
                    ButtonType = Models.TopBar.ButtonType.Text,
                    IsVisible = (_AddEditLoadRequest.WHStockRequestItemview?.Status == StockRequestStatus.Processed || _AddEditLoadRequest.WHStockRequestItemview?.Status == StockRequestStatus.Draft || uid == null) && IsPreviewBtnClicked == true ? true : false,

                },
                //UIButton3 = new Models.TopBar.Buttons
                //{
                //    Action = IsPreviewBtnClicked == true ? UpdateLoadRequestAsConfirm: AddNewRequest,
                //    ButtonText =IsPreviewBtnClicked == true?"CONFIRM":"",
                //    ButtonType = Models.TopBar.ButtonType.Text,
                //    IsVisible = _AddEditLoadRequest.WHStockRequestItemview?.Status == StockRequestStatus.Draft || uid == null  ? true : false

                //}

            };
            await Btnname.InvokeAsync(buttons); 
        }
        public void UpdateLoadRequestAsCollect()
        {
            _AddEditLoadRequest.WHStockRequestItemview.Status = StockRequestStatus.Collected;
            OnClickUpdateData(StockRequestStatus.Collected);
        }
        public void UpdateLoadRequestAsConfirm()
        {
            OnClickUpdateData(StockRequestStatus.Requested);
        }
        public void UpdateLoadRequestAsDraft()
        {
            OnClickUpdateData(StockRequestStatus.Draft);
        }

        public async void AddNewRequest()
        {
            if (_AddEditLoadRequest.SkuList == null)
            {
                await _AddEditLoadRequest.GetSKUMasterData();
            }
            AddSKURef?.OpenAddProductDialog();
            IsSKUDialogBoxOpen = true;
        }
        public async void OnClickUpdateData(string btnText)
        {
            if(_AddEditLoadRequest.DisplayWHStockRequestLineItemview == null || _AddEditLoadRequest.DisplayWHStockRequestLineItemview.Count <= 0)
            {

            }
            if(uid == null)
            {
                string formattedDate = DateTime.Now.ToString("MMddyyyyHHmmss");
                _AddEditLoadRequest.SelectedRouteCodeDate = $"{_AddEditLoadRequest.SelectedRouteCode}-{formattedDate}";
                if ( _AddEditLoadRequest.RouteListByOrgUID == null)
                //{ await ((AddEditLoadRequestAppViewModel)_AddEditLoadRequest).GetRouteByOrgUID("WINIT"); }
                { await ((AddEditLoadRequestAppViewModel)_AddEditLoadRequest).GetRouteByOrgUID(_appUser.SelectedJobPosition.OrgUID); }
                
            }
            var selectedTab = _AddEditLoadRequest.ConfirmationTab(btnText);
            //if (await _alertService.ShowConfirmationReturnType("", $"Are you sure you want to {selectedTab} the order?", "Yes", "No"))
            if (await _alertService.ShowConfirmationReturnType(
    "",
    $"{@Localizer["are_you_sure_you_want_to"]}  {selectedTab} {@Localizer["the_order?"]} ",
    @Localizer["yes"],
    @Localizer["no"]))
            {
                if (await _AddEditLoadRequest.CUDWHStock(btnText))
                {
                    string textMessage = _AddEditLoadRequest.SuccessTab(_AddEditLoadRequest.WHStockRequestItemview.Status);
                    IsPreviewBtnClicked = false;
                    //await _alertService.ShowSuccessAlert("Success", $"Load request {textMessage} successfully");
                    await _alertService.ShowSuccessAlert(@Localizer["success"], $"{@Localizer["load_request"]} {textMessage} {@Localizer["successfully"]}");
                    NavManager.NavigateTo("/ViewRequest/Load");
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["failed"], @Localizer["load_request_submitted_failed"]);
                }

            }


        }

        public async void GetSelectedSKUs(List<ISelectionItem> selectionSKUs)
        {
            try
            {

                int maxLineNumber = _AddEditLoadRequest.DisplayWHStockRequestLineItemview != null ? _AddEditLoadRequest.DisplayWHStockRequestLineItemview.Max(item => item.LineNumber): 0;
                foreach (var selectionItem in selectionSKUs)
                {
                    if (_AddEditLoadRequest.DisplayWHStockRequestLineItemview == null)
                    {
                        _AddEditLoadRequest.DisplayWHStockRequestLineItemview = new List<IWHStockRequestLineItemViewUI>();
                    }
                    if (_AddEditLoadRequest.DisplayWHStockRequestLineItemview.Any(item => item.SKUCode == selectionItem.Code))
                    {
                        // await _alertService.ShowErrorAlert("Quantity Alert", $"{selectionItem.Label} already added");
                        await _alertService.ShowErrorAlert(@Localizer["quantity_alert"],$"{selectionItem.Label} {@Localizer["already_added"]}");
                    }
                    else
                    {
                        var matchingSKU = _AddEditLoadRequest.SkuList.First(item => item.UID == selectionItem.UID);
                        var matchingSKUUOM = _AddEditLoadRequest.SkuUOMList.First(item => item.SKUUID == selectionItem.UID);


                        var selectedWHStockRequestLineItemViewUI = new WHStockRequestLineItemViewUI
                        {
                            SKUName = selectionItem.Label,
                            SKUCode = selectionItem.Code,
                            SKUUID = selectionItem.UID,
                            UOM1 = matchingSKU?.BaseUOM,
                            UOM2 = matchingSKU?.OuterUOM,
                            UOM = matchingSKU?.BaseUOM,
                            UOM1CNF = 1,
                            UOM2CNF = matchingSKUUOM?.Multiplier ?? 3,
                            RequestedQty1 = 0,
                            RequestedQty2 = 0,
                            RequestedQty = 0,
                            LineNumber = maxLineNumber+1
                        };
                        _AddEditLoadRequest.DisplayWHStockRequestLineItemview.Add((IWHStockRequestLineItemViewUI)selectedWHStockRequestLineItemViewUI);
                    }

                }
            }
            catch (Exception ex)
            {
              
            }
        }
        
        private void ToggleAllRows(ChangeEventArgs e)
        {
            CheckAllRows = !CheckAllRows;

            //foreach (var item in _AddEditLoadRequest.DisplayLoadRequestItemView.WHStockRequestLines)
            //{
            //    item.IsSelected = CheckAllRows;
            //    if (item.IsSelected)
            //    {

            //        // Add items to GetSKUListForAdd if they are checked
            //        //if (!GetSKUListForAdd.Contains(item))
            //        //{
            //        //    GetSKUListForAdd.Add(item);
            //        //}
            //    }
            //    else
            //    {
            //        // Remove items from GetSKUListForAdd if they are unchecked
            //        // GetSKUListForAdd.Remove(item);
            //    }

            //}
        }

        private void ToggleRow(WHStockRequestLineItemViewUI row)
        {
            // Update the row's IsSelected property when the row checkbox is clicked
            row.IsSelected = !row.IsSelected;
            if (row.IsSelected)
            {
               
            }
            else
            {
                // Remove items from GetSKUListForAdd if they are unchecked
                // GetSKUListForAdd.Remove(row);
            }
        }
     
        public async void PreviewBtnClicked()
        {
            if (_AddEditLoadRequest.DisplayWHStockRequestLineItemview != null)
            {
                var IsAnyOuterZeroCase = _AddEditLoadRequest.DisplayWHStockRequestLineItemview.Any(item => (item.RequestedQty2 * item.RequestedQty1)  == 0);
                if(IsAnyOuterZeroCase)
                    {
                    await _alertService.ShowErrorAlert(@Localizer["alert"],$"{@Localizer["requestedqrt2_should_not_be_zero"]}");

                    }
                else
                    {
                        IsPreviewBtnClicked = true;
                        await SetTopBar();
                }
                
            }
        } 
        public void CollectedQtyChange(string uid)
        {
            count = 0;

        }
        public void BackToViewPage()
        {
            NavManager.NavigateTo("viewloadrequest/Load");

        }
      

        public async Task ItemSearch(string searchValue)
        {
            await _AddEditLoadRequest.ApplySearch(searchValue);

        }
    }
}
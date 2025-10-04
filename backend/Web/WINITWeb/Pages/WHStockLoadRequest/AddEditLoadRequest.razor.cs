
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Microsoft.AspNetCore.Components;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Shared.Models.Common;
using WinIt.Pages.Base;
using Winit.Modules.WHStock.Model.Interfaces;
using NPOI.SS.Formula.Functions;
using Nest;
using Winit.UIComponents.Common;
using Winit.Shared.CommonUtilities.Common;
using System.Data;
using Winit.Shared.Models.Events;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;


namespace WinIt.Pages.WHStockLoadRequest
{
    public partial class AddEditLoadRequest
    {


        [CascadingParameter]
        public EventCallback<IDataService> CallbackService { get; set; }
        public string formattedDate;
        public string uid;
       
        int count = 1;
        public string ModifiedTime;
        public string AlertMessage;
        private Winit.UIComponents.Web.DialogBox.AddSKU AddSKURef;
        public bool IsAddSKUDialogOpen = false;
        private bool selectedOption = true;
        public string SelectedRouteCode = "";
       
        public string SelectedRouteName = "";
        private string selectedDate = DateTime.Now.ToString("dd/MM/yyyy");
        protected override void OnInitialized()
        {
            InitializeVariables();
            base.OnInitialized();
        }
        public void InitializeVariables()
        {
            _AddEditLoadRequest.RouteListForSelection?.ForEach(item => item.IsSelected = false);
            //if (uid == null)
            //{
            //    
            //}
        }
        public async Task SetHeaderName()
        {
            //_IDataService.BreadcrumList = new();

            //_IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = "View Load Request", IsClickable = true, URL = "viewloadrequest" });
            //_IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = " Add Load Request ", IsClickable = false });
            //_IDataService.HeaderText = "Add Load Request";
            //await CallbackService.InvokeAsync(_IDataService);
        }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Add Load Request",
            BreadcrumList = new List<IBreadCrum>() 
            {
                new BreadCrumModel(){SlNo = 1, Text = "View Load Request", IsClickable = true, URL = "viewloadrequest" },
                new BreadCrumModel() {SlNo = 1, Text = "Add Load Request ",IsClickable = false }
            }
        };
        protected override async Task OnInitializedAsync()
        {
           
            _loadingService.ShowLoading("Loading Load Request");
            LoadResources(null, _languageService.SelectedCulture);
            await Task.Run(async () =>
            {
                try
                {
                    await SetAddEditState();                   
                    InvokeAsync(() =>
                    {
                        _loadingService.HideLoading();


                        StateHasChanged(); // Ensure UI reflects changes
                    });




                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });
        }
       
        public async Task SetAddEditState()
        {

            await FetchFromQuerySting();
            await SetVariables();
            await SetHeaderName();
        }
        public async Task FetchFromQuerySting()
        {
            var uri = new Uri(NavManager.Uri);
            var query = uri.Query;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                if (queryParams != null)
                {
                    uid = queryParams.Get("UID");
                    _AddEditLoadRequest.RequestType = queryParams.Get("RequestType");

                }
                //await _AddEditLoadRequest.PopulateViewModel(uid);
               
            }
        }
        public async Task SetVariables()
        {
            if (uid == null)
            {
                _AddEditLoadRequest.Stocktype = StockType.Salable.ToString();
                if (_AddEditLoadRequest.SkuAttributesList == null)
                { await _AddEditLoadRequest.GetSKUMasterData(); }
                _AddEditLoadRequest.WHStockRequestItemview = null;
                _AddEditLoadRequest.DisplayWHStockRequestLineItemview = null;
                _AddEditLoadRequest.Remark = null;
                _AddEditLoadRequest.SelectedRouteUID = null;
            }
            else if(uid != null)
            {
                await _AddEditLoadRequest.PopulateViewModel(uid);
            }
        }
       

        public void BackToViewPage()
        {
            
            NavManager.NavigateTo("viewloadrequest");
        }
        public async void OnClickUpdateData(string btnText)
        {
            _loadingService.ShowLoading("Loading Load Request");
            await Task.Run(async () =>
            {
                try
                {
                    InvokeAsync(() =>
                    {
                        _loadingService.HideLoading();
                    });

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });





            if (_AddEditLoadRequest.DisplayWHStockRequestLineItemview == null || _AddEditLoadRequest.DisplayWHStockRequestLineItemview.Count <= 0)
            {
                _tost.Add("Alert", $"Please add atleast one product", Winit.UIComponents.SnackBar.Enum.Severity.Error);

            }

            else
            {
                bool foundInvalidCase = false;
                string skuCode = "";
                if (btnText == StockRequestStatus.Draft)
                {
                    bool IsAnyQtyGreaterThanZero = _AddEditLoadRequest.DisplayWHStockRequestLineItemview.Any(lodreq => lodreq.RequestedQty > 0);
                    foundInvalidCase = !IsAnyQtyGreaterThanZero;
                }
                else
                {

                    foreach (var lodreq in _AddEditLoadRequest.DisplayWHStockRequestLineItemview)
                    {
                        if (lodreq.RequestedQty == 0)
                        {
                            foundInvalidCase = true;
                            skuCode = lodreq.SKUCode;
                            break;

                        }
                    }
                }
                if (!foundInvalidCase)
                {

                    var selectedTab = _AddEditLoadRequest.ConfirmationTab(btnText);
                    if (await PrepareAlertMessage(selectedTab))
                    {
                        _loadingService.ShowLoading("Loading Load Request");
                        await Task.Run(async () =>
                        {
                            if (uid == null)
                            {
                                if (selectedDate != null)
                                {
                                    DateTime parsedDate = DateTime.ParseExact(selectedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    _AddEditLoadRequest.RequiredByDate = parsedDate;
                                    string formattedDate = parsedDate.ToString("yyyyMMdd") + DateTime.Now.ToString("HHmmss");
                                    _AddEditLoadRequest.SelectedRouteCodeDate = $"{SelectedRouteCode}-{formattedDate}";
                                }
                            }
                            if (await _AddEditLoadRequest.CUDWHStock(btnText))
                            {

                                NavManager.NavigateTo("viewloadrequest");
                                var selectedtab = _AddEditLoadRequest.SuccessTab(_AddEditLoadRequest.WHStockRequestItemview.Status);
                                _tost.Add("SKUs", $"Load Request {selectedtab} successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                            }
                            else
                            {
                                _tost.Add("SKUs", $"Status changed to {selectedTab} failed", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                            }
                            InvokeAsync(() =>
                            {
                                _loadingService.HideLoading();
                            });

                        });

                    }
                }
                else
                {
                    if (btnText == StockRequestStatus.Draft)
                    {
                        _tost.Add("Alert", "Atleast one SKU Quantity should be greater than zero", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    }
                    else
                    {
                        _tost.Add("Alert", $"{skuCode} Quantity should not be zero", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    }
                }
            
        }
                
        }

        public void CheckIsAllItemwithZeroQty()
        {

        }
        public void GetSelectedStockType(ChangeEventArgs stockType)
        {
            // Cast stockType to ChangeEventArgs and access the Value property to get the selected value
            _AddEditLoadRequest.Stocktype = stockType.Value.ToString();
        }
        public void GetSelectedRoute(DropDownEvent dropDown)
        {
            if(dropDown != null) {

                // Find the first matching route
                try {
                    var selectedRoute = _AddEditLoadRequest.RouteList
                    .FirstOrDefault(e => dropDown.SelectionItems.Any(r => r.UID == e.UID));

                    // Explicitly cast to IRoute if a match is found
                    if (selectedRoute != null)
                    {
                        _AddEditLoadRequest.SelectedRoute = (Winit.Modules.Route.Model.Interfaces.IRoute)selectedRoute;
                    }
                    else
                    {
                        _AddEditLoadRequest.SelectedRoute = null; // Handle the case where no match is found
                    }
                } catch(Exception EX)
                {
                
                }
                
            }
            _AddEditLoadRequest.SelectedRouteUID = dropDown.SelectionItems[0].UID;
            SelectedRouteName = dropDown.SelectionItems[0].Label;
            SelectedRouteCode = dropDown.SelectionItems[0].Code;

        }
        public async void OpenAddProductDialog()
        {

            if (_AddEditLoadRequest.SkuAttributesList == null)
            { await _AddEditLoadRequest.GetSKUMasterData(); }
            IsAddSKUDialogOpen = true;
            AddSKURef?.OpenAddProductDialog();
        }

        public void GetSelectedSKUs(List<ISelectionItem> selectionSKUs)
        {
            try
            {
                if (_AddEditLoadRequest.DisplayWHStockRequestLineItemview == null)
                {
                    _AddEditLoadRequest.DisplayWHStockRequestLineItemview = new List<IWHStockRequestLineItemViewUI>();
                                        
                }
                int maxLineNumber = _AddEditLoadRequest.DisplayWHStockRequestLineItemview != null && _AddEditLoadRequest.DisplayWHStockRequestLineItemview.Count >0 ? _AddEditLoadRequest.DisplayWHStockRequestLineItemview.Max(item => item.LineNumber) : 0;

                foreach (var selectionItem in selectionSKUs)
                {
                    if (_AddEditLoadRequest.DisplayWHStockRequestLineItemview?.Any(item => item.SKUCode == selectionItem.Code) == true)
                    {
                        _tost.Add("Route", $"Item {selectionItem.Label} already added.", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                    }
                    else
                    {
                        var matchingSKU = _AddEditLoadRequest.SkuList.First(item => item.UID == selectionItem.UID);
                        var matchingSKUUOM = _AddEditLoadRequest.SkuUOMList.FirstOrDefault(item => item.SKUUID == selectionItem.UID);


                        var selectedWHStockRequestLineItemViewUI = new WHStockRequestLineItemViewUI
                        {
                            SKUUID = selectionItem.UID,
                            SKUCode = selectionItem.Code,
                            SKUName = selectionItem.Label,
                            UOM1 = matchingSKU?.BaseUOM,
                            UOM2 = matchingSKU?.OuterUOM,
                            UOM = matchingSKU?.BaseUOM,
                            UOM2CNF = matchingSKUUOM?.Multiplier ?? 3,
                            UOM1CNF = 1,
                            LineNumber= maxLineNumber+1,
                        };
                        _AddEditLoadRequest.DisplayWHStockRequestLineItemview.Add(selectedWHStockRequestLineItemViewUI);
                        maxLineNumber = maxLineNumber + 1;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }
        }
        
        public async Task<bool> PrepareAlertMessage(string selectedTab)
        {
            bool result = await _alertService.ShowConfirmationReturnType("", $"Are you sure you want to {selectedTab}? ", "Yes", "No");
            return result;
        }        
        public void QTYChange()
        {
            count = 0;

        }
        public void OnRequiredByDateChange(CalenderWrappedData calenderWrappedData)
        {
                selectedDate = calenderWrappedData.SelectedValue;
                _AddEditLoadRequest.RequiredByDate = CommonFunctions.GetDate(selectedDate);
        }
    }
}

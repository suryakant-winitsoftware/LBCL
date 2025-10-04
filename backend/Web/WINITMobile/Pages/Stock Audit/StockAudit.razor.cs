using iTextSharp.text.html.simpleparser;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.UIComponents.Common.Common;
using Winit.Shared.Models.Events;
using Winit.Modules.StockAudit.Model.Interfaces;
using WINITMobile.Models.TopBar;
using Winit.Modules.StockAudit.Model.Classes;
using Microsoft.AspNetCore.Components;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Modules.StockAudit.BL.Interfaces;

namespace WINITMobile.Pages.Stock_Audit
{
    public partial class StockAudit
    {
        private bool IsPreviewClicked = false;
        private bool IsSignatureView = false;

        private bool IsOrderPlacedPopupVisible = false;
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
        SelectionManager SelectedTab;
        public List<ISelectionItem> TabSelectionItems = new List<ISelectionItem>
         {
              new SelectionItem{ Label="Saleable", Code=StockAuditConst.Saleable, UID="1"},
              new SelectionItem{ Label="FOC", Code=StockAuditConst.FOC, UID="2"},
         };
        public string SelectedUOM = StockAuditConst.OU;
        

        protected override void OnInitialized()
        {
            SetVariales();
            base.OnInitialized();
            LoadResources(null, _languageService.SelectedCulture);
        }

        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading(@Localizer["stock_audit_loading"]);
            await Task.Run(async () =>
            {
                try
                {
                    await SetAddEditStockAudit();
                }

                catch (Exception ex)
                {

                    Console.WriteLine($"{@Localizer["error_loading_data"]}: {ex.Message}");
                }

                await InvokeAsync(() =>
                {
                    SetTopBar();
                    _loadingService.HideLoading();
                });

            });
        }
     
        public async Task SetAddEditStockAudit()
        {
            await _AddEditStockAudit.PopulateViewModel("WINIT");
            await PrepareSKUMaster();
            await SetTopBar();
        }

        public async Task PrepareSKUMaster()
        {
            
                 await _AddEditStockAudit.GetSKUMasterData();
 
        }

        public void SetVariales()   
        {
            _AddEditStockAudit.ActiveTab = StockAuditConst.Saleable;
            SelectedTab = new SelectionManager(TabSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
            TabSelectionItems[0].IsSelected = _AddEditStockAudit.ActiveTab == StockAuditConst.Saleable;
            _AddEditStockAudit.SelectedRouteUID = "RouteUID1";
            _AddEditStockAudit.PageLoadTime = DateTime.Now;
        }

        public async void OnTabSelect(ISelectionItem selectionItem)
        {
            if (!selectionItem.IsSelected)
            {
                SelectedTab.Select(selectionItem);
                _AddEditStockAudit.ActiveTab = selectionItem.Code;

            }
        }
 
        async Task SetTopBar()
        {
            MainButtons buttons = new MainButtons()
            {


                TopLabel = @Localizer["stock_audit"],
                // BottomLabel = $"Raman/need to ask",
                UIButton1 = new Models.TopBar.Buttons
                {
                    Action =SaveUpdateData,
                    ButtonText = IsPreviewClicked==false? @Localizer["preview"]:@Localizer["confirm"],
                    ButtonType = Models.TopBar.ButtonType.Text,
                    IsVisible = true

                },
            };
            await Btnname.InvokeAsync(buttons);
           
        }
        public async void SaveUpdateData()
        {
            if(IsPreviewClicked)
            {
              if(await _alertService.ShowConfirmationReturnType(@Localizer["confirm"], @Localizer["are_you_sure_you_want_to_submit?"]))
                {
                    IsSignatureView = true;
                    await AddStock();
                }
            }
            else
            {
                await PreviewClicked();
            }
        }
      public async Task PreviewClicked()
        {
            if(_AddEditStockAudit.ActiveTab==StockAuditConst.Saleable)
            {

                if(await CheckIsZeroSelection(_AddEditStockAudit.DisplaySaleableStockAudit))
                {
                    await FetchNonNullQtyItems();
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["enter_qty_atleast_one_product"]);
                }
            }
            else
            {
                if(await CheckIsZeroSelection(_AddEditStockAudit.DisplayFocStockAudit))
                {
                    await FetchNonNullQtyItems();
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["slert"], @Localizer["enter_qty_atleast_one_product"]);
                }
            }


        }

        public async Task<bool> CheckIsZeroSelection(List<IStockAuditItemView> StockAuditItemView)
        {
            if (StockAuditItemView.Any(item => item.UOMQty != null))
            {
                return true;
            }
            else
            {
                return false;
                //await _alertService.ShowErrorAlert("Alert", "Enter Qty Atleast one product");
            }
        }
        public async Task FetchNonNullQtyItems()
        {

            IsPreviewClicked = true;
            await SetTopBar();
            if(_AddEditStockAudit.ActiveTab == StockAuditConst.Saleable)
            {
                _AddEditStockAudit.DisplaySaleableStockAudit = _AddEditStockAudit.DisplaySaleableStockAudit
                   .Where(item => item.UOMQty != null)
                   .ToList();
            }
            else
            {
                _AddEditStockAudit.DisplayFocStockAudit = _AddEditStockAudit.DisplayFocStockAudit
                   .Where(item => item.UOMQty != null)
                   .ToList();
            }
         
        }

        public async Task AddStock()
        {
            if (await _AddEditStockAudit.AddStock())
            {
                await _beatHistoryViewModel.StopBeatHistory(_appUser.SelectedBeatHistory, _AddEditStockAudit.PendingWHStockAuditRequest?.WHStockAuditItemView?.UID);
                await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["stock_audit_saved_successfully"]);
                NavManager.NavigateTo("CloseOfTheDay");
            }
        }
        public async Task ItemSearch(string searchString)
        {
            if(_AddEditStockAudit.ActiveTab == StockAuditConst.Saleable)
            {
                _AddEditStockAudit.DisplaySaleableStockAudit.Clear();
               var saleableStockAuditItemView = await _AddEditStockAudit.ItemSearch(searchString, _AddEditStockAudit.SaleableStockAuditItemView);
                _AddEditStockAudit.DisplaySaleableStockAudit.AddRange(saleableStockAuditItemView);
            }
            else
            {
                _AddEditStockAudit.DisplayFocStockAudit.Clear();
                var focStockAuditItemView = await _AddEditStockAudit.ItemSearch(searchString, _AddEditStockAudit.StockAuditItemViews);
                _AddEditStockAudit.DisplayFocStockAudit.AddRange(focStockAuditItemView);

            }

        }
        private List<Winit.Shared.Models.Common.ISelectionItem> UomSelectionItemsList;
        private List<Winit.Shared.Models.Common.ISelectionItem> UomCloneSelectionItemsList;
        public void GetSelectionItemsForUOM(IStockAuditItemView stockAuditItemView)
        {
            UomSelectionItemsList = _AddEditStockAudit.GetAvailableUOMForDDL(stockAuditItemView);

        }
        

        public void GetCloneUOM_SelectionItems(IStockAuditItemView stockAuditItemView)
        {
            UomCloneSelectionItemsList = _AddEditStockAudit.GetAvailableUOMForClone(stockAuditItemView);
        }
        public async Task DropDownSelection_OnSingleSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if(_AddEditStockAudit.ActiveTab == StockAuditConst.Saleable)
                {
                    IStockAuditItemView stockAuditItemView = _AddEditStockAudit.SaleableStockAuditItemView
                                        .Where(e => e.UID == dropDownEvent.UID)
                                        .FirstOrDefault();
                    await DropDownSelection(stockAuditItemView, dropDownEvent);
                }
                else
                {
                    IStockAuditItemView stockAuditItemView = _AddEditStockAudit.StockAuditItemViews
                                       .Where(e => e.UID == dropDownEvent.UID)
                                       .FirstOrDefault();
                    await DropDownSelection(stockAuditItemView, dropDownEvent);
                }
               
            }
            StateHasChanged();
        }

        public async Task DropDownSelection(IStockAuditItemView stockAuditItemView,DropDownEvent dropDownEvent)
        {
            if (stockAuditItemView != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                Winit.Shared.Models.Common.ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectionItem != null)
                {
                    stockAuditItemView.SelectedUOM = stockAuditItemView.AllowedUOMs.Find(uom => uom.Code == selectionItem.Code);

                    //await _salesOrderViewModel.OnQtyChange(stockAuditItemView);
                }
            }
        }
  
        public async Task DropDownClone_OnSingleSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if(_AddEditStockAudit.ActiveTab==StockAuditConst.Saleable)
                {
                    IStockAuditItemView stockAuditItemView = _AddEditStockAudit.SaleableStockAuditItemView
               .Where(e => e.UID == dropDownEvent.UID)
               .FirstOrDefault();
                    await CloneDropDownSelection(stockAuditItemView, dropDownEvent);
                }
                else
                {
                    IStockAuditItemView stockAuditItemView = _AddEditStockAudit.StockAuditItemViews
               .Where(e => e.UID == dropDownEvent.UID)
               .FirstOrDefault();
                    await CloneDropDownSelection(stockAuditItemView, dropDownEvent);
                }
               
            }



        }
        public async Task CloneDropDownSelection(IStockAuditItemView stockAuditItemView, DropDownEvent dropDownEvent)
        {

            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                Winit.Shared.Models.Common.ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectionItem != null)
                {
                    var newUOM = stockAuditItemView.AllowedUOMs.Find(prod => prod.Code == selectionItem.Code);
                    var clonedcopy = stockAuditItemView.Clone(newUOM, Winit.Shared.Models.Enums.ItemState.Cloned, newUOM.Code);
                    stockAuditItemView.UsedUOMCodes.Add(newUOM.Code);
                    await _AddEditStockAudit.AddClonedItemToList(clonedcopy);
                }
            }
        }
        public async Task DeleteCloned_Product(IStockAuditItemView salesOrderItemView)
        {
            await _AddEditStockAudit.RemoveItemFromList(salesOrderItemView);
            StateHasChanged();
        }

       
    }
}

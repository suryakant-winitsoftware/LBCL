using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Org.BL.Classes;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Printing.BL.Classes.SalesOrder;
using Winit.Modules.Printing.BL.Classes;
using Winit.Modules.Printing.BL.Interfaces;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Printing.BL.Classes.VanStock;
using Winit.Shared.Models.Common;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
namespace WINITMobile.Pages.StoreTransactions.ViewVanStock;

partial class ViewVanStock
{

    public List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> StockItemsShow { get; set; }
    public List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> PrintVanStockItems = new List<IWarehouseStockItemView>();
    public List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> StockItemsStore { get; set; }
    public Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView selectedItem { get; set; }
    public string VehicleNo { get; set; }
    public string EmpNo { get; set; }
    public int TotalSKUs { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalSkuQty { get; set; }
    public int selectedCaseQty;
    public int selectedPieceQty;
    public string CurrentDate { get; set; } = DateTime.Now.ToString("dd/mm/yyyy");
    public ISelectionItem SelectedTab { get; set; }
    public string SelectedStockType { get; set; } = StockType.Salable.ToString();
    private string searchQuery;
    public List<ISelectionItem> TabSelectionItems = new List<ISelectionItem>
         {
              new SelectionItem{ Label="Salable", Code=nameof(StockType.Salable), UID="1", IsSelected=true},
              new SelectionItem{ Label="Non Salable", Code=nameof(StockType.NonSalable), UID="2"},
              new SelectionItem{ Label="Reserved", Code=nameof(StockType.ReservedSalable), UID="3"},

         };
    private async Task OnSearching(string searching)
    {
        if (!string.IsNullOrEmpty(searching))
        {
            searching = searching.ToLower();

            StockItemsShow = StockItemsShow.Where(c =>
                (c.SKUName?.ToLower() ?? "").Contains(searching) ||
                (c.SKUCode?.ToLower() ?? "").Contains(searching) 
            ).ToList();
        }
        else
        {
            StockItemsShow = StockItemsStore;
        }

        StateHasChanged();
        await Task.CompletedTask;
    }
    protected override async void OnInitialized()
    {
        try
        {
            if (_appUser != null)
            {
                EmpNo = _appUser?.Emp?.Code;
                VehicleNo = _appUser.Vehicle?.VehicleNo;
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("An error occurred", ex.Message);
        }

    }
    public async Task OnTabSelect(ISelectionItem selectionItemTab)
    {
        if (StockItemsShow == null)
        {
            // Handle null Customerlist
            return;
        }

        SelectedTab = selectionItemTab;

        foreach (var tabitem in TabSelectionItems)
        {
            tabitem.IsSelected = (tabitem == selectionItemTab);
        }
        Enum.TryParse(selectionItemTab.Code, out StockType enumValue);
        await GetVanStockListByStockType(enumValue);
        CalculateTotals();
        StateHasChanged();
        await Task.CompletedTask;
    }
    protected async Task ShowItemDetails(IWarehouseStockItemView vanStockItemView)
    {
        selectedItem = vanStockItemView;
        CalculateCaseAndPieceQty(vanStockItemView, out selectedCaseQty, out selectedPieceQty);
        StateHasChanged();
        await Task.CompletedTask;
    }
    private void CalculateCaseAndPieceQty(IWarehouseStockItemView item, out int caseQty, out int pieceQty)
    {
        caseQty = (int)(item.TotalEAQty / item.OuterMultiplier);
        pieceQty = (int)(item.TotalEAQty % item.OuterMultiplier);
    }
    private void CalculateTotals()
    {
        if (StockItemsShow != null)
        {
            TotalSKUs = StockItemsShow.Count;
            TotalSkuQty = StockItemsShow.Sum(item => item.TotalEAQty);
            TotalValue = StockItemsShow.Sum(item => item.TotalCost);

        }
        else
        {
            TotalSKUs = 0;
            TotalSkuQty = 0;
            TotalValue = 0;
        }
    }
    private int CalculateCaseQty(decimal totalQty, decimal outerMultiplier)
    {
        return (int)(totalQty / outerMultiplier);
    }

    private int CalculatePieceQty(decimal totalQty, decimal outerMultiplier)
    {
        return (int)(totalQty % outerMultiplier);
    }

    protected override async Task OnInitializedAsync()
    {
        _loadingService.ShowLoading("lOADING Customers..");
                await Task.Run(async () =>
                {
                    try
                    {
                       await GetVanStockListByStockType(StockType.Salable);

                         await InvokeAsync(async () =>
                        {
                            _loadingService.HideLoading();
                            StateHasChanged(); // Ensure UI reflects changes
                        });
                    }
                    catch (Exception ex)
                    {
                        await _alertService.ShowErrorAlert("An error occurred", ex.Message);
                    }
                });
            
        await Task.CompletedTask;

    }
    protected async Task GetVanStockListByStockType(StockType Stocktype)
    {
        StockItemsStore = await ((VanStockAppViewModel)_viewmodel).GetVanStockList(_appUser.Vehicle.UID, _appUser.SelectedJobPosition.OrgUID, Stocktype);
        StockItemsShow = StockItemsStore;
        CalculateTotals();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

        if (firstRender)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }
        await Task.CompletedTask;
    }
    protected void CloseModal()
    {
        selectedItem = null;
        StateHasChanged();
    }
    public async Task PrintVanStock()
    {
        if (true)
        {
            
            IPrint vanStockPrint = new VanStockPrint();
            //string printerTypeString =   _storageHelper.GetStringFromPreferences("PrinterTypeOrBrand");
            //string printerSizeString = _storageHelper.GetStringFromPreferences("PrinterPaperSize");

            // Now convert the strings to the corresponding enum values
            PrinterType printerType = PrinterType.Zebra;       //(PrinterType)Enum.Parse(typeof(PrinterType), printerTypeString);
            PrinterSize printerPaperSize = PrinterSize.FourInch;     //(PrinterSize)Enum.Parse(typeof(PrinterSize), printerSizeString);
            
            string vanstockPrintString = 
                vanStockPrint.CreatePrintString(printerType, printerPaperSize,(StockItemsStore,_appUser.Vehicle,_appUser.Emp));
            //string salesOrderPrintString = salesOrderPrint.CreatePrintString(printerType, printerPaperSize, _salesOrderViewModel.SelectedSalesOrderItemViews);
            Winit.Modules.Printing.BL.Interfaces.IPrinter userPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("PrinterMacAddresses"), printerType, _storageHelper.GetStringFromPreferences("PrinterMacAddresses"));
            if (userPrinter.Type == PrinterType.Zebra)
            {
                await ((ZebraPrinter)userPrinter).Print(vanstockPrintString);
            }
        }
        else
        {
            await _alertService.ShowErrorAlert("No Printer ", "No Printer Selected at Printer Service for Printing.");
        }

    }
}

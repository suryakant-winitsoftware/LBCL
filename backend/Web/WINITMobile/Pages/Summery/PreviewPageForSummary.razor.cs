using Microsoft.AspNetCore.Components;
using Winit.Modules.Printing.BL.Classes;
using Winit.Modules.Printing.BL.Classes.SalesOrder;
using Winit.Modules.Printing.BL.Interfaces;
using Winit.Modules.Printing.Model.Enum;

namespace WINITMobile.Pages.Summery;

public partial class PreviewPageForSummary
{
    [Parameter]
    public string SalesOrderUID { get; set; }

    [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }

    private readonly Winit.Modules.SalesOrder.Model.Classes.SalesOrder salesOrder;
    private Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView salesOrderHeader;
    private List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView> salesOrderLines;

    protected override async Task OnInitializedAsync()
    {
        if (SalesOrderUID != null)
        {
            salesOrderHeader = await GetSalesOrderDetailsFromBL();
            salesOrderLines = await GetSalesOrderlinesFromBL();
            await SetTopBar();
        }
        else
        {

        }
        LoadResources(null, _languageService.SelectedCulture);

    }

    private async Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView> GetSalesOrderDetailsFromBL()
    {
        try
        {
            Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView HeaderData = await _SalesOrderBL.GetSalesOrderPrintView(SalesOrderUID);

            return HeaderData;
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>> GetSalesOrderlinesFromBL()
    {
        try
        {
            IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView> LineData = await _SalesOrderBL.GetSalesOrderLinePrintView(SalesOrderUID);
            return LineData.ToList();
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void GoBack()
    {
        _navigationManager.NavigateTo("Summary_DeliverySummary");
    }

    private async Task SetTopBar()
    {
        WINITMobile.Models.TopBar.MainButtons buttons = new()
        {
            //UIButton1 = new Models.TopBar.Buttons()
            //{
            //    ButtonType = Models.TopBar.ButtonType.Image,
            //    URL = "/Images/print_btn.png",
            //    IsVisible = true,
            //    Action =async () => await PreviewPrintClick()
            //},
            TopLabel = @Localizer["sales_order_preview"],
            BottomLabel = salesOrderHeader.StoreName
        };
        await Btnname.InvokeAsync(buttons);
    }
    public async Task PreviewPrintClick()
    {
        //string printerName = _bluetoothDeviceInfo.BtName;//"XXRBJ17461346";;
        string macAddress = _bluetoothDeviceInfo.macaddress;// "B0:91:22:7F:B9:B2";
        IPrint salesOrderPrint = new SalesOrderPrint();
        string salesOrderPrintString = salesOrderPrint.CreatePrintString(_bluetoothDeviceInfo.PrinterType, _bluetoothDeviceInfo.PrinterSize, (salesOrderHeader, salesOrderLines));
        Winit.Modules.Printing.BL.Interfaces.IPrinter userPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(macAddress, _bluetoothDeviceInfo.PrinterType, macAddress);
        if (userPrinter.Type == PrinterType.Zebra)
        {
            await ((ZebraPrinter)userPrinter).Print(salesOrderPrintString);
        }
    }
}

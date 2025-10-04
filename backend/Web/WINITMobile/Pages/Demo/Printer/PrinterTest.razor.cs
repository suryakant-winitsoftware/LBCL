using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using Winit.Modules.Printing.BL.Classes;
using Winit.Modules.Printing.Model.Enum;
using WINITMobile.Models.TopBar;

namespace WINITMobile.Pages.Demo.Printer
{
    public partial class PrinterTest : ComponentBase
    {
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
        private string inputText = "";
        bool isPrintSetup=false;
        private PrinterType DefPrinterType;
        private PrinterSize SelectedPrinterSize;
        private string macAddress;
        //private PrinterSize DefPrinterSize;
        private bool IsInitialized;
        protected override async Task OnInitializedAsync()
        {
            IsInitialized = true;
            await SetTopBar();

            
            StateHasChanged();
        }
        async Task SetTopBar()
        {
            MainButtons buttons = new MainButtons()
            {
                TopLabel = "Printer",
                BottomLabel = "SetUp"
            };
            await Btnname.InvokeAsync(buttons);
        }

        public async Task FindPrinterAvailability()
        {
            //await _jSRuntime.InvokeVoidAsync("startAppAService");
            isPrintSetup = true;
            await GetConnectedPrinter();
        }


        //public async Task FindPrinterAvailability()
        //{
        //    // Log before invoking the JavaScript function
        //    Console.WriteLine("Invoking startAppAService...");

        //    // Invoke the JavaScript function asynchronously
        //    await _jSRuntime.InvokeVoidAsync("startAppAService");

        //    // Log after invoking the JavaScript function
        //    Console.WriteLine("startAppAService invoked successfully.");

        //    // Update your state or perform other actions
        //    isPrintSetup = true;
        //    // await GetConnectedPrinter();
        //}


        public async Task<(string, string, string)> GetConnectedPrinter()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:8085");
                    HttpResponseMessage response = await client.GetAsync("/GetConnectedPrinterCredentails");
                    if (response.IsSuccessStatusCode)
                    {
                        string textResult = await response.Content.ReadAsStringAsync();
                        string[] parts=textResult.Split(',');
                        string PrinterBrandOrType = parts[0];
                        string PaperSize = parts[1];
                        string MacAddress= parts[2];
                        
                            DefPrinterType = (PrinterType)Enum.Parse(typeof(PrinterType), PrinterBrandOrType);
                            SelectedPrinterSize = (PrinterSize)Enum.Parse(typeof(PrinterSize), PaperSize);
                            macAddress = MacAddress;
                        
                            _btinfo.PrinterType=DefPrinterType;
                            _btinfo.PrinterSize = SelectedPrinterSize;
                            _btinfo.macaddress = MacAddress;

                            _storageHelper.SaveStringToPreference("PrinterTypeOrBrand", PrinterBrandOrType);
                            _storageHelper.SaveStringToPreference("PrinterPaperSize", PaperSize);
                            _storageHelper.SaveStringToPreference("PrinterMacAddresses", MacAddress);
                            StateHasChanged();
                            if (!string.IsNullOrEmpty(PrinterBrandOrType) || !string.IsNullOrEmpty(PaperSize) || !string.IsNullOrEmpty(MacAddress))
                            {
                                if (isPrintSetup)
                                {
                                    await PrintText("Device available and ready to print.");
                                }
                            }
                            else
                            {
                                await _alertService.ShowErrorAlert("Error", "No device selected at Printer Sevice Application");
                            }
                            return (PrinterBrandOrType, PaperSize, MacAddress);
                    }
                    else if (await _alertService.ShowConfirmationReturnType("Error", "Printer Service Application not initialised Properly", "yes", "no"))
                    {
                        await _jSRuntime.InvokeVoidAsync("startAppAService");
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert("Error", "No device selected at Printer Sevice Application");
                    }
                    //{
                    //   // await _alertService.ShowErrorAlert("Error", $"Printer Service Application not initialised Properly \n {response.StatusCode} \n {response.ReasonPhrase}");
                    //}
                }
            }
            catch (Exception ex)
            {
                 //   await _alertService.ShowErrorAlert("Error", "Printer Service Application not initialised Properly");
                if (await _alertService.ShowConfirmationReturnType("Error", "Printer Service Application not initialised Properly. Do you want to Initialise Printer Service ? ", "Yes", "No"))
                {
                    await _jSRuntime.InvokeVoidAsync("startAppAService");
                }
            }
            return (null, null, null);
        }
       
        public async Task PrintText(string printString)
        {
            switch (DefPrinterType)
            {
                case PrinterType.Zebra:
                    string fontSize = "20";
                    switch (SelectedPrinterSize)
                    {
                        case PrinterSize.TwoInch:
                            fontSize = "20";
                            break;
                        case PrinterSize.ThreeInch:
                            fontSize = "30";
                            break;
                        case PrinterSize.FourInch:
                            fontSize = "20";
                            break;
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    StringBuilder zebraStringBuilder = new StringBuilder($"^XA ^POI ^FO ^^LL 300\r\n^CFA,{fontSize}\r\n^FO30,50^FD {printString} ^FS ^LL ^XZ ");
                    Winit.Modules.Printing.BL.Interfaces.IPrinter zebraPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("MacAddress"), PrinterType.Zebra, _storageHelper.GetStringFromPreferences("MacAddress"));
                    if (zebraPrinter.Type == PrinterType.Zebra)
                    {
                        if (zebraPrinter is ZebraPrinter zebra)
                        {
                            await zebra.Print(zebraStringBuilder.Append(inputText).ToString());
                        }
                    }
                    break;

                case PrinterType.Honeywell:
                    StringBuilder honeywellStringBuilder = new StringBuilder($"{printString} ");
                    Winit.Modules.Printing.BL.Interfaces.IPrinter honeywellPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("MacAddress"), PrinterType.Honeywell, _storageHelper.GetStringFromPreferences("MacAddress"));
                    if (honeywellPrinter.Type == PrinterType.Honeywell)
                    {
                        if (honeywellPrinter is HoneywellPrinter honeywell)
                        {
                            await honeywell.Print(honeywellStringBuilder.ToString());
                        }
                    }
                    break;

                case PrinterType.Woosim:
                    StringBuilder woosimStringBuilder = new StringBuilder($"{printString} ");
                    Winit.Modules.Printing.BL.Interfaces.IPrinter wooosimPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("MacAddress"), PrinterType.Woosim, _storageHelper.GetStringFromPreferences("MacAddress"));
                    if (wooosimPrinter.Type == PrinterType.Woosim)
                    {
                        if (wooosimPrinter is WoosimPrinter woosim)
                        {
                            await woosim.Print(woosimStringBuilder.ToString());
                        }
                    }
                    break;
                case PrinterType.HoneywellThermal:
                    StringBuilder HoneywellThermalStringBuilder = new StringBuilder($"{printString} ");
                    Winit.Modules.Printing.BL.Interfaces.IPrinter HoneyWellThermalPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("MacAddress"), PrinterType.HoneywellThermal, _storageHelper.GetStringFromPreferences("MacAddress"));
                    if (HoneyWellThermalPrinter.Type == PrinterType.HoneywellThermal)
                    {
                        if (HoneyWellThermalPrinter is HoneywellThermalPrinter honeywellThermalprinter)
                        {
                            await honeywellThermalprinter.Print(HoneywellThermalStringBuilder.ToString());
                        }
                    }
                    break;
            }
        }
    }
}

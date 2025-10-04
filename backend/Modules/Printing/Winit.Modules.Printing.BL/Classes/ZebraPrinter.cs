using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.Printing.Model.Interfaces;
using Winit.UIComponents.SnackBar.Services;
namespace Winit.Modules.Printing.BL.Classes
{
    public class ZebraPrinter : BasePrinter
    {
        public override async Task Print(string printString)
        {
            string fileCode = "";
            await PrintZebra(printString , fileCode);
        }
       
        public async Task PrintZebra(string document ,  string fileCode)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Byte array and encoding
                    byte[] configPrintData = null;
                    Encoding encoding = Encoding.UTF8;
                    //PrintCode = "^POI^FO" + $"^LL{PageLength }" + zplCode;
                    configPrintData = encoding.GetBytes(document);
                    client.BaseAddress = new Uri("http://localhost:8085");
                    string base64Data = Convert.ToBase64String(configPrintData);
                    var payload = new
                    {
                        Data = base64Data,
                        FileCode = fileCode
                    };
                    string jsonData = JsonConvert.SerializeObject(payload);
                    MediaTypeHeaderValue jsonMediaType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage response = await client.PostAsync("/PrintData", new StringContent(jsonData, Encoding.UTF8, jsonMediaType));
                    if (response.IsSuccessStatusCode)
                    {
                       // await _alertService.ShowErrorAlert("Error", "No device selected at Printer Sevice Application");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}

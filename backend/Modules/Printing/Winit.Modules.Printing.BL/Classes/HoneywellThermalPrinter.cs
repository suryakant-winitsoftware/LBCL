using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Printing.BL.Classes
{
    public class HoneywellThermalPrinter :BasePrinter
    {
        public override async Task Print(string printString)
        {
            string fileCode = "";
            await PrintHoneywellThermal(printString);
        }

        private async Task PrintHoneywellThermal(string document)
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
                        Data = base64Data
                    };
                    string jsonData = JsonConvert.SerializeObject(payload);
                    MediaTypeHeaderValue jsonMediaType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage response = await client.PostAsync("/PrintData", new StringContent(jsonData, Encoding.UTF8, jsonMediaType));
                    if (response.IsSuccessStatusCode)
                    {

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

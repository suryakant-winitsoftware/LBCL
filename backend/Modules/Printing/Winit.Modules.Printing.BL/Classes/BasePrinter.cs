using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.Printing.BL.Interfaces;
using Winit.Modules.Printing.Model.Classes;
namespace Winit.Modules.Printing.BL.Classes
{
    public abstract class BasePrinter : IPrinter
    {
        public string Name { get; set; }
        public PrinterType Type { get; set; }
        public string MacAddress { get; set; }

        //List to make view of available Bonded bluetooth devices for selection
        public List<BluetoothDeviceInfo> devicesList=new List<BluetoothDeviceInfo>();

        // Common method implemented in the base class
        public void CommonMethod()
        {
            // Implement common functionality shared by all printers
        }
        public async Task getPrintData(string salesOrderUID)
        {
            //var salesOrderPrintView = await _salesOrderBL.GetSalesOrderPrintView(salesOrderUID);
            // Implementation of the method
        }


        // Implementing the interface method, can be overridden by derived classes if needed
        public virtual async Task Print(string printString )
        {
            // Implement print logic that might be common among printers
            Console.WriteLine("Printer type is not available in the Specified list : " + Type);
        }

        public Task PrintAsync(string printString)
        {
            throw new NotImplementedException();
        }
    }
}

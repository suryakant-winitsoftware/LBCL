using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Printing.Model.Enum;

namespace Winit.Modules.Printing.Model.Classes
{
    public class BluetoothDeviceInfo
    {
        public string? BtName { get; set; }
        public string? macaddress { get; set; }
        public PrinterType PrinterType { get; set; }
        public PrinterSize PrinterSize { get; set; }
        
       
    }
}

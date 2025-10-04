using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Printing.Model.Enum;

namespace Winit.Modules.Printing.Model.Classes
{
    public class BluetoothDeviceInfoRequest
    {
        public string DeviceName { get; set; }
        public string MacAddress { get; set; }
        public PrinterType PrinterType { get; set; }
    }
}

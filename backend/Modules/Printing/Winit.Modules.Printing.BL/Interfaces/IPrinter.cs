using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Printing.Model.Enum;

namespace Winit.Modules.Printing.BL.Interfaces
{
    public interface IPrinter
    {
        string Name { get; set; }
        PrinterType Type { get; set; }
        string MacAddress { get; set; }
        Task Print(string printString); 
    }
}

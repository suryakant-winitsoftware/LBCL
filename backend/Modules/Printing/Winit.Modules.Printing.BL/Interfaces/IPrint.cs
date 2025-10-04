using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Printing.Model.Enum;

namespace Winit.Modules.Printing.BL.Interfaces
{
    public interface IPrint
    {
        string CreatePrintString(PrinterType printerType,PrinterSize printerSize,  object data);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Printing.Model.Enum;

namespace Winit.Modules.Printing.BL.Classes.SalesOrder
{
    public class DraftOrderPrint : BasePrint
    {
        public override string CreatePrintString(PrinterType printerType, PrinterSize printerSize, object data)
        {
            return "Draft Order Print string";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Printing.BL.Classes;
using Winit.Modules.Printing.Model.Enum;

namespace Winit.Modules.Printing.BL.Factory
{
    public static class PrinterFactory
    {
        public static BasePrinter CreatePrinter(string printerName, PrinterType type, string macAddress)
        {
            // Logic to determine the specific printer type based on attributes
            if (type == PrinterType.Zebra)
            {
                return new ZebraPrinter() { Name = printerName, Type = type, MacAddress = macAddress };
            }
            else if (type == PrinterType.Honeywell)
            {
                return new HoneywellPrinter() { Name = printerName, Type = type, MacAddress = macAddress };
            }
            else if (type == PrinterType.Woosim)
            {
                return new WoosimPrinter() { Name = printerName, Type = type, MacAddress = macAddress };
            }
            else if (type == PrinterType.HoneywellThermal)
            {
                return new HoneywellThermalPrinter() { Name = printerName, Type = type, MacAddress = macAddress };
            }
            // If no match is found, return a default printer or throw an exception
            throw new NotSupportedException("Printer type not supported");
        }
    }
}

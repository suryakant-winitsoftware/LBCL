using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.Printing.BL.Interfaces;

namespace Winit.Modules.Printing.BL.Classes
{
    public abstract class BasePrint : IPrint
    {
        public abstract string CreatePrintString(PrinterType printerType,PrinterSize printerSize , object data);
        public Dictionary<string, int> CalculateCharacterCounts(string input)
        {
            int lowerCaseCount = 0;
            int upperCaseCount = 0;
            int numericCount = 0;
            int specialCharacterCount = 0;
            foreach (char c in input)
            {
                if (char.IsLower(c))
                {
                    lowerCaseCount++;
                }
                else if (char.IsUpper(c))
                {
                    upperCaseCount++;
                }
                else if (char.IsDigit(c))
                {
                    numericCount++;
                }
                else
                {
                    specialCharacterCount++;
                }
            }
            return new Dictionary<string, int>
            {
                { "LowerCase", lowerCaseCount },
                { "UpperCase", upperCaseCount },
                { "Numeric", numericCount },
                { "Special", specialCharacterCount }
            };
        }

    }
}

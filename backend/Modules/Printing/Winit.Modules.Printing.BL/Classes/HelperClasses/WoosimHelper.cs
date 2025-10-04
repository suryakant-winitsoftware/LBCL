using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Presentation;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Printing.BL.Classes.HelperClasses
{
    public class WoosimHelper
    {
        public static int paperwidth = 90;
        public List<string> AddressLines = new List<string>
                {
                    "Win Information Technology Pvt Ltd, ",
                    "Level-16, Block A, ",
                    "Sky-1, " ,
                    "Prestige Towers, ",
                    "Hyderabad."
                };

        public static string GenerateTextAtPosition(int hWxposition, string fontToUse, string v)
        {
            StringBuilder PositionCommand = new StringBuilder();
            int position = 0;
            if(fontToUse == "\u001b|N")
            {
                position = hWxposition;
            }
            else if(fontToUse == "\u001b|4C"  || fontToUse == "\u001b|2C")
            { 
                position = hWxposition/2;
            }
            for (int i = 0; i < position; i++)
            {
                PositionCommand.Append(" ");
            }
            string a = PositionCommand + fontToUse + v;
            return a;
        }

        public string NextLine(int count)
        {
            if (count <= 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append("\r\n");
            }
            return sb.ToString();
        }
        public string GenerateunderlineOfLength(int length)
        {
            String horizontalLineCommand = "";
            for (int i = 0; i < length; i++)
            {
                String a = "-";
                horizontalLineCommand += a;
            }
            horizontalLineCommand += NextLine(1);
            return horizontalLineCommand;
        }
        public string AddHorizontalDeviderLines(int v)
        {
            String horizontalLineCommand = "";
            for (int j = 0; j < v; j++)
            {
                for (int i = 0; i < paperwidth; i++)
                {
                    String a = "-";
                    horizontalLineCommand += a;
                }
                horizontalLineCommand += NextLine(1);
            }
            return horizontalLineCommand;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Winit.Modules.Printing.Model.Enum;
using Microsoft.Extensions.Primitives;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Presentation;

namespace Winit.Modules.Printing.BL.Classes.HelperClasses
{
    public class HoneywellHelper
    {
        public static int paperwidth = 80;
        public static int paperheight = 71;
        public List<string> AddressLines = new List<string>
                {
                    "Win Information Technology Pvt Ltd, ",
                    "Level-16, Block A, ",
                    "Sky-1, " ,
                    "Prestige Towers, ",
                    "Hyderabad."
                };
        public static string GenerateTextAtPosition(int xposition, string font, string text)
        {
            StringBuilder PositionCommand = new StringBuilder();
            for (int i = 0; i < xposition; i++)
            {
                PositionCommand.Append(" ");
            }
            string a = PositionCommand + font  + text ;
            return a;
        }
        public int GetMaxLength(List<string> strings)
        {
            int maxLength = 0;
            foreach (string str in strings)
            {
                if (str.Length > maxLength)
                {
                    maxLength = str.Length;
                }
            }
            return maxLength;
        }
        public string NextLine(int count , ref int HWYposition)
        {
            if (count <= 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append("\r\n");
                HWYposition++;
            }
            return sb.ToString();
        }
        public string AddHorizontalDeviderLines(int v, ref int HWYposition)
        {
            String horizontalLineCommand = "";
            for (int j = 0; j < v; j++)
            {
                for (int i = 0; i < 80; i++)
                {
                    String a = "-";
                    horizontalLineCommand += a;
                }
                horizontalLineCommand += NextLine(1, ref HWYposition);
            }
            return horizontalLineCommand;
        }
        public string GenerateunderlineOfLength(int length, ref int HWYposition)
        {
            String horizontalLineCommand = "";
            for (int i = 0; i < length; i++)
            {
                String a = "-";
                horizontalLineCommand += a;
            }
            HWYposition++;
            horizontalLineCommand += NextLine(1, ref HWYposition);
            return horizontalLineCommand;
        }
        public static List<List<string>> ProcessHeaderLines<T>(List<T> inputList, int[] maxLengths)
        {
            var outputList = new List<List<string>>();
            foreach (var item in inputList)
            {
                var itemParts = item.GetType().GetFields().Select(f => f.GetValue(item)?.ToString() ?? string.Empty).ToArray();
                int[] maxLengthsExtended;
                if (itemParts.Length > maxLengths.Length)
                {
                    maxLengthsExtended = maxLengths.Concat(Enumerable.Repeat(0, itemParts.Length - maxLengths.Length)).ToArray();
                }
                else
                {
                    maxLengthsExtended = maxLengths;
                }
                var tempList = itemParts.ToList();
                bool hasRemaining;
                do
                {
                    var subList = new List<string>();
                    hasRemaining = false;
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        var part = tempList[i];
                        var maxLength = maxLengthsExtended[i];
                        if (maxLength > 0 && part.Length > maxLength)
                        {
                            subList.Add(part.Substring(0, maxLength));
                            tempList[i] = part.Substring(maxLength);
                            hasRemaining = true;
                        }
                        else
                        {
                            subList.Add(part);
                            tempList[i] = string.Empty;
                        }
                    }
                    outputList.Add(subList);
                }
                while (hasRemaining);
            }
            return outputList;
        }
    }
}

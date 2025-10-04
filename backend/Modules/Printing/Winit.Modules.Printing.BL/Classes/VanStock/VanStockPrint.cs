using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Printing.BL.Classes.HelperClasses;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.ReturnOrder.BL.Classes;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using WinIt.Models.Customers;

namespace Winit.Modules.Printing.BL.Classes.VanStock
{
    public class VanStockPrint : BasePrint
    {
        private List<(string description, string value)> HeaderLines = new List<(string description, string value)>();
        private List<string> AddressLines = new List<string>();
        private List<(string description, string value)> FooterLines = new List<(string description, string value)>();
        private List<(string description, string value)> AmountLines = new List<(string description, string value)>();
        List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> VanStockLinePrintViews = new List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView>();
        private string ReceiptHeader = "Van Stock";

        //For HoneyWell
        private List<(string description, string separator, string value)> HWHeaderLines = new() { };
        private List<(string description, string separator, string value)> HWFooterLines = new() { };
        private List<(string p1, string p2, string p3, string p4, string p5)> HWBodyHeaderLines = new() {("S.NO","SKU Code","SKU Name", "Qty", "Value") };


        private PrinterAlignment HeaderAlignment = PrinterAlignment.Right;
        private PrinterAlignment BodyAlignment = PrinterAlignment.Center;
        private PrinterAlignment FooterAlignment = PrinterAlignment.Left;
        private int ZebraPageWidth = 800;
        private int XPosition = 0;
        private int YPosition = 40;
        private int HeaderfontSize = 20;
        private int linesNeeded = 1;
        private int AddressLinesXPosition = 420;
        private string amount;

        // Properties for font sizes, row height, and column width
        private int PrimaryFontSize
        {
            get
            {
                return 22;
            }
        }
        private int SecondaryFontSize
        {
            get
            {
                return ZebraPageWidth / 28;
            }
        }
        private int RowHeight
        {
            get
            {
                return PrimaryFontSize / 2;
            }
        }
        private int ColumnWidth
        {
            get
            {
                return ZebraPageWidth / TableHeaders.Count;
            }
        }
        private List<string> TableHeaders = new List<string> { "SKU Code/Name", "Qty", "Value" };

        public override string CreatePrintString(PrinterType printerType, PrinterSize printerSize, object data)
        {
            if (data is (List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> StockItemsStore, Winit.Modules.Vehicle.Model.Interfaces.IVehicle vehicle, Emp.Model.Interfaces.IEmp Emp))
            {
                //Assign the linePrintViews to a local variable

                VanStockLinePrintViews = StockItemsStore;
                //Prepare the AddressLines with sales order information
                HeaderLines = new List<(string, string)>
                {
                    ("Vehicle No", vehicle.VehicleNo ?? string.Empty),
                    ("Emp No", Emp.Code?? string.Empty),
                    ("Time", DateTime.Now.ToString("dd MMM yyyy HH:mm") ?? string.Empty)

                };
                HWHeaderLines = new List<(string, string, string)>
                {
                    ("Vehicle No",":", vehicle.VehicleNo ?? string.Empty),
                    ("Emp No",":", Emp.Code ?? string.Empty),
                    ("Time",":", DateTime.Now.ToString("dd MMM yyyy HH:mm") ?? string.Empty)

                };

                AddressLines = new HoneywellHelper().AddressLines;

                decimal totalCost = StockItemsStore.Sum(item => item.TotalCost);
                amount = totalCost.ToString();

                // Prepare the FooterLines with sales order summary information
                AmountLines = new List<(string description, string value)>
                    {
                        ("Total Amount", totalCost.ToString() ?? string.Empty)
                    };

            }
            switch (printerSize)
            {
                case PrinterSize.TwoInch:
                    ZebraPageWidth = 200 * 2;
                    break;
                case PrinterSize.ThreeInch:
                    ZebraPageWidth = 200 * 3;
                    break;
                case PrinterSize.FourInch:
                    ZebraPageWidth = 200 * 4;
                    break;
            }

            // Initialize retValue as an empty string
            string retValue = string.Empty;

            // Choose the appropriate method to create the print string based on the printer type
            switch (printerType)
            {
                case PrinterType.Zebra:
                    retValue = CreateVanStockPrintZebra(data);
                    break;
                case PrinterType.Honeywell:
                    retValue = CreateVanStockPrintHoneywell(data);
                    break;
                case PrinterType.Woosim:
                     retValue = CreateVanStockPrintWoosim(data);
                    break;
                case PrinterType.HoneywellThermal:
                    retValue = CreateVanStockPrintHoneywellThermal(data);
                    break;
                default:
                    retValue = CreateVanStockPrintZebra(data); // Default to Zebra if the printer type is not specified
                    break;
            }
            // Return the generated print string
            return retValue;
        }

        


        // Method to create the Zebra print string for sales order
        private string CreateVanStockPrintZebra(object data)
        {
            if (AddressLines.Count != 0 && VanStockLinePrintViews != null)
            {
                StringBuilder salesOrderZplCode = new StringBuilder();
                salesOrderZplCode.Append(ZebraHelper.GetLogo(20, YPosition));
                salesOrderZplCode.Append(GenerateAddressCard(AddressLines));
                salesOrderZplCode.Append(GenerateZebraTitleCard("Van", " Stock", 40, "C"));
                salesOrderZplCode.Append(GenerateZebraHeaderCode(HeaderLines));
                salesOrderZplCode.Append(GenerateZebraBodyCode(VanStockLinePrintViews));
                salesOrderZplCode.Append(GenerateCashPaymentFooter(AmountLines));
                //salesOrderZplCode.Append(GenerateZebraHeaderCode(FooterLines));
                YPosition += RowHeight * 2;
                salesOrderZplCode.Insert(0, ZebraHelper.SetLabelLength(YPosition));
                return salesOrderZplCode.ToString();
            }
            return string.Empty;
        }
        private string GenerateCashPaymentFooter(List<(string description, string value)> footerLines)
        {
            StringBuilder TitleCode = new StringBuilder();
            for (int i = 0; i < footerLines.Count; i++)
            {
                string Value = footerLines[i].value;
                string Description = footerLines[i].description;
                TitleCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, 0, "C", PrimaryFontSize, ColumnWidth, Description));
                XPosition += 2 * ColumnWidth;
                TitleCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, 0, "C", PrimaryFontSize, ColumnWidth, Value));
            }
            XPosition = default;
            YPosition = YPosition + PrimaryFontSize;
            TitleCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, 0, YPosition, 1, 4));
            YPosition += PrimaryFontSize;
            return TitleCode.ToString();
        }
        private string GenerateAddressCard(List<string> addressLines)
        {
            StringBuilder AddressCardCode = new StringBuilder();

            //AddressCardCode.Append(ZebraHelper.GenerateTitleCardwithAlignment(AddressLinesXPosition, YPosition, "L", 25, addressLines[0], HeaderfontSize));
            YPosition += 30;
            for (int i = 1; i < addressLines.Count; i++)
            {
                //    AddressCardCode.Append(ZebraHelper.GenerateTitleCardwithAlignment(AddressLinesXPosition, YPosition, "", 17, addressLines[i], HeaderfontSize));
                YPosition += 17;
            }
            YPosition += 50;
            return AddressCardCode.ToString();

        }
        private string GenerateZebraHeaderCode(List<(string description, string value)> footerLines)
        {
            StringBuilder HeaderCode = new StringBuilder();
            int x = 1;
            int charactersLengthForFontSize20 = 90;
            int x1 = charactersLengthForFontSize20 / 2;
            for (int i = 0; i < footerLines.Count(); i++)
            {
                int maxLinesNeeded = 0;
                string fieldName = footerLines[i].Item1;
                string? fieldValue = footerLines[i].Item2;

                // Adjust column widths as needed
                int fieldNameWidth = 180;
                int middlePartWidth = 20;
                int fieldValueWidth = 200;
                int fieldNameRatio = (45 * x1 / 100);
                int fieldValueRatio = (50 * x1 / 100);
                int linesneededForItem1 = fieldName.Length / fieldNameRatio;
                int linesneededForItem2 = fieldValue.Length / fieldValueRatio;

                int linesNeeded = Math.Max(linesneededForItem1, linesneededForItem2);

                // Update maxLinesNeeded if the current header requires more lines
                if (linesNeeded > maxLinesNeeded)
                {
                    maxLinesNeeded = linesNeeded;
                    YPosition += HeaderfontSize;
                }

                string line = $"{ZebraHelper.GenerateFieldOrigin(x, YPosition + HeaderfontSize)}" +
                              $"{ZebraHelper.GenerateTableHeaderCellData(x, YPosition, maxLinesNeeded, "L", HeaderfontSize, fieldNameWidth, fieldName)}" +
                              $"{ZebraHelper.GenerateFieldOrigin(x + fieldNameWidth, YPosition + HeaderfontSize)}" +
                              $"{ZebraHelper.GenerateTableHeaderCellData(x + fieldNameWidth, YPosition, maxLinesNeeded, "C", HeaderfontSize, middlePartWidth, ":")}" +
                              $"{ZebraHelper.GenerateFieldOrigin(x + fieldNameWidth + middlePartWidth, YPosition + HeaderfontSize)}" +
                              $"{ZebraHelper.GenerateTableHeaderCellData(x + fieldNameWidth + middlePartWidth, YPosition, maxLinesNeeded, "L", HeaderfontSize, fieldValueWidth, fieldValue)}";
                // Append the ZPL code to the StringBuilder
                HeaderCode.Append(line);
                YPosition += 3 * HeaderfontSize / 2;
            }
            YPosition += 2 * HeaderfontSize;

            return HeaderCode.ToString();
        }
        public string GenerateZebraBodyCode(List<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> VanPrintViews)
        {
            StringBuilder tableHeaderCode = new StringBuilder();
            StringBuilder tableBodyCode = new StringBuilder();
            StringBuilder tableCode = new StringBuilder();
            tableHeaderCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, 0, YPosition, 1, 5));
            YPosition = YPosition + RowHeight;
            int MaxHeaderlines = 1;
            foreach (string headerValue in TableHeaders)
            {
                int linesneeded = (int)ZebraHelper.MeasureTextWidth(headerValue, PrimaryFontSize) / ColumnWidth;
                tableHeaderCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesneeded, "C", PrimaryFontSize, ColumnWidth, headerValue));
                XPosition += ColumnWidth;
                if (linesneeded >= MaxHeaderlines) MaxHeaderlines = linesneeded;
            }
            YPosition += (MaxHeaderlines * PrimaryFontSize);
            tableHeaderCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, 0, YPosition, 1, 5));
            YPosition += RowHeight;
            foreach (IWarehouseStockItemView warehouseLinePrintView in VanStockLinePrintViews)
            {
                XPosition = default;

                List<string> Values = new List<string> {
                    //warehouseLinePrintView.LineNumber.ToString(),
                    warehouseLinePrintView.SKUCode.ToString(),
                    warehouseLinePrintView.Qty.ToString(),
                    warehouseLinePrintView.TotalCost.ToString()

                };
                int linesNeededForTableCelldata = (int)ZebraHelper.MeasureTextWidth(Values.OrderByDescending(s => s.Length).FirstOrDefault(), SecondaryFontSize) / ColumnWidth;
                if (linesNeededForTableCelldata > linesNeeded)
                    linesNeeded = linesNeededForTableCelldata;
                foreach (string cellValue in Values)
                {
                    tableBodyCode.Append(ZebraHelper.GenerateTableCellData(XPosition, YPosition, ColumnWidth, linesNeeded, SecondaryFontSize, cellValue));
                    XPosition += ColumnWidth;
                }
                YPosition += linesNeeded * (SecondaryFontSize);
            }
            XPosition = default;
            YPosition += RowHeight;
            // tableBodyCode.Append(ZebraHelper.GenerateDottedLine(ZebraPageWidth, 0, YPosition, 20));
            tableHeaderCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, 0, YPosition, 1, 5));
            YPosition = YPosition + RowHeight;
            tableCode.Append(tableHeaderCode);
            tableCode.Append(tableBodyCode);
            return tableCode.ToString();
        }
        private string GenerateZebraTitleCard(string v1, string v2, int Titlefontsize, string Alignment)
        {
            StringBuilder TitleCode = new StringBuilder();
            int linesneededForItem = (int)Math.Ceiling((double)(v1.Length + v2.Length) / 51);
            string line = $"{ZebraHelper.GenerateTitleCard(0, YPosition, Alignment, Titlefontsize, (v1 + v2), HeaderfontSize)}";
            YPosition += linesneededForItem * 3 * Titlefontsize / 2;
            TitleCode.Append(line);
            return TitleCode.ToString();
        }





        /* ************************************************************************************************************************************************************************************************* */
        
        #region HoneyWell

        private int HoneywellPageWidth = HoneywellHelper.paperwidth;
        private int HoneywellPageHeight = HoneywellHelper.paperheight;
        private int HWxposition = 0;
        private int HWYposition = 0;
        private string HWPrimaryFontSize = "\u001B" + "W" + "0";
        private string HWHeaderFontSize = "\u001B" + "W" + "1";
        private string HWPrimaryFontStyle = "\u001B" + "E" + "0";
        private string HWBoldFontStyle = "\u001B" + "E" + "1";
        private int[] HWReceiptHeaderPositions = new int[] { };
        private int[] HWReceiptHeaderRatio = new int[] { };
        private int[] HWReceiptTableHeaderPositions = new int[] { };
        private int[] HWReceiptTableHeaderRatio = new int[] { };
        private string CreateVanStockPrintHoneywell(object data)
        {
            StringBuilder vanStock = new StringBuilder();
            if (VanStockLinePrintViews != null)
            {
                string addressCard = GenerateAddressCardAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeader(ReceiptHeader);
                string receiptHeaderData = GenerateReceiptHeaderData(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderData(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyData(VanStockLinePrintViews);
                string receiptAmountData = GenerateReceiptAmountData(AmountLines);
                string AddReceiptAlignmentForPage = AlignGapBetweenBodyAndFooter();


                vanStock.Append(addressCard);
                vanStock.Append(receiptTitleHeader);
                vanStock.Append(receiptHeaderData);
                vanStock.Append(receiptTableHeaderData);
                vanStock.Append(receiptTableBodyData);
                vanStock.Append(receiptAmountData);
                vanStock.Append(AddReceiptAlignmentForPage);
            }
            return vanStock.ToString();
        }

        

        private string GenerateAddressCardAtTop(List<string> addressLines)
        {
            HWxposition = HoneywellPageWidth - new HoneywellHelper().GetMaxLength(addressLines);
            StringBuilder sb = new StringBuilder();
            sb.Append(new HoneywellHelper().NextLine(3, ref HWYposition));
            for (int i = 0; i < addressLines.Count; i++)
            {
                string fontToUse = (i == 0) ? "\u001bW0" : "\u001bW0";
                sb.Append(HoneywellHelper.GenerateTextAtPosition(HWxposition, fontToUse, addressLines[i]));
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            }
            sb.Append(new HoneywellHelper().NextLine(3, ref HWYposition));
            HWxposition = default;
            return sb.ToString();
        }
        private string GenerateReceiptTitleHeader(string receiptHeader)
        {
            HWxposition = (HoneywellPageWidth - (2 * receiptHeader.Length)) / 2;
            StringBuilder sb = new StringBuilder();
            sb.Append(HoneywellHelper.GenerateTextAtPosition(HWxposition, HWHeaderFontSize, receiptHeader));
            sb.Append(new HoneywellHelper().NextLine(2, ref HWYposition));
            HWxposition = default;
            return sb.ToString();
        }
        private string GenerateReceiptHeaderData(List<(string description, string separator, string value)>? headerLines)
        {
            StringBuilder sb = new StringBuilder();
            HWReceiptHeaderPositions = new int[] { 0, 17, 19 };
            HWReceiptHeaderRatio = new int[] { 15, 1, 50 };
            sb.Append(HoneywellHelper.GenerateTextAtPosition(0, HWPrimaryFontSize, ""));
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            var processedList = HoneywellHelper.ProcessHeaderLines(headerLines, HWReceiptHeaderRatio);
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = HWReceiptHeaderPositions[j] - xpp;
                    string value = processedList[i][j];
                    string generatedText = HoneywellHelper.GenerateTextAtPosition(xp, HWPrimaryFontSize, value);
                    xpp = HWReceiptHeaderPositions[j] + value.Length;
                    sb.ToString();
                    sb.Append(generatedText);
                }
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            }
            sb.Append(new HoneywellHelper().NextLine(2, ref HWYposition));
            return sb.ToString();
        }
        private string GenerateReceiptTableHeaderData(List<(string p1, string p2, string p3, string p4, string p5)> hWBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            HWReceiptTableHeaderPositions = new int[] { 0, 5, 21, 52, 65 };
            HWReceiptTableHeaderRatio = new int[] { 5, 15, 30, 12, 15 };
            var processedList = HoneywellHelper.ProcessHeaderLines(hWBodyHeaderLines, HWReceiptTableHeaderRatio);
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = HWReceiptTableHeaderPositions[j] - xpp;
                    string value = processedList[i][j];
                    string generatedText = HoneywellHelper.GenerateTextAtPosition(xp, HWPrimaryFontSize, value);
                    sb.Append(generatedText);
                    xpp = HWReceiptTableHeaderPositions[j] + value.Length;
                }
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            }
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptTableBodyData(List<IWarehouseStockItemView> vanStockLinePrintViews)
        {
            StringBuilder sb = new StringBuilder();
            HWReceiptTableHeaderPositions = new int[] { 0, 5, 21, 52, 65 };
            HWReceiptTableHeaderRatio = new int[] { 5, 15, 30, 12, 15 };
            int linenumber = 1;
            foreach (IWarehouseStockItemView warehouseLinePrintView in VanStockLinePrintViews)
            {
                List<(string p1, string p2, string p3, string p4)> Values = new List<(string p1, string p2, string p3, string p4)>
                {
                    (linenumber.ToString() ?? string.Empty,
                    warehouseLinePrintView.SKUCode.ToString() ?? string.Empty,
                    warehouseLinePrintView.Qty.ToString() ?? string.Empty,
                    warehouseLinePrintView.TotalCost.ToString() ?? string.Empty)
                };
                linenumber++;
                var processedList = HoneywellHelper.ProcessHeaderLines(Values, HWReceiptTableHeaderRatio);
                for (int i = 0; i < processedList.Count; i++)
                {
                    int xpp = 0;
                    for (int j = 0; j < processedList[i].Count; j++)
                    {
                        int xp = HWReceiptTableHeaderPositions[j] - xpp;
                        string value = processedList[i][j];
                        string generatedText = HoneywellHelper.GenerateTextAtPosition(xp, HWPrimaryFontSize, value);
                        sb.Append(generatedText);
                        xpp = HWReceiptTableHeaderPositions[j] + value.Length;
                    }
                }
                sb.Append(new HoneywellHelper().NextLine(2, ref HWYposition));
            }
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptAmountData(List<(string description, string value)> amountLines)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < amountLines.Count; i++)
            {
                string des = amountLines[i].description;
                string value = amountLines[i].value;
                int x1 = HWReceiptTableHeaderPositions[0];
                string generatedDes = HoneywellHelper.GenerateTextAtPosition(x1, HWPrimaryFontSize, des);
                int x2 = HWReceiptTableHeaderPositions[HWReceiptTableHeaderPositions.Length -1] - des.Length - x1;
                string generatedValue = HoneywellHelper.GenerateTextAtPosition((x2), HWPrimaryFontSize, value);
                sb.Append(generatedDes);
                sb.Append(generatedValue);
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            }
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptFooterData(List<(string description, string separator, string value)> hWFooterLines)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            for (int i = 0; i < hWFooterLines.Count; i++)
            {
                int x1 = HWReceiptTableHeaderPositions[0];
                int x2 = HWReceiptTableHeaderPositions[1];
                string value = hWFooterLines[i].ToString();
                if (i == 0)
                {
                    string generatedText = HoneywellHelper.GenerateTextAtPosition(x1, HWPrimaryFontSize, value);
                    sb.Append(generatedText);
                }
                else
                {
                    string generatedText = HoneywellHelper.GenerateTextAtPosition((x2 - x1), HWPrimaryFontSize, value);
                    sb.Append(generatedText);
                }
                sb.Append(new HoneywellHelper().NextLine(4, ref HWYposition));
            }
            return sb.ToString();
        }
        private string AlignGapBetweenBodyAndFooter()
        {
            return new HoneywellHelper().NextLine(HoneywellPageHeight - (YPosition % HoneywellPageHeight), ref HWYposition);
        }


        #endregion

        /* ************************************************************************************************************************************************************************************************* */
        #region  Woosim

        private int WoosimPageWidth = WoosimHelper.paperwidth;
        private int Wxposition = 0;
        private string WPrimaryFontSize = "\u001b|N";
        private string WHeadingFontSize = "\u001b|4C";
        private string WSubHeadingFontSize = "\u001b|2C";
        private int[] WReceiptHeaderPositions = new int[] { };
        private int[] WReceiptHeaderRatio = new int[] { };
        private int[] WReceiptTableHeaderPositions = new int[] { };
        private int[] WReceiptTableHeaderRatio = new int[] { };
        private int[] WReceiptFooterPositions = new int[] { };
        private int[] WReceiptFooterRatio = new int[] { };

        private string CreateVanStockPrintWoosim(object data)
        {
            StringBuilder vanStock = new StringBuilder();
            if (VanStockLinePrintViews != null)
            {
                string addressCard = GenerateAddressCardWoosimAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeaderWoosim(ReceiptHeader);
                string receiptHeaderData = GenerateReceiptHeaderDataWoosim(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderDataWoosim(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyDataWoosim(VanStockLinePrintViews);
                string receiptAmountData = GenerateReceiptAmountDataWoosim(AmountLines);


                vanStock.Append(addressCard);
                vanStock.Append(receiptTitleHeader);
                vanStock.Append(receiptHeaderData);
                vanStock.Append(receiptTableHeaderData);
                vanStock.Append(receiptTableBodyData);
                vanStock.Append(receiptAmountData);
            }
            return vanStock.ToString();
        }
        private string GenerateAddressCardWoosimAtTop(List<string> addressLines)
        {
            Wxposition = WoosimPageWidth - new HoneywellHelper().GetMaxLength(addressLines);
            StringBuilder sb = new StringBuilder();
            sb.Append(new WoosimHelper().NextLine(3));
            for (int i = 0; i < addressLines.Count; i++)
            {
                sb.Append(WoosimHelper.GenerateTextAtPosition(Wxposition, WPrimaryFontSize, addressLines[i]));
                sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().NextLine(1));
            Wxposition = default;
            return sb.ToString();
        }
        private string GenerateReceiptTitleHeaderWoosim(string receiptHeader)
        {
            Wxposition = (WoosimPageWidth - (2 * receiptHeader.Length));
            StringBuilder sb = new StringBuilder();
            sb.Append(WoosimHelper.GenerateTextAtPosition(Wxposition, WHeadingFontSize, receiptHeader));
            sb.Append(new WoosimHelper().NextLine(1));
            HWxposition = default;
            return sb.ToString();
        }
        private string GenerateReceiptHeaderDataWoosim(List<(string description, string separator, string value)>? headerLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptHeaderPositions = new int[] { 0, 17, 19 };
            WReceiptHeaderRatio = new int[] { 15, 1, 50 };
            sb.Append(WoosimHelper.GenerateTextAtPosition(0, WPrimaryFontSize, ""));
            sb.Append(new WoosimHelper().NextLine(1));
            var processedList = HoneywellHelper.ProcessHeaderLines(headerLines, WReceiptHeaderRatio);
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = WReceiptHeaderPositions[j] - xpp;
                    string value = processedList[i][j];
                    string generatedText = WoosimHelper.GenerateTextAtPosition(xp, WPrimaryFontSize, value);
                    xpp = WReceiptHeaderPositions[j] + value.Length;
                    sb.ToString();
                    sb.Append(generatedText);
                }
                sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().NextLine(2));
            return sb.ToString();
        }
        private string GenerateReceiptTableHeaderDataWoosim(List<(string p1, string p2, string p3, string p4, string p5)> hWBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptTableHeaderPositions = new int[] { 0, 7, 20, 60, 72 };
            WReceiptTableHeaderRatio = new int[] { 5, 15, 35, 12, 6 };
            var processedList = HoneywellHelper.ProcessHeaderLines(hWBodyHeaderLines, WReceiptTableHeaderRatio);
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = WReceiptTableHeaderPositions[j] - xpp;
                    string value = processedList[i][j];
                    string generatedText = WoosimHelper.GenerateTextAtPosition(xp, WPrimaryFontSize, value);
                    sb.Append(generatedText);
                    xpp = WReceiptTableHeaderPositions[j] + value.Length;
                }
                sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptTableBodyDataWoosim(List<IWarehouseStockItemView> vanStockLinePrintViews)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptTableHeaderPositions = new int[] { 0, 7, 20, 60, 72 };
            WReceiptTableHeaderRatio = new int[] { 5, 15, 35, 12, 6 };
            int linenumber = 1;
            foreach (IWarehouseStockItemView warehouseLinePrintView in VanStockLinePrintViews)
            {
                List<(string p1, string p2, string p3, string p4, string p5)> Values = new List<(string p1, string p2, string p3, string p4, string p5)>
                {
                    (linenumber.ToString() ?? string.Empty,
                    warehouseLinePrintView.SKUCode.ToString() ?? string.Empty,
                    warehouseLinePrintView.SKUName.ToString() ?? string.Empty,
                    warehouseLinePrintView.Qty.ToString() ?? string.Empty,
                    warehouseLinePrintView.TotalCost.ToString() ?? string.Empty)
                };
                linenumber++;
                var processedList = HoneywellHelper.ProcessHeaderLines(Values, WReceiptTableHeaderRatio);
                for (int i = 0; i < processedList.Count; i++)
                {
                    int xpp = 0;
                    for (int j = 0; j < processedList[i].Count; j++)
                    {
                        int xp = WReceiptTableHeaderPositions[j] - xpp;
                        string value = processedList[i][j];
                        string generatedText = WoosimHelper.GenerateTextAtPosition(xp, WPrimaryFontSize, value);
                        sb.Append(generatedText);
                        xpp = WReceiptTableHeaderPositions[j] + value.Length;
                    }
                    sb.Append(new WoosimHelper().NextLine(1));
                }
               // sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptAmountDataWoosim(List<(string description, string value)> amountLines)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < amountLines.Count; i++)
            {
                string des = amountLines[i].description;
                string value = amountLines[i].value;
                int x1 = WReceiptTableHeaderPositions[0];
                string generatedDes = WoosimHelper.GenerateTextAtPosition(x1, WPrimaryFontSize, des);
                int x2 = WReceiptTableHeaderPositions[WReceiptTableHeaderPositions.Length - 1] - des.Length - x1;
                string generatedValue = WoosimHelper.GenerateTextAtPosition(x2, WPrimaryFontSize, value);
                sb.Append(generatedDes);
                sb.Append(generatedValue);
                sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            string output = sb.ToString();
            return output;
        }


        #endregion

        /* ************************************************************************************************************************************************************************************************* */
        #region Honeywell Thermal Bitmap

        private int HoneywellThermalPrinterWidth = 830;
        private int HTPHeaderFontSize = 20;

        private int bitmapxposition = 0;
        private int bitmapyposition = 0;
        private string CreateVanStockPrintHoneywellThermal(object data)
        {
            if (VanStockLinePrintViews != null)
            {
                // Create bitmap parts
                //Bitmap headerBitmap = CreateAddressCardAtTopHeaderBitmap(AddressLines);
                //Bitmap bodyBitmap = CreateBodyBitmap(HoneywellThermalPrinterWidth);
                //Bitmap footerBitmap = CreateFooterBitmap(HoneywellThermalPrinterWidth);
                //int totalHeight = headerBitmap.Height + bodyBitmap.Height + footerBitmap.Height;
            }
            return "";
        }

        //private Bitmap CreateAddressCardAtTopHeaderBitmap(List<string> addressLines)
        //{
        //    bitmapxposition = HoneywellThermalPrinterWidth - new HoneywellHelper().GetMaxLength(addressLines);

        //    throw new NotImplementedException();
        //}

        //private Bitmap CreateFooterBitmap(object fixedWidth)
        //{
        //    throw new NotImplementedException();
        //}

        //private Bitmap CreateBodyBitmap(object fixedWidth)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

    }
}

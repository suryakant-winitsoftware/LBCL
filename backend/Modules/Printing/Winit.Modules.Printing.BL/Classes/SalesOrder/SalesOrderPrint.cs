using NPOI.Util;
using System.Text;
using Winit.Modules.Printing.BL.Classes.HelperClasses;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.Printing.BL.Classes.SalesOrder
{
    public class SalesOrderPrint : BasePrint
    {
        private string ReceiptHeader = "Sales Order";

        // Basic Lists
        private List<(string description, string value)> HeaderLines = new() { };
        private List<string> AddressLines = new() { };
        private List<(string description, string value)> FooterLines = new() { };
        private List<(string description, string value)> AmountLines = new();
        private List<ISalesOrderLinePrintView> salesOrderLinePrintViews = new();

        // For HoneyWell 
        private List<(string description, string separator, string value)> HWHeaderLines = new() { };
        private List<(string description, string separator, string value)> HWFooterLines = new() { };
        private List<(string p1, string p2, string p3, string p4, string p5, string p6, string p7)> HWBodyHeaderLines = new() { };
        
       

        /* ************************************************************************************************************************************************************************************************* */
          //Zebra
        private int ZebraPageWidth;
        private readonly string NormalFontypeSizeType = "N";
        private readonly string BoldFontType = "0";
        private readonly string NormalFont = "N";
        private readonly string LeftAlign = "L";
        private readonly string rightAlign = "R";
        private readonly string CentreAlign = "C";
        private readonly string Separator = ":";
        private int XPosition = 0;
        private readonly int AddressLinesXPosition = 420;
        private int YPosition = 0;
        private readonly int TextFontSize = 20;
        private readonly int BodyTableHeaderFontSize = 22;
        private readonly int Header1TextFontSize = 40;
        private readonly int AddressCard1TextFontSize = 25;
        private readonly int AddressCard2TextFontSize = 17;
        private readonly int AddBlockSpace = 50;
        private readonly int LineThickness = 5;
        private int UpperCaseCount;
        private int LowerCaseCount;
        private int NumericCaseCount;
        private int SpecialCharacterCount;
        private string? amount;
        private string? OrderDate;
        private string? OrderNum;
        private List<string> TableHeaders = new() { };
        private int[]? TableHeadersRatio;
        
   /* ************************************************************************************************************************************************************************************************* */
        public override string CreatePrintString(PrinterType printerType, PrinterSize printerSize, object data)
        {
            TableHeaders = new List<string> { "SNo", "Description", "UOM", "QTY", "Unit Price", "Discount", "Total Price" };
            HWBodyHeaderLines = new() { ("SNo", "Description", "UOM", "QTY", "Unit-Price", "Discount", "Total-Price") };
            TableHeadersRatio = new int[] { 114, 114, 114, 114, 114, 114, 114 };
            if (data is (ISalesOrderPrintView salesOrderPrintView, List<ISalesOrderLinePrintView> linePrintViews))
            {
                salesOrderLinePrintViews = linePrintViews;
                HeaderLines = new List<(string, string)>
                {
                    ("Store Name", salesOrderPrintView.StoreName ?? string.Empty),
                    ("Store Code", salesOrderPrintView.StoreCode ?? string.Empty),
                    ("Address ", (salesOrderPrintView.AddressLine1 + salesOrderPrintView.AddressLine2 + salesOrderPrintView.AddressLine3) ?? string.Empty),
                    ("Store Number",salesOrderPrintView.StoreNumber ?? string.Empty)
                };
                HWHeaderLines = new List<(string, string, string)>
                {
                    ("Store Name",":", salesOrderPrintView.StoreName ?? string.Empty),
                    ("Store Code",":", salesOrderPrintView.StoreCode ?? string.Empty),
                    ("Address ", ":",(salesOrderPrintView.AddressLine1 + salesOrderPrintView.AddressLine2 + salesOrderPrintView.AddressLine3) ?? string.Empty),
                    ("Store Number",":",salesOrderPrintView.StoreNumber ?? string.Empty)
                };
                AddressLines = new List<string>
                {
                    "Win Information Technology Pvt Ltd, ",
                    "Level-16, Block A, ","Sky-1, " ,"Prestige Towers, ",
                    "Hyderabad."
                };
                amount = salesOrderPrintView.TotalAmount.ToString();
                OrderDate = salesOrderPrintView.DeliveredDateTime.ToString("dd MMM yyyy HH:mm");
                OrderNum = salesOrderPrintView.SalesOrderNumber;
                AmountLines = new List<(string description, string value)>
                {
                         ("Total Amount",amount)
                };
                FooterLines = new List<(string, string)>
                {
                    ("Quantity", salesOrderPrintView.QtyCount.ToString()),
                    ("Total Discount", salesOrderPrintView.TotalDiscount.ToString()),
                    ("Total Tax", salesOrderPrintView.TotalTax.ToString())
                };
                HWFooterLines = new List<(string, string, string)>
                {
                    ("Quantity", ":",salesOrderPrintView.QtyCount.ToString()),
                    ("Total Discount",":", salesOrderPrintView.TotalDiscount.ToString()),
                    ("Total Tax",":", salesOrderPrintView.TotalTax.ToString())
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
            string retValue = printerType switch
            {
                PrinterType.Zebra => CreateSalesOrderPrintZebra(data),
                PrinterType.Honeywell => CreateSalesOrderPrintHoneywell(data),
                PrinterType.Woosim => CreateSalesOrderPrintWoosim(data),
                _ => CreateSalesOrderPrintZebra(data),      // Default to Zebra if the printer type is not specified
            };
            return retValue;
        }

        
        /* ************************************************************************************************************************************************************************************************* */

        #region Zebra 

        private string CreateSalesOrderPrintZebra(object data)
        {
            if (AddressLines.Count != 0 && FooterLines.Count != 0 && salesOrderLinePrintViews != null)
            {
                StringBuilder salesOrderZplCode = new();
                _ = salesOrderZplCode.Append(ZebraHelper.GetLogo(XPosition, YPosition));
                _ = salesOrderZplCode.Append(GenerateAddressCard(AddressLines));
                _ = salesOrderZplCode.Append(GenerateZebraTitleCard("Sales ", "Order", Header1TextFontSize, CentreAlign));
                _ = salesOrderZplCode.Append(GenerateZebraHeaderCode(HeaderLines, NormalFontypeSizeType));
                _ = salesOrderZplCode.Append(GenerateZebraBodyCode(salesOrderLinePrintViews));
                _ = salesOrderZplCode.Append(GenerateZebraHeaderCode(FooterLines, NormalFontypeSizeType));
                _ = salesOrderZplCode.Insert(0, ZebraHelper.SetLabelLength(YPosition));
                return salesOrderZplCode.ToString();
            }
            return string.Empty;
        }
        private string GenerateAddressCard(List<string> addressLines)
        {
            StringBuilder AddressCardCode = new();

            //  AddressCardCode.Append(ZebraHelper.GenerateTitleCardwithAlignment(AddressLinesXPosition, YPosition, LeftAlign, AddressCard1TextFontSize, addressLines[0], AddressCard1TextFontSize));
            YPosition += AddressCard1TextFontSize;
            for (int i = 1; i < addressLines.Count; i++)
            {
                //      AddressCardCode.Append(ZebraHelper.GenerateTitleCardwithAlignment(AddressLinesXPosition, YPosition, LeftAlign, AddressCard2TextFontSize, addressLines[i], AddressCard2TextFontSize));
                YPosition += AddressCard2TextFontSize;
            }
            YPosition += AddBlockSpace;
            return AddressCardCode.ToString();
        }
        private string GenerateZebraTitleCard(string v1, string v2, int Titlefontsize, string Alignment)
        {
            StringBuilder TitleCode = new();
            int linesneededForItem = (int)Math.Ceiling((double)(v1.Length + v2.Length) / 51);
            string line = $"{ZebraHelper.GenerateTitleCard(0, YPosition, Alignment, Titlefontsize, v1 + v2, Header1TextFontSize)}";
            YPosition += linesneededForItem * 3 * Titlefontsize / 2;
            _ = TitleCode.Append(line);
            return TitleCode.ToString();
        }
        private string GenerateZebraHeaderCode(List<(string description, string value)> headerLines, string fonttype)
        {
            StringBuilder HeaderCode = new();
            int fieldNameWidth = 200;
            int middlePartWidth = 20;
            int fieldValueWidth = 580;
            int maxLinesNeeded = 1;
            GetCharactersDictionaryCountWithFontTypeAndFontSize(TextFontSize, fonttype);
            for (int i = 0; i < headerLines.Count(); i++)
            {
                string fieldName = headerLines[i].description;
                Dictionary<string, int> fieldNameCounts = CalculateCharacterCounts(fieldName);
                int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                int fieldNameNumericCount = fieldNameCounts["Numeric"];
                int fieldNameSpecialCount = fieldNameCounts["Special"];
                double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);

                string fieldValue = headerLines[i].value;
                Dictionary<string, int> fieldValueCounts = CalculateCharacterCounts(fieldValue);
                int fieldValueLowerCount = fieldValueCounts["LowerCase"];
                int fieldValueUpperCount = fieldValueCounts["UpperCase"];
                int fieldValueNumericCount = fieldValueCounts["Numeric"];
                int fieldValueSpecialCount = fieldValueCounts["Special"];
                double fieldValuelength = CalculateTextFieldLength(fieldValueLowerCount, fieldValueUpperCount, fieldValueNumericCount, fieldValueSpecialCount);

                double linesneededForItem1 = (double)fieldName.Length / fieldNameWidth;
                double linesneededForItem2 = (double)fieldValue.Length / fieldValueWidth;

                int linesNeededForItem1 = (int)Math.Ceiling(linesneededForItem1);
                int linesNeededForItem2 = (int)Math.Ceiling(linesneededForItem2);

                int linesNeeded = Math.Max(linesNeededForItem1, linesNeededForItem2);

                string line = $"{ZebraHelper.GenerateFieldOrigin(XPosition, YPosition + TextFontSize)}" +
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition, YPosition, maxLinesNeeded, LeftAlign, NormalFontypeSizeType, TextFontSize, fieldNameWidth, fieldName)}" +
                              $"{ZebraHelper.GenerateFieldOrigin(XPosition + fieldNameWidth, YPosition + TextFontSize)}" +
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition + fieldNameWidth, YPosition, maxLinesNeeded, CentreAlign, NormalFontypeSizeType, TextFontSize, middlePartWidth, Separator)}" +
                              $"{ZebraHelper.GenerateFieldOrigin(XPosition + fieldNameWidth + middlePartWidth, YPosition + TextFontSize)}" +
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition + fieldNameWidth + middlePartWidth, YPosition, maxLinesNeeded, LeftAlign, NormalFontypeSizeType, TextFontSize, fieldValueWidth, fieldValue)}";
                _ = HeaderCode.Append(line);
                if (linesNeeded > maxLinesNeeded)
                {
                    maxLinesNeeded = 1 + linesNeeded;
                    YPosition += maxLinesNeeded * TextFontSize;
                }
                else
                {
                    YPosition += TextFontSize;
                }
            }
            YPosition += 2 * TextFontSize;
            return HeaderCode.ToString();
        }
        public string GenerateZebraBodyCode(List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView> salesOrderLinePrintViews)
        {
            StringBuilder TableCode = new();
            _ = TableCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition += (2 * LineThickness);
            string fonttype = NormalFontypeSizeType;
            GetCharactersDictionaryCountWithFontTypeAndFontSize(TextFontSize, fonttype);
            int Bodylines = 1;
            foreach (string headerValue in TableHeaders)
            {
                int i = 0;
                string fieldName = headerValue;
                Dictionary<string, int> fieldNameCounts = CalculateCharacterCounts(fieldName);
                int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                int fieldNameNumericCount = fieldNameCounts["Numeric"];
                int fieldNameSpecialCount = fieldNameCounts["Special"];
                double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);
                int linesNeeded = (int)Math.Ceiling((double)fieldName.Length / TableHeadersRatio[i]);
                _ = TableCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesNeeded, "C", TextFontSize, TableHeadersRatio[i], headerValue));
                XPosition += TableHeadersRatio[i];
                if (linesNeeded > Bodylines)
                {
                    Bodylines = linesNeeded;
                }
            }
            YPosition += Bodylines * TextFontSize;
            XPosition = default;
            _ = TableCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition += (2 * LineThickness);
            string fontChangetype = NormalFontypeSizeType;
            GetCharactersDictionaryCountWithFontTypeAndFontSize(TextFontSize, fontChangetype);
            foreach (ISalesOrderLinePrintView salesOrderLinePrintView in salesOrderLinePrintViews)
            {
                List<string> Values = new()
                {
                    salesOrderLinePrintView.LineNumber.ToString(),
                    salesOrderLinePrintView.SKUDescription,
                    salesOrderLinePrintView.UoM,
                    salesOrderLinePrintView.Qty.ToString(),
                    salesOrderLinePrintView.UnitPrice.ToString(),
                    salesOrderLinePrintView.TotalDiscount.ToString(),
                    salesOrderLinePrintView.TotalAmount.ToString()
                };
                Bodylines = 1;
                foreach (string Tablevalue in Values)
                {
                    int i = 0;
                    string fieldName = Tablevalue;
                    Dictionary<string, int> fieldNameCounts = CalculateCharacterCounts(fieldName);
                    int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                    int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                    int fieldNameNumericCount = fieldNameCounts["Numeric"];
                    int fieldNameSpecialCount = fieldNameCounts["Special"];
                    double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);
                    int linesNeeded = (int)Math.Ceiling((double)fieldNamelength / TableHeadersRatio[i]);
                    _ = TableCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesNeeded, "C", TextFontSize, TableHeadersRatio[i], Tablevalue));
                    XPosition += TableHeadersRatio[i];
                    if (linesNeeded > Bodylines)
                    {
                        Bodylines = linesNeeded;
                    }
                }
                YPosition += Bodylines * TextFontSize;
                XPosition = default;
            }
            YPosition += LineThickness;
            _ = TableCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition += (2 * LineThickness);

            for (int i = 0; i < AmountLines.Count; i++)
            {
                string Value = AmountLines[i].value;
                string Description = AmountLines[i].description;
                _ = TableCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, 0, "L", TextFontSize, 400, Description));
                XPosition += 400;
                _ = TableCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, 0, "R", TextFontSize, 400, Value + "  /-"));
            }
            YPosition += AddBlockSpace;
            return TableCode.ToString();
        }
        private void GetCharactersDictionaryCountWithFontTypeAndFontSize(int textFontSize, string fonttype)
        {
            ZebraHelper zebraHelper = new ZebraHelper();
            var characterCounts = zebraHelper.GetCharacterCountsForPageWidth(textFontSize, fonttype, ZebraPageWidth);
            UpperCaseCount = characterCounts[CharacterType.UpperCase];
            LowerCaseCount = characterCounts[CharacterType.LowerCase];
            NumericCaseCount = characterCounts[CharacterType.Numeric];
            SpecialCharacterCount = characterCounts[CharacterType.Special];
        }
        private double CalculateTextFieldLength(int fieldValueLowerCount, int fieldValueUpperCount, int fieldValueNumericCount, int fieldValueSpecialCount)
        {
            double LowerCount = fieldValueLowerCount * ZebraPageWidth / LowerCaseCount;
            double UpperCount = fieldValueUpperCount * ZebraPageWidth / UpperCaseCount;
            double NumberCount = fieldValueNumericCount * ZebraPageWidth / NumericCaseCount;
            double SpecialCount = fieldValueSpecialCount * ZebraPageWidth / SpecialCharacterCount;
            return LowerCount + UpperCount + NumberCount + SpecialCount;
        }


        #endregion

        /* ************************************************************************************************************************************************************************************************* */

        #region Honeywell Print

        //HoneyWell
        private int HoneywellPageWidth = HoneywellHelper.paperwidth;
        private int HoneywellPageHeight = HoneywellHelper.paperheight;
        private int HWxposition = 0;
        private int HWYposition = 0;
        private string cmd = "\u001B";
        private string HWPrimaryFontSize = "\u001B" + "W" + "0";
        private string HWHeaderFontSize = "\u001bW1";
        private string HWPrimaryFontStyle = "\u001B" + "E" + "0";
        private string HWBoldFontStyle = "\u001B" + "E" + "1";
        private int[] HWReceiptHeaderPositions = new int[] { };
        private int[] HWReceiptHeaderRatio = new int[] { };
        private int[] HWReceiptTableHeaderPositions = new int[] { };
        private int[] HWReceiptTableHeaderRatio = new int[] { };
        private int[] HWReceiptFooterPositions = new int[] { };
        private int[] HWReceiptFooterRatio = new int[] { };

        private string CreateSalesOrderPrintHoneywell(object data)
        {
            StringBuilder salesorder = new StringBuilder();
            if (AddressLines.Count != 0 && FooterLines.Count != 0 && salesOrderLinePrintViews != null)
            {
                string addressCard = GenerateAddressCardAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeader(ReceiptHeader);
                string receiptHeaderData = GenerateReceiptHeaderData(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderData(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyData(salesOrderLinePrintViews);
                string receiptAmountData = GenerateReceiptAmountData(AmountLines);
                string receiptFooterData = GenerateReceiptFooterData(HWFooterLines);
                string AddReceiptAlignmentForPage = AlignGapBetweenBodyAndFooter();

                salesorder.Append(addressCard);
                salesorder.Append(receiptTitleHeader);
                salesorder.Append(receiptHeaderData);
                salesorder.Append(receiptTableHeaderData);
                salesorder.Append(receiptTableBodyData);
                salesorder.Append(receiptAmountData);
                salesorder.Append(AddReceiptAlignmentForPage);
                salesorder.Append(receiptFooterData);
            }
            return salesorder.ToString();
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
            sb.Append(new HoneywellHelper().NextLine(2, ref HWYposition));
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
            HWReceiptHeaderPositions = new int[] { 1, 17, 19 };
            HWReceiptHeaderRatio = new int[] { 15, 1, 30 };
            sb.Append(HoneywellHelper.GenerateTextAtPosition(0, HWPrimaryFontSize, ""));
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            var processedList =  HoneywellHelper.ProcessHeaderLines(headerLines, HWReceiptHeaderRatio);
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = HWReceiptHeaderPositions[j] - xpp;
                    string value = processedList[i][j];
                    string generatedText = HoneywellHelper.GenerateTextAtPosition(xp, HWPrimaryFontSize, value);
                    sb.ToString();
                    xpp = HWReceiptHeaderPositions[j] + value.Length;
                    sb.Append(generatedText);
                }
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            }
            return sb.ToString();
        }
        private string GenerateReceiptTableHeaderData(List<(string p1, string p2, string p3, string p4, string p5, string p6, string p7)> hWBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            HWReceiptTableHeaderPositions = new int[] { 0, 5, 28, 34, 44, 56, 67 };
            HWReceiptTableHeaderRatio = new int[]     { 5, 20, 6, 6, 10, 8, 11 };
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
        private string GenerateReceiptTableBodyData(List<ISalesOrderLinePrintView> salesOrderLinePrintViews)
        {
            HWReceiptTableHeaderPositions = new int[] { 0, 5, 28, 34, 44, 56, 67 };
            HWReceiptTableHeaderRatio = new int[] { 5, 20, 6, 6, 10, 8, 11 };
            StringBuilder sb = new StringBuilder();
            foreach (ISalesOrderLinePrintView salesOrderLinePrintView in salesOrderLinePrintViews)
            {
                List<(string p1, string p2, string p3, string p4, string p5, string p6, string p7)> Values = new List<(string p1, string p2, string p3, string p4, string p5, string p6, string p7)>
                {
                    (salesOrderLinePrintView.LineNumber.ToString() ?? string.Empty,
                    salesOrderLinePrintView.SKUDescription ?? string.Empty,
                    salesOrderLinePrintView.UoM ?? string.Empty,
                    salesOrderLinePrintView.Qty.ToString() ?? string.Empty,
                    salesOrderLinePrintView.UnitPrice.ToString() ?? string.Empty,
                    salesOrderLinePrintView.TotalDiscount.ToString() ?? string.Empty,
                    salesOrderLinePrintView.TotalAmount.ToString() ?? string.Empty)
                };
                var processedList =  HoneywellHelper.ProcessHeaderLines(Values, HWReceiptTableHeaderRatio);
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
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            }
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptAmountData(List<(string description, string value)> amountLines)
        {
            StringBuilder sb =  new StringBuilder();
            for(int i = 0; i < amountLines.Count; i++)
            {
                string des = amountLines[i].description;
                string value = amountLines[i].value;
                int x1 = HWReceiptTableHeaderPositions[1];
                string generatedDes = HoneywellHelper.GenerateTextAtPosition(x1, HWPrimaryFontSize, des);
                int x2 = HWReceiptTableHeaderPositions[HWReceiptTableHeaderPositions.Length - 1] - des.Length - x1;
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
            HWReceiptFooterPositions = new int[] { 40, 57, 59 };
            HWReceiptFooterRatio = new int[] { 15, 1, 30 };
            var processedList = HoneywellHelper.ProcessHeaderLines(hWFooterLines, HWReceiptFooterRatio);
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = HWReceiptFooterPositions[j] - xpp;
                    string value = processedList[i][j];
                    string generatedText = HoneywellHelper.GenerateTextAtPosition(xp, HWPrimaryFontSize, value);
                    sb.ToString();
                    sb.Append(generatedText);
                    xpp = HWReceiptFooterPositions[j] + value.Length;
                }
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            }
            sb.Append(new HoneywellHelper().NextLine(3, ref HWYposition));
            return sb.ToString();
        }
        private string AlignGapBetweenBodyAndFooter()
        {
             
            return new HoneywellHelper().NextLine(HoneywellPageHeight - (HWYposition % HoneywellPageHeight ), ref HWYposition);
        }



        #endregion

        /* ************************************************************************************************************************************************************************************************* */

        #region  Woosim Print

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
        private string CreateSalesOrderPrintWoosim(object data)
        {
            StringBuilder salesOrderWoosimCode = new StringBuilder();
            if (AddressLines.Count != 0 && FooterLines.Count != 0 && salesOrderLinePrintViews != null)
            {
                
                string addressCard = GenerateAddressCardWoosimAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeaderWoosim(ReceiptHeader);
                string receiptHeaderData = GenerateReceiptHeaderDataWoosim(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderDataWoosim(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyDataWoosim(salesOrderLinePrintViews);
                string receiptAmountData = GenerateReceiptAmountDataWoosim(AmountLines);
                string receiptFooterData = GenerateReceiptFooterDataWoosim(HWFooterLines);

                salesOrderWoosimCode.Append(addressCard);
                salesOrderWoosimCode.Append(receiptTitleHeader);
                salesOrderWoosimCode.Append(receiptHeaderData);
                salesOrderWoosimCode.Append(receiptTableHeaderData);
                salesOrderWoosimCode.Append(receiptTableBodyData);
                salesOrderWoosimCode.Append(receiptAmountData);
                salesOrderWoosimCode.Append(receiptFooterData);

            }
            return salesOrderWoosimCode.ToString();
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
            sb.Append(new WoosimHelper().NextLine(2));
            Wxposition = default;
            return sb.ToString();
        }
        private string GenerateReceiptTitleHeaderWoosim(string receiptHeader)
        {
            Wxposition = (WoosimPageWidth - (2 * receiptHeader.Length)) ;
            StringBuilder sb = new StringBuilder();
            sb.Append(WoosimHelper.GenerateTextAtPosition(Wxposition, WHeadingFontSize, receiptHeader));
            sb.Append(new WoosimHelper().NextLine(2));
            Wxposition = default;
            return sb.ToString();
        }
        private string GenerateReceiptHeaderDataWoosim(List<(string description, string separator, string value)> WHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptHeaderPositions = new int[] { 1, 17, 19 };
            WReceiptHeaderRatio = new int[] { 15, 1, 30 };
            sb.Append(WoosimHelper.GenerateTextAtPosition(0, WPrimaryFontSize, ""));
            sb.Append(new WoosimHelper().NextLine(1));
            var processedList = HoneywellHelper.ProcessHeaderLines(WHeaderLines, WReceiptHeaderRatio);
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = WReceiptHeaderPositions[j] - xpp;
                    string value = processedList[i][j];
                    string generatedText = WoosimHelper.GenerateTextAtPosition(xp, WPrimaryFontSize, value);
                    sb.ToString();
                    xpp = WReceiptHeaderPositions[j] + value.Length;
                    sb.Append(generatedText);
                }
                sb.Append(new WoosimHelper().NextLine(1));
            }
            return sb.ToString();
        }
        private string GenerateReceiptTableHeaderDataWoosim(List<(string p1, string p2, string p3, string p4, string p5, string p6, string p7)> WBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptTableHeaderPositions = new int[] { 0, 5, 41, 50, 59, 70, 79 };
            WReceiptTableHeaderRatio = new int[] { 5, 35, 8, 8, 10, 8, 11 };
            var processedList = HoneywellHelper.ProcessHeaderLines(WBodyHeaderLines, WReceiptTableHeaderRatio);
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
        private string GenerateReceiptTableBodyDataWoosim(List<ISalesOrderLinePrintView> salesOrderLinePrintViews)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ISalesOrderLinePrintView salesOrderLinePrintView in salesOrderLinePrintViews)
            {
                List<(string p1, string p2, string p3, string p4, string p5, string p6, string p7)> Values = new List<(string p1, string p2, string p3, string p4, string p5, string p6, string p7)>
                {
                    (salesOrderLinePrintView.LineNumber.ToString() ?? string.Empty,
                    salesOrderLinePrintView.SKUDescription ?? string.Empty,
                    salesOrderLinePrintView.UoM ?? string.Empty,
                    salesOrderLinePrintView.Qty.ToString() ?? string.Empty,
                    salesOrderLinePrintView.UnitPrice.ToString() ?? string.Empty,
                    salesOrderLinePrintView.TotalDiscount.ToString() ?? string.Empty,
                    salesOrderLinePrintView.TotalAmount.ToString() ?? string.Empty)
                };
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
                sb.Append(new WoosimHelper().NextLine(1));
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
                string des = amountLines[i].description ?? string.Empty;
                string value = amountLines[i].value ?? string.Empty;
                int x1 = WReceiptTableHeaderPositions[1];
                string generatedDes = WoosimHelper.GenerateTextAtPosition(x1, WPrimaryFontSize, des);
                int x2 = WReceiptTableHeaderPositions[WReceiptTableHeaderPositions.Length - 1] - des.Length - x1;
                string generatedValue = WoosimHelper.GenerateTextAtPosition((x2), WPrimaryFontSize, value);
                sb.Append(generatedDes);
                sb.Append(generatedValue);
                sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptFooterDataWoosim(List<(string description, string separator, string value)> WFooterLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptFooterPositions = new int[] { 40, 57, 59 };
            WReceiptFooterRatio = new int[] { 15, 1, 30 };
            var processedList = HoneywellHelper.ProcessHeaderLines(WFooterLines, WReceiptFooterRatio);
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = WReceiptFooterPositions[j] - xpp;
                    string value = processedList[i][j];
                    string generatedText = WoosimHelper.GenerateTextAtPosition(xp, WPrimaryFontSize, value);
                    sb.ToString();
                    sb.Append(generatedText);
                    xpp = WReceiptFooterPositions[j] + value.Length;
                }
                sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().NextLine(3));
            return sb.ToString();
        }

        #endregion

        /* ************************************************************************************************************************************************************************************************* */
    }
}

using DocumentFormat.OpenXml.Office.CustomUI;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Printing.BL.Classes.HelperClasses;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using WinIt.Models.Customers;

namespace Winit.Modules.Printing.BL.Classes.ReturnOrder
{
    public class ReturnOrderPrint : BasePrint
    {
        
        // Lists
        private List<(string description, string value)> HeaderLines = new() { };
        private List<string> AddressLines = new() { };
        private List<(string description, string value)> AmountLines = new();
        private List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine> returnOrderitems = new List<IReturnOrderLine> { };
        private string ReceiptHeader = "Return Order";
        private string price ;

        //For HoneyWell
        private List<(string description, string separator, string value)> HWHeaderLines = new() { };
        private List<(string description, string separator, string value)> HWFooterLines = new() { };
        private List<(string p1, string p2, string p3, string p4, string p5, string p6)> HWBodyHeaderLines;

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
        private readonly int AddressCard1TextFontSize = 20;
        private readonly int AddressCard2TextFontSize = 17;
        private readonly int AddBlockSpace = 50;
        private readonly int LineThickness = 5;
        private int UpperCaseCount;
        private int LowerCaseCount;
        private int NumericCaseCount;
        private int SpecialCharacterCount;
        private string? currency;
        private string? OrderDate;
        private string? amount ;
        private List<string> TableHeaders = new() { };
        private List<string> TableReason = new() { };
        private int[]? TableHeadersRatio;
        private int[]? TableReasonRatio;
        private int ReasonTextstartPosition = 67;

        /* ************************************************************************************************************************************************************************************************* */


        
        public override string CreatePrintString(PrinterType printerType,PrinterSize printerSize, object data)
        {
            if (data is (IReturnOrderMaster ReturnOrderData , IStoreItemView SelectedStoreDetails))
            {
                returnOrderitems = ReturnOrderData.ReturnOrderLineList;

                AddressLines = new HoneywellHelper().AddressLines;
                HeaderLines = new List<(string, string)>
                {
                    ("Store Name", SelectedStoreDetails.Name ?? string.Empty),
                    ("Store Code", SelectedStoreDetails.StoreCode ?? string.Empty),
                    ("Address ", SelectedStoreDetails.Address ?? string.Empty),
                    ("Order-Type", ReturnOrderData.ReturnOrder.OrderType ?? string.Empty),
                    ("Return Date", CommonFunctions.GetDateTimeInFormat(SelectedStoreDetails.CreatedTime) ?? string.Empty),
                    ("User", (ReturnOrderData.ReturnOrder.CreatedBy +"["+ ReturnOrderData.ReturnOrder.EmpUID+"]") ?? string.Empty),
                    ("Route",ReturnOrderData.ReturnOrder.RouteUID ?? string.Empty)
                };
                HWHeaderLines = new List<(string, string, string)>
                {
                    ("Store Name", ":",SelectedStoreDetails.Name ?? string.Empty),
                    ("Store Code", ":", SelectedStoreDetails.StoreCode ?? string.Empty),
                    ("Address ", ":", SelectedStoreDetails.Address ?? string.Empty),
                    ("Order-Type", ":", ReturnOrderData.ReturnOrder.OrderType ?? string.Empty),
                    ("Return Date", ":", CommonFunctions.GetDateTimeInFormat(SelectedStoreDetails.CreatedTime) ?? string.Empty),
                    ("User", ":", (ReturnOrderData.ReturnOrder.CreatedBy +"["+ ReturnOrderData.ReturnOrder.EmpUID+"]") ?? string.Empty),
                    ("Route", ":",ReturnOrderData.ReturnOrder.RouteUID ?? string.Empty)
                };
                currency = ReturnOrderData.ReturnOrder.CurrencyUID;
                amount = currency + " " + (ReturnOrderData.ReturnOrder.TotalAmount.ToString());
                TableHeaders = new List<string> { "SNo", "PO Number ", "UOM", "Type", "QTY", ("Price" + "(" + currency + ")")};
                price = "Price" + "(" + currency + ")";
                TableReason = new List<string> { "Reason" };
                HWBodyHeaderLines = new() { ("SNo", "PO Number ", "UOM", "Type", "QTY", ("Price" + "(" + currency + ")")) };
                TableHeadersRatio = new int[] { 67, 264, 67, 134, 134, 134 };
                TableReasonRatio = new int[] { 600, };
                AmountLines = new List<(string description, string value)>
                {
                    ("Total Amount",amount )
                };
                HWFooterLines = new List<(string, string, string)>
                {
                    ("Total Amount",":",amount )
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
                PrinterType.Honeywell => CreateReturnOrderPrintHoneywell(data),
                PrinterType.Woosim => CreateSalesOrderPrintWoosim(data),
                _ => CreateSalesOrderPrintZebra(data),// Default to Zebra if the printer type is not specified
            };
            return retValue;
        }

        

        /* ************************************************************************************************************************************************************************************************* */

        #region Zebra
        private string CreateSalesOrderPrintZebra(object data)
        {
            if (AddressLines.Count != 0  && returnOrderitems.Count != null)
            {
                StringBuilder salesOrderZplCode = new ();
                _ = salesOrderZplCode.Append(ZebraHelper.GetLogo(XPosition, YPosition));
                _ = salesOrderZplCode.Append(GenerateAddressCard(AddressLines, NormalFontypeSizeType));
                _ = salesOrderZplCode.Append(GenerateZebraTitleCard("Return ", "Order", Header1TextFontSize, CentreAlign));
                _ = salesOrderZplCode.Append(GenerateZebraHeaderCode(HeaderLines, NormalFontypeSizeType));
                _ = salesOrderZplCode.Append(GenerateZebraBodyCode(returnOrderitems, NormalFontypeSizeType));
                salesOrderZplCode.Insert(0, ZebraHelper.SetLabelLength(YPosition));
                return salesOrderZplCode.ToString();
            }
            return string.Empty;
        }
        private string GenerateAddressCard(List<string> addressLines, string Font)
        {
            StringBuilder AddressCardCode = new StringBuilder();

            AddressCardCode.Append(ZebraHelper.GenerateTitleCardwithAlignment(AddressLinesXPosition, YPosition, LeftAlign, AddressCard1TextFontSize, addressLines[0], AddressCard1TextFontSize, Font));
            YPosition += AddressCard1TextFontSize;
            for (int i = 1; i < addressLines.Count; i++)
            {
                AddressCardCode.Append(ZebraHelper.GenerateTitleCardwithAlignment(AddressLinesXPosition, YPosition, LeftAlign, AddressCard2TextFontSize, addressLines[i], AddressCard2TextFontSize, Font));
                YPosition += AddressCard2TextFontSize;
            }
            YPosition += AddBlockSpace;
            return AddressCardCode.ToString();
        }
        private string GenerateZebraTitleCard(string v1, string v2, int Titlefontsize, string Alignment)
        {
            StringBuilder TitleCode = new StringBuilder();
            int linesneededForItem = (int)Math.Ceiling((double)(v1.Length + v2.Length) / 51);
            string line = $"{ZebraHelper.GenerateTitleCard(0, YPosition, Alignment, Titlefontsize, (v1 + v2), Header1TextFontSize)}";
            YPosition += linesneededForItem * 3 * Titlefontsize / 2;
            TitleCode.Append(line);
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
        private string GenerateZebraBodyCode(List<IReturnOrderLine> returnOrderLineitems, string normalFontypeSizeType)
        {
            StringBuilder HeaderCode = new();
            _ = HeaderCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition += (2 * LineThickness);
            GetCharactersDictionaryCountWithFontTypeAndFontSize(TextFontSize, normalFontypeSizeType);
            int Bodylines = 1;
            int index = 0;
            foreach (string headerValue in TableHeaders)
            {
                string fieldName = headerValue;
                Dictionary<string, int> fieldNameCounts = CalculateCharacterCounts(fieldName);
                int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                int fieldNameNumericCount = fieldNameCounts["Numeric"];
                int fieldNameSpecialCount = fieldNameCounts["Special"];
                double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);
                int linesNeeded = (int)Math.Ceiling((double)fieldNamelength / TableHeadersRatio[index]);
                _ = HeaderCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesNeeded, LeftAlign, TextFontSize, TableHeadersRatio[index], headerValue));
                XPosition += TableHeadersRatio[index];
                index++;
                if (linesNeeded > Bodylines)
                {
                    Bodylines = linesNeeded;
                }
            }
            YPosition += Bodylines * TextFontSize;
            XPosition = ReasonTextstartPosition;
            index = default;
            foreach (string headerValue in TableReason)
            {
                string fieldName = headerValue;
                Dictionary<string, int> fieldNameCounts = CalculateCharacterCounts(fieldName);
                int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                int fieldNameNumericCount = fieldNameCounts["Numeric"];
                int fieldNameSpecialCount = fieldNameCounts["Special"];
                double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);
                int linesNeeded = (int)Math.Ceiling((double)fieldName.Length / TableReasonRatio[index]);
                _ = HeaderCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesNeeded, LeftAlign, TextFontSize, TableReasonRatio[index], headerValue));
                XPosition += TableReasonRatio[index];
                index++;
                if (linesNeeded > Bodylines)
                {
                    Bodylines = linesNeeded;
                }
            }
            YPosition += Bodylines * TextFontSize;
            XPosition = default;
            _ = HeaderCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition += (2 * LineThickness);
            int linenumber = 1;
            foreach (IReturnOrderLine returnOrderLineitem in returnOrderLineitems)
            {
                List<string> Values = new()
                {
                    linenumber.ToString() ?? string.Empty,
                    returnOrderLineitem.PONumber ?? string.Empty,
                    returnOrderLineitem.UoM ?? string.Empty,
                    returnOrderLineitem.SKUType ?? string.Empty,
                    returnOrderLineitem.Qty.ToString() ?? string.Empty,
                    returnOrderLineitem.NetAmount.ToString() ?? string.Empty
                };
                linenumber++;
                List<string> Reasons = new()
                {
                    returnOrderLineitem.ReasonText ?? string.Empty,

                };
                Bodylines = 1;
                int i = 0;
                foreach (string Tablevalue in Values)
                {
                    string alignment = ( i == 5) ? rightAlign : LeftAlign;
                    string fieldName = Tablevalue;
                    Dictionary<string, int> fieldNameCounts = CalculateCharacterCounts(fieldName);
                    int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                    int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                    int fieldNameNumericCount = fieldNameCounts["Numeric"];
                    int fieldNameSpecialCount = fieldNameCounts["Special"];
                    double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);
                    int linesNeeded = (int)Math.Ceiling((double)fieldNamelength / TableHeadersRatio[i]);
                    _ = HeaderCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesNeeded, alignment, TextFontSize, TableHeadersRatio[i], Tablevalue));
                    XPosition += TableHeadersRatio[i];
                    if (linesNeeded > Bodylines)
                    {
                        Bodylines = linesNeeded;
                    }
                    i++;
                }
                YPosition += Bodylines * TextFontSize;
                XPosition = ReasonTextstartPosition;
                i = 0;
                Bodylines = 1;
                foreach (string Tablevalue in Reasons)
                {
                    string fieldName = Tablevalue;
                    Dictionary<string, int> fieldNameCounts = CalculateCharacterCounts(fieldName);
                    int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                    int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                    int fieldNameNumericCount = fieldNameCounts["Numeric"];
                    int fieldNameSpecialCount = fieldNameCounts["Special"];
                    double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);
                    int linesNeeded = (int)Math.Ceiling((double)fieldNamelength / TableReasonRatio[i]);
                    _ = HeaderCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesNeeded, LeftAlign, TextFontSize, TableReasonRatio[i], Tablevalue));
                    XPosition += TableReasonRatio[i];
                    if (linesNeeded > Bodylines)
                    {
                        Bodylines = linesNeeded;
                    }
                    i++;
                }
                YPosition += Bodylines * TextFontSize;
                XPosition = default;
            }
            _ = HeaderCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition += (2 * LineThickness);

            for (int i = 0; i < AmountLines.Count; i++)
            {
                string Value = AmountLines[i].value;
                string Description = AmountLines[i].description;
                HeaderCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, 0, LeftAlign, TextFontSize, 400, Description));
                XPosition += 400;
                HeaderCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, 0, rightAlign, TextFontSize, 400, Value  ));
            }
            YPosition += AddBlockSpace;
            XPosition = default;
            return HeaderCode.ToString();
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
        
        #region Honeywell
        private int HoneywellPageWidth = HoneywellHelper.paperwidth;
        private int HoneywellPageHeight = HoneywellHelper.paperheight;
        private int HWxposition = 0;
        private int HWYposition = 0;
        private string HWPrimaryFontSize = "\u001B" + "W" + "0";
        private string HWHeaderFontSize = "\u001B" + "W" + "1";
        private string HWPrimaryFontStyle = "\u001B" + "E" + "0";
        private string HWBoldFontStyle = "\u001B" + "E" + "1";
        private int[] HWReceiptHeaderPositions = new int[] {};
        private int[] HWReceiptHeaderRatio = new int[] {};
        private int[] HWReceiptTableHeaderPositions = new int[] { };
        private int[] HWReceiptTableHeaderRatio = new int[] { };
        
        private string CreateReturnOrderPrintHoneywell(object data)
        {
            StringBuilder returnorder = new StringBuilder();
            if (returnOrderitems != null || HeaderLines != null || AddressLines != null)
            {
                string addressCard = GenerateAddressCardAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeader(ReceiptHeader);
                string receiptHeaderData = GenerateReceiptHeaderData(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderData(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyData(returnOrderitems);
                string receiptAmountData = GenerateReceiptAmountData(AmountLines);
                string AddReceiptAlignmentForPage = AlignGapBetweenBodyAndFooter();

                returnorder.Append(addressCard);
                returnorder.Append(receiptTitleHeader);
                returnorder.Append(receiptHeaderData);
                returnorder.Append(receiptTableHeaderData);
                returnorder.Append(receiptTableBodyData);
                returnorder.Append(receiptAmountData);
                returnorder.Append(AddReceiptAlignmentForPage);
            }
            return returnorder.ToString();
        }
        private string GenerateAddressCardAtTop(List<string> addressLines)
        {
            HWxposition= HoneywellPageWidth - new HoneywellHelper().GetMaxLength(addressLines);
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
        private string GenerateReceiptTableHeaderData(List<(string p1, string p2, string p3, string p4, string p5, string p6)> hWBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            HWReceiptTableHeaderPositions = new int[] { 0, 5, 25, 32, 48, 59 };
            HWReceiptTableHeaderRatio = new int[] { 5, 20, 5, 15, 10, 10 };
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
            sb.Append(HoneywellHelper.GenerateTextAtPosition(5, HWPrimaryFontSize, "Reason"));
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptTableBodyData(List<IReturnOrderLine>? returnOrderLineitems)
        {
            StringBuilder sb = new StringBuilder();
            int linenumber = 1;
            foreach (IReturnOrderLine returnOrderLineitem in returnOrderLineitems)
            {
                List<(string p1, string p2, string p3, string p4, string p5, string p6)> Values = new List<(string p1, string p2, string p3, string p4, string p5, string p6)>
                {
                    (linenumber.ToString() ?? string.Empty,
                    returnOrderLineitem.PONumber ?? string.Empty,
                    returnOrderLineitem.UoM ?? string.Empty,
                    returnOrderLineitem.SKUType ?? string.Empty,
                    returnOrderLineitem.Qty.ToString() ?? string.Empty,
                    returnOrderLineitem.NetAmount.ToString() ?? string.Empty)
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
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
                string reason = returnOrderLineitem.ReasonText ?? string.Empty ;
                string generatedreason = HoneywellHelper.GenerateTextAtPosition(5, HWPrimaryFontSize, reason);
                sb.Append(generatedreason);
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
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            for (int i=0 ; i< hWFooterLines.Count ; i++)
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
                    string generatedText = HoneywellHelper.GenerateTextAtPosition((x2-x1), HWPrimaryFontSize, value);
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
        #region    Woosim

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
            StringBuilder returnorder = new StringBuilder();
            if (returnOrderitems != null || HeaderLines != null || AddressLines != null)
            {
                string addressCard = GenerateAddressCardWoosimAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeaderWoosim(ReceiptHeader);
                string receiptHeaderData = GenerateReceiptHeaderDataWoosim(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderDataWoosim(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyDataWoosim(returnOrderitems);
                string receiptAmountData = GenerateReceiptAmountDataWoosim(AmountLines);

                returnorder.Append(addressCard);
                returnorder.Append(receiptTitleHeader);
                returnorder.Append(receiptHeaderData);
                returnorder.Append(receiptTableHeaderData);
                returnorder.Append(receiptTableBodyData);
                returnorder.Append(receiptAmountData);
            }
            return returnorder.ToString();
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
            sb.Append(new WoosimHelper().NextLine(3));
            HWxposition = default;
            return sb.ToString();
        }
        private string GenerateReceiptTitleHeaderWoosim(string receiptHeader)
        {
            Wxposition = (WoosimPageWidth - (2 * receiptHeader.Length)) / 2;
            StringBuilder sb = new StringBuilder();
            sb.Append(WoosimHelper.GenerateTextAtPosition(Wxposition, WHeadingFontSize, receiptHeader));
            sb.Append(new WoosimHelper().NextLine(2));
            Wxposition = default;
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
        private string GenerateReceiptTableHeaderDataWoosim(List<(string p1, string p2, string p3, string p4, string p5, string p6)> hWBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptTableHeaderPositions = new int[] { 0, 5, 25, 32, 48, 59 };
            WReceiptTableHeaderRatio = new int[] { 5, 20, 5, 15, 10, 10 };
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
            sb.Append(WoosimHelper.GenerateTextAtPosition(5, WPrimaryFontSize, "Reason"));
            sb.Append(new WoosimHelper().NextLine(1));
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptTableBodyDataWoosim(List<IReturnOrderLine>? returnOrderLineitems)
        {
            StringBuilder sb = new StringBuilder();
            int linenumber = 1;
            foreach (IReturnOrderLine returnOrderLineitem in returnOrderLineitems)
            {
                List<(string p1, string p2, string p3, string p4, string p5, string p6)> Values = new List<(string p1, string p2, string p3, string p4, string p5, string p6)>
                {
                    (linenumber.ToString() ?? string.Empty,
                    returnOrderLineitem.PONumber ?? string.Empty,
                    returnOrderLineitem.UoM ?? string.Empty,
                    returnOrderLineitem.SKUType ?? string.Empty,
                    returnOrderLineitem.Qty.ToString() ?? string.Empty,
                    returnOrderLineitem.NetAmount.ToString() ?? string.Empty)
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
                }
                sb.Append(new WoosimHelper().NextLine(1));
                string reason = returnOrderLineitem.ReasonText ?? string.Empty;
                string generatedreason = WoosimHelper.GenerateTextAtPosition(5, WPrimaryFontSize, reason);
                sb.Append(generatedreason);
                sb.Append(new WoosimHelper().NextLine(2));
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
                string generatedValue = WoosimHelper.GenerateTextAtPosition((x2), WPrimaryFontSize, value);
                sb.Append(generatedDes);
                sb.Append(generatedValue);
                sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptFooterDataWoosim(List<(string description, string separator, string value)> hWFooterLines)
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
        

        #endregion


        /* ************************************************************************************************************************************************************************************************* */


    }
}

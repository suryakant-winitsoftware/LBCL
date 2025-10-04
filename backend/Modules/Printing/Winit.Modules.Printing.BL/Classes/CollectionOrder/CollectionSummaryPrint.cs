using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Printing.BL.Classes.HelperClasses;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.Printing.BL.Classes.CollectionOrder
{
    public class CollectionSummaryPrint : BasePrint
    {
        // Lists
        private List<(string description, string value )> HeaderLines = new() { };
        private List<string> AddressLines = new() { };
        private List<(string description, string value, string value2, string value3)> BodyLines = new() { };
        private List<(string description, string value)> ReasonLines = new() { };
        private List<(string description, string value)> FooterLines = new() { };
        private List<(string description, string value)> AmountLines = new();
        private List<IPaymentSummary> salesOrderLinePrintViews = new();

        //For HoneyWell
        private List<(string description, string separator, string value)> HWHeaderLines = new() { };
        private List<(string description, string separator, string value)> HWFooterLines = new() { };
        private List<(string p1, string p2, string p3, string p4)> HWBodyHeaderLines;
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
        private readonly int AddressCard2TextFontSize = 16;
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
        private string? cashTotalAmount;
        private string? chequeTotalAmount;
        private string? pOSTotalAmount;
        private string? onlineTotalAmount;
        private string? TotalAmount;


        /* ************************************************************************************************************************************************************************************************* */



        public override string CreatePrintString(PrinterType printerType, PrinterSize printerSize, object data)
        {
            TableHeaders = new List<string> {"Receipt Number","Store Name", "Mode", "Amount" };
            HWBodyHeaderLines = new() { ("Receipt Number", "Store Name", "Mode", "Amount") };
            TableHeadersRatio = new int[] { 320,160,170,200 };
            if (data is (List<IPaymentSummary> collectionOrderSummaryPrintView , decimal CashTotalAmount, decimal ChequeTotalAmount, decimal POSTotalAmount, decimal OnlineTotalAmount))
            {
                cashTotalAmount = CashTotalAmount.ToString();
                chequeTotalAmount = ChequeTotalAmount.ToString();
                pOSTotalAmount = POSTotalAmount.ToString();
                onlineTotalAmount = OnlineTotalAmount.ToString();
                TotalAmount= (CashTotalAmount + ChequeTotalAmount + POSTotalAmount + OnlineTotalAmount).ToString();
                AddressLines = new List<string>
                {
                    "Win Information Technology Pvt Ltd, ",
                    "Level-16, Block A, ",
                    "Sky-1, " ,
                    "Prestige Towers, ",
                    "Hyderabad."
                };

                HeaderLines = new List<(string description, string value)>
                    {
                        ("Salesman Name","Van User2"),
                        ("Salesman Code", "VanUser2"),
                         ("Print Time", CommonFunctions.GetDateTimeInFormat(DateTime.Now).ToString())
                    };
                HWHeaderLines = new List<(string, string, string)>
                {
                    ("Salesman Name",":","Van User2"),
                        ("Salesman Code",":", "VanUser2"),
                        ("Print Time",":", CommonFunctions.GetDateTimeInFormat(DateTime.Now).ToString())
                };
                FooterLines = new List<(string description, string value)>
                    {
                        ("Cash Total Amount",cashTotalAmount),
                        ("Cheque Total Amount", chequeTotalAmount),
                        ("POS Total Amount",pOSTotalAmount),
                        ("Online Total Amount", onlineTotalAmount),
                        ("TotalAmount",TotalAmount)

                    };
                HWFooterLines = new List<(string, string, string)>
                {
                        ("Cash Total Amount",":",cashTotalAmount),
                        ("Cheque Total Amount",":", chequeTotalAmount),
                        ("POS Total Amount",":",pOSTotalAmount),
                        ("Online Total Amount",":", onlineTotalAmount),
                        ("Total Amount",":",TotalAmount)
                };

                foreach (var item in collectionOrderSummaryPrintView)
                {
                    string store = "";
                    if (item.StoreName != null && item.StoreCode != null)
                    {
                        store = "["+ item.StoreCode +"] "+ item.StoreName ;
                    }
                    else
                    {
                        store = (item.StoreName + item.StoreCode) ?? string.Empty ;
                    }
                    
                    BodyLines.Add((item.ReceiptNumber ?? string.Empty, store, item.Category ?? string.Empty, item.Amount.ToString()));
                }

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
                PrinterType.Honeywell => CreateSCollectionSummaryPrintHoneywell(data),
                PrinterType.Woosim => CreateSalesOrderPrintWoosim(data),
                _ => CreateSalesOrderPrintZebra(data),// Default to Zebra if the printer type is not specified
            };
            return retValue;
        }

        /* ************************************************************************************************************************************************************************************************* */
        
        #region Zebra
        private string CreateSalesOrderPrintZebra(object data)
        {
            if (BodyLines.Count != 0)
            {
                StringBuilder salesOrderZplCode = new();
                _ = salesOrderZplCode.Append(ZebraHelper.GetLogo(XPosition, YPosition));
                _ = salesOrderZplCode.Append(GenerateAddressCard(AddressLines, NormalFontypeSizeType));
                _ = salesOrderZplCode.Append(GenerateZebraTitleCard("Collection ", " Summary", Header1TextFontSize, CentreAlign));
                _ = salesOrderZplCode.Append(GenerateZebraHeaderCode(HeaderLines, NormalFontypeSizeType));
                _ = salesOrderZplCode.Append(GenerateZebraBodyCode(BodyLines , NormalFontypeSizeType));
                _ = salesOrderZplCode.Append(GenerateZebraHeaderCode(FooterLines, NormalFontypeSizeType));
                _ = salesOrderZplCode.Insert(0, ZebraHelper.SetLabelLength(YPosition));
                return salesOrderZplCode.ToString();
            }
            return string.Empty;
        }
        private string GenerateZebraHeaderCode(List<(string description, string value)> headerLines, string fonttype)
        {
            StringBuilder HeaderCode = new StringBuilder();
            int fieldNameWidth = 250;
            int middlePartWidth = 20;
            int fieldValueWidth = 500;
            int maxLinesNeeded = 1;
            GetCharactersDictionaryCountWithFontTypeAndFontSize(TextFontSize, fonttype);
            for (int i = 0; i < headerLines.Count(); i++)
            {
                string fieldName = headerLines[i].Item1;
                var fieldNameCounts = CalculateCharacterCounts(fieldName);
                int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                int fieldNameNumericCount = fieldNameCounts["Numeric"];
                int fieldNameSpecialCount = fieldNameCounts["Special"];
                double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);

                string fieldValue = headerLines[i].Item2;
                var fieldValueCounts = CalculateCharacterCounts(fieldValue);
                int fieldValueLowerCount = fieldValueCounts["LowerCase"];
                int fieldValueUpperCount = fieldValueCounts["UpperCase"];
                int fieldValueNumericCount = fieldValueCounts["Numeric"];
                int fieldValueSpecialCount = fieldValueCounts["Special"];
                double fieldValuelength = CalculateTextFieldLength(fieldValueLowerCount, fieldValueUpperCount, fieldValueNumericCount, fieldValueSpecialCount);

                double linesneededForItem1 = (double)fieldNamelength / fieldNameWidth;
                double linesneededForItem2 = (double)fieldValuelength / fieldValueWidth;

                int linesNeededForItem1 = (int)Math.Ceiling(linesneededForItem1);
                int linesNeededForItem2 = (int)Math.Ceiling(linesneededForItem2);

                int linesNeeded = Math.Max(linesNeededForItem1, linesNeededForItem2);

                string line = /*$"{ZebraHelper.GenerateFieldOrigin(XPosition, YPosition + TextFontSize)}" +*/
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition, YPosition, maxLinesNeeded, LeftAlign, fonttype, TextFontSize, fieldNameWidth, fieldName)}" +
                              //$"{ZebraHelper.GenerateFieldOrigin(XPosition + fieldNameWidth, YPosition + TextFontSize)}" +
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition + fieldNameWidth, YPosition, maxLinesNeeded, CentreAlign, fonttype, TextFontSize, middlePartWidth, Separator)}" +
                              //$"{ZebraHelper.GenerateFieldOrigin(XPosition + fieldNameWidth + middlePartWidth, YPosition + TextFontSize)}" +
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition + fieldNameWidth + middlePartWidth, YPosition, maxLinesNeeded, LeftAlign, fonttype, TextFontSize, fieldValueWidth, fieldValue)}";
                HeaderCode.Append(line);
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
        private string GenerateZebraTitleCard(string v1, string v2, int Titlefontsize, string Alignment)
        {
            StringBuilder TitleCode = new StringBuilder();
            int linesneededForItem = (int)Math.Ceiling((double)(v1.Length + v2.Length) / 51);
            string line = $"{ZebraHelper.GenerateTitleCard(0, YPosition, Alignment, Titlefontsize, (v1 + v2), Header1TextFontSize)}";
            YPosition += linesneededForItem * 3 * Titlefontsize / 2;
            TitleCode.Append(line);
            return TitleCode.ToString();
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
        private string GenerateZebraBodyCode(List<(string description, string value ,string value2 ,string value3)> BodyLines, string fonttype)
        {
            StringBuilder HeaderCode = new();
            _ = HeaderCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition += (2 * LineThickness);
            GetCharactersDictionaryCountWithFontTypeAndFontSize(TextFontSize, fonttype);
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
                int linesNeeded = (int)Math.Ceiling((double)fieldName.Length / TableHeadersRatio[index]);
                _ = HeaderCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesNeeded, "L", TextFontSize, TableHeadersRatio[index], headerValue));
                XPosition += TableHeadersRatio[index];
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
            int fieldNameWidth = 250;
            int fieldValueWidth = 250;
            int fieldValue1Width = 180;
            int fieldValue2Width = 120;
            int maxLinesNeeded = 1;
            GetCharactersDictionaryCountWithFontTypeAndFontSize(TextFontSize, fonttype);
            for (int i = 0; i < BodyLines.Count(); i++)
            {
                string fieldName = BodyLines[i].description;
                Dictionary<string, int> fieldNameCounts = CalculateCharacterCounts(fieldName);
                int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                int fieldNameNumericCount = fieldNameCounts["Numeric"];
                int fieldNameSpecialCount = fieldNameCounts["Special"];
                double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);

                string fieldValue = BodyLines[i].value;
                Dictionary<string, int> fieldValueCounts = CalculateCharacterCounts(fieldValue);
                int fieldValueLowerCount = fieldValueCounts["LowerCase"];
                int fieldValueUpperCount = fieldValueCounts["UpperCase"];
                int fieldValueNumericCount = fieldValueCounts["Numeric"];
                int fieldValueSpecialCount = fieldValueCounts["Special"];
                double fieldValuelength = CalculateTextFieldLength(fieldValueLowerCount, fieldValueUpperCount, fieldValueNumericCount, fieldValueSpecialCount);

                string fieldValue1 = BodyLines[i].value2;
                Dictionary<string, int> fieldValue1Counts = CalculateCharacterCounts(fieldValue);
                int fieldValue1LowerCount = fieldValueCounts["LowerCase"];
                int fieldValue1UpperCount = fieldValueCounts["UpperCase"];
                int fieldValue1NumericCount = fieldValueCounts["Numeric"];
                int fieldValue1SpecialCount = fieldValueCounts["Special"];
                double fieldValue1length = CalculateTextFieldLength(fieldValue1LowerCount, fieldValue1UpperCount, fieldValue1NumericCount, fieldValue1SpecialCount);


                string fieldValue2 = BodyLines[i].value3;
                Dictionary<string, int> fieldValue2Counts = CalculateCharacterCounts(fieldValue);
                int fieldValue2LowerCount = fieldValueCounts["LowerCase"];
                int fieldValue2UpperCount = fieldValueCounts["UpperCase"];
                int fieldValue2NumericCount = fieldValueCounts["Numeric"];
                int fieldValue2SpecialCount = fieldValueCounts["Special"];
                double fieldValue2length = CalculateTextFieldLength(fieldValue2LowerCount, fieldValue2UpperCount, fieldValue2NumericCount, fieldValue2SpecialCount);

                double linesneededForItem1 = (double)fieldNamelength / fieldNameWidth;
                double linesneededForItem2 = (double)fieldValuelength / fieldValueWidth;
                double linesneededForItem3 = (double)fieldValue1length / fieldValue1Width;
                double linesneededForItem4 = (double)fieldValue2length / fieldValue1Width;

                int linesNeededForItem1 = (int)Math.Ceiling(linesneededForItem1);
                int linesNeededForItem2 = (int)Math.Ceiling(linesneededForItem2);
                int linesNeededForItem3 = (int)Math.Ceiling(linesneededForItem3);
                int linesNeededForItem4 = (int)Math.Ceiling(linesneededForItem4);

                int linesNeeded = Math.Max(Math.Max(linesNeededForItem1, linesNeededForItem2), Math.Max(linesNeededForItem3, linesNeededForItem4));


                string line = $"{ZebraHelper.GenerateFieldOrigin(XPosition, YPosition + TextFontSize)}" +
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition, YPosition, maxLinesNeeded, LeftAlign, NormalFontypeSizeType, TextFontSize, fieldNameWidth, fieldName)}" +
                              $"{ZebraHelper.GenerateFieldOrigin(XPosition + fieldNameWidth, YPosition + TextFontSize)}" +
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition + fieldNameWidth, YPosition, maxLinesNeeded, CentreAlign, NormalFontypeSizeType, TextFontSize, fieldValueWidth, fieldValue)}" +
                              $"{ZebraHelper.GenerateFieldOrigin(XPosition + fieldNameWidth + fieldValueWidth, YPosition + TextFontSize)}" +
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition + fieldNameWidth + fieldValueWidth, YPosition, maxLinesNeeded, LeftAlign, NormalFontypeSizeType, TextFontSize, fieldValueWidth, fieldValue1)}"+
                              $"{ZebraHelper.GenerateFieldOrigin(XPosition + fieldNameWidth + fieldValueWidth + fieldValue1Width, YPosition + TextFontSize)}" +
                              $"{ZebraHelper.GenerateFieldNameCellData(XPosition + fieldNameWidth + fieldValueWidth + fieldValue1Width, YPosition, maxLinesNeeded, LeftAlign, NormalFontypeSizeType, TextFontSize, fieldValueWidth, fieldValue2)}";

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
            _ = HeaderCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition += 2 * TextFontSize;
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
        //HoneyWell

        #region Honeywell


        private int HoneywellPageWidth = HoneywellHelper.paperwidth;
        private int HoneywellPageHeight = HoneywellHelper.paperheight;
        private int HWxposition = 0;
        private int HWYposition = 0;
        private string ReceiptHeader = "Collection Summary";
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
        
        private string CreateSCollectionSummaryPrintHoneywell(object data)
        {
            StringBuilder returnorder = new StringBuilder();
            if (BodyLines.Count != 0)
            {
                string addressCard = GenerateAddressCardAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeader(ReceiptHeader);
                string receiptHeaderData = GenerateReceiptHeaderData(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderData(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyData(BodyLines);
                string receiptFooterData = GenerateReceiptFooterData(HWFooterLines);
                string AddReceiptAlignmentForPage = AlignGapBetweenBodyAndFooter();

                returnorder.Append(addressCard);
                returnorder.Append(receiptTitleHeader);
                returnorder.Append(receiptHeaderData);
                returnorder.Append(receiptTableHeaderData);
                returnorder.Append(receiptTableBodyData);
                returnorder.Append(AddReceiptAlignmentForPage);
                returnorder.Append(receiptFooterData);
            }
            return returnorder.ToString();
        }

        private string GenerateAddressCardAtTop(List<string> addressLines)
        {
            HWxposition = HoneywellPageWidth - new HoneywellHelper().GetMaxLength(addressLines) ;
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
            HWReceiptHeaderPositions = new int[] { 0, 17, 19 };
            HWReceiptHeaderRatio = new int[] { 15, 1, 30 };
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
        private string GenerateReceiptTableHeaderData(List<(string p1, string p2, string p3, string p4)> hWBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            HWReceiptTableHeaderPositions = new int[] { 0, 22, 52, 62 };
            HWReceiptTableHeaderRatio = new int[] { 20, 30, 10, 8 };
            var processedList = HoneywellHelper.ProcessHeaderLines(hWBodyHeaderLines, HWReceiptTableHeaderRatio);
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = HWReceiptTableHeaderPositions[j] - xpp ;
                    string value = processedList[i][j];
                    string generatedText = HoneywellHelper.GenerateTextAtPosition(xp, HWPrimaryFontSize, value);
                    sb.Append(generatedText);
                    xpp = HWReceiptTableHeaderPositions[j] + value.Length;
                }
            }
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptTableBodyData(List<(string description, string value, string value2, string value3)> bodyLines)
        {
            HWReceiptTableHeaderPositions = new int[] { 0, 22, 52, 65 };
            HWReceiptTableHeaderRatio = new int[] { 20, 30, 10, 8 };
            StringBuilder sb = new StringBuilder();
            var processedList = HoneywellHelper.ProcessHeaderLines(bodyLines, HWReceiptTableHeaderRatio);
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
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptFooterData(List<(string description, string separator, string value)> hWFooterLines)
        {
            StringBuilder sb = new StringBuilder();
            HWReceiptFooterPositions = new int[] { 40, 67, 59 };
            HWReceiptFooterRatio = new int[] { 25, 1, 30 };
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
            sb.Append(new HoneywellHelper().NextLine(5, ref HWYposition));
            return sb.ToString();
        }
        private string AlignGapBetweenBodyAndFooter()
        {
            return new HoneywellHelper().NextLine(HoneywellPageHeight - (YPosition % HoneywellPageHeight), ref HWYposition);
        }

        #endregion

        /* ************************************************************************************************************************************************************************************************* */

        #region Woosim

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
            if (BodyLines.Count != 0)
            {
                string addressCard = GenerateAddressCardWoosimAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeaderWoosim(ReceiptHeader);
                string receiptHeaderData = GenerateReceiptHeaderDataWoosim(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderDataWoosim(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyDataWoosim(BodyLines);
                string receiptFooterData = GenerateReceiptFooterDataWoosim(HWFooterLines);

                returnorder.Append(addressCard);
                returnorder.Append(receiptTitleHeader);
                returnorder.Append(receiptHeaderData);
                returnorder.Append(receiptTableHeaderData);
                returnorder.Append(receiptTableBodyData);
                returnorder.Append(receiptFooterData);
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
                sb.Append(new   WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().NextLine(2));
            Wxposition = default;
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
            WReceiptHeaderRatio = new int[] { 15, 1, 30 };
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
        private string GenerateReceiptTableHeaderDataWoosim(List<(string p1, string p2, string p3, string p4)> hWBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptTableHeaderPositions = new int[] { 0, 22, 52, 62 };
            WReceiptTableHeaderRatio = new int[] { 20, 30, 10, 8 };
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
            }
            sb.Append(new WoosimHelper().NextLine(1));
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptTableBodyDataWoosim(List<(string description, string value, string value2, string value3)> bodyLines)
        {
            WReceiptTableHeaderPositions = new int[] { 0, 22, 52, 65 };
            WReceiptTableHeaderRatio = new int[] { 20, 30, 10, 8 };
            StringBuilder sb = new StringBuilder();
            var processedList = HoneywellHelper.ProcessHeaderLines(bodyLines, WReceiptTableHeaderRatio);
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
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            sb.Append(new WoosimHelper().NextLine(1));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptFooterDataWoosim(List<(string description, string separator, string value)> hWFooterLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptFooterPositions = new int[] { 40, 67, 59 };
            WReceiptFooterRatio = new int[] { 25, 1, 30 };
            var processedList = HoneywellHelper.ProcessHeaderLines(hWFooterLines, WReceiptFooterRatio);
            for (int i = 0; i < processedList.Count; i++)
            {
                int xpp = 0;
                for (int j = 0; j < processedList[i].Count; j++)
                {
                    int xp = WReceiptFooterPositions[j] - xpp;
                    string value = processedList[i][j];
                    string generatedText = WoosimHelper.GenerateTextAtPosition(xp, HWPrimaryFontSize, value);
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

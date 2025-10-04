using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.Extensions.Primitives;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Printing.BL.Classes.HelperClasses;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.Printing.BL.Classes.CollectionOrder
{
    public class CollectionOrderPrint : BasePrint
    {
        // Lists
        private List<(string description, string value)> HeaderLines = new List<(string description, string value)> { };
        private List<string> AddressLines = new List<string> { };
        private List<(string description, string value)> FooterLines = new List<(string description, string value)> { };
        private List<(string description, string value)> AmountLines = new List<(string description, string value)>();
        List<ICollectionPrint> collectionOrderStorePrint = new List<ICollectionPrint>();
        List<ICollectionPrintDetails> collectionOrderlinePrintViews = new List<ICollectionPrintDetails>();

        // For HoneyWell
        private List<(string description, string separator, string value)> HWHeaderLines = new() { };
        private List<(string description, string separator, string value)> HWFooterLines = new() { };
        private List<(string p1, string p2)> HWBodyHeaderLines;

        //Zebra
        private int ZebraPageWidth;
        private string NormalFontypeSizeType = "N";             //"C"
        private string BoldFontType = "0";
        private string NormalFont = "N";
        private string LeftAlign = "L";
        private string RightAlign = "R";
        private string CentreAlign = "C";
        private string Separator = ":";
        private int XPosition = 0;
        private int AddressLinesXPosition = 420;
        private int YPosition = 0;
        private int TextFontSize = 20;
        private int BodyTableHeaderFontSize = 22;
        private int Header1TextFontSize = 40;
        private int Header2TextFontSize = 30;
        private int Header3TextFontSize = 25;
        private int AddressCard1TextFontSize = 20;
        private int AddressCard2TextFontSize = 17;
        private int AddBlockSpace = 50;
        private int LineThickness = 5;
        private int UpperCaseCount;
        private int LowerCaseCount;
        private int NumericCaseCount;
        private int SpecialCharacterCount;
        private int[]? UpperCaseCharactersCountForPageWidth;
        private int[]? LowerCaseCharacterCountForPageWidth;
        private int[]? NumericCaseCharacterCountForPageWidth;
        private int[]? SpecialCharactersCountForPageWidth;
        private string? PaymentMode;
        private string? Receiptnumber;
        private string? Amount;
        private string? PrintDate;
        private string? currency;
        private string? PaymentTypeDetails;
        private List<string> TableHeaders = new List<string> { };
        private int[]? TableHeadersRatio;

        public override string CreatePrintString(PrinterType printerType, PrinterSize printerSize, object data)
        {

            
            StringBuilder sb = new StringBuilder();
            if (data is (List<ICollectionPrint> collectionOrderPrintView ))
            {
                AddressLines = new List<string>
                {
                    "Win Information Technology Pvt Ltd, ",
                    "Level-16, Block A, ","Sky-1, " ,"Prestige Towers, ",
                    "Hyderabad."
                };
                foreach (var item in collectionOrderPrintView)
                {
                    collectionOrderlinePrintViews = item.collectionPrintDetails;
                    PaymentMode = item.Category ?? string.Empty;
                    Receiptnumber = item.ReceiptNumber ?? string.Empty;
                    Amount = item.Amount.ToString() ?? string.Empty;
                    PrintDate = CommonFunctions.GetDateTimeInFormat(item.ChequeDate) ?? string.Empty;
                    currency =item.CurrencyUID ?? string.Empty;
                    HeaderLines = new List<(string description, string value)>
                    {
                        ("Store Name", item.Name ?? string.Empty),
                        ("Store Code", item.Code ?? string.Empty),
                        ("Collected By", (item.EmpUID +"["+"]") ?? string.Empty),
                        ("Payment Status", item.Status ?? string.Empty),
                        ("Address ", $"{item.Line1 ?? string.Empty}, {item.Line2 ?? string.Empty}, {item.Line3 ?? string.Empty}")

                    };

                    HWHeaderLines = new List<(string, string, string)>
                    {
                         ("Store Name",":", item.Name ?? string.Empty),
                         ("Store Code",":",  item.Code ?? string.Empty),
                         ("Collected By",":",  (item.EmpUID +"["+"]") ?? string.Empty),
                         ("Payment Status",":",  item.Status ?? string.Empty),
                         ("Address ",":",  $"{item.Line1 ?? string.Empty}, {item.Line2 ?? string.Empty}, {item.Line3 ?? string.Empty}"),
                    };
                    TableHeaders = new List<string> { "Reference Number", "Paid Amount" + " " + currency };
                    HWBodyHeaderLines = new() { ("Reference Number", "Paid Amount" + " " + currency) };
                    AmountLines = new List<(string description, string value)>
                        {
                            ("Total Amount",(Amount+"  "+currency+" ") )
                        };
                    if (item.Category != "Cash")
                    {
                        switch (item.Category)
                        {
                            case "Cheque":
                                PaymentTypeDetails = "Cheque No";
                                break;
                            case "POS":
                                PaymentTypeDetails = "POS Number";
                                break;
                            case "Online":
                                PaymentTypeDetails = "UTR Number";
                                break;
                        }
                        TableHeadersRatio = new int[] { 500, 300 };
                        FooterLines = new List<(string description, string value)>
                        {
                            ("Bank Name", item.BankUID ?? string.Empty),
                            ("Branch",item.Branch.ToString()),
                            (PaymentTypeDetails, item.ChequeNo ?? string.Empty),
                            ("Date", PrintDate ?? string.Empty)
                        };
                        HWFooterLines = new List<(string description, string value, string value1)>
                        {
                            ("Bank Name", ":",item.BankUID ?? string.Empty),
                            ("Branch",":",item.Branch.ToString()),
                            (PaymentTypeDetails , ":", item.ChequeNo ?? string.Empty),
                            ("Date", ":", PrintDate ?? string.Empty)
                        };

                    }
                    sb.Append(PreparePrintString(printerType, printerSize, (AddressLines, HeaderLines, collectionOrderlinePrintViews, FooterLines)));
                }
                if (printerType == PrinterType.Zebra)
                {
                    sb.Append(sb.Insert(0, ZebraHelper.SetLabelLength(YPosition)));
                }
            }
            return sb.ToString();
        }


        
        
        private StringBuilder PreparePrintString(PrinterType printerType, PrinterSize printerSize, (List<string> AddressLines, List<(string description, string value)> HeaderLines, List<ICollectionPrintDetails> collectionOrderLinePrintViews,  List<(string description, string value)> FooterLines) value)
        {
            StringBuilder retValue = new StringBuilder();
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
            switch (printerType)
            {
                case PrinterType.Zebra:
                    retValue = CreateSalesOrderPrintZebra(value);
                    break;
                case PrinterType.Honeywell:
                    retValue = CreateSalesOrderPrintHoneywell(value);
                    break;
                case PrinterType.Woosim:
                    retValue = CreateSalesOrderPrintWoosim(value);
                    break;
                default:
                    retValue = CreateSalesOrderPrintZebra(value);   // Default to Zebra if the printer type is not specified
                    break;
            }
            return retValue;
        }

        #region Zebra

        private StringBuilder CreateSalesOrderPrintZebra(object data)
        {
            StringBuilder CollectionOrderZplCode = new StringBuilder();
            if (HeaderLines.Count != 0 && collectionOrderlinePrintViews != null)
            {
                CollectionOrderZplCode.Append(ZebraHelper.GetLogo(XPosition, YPosition));
                CollectionOrderZplCode.Append(GenerateAddressCard(AddressLines, NormalFontypeSizeType));
                CollectionOrderZplCode.Append(GenerateZebraTitleCard("Receipt : ", Receiptnumber, Header1TextFontSize, CentreAlign));
                CollectionOrderZplCode.Append(GenerateZebraTitleCard("Payment Mode :", PaymentMode, Header2TextFontSize, CentreAlign));
                CollectionOrderZplCode.Append(GenerateZebraTitleCard("Date :", PrintDate, Header3TextFontSize, CentreAlign));
                CollectionOrderZplCode.Append(GenerateZebraHeaderCode(HeaderLines, NormalFontypeSizeType));
                CollectionOrderZplCode.Append(GenerateZebraBodyCode(collectionOrderlinePrintViews));
                //CollectionOrderZplCode.Append(GenerateCashPaymentFooter(AmountLines , NormalFontypeSizeType));
                if (PaymentMode != "Cash")
                {
                    CollectionOrderZplCode.Append(GenerateZebraTextWithUnderLine(PaymentMode + " Details :", Header3TextFontSize, LeftAlign));
                    CollectionOrderZplCode.Append(GenerateZebraHeaderCode(FooterLines, NormalFontypeSizeType));
                }
            }
            return CollectionOrderZplCode;
        }
        private string GenerateAddressCard(List<string> addressLines , string Font)
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
            StringBuilder HeaderCode = new StringBuilder();
            int fieldNameWidth = 200;
            int middlePartWidth = 20;
            int fieldValueWidth = 580;
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
        public string GenerateZebraBodyCode(List<ICollectionPrintDetails> collectionOrderLinePrintViews)
        {
            StringBuilder TableCode = new StringBuilder();
            TableCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition = YPosition + 2 * LineThickness;
            string fonttype = NormalFontypeSizeType;
            GetCharactersDictionaryCountWithFontTypeAndFontSize(TextFontSize, fonttype);
            int Bodylines = 1;
            foreach (string headerValue in TableHeaders)
            {
                int i = 0;
                string fieldName = headerValue;
                var fieldNameCounts = CalculateCharacterCounts(fieldName);
                int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                int fieldNameNumericCount = fieldNameCounts["Numeric"];
                int fieldNameSpecialCount = fieldNameCounts["Special"];
                double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);
                int linesNeeded = (int)Math.Ceiling((double)fieldName.Length / TableHeadersRatio[i]);
                TableCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesNeeded,CentreAlign, TextFontSize, TableHeadersRatio[i], headerValue));
                XPosition += TableHeadersRatio[i];
                if (linesNeeded > Bodylines) Bodylines = linesNeeded;
            }
            YPosition += (Bodylines * TextFontSize);
            XPosition = default;
            TableCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition = YPosition + 2 * LineThickness;
            string fontChangetype = NormalFontypeSizeType;
            GetCharactersDictionaryCountWithFontTypeAndFontSize(TextFontSize, fontChangetype);
            foreach (ICollectionPrintDetails collectionOrderLinePrintView in collectionOrderLinePrintViews)
            {
                List<string> Values = new List<string> {
                    collectionOrderLinePrintView.ReferenceNumber,
                    collectionOrderLinePrintView.PaidAmount.ToString()                   
                };
                Bodylines = 1;
                foreach (string Tablevalue in Values)
                {
                    int i = 0;
                    string fieldName = Tablevalue;
                    var fieldNameCounts = CalculateCharacterCounts(fieldName);
                    int fieldNameLowerCount = fieldNameCounts["LowerCase"];
                    int fieldNameUpperCount = fieldNameCounts["UpperCase"];
                    int fieldNameNumericCount = fieldNameCounts["Numeric"];
                    int fieldNameSpecialCount = fieldNameCounts["Special"];
                    double fieldNamelength = CalculateTextFieldLength(fieldNameLowerCount, fieldNameUpperCount, fieldNameNumericCount, fieldNameSpecialCount);
                    int linesNeeded = (int)Math.Ceiling((double)fieldName.Length / TableHeadersRatio[i]);
                    TableCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, linesNeeded, "C", TextFontSize, TableHeadersRatio[i], Tablevalue));
                    XPosition += TableHeadersRatio[i];
                    if (linesNeeded > Bodylines) Bodylines = linesNeeded;
                }
                YPosition += (Bodylines * TextFontSize);
                XPosition = default;
            }
            YPosition += LineThickness;
            TableCode.Append(ZebraHelper.GenerateHorizontalLine(ZebraPageWidth, XPosition, YPosition, 1, LineThickness));
            YPosition = YPosition + 2 * LineThickness;

            for (int i = 0; i < AmountLines.Count; i++)
            {
                string Value = AmountLines[i].value;
                string Description = AmountLines[i].description;
                TableCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, 0, LeftAlign, TextFontSize, 400, Description));
                XPosition += 400;
                TableCode.Append(ZebraHelper.GenerateTableHeaderCellData(XPosition, YPosition, 0, RightAlign, TextFontSize, 400, Value ));
            }
            YPosition += AddBlockSpace;
            XPosition = default;
            return TableCode.ToString();
        }
        private string GenerateZebraTextWithUnderLine(string mode, int Titlefontsize, string Alignment)
        {
            YPosition += Titlefontsize;
            StringBuilder TitleCode = new StringBuilder();
            string line = $"{ZebraHelper.GenerateTitleCardWithUnderLine(XPosition, YPosition, Alignment, Titlefontsize, mode, TextFontSize)}";
            YPosition += 2 * Titlefontsize;
            TitleCode.Append(line);
            return TitleCode.ToString();
        }
        private void GetCharactersDictionaryCountWithFontTypeAndFontSize(int textFontSize, string fonttype)
        {
            ZebraHelper zebraHelper = new ZebraHelper();
            var characterCounts = zebraHelper.GetCharacterCountsForPageWidth(textFontSize, fonttype , ZebraPageWidth);
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

        #region   HoneyWell 

        private int HoneywellPageWidth = HoneywellHelper.paperwidth;
        private int HoneywellPageHeight = HoneywellHelper.paperheight;
        private int HWxposition = 0;
        private int HWYposition = 0;
        private string? ReceiptHeader;
        private string? ReceiptPaymentHeader;
        private string? PaymentModeHeader;
        private string cmd = "\u001B";
        private string reset = "\u001B" + "@";
        private string HWPrimaryFontSize = "\u001B" + "W" + "0";
        private string HWHeaderFontSize = "\u001B" + "W" + "1";
        private string HWPrimaryFontStyle = "\u001B" + "E" + "0";
        private string HWBoldFontStyle = "\u001B" + "E" + "1";
        private int[] HWReceiptHeaderPositions = new int[] { };
        private int[] HWReceiptHeaderRatio = new int[] { };
        private int[] HWReceiptTableHeaderPositions = new int[] { };
        private int[] HWReceiptTableHeaderRatio = new int[] { };
        private int[] HWReceiptFooterPositions = new int[] { };
        private int[] HWReceiptFooterRatio = new int[] { };
        
        private StringBuilder CreateSalesOrderPrintHoneywell((List<string> AddressLines, List<(string description, string value)> HeaderLines, List<ICollectionPrintDetails> collectionOrderLinePrintViews, List<(string description, string value)> FooterLines) value)
        {
            ReceiptHeader = "Receipt: " + Receiptnumber;
            ReceiptPaymentHeader = "Payment Mode:" + PaymentMode;
            PaymentModeHeader = PaymentMode + " Details :";
            StringBuilder returnorder = new StringBuilder();
            if (HeaderLines.Count != 0 && collectionOrderlinePrintViews != null)
            {
                string addressCard = GenerateAddressCardAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeader(ReceiptHeader);
                string receiptTitlePaymentHeader = GenerateReceiptTitleHeader(ReceiptPaymentHeader);
                string receiptHeaderData = GenerateReceiptHeaderData(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderData(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyData(collectionOrderlinePrintViews);
                string receiptAmountData = GenerateReceiptAmountData(AmountLines);
                string receiptPaymentDetails = string.Empty;
                if (PaymentMode != "Cash")
                {
                   StringBuilder sb= new StringBuilder();
                   sb.Append(HoneywellHelper.GenerateTextAtPosition(40, HWPrimaryFontSize, PaymentModeHeader)).ToString();
                   sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
                   string PaymentModeHeaderline = new HoneywellHelper().GenerateunderlineOfLength(PaymentModeHeader.Length, ref HWYposition);
                   sb.Append(HoneywellHelper.GenerateTextAtPosition(40, HWPrimaryFontSize, PaymentModeHeaderline));
                   sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
                   string receiptFooterData = GenerateReceiptFooterData(HWFooterLines);
                   receiptPaymentDetails = sb.ToString() + receiptFooterData;
                }
                string AddReceiptAlignmentForPage = AlignGapBetweenBodyAndFooter();

                returnorder.Append(addressCard);
                returnorder.Append(receiptTitleHeader);
                returnorder.Append(receiptTitlePaymentHeader);
                returnorder.Append(receiptHeaderData);
                returnorder.Append(receiptTableHeaderData);
                returnorder.Append(receiptTableBodyData);
                returnorder.Append(receiptAmountData);
                if (PaymentMode != "Cash" && receiptPaymentDetails.Length > 0)
                {
                    returnorder.Append(receiptPaymentDetails);
                }
                returnorder.Append(AddReceiptAlignmentForPage);
            }
            return returnorder;
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
            sb.Append(HoneywellHelper.GenerateTextAtPosition(0, HWPrimaryFontSize, ""));
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            sb.Append(HoneywellHelper.GenerateTextAtPosition(HWxposition, HWHeaderFontSize, receiptHeader));
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
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
                    sb.Append(generatedText);
                    xpp = HWReceiptHeaderPositions[j] + value.Length;
                }
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            }
            return sb.ToString();
        }
        private string GenerateReceiptTableHeaderData(List<(string p1, string p2)> hWBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            HWReceiptTableHeaderPositions = new int[] { 10, 50 };
            HWReceiptTableHeaderRatio = new int[] { 30, 30 };
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
        private string GenerateReceiptTableBodyData(List<ICollectionPrintDetails> collectionOrderlinePrintViews)
        {
            HWReceiptTableHeaderPositions = new int[] { 10, 50 };
            HWReceiptTableHeaderRatio = new int[] { 30, 30 };
            StringBuilder sb = new StringBuilder();
            foreach (ICollectionPrintDetails collectionOrderLinePrintView in collectionOrderlinePrintViews)
            {
                List<(string p1, string p2)> Values = new List<(string p1, string p2)>
                {
                    (collectionOrderLinePrintView.ReferenceNumber,
                    collectionOrderLinePrintView.PaidAmount.ToString())
                };
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
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < amountLines.Count; i++)
            {
                string des = amountLines[i].description;
                string value = amountLines[i].value;
                int x1 = HWReceiptTableHeaderPositions[0];
                string generatedDes = HoneywellHelper.GenerateTextAtPosition(x1, HWPrimaryFontSize, des);
                int x2 = HWReceiptTableHeaderPositions[HWReceiptTableHeaderPositions.Length-1] - des.Length - x1 ;
                string generatedValue = HoneywellHelper.GenerateTextAtPosition((x2), HWPrimaryFontSize, value);
                sb.Append(generatedDes);
                sb.Append(generatedValue);
                sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            }
            sb.Append(new HoneywellHelper().AddHorizontalDeviderLines(1, ref HWYposition));
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptFooterData(List<(string description, string separator, string value)> hWFooterLines)
        {
            StringBuilder sb = new StringBuilder();
            HWReceiptFooterPositions = new int[] { 40, 57, 59 };
            HWReceiptFooterRatio = new int[] { 15, 1, 30 };
            sb.Append(new HoneywellHelper().NextLine(1, ref HWYposition));
            var processedList =  HoneywellHelper.ProcessHeaderLines(hWFooterLines, HWReceiptFooterRatio);
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
            return sb.ToString();
        }
        private string AlignGapBetweenBodyAndFooter()
        {
            return new HoneywellHelper().NextLine(HoneywellPageHeight - (YPosition % HoneywellPageHeight), ref HWYposition);
        }

        #endregion

        /* ************************************************************************************************************************************************************************************************* */
        //Woosim

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

        private StringBuilder CreateSalesOrderPrintWoosim((List<string> AddressLines, List<(string description, string value)> HeaderLines, List<ICollectionPrintDetails> collectionOrderLinePrintViews, List<(string description, string value)> FooterLines) value)
        {
            ReceiptHeader = "Receipt: " + Receiptnumber;
            ReceiptPaymentHeader = "Payment Mode:" + PaymentMode;
            PaymentModeHeader = PaymentMode + " Details :";
            StringBuilder returnorder = new StringBuilder();
            if (HeaderLines.Count != 0 && collectionOrderlinePrintViews != null)
            {
                string addressCard = GenerateAddressCardWoosimAtTop(AddressLines);
                string receiptTitleHeader = GenerateReceiptTitleHeaderWoosim(ReceiptHeader);
                string receiptTitlePaymentHeader = GenerateReceiptTitleHeaderWoosim(ReceiptPaymentHeader);
                string receiptHeaderData = GenerateReceiptHeaderDataWoosim(HWHeaderLines);
                string receiptTableHeaderData = GenerateReceiptTableHeaderDataWoosim(HWBodyHeaderLines);
                string receiptTableBodyData = GenerateReceiptTableBodyDataWoosim(collectionOrderlinePrintViews);
                string receiptAmountData = GenerateReceiptAmountDataWoosim(AmountLines);
                string receiptPaymentDetails = string.Empty;
                if (PaymentMode != "Cash")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(HoneywellHelper.GenerateTextAtPosition(40, WSubHeadingFontSize, PaymentModeHeader)).ToString();
                    sb.Append(new WoosimHelper().NextLine(1));
                    string PaymentModeHeaderline = new WoosimHelper().GenerateunderlineOfLength(PaymentModeHeader.Length);
                    sb.Append(WoosimHelper.GenerateTextAtPosition(40, WHeadingFontSize, PaymentModeHeaderline));
                    sb.Append(new WoosimHelper().NextLine(1));
                    string receiptFooterData = GenerateReceiptFooterDataWoosim(HWFooterLines);
                    receiptPaymentDetails = sb.ToString() + receiptFooterData;
                }
               

                returnorder.Append(addressCard);
                returnorder.Append(receiptTitleHeader);
                returnorder.Append(receiptTitlePaymentHeader);
                returnorder.Append(receiptHeaderData);
                returnorder.Append(receiptTableHeaderData);
                returnorder.Append(receiptTableBodyData);
                returnorder.Append(receiptAmountData);
                if (PaymentMode != "Cash" && receiptPaymentDetails.Length > 0)
                {
                    returnorder.Append(receiptPaymentDetails);
                }
                
            }
            return returnorder;
        }
        private string GenerateAddressCardWoosimAtTop(List<string> addressLines)
        {
            Wxposition = WoosimPageWidth - new HoneywellHelper().GetMaxLength(addressLines);
            StringBuilder sb = new StringBuilder();
            sb.Append(new WoosimHelper().NextLine(3));
            for (int i = 0; i < addressLines.Count; i++)
            {
                //string fontToUse = (i == 0) ? "\u001bW0" : "\u001bW0";
                sb.Append(WoosimHelper.GenerateTextAtPosition(HWxposition, WPrimaryFontSize, addressLines[i]));
                sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().NextLine(2));
            Wxposition = default;
            return sb.ToString();
        }
        private string GenerateReceiptTitleHeaderWoosim(string receiptHeader)
        {
            Wxposition = (WoosimPageWidth - (2 * receiptHeader.Length)) / 2;
            StringBuilder sb = new StringBuilder();
            sb.Append(WoosimHelper.GenerateTextAtPosition(0, WPrimaryFontSize, ""));
            sb.Append(new WoosimHelper().NextLine(1));
            sb.Append(WoosimHelper.GenerateTextAtPosition(HWxposition, WHeadingFontSize, receiptHeader));
            sb.Append(new WoosimHelper().NextLine(1));
            Wxposition = default;
            return sb.ToString();
        }
        private string GenerateReceiptHeaderDataWoosim(List<(string description, string separator, string value)>? headerLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptHeaderPositions = new int[] { 1, 17, 19 };
            WReceiptHeaderRatio = new int[] { 15, 1, 30 };
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
                    string generatedText = HoneywellHelper.GenerateTextAtPosition(xp, WPrimaryFontSize, value);
                    sb.ToString();
                    sb.Append(generatedText);
                    xpp = WReceiptHeaderPositions[j] + value.Length;
                }
                sb.Append(new WoosimHelper().NextLine(1));
            }
            return sb.ToString();
        }
        private string GenerateReceiptTableHeaderDataWoosim(List<(string p1, string p2)> WBodyHeaderLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptTableHeaderPositions = new int[] { 10, 50 };
            WReceiptTableHeaderRatio = new int[] { 30, 30 };
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
        private string GenerateReceiptTableBodyDataWoosim(List<ICollectionPrintDetails> collectionOrderlinePrintViews)
        {
            WReceiptTableHeaderPositions = new int[] { 10, 50 };
            WReceiptTableHeaderRatio = new int[] { 30, 30 };
            StringBuilder sb = new StringBuilder();
            foreach (ICollectionPrintDetails collectionOrderLinePrintView in collectionOrderlinePrintViews)
            {
                List<(string p1, string p2)> Values = new List<(string p1, string p2)>
                {
                    (collectionOrderLinePrintView.ReferenceNumber,
                    collectionOrderLinePrintView.PaidAmount.ToString())
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
                sb.Append(new WoosimHelper().NextLine(1 ));
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
                string generatedValue = HoneywellHelper.GenerateTextAtPosition((x2), HWPrimaryFontSize, value);
                sb.Append(generatedDes);
                sb.Append(generatedValue);
                sb.Append(new WoosimHelper().NextLine(1));
            }
            sb.Append(new WoosimHelper().AddHorizontalDeviderLines(1));
            sb.Append(new WoosimHelper().NextLine(1));
            string output = sb.ToString();
            return output;
        }
        private string GenerateReceiptFooterDataWoosim(List<(string description, string separator, string value)> hWFooterLines)
        {
            StringBuilder sb = new StringBuilder();
            WReceiptFooterPositions = new int[] { 40, 57, 59 };
            WReceiptFooterRatio = new int[] { 15, 1, 30 };
            sb.Append(new WoosimHelper().NextLine(1));
            var processedList = HoneywellHelper.ProcessHeaderLines(hWFooterLines, WReceiptFooterRatio);
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
            return sb.ToString();
        }



        #endregion
    }
}

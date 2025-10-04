using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Printing.Model.Enum;
namespace Winit.Modules.Printing.BL.Classes.HelperClasses
{
    public class ZebraHelper
    {
        private int[]? UpperCaseCharactersCountForPageWidth;
        private int[]? LowerCaseCharacterCountForPageWidth;
        private int[]? NumericCaseCharacterCountForPageWidth;
        private int[]? SpecialCharactersCountForPageWidth;
        private int ZebraPageWidth;
        public Dictionary<CharacterType, int> GetCharacterCountsForPageWidth(int fontSize, string fontType,  int paperWidth)
        {
            var result = new Dictionary<CharacterType, int>();
            ZebraPageWidth = paperWidth;
            if (ZebraPageWidth == 800)
            {
                switch (fontType)
                {
                    case "A":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 68, 67, 67, 45, 45, 45, 45, 45, 45, 45, 45 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                    case "B":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 45, 45, 45, 45, 45, 45, 45, 45, 30, 30, 30 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                    case "C":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                    case "0":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 77, 73, 0, 0, 0, 0, 58, 0, 0, 0, 0, 49, 0, 0, 0, 0, 42, 0, 0, 0, 0, 36 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 92, 88, 0, 0, 0, 0, 70, 0, 0, 0, 0, 58, 0, 0, 0, 0, 51, 0, 0, 0, 0, 45 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 89, 84, 80, 77, 73, 70, 68, 65, 62, 60, 58, 55, 0, 0, 0, 0, 48, 0, 0, 0, 0, 42 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 47, 45, 0, 0, 0, 0, 35, 0, 0, 0, 0, 30, 0, 0, 0, 0, 25, 0, 0, 0, 0, 22 };
                        break;
                    case "J":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 69, 66, 63, 61, 58, 56, 54, 52, 50, 49 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                    case "N":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 84, 79, 74, 70, 67, 63, 0, 0, 0, 0, 51, 0, 0, 0, 0, 42, 0, 0, 0, 0, 36, 0, 0, 0, 0, 32 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 96, 90, 85, 80, 76, 72, 0, 0, 0, 0, 58, 0, 0, 0, 0, 48, 0, 0, 0, 0, 41, 0, 0, 0, 0, 36 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 96, 89, 83, 79, 75, 71, 67, 64, 61, 59, 57, 54, 52, 51, 49, 47, 0, 0, 0, 0, 40, 0, 0, 0, 0, 35 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 60, 56, 53, 50, 47, 45, 0, 0, 0, 0, 36, 0, 0, 0, 0, 30, 0, 0, 0, 0, 26, 0, 0, 0, 0, 22 };
                        break;
                    case "P":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 104, 104, 104, 104, 104, 104, 104, 104, 104, 104, 104 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                    case "Q":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 74, 74, 74, 74, 74, 74, 74, 74, 74, 74, 74 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                }
            }
            else if (ZebraPageWidth == 600)
            {
                switch (fontType)
                {
                    case "C":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                    case "0":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                    case "N":
                        UpperCaseCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        LowerCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        NumericCaseCharacterCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        SpecialCharactersCountForPageWidth = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        break;
                }
            }

            result[CharacterType.UpperCase] = UpperCaseCharactersCountForPageWidth[fontSize - 1];
            result[CharacterType.LowerCase] = LowerCaseCharacterCountForPageWidth[fontSize - 1];
            result[CharacterType.Numeric] = NumericCaseCharacterCountForPageWidth[fontSize - 1];
            result[CharacterType.Special] = SpecialCharactersCountForPageWidth[fontSize - 1];
            return result;
        }



        public const string LineBreak = "^FS \n";
        public static string GenerateHorizontalLine(int width, int xaxis, int yaxis, int height, int thickness)
        {
            return $"^FO{xaxis},{yaxis} ^GB{width},{height},{thickness}{LineBreak}";
        }

        //Dotted Line
        public static string GenerateDottedLine(int length, int xaxis, int yaxis, int dotSpacing)
        {
            StringBuilder dottedLineBuilder = new StringBuilder();

            // Iterate over the length, adding dots with the specified spacing
            for (int i = 0; i < length; i += dotSpacing)
            {
                dottedLineBuilder.Append($"^FO{xaxis + i},{yaxis}^GB10,1,1{LineBreak}");
            }

            return dottedLineBuilder.ToString();
        }

       
        public static string GenerateFieldOrigin(int x, int y)
        {
            return $"^FO{x},{y}";
        }
        public static string GenerateFieldBlock(int width, string content, int fontSize, int fontStyle, PrinterAlignment alignment) //add text
        {
            char alignChar = alignment switch
            {
                PrinterAlignment.Left => 'L',
                PrinterAlignment.Center => 'C',
                PrinterAlignment.Right => 'R',
                _ => throw new ArgumentException("Invalid alignment specified"),
            };
            string adjustedContent = content.PadRight(width);

           // return $"^FO^CF{fontStyle},{fontSize}^CI0^FR^GB{width},1,1^FS^FO^FD^CI0^CF{fontStyle},{fontSize}^FR{adjustedContent}^FS^FO^GB{width},1,1^FS^FO^FD^FS^FO^FD^FS";

             return $"^FB{width},1,0,{alignChar}^FD^CF{fontStyle},{fontSize}^FD{content}{LineBreak}";
        }
        public static string GenerateFieldBlockCellData(int width, string content, int fontSize, int fontStyle, PrinterAlignment alignment) //add text
        {
            char alignChar = alignment switch
            {
                PrinterAlignment.Left => 'L',
                PrinterAlignment.Center => 'C',
                PrinterAlignment.Right => 'R',
                _ => throw new ArgumentException("Invalid alignment specified"),
            };
            string adjustedContent = content.PadRight(width);

            // return $"^FO^CF{fontStyle},{fontSize}^CI0^FR^GB{width},1,1^FS^FO^FD^CI0^CF{fontStyle},{fontSize}^FR{adjustedContent}^FS^FO^GB{width},1,1^FS^FO^FD^FS^FO^FD^FS";
            //return $"^FO {xPosition},{yPosition}  ^FB{maxColumnWidth},{linesNeeded},0,C,0 ^A 0, {primaryFontSize}  ^FD {propertyValue} {LineBreak}";
            return $"^FB{width},1,0,{alignChar}^FD^CF{fontStyle},{fontSize}^FD{content}{LineBreak}";
        }



        public static decimal MeasureTextWidth(string text, int fontSize)
        {
            return text.Length * fontSize;
        }

        public static string GenerateTableCellData(int xPosition, int yPosition, int maxColumnWidth,int linesNeeded,int SecondaryFontSize,string propertyValue)
        {
            //return $"^FO{xPosition},{yPosition + primaryFontSize}" +
            //                       $"^GB{maxColumnWidth},40,40^FB{maxColumnWidth},{linesNeeded},0,C^CF0,{SecondaryFontSize}^FD{propertyValue}{LineBreak}";
            return $"^FO {xPosition},{yPosition}  ^FB{maxColumnWidth},{linesNeeded},0,C,0 ^A 0, {SecondaryFontSize}  ^FD {propertyValue} {LineBreak}";
        }

        public static string GenerateTableHeaderCellData(int xPosition, int yPosition,int linesNeeded, string Alignment , int primaryFontSize, int maxColumnWidth,string propertyValue)
        {
            return $"^FO{xPosition},{yPosition}" + $"^A 0, {primaryFontSize},{primaryFontSize}" +
                        $"^FB{maxColumnWidth},4,0,{Alignment}^CFC,{primaryFontSize}^FD{propertyValue}{LineBreak}";
        }
        public static string GetLogo(int x ,int y)
        {
            // Implementation of GetLogoCode for ZebraPrinter
            return $@"^FO{x},{y}^GFA,3731,3731,41,,::::V07E,V0FF8,U01FFC,U03FFC,U03FFE,::U03FFEgP07J07L038I07,U03FFCgP07J07L038I07,U01FFCgM01C07J07L038I07,
            V0FF8gM01C07J07L038I07,V07FgN01C07Q038,V01CgN01C07Q038,hL01C07Q038,hL0FF873F807071FC0380387071FC007F9C,hL0FFC77FE07073FF0380787077FE00FFDC,
            hL0FF87IF0707IF0380F0707IF01IFC,R07E007E001F8gI01C07C0F0707E078381E0707C0F03C07C,Q01FF00FF807FCgI01C078070707C0383838070780783803C,
            Q03FF81FFC0FFEgI01C07007070780383870070780787803C,Q03FFC3FFC0IFgI01C070078707803838E0070700787001C,Q07FFC3FFE1IFgI01C07007870780383BE0070700787001C,
            Q07FFC3FFE1IFgI01C07007870780383FE0070700787001C,Q07FFC3FFE1IFgI01C07007870780383FF0070700787001C,Q07FFC3FFE1IFgI01C07007870780383E78070700787001C,
            Q03FFC3FFC0IFgI01C07007870780383C38070700787001C,Q03FF81FFC0FFEgI01C0700787078038383C070700787001C,Q01FF00FF807FCgI01C0700787078038381C070700787801C,
            R0FE007F003F8gI01C0700787078038381E070700787803C,R038001CI0EgJ01C0700787078038380F070700783C03C,hL01E07007870780383807070700783E0FC,
            hL01FC7007870780383807870700781IFC,hM0FC7007870780383803870700780FFDC,hM03gL03F1C,iV01C,iS03001C,iS07801C,iS038038,
            003FFE00F803FF87IF07FFI03FFC3IFC1MFgP03IF8,003FFE00F803FF87IF07FF8003FFC3IFC3MFgP01IF,003FFE01F801FF87IF07FFC003FFC3IFC3MFgQ07FC,
            I03E001F80038003EI07FCI03C001F803C01F007,I01E001FC0038003EI07FEI03C001F803C01F007,I01F001FC0078003EI07BEI03C001F803C01F007,
            I01F001FC0078003EI07BFI03C001F803C01F007,I01F003FC007I03EI079FI03C001F803C01F007,I01F003BE007I03EI078F8003C001F803C01F007,
            J0F003BE007I03EI078FC003C001F803C01F007,J0F8039E00FI03EI0787C003C001F803C01F007,J0F8039E00EI03EI0787E003C001F803C01F007,
            J0F8079E00EI03EI0783E003C001F803C01F007,J078071F00EI03EI0783F003C001F801C01F007V01CJ0E0F,J07C071F00EI03EI0781F003C001F8J01FY01CJ0E0F,
            J07C070F01EI03EI0781F803C001F8J01FY01CJ0E0F,J07C070F01CI03EI0780FC03C001F8J01FY01CL0F,J03C0E0F81CI03EI07807C03C001F8J01FY01CL0F,
            J03C0E0F81CI03EI07807E03C001F8J01FY01CL0F,J03E0E0781CI03EI07803E03C001F8J01FN03807I01F001C3CJ0F003E,
            J03E0E0783CI03EI07803F03C001F8J01FL039FE1FE007FE01CFF00E0F01FF8,J01E1E07C38I03EI07801F03C001F8J01FL03IF7FF01IF01DFFC0E0F03FFC,
            J01E1C07C38I03EI07801F83C001F8J01FL03F8FF0F03E0F81F87C0E0F07C3E,J01F1C03C38I03EI07800FC3C001F8J01FL03E07E0783C03C1F01E0E0F0780F,
            J01F1C03C78I03EI078007C3C001F8J01FL03C03C0787803C1E00F0E0F0F007,K0F1C03E7J03EI078007E3C001F8J01FL03C0380787001C1C00F0E0F0E007,
            K0F3C03E7J03EI078003E3C001F8J01FL03C0380387001E1C0070E0F0E0078,K0FB801E7J03EI078003F3C001F8J01FL03C0380387001E1C0070E0F0E0078,
            K0FB801E7J03EI078001F3C001F8J01FL03C0380387001E1C0070E0F0JF8,K0FB801EFJ03EI078001FBC001F8J01FL03C0380387001E1C0070E0F0JF8,
            K07F801FEJ03EI078I0FBC001F8J01FL03C0380387001E1C0070E0F0E,K07F800FEJ03EI078I0FFC001F8J01FL03C0380387001E1C0070E0F0E,
            K07FI0FEJ03EI078I07FC001F8J01FL03C0380387001C1C0070E0F0E,K07FI0FEJ03EI078I03FC001F8J01FL03C0380387801C1C00F0E0F0F007,
            K03FI0FCJ03EI078I03FC001F8J01FL03C0380387803C1E00E0E0F0F007,K03FI0FCJ03EI078I01FC001F8J01FL03C0380383C0781F01E0E0F0780E,
            K03FI07CJ03EI078I01FC001F8J01FL03C0380381F9F81FCFC0E0F03E7E,K03EI07CJ03EI078J0FC001F8J01FL03C0380380IF01DFF80E0F01FFC,
            K01EI07CI07IF07FF8I0FC03IFC007IFCJ01C03803807FC01CFF00E0F00FF,K01EI078I07IF07FF8I07C03IFC007IFC,K01EI038I07IF07FF8I03803IFC007IFC,
            ,::::::{LineBreak}";
        }
        public static string SetLabelLength(int pageHeight)
        {
            return $"^POI \r\n ^FO100,50 \r\n ^LL{pageHeight+50} \r\n";
        }

        public static string GenerateTitleCard( int xPosition ,int yPosition, string Alignment,int titleFontSize, string text, int DefFontSize   )
        {
            return $" ^CF{0},{titleFontSize} \r\n ^FO {xPosition},{yPosition} \r\n ^FB840,3,0,{Alignment},0 \r\n ^FD{text}{LineBreak} ^CF{0},{DefFontSize} ";
        }

        public static string GenerateTitleCardwithAlignment(int xPosition, int yPosition,string Alignment, int titleFontSize, string text, int DefFontSize,string font)
        {
            return $" ^CF{font},{titleFontSize} \r\n ^FO {xPosition},{yPosition} \r\n ^FB840,3,0,{Alignment},0 \r\n ^FD{text}{LineBreak} ^CF{font},{DefFontSize} ";
        }

        public static string GenerateTitleCardWithUnderLine(int xPosition, int yPosition, string Alignment, int titleFontSize, string text, int DefFontSize)
        {
            int underlineLength = titleFontSize * text.Length / 2;
            return $" ^CF{0},{titleFontSize} \r\n ^FO {xPosition},{yPosition} \r\n ^FB840,3,0,{Alignment},0 \r\n ^FD{text} ^FS ^FO {xPosition},{yPosition + titleFontSize} ^GB{underlineLength},3,3^F ^CF{0},{DefFontSize} {LineBreak}";
        }

        public static string GenerateFieldNameCellData(int xPosition, int yPosition, int linesNeeded, string Alignment,string fonttype, int primaryFontSize, int maxColumnWidth, string propertyValue)
        {
            return $"^FO{xPosition},{yPosition}" + $"^A 0, {primaryFontSize},{primaryFontSize}" +
                        $"^FB{maxColumnWidth},4,0,{Alignment}^CF{fonttype},{primaryFontSize}^FD{propertyValue}{LineBreak}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants.ReportEngine;

namespace Winit.Shared.Models.Models.ReportController
{
    public class LabelOptions
    {
        public bool? Show { get; set; } = true;
        public string? Position { get; set; } = LabelPosition.Inside;
        public int? FontSize { get; set; } = 12;
        public string? FontWeight { get; set; } 
        public string? Color { get; set; } = "#000000";
        public int? Rotate { get; set; }
        public int? Distance { get; set; }
        public string? VerticalAlign { get; set; }
        public string? Align { get; set; }
        public object? Formatter { get; set; }
    }
}

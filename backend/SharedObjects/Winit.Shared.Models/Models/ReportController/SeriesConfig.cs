using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants.ReportEngine;

namespace Winit.Shared.Models.Models.ReportController
{
    public class SeriesConfig
    {
        public string? Name { get; set; }
        public string Type { get; set; } = WChartType.Bar;  // "bar" or "line"
        public string? Color { get; set; }         // Bar/Line color (optional)
        public LabelOptions? Label { get; set; }   // Label configuration (optional)
        public string? Formatter { get; set; }
    }
}

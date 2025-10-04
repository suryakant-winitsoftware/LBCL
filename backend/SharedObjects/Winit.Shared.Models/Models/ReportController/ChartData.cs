using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Models.ReportController
{
    public class ChartData
    {
        public double Value { get; set; }
        public LabelOptions? Label { get; set; } = new(); 
        public ItemStyleOptions? ItemStyle { get; set; } = new();
    }
}

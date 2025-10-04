using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Common.GraphModels
{
    public class HallowGraphModel
    {
        public int CurrentProgress { get; set; }  // Current progress (e.g., quantity completed)
        public int TotalTarget { get; set; }      // Total target (e.g., goal quantity)
        public string ProgressColor { get; set; } // Color of the progress (e.g., blue)
        public string BackgroundColor { get; set; } // Color of the remaining progress (e.g., gray)
    }
}

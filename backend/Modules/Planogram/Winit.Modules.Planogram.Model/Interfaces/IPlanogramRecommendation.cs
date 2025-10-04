using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Planogram.Model.Interfaces
{
    public interface IPlanogramRecommendation
    {
        public string UID { get; set; }
        public string CategoryCode { get; set; }
        public decimal? ShareOfShelfCm { get; set; }
        public string SelectionType { get; set; }
        public string SelectionValue { get; set; }
        public string RecommendedImagePath { get; set; }
    }
}

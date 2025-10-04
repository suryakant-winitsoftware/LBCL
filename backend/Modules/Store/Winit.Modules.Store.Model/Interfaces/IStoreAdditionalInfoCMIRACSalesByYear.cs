using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreAdditionalInfoCMIRACSalesByYear
    {
        public int? Sn { get; set; }
        public int? Year { get; set; }
        public int? Year1 { get; set; }
        public int? Year2 { get; set; }
        public int? Year3 { get; set; }
        public int? Qty { get; set; }
        public int? Qty1 { get; set; }
        public int? Qty2 { get; set; }
        public int? Qty3 { get; set; }
    }
    public interface IStoreAdditionalInfoCMIRACSalesByYear1
    {
        public int? Sn { get; set; }
        public int? Year { get; set; }
        public int? Qty { get; set; }

    }
}

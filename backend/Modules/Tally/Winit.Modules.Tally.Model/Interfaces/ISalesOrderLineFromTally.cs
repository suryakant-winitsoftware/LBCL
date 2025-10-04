using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ISalesOrderLineFromTally : IBaseModel

    {
        public string DMSUID { get; set; }
        public string GUID { get; set; }
        public string VOUCHERNUMBER { get; set; }
        public string STOCKITEMNAME { get; set; }
        public string RATE { get; set; }
        public string DISCOUNT { get; set; }
        public string AMOUNT { get; set; }
        public string ACTUALQTY { get; set; }
        public string BILLEDQTY { get; set; }
        public string GST { get; set; }
    }
}

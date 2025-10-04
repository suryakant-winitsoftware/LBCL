using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class SalesOrderLineFromTally : BaseModel, ISalesOrderLineFromTally
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

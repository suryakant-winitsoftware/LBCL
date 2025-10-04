using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITSharedObjects.Models
{
   public class TrxStatusDco
    {
        //public string TrxCode { get; set; }
        //public string AppTrxId { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public string ResponseUID { get; set; }
        //public string GcmKey { get; set; }
        //public string Module { get; set; } = "SalesOrder";
   }
}

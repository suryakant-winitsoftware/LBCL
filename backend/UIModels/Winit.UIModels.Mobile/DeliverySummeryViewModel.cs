using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Mobile
{
    public class DeliverySummeryViewModel
    {
        public string CustomerName { set; get; } = "";
        public string CustomerCode { set; get; } = "";
        public string OrderNo { set; get; } = "";
        public DateTime Date { set; get; } 
        public bool IsDelivered { set; get; }
    }
}

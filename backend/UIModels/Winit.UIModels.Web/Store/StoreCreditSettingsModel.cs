using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class StoreCreditSettingsModel
    {
        public bool PurchaseOrderAlwaysRequired { get; set; }
        public bool WithPrintedPrices { get; set; }
        public bool CaptureSignature { get; set; }
        public bool AlwaysPrinted { get; set; }
        public bool AllowGoodReturns { get; set; }
        public bool AllowBadReturns { get; set; }
        public bool AllowReturnAgainstInvoice { get; set; }
        public bool AllowReturnWithSalesOrder { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class OrderSettingsModel
    {
        [Required(ErrorMessage = "Standing PO Number is required.")]
        public string PurchaseOrderNumber { get; set; }

        [Required(ErrorMessage = "Price List is required.")]
        public string PriceList { get; set; }

       
        public int IsPurchaseOrderRequired { get; set; }

        
        public bool IsWithPrintedInvoices { get; set; }

       
        public bool IsCaptureSignatureRequired { get; set; }

        
        public bool IsAlwaysPrinted { get; set; }
    }
}

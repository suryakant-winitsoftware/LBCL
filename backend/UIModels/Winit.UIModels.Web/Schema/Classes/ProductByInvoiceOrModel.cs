using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Schema
{
    public class ProductByInvoiceOrModel
    {
        public string InvoiceNumber {  get; set; }
        public DateTime? Date { get; set; }
        public int TotalQty {  get; set; }

        public bool IsSelected {  get; set; }

        public string ModelName {  get; set; }
        public string ModelNumber { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ETL.Invoice.Model.Interfaces
{
    public interface IInvoiceDataChangeEvent
    {
        public string InvoiceUID { get; init; }
        public string ActionType { get; init; } // "Insert", "Update", "Delete"
        public Dictionary<string, object>? AdditionalData { get; init; }
    }
}

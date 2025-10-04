using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ETL.Invoice.Model.Interfaces;

namespace Winit.Modules.ETL.Invoice.Model.Classes
{
    public class InvoiceDataChangeEvent : IInvoiceDataChangeEvent
    {
        public string InvoiceUID { get; init; }
        public string ActionType { get; init; } // "Insert", "Update", "Delete"
        public Dictionary<string, object>? AdditionalData { get; init; }
    }
}

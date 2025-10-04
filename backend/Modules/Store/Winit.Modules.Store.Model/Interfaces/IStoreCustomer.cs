using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreCustomer
    {
        public string UID { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
        public string Address { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSelected { get; set; }
        public string RouteCustomerUID { get; set; }
        public int SeqNo { get; set; }
    }
}

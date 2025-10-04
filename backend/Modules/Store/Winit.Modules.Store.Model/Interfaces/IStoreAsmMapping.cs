using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreAsmMapping
    {
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string SiteCode { get; set; }
        public string Division { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string ErrorMessage { get; set; }
        public string LinkedItemType { get; set; }
        public bool IsValid { get; set; }
        public string Id => CustomerCode + SiteCode + Division;

        //To get from DB

        public string EmpUID { get; set; }
        public string StoreUID { get; set; }
        public string SiteUID { get; set; }
        public bool IsAsmMappedByCustomer { get; set; }

    }
}

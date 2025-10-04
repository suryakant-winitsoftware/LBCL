using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinIT.RuleEngine.UI.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool IsNew { get; set; }
        public int OrderCount { get; set; }
    }
}

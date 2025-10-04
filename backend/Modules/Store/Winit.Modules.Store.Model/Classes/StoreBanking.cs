using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreBanking :IStoreBanking
    {
        public int? Sn { get; set; }
        public long? BankAccountNo1 { get; set; }
        public string BankName1 { get; set; }
        public string BankAddressFirst1 { get; set; }
        public string BankAddressFirst2 { get; set; }
        public string IFSCCode1 { get; set; }
        public long? BankAccountNo2 { get; set; }
        public string BankName2 { get; set; }
        public string BankAddressSecond1 { get; set; }
        public string BankAddressSecond2 { get; set; }
        public string IFSCCode2 { get; set; }
    }
    public class StoreBankingJson : IStoreBankingJson
    {
        public int? Sn { get; set; }
        public long? BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string BankAddress1 { get; set; }
        public string BankAddress2 { get; set; }
        public string IFSCCode { get; set; }
    }
}

using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Bank.Model.Classes
{
    public class Bank : BaseModel, IBank
    {
        public int? SS { get; set; }
        public string? CompanyUID { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string CountryUID { get; set; }
        public decimal ChequeFee { get; set; }
        public string? CountryName { get; set; }
    }
}

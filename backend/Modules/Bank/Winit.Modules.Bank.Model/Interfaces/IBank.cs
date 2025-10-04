namespace Winit.Modules.Bank.Model.Interfaces
{
    public interface IBank : Winit.Modules.Base.Model.IBaseModel
    {
        public string? CompanyUID { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string CountryUID { get; set; }
        public decimal ChequeFee { get; set; }
        public string CountryName { get; set; }
    }
}

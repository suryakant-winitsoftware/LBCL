using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface IEarlyPaymentConfigurationViewModel
    {
        public IEarlyPaymentDiscountConfiguration EarlyPayment { get; set; }
        public AccCustomer[] Responsedata { get; set; }
        public DateTime? selectedDate { get; set; }
        public string selectedValueText { get; set; }
        public string selectedValue1 { get; set; }
        public string _date { get; set; }
        Task GetCustomers(string CustomerCode);
        Task<string> AddEarlyPayment(IEarlyPaymentDiscountConfiguration EarlyPayment);
        Task<List<IEarlyPaymentDiscountConfiguration>> GetConfigurationDetails();
    }
}

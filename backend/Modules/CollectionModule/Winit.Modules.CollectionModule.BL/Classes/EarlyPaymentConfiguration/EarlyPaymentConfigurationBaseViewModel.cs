using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes.EarlyPaymentConfiguration
{
    public abstract class EarlyPaymentConfigurationBaseViewModel : IEarlyPaymentConfigurationViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public DateTime? selectedDate { get; set; } = DateTime.Now;
        public string selectedValueText { get; set; } = "-- Select Customer --";
        public string selectedValue1 { get; set; } = "";
        public string _date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public AccCustomer[] Responsedata { get; set; } = new AccCustomer[0];
        public IEarlyPaymentDiscountConfiguration EarlyPayment { get; set; } = new EarlyPaymentDiscountConfiguration();
        public EarlyPaymentConfigurationBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appConfig = appConfig;
            _appUser = appUser;
        }

        public abstract Task GetCustomers(string CustomerCode);
        public abstract Task<string> AddEarlyPayment(IEarlyPaymentDiscountConfiguration EarlyPayment);
        public abstract Task<List<IEarlyPaymentDiscountConfiguration>> GetConfigurationDetails();
    }
}

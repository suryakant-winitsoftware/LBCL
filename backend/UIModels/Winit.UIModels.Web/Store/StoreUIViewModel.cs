using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class StoreUIViewModel
    {
        public CustomerInformationModel _CustomerInformation { get; set; }=new CustomerInformationModel();
        public List<ContactPersonDetailsModel> _ContactDetails { get; set; } = new List<ContactPersonDetailsModel>();
        public AddressModel _BillToAddress { get; set; } = new AddressModel() ;
        public AddressModel _ShipToAddress { get; set; } = new AddressModel() ;
        public InvoiceInfoModel _InvoiceInfo { get; set; } = new InvoiceInfoModel();
        public RouteDeliveryProfileModel _RouteDelivery { get; set; } = new RouteDeliveryProfileModel();
        public OrganisationConfigurationModel _Organisation { get; set; } = new OrganisationConfigurationModel();

        public OrderSettingsModel _OrderSettings { get; set; }=new OrderSettingsModel();
        public StoreCreditSettingsModel _CreditSettings { get; set; }=new StoreCreditSettingsModel();
        public AwayPeriodModel _AwayPeriod { get; set; }=new AwayPeriodModel();
        public CustomerDetailsModel _CustomerDetails { get; set; }=new CustomerDetailsModel();
        public CustomerCreationExpirationModel _CustomerCreation { get; set; }=new CustomerCreationExpirationModel();
    }
}

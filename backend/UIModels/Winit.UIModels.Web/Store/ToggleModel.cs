using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class ToggleModel
    {
        private bool customerInformation = true;
        private bool contactDetails = false;
        private bool contactPersonDetails = false;
        private bool shipToAddress = false;
        private bool billToAddress = false;

        private bool organisationConfiguration = false;
        private bool storeDocuments = false;
        private bool paymentDetails = false;

        // New properties to handle toggling
        private bool invoiceInformation = false;
        private bool routeDeliveryProfile = false;
        private bool showOrderSettings = false;
        private bool showStoredCreditSettings = false;
        private bool showAwayPeriodDetails = false;
        private bool showCustomerDetail = false;
        private bool showCustomerCreationExpiration = false;

        public bool CustomerInformation
        {
            get { return customerInformation; }
            set
            {
                customerInformation = value;
                if (value)
                {
                    ResetOtherProperties(nameof(CustomerInformation));
                }
            }
        }

        public bool ContactDetails
        {
            get { return contactDetails; }
            set
            {
                contactDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(ContactDetails));
                }
            }
        }
        public bool ContactPersonDetails
        {
            get { return contactPersonDetails; }
            set
            {
                contactPersonDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(ContactPersonDetails));
                }
            }
        }
        public bool StoreDocuments
        {
            get { return storeDocuments; }
            set
            {
                storeDocuments = value;
                if (value)
                {
                    ResetOtherProperties(nameof(StoreDocuments));
                }
            }
        }
        public bool PaymentDetails
        {
            get { return paymentDetails; }
            set
            {
                paymentDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(PaymentDetails));
                }
            }
        }

        public bool ShipToAddress
        {
            get { return shipToAddress; }
            set
            {
                shipToAddress = value;
                if (value)
                {
                    ResetOtherProperties(nameof(ShipToAddress));
                }
            }
        }

        public bool BillToAddress
        {
            get { return billToAddress; }
            set
            {
                billToAddress = value;
                if (value)
                {
                    ResetOtherProperties(nameof(BillToAddress));
                }
            }
        }

        public bool InvoiceInformation
        {
            get { return invoiceInformation; }
            set
            {
                invoiceInformation = value;
                if (value)
                {
                    ResetOtherProperties(nameof(InvoiceInformation));
                }
            }
        }

        public bool RouteDeliveryProfile
        {
            get { return routeDeliveryProfile; }
            set
            {
                routeDeliveryProfile = value;
                if (value)
                {
                    ResetOtherProperties(nameof(RouteDeliveryProfile));
                }
            }
        }

        public bool OrganisationConfiguration
        {
            get { return organisationConfiguration; }
            set
            {
                organisationConfiguration = value;
                if (value)
                {
                    ResetOtherProperties(nameof(OrganisationConfiguration));
                }
            }
        }

        public bool ShowOrderSettings
        {
            get { return showOrderSettings; }
            set
            {
                showOrderSettings = value;
                if (value)
                {
                    ResetOtherProperties(nameof(ShowOrderSettings));
                }
            }
        }

        public bool ShowStoredCreditSettings
        {
            get { return showStoredCreditSettings; }
            set
            {
                showStoredCreditSettings = value;
                if (value)
                {
                    ResetOtherProperties(nameof(ShowStoredCreditSettings));
                }
            }
        }

        public bool ShowAwayPeriodDetails
        {
            get { return showAwayPeriodDetails; }
            set
            {
                showAwayPeriodDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(ShowAwayPeriodDetails));
                }
            }
        }

        public bool ShowCustomerDetail
        {
            get { return showCustomerDetail; }
            set
            {
                showCustomerDetail = value;
                if (value)
                {
                    ResetOtherProperties(nameof(ShowCustomerDetail));
                }
            }
        }

        public bool ShowCustomerCreationExpiration
        {
            get { return showCustomerCreationExpiration; }
            set
            {
                showCustomerCreationExpiration = value;
                if (value)
                {
                    ResetOtherProperties(nameof(ShowCustomerCreationExpiration));
                }
            }
        }

        // Helper method to reset other properties
        private void ResetOtherProperties(string currentProperty)
        {
            // Reset all properties except the current one to false
            if (currentProperty != nameof(CustomerInformation)) customerInformation = false;
            if (currentProperty != nameof(ContactDetails)) contactDetails = false;
            if (currentProperty != nameof(ContactPersonDetails)) contactPersonDetails = false;
            if (currentProperty != nameof(ShipToAddress)) shipToAddress = false;
            if (currentProperty != nameof(BillToAddress)) billToAddress = false;
            if (currentProperty != nameof(InvoiceInformation)) invoiceInformation = false;
            if (currentProperty != nameof(RouteDeliveryProfile)) routeDeliveryProfile = false;
            if (currentProperty != nameof(OrganisationConfiguration)) organisationConfiguration = false;
            if (currentProperty != nameof(StoreDocuments)) storeDocuments = false;

            // Reset all new properties except the current one to false
            if (currentProperty != nameof(ShowOrderSettings)) showOrderSettings = false;
            if (currentProperty != nameof(ShowStoredCreditSettings)) showStoredCreditSettings = false;
            if (currentProperty != nameof(ShowAwayPeriodDetails)) showAwayPeriodDetails = false;
            if (currentProperty != nameof(ShowCustomerDetail)) showCustomerDetail = false;
            if (currentProperty != nameof(ShowCustomerCreationExpiration)) showCustomerCreationExpiration = false;
        }
    }
}

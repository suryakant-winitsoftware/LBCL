using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Mobile.Store
{
    public class Toggle
    {
        private bool storeInformation = true;
       
        private bool contactDetails = false;
        private bool contactPersonDetails = false;
        private bool shipToAddress = false;
        private bool billToAddress = false;

        private bool storeAddress = false;
        private bool storeDocuments = false;
        private bool paymentDetails = false;


        public bool CustomerInformation
        {
            get { return storeInformation; }
            set
            {
                storeInformation = value;
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
        public bool StoreAddress
        {
            get { return storeAddress; }
            set
            {
                storeAddress = value;
                if (value)
                {
                    ResetOtherProperties(nameof(StoreAddress));
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


        private void ResetOtherProperties(string currentProperty)
        {
            // Reset all properties except the current one to false
            if (currentProperty != nameof(CustomerInformation)) storeInformation = false;
            if (currentProperty != nameof(ContactDetails)) contactDetails = false;
            if (currentProperty != nameof(ContactPersonDetails)) contactPersonDetails = false;
            if (currentProperty != nameof(ShipToAddress)) shipToAddress = false;
            if (currentProperty != nameof(BillToAddress)) billToAddress = false;
            if (currentProperty != nameof(StoreAddress)) storeAddress = false;
            if (currentProperty != nameof(PaymentDetails)) paymentDetails = false;
            
            if (currentProperty != nameof(StoreDocuments)) storeDocuments = false;

        }



    }
}

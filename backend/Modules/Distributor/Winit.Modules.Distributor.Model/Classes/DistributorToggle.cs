using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.Distributor.Model.Interfaces;

namespace Winit.Modules.Distributor.Model.Classes
{
    public class DistributorToggle : IDistributorToggle
    {
        bool isInformation { get; set; } 
        bool isAddess { get; set; }
        bool isContact { get; set; }
        bool isDocument { get; set; }
        bool isCurrency { get; set; }
        bool isCustomerDetails { get; set; }
        bool isCaptureSelfie { get; set; }
        bool isOtpVerified { get; set; }
        bool isContactDetails { get; set; }
        bool isBillToAddress { get; set; }
        bool isShipToAddress { get; set; }
        bool isKarta { get; set; }
        bool isAsmMapping { get; set; }
        bool isEmployeeDetails { get; set; }
        bool isShowRoomDetails { get; set; }
        bool isServiceCenterDetails { get; set; }
        bool isBankersDetails { get; set; }
        bool isDistributorBusinessDetails { get; set; }
        bool isBusinessDetails { get; set; }
        bool isEarlierWorkedWithCMI { get; set; }
        bool isAreaofDistributionAgreed { get; set; }
        bool isAreaofOperationAgreed { get; set; }
        bool isDocumentsAppendixA { get; set; }
        bool isCodeEthicsConductPartners { get; set; }
        bool isTermsConditionsAgreements { get; set; }
        public bool IsInformation
        {
            get
            {
                return isInformation;
            }
            set
            {
                isInformation = value;
                if(value)
                {
                    ResetOtherProperties(nameof(IsInformation));
                }
            }
        }

        public bool IsAddess
        {
            get
            {
                return isAddess;
            }
            set
            {
                isAddess = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsAddess));
                }
            }
        }
        public bool IsContact
        {
            get
            {
                return isContact;
            }
            set
            {
                isContact = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsContact));
                }
            }
        }
        public bool IsDocument
        {
            get
            {
                return isDocument;
            }
            set
            {
                isDocument = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsDocument));
                }
            }
        }
        public bool IsCurrency
        {
            get
            {
                return isCurrency;
            }
            set
            {
                isCurrency = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsCurrency));
                }
            }
        }
        public bool IsCustomerDetails
        {
            get
            {
                return isCustomerDetails;
            }
            set
            {
                isCustomerDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsCustomerDetails));
                }
            }
        }
        public bool IsCaptureSelfie
        {
            get
            {
                return isCaptureSelfie;
            }
            set
            {
                isCaptureSelfie = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsCaptureSelfie));
                }
            }
        }
        public bool IsOtpVerified
        {
            get
            {
                return isOtpVerified;
            }
            set
            {
                isOtpVerified = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsOtpVerified));
                }
            }
        }
        public bool IsContactDetails
        {
            get
            {
                return isContactDetails;
            }
            set
            {
                isContactDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsContactDetails));
                }
            }
        }
        public bool IsBillToAddress
        {
            get
            {
                return isBillToAddress;
            }
            set
            {
                isBillToAddress = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsBillToAddress));
                }
            }
        }
        public bool IsShipToAddress
        {
            get
            {
                return isShipToAddress;
            }
            set
            {
                isShipToAddress = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsShipToAddress));
                }
            }
        }
        public bool IsKarta
        {
            get
            {
                return isKarta;
            }
            set
            {
                isKarta = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsKarta));
                }
            }
        }
        public bool IsAsmMapping
        {
            get
            {
                return isAsmMapping;
            }
            set
            {
                isAsmMapping = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsAsmMapping));
                }
            }
        }
        public bool IsServiceCenterDetails
        {
            get
            {
                return isServiceCenterDetails;
            }
            set
            {
                isServiceCenterDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsServiceCenterDetails));
                }
            }
        }
        public bool IsEmployeeDetails
        {
            get
            {
                return isEmployeeDetails;
            }
            set
            {
                isEmployeeDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsEmployeeDetails));
                }
            }
        }
        public bool IsShowRoomDetails
        {
            get
            {
                return isShowRoomDetails;
            }
            set
            {
                isShowRoomDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsShowRoomDetails));
                }
            }
        }
        public bool IsBankersDetails
        {
            get
            {
                return isBankersDetails;
            }
            set
            {
                isBankersDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsBankersDetails));
                }
            }
        }
        public bool IsDistributorBusinessDetails
        {
            get
            {
                return isDistributorBusinessDetails;
            }
            set
            {
                isDistributorBusinessDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsDistributorBusinessDetails));
                }
            }
        }
        public bool IsBusinessDetails
        {
            get
            {
                return isBusinessDetails;
            }
            set
            {
                isBusinessDetails = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsBusinessDetails));
                }
            }
        }
        public bool IsEarlierWorkedWithCMI
        {
            get
            {
                return isEarlierWorkedWithCMI;
            }
            set
            {
                isEarlierWorkedWithCMI = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsEarlierWorkedWithCMI));
                }
            }
        }
        public bool IsAreaofDistributionAgreed
        {
            get
            {
                return isAreaofDistributionAgreed;
            }
            set
            {
                isAreaofDistributionAgreed = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsAreaofDistributionAgreed));
                }
            }
        }
        public bool IsAreaofOperationAgreed
        {
            get
            {
                return isAreaofOperationAgreed;
            }
            set
            {
                isAreaofOperationAgreed = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsAreaofOperationAgreed));
                }
            }
        }
        public bool IsDocumentsAppendixA
        {
            get
            {
                return isDocumentsAppendixA;
            }
            set
            {
                isDocumentsAppendixA = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsDocumentsAppendixA));
                }
            }
        }
        public bool IsCodeEthicsConductPartners
        {
            get
            {
                return isCodeEthicsConductPartners;
            }
            set
            {
                isCodeEthicsConductPartners = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsCodeEthicsConductPartners));
                }
            }
        }
        public bool IsTermsConditionsAgreements
        {
            get
            {
                return isTermsConditionsAgreements;
            }
            set
            {
                isTermsConditionsAgreements = value;
                if (value)
                {
                    ResetOtherProperties(nameof(IsTermsConditionsAgreements));
                }
            }
        }
        protected void ResetOtherProperties(string propertyName)
        {
            if(propertyName != nameof(IsInformation)) { isInformation = false; }
            if(propertyName != nameof(IsDocument)) { isDocument = false; }
            if(propertyName != nameof(IsContact)) { isContact = false; }
            if(propertyName != nameof(IsAddess)) { isAddess = false; }
            if(propertyName != nameof(IsCurrency)) { isCurrency = false; }
            if(propertyName != nameof(IsCustomerDetails)) { isCustomerDetails = false; }
            if(propertyName != nameof(IsContactDetails)) { isContactDetails = false; }
            if(propertyName != nameof(IsBillToAddress)) { isBillToAddress = false; }
            if(propertyName != nameof(IsShipToAddress)) { isShipToAddress = false; }
            if(propertyName != nameof(IsAsmMapping)) { isAsmMapping = false; }
            if(propertyName != nameof(IsServiceCenterDetails)) { isServiceCenterDetails = false; }
            if(propertyName != nameof(IsEmployeeDetails)) { isEmployeeDetails = false; }
            if(propertyName != nameof(IsShowRoomDetails)) { isShowRoomDetails = false; }
            if(propertyName != nameof(IsBankersDetails)) { isBankersDetails = false; }
            if(propertyName != nameof(IsDistributorBusinessDetails)) { isDistributorBusinessDetails = false; }
            if(propertyName != nameof(IsKarta)) { isKarta = false; }
            if(propertyName != nameof(IsBusinessDetails)) { isBusinessDetails = false; }
            if(propertyName != nameof(IsEarlierWorkedWithCMI)) { isEarlierWorkedWithCMI = false; }
            if(propertyName != nameof(IsAreaofDistributionAgreed)) { isAreaofDistributionAgreed = false; }
            if(propertyName != nameof(IsAreaofOperationAgreed)) { isAreaofOperationAgreed = false; }
            if(propertyName != nameof(IsDocumentsAppendixA)) { isDocumentsAppendixA = false; }
            if(propertyName != nameof(IsCodeEthicsConductPartners)) { isCodeEthicsConductPartners = false; }
            if(propertyName != nameof(IsTermsConditionsAgreements)) { isTermsConditionsAgreements = false; }
            if(propertyName != nameof(IsCaptureSelfie)) { isCaptureSelfie = false; }
            if(propertyName != nameof(IsOtpVerified)) { isOtpVerified = false; }
        }
    }
}

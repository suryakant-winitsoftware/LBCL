using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Distributor.Model.Interfaces
{
    public interface IDistributorToggle
    {
        bool IsInformation { get; set; }
        bool IsAddess {  get; set; }
        bool IsContact {  get; set; }
        bool IsDocument {  get; set; }
        bool IsCurrency {  get; set; }
        bool IsCustomerDetails { get; set; }
        bool IsCaptureSelfie { get; set; }
        bool IsOtpVerified { get; set; }
        bool IsContactDetails { get; set; }
        bool IsBillToAddress { get; set; }
        bool IsShipToAddress { get; set; }
        bool IsKarta { get; set; }
        bool IsAsmMapping { get; set; }
        bool IsServiceCenterDetails { get; set; }
        bool IsEmployeeDetails { get; set; }
        bool IsShowRoomDetails { get; set; }
        bool IsBankersDetails { get; set; }
        bool IsDistributorBusinessDetails { get; set; }
        bool IsBusinessDetails { get; set; }
        bool IsEarlierWorkedWithCMI { get; set; }
        bool IsAreaofDistributionAgreed { get; set; }
        bool IsAreaofOperationAgreed { get; set; }
        bool IsDocumentsAppendixA { get; set; }
        bool IsCodeEthicsConductPartners { get; set; }
        bool IsTermsConditionsAgreements { get; set; }
    }
}

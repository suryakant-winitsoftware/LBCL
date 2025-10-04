using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using static Winit.Modules.CollectionModule.BL.Classes.CollectionAppViewModel;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface ICollectionModuleViewModel
    {
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _PopulateList { get; set; }
        public IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore> _customersList { get; set; }
        Task PopulateViewModel();
        void OnSignatureProceedClick();
        void PrepareSignatureFields();
        public string CustomerSignatureFolderPath { get; set; }
        public string UserSignatureFolderPath { get; set; }
        public string CustomerSignatureFileName { get; set; }
        public string UserSignatureFileName { get; set; }
        public string CustomerSignatureFilePath { get; set; }
        public string UserSignatureFilePath { get; set; }
        public string ConsolidatedReceiptNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public string SignatureFolderPath { get; set; }
        public string CustomerName { get; set; }
        public List<IFileSys> SignatureFileSysList { get; set; }
        public string Guidstring();
        public string sixGuidstring();
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _invoiceList { get; set; }
        Task<List<Model.Interfaces.IAccPayable>> GetInvoicesMobile(string AccCollectionUID);
        Task<List<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs);
        Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode);
        public List<IAccCollection> _collectionList { get; set; }
        Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode);
        Task<string> CreateReceipt(ICollections collection);
        Task<string> CreateOnAccount(ICollections collection);
        Task<List<IAccCollection>> PaymentReceipts(string FromDate, string ToDate, string Payment);
        Task<List<IAccCollectionAllotment>> AllotmentReceipts(string AccCollectionUID);
        Task<List<IAccCollectionPaymentMode>> CPOData(string AccCollectionUID);
        Task<List<ICollectionPrint>> GetCollectionStoreDataForPrinter(List<string> UID);
        Task<List<ICollectionPrintDetails>> GetAllotmentDataForPrinter(string AccCollectionUID);
    }
}

using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.Distributor.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common.GST; 

namespace Winit.Modules.Distributor.BL.Interfaces
{
    public interface ICreateDistributorBaseViewModel
    {
        bool IsStatusVisible { get; set; }
        string StatusLable { get; set; }
        bool IsLoad { get; set; }
        bool IsShowPopUp { get; set; }
        bool IsNewDistributor { get; set; }
        string FilePath {  get; set; }
      
        List<ISelectionItem> StatusList { get; set; }
        Model.Classes.Distributor Distributor { get; set; }
       
        Contact.Model.Classes.Contact _Contact { get; set; }
        bool IsNewDoc { get; set; }
        Winit.Modules.StoreDocument.Model.Classes.StoreDocument _StoreDocument { get; set; }
        List<ISelectionItem> TaxGroupList { get; set; }
        bool IsTaxGroupVisible { get; set; }
        string TaxGroupLabel { get; set; }
        List<ISelectionItem> CurrencyList { get; set; }
        bool IsCurrencyVisible { get; set; }
        string CurrencyLabel { get; set; }
        List<ISelectionItem> DocumentList { get; set; }

        public GSTINDetailsModel GSTINDetails { get; set; }
        string? DocumentLabel { get; set; } 
        bool IsDocumentVisible { get; set; }
        List<DataGridColumn> Columns { get; set; }
        DistributorMasterView DistributorMasterView { get; set; }
        List<IFileSys> DisplayFileSysList {  get; set; }
        void GetFilesysList(List<IFileSys> fileSys);
        Task<bool> CreateStoreImage();
        Task ChangePrimaryCurrency(IOrgCurrency orgCurrency);
        Task DeleteCurrency(IOrgCurrency orgCurrency);

        Task<bool> GetGstDetails(string gstNumber);
        Task<(bool,bool)> Save();
        Task PopulateViewModel();
        void OnDropDownSelected(DropDownEvent dropDownEvent);
        void OnStatuseSelected(DropDownEvent dropDownEvent);
        void OnTaxGroupSelected(DropDownEvent dropDownEvent);
        void OnCurrencySelected(DropDownEvent dropDownEvent);
        void EditContact(Contact.Model.Classes.Contact contact);
        void AddContact();
        void CloseContactTab();
        void AddDocument();
        void EditDocs(Winit.Modules.StoreDocument.Model.Classes.StoreDocument StoreDocument);
    }
}

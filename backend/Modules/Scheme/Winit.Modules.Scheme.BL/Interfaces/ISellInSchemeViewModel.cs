using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ISellInSchemeViewModel : ISchemeViewModelBase
    {
        void CalculateFinalDealerPriceMargin(ISellInSchemeLine sellInSchemeLine);
        ISellInSchemeDTO _SellInSchemeDTO { get; set; }
        List<SKUAttributeDropdownModel>? SKUAttributeData { get; }
        Task PopulateViewModel();
        bool IsDisplayChannelPartner { get; set; }
        string ChannelPartnerName { get; set; }
        bool IsBranchDisabled { get; }
        bool IsExpired { get; set; }
        Task GetSelectedItems(List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> sKULIst);
        void OnProvisionAmount2Enter(ISellInSchemeLine sellInSchemeLine, string Amount);
        void OnProvisionAmount3Enter(ISellInSchemeLine sellInSchemeLine, string Amount);
        void OnInvoiceDiscountEnter(ISellInSchemeLine sellInSchemeLine, string Amount);
        void OnCreditNoteEnter(ISellInSchemeLine sellInSchemeLine, string Amount);

        void OnDealerRequestedPriceEnter(ISellInSchemeLine sellInSchemeLine, string Amount);
        Task<bool> OnAddProduct_Click();
        void IsItemsDiscountValidated(out bool isVal, out string message);
        void ValidateOnAddProduct_Click(out bool isVal, out string message);

    }
}

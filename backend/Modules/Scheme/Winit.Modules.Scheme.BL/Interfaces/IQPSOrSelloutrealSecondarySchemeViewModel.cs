using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface IQPSOrSelloutrealSecondarySchemeViewModel : ISchemeViewModelBase
    {
        List<SKUAttributeDropdownModel>? SKUAttributeData { get; }
        PromoMasterView PromoMasterView { get; set; }
        List<ISelectionItem> OrderTypeDDL { get; set; }
        List<ISelectionItem> SKUGroupTypeDDL { get; set; }
        List<ISelectionItem> SKUGroupDDL { get; set; }
        List<IQPSSchemeProducts> SchemeProducts { get; set; }
        List<ISelectionItem> OfferTypeDDL { get; set; }
        List<ISchemeSlab> SchemeSlabs { get; set; }
        ISchemeSlab SchemeSlab { get; set; }
        List<Winit.Modules.SKU.Model.Interfaces.ISKU> SKUList { get; set; }
        string FreeSKULabel { get; set; }
        string SelectedValueSkuType { get; set; }
        bool IsNoEndDate { get; set; }
        bool IsFOCType { get; set; }
        bool IsInitialize { get; set; }
        bool IsQPSScheme { get; set; }
        bool IsGroupTypeSKU { get; set; }
        Task PopulateViewModel();
        void OnGroupTypeSelected(DropDownEvent dropDownEvent);
        void OnGroupSelected(DropDownEvent dropDownEvent);
        void OnOrderTypeSelected(DropDownEvent dropDownEvent);
        void OnOfferTypeSelected(DropDownEvent dropDownEvent);
        void AddSelectedItems();
        void GetSelectedItems(List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> sKUs);
        void AddSchemeSlab();
        void IsDetailsValidated(out bool isVal, out string message);
        void IsSchemeProductsValidated(out bool isVal, out string message);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public interface ICreateStandingConfigurationViewModel:ISchemeViewModelBase
    {
        bool IsDisable { get; set; }
        List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> SKUList { get; set; }
        List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> ExcludedModels { get; set; }
        List<ISelectionItem> CategoryDDL { get; set; }  
        List<ISelectionItem> TypeDDL { get; set; }
        List<ISelectionItem> StarRatingDDL { get; set; }
        List<ISelectionItem> TonnageDDL { get; set; }
        
        List<ISelectionItem> ProductDivisions { get; set; }
        
        List<ISelectionItem> CapacityDDL { get; set; }
        List<ISelectionItem> ProductGroupDDL { get; set; }
        List<ISelectionItem> SeriesDDL { get; set; }
        IStandingProvisionSchemeMaster StandingProvisionSchemeMaster { get; set; }
        Task PopulateViewModel();
        void OnCatogorySelected(DropDownEvent dropDownEvent);
        void OnTypeSelected(DropDownEvent dropDownEvent);
        void OnStarRatingSelected(DropDownEvent dropDownEvent);
        void OnBroadClassificationSelected(DropDownEvent dropDownEvent);
        void OnBranchSelected(DropDownEvent dropDownEvent);
        void OnTonnageSelected(DropDownEvent dropDownEvent);
        void OnDivisionSelected(DropDownEvent dropDownEvent);
        void OnItemSeriesSelected(DropDownEvent dropDownEvent);
        void OnCapacitySelected(DropDownEvent dropDownEvent);
        void OnGroupSelected(DropDownEvent dropDownEvent);
        void GetSelectedItems(List<ISKUV1> sKUs);
        Task GetProductsOnAddButtonClick();
       
    }
}

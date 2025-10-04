using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface IMaintainStandardPriceListBaseViewModel
    {
         int _currentPage { get; set; }
        int _pageSize { get; set; }
        int _totalItems { get; set; }
        string CodeOrName { get; set; }
        List<ISKUPrice> SKUPriceList { get; set; }
        Winit.Modules.SKU.Model.Interfaces.ISKUPriceList FRPrice { get; set; } 
        List<ISKUPrice> SerchedSKUPriceList { get; set; }
        List<ISKUPrice> sKU { get; set; }
        bool isLoad { get; set; }
        public ISKUAttributeLevel sKUAttributeLevel { get; set; }
        public List<ISelectionItem> AttributeTypeSelectionItems { get; set; }
        public List<ISelectionItem> AttributeNameSelectionItems { get; set; }
        public List<ISelectionItem> ProductDivisionSelectionItems { get; set; }
        void OnSort(SortCriteria criteria);
        Task PopulateViewModel();
        Task OnFilterApply(Dictionary<string, string> filterCriterias);
        Task OnPageChange(int pageNo);
        Task<ISKUAttributeLevel> GetAttributeType();
        Task OnAttributeTypeSelect(string code);
    }
}

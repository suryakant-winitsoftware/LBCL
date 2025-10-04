using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Common;

namespace Winit.Modules.ErrorHandling.BL.Interfaces
{
    public interface IAddEditMaintainErrorViewModel
    {
        public List<ISelectionItem> ServeritySelectionItems { get; set; }
        public List<ISelectionItem> CategorySelectionItems { get; set; }
        public List<ISelectionItem> PlatformSelectionItems { get; set; }
        public List<ISelectionItem> ModuleSelectionItems { get; set; }
        public List<ISelectionItem> SubModuleSelectionItems { get; set; }
        public bool IsEditError { get; set; }
        public IErrorDetail ErrorDetail { get; set; }
        //public DropDown ServerityDropDown { get; set; }
        //public DropDown CategoryDropDown { get; set; }
        //public DropDown PlatformDropDown { get; set; }
        //public DropDown ModuleDropDown { get; set; }
        //public DropDown SubModuleDropDown { get; set; }
        void InstilizeFieldsForEditPage(IErrorDetail sKUMaster);
        Task PopulateViewModel(string errorUID);

        Task<bool> Update();
        Task<bool> Save();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Survey.BL.Interfaces
{
    public interface IStoreQuestionFrequencyViewModel : Winit.Modules.Base.BL.Interfaces.ITableGridViewModel
    {
        public List<IStoreQuestionFrequency> StoreQuestionFrequencyList { get; set; }
        Task GetUsers(string OrgUID);
        Task GetStores_Customers(string orgUID);

        public List<ISelectionItem> EmpSelectionList { get; set; }

        public List<ISelectionItem> Stores_CustSelectionList { get; set; }
        Task PopulateViewModel();
        public List<ISelectionItem> StateselectionItems { get; set; }
        Task GetStates();
        public List<ISelectionItem> RoleSelectionList { get; set; }

        Task GetRoles();
        public bool IsExportClicked { get; set; }
    }
}

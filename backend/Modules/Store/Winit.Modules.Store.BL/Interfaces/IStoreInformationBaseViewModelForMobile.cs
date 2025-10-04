using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreInformationBaseViewModelForMobile
    {
        Winit.Modules.Store.Model.Interfaces.IWeekDays weekDays { get; set; }
        Modules.Store.Model.Interfaces.IStore iStore { get; set; }
        bool isShowCustomersinPopup { get; set; }
        bool isShowCustomerChannelinPopup { get; set; }
        bool isShowCustomerSubChannelinPopup { get; set; }
        bool isShowClassificationinPopUp { get; set; }
        bool isShowChaininPopUp { get; set; }
        bool isShowGroupinPopUp { get; set; }
        bool isShowMODinPopUp { get; set; }
        bool isShowStoreSizeinPopUp { get; set; }
        bool isShowVisitFrequencyinPopUp { get; set; }

        string CustomerTypeLabel { get; set; }
        string CustomerChannelLabel { get; set; }
        string CustomerSubChannelLabel { get; set; }
        string ClassificationLabel { get; set; }
        string ChainLabel { get; set; }
        string CustomerGroupLabel { get; set; }
        string MODLabel { get; set; }
        string StoreSizeLabel { get; set; }
        string VisitFrequencyLabel { get; set; }

        List<Winit.Shared.Models.Common.ISelectionItem> _customerType { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> _channel { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> _subChannel { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> _chain { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> _customerGroup { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> _mOD { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> _storeSize { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> _classification { get; set; }
        List<Winit.Shared.Models.Common.ISelectionItem> _visitFrequency { get; set; }



        Task PopulateViewModel(bool IsNewStore, string UID);
        Task<StoreSavedAlert> Save();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Web.Store;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStorBaseViewModelForWeb
    {
        Model.Classes.Store CustomerInformation { get; set; }
        Winit.Modules.Contact.Model.Interfaces.IContact Contact { get; set; }
        Contact.Model.Interfaces.IContact ContactPerson { get; set; }
        List<Contact.Model.Interfaces.IContact> ContactPersonList { get; set; }
        IAddress BillingAddress { get; set; }
        IAddress ShippingAddress { get; set; }
        List<IAddress> Addresses { get; set; }
        List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> FileSysList { get; set; }
        OrganisationConfigurationModel _OrganisationConfiguration { get; set; }
        AwayPeriod.Model.Interfaces.IAwayPeriod? AwayPeriod { get; set; }
        List<AwayPeriod.Model.Interfaces.IAwayPeriod> AwayPeriodList { get; set; }
        IStoreAdditionalInfo? _StoreAdditionalInfo { get; set; }
        List<ISelectionItem> Route { get; set; }
        string StoreUID { get; set; }
        bool IsNewStore { get; set; }
        bool IsDisabled { get; set; }
        bool IsNewOrganisation { get; set; }
        bool IsInitialize { get; set; }
        List<ListItem> ListItems { get; set; }
        List<ISelectionItem> SalesOrgList { get; set; }
        void ViewOrEditOrganisationConfig(StoreCredit storeCredit);
        StoreGroupData SelectedStoreGroupDataInOrgConfig { get; set; }
        List<StoreCredit>? StoreCreditList { get; set; }
        Task PopulateViewModel();
        (string, string) GetLocationLabelByPrimaryUID(string locationUID);
        void ViewEditShippingAddress(Winit.Modules.Address.Model.Classes.Address address);
    }
}

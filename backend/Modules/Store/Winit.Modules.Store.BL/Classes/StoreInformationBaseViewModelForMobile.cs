using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ListHeader.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreInformationBaseViewModelForMobile : IStoreInformationBaseViewModelForMobile
    {
        IAppUser _appUser { get; set; }
        Winit.Modules.Store.BL.Interfaces.IStoreBL _storeBL { get; set; }
        public Modules.Store.Model.Interfaces.IStore iStore { get; set; }
        Winit.Modules.Store.BL.Interfaces.IStoreAdditionalInfoBL WeekDaysBL { get; set; }
        public Winit.Modules.Store.Model.Interfaces.IWeekDays weekDays { get; set; }
        IListHeaderBL _ListHeaderBL;

        public StoreInformationBaseViewModelForMobile(IAppUser appUser, Winit.Modules.Store.BL.Interfaces.IStoreBL storeBL, Model.Interfaces.IStore iStore, IListHeaderBL ListHeaderBL)
        {
            _appUser = appUser;
            _storeBL = storeBL;
            this.iStore = iStore;
            _ListHeaderBL = ListHeaderBL;
        }

        public UIModels.Common.Store.CustomerInformationModel _CustomerInformation { get; set; } = new Winit.UIModels.Common.Store.CustomerInformationModel();



        // public Modules.Store.Model.Interfaces.IStore iStore { get; set; }

        public string UID { get; set; }
        public bool IsNewStore { get; set; }

        public string SaveOrUpdate { get; set; }

        public bool isShowCustomersinPopup { get; set; } = true;
        public bool isShowCustomerChannelinPopup { get; set; } = true;
        public bool isShowCustomerSubChannelinPopup { get; set; } = true;
        public bool isShowClassificationinPopUp { get; set; } = true;
        public bool isShowChaininPopUp { get; set; } = true;
        public bool isShowGroupinPopUp { get; set; } = true;
        public bool isShowMODinPopUp { get; set; } = true;
        public bool isShowStoreSizeinPopUp { get; set; } = true;
        public bool isShowVisitFrequencyinPopUp { get; set; } = true;

        protected bool Active;
        protected bool InActive;
        protected bool Blocked;
        protected bool Warehouse;
        protected bool School;

        public string CustomerTypeLabel { get; set; } = "Select Customer Type";
        public string CustomerChannelLabel { get; set; } = "Select Customer Channel";
        public string CustomerSubChannelLabel { get; set; } = "Select Customer Sub Channel";
        public string ClassificationLabel { get; set; } = "Select Classification";
        public string ChainLabel { get; set; } = "Select Customer Chain";
        public string CustomerGroupLabel { get; set; } = "Select Customer Group";
        public string MODLabel { get; set; } = "Select Delivery Mode";
        public string StoreSizeLabel { get; set; } = "Select Store Size";
        public string VisitFrequencyLabel { get; set; } = "Select Visit Frequency";

        public string status;
        public bool isShowPop { get; set; } = true;

        private List<Winit.Modules.ListHeader.Model.Classes.ListItem> listItems { get; set; } = new List<Winit.Modules.ListHeader.Model.Classes.ListItem>();
        public List<Winit.Shared.Models.Common.ISelectionItem> _customerType { get; set; } = new();
        public List<Winit.Shared.Models.Common.ISelectionItem> _channel { get; set; } = new();
        public List<Winit.Shared.Models.Common.ISelectionItem> _subChannel { get; set; } = new();
        public List<Winit.Shared.Models.Common.ISelectionItem> _chain { get; set; } = new();
        public List<Winit.Shared.Models.Common.ISelectionItem> _customerGroup { get; set; } = new();
        public List<Winit.Shared.Models.Common.ISelectionItem> _mOD { get; set; } = new();
        public List<Winit.Shared.Models.Common.ISelectionItem> _storeSize { get; set; } = new();
        public List<Winit.Shared.Models.Common.ISelectionItem> _classification { get; set; }= new();
        public List<Winit.Shared.Models.Common.ISelectionItem> _visitFrequency { get; set; } = new();

        public async Task PopulateViewModel(bool IsNewStore, string UID)
        {

            if (IsNewStore)
            {
                SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
                iStore.Number = DateTime.Now.ToString("ddMMyyyyhhmmss");
                iStore.Code = "C" + DateTime.Now.ToString("ddMMyyyyhhmmss");
            }
            else
            {
                SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Update;
                try
                {
                    iStore = await _storeBL.SelectStoreByUID(UID);
                    Active = iStore.Status == Shared.Models.Constants.CustomerStatus.Active ? true : false;
                    InActive = iStore.Status == Shared.Models.Constants.CustomerStatus.InActive ? true : false;
                    Blocked = iStore.Status == Shared.Models.Constants.CustomerStatus.Blocked ? true : false;
                    School = iStore.SchoolWarehouse == Shared.Models.Constants.ListHeaderType.School ? true : false;
                    Warehouse = iStore.SchoolWarehouse == Shared.Models.Constants.ListHeaderType.Warehouse ? true : false;
                    CustomerTypeLabel = string.IsNullOrEmpty(iStore.Type) ? "Select Customer Type" : iStore.Type;
                    weekDays = await WeekDaysBL.SelectWeekDaysByUID(UID);
                }
                catch (Exception ex)
                {

                }
            }


            await getListItemsAsync();
        }

        string ErrorMessage = string.Empty;
        private bool Validate()
        {
            bool isValidated = true;

            if (string.IsNullOrEmpty(iStore.OutletName))
            {
                ErrorMessage += nameof(iStore.OutletName); isValidated = false;
            }
            if (string.IsNullOrEmpty(iStore.ArabicName))
            {
                ErrorMessage += nameof(iStore.ArabicName) + ","; isValidated = false;
            }
            if (string.IsNullOrEmpty(iStore.LegalName))
            {
                ErrorMessage += nameof(iStore.LegalName) + ","; isValidated = false;
            }
            return isValidated;
        }

        public async Task<StoreSavedAlert> Save()
        {
            StoreSavedAlert storeSavedAlert = new();
            if (Validate())
            {
                if (iStore.UID == null || iStore.UID == "")
                {
                    string UID = Guid.NewGuid().ToString();
                    iStore.UID = UID;
                    iStore.CreatedTime = DateTime.Now;
                    iStore.ModifiedTime = DateTime.Now;
                    iStore.ServerAddTime = DateTime.Now;
                    iStore.ServerModifiedTime = DateTime.Now;
                    iStore.CreatedByEmpUID = _appUser.Emp.CreatedBy;
                    iStore.CompanyUID = _appUser.Emp.CreatedBy;
                    iStore.CreatedByJobPositionUID = _appUser.SelectedJobPosition.UID;
                    //iStore.CountryUID = _appUser.Emp.CreatedBy;
                    //iStore.RegionUID = _appUser.Emp.CreatedBy;
                    //iStore.CityUID = _appUser.Emp.CreatedBy;
                    iStore.CreatedBy = _appUser.Emp.CreatedBy;
                    iStore.ModifiedBy = _appUser.Emp.ModifiedBy;
                    var res = await _storeBL.CreateStore(iStore);

                    weekDays.CreatedTime = DateTime.Now;
                    weekDays.ModifiedTime = DateTime.Now;
                    weekDays.ServerAddTime = DateTime.Now;
                    weekDays.ServerModifiedTime = DateTime.Now;
                    weekDays.CreatedBy = _appUser.Emp.CreatedBy;
                    weekDays.ModifiedBy = _appUser.Emp.ModifiedBy;

                    weekDays.UID = Guid.NewGuid().ToString();
                    weekDays.StoreUID = UID;
                    res += await WeekDaysBL.CreateWeekDaysForMobile(weekDays);


                    if (res > 0)
                    {
                        storeSavedAlert.IsSaved = true;
                        storeSavedAlert.Message = "Saved Successfully!";
                        storeSavedAlert.Value = UID;
                    }

                }
                else
                {
                    var res = await _storeBL.UpdateStore(iStore);
                    res += await WeekDaysBL.UpdateWeekDaysForMobile(weekDays);
                    if (res > 0)
                    {
                        storeSavedAlert.IsSaved = true;
                        storeSavedAlert.Message = "Updated Successfully!";
                    }
                }
            }
            else
            {

                storeSavedAlert.IsSaved = false;
                storeSavedAlert.Message = ErrorMessage;

            }
            return storeSavedAlert;
        }



        private async Task getListItemsAsync()
        {
            List<string> codes = new List<string>()
            {
                Winit.Shared.Models.Constants.ListHeaderType.CustomerType,
                Winit.Shared.Models.Constants.ListHeaderType.StoreSize,
                Winit.Shared.Models.Constants.ListHeaderType.ModeOfDelivery,
                Winit.Shared.Models.Constants.ListHeaderType.Group,
                Winit.Shared.Models.Constants.ListHeaderType.Chain,
                Winit.Shared.Models.Constants.ListHeaderType.Classification,
                Winit.Shared.Models.Constants.ListHeaderType.SubChannel,
                Winit.Shared.Models.Constants.ListHeaderType.Channel,
                Winit.Shared.Models.Constants.ListHeaderType.VisitFrequency
            };

            try
            {
                PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem> pagedResponse = await _ListHeaderBL.GetListItemsByCodes(codes, true);

                if (pagedResponse != null)
                {
                    if (pagedResponse.TotalCount > 0)
                    {
                        foreach (var item in pagedResponse.PagedData)
                        {
                            SelectionItem selectionItem = new SelectionItem()
                            {
                                Code = item.Code,
                                UID = item.UID,
                                Label = item.Name,
                                IsSelected = iStore.Type == item.UID ? true : false,
                            };

                            switch (item.Code)
                            {
                                case Winit.Shared.Models.Constants.ListHeaderType.CustomerType:
                                    _customerType.Add(selectionItem);
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.StoreSize:
                                    _storeSize.Add(selectionItem);
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.ModeOfDelivery:
                                    _mOD.Add(selectionItem);
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.Group:
                                    _customerGroup.Add(selectionItem);
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.Chain:
                                    _chain.Add(selectionItem);
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.Classification:
                                    _classification.Add(selectionItem);
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.SubChannel:
                                    _subChannel.Add(selectionItem);
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.VisitFrequency:
                                    _channel.Add(selectionItem);
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.Channel:
                                    _visitFrequency.Add(selectionItem);
                                    break;

                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {

            }


        }

        public void SelectedValueChanged(DropDownEvent dropDownEvent, string type)
        {

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    ChangeSelection(dropDownEvent.SelectionItems.FirstOrDefault(), type);
                }
                else
                {
                    ChangeSelection(new SelectionItem(), type);
                }

            }
            else
            {
                switch (type)
                {
                    case Winit.Shared.Models.Constants.ListHeaderType.CustomerType:

                        isShowCustomersinPopup = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.StoreSize:

                        isShowStoreSizeinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.ModeOfDelivery:

                        isShowMODinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.Group:

                        isShowGroupinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.Chain:

                        isShowChaininPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.Classification:

                        isShowClassificationinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.SubChannel:

                        isShowCustomerSubChannelinPopup = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.VisitFrequency:

                        isShowVisitFrequencyinPopUp = true;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.Channel:

                        isShowCustomerChannelinPopup = true;
                        break;

                }
            }

        }


        public void ChangeSelection(Winit.Shared.Models.Common.ISelectionItem item, string Type)
        {

            switch (Type)
            {
                case Winit.Shared.Models.Constants.ListHeaderType.CustomerType:
                    iStore.Type = item.UID;
                    CustomerTypeLabel = string.IsNullOrEmpty(iStore.Type) ? "Select Customer Type" : item.UID;
                    isShowCustomersinPopup = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.StoreSize:
                    iStore.StoreSize = CommonFunctions.GetDecimalValue(item.Code);
                    StoreSizeLabel = item.Label;
                    isShowStoreSizeinPopUp = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.ModeOfDelivery:
                    MODLabel = item.Label;
                    isShowMODinPopUp = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.Group:
                    CustomerGroupLabel = item.Label;
                    //iStore.Group = e.Value.ToString();
                    isShowGroupinPopUp = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.Chain:
                    //  iStore.ProspectEmpUID = e.Value.ToString();
                    ChainLabel = item.Label;
                    isShowChaininPopUp = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.Classification:
                    //  iStore.ProspectEmpUID = e.Value.ToString();
                    ClassificationLabel = item.Label;
                    isShowClassificationinPopUp = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.SubChannel:
                    CustomerSubChannelLabel = item.Label;
                    //iStore.Suffix = e.Value.ToString();
                    isShowCustomerSubChannelinPopup = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.VisitFrequency:
                    VisitFrequencyLabel = item.Label;
                    isShowVisitFrequencyinPopUp = true;
                    break;
                case Winit.Shared.Models.Constants.ListHeaderType.Channel:
                    CustomerChannelLabel = item.Label;
                    //  iStore.ProspectEmpUID = e.Value.ToString();
                    isShowCustomerChannelinPopup = true;
                    break;

            }
        }
    }
}

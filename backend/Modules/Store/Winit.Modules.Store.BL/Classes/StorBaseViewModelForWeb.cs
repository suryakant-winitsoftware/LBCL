using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIComponents.Common;
using Winit.UIModels.Common.Store;
using Winit.UIModels.Web.Store;

namespace Winit.Modules.Store.BL.Classes
{
    public abstract class StorBaseViewModelForWeb : Interfaces.IStorBaseViewModelForWeb
    {
        Winit.Modules.Common.Model.Interfaces.IDataManager _dataManager;
        private CommonFunctions _commonFunctions { get; set; }
        public StorBaseViewModelForWeb(CommonFunctions commonFunctions, NavigationManager navigationManager,
            Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService,
            IAppUser iAppUser, Winit.UIComponents.Common.Services.ILoadingService _loadingService)
        {
            _dataManager = dataManager;
            _commonFunctions = commonFunctions;
            _iAppUser = iAppUser;
        }
        IAppUser _iAppUser;
        public bool IsInitialize { get; set; }
        public bool IsNewOrganisation { get; set; }
        public Winit.Modules.Store.Model.Classes.Store CustomerInformation { get; set; }
        public Contact.Model.Interfaces.IContact? Contact { get; set; } = new Contact.Model.Classes.Contact();
        public Contact.Model.Interfaces.IContact ContactPerson { get; set; } = new Contact.Model.Classes.Contact();
        public List<Contact.Model.Interfaces.IContact> ContactPersonList { get; set; } = new List<Contact.Model.Interfaces.IContact>();

        public IAddress BillingAddress { get; set; } = new Address.Model.Classes.Address();
        public IAddress ShippingAddress { get; set; } = new Address.Model.Classes.Address();
        public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> FileSysList { get; set; }
        public List<IAddress> Addresses { get; set; } = new List<IAddress>();
        public OrganisationConfigurationModel _OrganisationConfiguration { get; set; } = new();
        protected StoreCredit _StoreCredit;
        public List<StoreCredit>? StoreCreditList { get; set; } = new();
        public List<StoreAttributes>? StoreAttributesList { get; set; } = new();
        public AwayPeriod.Model.Interfaces.IAwayPeriod AwayPeriod { get; set; } = new Winit.Modules.AwayPeriod.Model.Classes.AwayPeriod();
        public List<AwayPeriod.Model.Interfaces.IAwayPeriod> AwayPeriodList { get; set; } = new List<AwayPeriod.Model.Interfaces.IAwayPeriod>();
        public IStoreAdditionalInfo? _StoreAdditionalInfo { get; set; } = new StoreAdditionalInfo();
        public List<ListItem> ListItems { get; set; } = new();
        public List<ISelectionItem> SalesOrgList { get; set; }
        public List<ISelectionItem> Route { get; set; } = new List<ISelectionItem>();
        public string StoreUID { get; set; }
        public bool IsNewStore { get; set; }
        public bool IsDisabled { get; set; }
        List<LocationData> LocationMasterData;
        List<StoreGroupData> StoreGroupMasterData;
        public StoreGroupData SelectedStoreGroupDataInOrgConfig { get; set; }

        public async Task PopulateViewModel()
        {
            object obj = _dataManager.GetData(Common.Model.Constants.CommonConstants.LocationMasterData);
            if (obj != null)
            {
                LocationMasterData = (List<LocationData>)obj;
            }
            object storeGroupData = _dataManager.GetData(Common.Model.Constants.CommonConstants.ChannelMasterData);
            if (storeGroupData != null)
            {
                StoreGroupMasterData = (List<StoreGroupData>)storeGroupData;
            }
            string pagetype = _commonFunctions.GetParameterValueFromURL(PageType.Page);
            if (PageType.New.Equals(pagetype))
            {
                StoreUID = Guid.NewGuid().ToString();
                CustomerInformation = new()
                {
                    IsActive = true,
                    IsNew = true,
                    IsTaxApplicable = true,
                    UID = StoreUID,

                };
                IsDisabled = true;
                IsNewStore = true;
            }
            else
            {
                StoreUID = _commonFunctions.GetParameterValueFromURL("UID");
                await GetCustomerInfo();
                await GetStoreImages();
                await GetAddress();
                IsDisabled = false;
                IsNewStore = false;
            }
            await GetDropDownLists();
            IsInitialize = true;
            _OrganisationConfiguration = CreateOrganisationConfigurationModelInstance();
        }
        protected OrganisationConfigurationModel CreateOrganisationConfigurationModelInstance()
        {
            IsNewOrganisation = true;
            return new OrganisationConfigurationModel()
            {
                ChannelSubChannelLabel = "Select Store group",
                SalesOrgLabel = "Select Sales Organisation",
                PaymentTypeLabel = "Select Payment Type",
                PaymentTermLabel = "Select Payment Term",
                PaymentMethodLabel = "Select Payment Method",
                PreferredPaymentMethodLabel = "Select Preferred Payment Method",
                CustomerClassificationLabel = "Select Customer Classification",
                CustomerGroupLabel = "Select Customer Group",
                ChainLabel = "Select Chain",
            };
        }
        //protected void StoreGroupDataSelected(StoreGroupData storeGroupData)
        //{
        //    //SelectedStoreGroupData = storeGroupData;
        //    //ViewLocation = false;
        //    //_OrganisationConfiguration.ChannelSubChannelLabel = storeGroupData == null ? "Select Store Group" : storeGroupData.Label;

        //    //OnStoreGroupDataSelected.InvokeAsync(storeGroupData);
        //}

        public void ViewEditShippingAddress(Winit.Modules.Address.Model.Classes.Address address)
        {
            var add = GetLocationLabelByPrimaryUID(address.LocationUID);
            address.LocationLabel = add.Item1;
            address.LocationJson = add.Item2;
            ShippingAddress = address;
        }
        public (string, string) GetLocationLabelByPrimaryUID(string locationUID)
        {
            var data = LocationMasterData?.Find(p => p.PrimaryUid == locationUID);
            return (data?.PrimaryLabel, data?.JsonData);
        }
        public (string, string) GetStoreGroupDataLabelByPrimaryUID(string locationUID)
        {
            var data = StoreGroupMasterData?.Find(p => p.PrimaryUID == locationUID);
            return (data?.PrimaryType, data?.JsonData);
        }
        public OrgConfigurationUIModel OrgConfiguration;
        protected void PrepareOrgConfigurationToSaveOrUpdate()
        {
            OrgConfiguration = new OrgConfigurationUIModel()
            {
                StoreAttributes = new(),
                StoreCredit = new(),
            };
            AddStoreCredit();
            if (SelectedStoreGroupDataInOrgConfig != null)
            {
                if (IsNewOrganisation)
                {
                    OrgConfiguration.StoreAttributes.AddRange(DeseriliazeStoreGroupDataFromJson(SelectedStoreGroupDataInOrgConfig.JsonData));
                }
                else
                {
                    if (_StoreCredit.StoreGroupDataUID != SelectedStoreGroupDataInOrgConfig?.PrimaryUID)
                    {
                        _StoreCredit.StoreGroupDataUID = SelectedStoreGroupDataInOrgConfig?.PrimaryUID;
                        OrgConfiguration.StoreAttributes.AddRange(DeseriliazeStoreGroupDataFromJson(SelectedStoreGroupDataInOrgConfig.JsonData, false));
                    }
                }
            }
            OrgConfiguration.StoreAttributes.Add(AddStoreAttributes(_OrganisationConfiguration.SalesOrg, DropDownConstant.CustomerGroup, string.IsNullOrEmpty(_OrganisationConfiguration?.CustomerGroup) ? string.Empty : _OrganisationConfiguration.CustomerGroupLabel, _OrganisationConfiguration.CustomerGroup, UIModels.Web.Store.DropDownConstant.CustomerGroup));
            OrgConfiguration.StoreAttributes.Add(AddStoreAttributes(_OrganisationConfiguration.SalesOrg, DropDownConstant.CustomerClassification, string.IsNullOrEmpty(_OrganisationConfiguration.CustomerClassification) ? string.Empty : _OrganisationConfiguration.CustomerClassificationLabel, _OrganisationConfiguration.CustomerClassification, UIModels.Web.Store.DropDownConstant.CustomerClassification));
            OrgConfiguration.StoreAttributes.Add(AddStoreAttributes(_OrganisationConfiguration.SalesOrg, DropDownConstant.Chain, string.IsNullOrEmpty(_OrganisationConfiguration.CustomerChain) ? string.Empty : _OrganisationConfiguration.ChainLabel, _OrganisationConfiguration.CustomerChain, UIModels.Web.Store.DropDownConstant.Chain));
        }
        protected StoreAttributes AddStoreAttributes(string OrgUID, string code, string name, string value, string ParentName)
        {
            if (!IsNewOrganisation)
            {
                foreach (StoreAttributes storeAttributes1 in StoreAttributesList)
                {
                    if (storeAttributes1.OrgUID == OrgUID && storeAttributes1.Code == code)
                    {
                        storeAttributes1.Name = name;
                        storeAttributes1.Value = value;
                        storeAttributes1.ModifiedBy = _iAppUser.Emp.UID;
                        storeAttributes1.ModifiedTime = DateTime.Now;
                        return storeAttributes1;
                    }
                }
            }

            StoreAttributes storeAttributes = new()
            {
                UID = Guid.NewGuid().ToString(),
                StoreUID = StoreUID,
                CreatedBy = _iAppUser.Emp.UID,
                CompanyUID = _iAppUser.Emp.CompanyUID,
                CreatedTime = DateTime.Now,
                ModifiedBy = _iAppUser.Emp.UID,
                ModifiedTime = DateTime.Now,
                OrgUID = OrgUID,
                Code = code,
                Name = name,
                Value = value,
                ParentName = ParentName,
            };
            StoreAttributesList?.Add(storeAttributes);
            return storeAttributes;
        }
        protected void AddStoreCredit()
        {
            if (IsNewOrganisation)
            {
                _StoreCredit = new StoreCredit();
                _StoreCredit.UID = Guid.NewGuid().ToString();
                _StoreCredit.StoreUID = StoreUID;
                _StoreCredit.CreatedBy = _iAppUser.Emp.UID;
                _StoreCredit.CreatedTime = DateTime.Now;
                _StoreCredit.OrgUID = _OrganisationConfiguration.SalesOrg;
                _StoreCredit.StoreGroupDataUID = SelectedStoreGroupDataInOrgConfig?.PrimaryUID;
            }
            _StoreCredit.PriceList = _OrganisationConfiguration.PriceList;
            _StoreCredit.AuthorizedItemGRPKey = _OrganisationConfiguration.AuthorizedItemGRPKey;
            _StoreCredit.MessageKey = _OrganisationConfiguration.MessageKey;
            _StoreCredit.TaxKeyField = _OrganisationConfiguration.TaxKeyField;
            _StoreCredit.PromotionKey = _OrganisationConfiguration.PromotionKey;
            _StoreCredit.IsActive = _OrganisationConfiguration.IsActive;
            _StoreCredit.CreditLimit = _OrganisationConfiguration.CreditLimit;
            _StoreCredit.OutstandingInvoices = _OrganisationConfiguration.OutstandingInvoices;
            _StoreCredit.PreferredPaymentMethod = _OrganisationConfiguration.PreferredPaymentMethod;
            _StoreCredit.PaymentType = _OrganisationConfiguration.PaymentType;
            _StoreCredit.PreferredPaymentMode = _OrganisationConfiguration.PaymentMethod;
            _StoreCredit.PaymentTermUID = _OrganisationConfiguration.PaymentTerm;
            _StoreCredit.IsBlocked = _OrganisationConfiguration.IsBlocked;
            _StoreCredit.BlockingReasonCode = _OrganisationConfiguration.BlockedReason;
            _StoreCredit.InvoiceAdminFeePerBillingCycle = _OrganisationConfiguration.InvoiceAdminFeePerBillingCycle;
            _StoreCredit.InvoiceAdminFeePerDelivery = _OrganisationConfiguration.InvoiceAdminFeePerDelivery;
            _StoreCredit.InvoiceLatePaymentFee = _OrganisationConfiguration.InvoiceLatePaymentFeePer;
            _StoreCredit.IsCancellationOfInvoiceAllowed = _OrganisationConfiguration.IsCancellationOfInvoiceAllowed;
            _StoreCredit.IsOutstandingBillControl = _OrganisationConfiguration.IsOutStandingBillControl;
            _StoreCredit.IsNegativeInvoiceAllowed = _OrganisationConfiguration.IsNegativeInvoiceAllowed;
            _StoreCredit.IsAllowCashOnCreditExceed = _OrganisationConfiguration.IsAllowCashOnCreditExceed;
            _StoreCredit.ModifiedBy = _iAppUser.Emp.UID;
            _StoreCredit.ModifiedTime = DateTime.Now;
            OrgConfiguration.StoreCredit.Add(_StoreCredit);
        }

        protected List<StoreAttributes> DeseriliazeStoreGroupDataFromJson(string jsonData, bool isNew = true)
        {
            List<StoreAttributes> storeAttributesListFromStoreGroup = new List<StoreAttributes>();
            if (jsonData != null)
            {
                // List<StoreGroupDataFromJson>? storeGroupDataFromJsons = JsonConvert.DeserializeObject<List<StoreGroupDataFromJson>>(jsonData);
                _OrganisationConfiguration.StoreGroupDataFromJsons = _OrganisationConfiguration.StoreGroupDataFromJsons?.OrderBy(p => p.Level).ToList();
                if (_OrganisationConfiguration.StoreGroupDataFromJsons != null)
                {
                    foreach (StoreGroupDataFromJson store in _OrganisationConfiguration.StoreGroupDataFromJsons)
                    {
                        if (isNew)
                        {
                            storeAttributesListFromStoreGroup.Add(new()
                            {
                                UID = Guid.NewGuid().ToString(),
                                StoreUID = StoreUID,
                                CreatedBy = _iAppUser.Emp.UID,
                                CompanyUID = _iAppUser.Emp.CompanyUID,
                                CreatedTime = DateTime.Now,
                                ModifiedBy = _iAppUser.Emp.UID,
                                ModifiedTime = DateTime.Now,
                                OrgUID = _OrganisationConfiguration?.SalesOrg,
                                Code = store.StoreGroupTypeName,
                                Name = store.Label,
                                Value = store.UID,
                                ParentName = store.StoreGroupTypeName,
                            });
                        }
                        else
                        {
                            foreach (StoreAttributes storeAttribute in StoreAttributesList)
                            {
                                if (storeAttribute.OrgUID == _OrganisationConfiguration?.SalesOrg && storeAttribute.ParentName == store.StoreGroupTypeName)
                                {
                                    storeAttribute.Name = store.Label;
                                    storeAttribute.Value = store.UID;
                                    storeAttributesListFromStoreGroup.Add(storeAttribute);
                                    break;
                                }
                            }
                        }

                    }
                }
            }
            return storeAttributesListFromStoreGroup;


        }
        protected void DeserializeStoregroupData()
        {
            _OrganisationConfiguration.StoreGroupDataFromJsons = JsonConvert.DeserializeObject<List<StoreGroupDataFromJson>>(_OrganisationConfiguration.JsonData);
            if (_OrganisationConfiguration.StoreGroupDataFromJsons != null)
            {
                _OrganisationConfiguration.StoreGroupDataFromJsons = _OrganisationConfiguration.StoreGroupDataFromJsons.OrderBy(p => p.Level).ToList();
            }
        }
        public void ViewOrEditOrganisationConfig(StoreCredit storeCredit)
        {
            IsNewOrganisation = false;
            _StoreCredit = storeCredit;
            _OrganisationConfiguration = CreateOrganisationConfigurationModelInstance();
            _OrganisationConfiguration.SalesOrg = storeCredit.OrgUID;
            _OrganisationConfiguration.SalesOrgLabel = SalesOrgList.Find(p => p.UID == storeCredit.OrgUID)?.Label;
            if (!string.IsNullOrEmpty(storeCredit.StoreGroupDataUID))
            {
                _OrganisationConfiguration.ChannelSubChannelUID = storeCredit.StoreGroupDataUID;
                var storeGroup = GetStoreGroupDataLabelByPrimaryUID(storeCredit.StoreGroupDataUID);
                if (!string.IsNullOrEmpty(storeGroup.Item1))
                {
                    _OrganisationConfiguration.ChannelSubChannelLabel = storeGroup.Item1;
                }
                if (!string.IsNullOrEmpty(storeGroup.Item2))
                {
                    _OrganisationConfiguration.JsonData = storeGroup.Item2;
                    DeserializeStoregroupData();
                }
            }
            _OrganisationConfiguration.PriceList = storeCredit.PriceList;

            ListItem? listitemModel = ListItems.Find(p => p.UID == storeCredit.PriceList);
            if (listitemModel != null)
            {
                _OrganisationConfiguration.PriceList = listitemModel.Name;
            }
            _OrganisationConfiguration.AuthorizedItemGRPKey = storeCredit.AuthorizedItemGRPKey;
            _OrganisationConfiguration.MessageKey = storeCredit.MessageKey;
            _OrganisationConfiguration.TaxKeyField = storeCredit.TaxKeyField;
            _OrganisationConfiguration.PromotionKey = storeCredit.PromotionKey;
            _OrganisationConfiguration.IsActive = storeCredit.IsActive;
            _OrganisationConfiguration.BillTo = storeCredit.BillToAddressUID;
            _OrganisationConfiguration.ShipTo = storeCredit.ShipToAddressUID;
            _OrganisationConfiguration.CreditLimit = storeCredit.CreditLimit;
            _OrganisationConfiguration.OutstandingInvoices = storeCredit.OutstandingInvoices;
            _OrganisationConfiguration.PreferredPaymentMethod = storeCredit.PreferredPaymentMethod;
            listitemModel = ListItems.Find(p => p.Code == storeCredit.PreferredPaymentMethod);
            if (listitemModel != null)
            {
                _OrganisationConfiguration.PreferredPaymentMethodLabel = listitemModel.Name;
            }
            if (storeCredit.PaymentType?.ToLower() == "cash")
            {
                _OrganisationConfiguration.IsPaymentTermDisable = true;
            }
            _OrganisationConfiguration.PaymentType = storeCredit.PaymentType;
            listitemModel = ListItems.Find(p => p.Code == storeCredit.PaymentType);
            if (listitemModel != null)
            {
                _OrganisationConfiguration.PaymentTypeLabel = listitemModel.Name;
            }
            _OrganisationConfiguration.PaymentMethod = storeCredit.PreferredPaymentMode;
            if (!string.IsNullOrEmpty(_OrganisationConfiguration.PaymentMethod))
            {
                string[] paymentMenthds = _OrganisationConfiguration.PaymentMethod.Split(",");
                if (paymentMenthds != null && paymentMenthds.Length > 0)
                {
                    if (paymentMenthds.Length == 1)
                    {
                        listitemModel = ListItems.Find(p => p.Code == storeCredit.PreferredPaymentMode);
                        if (listitemModel != null)
                        {
                            _OrganisationConfiguration.PaymentMethodLabel = listitemModel.Name;
                        }
                    }
                    else
                    {
                        _OrganisationConfiguration.PaymentMethodLabel = $"{paymentMenthds.Length} items selected";
                    }
                }
            }

            _OrganisationConfiguration.PaymentTerm = storeCredit.PaymentTermUID;
            listitemModel = ListItems.Find(p => p.Code == storeCredit.PaymentTermUID);
            if (listitemModel != null)
            {
                _OrganisationConfiguration.PaymentTermLabel = listitemModel.Name;
            }
            _OrganisationConfiguration.IsBlocked = storeCredit.IsBlocked;
            _OrganisationConfiguration.BlockedReason = storeCredit.BlockingReasonCode;
            _OrganisationConfiguration.InvoiceAdminFeePerBillingCycle = storeCredit.InvoiceAdminFeePerBillingCycle;
            _OrganisationConfiguration.InvoiceAdminFeePerDelivery = storeCredit.InvoiceAdminFeePerDelivery;
            _OrganisationConfiguration.InvoiceLatePaymentFeePer = storeCredit.InvoiceLatePaymentFee;
            _OrganisationConfiguration.IsCancellationOfInvoiceAllowed = storeCredit.IsCancellationOfInvoiceAllowed;
            _OrganisationConfiguration.IsOutStandingBillControl = storeCredit.IsOutstandingBillControl;
            _OrganisationConfiguration.IsNegativeInvoiceAllowed = storeCredit.IsNegativeInvoiceAllowed;
            _OrganisationConfiguration.IsAllowCashOnCreditExceed = _StoreCredit.IsAllowCashOnCreditExceed;
            foreach (var storeAttribute in StoreAttributesList)
            {
                if (_OrganisationConfiguration.SalesOrg.Equals(storeAttribute.OrgUID))
                {
                    if (UIModels.Web.Store.DropDownConstant.CustomerGroup.Equals(storeAttribute.ParentName))
                    {
                        _OrganisationConfiguration.CustomerGroup = storeAttribute.Value;
                        _OrganisationConfiguration.CustomerGroupLabel = storeAttribute.Name;
                        ListItem? listItem = ListItems.Find(p => p.Code.Equals(storeAttribute.Code));
                        if (listItem != null)
                        {
                            _OrganisationConfiguration.CustomerGroupLabel = listItem.Name;
                        }
                    }
                    if (UIModels.Web.Store.DropDownConstant.CustomerClassification.Equals(storeAttribute.ParentName))
                    {
                        _OrganisationConfiguration.CustomerClassification = storeAttribute.Value;
                        _OrganisationConfiguration.CustomerClassificationLabel = storeAttribute.Name;
                        ListItem? listItem = ListItems.Find(p => p.Code.Equals(storeAttribute.Code));
                        if (listItem != null)
                        {
                            _OrganisationConfiguration.CustomerClassificationLabel = listItem.Name;
                        }
                    }
                    if (UIModels.Web.Store.DropDownConstant.Chain.Equals(storeAttribute.ParentName))
                    {
                        _OrganisationConfiguration.CustomerChain = storeAttribute.Value;
                        _OrganisationConfiguration.ChainLabel = storeAttribute.Name;
                        ListItem? listItem = ListItems.Find(p => p.Code.Equals(storeAttribute.Code));
                        if (listItem != null)
                        {
                            _OrganisationConfiguration.ChainLabel = listItem.Name;
                        }
                    }
                }
            }

        }



        #region abstract methodes
        protected abstract Task GetDropDownLists();
        protected abstract Task GetCustomerInfo();
        protected abstract Task GetStoreImages();
        protected abstract Task GetAddress();

        #endregion
    }
}

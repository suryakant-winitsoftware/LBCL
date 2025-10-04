using Microsoft.AspNetCore.Components;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common;
using Winit.UIModels.Web.Promotions;

namespace Winit.UIComponents.Web.SalesManagement.Promotions
{
    public partial class PromotionItems : ComponentBase
    {
        protected string str1;
        protected string DiscountLabel { get; set; } = "Discount";
        protected bool IsDiscountShow { get; set; } = true;
        [Parameter]
        public string PromoFormat 
        {
            get
            {
                return promoFormat;
            }
            set
            {
                if (value != null)
                {
                    promoFormat = value;
                    SetPromotionItemSetup(value);
                }
            }
        }
        public string promoFormat { get; set; }
        protected bool IsLoad { get; set; }
        List<SKUGroupType> sKUGroupTypes { get; set; } = new();
        List<ISelectionItem> sKUGroupList { get; set; }
        List<ISelectionItem> sKUGroupTypeList { get; set; }
        List<ISelectionItem> selectedItems { get; set; }
        List<SKUGroup> sKUGroup { get; set; } = new();
        string selectedGroupLabel { get; set; } = "Select Group";
        string selectedGroupTypeLabel { get; set; }
        string selectedGroupTypeName { get; set; }
        bool IsGroupTypeVisible { get; set; }
        bool IsGroupVisible { get; set; }
        bool IsShoDropDownPopup { get; set; } = false;
        Winit.Shared.Models.Common.ISelectionItem selectedGroupType { get; set; }
        List<DataGridColumn> Columns { get; set; }
        public int maxdealCount { get; set; }
        public int EligibleQty { get; set; }
        public int Discount { get; set; }
        public string Errormessage { get; set; }
        public string PromotionOrderUID { get; set; } = Guid.NewGuid().ToString();
        List<PromotionItemsModel> promotionItemsModelList { get; set; } = new();


        public List<PromoConditionView> PromoConditionViewList { get; set; }
        protected override async Task OnInitializedAsync()
        {

        }

        protected override void OnInitialized()
        {

            IsLoad = true;
            SetDropDownLists();
            Columns = SetColumnHeaders();
            base.OnInitialized();
        }
        protected void SetDropDownLists()
        {
            List<SelectionItem> sKUGroupTypeList = new();
            sKUGroupTypes = (List<SKUGroupType>)dataManager.GetData(Winit.Shared.Models.Constants.CommonMasterDataConstants.SKUGroupType);
            sKUGroup = (List<SKUGroup>)dataManager.GetData(Winit.Shared.Models.Constants.CommonMasterDataConstants.SKUGroup);
            foreach (var item in sKUGroupTypes)
            {
                sKUGroupTypeList.Add(new SelectionItem() { Code = item.Code, UID = item.UID, Label = item.Name, IsSelected = false });
            }
            this.sKUGroupTypeList = sKUGroupTypeList.ToList<ISelectionItem>();
        }
        protected List<DataGridColumn> SetColumnHeaders()
        {
            return new List<DataGridColumn>
        {
            new DataGridColumn { Header = "Type", GetValue = s => ((PromotionItemsModel)s).GroupTypeName, IsSortable = false, SortField = "Type" },
            new DataGridColumn { Header = "Code", GetValue = s => ((PromotionItemsModel)s).GroupCode, IsSortable = false, SortField = "Code" },
            new DataGridColumn { Header = "Name", GetValue = s => ((PromotionItemsModel)s).GroupName, IsSortable = false, SortField = "Name" },
            new DataGridColumn
            {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType=ButtonTypes.Image,
                        URL = "../images/delete.png",
                        Action = item => Delete((PromotionItemsModel)item)
                    },
                }
            }
        };
        }

        protected void Delete(PromotionItemsModel promotionItemsModel)
        {

        }
        protected void OnGroupTypeSelected(DropDownEvent dropDownEvent)
        {
            List<SelectionItem>  sKUGroupList = new List<SelectionItem>();

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null)
                {
                    selectedGroupType = dropDownEvent.SelectionItems.FirstOrDefault();
                    selectedGroupTypeLabel = selectedGroupType.Label;
                    foreach (var item in sKUGroup)
                    {
                        if (item.SKUGroupTypeUID.Equals(selectedGroupType.UID))
                        {
                            sKUGroupList.Add(new Shared.Models.Common.SelectionItem() { Code = item.Code, UID = item.UID, Label = item.Name, IsSelected = false });
                        }
                    }
                    this.sKUGroupList = sKUGroupList.ToList<ISelectionItem>();
                }
                else
                {
                    selectedGroupTypeLabel = string.Empty;
                }
            }
            else
            {
                selectedGroupTypeLabel = string.Empty;
            }
            IsGroupTypeVisible = false;

        }
        protected void OnGroupItemsSelected(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null)
                {
                    selectedItems = dropDownEvent.SelectionItems;
                    selectedGroupLabel = selectedItems.Count >= 1 ? (selectedItems.Count == 1 ? selectedItems.FirstOrDefault().Label : $"{selectedItems.Count} items selected") : "Select Group";
                }
            }

            IsGroupVisible = false;
        }
        protected void AddSelectedItems()
        {
            if (IsValidate())
            {
                foreach (ISelectionItem ISelectionItem in selectedItems == null ? new() : selectedItems)
                {
                    PromotionItemsModel model = promotionItemsModelList.Where(p => p.GroupUID == ISelectionItem.UID).ToList().FirstOrDefault();

                    if (model == null)
                    {
                        ISelectionItem.IsSelected = false;
                        promotionItemsModelList.Add(new PromotionItemsModel() { GroupTypeUID = selectedGroupType.UID, GroupTypeName = selectedGroupType.Label, GroupUID = ISelectionItem.UID, GroupCode = ISelectionItem.Code, GroupName = ISelectionItem.Label });
                    }
                }
            }
            else
            {
                alert.ShowErrorAlert("Error", Errormessage);
            }
            selectedGroupLabel = "Select Group ";
            selectedItems = null;
        }
        private bool IsValidate()
        {
            bool isVal = true;
            Errormessage = "The following field(s) have invalid value(s) :";

            if (maxdealCount == 0)
            {
                isVal = false;
                Errormessage += nameof(maxdealCount) + " ,";
            }
            if (EligibleQty == 0)
            {
                Errormessage += nameof(EligibleQty) + " ,";
            }
            if (isVal == false)
            {
                selectedItems = null;
                selectedGroupLabel = "Select group ,";
                Errormessage = Errormessage.Substring(0, Errormessage.Length - 2);
                Errormessage += "should be greater than 0";
            }

            if ((selectedItems == null || selectedItems.Count == 0) && isVal)
            {
                isVal = false;
                Errormessage += "Select group";
            }

            return isVal;
        }
        public Validation IsPromotionItemsValidated()
        {
            bool isVal = true;
            Errormessage = "The following field(s) have invalid value(s) :";

            if (maxdealCount == 0)
            {
                isVal = false;
                Errormessage += nameof(maxdealCount) + " ,";
            }
            if (EligibleQty == 0)
            {
                Errormessage += nameof(EligibleQty) + " ,";
            }
            if (isVal == false)
            {
                selectedItems = null;
                selectedGroupLabel = "Select group ,";
                Errormessage = Errormessage.Substring(0, Errormessage.Length - 2);
                Errormessage += "should be greater than 0";
            }

            if ((promotionItemsModelList == null || promotionItemsModelList.Count == 0) && isVal)
            {
                isVal = false;
                Errormessage += "Select group";
            }
            return new Validation(isVal, Errormessage);
        }
        public void GetRefresh(string str)
        {
            str1 = str;
        }
        public void SetPromotionItemSetup(string PromotionFormatUID)
        {
            DiscountLabel = "Discount";
            IsDiscountShow = true;
            switch (PromotionFormatUID)
            {
                case "IQFD":
                    break;
                case "IQPD":
                    DiscountLabel = "Discount Percentage";
                    break;
                case "IQXF":
                    IsDiscountShow = false;
                    break;
                case "3":
                    break;
                case "4":
                    break;
            }
            // StateHasChanged();
        }

        public List<PromoOrderView> SavePromotionOrder(string PromotionUID)
        {
            List<PromoOrderView> promoOrderView = new() { new PromoOrderView
            {
                UID=PromotionOrderUID,
                PromotionUID=PromotionUID,
                ActionType=Shared.Models.Enums.ActionType.Add,
                MaxDealCount=maxdealCount,
                QualificationLevel="",
                ModifiedBy="",
                ModifiedTime=DateTime.Now,
                ServerAddTime=DateTime.Now,
                ServerModifiedTime=DateTime.Now,
            }};

            return promoOrderView;
        }

        public List<PromoOrderItemView> SavePromoOrderItems(string PromotionUID)
        {
            PromoConditionViewList = new();
            List<PromoOrderItemView> promoOrderItemViews = new();
            foreach (PromotionItemsModel item in promotionItemsModelList)
            {
                string PromoOrderItemUID = Guid.NewGuid().ToString();
                promoOrderItemViews.Add(new PromoOrderItemView
                {
                    UID= PromoOrderItemUID,
                    PromoOrderUID = PromotionOrderUID,
                    ItemCriteriaType=item.GroupTypeName,
                    ItemCriteriaSelected = item.GroupCode,
                    ActionType = Shared.Models.Enums.ActionType.Add,
                    CreatedBy = _iAppUser.Emp.CreatedBy,
                    ModifiedBy = _iAppUser.Emp.ModifiedBy,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                });


                PromoConditionViewList.Add(
                    new PromoConditionView() {
                        UID = Guid.NewGuid().ToString(),
                        ReferenceUID= PromoOrderItemUID,
                        ReferenceType="PromoOrderItem",
                        Min=EligibleQty,
                        CreatedBy = _iAppUser.Emp.CreatedBy,
                        ModifiedBy = _iAppUser.Emp.ModifiedBy,
                        ModifiedTime = DateTime.Now,
                        ServerAddTime = DateTime.Now,
                        ServerModifiedTime = DateTime.Now,
                    });
            }
            return promoOrderItemViews;
        }

    }
}

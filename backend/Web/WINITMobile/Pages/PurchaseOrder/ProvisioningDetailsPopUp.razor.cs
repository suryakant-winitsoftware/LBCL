using Microsoft.AspNetCore.Components;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.PurchaseOrder
{
    public partial class ProvisioningDetailsPopUp : BaseComponentBase
    {
        [Parameter]
        public decimal? Margin { get; set; }
        public decimal DuplicatedMargin { get; set; }

        [Parameter]
        public decimal EffectiveUnitPrice { get; set; }
        [Parameter]
        public decimal MinSellingPrice { get; set; }
        [Parameter]
        public List<IPurchaseOrderLineProvision> PurchaseOrderLineProvisions { get; set; }
        private List<IPurchaseOrderLineProvision> DuplicatedPurchaseOrderLineProvisions = new List<IPurchaseOrderLineProvision>();
        [Parameter]
        public EventCallback OnCloseClick { get; set; }

        [Parameter]
        public EventCallback<List<IPurchaseOrderLineProvision>> OnUpdateClick { get; set; }

        [Parameter]
        public EventCallback<List<IPurchaseOrderLineProvision>> OnProvisionChangeClick { get; set; }

        [Parameter]
        public bool DisableFields { get; set; }


        [Parameter]
        public decimal Qty { get; set; }

        private bool IsAllSelected
        {
            get
            {
                return DuplicatedPurchaseOrderLineProvisions.Any() && !DuplicatedPurchaseOrderLineProvisions.Exists(e => !e.IsSelected);
            }
        }



        private async Task OnUpdateBtnClick()
        {
            foreach (IPurchaseOrderLineProvision purchaseOrderLineProvision in PurchaseOrderLineProvisions)
            {
                if (purchaseOrderLineProvision.IsSelected) continue;
                var duplicatedProvision = DuplicatedPurchaseOrderLineProvisions.Find(dp => (dp.ProvisionType == purchaseOrderLineProvision.ProvisionType && dp.ProvisionType != ProvisionTypeConst.StandingScheme)
                    || (dp.ProvisionType == ProvisionTypeConst.StandingScheme && dp.SchemeCode == purchaseOrderLineProvision.SchemeCode));
                duplicatedProvision.IsSelected = false;
            }
            PurchaseOrderLineProvisions.Clear();
            PurchaseOrderLineProvisions.AddRange(DuplicatedPurchaseOrderLineProvisions);
            await OnUpdateClick.InvokeAsync(PurchaseOrderLineProvisions);
        }

        private void OnCheckBoxClicked(ChangeEventArgs args, string schemeName)
        {
            bool value = (bool)args.Value;
            var selectedProvison = DuplicatedPurchaseOrderLineProvisions.Find(e => e.ProvisionType == schemeName);
            var actualProvison = PurchaseOrderLineProvisions.Find(e => e.ProvisionType == schemeName);
            if (selectedProvison == null) return;
            switch (schemeName)
            {
                case ProvisionTypeConst.SellInCnP1:
                    selectedProvison.IsSelected = value;
                    if (value)
                    {
                        // OnProvisionChangeClick.InvokeAsync(DuplicatedPurchaseOrderItem);
                        selectedProvison.ApprovedProvisionUnitAmount = actualProvison.ApprovedProvisionUnitAmount;
                    }
                    else
                    {
                        selectedProvison.ApprovedProvisionUnitAmount = default;
                    }
                    break;
                case ProvisionTypeConst.SellInP2:
                    selectedProvison.IsSelected = value;
                    if (value)
                    {
                        // OnProvisionChangeClick.InvokeAsync(DuplicatedPurchaseOrderItem);
                        selectedProvison.ApprovedProvisionUnitAmount = actualProvison.ApprovedProvisionUnitAmount;
                    }
                    else
                    {
                        selectedProvison.ApprovedProvisionUnitAmount = default;
                    }
                    break;
                case ProvisionTypeConst.SellInP3:
                    selectedProvison.IsSelected = value;
                    if (value)
                    {
                        // OnProvisionChangeClick.InvokeAsync(DuplicatedPurchaseOrderItem);
                        selectedProvison.ApprovedProvisionUnitAmount = actualProvison.ApprovedProvisionUnitAmount;
                    }
                    else
                    {
                        selectedProvison.ApprovedProvisionUnitAmount = default;
                    }
                    break;
                case ProvisionTypeConst.CashDiscount:
                    selectedProvison.IsSelected = value;
                    if (value)
                    {
                        // OnProvisionChangeClick.InvokeAsync(DuplicatedPurchaseOrderItem);
                        selectedProvison.ApprovedProvisionUnitAmount = actualProvison.ApprovedProvisionUnitAmount;
                    }
                    else
                    {
                        selectedProvison.ApprovedProvisionUnitAmount = default;
                    }
                    break;
                case ProvisionTypeConst.P2QPS:
                    selectedProvison.IsSelected = value;
                    if (value)
                    {
                        // OnProvisionChangeClick.InvokeAsync(DuplicatedPurchaseOrderItem);
                        selectedProvison.ApprovedProvisionUnitAmount = actualProvison.ApprovedProvisionUnitAmount;
                    }
                    else
                    {
                        selectedProvison.ApprovedProvisionUnitAmount = default;
                    }
                    break;
                case ProvisionTypeConst.P3QPS:

                    selectedProvison.IsSelected = value;
                    if (value)
                    {
                        // OnProvisionChangeClick.InvokeAsync(DuplicatedPurchaseOrderItem);
                        selectedProvison.ApprovedProvisionUnitAmount = actualProvison.ApprovedProvisionUnitAmount;
                    }
                    else
                    {
                        selectedProvison.ApprovedProvisionUnitAmount = default;
                    }
                    break;
                case ProvisionTypeConst.StandingScheme:
                    selectedProvison.IsSelected = value;
                    if (value)
                    {
                        // OnProvisionChangeClick.InvokeAsync(DuplicatedPurchaseOrderItem);
                        selectedProvison.ApprovedProvisionUnitAmount = actualProvison.ApprovedProvisionUnitAmount;
                    }
                    else
                    {
                        selectedProvison.ApprovedProvisionUnitAmount = default;
                    }
                    break;
            }
            ApplyMargin(DuplicatedPurchaseOrderLineProvisions);
            StateHasChanged();
        }

        private void OnStandingSchemeChanged(ChangeEventArgs args, IPurchaseOrderLineProvision selectedProvison)
        {
            bool value = (bool)args.Value;
            selectedProvison.IsSelected = value;
            var actualProvison = PurchaseOrderLineProvisions.Find(e => e.SchemeCode == selectedProvison.SchemeCode);
            if (value)
            {
                // OnProvisionChangeClick.InvokeAsync(DuplicatedPurchaseOrderItem);
                selectedProvison.ApprovedProvisionUnitAmount = actualProvison.ApprovedProvisionUnitAmount;
            }
            else
            {
                selectedProvison.ApprovedProvisionUnitAmount = default;
            }
            ApplyMargin(DuplicatedPurchaseOrderLineProvisions);
            StateHasChanged();
        }

        private void OnSelectAllClick(ChangeEventArgs args)
        {
            bool value = (bool)args.Value;
            DuplicatedPurchaseOrderLineProvisions.ForEach(e =>
            {
                if (e.ProvisionType == ProvisionTypeConst.StandingScheme)
                {
                    OnStandingSchemeChanged(args, e);
                    return;
                }
                OnCheckBoxClicked(args, e.ProvisionType);
            });

            ApplyMargin(DuplicatedPurchaseOrderLineProvisions);
            StateHasChanged();
        }

        protected async override Task OnInitializedAsync()
        {

            if (PurchaseOrderLineProvisions == null)
            {
                DuplicatedPurchaseOrderLineProvisions = new();
            }
            else
            {
                DuplicatedPurchaseOrderLineProvisions = PurchaseOrderLineProvisions.Select(e => e.DeepCopy(typeof(PurchaseOrderLineProvision))).ToList();
            }
        }
        private void ApplyMargin(List<IPurchaseOrderLineProvision> purchaseOrderLineProvisions)
        {
            // var billingPriceAfterProvision = purchaseOrderItemView.EffectiveUnitPrice - purchaseOrderItemView.SellInCnP1UnitValue
            //     - (purchaseOrderItemView.SellInP2Amount / purchaseOrderItemView.FinalQty) - (purchaseOrderItemView.SellInP3Amount / purchaseOrderItemView.FinalQty)
            //     - (purchaseOrderItemView.P2QPSTotalValue ?? 0 / purchaseOrderItemView.FinalQty) - (purchaseOrderItemView.P3QPSTotalValue ?? 0 / purchaseOrderItemView.FinalQty) - (purchaseOrderItemView.CashDiscountValue / purchaseOrderItemView.FinalQty) - (purchaseOrderItemView.P3StandingAmount / purchaseOrderItemView.FinalQty);

            var billingPriceAfterProvision = EffectiveUnitPrice - purchaseOrderLineProvisions.Sum(e =>
            {
                // if (ProvisionTypeConst.SellInCnP1 != e.ProvisionType)
                // {
                //     return e.ApprovedProvisionUnitAmount / (Qty == 0 ? 1 : Qty);
                // }
                return e.ApprovedProvisionUnitAmount;
            });

            Margin = billingPriceAfterProvision - MinSellingPrice;
        }

    }
}

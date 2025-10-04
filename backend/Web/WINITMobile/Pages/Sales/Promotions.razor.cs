using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Common;
using Winit.Modules.Promotion.Model.Classes;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Sales;

partial class Promotions:BaseComponentBase
{
    [Parameter]
    public Dictionary<string, DmsPromotion> PromotionsHeaderDict { get; set; } = new Dictionary<string, DmsPromotion>();
    [Parameter]
    public Dictionary<string, List<ISelectionItem>> PromotionItems { get; set; } = new Dictionary<string, List<ISelectionItem>>();
    [Parameter]
    public EventCallback OnPromotionsCloseClick { get; set; }
    [Parameter]
    public EventCallback<string> OnPromotionsOrderClick { get; set; }
    public bool ShowPromotionPopUp { get; set; }
    private DmsPromotion? SelectedPromotion = null;
    private List<ISelectionItem>? SelectedPromoItems;


    protected override async Task OnInitializedAsync()
    {
        //PopulateData();
    }
    private async Task Handle_ProceedClick(DmsPromotion dmsPromotion)
    {
        await OnPromotionsOrderClick.InvokeAsync(dmsPromotion.UID);
    }
    private void PopulateData()
    {
        PromotionsHeaderDict["Promo1"] = new DmsPromotion
        {
            UID = "Promo1",
            ValidFrom = DateTime.Now.AddDays(-20),
            ValidUpto = DateTime.Now.AddMonths(6),
            Code = "",
            Name = "Buy 10 quantity and get 2 quantity free"
        };
        PromotionsHeaderDict["Promo2"] = new DmsPromotion
        {
            UID = "Promo2",
            ValidFrom = DateTime.Now.AddDays(-200),
            ValidUpto = DateTime.Now.AddMonths(5),
            Code = "",
            Name = "Buy 1 Box and get 20% Discount"
        };
        PromotionsHeaderDict["Promo3"] = new DmsPromotion
        {
            UID = "Promo3",
            ValidFrom = DateTime.Now.AddDays(-30),
            ValidUpto = DateTime.Now.AddMonths(20),
            Code = "",
            Name = "Buy 10 quantity and get 2 quantity free"
        };
        PromotionsHeaderDict["Promo4"] = new DmsPromotion
        {
            UID = "Promo4",
            ValidFrom = DateTime.Now.AddDays(-20),
            ValidUpto = DateTime.Now.AddMonths(23),
            Code = "",
            Name = "Buy 1 Box and get 20% Discount"
        };
        PromotionItems["Promo1"] = new List<ISelectionItem> {
            new SelectionItem
            {
                UID = "sku1",
                Code = "Code1",
                Label= $"[Code1]SKU1",
                ExtData = "/Images/n_img.png"
            },new SelectionItem
            {
                UID = "sku2",
                Code = "Code2",
                Label= $"[Code2]SKU2",
                ExtData = "/Images/n_img.png"
            },new SelectionItem
            {
                UID = "sku3",
                Code = "Code3",
                Label= $"[Code3]SKU3",
                ExtData = "/Images/n_img.png"
            },new SelectionItem
            {
                UID = "sku4",
                Code = "Code4",
                Label= $"[Code4]SKU4",
                ExtData = "/Images/n_img.png"
            }
        };
        PromotionItems["Promo2"] = new List<ISelectionItem> {
            new SelectionItem
            {
                UID = "sku1",
                Code = "Code1",
                Label= $"[Code1]SKU1",
                ExtData = "/Images/n_img.png"
            },new SelectionItem
            {
                UID = "sku2",
                Code = "Code2",
                Label= $"[Code2]SKU2",
                ExtData = "/Images/n_img.png"
            },new SelectionItem
            {
                UID = "sku3",
                Code = "Code3",
                Label= $"[Code3]SKU3",
                ExtData = "/Images/n_img.png"
            }
        };
        PromotionItems["Promo3"] = new List<ISelectionItem> {
            new SelectionItem
            {
                UID = "sku1",
                Code = "Code1",
                Label= $"[Code1]SKU1",
                ExtData = "/Images/n_img.png"
            },new SelectionItem
            {
                UID = "sku2",
                Code = "Code2",
                Label= $"[Code2]SKU2",
                ExtData = "/Images/n_img.png"
            },new SelectionItem
            {
                UID = "sku3",
                Code = "Code3",
                Label= $"[Code3]SKU3",
                ExtData = "/Images/n_img.png"
            }
        };
    }
    private void HandleMoreClick(DmsPromotion dmsPromotion, List<ISelectionItem> promoItems)
    {
        SelectedPromotion = dmsPromotion;
        SelectedPromoItems = promoItems;
        ShowPromotionPopUp = true;
    }
}


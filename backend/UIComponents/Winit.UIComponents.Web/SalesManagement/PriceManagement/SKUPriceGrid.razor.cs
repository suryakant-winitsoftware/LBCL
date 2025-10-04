using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.UIComponents.Web.SalesManagement.PriceManagement
{
    public partial class SKUPriceGrid : ComponentBase
    {
        [Parameter]
        public string PriceListUID { get; set; }

        [Parameter]
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> SKUPriceList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> DisplaySKUPriceList { get; set; }

        [Parameter]
        public EventCallback<List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SKUPriceListSend { get; set; }



        [Parameter]
        public bool IsNew { get; set; }
        [Parameter] public EventCallback<SortCriteria> OnSort { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> ModifiedSkuPriceList { get; set; }
        Winit.UIComponents.Web.DialogBox.ProductDialogBox<Winit.Modules.SKU.Model.Interfaces.ISKUV1>? AddProductDialogBox { get; set; }
        private int count = 0;
        private bool _addProducts = false;
        private bool isItemSearched = false;

        private string currentSortField;
        private bool isAscending = true;
        private void SortColumn(string column)
        {

            if (currentSortField == column)
            {
                isAscending = !isAscending;
            }
            else
            {
                currentSortField = column;
                isAscending = true;
            }
            SortCriteria sortCriteria = new SortCriteria(currentSortField, isAscending ? SortDirection.Asc : SortDirection.Desc);
            OnSort.InvokeAsync(sortCriteria);
        }
        private IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> _data = new List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>()
        {
            new Winit.Modules.SKU.Model.Classes.SKUPrice()
            {
            SKUCode="Code",
            ValidFrom=DateTime.Now,
            ValidUpto=DateTime.Now,
            } , new Winit.Modules.SKU.Model.Classes.SKUPrice()
            {

            SKUCode="Code1",
            ValidFrom=DateTime.Now,
            ValidUpto=DateTime.Now,
            }
        };
        string Message = string.Empty;
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            await _skuPriceViewModel.GetAllProducts();
        }
        [Parameter] public int PageNumber { get; set; }
        [Parameter] public int PageSize { get; set; }
        [Parameter] public int TotalItemsCount { get; set; }
        [Parameter] public bool ShowRecordsCount { get; set; }
        private string ViewingMessage
        {

            get
            {
                int startRecord = 0;
                int endRecord = 0;
                if (TotalItemsCount == 0)
                {
                    return $"You are viewing {startRecord}-{endRecord} out of {TotalItemsCount}";
                }
                else
                {
                    startRecord = ((PageNumber - 1) * PageSize) + 1;
                    endRecord = Math.Min(PageNumber * PageSize, TotalItemsCount);
                    return $"You are viewing {startRecord}-{endRecord} out of {TotalItemsCount}";
                }
            }

        }
        public void ChangeDuplicate(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice)
        {
            if (sKUPrice.Id == 0 || SKUPriceList.Any(p => p.Id == 0 && p.SKUUID == sKUPrice.SKUUID))
            {
                _alertService.ShowErrorAlert("Alert", "Please Save this Sku then try again");
                return;
            }
            _loadingService.ShowLoading();
            Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice1 = new Winit.Modules.SKU.Model.Classes.SKUPrice()
            {
                ActionType = Winit.Shared.Models.Enums.ActionType.Add,
                SKUCode = sKUPrice.SKUCode,
                SKUName = sKUPrice.SKUName,
                UOM = sKUPrice.UOM,
                SKUUID = sKUPrice.SKUUID,
                SKUPriceListUID = sKUPrice.SKUPriceListUID,
                Price = sKUPrice.Price,
                DefaultWSPrice = sKUPrice.DefaultWSPrice,
                DefaultRetPrice = sKUPrice.DefaultWSPrice,
                CreatedBy = sKUPrice.CreatedBy,
                ModifiedBy = sKUPrice.ModifiedBy,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                PriceUpperLimit = sKUPrice.PriceUpperLimit,
                PriceLowerLimit = sKUPrice.PriceLowerLimit,
                Status = sKUPrice.Status,
                ValidFrom = DateTime.Now.AddDays(1),
                ValidUpto = DateTime.MaxValue,
                TempValidFrom = DateTime.Now.AddDays(1),
                TempValidUpto = DateTime.MaxValue,
                IsEdit = true,
                UID = Guid.NewGuid().ToString(),
                IsModified = true,
                ISDuplicate = false,
                ISNew = true,
                IsLatest = 1,
            };


            int indexSKUPriceList = SKUPriceList.IndexOf(sKUPrice);

            sKUPrice.IsModified = true;
            sKUPrice.ISDuplicate = true;
            sKUPrice.IsLatest = 0;

            sKUPrice.ModifiedTime = DateTime.Now;
            sKUPrice.TempValidUpto = DateTime.Now;
            sKUPrice.ValidUpto = DateTime.Now;
            sKUPrice.ActionType = Winit.Shared.Models.Enums.ActionType.Update;
            if (isItemSearched)
            {
                int indexSearchedItems = DisplaySKUPriceList.IndexOf(sKUPrice);
                DisplaySKUPriceList.Insert(indexSearchedItems + 1, sKUPrice1);
            }


            SKUPriceList.Insert(indexSKUPriceList + 1, sKUPrice1);
            StateHasChanged();
            _loadingService.HideLoading();
        }


        protected void EditSkuDetails(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice)
        {
            _loadingService.ShowLoading();
            sKUPrice.TempDummyPrice = sKUPrice.DummyPrice;
            sKUPrice.TempMRP = sKUPrice.MRP;
            sKUPrice.TempValidFrom = sKUPrice.ValidFrom;
            sKUPrice.TempValidUpto = sKUPrice.ValidUpto;
            sKUPrice.ActionType = Winit.Shared.Models.Enums.ActionType.Update;
            sKUPrice.IsModified = true;

            sKUPrice.IsEdit = true;
            _loadingService.HideLoading();
        }

        protected async Task DeleteSKUPrice(Winit.Modules.SKU.Model.Interfaces.ISKUPrice price)
        {
            _loadingService.ShowLoading();
            if (price.Id != 0)
            {
                count = await _skuPriceViewModel.SaveUpdateOrDelete($"DeleteSKUPrice?UID={price.UID}", HttpMethod.Delete);
            }
            if (count > 0 || price.Id == 0)
            {
                await _alertService.ShowSuccessAlert(@Localizer["sucess"], @Localizer["deleted_successfully"]);
                SKUPriceList.Remove(price);
                ISKUPrice? price1 = SKUPriceList.Find(p => p.ISDuplicate && p.SKUUID.Equals(price.SKUUID));
                if (price1 != null && price1.ISDuplicate)
                {
                    price1.ValidUpto = price.ValidUpto;
                }
            }
            StateHasChanged();
            _loadingService.HideLoading();
        }




        protected void GetSelectedItems(List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> sKUs)
        {
            string Message = string.Empty;
            if (sKUs != null)
            {
                foreach (var sku in sKUs)
                {
                    Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKU = SKUPriceList.Find(p => p.SKUCode == sku.Code);
                    if (sKU == null)
                    {

                        SKUPriceList.Insert(0, new Winit.Modules.SKU.Model.Classes.SKUPrice()
                        {
                            UID = Winit.Shared.Models.Constants.UIDType.SKU_UID_Type + Guid.NewGuid().ToString(),
                            SKUUID = sku.UID,
                            SKUPriceListUID = PriceListUID,
                            ActionType = Winit.Shared.Models.Enums.ActionType.Add,
                            IsModified = true,
                            SKUCode = sku.Code,
                            SKUName = sku.Name,
                            ValidFrom = DateTime.Now,
                            TempValidFrom = DateTime.Now,
                            ValidUpto = DateTime.MaxValue,
                            TempValidUpto = DateTime.MaxValue,
                            IsEdit = true,
                            IsLatest = 1,
                            ISNew = true,
                            CreatedBy = _iAppUser.Emp.UID,
                            ModifiedBy = _iAppUser.Emp.UID,
                            CreatedTime = DateTime.Now,
                            ModifiedTime = DateTime.Now,
                            ServerAddTime = DateTime.Now,
                            ServerModifiedTime = DateTime.Now,

                        });
                    }
                    else
                    {
                        Message += sku.Code + " ,";
                    }
                }
            }
            if (!string.IsNullOrEmpty(Message))
            {
                Message = Message.Substring(0, Message.Length - 2);
                _alertService.ShowErrorAlert(@Localizer["error"], Message + @Localizer["items_already_exists"]);
            }
            _addProducts = false;
        }



        public bool Validate(Winit.Modules.SKU.Model.Interfaces.ISKUPrice item, out string message)
        {
            message = @Localizer["the_following_sku(s)_having_invalid_price"] + ":";
            bool val = true;
            if (item.IsEdit && item.ISNew)
            {
                if (CommonFunctions.GetDecimalValue(item.MRP) == 0 && item.IsLatest == 1)
                {
                    message += $"MRP, ";
                    val = false;
                }
                if (CommonFunctions.GetDecimalValue(item.DummyPrice) == 0 && item.IsLatest == 1)
                {
                    message += $"Dummy Price,  ";
                    val = false;
                }
                if (CommonFunctions.GetDecimalValue(item.PriceLowerLimit) == 0 && item.IsLatest == 1)
                {
                    message += $"Minimum Selling Price  ";
                    val = false;
                }
            }
            else
            {

            }
            if (!val)
            {
                _alertService.ShowErrorAlert(@Localizer["error"], message.Substring(0, message.Length - 2));
            }
            return val;
        }

        public async Task SaveOrUpdateIndividual(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice, bool isSave)
        {
            _loadingService.ShowLoading();
            if (isSave)
            {
                sKUPrice.MRP = sKUPrice.TempMRP;
                sKUPrice.DummyPrice = sKUPrice.TempDummyPrice;
                sKUPrice.PriceLowerLimit = sKUPrice.TempPriceLowerLimit;

                sKUPrice.ValidFrom = sKUPrice.TempValidFrom;
                sKUPrice.ValidUpto = sKUPrice.TempValidUpto;
                sKUPrice.ModifiedTime = DateTime.Now;
                bool isValidated = Validate(sKUPrice, out Message);
                if (isValidated)
                {
                    bool isSuccess = false;
                    if (CommonFunctions.GetBooleanValue(sKUPrice.IsLatest))
                    {
                        var response = await _skuPriceViewModel.CUD_SKUPrice("CreateSKUPrice", HttpMethod.Post, sKUPrice);
                        if (response != null && response.IsSuccess)
                        {
                            ISKUPrice? oldSKUPrice = SKUPriceList.Find(p => p.ISDuplicate && p.SKUUID == sKUPrice.SKUUID);
                            if (oldSKUPrice != null)
                            {
                                response = await _skuPriceViewModel.CUD_SKUPrice("UpdateSKUPrice", HttpMethod.Put, oldSKUPrice);
                            }
                            if (sKUPrice.Id == 0)
                            {
                                sKUPrice.Id = SKUPriceList.Count;
                            }
                            isSuccess = response != null && response.IsSuccess;
                        }
                    }
                    else
                    {
                        var response = await _skuPriceViewModel.CUD_SKUPrice("UpdateSKUPrice", HttpMethod.Put, sKUPrice);
                        isSuccess = response != null && response.IsSuccess;
                    }
                    if (isSuccess)
                    {
                        await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["saved_successfully"]);
                        sKUPrice.IsEdit = false;
                        sKUPrice.IsModified = false;
                    }
                }
            }
            else
            {
                if (SKUPriceList.Any(p => p.Id == 0 && p.SKUUID == sKUPrice.SKUUID))
                {
                    await _alertService.ShowErrorAlert("Error", "You can't Cancel the Unsaved SKU!");
                }
                else
                {
                    sKUPrice.IsEdit = false;
                    sKUPrice.IsModified = false;
                }
            }
            _loadingService.HideLoading();
        }

        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
    }
}

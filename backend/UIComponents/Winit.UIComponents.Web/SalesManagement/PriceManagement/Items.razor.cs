using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.UIModels.Common.GridState;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;
using Newtonsoft.Json;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.SKU.Model.Classes;
using Winit.UIModels.Web.SKU;
using Winit.UIModels.Common;
using Nest;
using System.Globalization;
using System.Resources;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using DocumentFormat.OpenXml.Wordprocessing;
using Winit.Modules.SKU.Model.Interfaces;




namespace Winit.UIComponents.Web.SalesManagement.PriceManagement
{
    public partial class Items : ComponentBase
    {

        [Parameter]
        public string PriceListUID { get; set; }
        [Parameter]
        public bool IsIndividualPricelist { get; set; }

        [Parameter]
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> SKUPriceList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> DisplaySKUPriceList { get; set; }

        [Parameter]
        public EventCallback<List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>> SKUPriceListSend { get; set; }

        [Parameter]
        public Winit.Modules.SKU.Model.Interfaces.ISKUPriceList SKUPriceGroup { get; set; }

        [Parameter]
        public bool IsNew { get; set; }
        [Parameter] public EventCallback<SortCriteria> OnSort { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> ModifiedSkuPriceList { get; set; }
        Winit.UIComponents.Web.DialogBox.ProductDialogBox<Winit.Modules.SKU.Model.Interfaces.ISKUV1>? AddProductDialogBox {  get; set; }
        private int count = 0;
        private bool _addProducts = false;
        private bool isItemSearched = false;
        //private IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> _data = new List<Winit.Modules.SKU.Model.Classes.SKUPrice>()
        //{
        //    new Winit.Modules.SKU.Model.Classes.SKUPrice()
        //    {
        //    SKUCode="Code",
        //    ValidFrom=DateTime.Now,
        //    ValidUpto=DateTime.Now,
        //    } , new Winit.Modules.SKU.Model.Classes.SKUPrice()
        //    {

        //    SKUCode="Code1",
        //    ValidFrom=DateTime.Now,
        //    ValidUpto=DateTime.Now,
        //    }
        //};
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
                UID = Winit.Shared.Models.Constants.UIDType.SKU_UID_Type + Guid.NewGuid().ToString(),
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

        protected async void SaveIndividual(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice, bool isSave)
        {
            if (isSave)
            {
                sKUPrice.Price = sKUPrice.DummyPrice;
                sKUPrice.ValidFrom = sKUPrice.TempValidFrom;
                sKUPrice.ValidUpto = sKUPrice.TempValidUpto;

                await SKUPriceListSend.InvokeAsync(SKUPriceList);

                sKUPrice.IsEdit = false;
            }
            else
            {
                sKUPrice.IsEdit = false;
            }
        }

        protected void EditSkuDetails(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice)
        {
            _loadingService.ShowLoading();
            sKUPrice.TempPrice = sKUPrice.Price;
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
                count = await _skuPriceViewModel.SaveUpdateOrDelete($"DeleteSKUPrice?UID={price.UID}", HttpMethod.Delete, null);
            }
            if (count > 0 || price.Id == 0)
            {
                await _alertService.ShowSuccessAlert(@Localizer["sucess"], @Localizer["deleted_successfully"]);
                SKUPriceList.Remove(price);
            }
            StateHasChanged();
            _loadingService.HideLoading();
        }

        protected async Task AddNewProducts(bool isNew)
        {
            _loadingService.ShowLoading();
            if (isNew)
            {
                await _skuPriceViewModel.GetAllProducts();
                _addProducts = true;
            }
            _loadingService.HideLoading();

        }

        protected void GetSelectedItems(List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> sKUs)
        {
            string Message = string.Empty;
            if (sKUs != null)
            {
                foreach (var sku in sKUs)
                {
                    Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKU = SKUPriceList?.Find(p => p.SKUCode == sku.Code);
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

        public Validation GetValidated()
        {
            bool isValidated = true;
            string errorMessage = string.Empty;
            ModifiedSkuPriceList = new();
            foreach (Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice in SKUPriceList)
            {
                if (sKUPrice.IsModified = true)
                {
                    sKUPrice.Price = sKUPrice.DummyPrice;
                    if (sKUPrice.Price > 0)
                    {
                        if (isValidated)
                        {
                            ModifiedSkuPriceList.Add(sKUPrice);
                        }
                    }
                    else
                    {
                        isValidated = false;
                        errorMessage += $"{sKUPrice.SKUCode} ,";
                    }
                }
            }
            if (!isValidated)
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = $"{@Localizer["the_following_sku(s)_having_invalid_price"]}:{errorMessage.Substring(0, errorMessage.Length - 2)}!";
                }
            }
            return new Validation(isValidated, errorMessage);
        }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> GetModifiedSKUPricesList(string status)
        {
            List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> modifiedPriceList = new();
            foreach (Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice in SKUPriceList)
            {
                sKUPrice.Status = status;
                if (sKUPrice.IsModified)
                {
                    sKUPrice.Price = sKUPrice.TempPrice;
                    sKUPrice.MRP = sKUPrice.TempMRP;
                    sKUPrice.DefaultWSPrice = sKUPrice.TempDefaultWSPrice;
                    sKUPrice.DefaultRetPrice = sKUPrice.TempDefaultRetPrice;
                    sKUPrice.ValidFrom = sKUPrice.TempValidFrom;
                    sKUPrice.ValidUpto = sKUPrice.TempValidUpto;
                    modifiedPriceList.Add(sKUPrice);
                }
            }
            return modifiedPriceList;
        }

        public bool Validate(List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> sKUs)
        {
            Message = @Localizer["the_following_sku(s)_having_invalid_price"] + ":";
            bool val = true;
            if (!IsIndividualPricelist)
            {
                if (string.IsNullOrEmpty(SKUPriceGroup.Code))
                {
                    val = false;
                    Message += @Localizer["code"] + ", ";
                }
                if (string.IsNullOrEmpty(SKUPriceGroup.Name))
                {
                    val = false;
                    Message += @Localizer["name"] + ", ";
                }
                if (SKUPriceGroup.Priority == 0)
                {
                    val = false;
                    Message += @Localizer["priority"];
                }
            }


            foreach (ISKUPrice item in sKUs)
            {
                if(item.IsEdit && item.ISNew)
                {
                    if (CommonFunctions.GetDecimalValue(item.Price) == 0 && item.IsLatest == 1)
                    {
                        Message += $"{@Localizer["price_for"]} {item.SKUCode} {@Localizer["sku_code"]} ";
                        val = false;
                    }
                    if (CommonFunctions.GetDecimalValue(item.MRP) == 0 && item.IsLatest == 1)
                    {
                        Message += $"{@Localizer["cost_price"]} {item.SKUCode} {@Localizer["cost_price"]} ";
                        val = false;
                    }
                    if (CommonFunctions.GetDecimalValue(item.DefaultWSPrice) == 0 && item.IsLatest == 1)
                    {
                        Message += $"{@Localizer["ws_price"]} {item.SKUCode} {@Localizer["ws_price"]} ";
                        val = false;
                    }
                    if (CommonFunctions.GetDecimalValue(item.DefaultRetPrice) == 0 && item.IsLatest == 1)
                    {
                        Message += $"{@Localizer["rtl_price"]} {item.SKUCode} {@Localizer["rtl_price"]} ";
                        val = false;
                    }
                }
                else
                {

                }
                
            }

            if (!val)
            {
                _alertService.ShowErrorAlert(@Localizer["error"], Message);
            }
            return val;
        }
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
        public async Task SaveOrUpdateIndividual(Winit.Modules.SKU.Model.Interfaces.ISKUPrice sKUPrice, bool isSave)
        {
            _loadingService.ShowLoading();
            if (isSave)
            {
                sKUPrice.Price = sKUPrice.TempPrice;
                sKUPrice.MRP = sKUPrice.TempMRP;
                sKUPrice.DefaultWSPrice = sKUPrice.TempDefaultWSPrice;
                sKUPrice.DefaultRetPrice = sKUPrice.TempDefaultRetPrice;
                sKUPrice.ValidFrom = sKUPrice.TempValidFrom;
                sKUPrice.ValidUpto = sKUPrice.TempValidUpto;
                sKUPrice.ModifiedTime = DateTime.Now;

                List<Winit.Modules.SKU.Model.Interfaces.ISKUPrice> sKUPriceList = sKUPrice.IsLatest==1? 
                    SKUPriceList.FindAll(p => p.SKUUID == sKUPrice.SKUUID): new List<Modules.SKU.Model.Interfaces.ISKUPrice>() { sKUPrice };
                if (Validate(sKUPriceList))
                {
                    int count = 0;

                    //List<SKUPrice> priceList = SKUPriceList.FindAll(p => p.SKUUID == sKUPrice.SKUUID);
                    if (CommonFunctions.GetBooleanValue(sKUPrice.IsLatest))
                    {
                        if (IsNew)
                        {
                            Winit.Modules.SKU.Model.Interfaces.ISKUPriceView sKUPriceViewDTO = new Winit.Modules.SKU.Model.Classes.SKUPriceView();
                            sKUPriceViewDTO.SKUPriceGroup = SKUPriceGroup;
                            sKUPriceViewDTO.SKUPriceList = sKUPriceList.ToList<Winit.Modules.SKU.Model.Interfaces.ISKUPrice>();
                            count = await _skuPriceViewModel.SaveUpdateOrDelete("CreateSKUPriceView", HttpMethod.Post, sKUPriceViewDTO);
                            if (count > 0)
                            {
                                IsNew = false;
                            }
                        }
                        else
                        {
                            count = await _skuPriceViewModel.SaveUpdateOrDelete("UpdateSKUPriceList", HttpMethod.Put, sKUPriceList);
                        }
                    }
                    else
                    {
                        //sKUPrice.IsLatest = 1;
                        count = await _skuPriceViewModel.SaveUpdateOrDelete("UpdateSKUPrice", HttpMethod.Put, sKUPrice);
                    }
                    if (count > 0)
                    {
                        await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["saved_successfully"]);
                        sKUPrice.IsEdit = false;
                        sKUPrice.IsModified = false;
                        if (sKUPrice.Id == 0)
                        {
                            sKUPrice.Id = SKUPriceList.Count;
                        }


                    }
                }
            }
            else
            {
                sKUPrice.IsEdit = false;
                sKUPrice.IsModified = false;
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


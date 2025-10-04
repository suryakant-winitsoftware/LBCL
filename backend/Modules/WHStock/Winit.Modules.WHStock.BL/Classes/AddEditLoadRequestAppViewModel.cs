using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.StockUpdater.BL.Interfaces;
using Winit.Modules.StockUpdater.Model.Interfaces;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.WHStock.BL.Classes
{
    public class AddEditLoadRequestAppViewModel : AddEditLoadRequestBaseViewModel
    {
        public ISKUBL _ISKUBL { get; set; }
        List<string> orgUIDs = new List<string>();
        List<string> DistributionChannelUIDs = new List<string>();

        List<string> skuUIDs = new List<string>();
        List<string> attributeTypes = new List<string>();
        public AddEditLoadRequestAppViewModel(IServiceProvider serviceProvider,
                 IFilterHelper filter,
                 ISortHelper sorter,
                 IListHelper listHelper,
                 IAppUser appUser,
                 IWHStockBL iWHStockBL,
                   ISKUBL sKUBL,
                 Winit.Shared.Models.Common.IAppConfig appConfigs,
                 IRouteBL iRouteBL,
                 Base.BL.ApiService apiService,
                 Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting,
                 ISKUBL iSKUBL,
                 StockUpdater.BL.Interfaces.IStockUpdaterBL stockUpdaterBL)
            : base(serviceProvider, filter, sorter, listHelper, appUser, iWHStockBL, sKUBL, appConfigs, iRouteBL, apiService, appSetting, stockUpdaterBL)
        {
            _ISKUBL = iSKUBL;
        }
        public override async Task PopulateViewModel(string apiParam = null)
        {
            SelectedRoute = _appUser.SelectedRoute;
            OrgUID = _appUser.SelectedJobPosition.OrgUID;
            await GetLoadRequestByUID(apiParam);
            if (WHStockRequestItemview.UID != null && WHStockRequestItemview.Status == StockRequestStatus.Processed)
            { PrepareSignatureFields(); }

        }
        public override async Task GetSKUMasterData()
        {
            try
            {


                if (_IWHStockBL != null)
                {
                    var pagedResponse = await _ISKUBL.PrepareSKUMaster(
                        orgUIDs, DistributionChannelUIDs, skuUIDs, attributeTypes);

                    if (pagedResponse != null)
                    {
                        SkuMasterData = ConvertToSKUMasterData(pagedResponse);
                        await FetchSKUAttributes(SkuMasterData);
                        await FetchSKUUOMs(SkuMasterData);
                        await FetchSKUs(SkuMasterData);

                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        public static List<SKUMasterData> ConvertToSKUMasterData(List<ISKUMaster> iskuMasters)
        {
            List<SKUMasterData> skumasterDataList = new List<SKUMasterData>();

            foreach (var iskumaster in iskuMasters)
            {
                SKUMasterData skumasterData = new SKUMasterData
                {
                    SKU = (Winit.Modules.SKU.Model.Classes.SKU)iskumaster.SKU,
                    SKUAttributes = iskumaster.SKUAttributes.Cast<Winit.Modules.SKU.Model.Classes.SKUAttributes>().ToList(),
                    SKUUOMs = iskumaster.SKUUOMs.Cast<Winit.Modules.SKU.Model.Classes.SKUUOM>().ToList(),
                    //ApplicableTaxUIDs = iskumaster.ApplicableTaxUIDs,
                    //SKUConfigs = iskumaster.SKUConfigs.Cast<Winit.Modules.SKU.Model.Classes.SKUConfig>().ToList(),
                    //customSKUFields = iskumaster.customSKUFields.Cast<Winit.Modules.CustomSKUField.Model.Classes.CustomSKUFields>().ToList()
                };

                skumasterDataList.Add(skumasterData);
            }

            return skumasterDataList;
        }

        public override async Task<bool> CUDWHStock(string btnText)
        {
            try
            {
                await UpdateQuantities(btnText);
                if (pendingLoadRequestTemplateModel.WHStockRequest == null)
                {
                    return false;
                }
                if (pendingLoadRequestTemplateModel.WHStockRequest.Status == StockRequestStatus.Collected)
                {
                    List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>? wHStockLedgers = ConvertToWHStockLedger(pendingLoadRequestTemplateModel.WHStockRequest, pendingLoadRequestTemplateModel.WHStockRequestLines);
                    if (wHStockLedgers != null && wHStockLedgers.Count > 0)
                    {
                        pendingLoadRequestTemplateModel.WHStockLedgerList = wHStockLedgers.Cast<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>().ToList();
                    }
                }
                await _IWHStockBL.CUDWHStock(pendingLoadRequestTemplateModel);
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger? ConvertToWHStockLedger(WHStockRequest wHStockRequest, WHStockRequestLine whStockRequestLine)
        {
            string warehouseUID = string.Empty;
            int type = 1;
            if (wHStockRequest.RequestType == Enum.GetName(Winit.Shared.Models.Enums.RequestType.Load))
            {
                warehouseUID = wHStockRequest.TargetWHUID;
            }
            else
            {
                warehouseUID = wHStockRequest.SourceWHUID;
                type = -1;
            }
            Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger wHStockLedger = _serviceProvider.GetRequiredService<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>();
            if (wHStockLedger == null)
            {
                return wHStockLedger;
            }
            wHStockLedger.Id = 0;
            wHStockLedger.UID = Guid.NewGuid().ToString();
            wHStockLedger.SS = 0;
            wHStockLedger.CreatedBy = _appUser.SelectedJobPosition.EmpUID;
            wHStockLedger.CreatedTime = DateTime.Now;
            wHStockLedger.ModifiedBy = _appUser.SelectedJobPosition.EmpUID;
            wHStockLedger.ModifiedTime = DateTime.Now;
            wHStockLedger.ServerAddTime = DateTime.Now;
            wHStockLedger.ServerModifiedTime = DateTime.Now;
            wHStockLedger.CompanyUID = wHStockRequest.CompanyUID;
            wHStockLedger.WarehouseUID = warehouseUID;
            wHStockLedger.OrgUID = wHStockRequest.SourceOrgUID;
            wHStockLedger.SKUUID = whStockRequestLine.SKUUID;
            wHStockLedger.SKUCode = whStockRequestLine.SKUCode;
            wHStockLedger.Type = type;
            wHStockLedger.ReferenceType = LinkedItemType.WHStockRequestStock;
            wHStockLedger.ReferenceUID = whStockRequestLine.UID; // Later it will be taken from wh_stock_request_stock table
            wHStockLedger.BatchNumber = _appUser.DefaultBatchNumber; // Hardcoded for now
            wHStockLedger.ExpiryDate = null;
            wHStockLedger.Qty = whStockRequestLine.CollectedQty;
            wHStockLedger.UOM = whStockRequestLine.UOM;
            wHStockLedger.StockType = wHStockRequest.StockType;
            wHStockLedger.SerialNo = null;
            wHStockLedger.VersionNo = _appUser.DefaultStockVersion;
            wHStockLedger.ParentWhUID = null;
            wHStockLedger.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
            return wHStockLedger;
        }
        public virtual List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>? ConvertToWHStockLedger(WHStockRequest wHStockRequest, List<WHStockRequestLine> wHStockRequestLineList)
        {
            if (wHStockRequest == null || wHStockRequestLineList == null || wHStockRequestLineList.Count == 0)
            {
                return null;
            }
            List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>? wHStockLedgerList = null;
            if (wHStockRequestLineList != null && wHStockRequestLineList.Count > 0)
            {
                wHStockLedgerList = new List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>();
                foreach (WHStockRequestLine wHStockRequestLine in wHStockRequestLineList)
                {
                    Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger? wHStockLedger = ConvertToWHStockLedger(wHStockRequest, wHStockRequestLine);
                    if (wHStockLedger == null)
                    {
                        continue;
                    }
                    wHStockLedgerList.Add(wHStockLedger);
                }
            }
            return wHStockLedgerList;
        }

        public async Task GetRouteByOrgUID(string OrgUID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();

                if (_IRouteBL != null)
                {
                    var pagedResponse = await _IRouteBL.SelectRouteAllDetails(
                        pagingRequest?.SortCriterias,
                        pagingRequest?.PageNumber ?? 1,
                        pagingRequest?.PageSize ?? 10,
                        pagingRequest?.FilterCriterias,
                        pagingRequest?.IsCountRequired ?? false,
                        OrgUID
                    );
                    if (pagedResponse.PagedData != null)
                    {
                        RouteList = pagedResponse.PagedData.ToList();

                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public List<WHStockRequestLineItemViewUI> WHStockRequestItemviewui { get; set; }
        public async Task GetLoadRequestByUID(string UID)
        {
            try
            {
                if (_IWHStockBL != null)
                {
                    var pagedResponse = await _IWHStockBL.SelectLoadRequestDataByUID(UID);

                    if (pagedResponse != null)
                    {
                        WHStockRequestItemview = pagedResponse.WHStockRequest;
                        DisplayWHStockRequestLineItemview = await ConvertLoadRequestDataToUIModel(pagedResponse.WHStockRequestLines);

                    }
                }
                else
                {
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

        }

        public async Task<List<IWHStockRequestLineItemViewUI>> ConvertLoadRequestDataToUIModel(List<IWHStockRequestLineItemView> iViewLoadRequestItemView)
        {
            List<IWHStockRequestLineItemViewUI> viewLoadRequestItemViewUI = new List<IWHStockRequestLineItemViewUI>();

            if (iViewLoadRequestItemView != null)
            {
                foreach (var wHStockRequestLines in iViewLoadRequestItemView)
                {
                    var whStockRequestLineItemViewUI = new WHStockRequestLineItemViewUI
                    {
                        ApprovedQty = wHStockRequestLines.ApprovedQty,
                        ApprovedQty1 = wHStockRequestLines.ApprovedQty1,
                        CollectedQty = wHStockRequestLines.CollectedQty,
                        ApprovedQty2 = wHStockRequestLines.ApprovedQty2,
                        SKUUID = wHStockRequestLines.SKUUID,
                        SKUName = wHStockRequestLines.SKUName,
                        CollectedQty1 = wHStockRequestLines.CollectedQty1,
                        SKUCode = wHStockRequestLines.SKUCode,
                        CollectedQty2 = wHStockRequestLines.CollectedQty2,
                        CPEApprovedQty = wHStockRequestLines.CPEApprovedQty,
                        CPEApprovedQty1 = wHStockRequestLines.CPEApprovedQty1,
                        CPEApprovedQty2 = wHStockRequestLines.CPEApprovedQty2,
                        ForwardQty = wHStockRequestLines.ForwardQty,
                        ForwardQty1 = wHStockRequestLines.ForwardQty1,
                        ForwardQty2 = wHStockRequestLines.ForwardQty2,
                        RequestedQty = wHStockRequestLines.RequestedQty,
                        RequestedQty1 = wHStockRequestLines.RequestedQty1,
                        RequestedQty2 = wHStockRequestLines.RequestedQty2,
                        TemplateQty1 = wHStockRequestLines.TemplateQty1,
                        TemplateQty2 = wHStockRequestLines.TemplateQty2,
                        IsSelected = false,
                        UID = wHStockRequestLines.UID,
                        UOM = wHStockRequestLines.UOM,
                        UOM1 = wHStockRequestLines.UOM1,
                        UOM2 = wHStockRequestLines.UOM2,
                        UOM2CNF = wHStockRequestLines.UOM2CNF,
                        LineNumber = wHStockRequestLines.LineNumber,
                        OrgUID = wHStockRequestLines.OrgUID,
                        WareHouseUID = wHStockRequestLines.WareHouseUID,
                        YearMonth = wHStockRequestLines.YearMonth
                    };
                    viewLoadRequestItemViewUI.Add((IWHStockRequestLineItemViewUI)whStockRequestLineItemViewUI);
                }
            }
            //viewLoadRequestItemViewUI = viewLoadRequestItemViewUI.OrderBy(item => item.LineNumber).ToList();

            return viewLoadRequestItemViewUI;
        }

    }
}

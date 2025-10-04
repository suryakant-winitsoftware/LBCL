
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;

using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;

using Winit.Shared.Models.Enums;
using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Modules.CustomSKUField.Model.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Nest;
using Winit.Modules.WHStock.DL.Interfaces;
using System.Security.Cryptography;
using Winit.Modules.Setting.Model.Interfaces;
using Microsoft.AspNetCore.Components;


namespace Winit.Modules.WHStock.BL.Classes
{
    public class LoadRequestBaseViewModel : ILoadRequestView
    {
        public List<FilterCriteria> FilterCriterias { get; set; }
        public WHStockRequestItemView WHStockRequest { get; set; }
        public List<IWHStockRequestItemViewUI> DisplayWHStockRequestItemView { get; set; }

        public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteList { get; set; }
       
        public List<ISelectionItem> DisplayRequestToDDL { get; set; }

        public List<ISelectionItem> DisplayRequestFromDDL { get; set; }
        public Winit.Modules.Route.BL.Interfaces.IRouteBL _IRouteBL { get; set; }
        public Winit.Modules.WHStock.BL.Interfaces.IWHStockBL _IWHStockBL { get; set; }
        
        public IServiceProvider _serviceProvider;

        public  IFilterHelper _filter;

        public  ISortHelper _sorter;

      //  private ITaxCalculator _taxCalculator;

        private readonly IListHelper _listHelper;

       // IEnumerable<ISKUMaster> SKUMasterList;

        protected readonly IAppUser _appUser;

        private List<string> _propertiesToSearch = new List<string>();

        public Winit.Shared.Models.Common.IAppConfig _appConfigs;

        public Winit.Modules.Base.BL.ApiService _apiService;

        Winit.Modules.Org.Model.Classes.Org GetOrgList = new Org.Model.Classes.Org { };

        public List<Winit.Modules.Route.Model.Classes.Route> RouteDataByOrgUid = new List<Route.Model.Classes.Route> { };

        public bool CPEApprovalRequired = true;

        public bool ERPApprovalRequired = true;
        private ISKUBL _ISKUBL { get; set; }

        public WHRequestTempleteModel WHRequestTempletemodel { get; set; }

        public List<ISelectionItem> RouteListForSelection { get; set; }
      
        public List<WHStockRequestLineItemViewUI> SelectedWHStockRequestLineItemViewUI { get; set; }

        //Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSetting;
        public LoadRequestBaseViewModel(IServiceProvider serviceProvider,

      IFilterHelper filter,
      ISortHelper sorter,
      IListHelper listHelper,
      IAppUser appUser,
      IWHStockBL iWHStockBL,
      ISKUBL sKUBL,
      IAppConfig appConfigs,
      IRouteBL iRouteBL,
      Base.BL.ApiService apiService,
      Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting)
        {

            _filter = filter;
            _sorter = sorter;
            _serviceProvider = serviceProvider;
            _listHelper = listHelper;
            _apiService = apiService; 
            _appConfigs = appConfigs;
            _appUser = appUser;
            _IWHStockBL = iWHStockBL;
            _ISKUBL = sKUBL;
            _IRouteBL = iRouteBL;
          //_appSetting = appSetting;
            _propertiesToSearch.Add("SKUCode");
            _propertiesToSearch.Add("SKUName");
        }

        public virtual async Task PopulateViewModel(string apiParam = null)
        {
            throw new NotImplementedException();

        }
        public virtual async Task GetRoutesByOrgUID(string apiParam = null)
        {
            throw new NotImplementedException();

        }
        
        public virtual async Task GetVehicleDropDown()
        {
            throw new NotImplementedException();
        }

        public virtual async Task GetRequestFromDropDown()
        {
            throw new NotImplementedException();
        }
        public virtual async Task ApplyFilter(List<FilterCriteria> filterCriterias, string ActiveTab)
        {
            throw new NotImplementedException();

        }
        //public async Task<List<Winit.Modules.SKU.Model.Classes.SKUMasterData>> GetSKUDataFromAPIAsync()
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest();
        //        pagingRequest.PageSize = 10;
        //        pagingRequest.PageNumber = 1;
        //        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
        //            $"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData",

        //            HttpMethod.Post, pagingRequest);

        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {
        //            string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
        //            PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData> selectionSKUs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>>(data);
        //            if (selectionSKUs.PagedData != null)
        //            {
        //                SkuMasterData = selectionSKUs.PagedData.ToList();

        //                SkuAttributesList = await FindCompleteSKUAttributes(SkuMasterData);
        //                SkuUOMList = await FindCompleteSKUUOM(SkuMasterData);
        //                SkuList = await FindCompleteSKU(SkuMasterData);
        //                RouteList = await GetRouteAPIAsync("FR001");
        //                OrgsList = await GetORGByUIDAPIAsync("FR001");
        //                BtnSelectRoute();
        //            }
        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        // Handle exceptions
        //    }
        //    return SkuMasterData;
        //}
        //protected async Task BtnSelectRoute()
        //{


        //    if (RouteListForSelection == null)
        //    {
        //        RouteListForSelection = new List<ISelectionItem>();
        //    }

        //    if (RouteList == null)
        //    {
        //        // Handle the case when TemplateRouteList is null
        //        return;
        //    }

        //    else if (RouteListForSelection.Count == 0)
        //    {
        //        foreach (var route in RouteList)
        //        {
        //            RouteListForSelection.Add(new SelectionItem { UID = route.UID, IsSelected = false, Code = route.Code, Label = route.Name });
        //        }
        //    }
        //}

        //public async Task<List<Winit.Modules.Route.Model.Classes.Route>> GetRouteAPIAsync(string OrgUID)
        //{
        //    try
        //    {


        //        PagingRequest pagingRequest = new PagingRequest();
        //        //pagingRequest.PageSize = 10;
        //        //pagingRequest.PageNumber = 1;
        //        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
        //            $"{_appConfigs.ApiBaseUrl}Route/SelectAllRouteDetails?OrgUID={OrgUID}",
        //            HttpMethod.Post, pagingRequest);

        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {
        //            string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
        //            PagedResponse<Winit.Modules.Route.Model.Classes.Route> fetchApiData = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Route.Model.Classes.Route>>(data);
        //            if (fetchApiData.PagedData != null)
        //            {
        //                RouteDataByOrgUid = fetchApiData.PagedData.ToList();
        //                return RouteDataByOrgUid;
        //            }
        //        }


        //    }

        //    catch (Exception ex)
        //    {
        //        // Handle exceptions
        //    }
        //    return null;
        //}

        //public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKU>> FindCompleteSKU(IEnumerable<SKUMasterData> sKUMasters)
        //{
        //    var skuList = new List<Winit.Modules.SKU.Model.Interfaces.ISKU>(); // Declare the list here

        //    try
        //    {
        //        if (sKUMasters != null)
        //        {
        //            foreach (var skuMaster in sKUMasters)
        //            {

        //                skuList.Add(skuMaster.SKU);

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return skuList;
        //}

        //public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM>> FindCompleteSKUUOM(IEnumerable<SKUMasterData> sKUMasters)
        //{
        //    var skuUOMList = new List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM>(); // Declare the list here

        //    try
        //    {
        //        if (sKUMasters != null)
        //        {
        //            foreach (var skuMaster in sKUMasters)
        //            {
        //                foreach (var skuUOM in skuMaster.SKUUOMs)
        //                {
        //                    skuUOMList.Add(skuUOM);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle the exception
        //    }

        //    return skuUOMList;
        //}
        //public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>> FindCompleteSKUAttributes(IEnumerable<SKUMasterData> sKUMasters)
        //{
        //    var skuAttributesList = new List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>(); // Declare the list here

        //    try
        //    {
        //        if (sKUMasters != null)
        //        {
        //            foreach (var skuMaster in sKUMasters)
        //            {
        //                foreach (var skuAttributes in skuMaster.SKUAttributes)
        //                {
        //                    skuAttributesList.Add(skuAttributes);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message.ToString());
        //    }

        //    return skuAttributesList;
        //}


        //public async Task<Winit.Modules.Org.Model.Classes.Org> GetORGByUIDAPIAsync(string uid)
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest();
        //        pagingRequest.PageSize = 10;
        //        pagingRequest.PageNumber = 1;

        //        ApiResponse<Winit.Modules.Org.Model.Classes.Org> apiResponse =
        //        await _apiService.FetchDataAsync<Winit.Modules.Org.Model.Classes.Org>(
        //        $"{_appConfigs.ApiBaseUrl}Org/GetOrgByUID?UID={uid}",
        //        HttpMethod.Get);

        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        //        {

        //            return apiResponse.Data;
        //        }


        //    }

        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message.ToString());
        //    }
        //    return null;
        //}

        //public async Task<bool> CreateDateStock(string Remark, string status, string SelectedRouteCodeDate, string SelectedRouteUID, List<WHStockRequestLineItemViewUI> SelectedWHStockRequestLineItemViewUI, bool isMobile = false)
        //{
        //    if(SelectedWHStockRequestLineItemViewUI != null)
        //    {
        //        SelectedWHStockRequestLineItemViewUI.Reverse();
        //    }
        //    try
        //    {



        //        WHRequestTempletemodel.WHStockRequest = CreateWHStock();



        //        foreach (var selectedWHStockRequestLineItemViewUI in SelectedWHStockRequestLineItemViewUI)
        //        {

        //            var whStockRequestLine = CreateWhStockLine(selectedWHStockRequestLineItemViewUI);
        //            if (WHRequestTempletemodel.WHStockRequestLines == null)
        //            {
        //                WHRequestTempletemodel.WHStockRequestLines = new List<WHStockRequestLine>();
        //            }

        //            WHRequestTempletemodel.WHStockRequestLines.Add(whStockRequestLine);

        //        }
        //        if (WHRequestTempletemodel.WHStockRequest.Status == StockRequestStatus.Draft && status == StockRequestStatus.Requested)
        //        {
        //            WHRequestTempletemodel.WHStockRequest.Status = StockRequestStatus.Requested;
        //            UpdateRequestedQty();
        //        }
        //            if (isMobile)
        //        {
        //            return await CUDWHStockMobile(WHRequestTempletemodel);

        //        }

        //        else { return await CUDWHStock(WHRequestTempletemodel); }

        //    }
        //    catch (Exception ex) { Console.WriteLine(ex.Message.ToString()); }
        //    return false;
        //}

        //public WHStockRequest CreateWHStock()
        //{
        //    var matchingRouteDetails = IsMobile
        //            ?
        //            RouteListByOrgUID.First(item => item.UID == SelectedRouteUID)
        //            : RouteDataByOrgUid.First(item => item.UID == SelectedRouteUID);
        //    WHRequestTempletemodel = new WHRequestTempleteModel();
        //    var whStockRequest = new WHStockRequest
        //    {
        //        UID = Guid.NewGuid().ToString(),
        //        CompanyUID = "WINIT",
        //        SourceOrgUID = _appUser.SelectedJobPosition?.OrgUID ?? string.Empty,
        //        SourceWHUID = matchingRouteDetails?.WHOrgUID ?? string.Empty,
        //        // InitiatedByWHUID = "INITWH123",
        //        TargetOrgUID = _appUser.SelectedJobPosition?.OrgUID ?? string.Empty,
        //        TargetWHUID = matchingRouteDetails?.VehicleUID ?? string.Empty,
        //        Code = SelectedRouteCodeDate,
        //        RequestType = RequestType.Load.ToString(),
        //        RequestByEmpUID = _appUser.Emp?.UID ?? string.Empty,
        //        JobPositionUID = _appUser.SelectedJobPosition?.UID ?? string.Empty,
        //        RequiredByDate = DateTime.Now,
        //        Status = WHStockRequestItemview.Status,
        //        Remarks = Remark,
        //        Id = 0,
        //        StockType = StockType.Salable.ToString(),
        //        SS = 0,
        //        CreatedTime = DateTime.Now,
        //        ModifiedTime = DateTime.Now,
        //        ServerAddTime = DateTime.Now,
        //        ServerModifiedTime = DateTime.Now,
        //        RouteUID = SelectedRouteUID
        //    };
        //    return whStockRequest;
        //}
        //public WHStockRequestLine CreateWhStockLine (WHStockRequestLineItemViewUI wHStockRequestLineItemViewUI)
        //{
        //    var whStockRequestLine = new WHStockRequestLine
        //    {
        //        UID = Guid.NewGuid().ToString(),
        //        CompanyUID = "WINIT",

        //        WHStockRequestUID = WHStockRequestItemview.UID,
        //        SKUCode = wHStockRequestLineItemViewUI.SKUCode,
        //        SKUUID = wHStockRequestLineItemViewUI?.SKUUID ?? string.Empty,
        //        UOM1 = wHStockRequestLineItemViewUI?.UOM1 ?? string.Empty,
        //        UOM2 = wHStockRequestLineItemViewUI?.UOM2 ?? string.Empty,
        //        UOM = wHStockRequestLineItemViewUI?.UOM1 ?? string.Empty,
        //        UOM1CNF = wHStockRequestLineItemViewUI?.UOM1CNF ?? 1,
        //        UOM2CNF = wHStockRequestLineItemViewUI?.UOM2CNF ?? 4,
        //        RequestedQty1 = wHStockRequestLineItemViewUI?.RequestedQty1 ?? 0,
        //        RequestedQty2 = wHStockRequestLineItemViewUI?.RequestedQty2 ?? 0,
        //        RequestedQty = ((wHStockRequestLineItemViewUI?.RequestedQty1 ?? 0) * 1) +
        //                           ((wHStockRequestLineItemViewUI?.RequestedQty2 ?? 0) *
        //                           (wHStockRequestLineItemViewUI?.UOM2CNF ?? 4)),
        //        CPEApprovedQty1 = wHStockRequestLineItemViewUI?.CPEApprovedQty1 ?? 0,
        //        CPEApprovedQty2 = wHStockRequestLineItemViewUI?.CPEApprovedQty2 ?? 0,
        //        CPEApprovedQty = wHStockRequestLineItemViewUI?.CPEApprovedQty ?? 0,
        //        ApprovedQty1 = wHStockRequestLineItemViewUI?.ApprovedQty1 ?? 0,
        //        ApprovedQty2 = wHStockRequestLineItemViewUI?.ApprovedQty2 ?? 0,
        //        ApprovedQty = wHStockRequestLineItemViewUI?.ApprovedQty ?? 0,
        //        Id = 0,
        //        ForwardQty1 = wHStockRequestLineItemViewUI?.ForwardQty1 ?? 0,
        //        ForwardQty2 = wHStockRequestLineItemViewUI?.ForwardQty2 ?? 0,
        //        ForwardQty = wHStockRequestLineItemViewUI?.ForwardQty ?? 0,
        //        CollectedQty1 = wHStockRequestLineItemViewUI?.CollectedQty1 ?? 0,
        //        CollectedQty2 = wHStockRequestLineItemViewUI?.CollectedQty2 ?? 0,
        //        CollectedQty = wHStockRequestLineItemViewUI?.CollectedQty ?? 0,
        //        WHQty =  0,
        //        SS = 0,
        //        CreatedTime = DateTime.Now,
        //        ModifiedTime = DateTime.Now,
        //        ServerAddTime = DateTime.Now,
        //        ServerModifiedTime = DateTime.Now,
        //        TemplateQty1 = wHStockRequestLineItemViewUI?.ApprovedQty ?? 0,
        //        TemplateQty2 = wHStockRequestLineItemViewUI?.ApprovedQty ?? 0,

        //    };
        //    return whStockRequestLine;

        //}
        public string StatusAdd { get; set; }

        //public string ConfirmationTab(string btnText)
        //{
        //    var tabselect = StockRequestStatus.Draft;
        //    if (btnText == StockRequestStatus.Requested)
        //    {
        //        tabselect = "Confirm";
        //    }
        //    else if (btnText == StockRequestStatus.Rejected)
        //    {
        //        tabselect = "Reject";
        //    }
        //    else if (btnText == StockRequestStatus.Approved)
        //    {
        //        tabselect = "Approve";
        //    }
            
        //    else if (btnText == StockRequestStatus.Draft)
        //    {
        //        tabselect = "Save";
        //    }
        //    return tabselect;
        //}
        //public string SuccessTab(string btnText)
        //{
        //    var tabselect = "Save";
        //    if (btnText == StockRequestStatus.Requested)
        //    {
        //        tabselect = "Requested";
        //    }
        //    else if (btnText == StockRequestStatus.Rejected)
        //    {
        //        tabselect = "Rejected";
        //    }
        //    else if (btnText == StockRequestStatus.Approved)
        //    {
        //        tabselect = "Approve";
        //    }
        //    else if (btnText == StockRequestStatus.Processed)
        //    {
        //        tabselect = "Processed";
        //    }
           
        //    return tabselect;
        //}
        public async Task<bool> CUDWHStock(WHRequestTempleteModel wHRequestTempleteModel)
        {

            string jsonBody = JsonConvert.SerializeObject(wHRequestTempleteModel);
            string apiUrl = $"{_appConfigs.ApiBaseUrl}WHStock/CUDWHStock";
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(apiUrl, HttpMethod.Post, wHRequestTempleteModel);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                SelectedWHStockRequestLineItemViewUI = null;
                WHRequestTempletemodel = null;
               
                return apiResponse.IsSuccess;
                
            }
            return false;
        }

        //public bool IsMobile { get; set; } = false;
        //public async Task<bool> UpdateLoadRequest()
        //{
        //    var wHRequestTempleteModel = new WHRequestTempleteModel();
        //    wHRequestTempleteModel.WHStockRequest = new WHStockRequest();

        //    if(WHStockRequestItemview.UID == null)
        //    {
        //        wHRequestTempleteModel.WHStockRequest = CreateWHStock();
        //        WHStockRequestItemview.UID = wHRequestTempleteModel.WHStockRequest.UID;
        //    }
        //    else {
        //        wHRequestTempleteModel.WHStockRequest.UID = WHStockRequestItemview.UID;
        //        wHRequestTempleteModel.WHStockRequest.ModifiedTime = DateTime.Now;
        //        wHRequestTempleteModel.WHStockRequest.ServerModifiedTime = DateTime.Now;
        //        wHRequestTempleteModel.WHStockRequest.Status = WHStockRequestItemview.Status;
        //    }
            
        //    wHRequestTempleteModel.WHStockRequestLines = new List<WHStockRequestLine>();
        //    foreach (var whstockRequestLines in WHStockRequestLineItemview)
        //    {
        //        WHStockRequestLine wHStockRequestLines;
        //        if (whstockRequestLines.UID == null)
        //        {
        //             wHStockRequestLines = CreateWhStockLine(whstockRequestLines);
        //        }
        //        else
        //        {
        //             wHStockRequestLines = new WHStockRequestLine
        //            {

        //                ModifiedTime = DateTime.Now,
        //                ServerModifiedTime = DateTime.Now,
        //                UID = whstockRequestLines.UID,
        //                RequestedQty1 = whstockRequestLines.RequestedQty1,
        //                RequestedQty2 = whstockRequestLines.RequestedQty2,
        //                RequestedQty = whstockRequestLines.RequestedQty,
        //                ApprovedQty1 = whstockRequestLines.ApprovedQty1,
        //                ApprovedQty2 = whstockRequestLines.ApprovedQty2,
        //                ApprovedQty = whstockRequestLines.ApprovedQty,
        //                CPEApprovedQty1 = whstockRequestLines.CPEApprovedQty1,
        //                CPEApprovedQty2 = whstockRequestLines.CPEApprovedQty2,
        //                CPEApprovedQty = whstockRequestLines.CPEApprovedQty,
        //                CollectedQty1 = whstockRequestLines.CollectedQty1,
        //                CollectedQty2 = whstockRequestLines.CollectedQty2,
        //                CollectedQty = whstockRequestLines.CollectedQty
        //            };
        //        }
        //        wHRequestTempleteModel.WHStockRequestLines.Add(wHStockRequestLines);
        //    }
        //    if (IsMobile)
        //    {
        //        if (await CUDWHStockMobile(wHRequestTempleteModel))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else {
        //        if (await CUDWHStock(wHRequestTempleteModel))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
            

        //  }
       

        //public async Task<List<WHStockRequestLineItemViewUI>> UpdateCPEApprovedQty(string status, List<WHStockRequestLineItemViewUI> SelectedWHStockRequestLineItemViewUI)
        //{
        //    if (CPEApprovalRequired == false && status == "Requested")
        //    {
        //        foreach (var item in SelectedWHStockRequestLineItemViewUI)
        //        {
        //            item.RequestedQty = item.RequestedQty1 * 1 + item.RequestedQty2 * item.UOM2CNF;
        //            item.CPEApprovedQty1 = item.RequestedQty1;
        //            item.CPEApprovedQty2 = item.RequestedQty2;
        //            item.CPEApprovedQty = item.RequestedQty;
        //        }
        //    }
        //    else if (CPEApprovalRequired == true && status == "Requested")
        //    {
        //        foreach (var item in SelectedWHStockRequestLineItemViewUI)
        //        {
        //            item.CPEApprovedQty1 = 0;
        //            item.CPEApprovedQty2 = 0;
        //            item.CPEApprovedQty = 0;
        //        }
        //    }
        //    return SelectedWHStockRequestLineItemViewUI;

        //}
        //public async Task<List<WHStockRequestLineItemViewUI>> UpdateERPApprovedQty(string status, List<WHStockRequestLineItemViewUI> SelectedWHStockRequestLineItemViewUI)
        //{
        //    if (ERPApprovalRequired == false && status == "Requested")
        //    {
        //        foreach (var item in SelectedWHStockRequestLineItemViewUI)
        //        {
        //            item.RequestedQty = item.RequestedQty1 * 1 + item.RequestedQty2 * item.UOM2CNF;
        //            item.ApprovedQty1 = item.RequestedQty1;
        //            item.ApprovedQty2 = item.RequestedQty2;
        //            item.ApprovedQty = item.RequestedQty;
        //        }

        //    }

        //    else if (ERPApprovalRequired == true && status == "Requested")
        //    {
        //        foreach (var item in SelectedWHStockRequestLineItemViewUI)
        //        {
        //            item.ApprovedQty1 = 0;
        //            item.ApprovedQty2 = 0;
        //            item.ApprovedQty = 0;
        //        }

        //    }
        //    return SelectedWHStockRequestLineItemViewUI;
        //}
        //public string UpdateStatus(string status)
        //{
        //    if (!CPEApprovalRequired)
        //    {
        //        return StockRequestStatus.Approved;
        //    }
        //    else if (!ERPApprovalRequired)
        //    {
        //        return StockRequestStatus.Processed;
        //    }
        //    return status;
        //}
        //public void UpdateApprovedQtyByStatus(string status = StockRequestStatus.Requested)
        //{
        //    if (WHRequestTempletemodel == null)
        //    {
        //        WHStockRequestItemview.Status = status;
        //        if (WHStockRequestItemview.Status == StockRequestStatus.Requested || WHStockRequestItemview.Status == StockRequestStatus.Approved)
        //        {
        //            if (WHStockRequestItemview.Status == StockRequestStatus.Requested && !CPEApprovalRequired)
        //            {
        //                WHStockRequestItemview.Status = StockRequestStatus.Approved;
        //                UpdateCPEApprovedQtyByStatus();
        //            }
        //            if (WHStockRequestItemview.Status == StockRequestStatus.Approved && !ERPApprovalRequired)
        //            {
        //                WHStockRequestItemview.Status = StockRequestStatus.Processed;
        //                UpdateERPApprovedQtyByStatus();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        WHRequestTempletemodel.WHStockRequest.Status = status;
        //        if (WHRequestTempletemodel.WHStockRequest.Status == StockRequestStatus.Requested || WHRequestTempletemodel.WHStockRequest.Status == StockRequestStatus.Approved)
        //        {
        //            if (WHRequestTempletemodel.WHStockRequest.Status == StockRequestStatus.Requested && !CPEApprovalRequired)
        //            {
        //                WHRequestTempletemodel.WHStockRequest.Status = StockRequestStatus.Approved;
        //                UpdateCPEApprovedQtyByStatus();
        //            }
        //            if (WHRequestTempletemodel.WHStockRequest.Status == StockRequestStatus.Approved && !ERPApprovalRequired)
        //            {
        //                WHRequestTempletemodel.WHStockRequest.Status = StockRequestStatus.Processed;
        //                UpdateERPApprovedQtyByStatus();
        //            }
        //        }
        //    }
            
        //}
        //public async void UpdateCPEApprovedQtyByStatus()
        //{
        //    if (WHRequestTempletemodel == null)
        //    {
        //        foreach (var item in WHStockRequestLineItemview)
        //        {
        //            item.CPEApprovedQty1 = item.RequestedQty1;
        //            item.CPEApprovedQty2 = item.RequestedQty2;
        //            item.CPEApprovedQty = item.RequestedQty;
        //        }
        //    }
        //    else
        //    {
        //        foreach (var item in WHRequestTempletemodel.WHStockRequestLines)
        //        {
        //            item.CPEApprovedQty1 = item.RequestedQty1;
        //            item.CPEApprovedQty2 = item.RequestedQty2;
        //            item.CPEApprovedQty = item.RequestedQty;
        //        }
        //    }
        //}
        //public async void UpdateERPApprovedQtyByStatus()
        //{
           

        //    if (WHRequestTempletemodel == null)
        //    {
        //        foreach (var item in WHStockRequestLineItemview)
        //        {
        //            item.ApprovedQty1 = item.CPEApprovedQty1;
        //            item.ApprovedQty2 = item.CPEApprovedQty2;
        //            item.ApprovedQty = item.CPEApprovedQty;
        //        }
        //    }
        //    else
        //    {
        //        foreach (var item in WHRequestTempletemodel.WHStockRequestLines)
        //        {
        //            item.ApprovedQty1 = item.CPEApprovedQty1;
        //            item.ApprovedQty2 = item.CPEApprovedQty2;
        //            item.ApprovedQty = item.CPEApprovedQty;
        //        }
        //    }
        //}
        //public List<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView> DisplayWHStockRequestItemViewM { get; set; }
                                                           

        //public async Task GetLoadRequestSQLiteData(string activeTab)
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest();

        //        if (_IWHStockBL != null)
        //        {
        //            var pagedResponse = await _IWHStockBL.SelectLoadRequestData(
        //                pagingRequest?.SortCriterias,
        //                pagingRequest?.PageNumber ?? 1,
        //                pagingRequest?.PageSize ?? 10,
        //                pagingRequest?.FilterCriterias,
        //                pagingRequest?.IsCountRequired ?? false,
        //                activeTab
        //            );
        //            if (pagedResponse.PagedData != null)
        //            {
        //                DisplayWHStockRequestItemViewM = pagedResponse.PagedData.ToList();
        //                DisplayWHStockRequestItemViewM = DisplayWHStockRequestItemViewM
        //                                            .OrderByDescending(line => line.ModifiedTime)
        //                                            .ToList();
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine($"{_IWHStockBL} is null");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message.ToString());
        //    }
        //}

        //List<string> orgUIDs = new List<string>();
        //List<string> DistributionChannelUIDs = new List<string>();

        //List<string> skuUIDs = new List<string>();
        //List<string> attributeTypes = new List<string>();

        //public async Task GetSQLiteSKUDMasterData()
        //{
        //    try
        //    {


        //        if (_IWHStockBL != null)
        //        {
        //            var pagedResponse = await _ISKUBL.PrepareSKUMaster(
        //                orgUIDs, DistributionChannelUIDs, skuUIDs, attributeTypes);

        //            if (pagedResponse != null)
        //            {
        //                SkuMasterData = ConvertToSKUMasterData(pagedResponse);
        //                SkuAttributesList = await FindCompleteSKUAttributes(SkuMasterData);
        //                SkuUOMList = await FindCompleteSKUUOM(SkuMasterData);
        //                SkuList = await FindCompleteSKU(SkuMasterData);
                       
        //            }

        //        }
               
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message.ToString());
        //    }
           
        //}

        //public static List<SKUMasterData> ConvertToSKUMasterData(List<ISKUMaster> iskuMasters)
        //{
        //    List<SKUMasterData> skumasterDataList = new List<SKUMasterData>();

        //    foreach (var iskumaster in iskuMasters)
        //    {
        //        SKUMasterData skumasterData = new SKUMasterData
        //        {
        //            SKU = (Winit.Modules.SKU.Model.Classes.SKU)iskumaster.SKU,
        //            SKUAttributes = iskumaster.SKUAttributes.Cast<Winit.Modules.SKU.Model.Classes.SKUAttributes>().ToList(),
        //            SKUUOMs = iskumaster.SKUUOMs.Cast<Winit.Modules.SKU.Model.Classes.SKUUOM>().ToList(),
        //            //ApplicableTaxUIDs = iskumaster.ApplicableTaxUIDs,
        //            //SKUConfigs = iskumaster.SKUConfigs.Cast<Winit.Modules.SKU.Model.Classes.SKUConfig>().ToList(),
        //            //customSKUFields = iskumaster.customSKUFields.Cast<Winit.Modules.CustomSKUField.Model.Classes.CustomSKUFields>().ToList()
        //        };

        //        skumasterDataList.Add(skumasterData);
        //    }

        //    return skumasterDataList;
        //}

        //public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteListByOrgUID { get; set; }

        //public async Task GetRouteByOrgUID(string OrgUID)
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest();

        //        if (_IRouteBL != null)
        //        {
        //            var pagedResponse = await _IRouteBL.SelectRouteAllDetails(
        //                pagingRequest?.SortCriterias,
        //                pagingRequest?.PageNumber ?? 1,
        //                pagingRequest?.PageSize ?? 10,
        //                pagingRequest?.FilterCriterias,
        //                pagingRequest?.IsCountRequired ?? false,
        //                OrgUID
        //            );
        //            if (pagedResponse.PagedData != null)
        //            {
        //                RouteListByOrgUID = pagedResponse.PagedData.ToList();
                        
        //            }
        //        }
                
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message.ToString());
        //    }
           
        //}
       

        //public async Task<bool> UpdateQuantities(string btnText)
        //{
        //   if (WHStockRequestItemview == null || WHStockRequestItemview.Status == null)
        //    {
        //        WHStockRequestItemview = new WHStockRequestItemView();
        //        WHStockRequestItemview.Status = btnText;
        //    }
        //     if (btnText == StockRequestStatus.Rejected)
        //    {
        //        WHStockRequestItemview.Status = StockRequestStatus.Rejected;
        //    }

        //        else if(WHStockRequestItemview.Status == StockRequestStatus.Requested && btnText == StockRequestStatus.Approved)
        //    {
        //        WHStockRequestItemview.Status = StockRequestStatus.Approved;
        //        UpdateCPEApprovedQty();

        //    }
        //    else if (WHStockRequestItemview.Status == StockRequestStatus.Approved && btnText == StockRequestStatus.Approved)
        //    {
        //        WHStockRequestItemview.Status = StockRequestStatus.Processed;
        //        UpdateERPApprovedQty();
        //    }
        //    else if (WHStockRequestItemview.Status == StockRequestStatus.Draft && btnText == StockRequestStatus.Requested)
        //    {
        //        WHStockRequestItemview.Status = StockRequestStatus.Requested;
        //        UpdateRequestedQty();
        //    }
            
        //    if (await UpdateLoadRequest())
        //    {
        //        return true;
        //    }
        //    else { return false; }
        //}

        //private void UpdateCPEApprovedQty()
        //{

        //    UpdateApprovedQtyByStatus(WHStockRequestItemview.Status);
            
        //}
        //private void UpdateERPApprovedQty()
        //{
           
        //}
        //private void UpdateRequestedQty()
        //{
           

        //    //    if(WHRequestTempletemodel == null)
        //    //{
        //        UpdateApprovedQtyByStatus(WHStockRequestItemview.Status);
        //   // }
        //    //else
        //    //{
        //    //    UpdateApprovedQtyByStatus(WHRequestTempletemodel.WHStockRequest.Status);
        //    //}
               
            
        //}
      
        //public async Task<bool> CUDWHStockMobile(WHRequestTempleteModel wHRequestTempleteModel)
        //{
        //    var retVal = await _IWHStockBL.CUDWHStock(wHRequestTempleteModel);
        //    if (retVal > 0)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
    }
}

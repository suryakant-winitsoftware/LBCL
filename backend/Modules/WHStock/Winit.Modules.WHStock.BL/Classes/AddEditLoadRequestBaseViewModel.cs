using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.WHStock.DL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.BL.Classes
{
    public class AddEditLoadRequestBaseViewModel : IAddEditLoadRequest
    {
        public List<IWHStockRequestLineItemViewUI> DisplayWHStockRequestLineItemview { get; set; }
        public List<ISelectionItem> RouteListForSelection { get; set; }
        public IWHStockRequestItemView WHStockRequestItemview { get; set; }
        public WHRequestTempleteModel WHRequestTempletemodel { get; set; }
        public DateTime RequiredByDate { get; set; }
        public string RequestType { get; set; }
        public ViewLoadRequestItemViewUI DisplayLoadRequestItemView { get; set; }
        public WHRequestTempleteModel pendingLoadRequestTemplateModel = new WHRequestTempleteModel();
        public string SelectedRouteCodeDate { get; set; }
        public string SelectedRouteUID { get; set; }
        public string SelectedRouteCode { get; set; }

        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SkuUOMList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteListByOrgUID { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteList { get; set; }

        public string CustomerSignatureFolderPath { get; set; }
        public string UserSignatureFolderPath { get; set; }
        public string CustomerSignatureFileName { get; set; }
        public string UserSignatureFileName { get; set; }
        public string CustomerSignatureFilePath { get; set; }
        public string UserSignatureFilePath { get; set; }
        public List<IFileSys> SignatureFileSysList { get; set; }

        public string Stocktype { get; set; }
        public string Remark { get; set; }
        public string OrgUID { get; set; }
        // public IRoute SelectedRoute{get;set;}
        public Winit.Modules.Route.Model.Interfaces.IRoute SelectedRoute { get; set; }
        public IServiceProvider _serviceProvider;
        public IFilterHelper _filter;
        public ISortHelper _sorter;
        public IListHelper _listHelper;
        public IEnumerable<ISKUMaster> SKUMasterList;
        public IAppUser _appUser;

        public List<string> _propertiesToSearch = new List<string>();
        public Winit.Shared.Models.Common.IAppConfig _appConfigs;
        public Winit.Modules.Base.BL.ApiService _apiService;
        public Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSetting;
        public Winit.Modules.Route.BL.Interfaces.IRouteBL _IRouteBL { get; set; }
        public Winit.Modules.WHStock.BL.Interfaces.IWHStockBL _IWHStockBL { get; set; }
        private ISKUBL _ISKUBL { get; set; }
        public Winit.Modules.StockUpdater.BL.Interfaces.IStockUpdaterBL _stockUpdaterBL;
        public AddEditLoadRequestBaseViewModel(IServiceProvider serviceProvider,
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
            StockUpdater.BL.Interfaces.IStockUpdaterBL stockUpdaterBL)
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
            _appSetting = appSetting;
            SignatureFileSysList = new List<IFileSys>();
            Stocktype = LoadRequestConst.Salable;
            _stockUpdaterBL = stockUpdaterBL;
        }
        public virtual async Task GetRouteDataByOrgUID(string OrgUID)
        {
            throw new NotImplementedException();
        }
        //public virtual async Task GetLoadRequestByUID(string uid)
        //{
        //    throw new NotImplementedException();
        //}
        public virtual async Task<bool> CUDWHStock(string btnText)
        {
            throw new NotImplementedException();
        }
        public virtual async Task PopulateViewModel(string apiParam = null)
        {
            throw new NotImplementedException();
        }

        public virtual async Task GetSKUMasterData()
        {
            throw new NotImplementedException();
        }




        public bool CPEApprovalRequired = true;
        public bool ERPApprovalRequired = true;

        public bool IsMobile { get; set; } = false;

        public List<Winit.Modules.Route.Model.Classes.Route> RouteDataByOrgUid = new List<Route.Model.Classes.Route> { };


        public List<Winit.Modules.SKU.Model.Classes.SKUMasterData> SkuMasterData;

        public List<Winit.Modules.Route.Model.Classes.Route> SelectedRouteList { get; set; }

        public Winit.Modules.Org.Model.Classes.Org OrgsList { get; set; }




        public void InitializeVariables()
        {
            CPEApprovalRequired = _appSetting.LOAD_IS_CPE_APPROVAL_REQUIRED;
            ERPApprovalRequired = _appSetting.LOAD_IS_ERP_APPROVAL_REQUIRED;
        }

        public string ConfirmationTab(string btnText)
        {
            var tabselect = StockRequestStatus.Draft;
            if (btnText == StockRequestStatus.Requested)
            {
                tabselect = "confirm";
            }
            else if (btnText == StockRequestStatus.Rejected)
            {
                tabselect = "reject";
            }
            else if (btnText == StockRequestStatus.Approved)
            {
                tabselect = "approve";
            }
            else if (btnText == StockRequestStatus.Collected)
            {
                tabselect = "collect";
            }
            else if (btnText == StockRequestStatus.Draft)
            {
                tabselect = "save";
            }
            return tabselect;
        }
        public string SuccessTab(string btnText)
        {
            var tabselect = "save";
            if (btnText == StockRequestStatus.Requested)
            {
                tabselect = "requested";
            }
            else if (btnText == StockRequestStatus.Rejected)
            {
                tabselect = "rejected";
            }
            else if (btnText == StockRequestStatus.Approved)
            {
                tabselect = "approve";
            }
            else if (btnText == StockRequestStatus.Processed)
            {
                tabselect = "processed";
            }
            else if (btnText == StockRequestStatus.Collected)
            {
                tabselect = "collected";
            }

            return tabselect;
        }

        public async Task UpdateQuantities(string btnText)
        {
            InitializeVariables();
            if (WHStockRequestItemview == null || WHStockRequestItemview.Status == null)
            {
                WHStockRequestItemview = new WHStockRequestItemViewUI();
                WHStockRequestItemview.Status = StockRequestStatus.Draft;
            }
            if (btnText == StockRequestStatus.Rejected)
            {
                WHStockRequestItemview.Status = StockRequestStatus.Rejected;
            }

            else if (WHStockRequestItemview.Status == StockRequestStatus.Requested && btnText == StockRequestStatus.Approved)
            {
                WHStockRequestItemview.Status = StockRequestStatus.Approved;
                UpdateCPEApprovedQty();

            }
            else if (WHStockRequestItemview.Status == StockRequestStatus.Approved && btnText == StockRequestStatus.Approved)
            {
                WHStockRequestItemview.Status = StockRequestStatus.Processed;

            }
            else if (WHStockRequestItemview.Status == StockRequestStatus.Draft && btnText == StockRequestStatus.Requested)
            {
                WHStockRequestItemview.Status = StockRequestStatus.Requested;
                UpdateRequestedQty();
            }

            await UpdateLoadRequest();

        }
        private void UpdateCPEApprovedQty()
        {

            UpdateApprovedQtyByStatus(WHStockRequestItemview.Status);

        }
        public void UpdateApprovedQtyByStatus(string status = StockRequestStatus.Requested)
        {

            WHStockRequestItemview.Status = status;
            if (WHStockRequestItemview.Status == StockRequestStatus.Requested || WHStockRequestItemview.Status == StockRequestStatus.Approved)
            {
                if (WHStockRequestItemview.Status == StockRequestStatus.Requested && !CPEApprovalRequired)
                {
                    WHStockRequestItemview.Status = StockRequestStatus.Approved;
                    UpdateCPEApprovedQtyByStatus();
                }
                if (WHStockRequestItemview.Status == StockRequestStatus.Approved && !ERPApprovalRequired)
                {
                    WHStockRequestItemview.Status = StockRequestStatus.Processed;
                    UpdateERPApprovedQtyByStatus();
                }
            }



        }
        public async void UpdateCPEApprovedQtyByStatus()
        {
            foreach (var item in DisplayWHStockRequestLineItemview)
            {
                item.CPEApprovedQty1 = item.RequestedQty1;
                item.CPEApprovedQty2 = item.RequestedQty2;
                item.CPEApprovedQty = item.RequestedQty;
            }
        }
        public async void UpdateERPApprovedQtyByStatus()
        {
            foreach (var item in DisplayWHStockRequestLineItemview)
            {
                item.ApprovedQty1 = item.CPEApprovedQty1;
                item.ApprovedQty2 = item.CPEApprovedQty2;
                item.ApprovedQty = item.CPEApprovedQty;
            }
        }

        private void UpdateRequestedQty()
        {
            UpdateApprovedQtyByStatus(WHStockRequestItemview.Status);
        }

        public async Task UpdateLoadRequest()
        {
            int yearMonth = CommonFunctions.GetYearMonth(DateTime.Now);
            if (WHStockRequestItemview.UID == null)
            {
                pendingLoadRequestTemplateModel.WHStockRequest = CreateWHStock();
                WHStockRequestItemview.UID = pendingLoadRequestTemplateModel.WHStockRequest.UID;
            }
            else
            {
                pendingLoadRequestTemplateModel.WHStockRequest = new WHStockRequest();
                pendingLoadRequestTemplateModel.WHStockRequest.UID = WHStockRequestItemview.UID;
                pendingLoadRequestTemplateModel.WHStockRequest.ModifiedTime = DateTime.Now;
                pendingLoadRequestTemplateModel.WHStockRequest.ServerModifiedTime = DateTime.Now;
                pendingLoadRequestTemplateModel.WHStockRequest.Status = WHStockRequestItemview.Status;
                pendingLoadRequestTemplateModel.WHStockRequest.RequestType = WHStockRequestItemview.RequestType;
                pendingLoadRequestTemplateModel.WHStockRequest.SourceOrgUID = WHStockRequestItemview.SourceOrgUID;
                pendingLoadRequestTemplateModel.WHStockRequest.SourceWHUID = WHStockRequestItemview.SourceWHUID;
                pendingLoadRequestTemplateModel.WHStockRequest.TargetOrgUID = WHStockRequestItemview.TargetOrgUID;
                pendingLoadRequestTemplateModel.WHStockRequest.TargetWHUID = WHStockRequestItemview.TargetWHUID;
                pendingLoadRequestTemplateModel.WHStockRequest.RouteUID = WHStockRequestItemview.RouteUID;
                pendingLoadRequestTemplateModel.WHStockRequest.StockType = Stocktype;
                pendingLoadRequestTemplateModel.WHStockRequest.OrgUID = WHStockRequestItemview.OrgUID;
                pendingLoadRequestTemplateModel.WHStockRequest.WareHouseUID = WHStockRequestItemview.WareHouseUID;
                pendingLoadRequestTemplateModel.WHStockRequest.YearMonth = WHStockRequestItemview.YearMonth;
            }

            pendingLoadRequestTemplateModel.WHStockRequestLines = new List<WHStockRequestLine>();
            foreach (var whstockRequestLines in DisplayWHStockRequestLineItemview)
            {
                WHStockRequestLine wHStockRequestLines;
                if (whstockRequestLines.UID == null)
                {
                    wHStockRequestLines = CreateWhStockLine(whstockRequestLines);
                }
                else
                {
                    wHStockRequestLines = new WHStockRequestLine
                    {
                        ModifiedTime = DateTime.Now,
                        ServerModifiedTime = DateTime.Now,
                        UID = whstockRequestLines.UID,
                        RequestedQty1 = whstockRequestLines.RequestedQty1,
                        RequestedQty2 = whstockRequestLines.RequestedQty2,
                        RequestedQty = whstockRequestLines.RequestedQty,
                        ApprovedQty1 = whstockRequestLines.ApprovedQty1,
                        ApprovedQty2 = whstockRequestLines.ApprovedQty2,
                        ApprovedQty = whstockRequestLines.ApprovedQty,
                        CPEApprovedQty1 = whstockRequestLines.CPEApprovedQty1,
                        CPEApprovedQty2 = whstockRequestLines.CPEApprovedQty2,
                        CPEApprovedQty = whstockRequestLines.CPEApprovedQty,
                        CollectedQty1 = whstockRequestLines.CollectedQty1,
                        CollectedQty2 = whstockRequestLines.CollectedQty2,
                        CollectedQty = whstockRequestLines.CollectedQty,
                        UOM = whstockRequestLines.UOM,
                        SKUUID = whstockRequestLines.SKUUID,
                        SKUCode = whstockRequestLines.SKUCode,
                        OrgUID = whstockRequestLines.OrgUID,
                        WareHouseUID = whstockRequestLines.WareHouseUID,
                        YearMonth = whstockRequestLines.YearMonth,
                    };
                }
                pendingLoadRequestTemplateModel.WHStockRequestLines.Add(wHStockRequestLines);
            }


        }
        public WHStockRequest CreateWHStock()
        {
            int yearMonth = CommonFunctions.GetYearMonth(DateTime.Now);
            string sourceOrgUID = string.Empty;
            string targetOrgUID = string.Empty;
            string sourceWHUID = string.Empty;
            string targetWHUID = string.Empty;
            string wareHouseUID = string.Empty;
            sourceOrgUID = _appUser.SelectedJobPosition?.OrgUID ?? "";
            targetOrgUID = _appUser.SelectedJobPosition?.OrgUID ?? "";
            wareHouseUID = SelectedRoute?.WHOrgUID;
            if (RequestType == Enum.GetName(Winit.Shared.Models.Enums.RequestType.Load))
            {
                sourceWHUID = SelectedRoute?.WHOrgUID;
                targetWHUID = SelectedRoute?.VehicleUID;
            }
            else
            {
                sourceWHUID = SelectedRoute?.VehicleUID;
                targetWHUID = SelectedRoute?.WHOrgUID;
            }

            WHRequestTempletemodel = new WHRequestTempleteModel();
            var whStockRequest = new WHStockRequest
            {
                UID = Guid.NewGuid().ToString(),
                CompanyUID = _appUser.Emp.CompanyUID,
                RequestType = RequestType,
                SourceOrgUID = sourceOrgUID,
                SourceWHUID = sourceWHUID,
                TargetOrgUID = targetOrgUID,
                TargetWHUID = targetWHUID,
                Code = SelectedRouteCodeDate,
                RequestByEmpUID = _appUser.Emp?.UID ?? string.Empty,
                JobPositionUID = _appUser.SelectedJobPosition?.UID ?? string.Empty,
                RequiredByDate = RequiredByDate,
                Status = WHStockRequestItemview.Status,
                Remarks = Remark,
                Id = 0,
                StockType = Stocktype,
                SS = 0,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
                RouteUID = SelectedRouteUID,
                WareHouseUID = wareHouseUID,
                OrgUID = _appUser.SelectedJobPosition?.OrgUID ?? "",
                YearMonth = yearMonth
            };
            return whStockRequest;
        }
        public WHStockRequestLine CreateWhStockLine(IWHStockRequestLineItemViewUI wHStockRequestLineItemViewUI)
        {
            int yearMonth = CommonFunctions.GetYearMonth(DateTime.Now);
            var matchingRouteDetails = RouteList.First(item => item.UID == SelectedRouteUID);
            var whStockRequestLine = new WHStockRequestLine
            {
                UID = Guid.NewGuid().ToString(),
                CompanyUID = _appUser.Emp.CompanyUID,
                WHStockRequestUID = WHStockRequestItemview.UID,
                SKUCode = wHStockRequestLineItemViewUI.SKUCode,
                SKUUID = wHStockRequestLineItemViewUI?.SKUUID ?? string.Empty,
                UOM1 = wHStockRequestLineItemViewUI?.UOM1 ?? string.Empty,
                UOM2 = wHStockRequestLineItemViewUI?.UOM2 ?? string.Empty,
                UOM = wHStockRequestLineItemViewUI?.UOM1 ?? string.Empty,
                UOM1CNF = wHStockRequestLineItemViewUI?.UOM1CNF ?? 1,
                UOM2CNF = wHStockRequestLineItemViewUI?.UOM2CNF ?? 4,
                RequestedQty1 = wHStockRequestLineItemViewUI?.RequestedQty1 ?? 0,
                RequestedQty2 = wHStockRequestLineItemViewUI?.RequestedQty2 ?? 0,
                RequestedQty = ((wHStockRequestLineItemViewUI?.RequestedQty1 ?? 0) * 1) + ((wHStockRequestLineItemViewUI?.RequestedQty2 ?? 0) * (wHStockRequestLineItemViewUI?.UOM2CNF ?? 4)),
                CPEApprovedQty1 = wHStockRequestLineItemViewUI?.CPEApprovedQty1 ?? 0,
                CPEApprovedQty2 = wHStockRequestLineItemViewUI?.CPEApprovedQty2 ?? 0,
                CPEApprovedQty = wHStockRequestLineItemViewUI?.CPEApprovedQty ?? 0,
                ApprovedQty1 = wHStockRequestLineItemViewUI?.ApprovedQty1 ?? 0,
                ApprovedQty2 = wHStockRequestLineItemViewUI?.ApprovedQty2 ?? 0,
                ApprovedQty = wHStockRequestLineItemViewUI?.ApprovedQty ?? 0,
                Id = 0,
                ForwardQty1 = wHStockRequestLineItemViewUI?.ForwardQty1 ?? 0,
                ForwardQty2 = wHStockRequestLineItemViewUI?.ForwardQty2 ?? 0,
                ForwardQty = wHStockRequestLineItemViewUI?.ForwardQty ?? 0,
                CollectedQty1 = wHStockRequestLineItemViewUI?.CollectedQty1 ?? 0,
                CollectedQty2 = wHStockRequestLineItemViewUI?.CollectedQty2 ?? 0,
                CollectedQty = wHStockRequestLineItemViewUI?.CollectedQty ?? 0,
                WHQty = 0,
                SS = 0,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
                TemplateQty1 = wHStockRequestLineItemViewUI?.ApprovedQty ?? 0,
                TemplateQty2 = wHStockRequestLineItemViewUI?.ApprovedQty ?? 0,
                LineNumber = wHStockRequestLineItemViewUI.LineNumber,
                OrgUID = _appUser.SelectedJobPosition?.OrgUID ?? "",
                WareHouseUID = SelectedRoute?.WHOrgUID,
                YearMonth = yearMonth

            };
            return whStockRequestLine;

        }


        protected async Task FetchRouteForSelection()
        {


            if (RouteListForSelection == null)
            {
                RouteListForSelection = new List<ISelectionItem>();
            }

            if (RouteList == null)
            {
                // Handle the case when TemplateRouteList is null
                return;
            }

            else if (RouteListForSelection.Count == 0)
            {
                foreach (var route in RouteList)
                {
                    RouteListForSelection.Add(new SelectionItem { UID = route.UID, IsSelected = false, Code = route.Code, Label = route.Name });
                }
            }
        }
        public async Task FetchSKUs(IEnumerable<SKUMasterData> sKUMasters)
        {


            try
            {
                if (sKUMasters != null)
                {
                    if (SkuList == null)
                    {
                        SkuList = new List<ISKU>();
                    }
                    foreach (var skuMaster in sKUMasters)
                    {

                        SkuList.Add(skuMaster.SKU);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }

        public async Task FetchSKUUOMs(IEnumerable<SKUMasterData> sKUMasters)
        {


            try
            {
                if (sKUMasters != null)
                {
                    if (SkuUOMList == null)
                    {
                        SkuUOMList = new List<ISKUUOM>();
                    }
                    foreach (var skuMaster in sKUMasters)
                    {
                        foreach (var skuUOM in skuMaster.SKUUOMs)
                        {
                            SkuUOMList.Add(skuUOM);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }
        public async Task FetchSKUAttributes(IEnumerable<SKUMasterData> sKUMasters)
        {

            try
            {
                if (sKUMasters != null)
                {
                    if (SkuAttributesList == null)
                    {
                        SkuAttributesList = new List<ISKUAttributes>();
                    }
                    foreach (var skuMaster in sKUMasters)
                    {
                        foreach (var skuAttributes in skuMaster.SKUAttributes)
                        {
                            SkuAttributesList.Add(skuAttributes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }

        public async Task<ViewLoadRequestItemViewUI> ApplySearch(string searchString)
        {

            return null;
        }

        //Signature Logic

        public void PrepareSignatureFields()
        {
            // original  string baseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), FileSysTemplateControles.GetSignatureFolderPath("SalesOrder", SalesOrderUID));
            string baseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), FileSysTemplateControles.GetSignatureFolderPath("LoadRequest", WHStockRequestItemview.UID));
            CustomerSignatureFolderPath = baseFolder;
            UserSignatureFolderPath = baseFolder;
            CustomerSignatureFileName = $"Customer_{WHStockRequestItemview.UID}.png";
            UserSignatureFileName = $"User_{WHStockRequestItemview.UID}.png";
        }
        public void OnSignatureProceedClick()
        {
            CustomerSignatureFilePath = Path.Combine(CustomerSignatureFolderPath, CustomerSignatureFileName);
            UserSignatureFilePath = Path.Combine(UserSignatureFolderPath, UserSignatureFileName);
        }
    }
}

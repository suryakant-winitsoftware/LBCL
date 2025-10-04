using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;
namespace Winit.Modules.WHStock.BL.Interfaces
{
    public interface IAddEditLoadRequest
    {
        public ViewLoadRequestItemViewUI DisplayLoadRequestItemView { get; set; }

        public string CustomerSignatureFolderPath { get; set; }
        public string UserSignatureFolderPath { get; set; }
        public string CustomerSignatureFileName { get; set; }
        public string UserSignatureFileName { get; set; }
        public string CustomerSignatureFilePath { get; set; }
        public string UserSignatureFilePath { get; set; }
        public List<IFileSys> SignatureFileSysList { get; set; }
        Task UpdateQuantities(string btnText);
        string ConfirmationTab(string btnText);
        public string SuccessTab(string btnText);
        public bool IsMobile { get; set; }
        Task<bool> CUDWHStock(string btnText);
        public List<IWHStockRequestLineItemViewUI> DisplayWHStockRequestLineItemview { get; set; }
        public List<ISelectionItem> RouteListForSelection { get; set; }
        public List<Winit.Modules.Route.Model.Classes.Route> SelectedRouteList { get; set; }
        public Winit.Modules.Route.Model.Interfaces.IRoute SelectedRoute { get; set; }
        public Winit.Modules.Org.Model.Classes.Org OrgsList { get; set; }
        public IWHStockRequestItemView WHStockRequestItemview { get; set; }
        public string RequestType { get; set; }

        public WHRequestTempleteModel WHRequestTempletemodel { get; set; }
        Task PopulateViewModel(string apiParam = null);
        public string SelectedRouteCodeDate { get; set; }
        public string SelectedRouteUID { get; set; }
        public string SelectedRouteCode { get; set; }
        Task GetSKUMasterData();
        Task GetRouteDataByOrgUID(string OrgUID);
        Task<ViewLoadRequestItemViewUI> ApplySearch(string searchString);
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SkuUOMList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteListByOrgUID { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteList { get; set; }
        public string OrgUID { get; set; }
        public string Remark { get; set; }
        void OnSignatureProceedClick();
        public string Stocktype { get; set; }
        public DateTime RequiredByDate { get; set; }

    }
}

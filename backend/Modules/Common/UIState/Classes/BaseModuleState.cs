using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.UIState.Interfaces;

namespace Winit.Modules.Common.UIState.Classes
{
    public class BaseModuleState : IClearableState
    {
        private Dictionary<string, PageState> _stateByPage = new Dictionary<string, PageState>();

        // List of pages that require state persistence
        private HashSet<string> _stateEnabledPages = new HashSet<string>
        {
            "viewpurchaseorderstatus","ManageScheme","ManageHOProvisionStandingConfiguration","ManageCustomerPriceList",
            "TemporaryCreditEnhancement","AllowedSKU","MaintainSKU","purchaseordertemplate","manageinvoice",
            "CustomerDetails","MaintainUsers","maintainUserRole","MaintainWarehouse","ViewErrorDetails",
            "viewaudittrial","MaintainTax","MaintainCurrency","viewallchangerequestapprovalinfo","ViewBankDetails",
            "MaintainBroadClassification","MaintainBranch","MaintainNewsActivity","MaintainPromotions","maintaindistributor","ManageCustomers"
            
            // add other 
        };
        // List of acceptable previous pages for navigation history
        private HashSet<string> _acceptablePreviousPages = new HashSet<string>
        {
            "purchaseorder",
            "createpurchaseorder","SellInSchemeBranchView",
            "QPSScheme/true","SelloutLiquidation","" ,"AddEditPrice",
            "SelloutrealSecondaryScheme",
            "salespromotion","AddHOProvisionStandingConfiguration","NewTemporaryCreditEnhancement"
            ,"SkuClassificationItemsMap","AddEditMaintainSKU","addeditpotemplate","viewinvoice","AddCustomers",
            "AddEditEmployee","maintainmobilemenu","MaintainWebMenu","AddEditWareHouse","AddEditMaintainError",
            "viewaudittraildetail","AddEditTax","AddEditCurrency","changerequestapprovalinfo","ViewBankMaster",
            "EditMaintainBroadClassification","EditMaintainBranch","Activity","create-promotion","newdistributor","Store"
            
            // add other acceptable previous pages as needed
        };
        public bool IsStateEnabledPage(string pageRoute)
        {
            return _stateEnabledPages.Any(p => p.Equals(pageRoute, StringComparison.OrdinalIgnoreCase));
        }
        public bool IsAcceptablePreviousPage(string pageRoute)
        {
            return _acceptablePreviousPages.Contains(pageRoute);
        }
        public void SavePageState(string pageRoute, PageState state)
        {
            if (IsStateEnabledPage(pageRoute))
            {
                _stateByPage[pageRoute] = state;
            }
        }
        public void SaveRefferedState(string pageRoute, PageState state)
        {

            _stateByPage[pageRoute] = state;

        }

        public PageState GetPageState(string pageRoute)
        {
            if (IsStateEnabledPage(pageRoute) && _stateByPage.ContainsKey(pageRoute))
            {
                return _stateByPage[pageRoute];
            }

            return null;
        }
        public PageState GetReferedPageState(string pageRoute)
        {
            if (_stateByPage.ContainsKey(pageRoute))
            {
                return _stateByPage[pageRoute];
            }

            return null;
        }
        public void AddAcceptablePreviousPage(string pageRoute)
        {
            _acceptablePreviousPages.Add(pageRoute);
        }

        public void RemoveAcceptablePreviousPage(string pageRoute)
        {
            _acceptablePreviousPages.Remove(pageRoute);
        }

        public List<string> GetAcceptablePreviousPages()
        {
            return _acceptablePreviousPages.ToList();
        }
        public void ClearPageState(string pageRoute)
        {
            if (_stateByPage.ContainsKey(pageRoute))
            {
                _stateByPage.Remove(pageRoute);
            }
        }
        public void Clear()
        {
            //Filters = null;
            //TabUID = null;
            _stateByPage.Clear();
            OnClear();
        }
        private protected virtual void OnClear() { }
    }

}

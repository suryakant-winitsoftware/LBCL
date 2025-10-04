using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.Collection.BalanceConfirmation
{
    public partial class MaintainBalanceConfirmation : ComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        public List<IBalanceConfirmation> BalanceConfirmationRecords { get; set; } = new List<IBalanceConfirmation>();
        private List<DataGridColumn> BalanceConfirmationColumns { get; set; } = new List<DataGridColumn>();
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            SetHeaderName();
            await _balanceConfirmationViewmodel.GetBalanceConfirmationTableListDetails();
            BalanceConfirmationRecords = _balanceConfirmationViewmodel.BalanceConfirmationListDetails;
            GridColumns();
            IsInitialised = true;
            _loadingService.HideLoading();
        }
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? iDataBreadcrumbService;
        protected void SetHeaderName()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "Maintain Balance Confirmation",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
        {
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Primary Channel Partner Balance Confirmation", IsClickable = true, URL = "balanceconfirmation"},
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = "Maintain Balance Confirmation", IsClickable = false},
        }
            };
        }
        public void GridColumns()
        {
            BalanceConfirmationColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Channel Partner Name", GetValue = s => ((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation)s).Store,  IsSortable = true, SortField = "CreatedTime"  },
                new DataGridColumn { Header = "Start Date", GetValue = s => ((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation)s).StartDate ,  IsSortable = true, SortField = "ReferenceNumber"  },
                new DataGridColumn { Header = "End Date", GetValue = s => ((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation)s).EndDate,  IsSortable = true, SortField = "OrderType" },
                new DataGridColumn { Header = "Total Outstanding Balance(₹)", GetValue = s => CommonFunctions.RoundForSystem(((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation)s).ClosingBalance),  IsSortable = true, SortField = "TransactionType" },
                new DataGridColumn { Header = "Status", GetValue = s => ((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation)s).Status , IsSortable = true, SortField = "Amount" },
                 new DataGridColumn
                 {
                     Header = "Actions",
                     IsButtonColumn = true,
                     ButtonActions = new List<ButtonAction>
                     {
                         new ButtonAction
                         {
                             Text = "✎",
                             Action = async item => await RedirectToViewDisputes((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation)item)
                         },
                     }
                 }
            };
        }
        private async Task RedirectToViewDisputes(Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation item)
        {
            try
            {
                _navigationManager.NavigateTo("balanceconfirmation?IsView=" + "true");
            }
            catch (Exception ex)
            {

            }
        }
    }
}

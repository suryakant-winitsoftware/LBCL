using Microsoft.AspNetCore.Components;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.Collection.BalanceConfirmation
{
    public partial class DisputeDetails : ComponentBase
    {
        public List<ISelectionItem> TabItems { get; set; } = new List<ISelectionItem>();
        private List<DataGridColumn> PendingDisputeColumns { get; set; } = new List<DataGridColumn>();
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IBalanceConfirmation> PendingDisputes { get; set; } = new List<Winit.Modules.CollectionModule.Model.Interfaces.IBalanceConfirmation>();
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            SetHeaderName();
            PendingDisputes = new List<Winit.Modules.CollectionModule.Model.Interfaces.IBalanceConfirmation>();
            TabItems.Add(new SelectionItem { Label = "Pending to Resolve",IsSelected=true });
            TabItems.Add(new SelectionItem { Label = "Resolved" });
            GridColumns();
            await _balanceConfirmationViewmodel.GetBalanceConfirmationTableDetails(_iAppUser.Emp.Code);
            if (_balanceConfirmationViewmodel.BalanceConfirmationDetails.Status == "Pending")
            {
                PendingDisputes.Add(_balanceConfirmationViewmodel.BalanceConfirmationDetails);
            }
            _loadingService.HideLoading();
            StateHasChanged();
        }
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? iDataBreadcrumbService;
        protected void SetHeaderName()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "Balance Confirmation Disputes",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
        {
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Balance Confirmation Disputes", IsClickable = false },
        }
            };
        }
        public void GridColumns()
        {
            PendingDisputeColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Channel Partner Code / Name", GetValue = s => ((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation)s).Store},
                new DataGridColumn { Header = "Zone", GetValue = s =>  "North"   },
                new DataGridColumn { Header = "Branch", GetValue = s => "Hyderabad"},
                new DataGridColumn { Header = "CMI Balance", GetValue = s => ((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation)s).ClosingBalance },
                new DataGridColumn { Header = "Custommer Dispute Amount", GetValue = s => ((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation)s).TotalDisputeAmount },
                new DataGridColumn {Header = "Status", GetValue = s =>((Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation) s).Status},
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
        private async Task RedirectToViewDisputes(Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation item )
        {
            try
            {
                _navigationManager.NavigateTo("viewdispute?UID="+ item.UID);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            try
            {
                TabItems.ForEach(item => item.IsSelected = false);
                selectionItem.IsSelected = !selectionItem.IsSelected;
                await _balanceConfirmationViewmodel.GetBalanceConfirmationTableDetails(_iAppUser.Emp.Code);
                PendingDisputes.Clear();
                if (_balanceConfirmationViewmodel.BalanceConfirmationDetails.Status == "Pending" && selectionItem.Label == "Pending to Resolve")
                {
                    PendingDisputes.Add(_balanceConfirmationViewmodel.BalanceConfirmationDetails);
                }
                if(_balanceConfirmationViewmodel.BalanceConfirmationDetails.Status == "Balance Approval Completed" && selectionItem.Label == "Resolved")
                {
                    PendingDisputes.Add(_balanceConfirmationViewmodel.BalanceConfirmationDetails);
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
    }
}

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.Tax.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace WinIt.Pages.Collection.CashCollectionDeposit
{
    public partial class MaintainCashCollectionDeposit
    {
        List<ISelectionItem> TabHeaders { get; set; } = new List<ISelectionItem> { new SelectionItem { Label="Pending", Code= "Pending", IsSelected=true },
                                                    new SelectionItem { Label = "Approved", Code = "Approved" },
                                                    new SelectionItem { Label = "Rejected", Code = "Rejected" }, 
                                                    new SelectionItem { Label = "Action Required", Code = "Action Required" }, };
        private SelectionManager TabSM;
        private List<DataGridColumn> Columns { get; set; } = new List<DataGridColumn>();
        public List<IAccCollectionDeposit> accCollectionDeposit { get; set; } = new List<IAccCollectionDeposit>();
        
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            TabSM = new SelectionManager(TabHeaders, SelectionMode.Single);
            GridColumns();
            await BindData("Pending");
            StateHasChanged();
            _loadingService.HideLoading();
        }
        private async void ActiveTabChanged(Winit.Shared.Models.Common.ISelectionItem selectedTab)
        {
            if (!selectedTab.IsSelected)
            {
                TabSM.Select(selectedTab);
                await BindData(selectedTab.Code);
            }
            StateHasChanged(); 
        }

        public async Task BindData(string Status)
        {
            try
            {
                accCollectionDeposit = await _cashCollectionDepostViewModel.GetRequestReceipts(Status);
                StateHasChanged();
            }
            catch(Exception ex)
            {

            }
        }
        public void GridColumns()
        {
            Columns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["Request No"], GetValue = s => ((AccCollectionDeposit)s).RequestNo },
                new DataGridColumn { Header = @Localizer["Request Date"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((AccCollectionDeposit)s).RequestDate) },
                new DataGridColumn { Header = @Localizer["Bank Name"], GetValue = s => ((AccCollectionDeposit)s).BankUID},
                new DataGridColumn { Header = @Localizer["Branch"], GetValue = s => ((AccCollectionDeposit)s).Branch },
                new DataGridColumn { Header = @Localizer["Amount"], GetValue = s => ((AccCollectionDeposit)s).Amount},
                new DataGridColumn { Header = @Localizer["Status"], GetValue = s => ((AccCollectionDeposit)s).Status },
                new DataGridColumn
                {
                    Header = @Localizer["action"],
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            Text = "View",
                            Action = async item => await HandleAction1_Product((AccCollectionDeposit)item)
                        },
                    }
                }
            };
        }
        private async Task HandleAction1_Product(AccCollectionDeposit item)
        {
            try
            {
                _navigationManager.NavigateTo("approveorrejectdeposit?RequestNo=" + item.RequestNo);
            }
            catch (Exception ex)
            {

            }
        }
    }
}

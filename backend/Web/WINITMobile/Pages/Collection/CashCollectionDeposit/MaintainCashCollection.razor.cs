using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace WINITMobile.Pages.Collection.CashCollectionDeposit
{
    public partial class MaintainCashCollection : ComponentBase
    {
        public List<string> tabItems { get; set; } = new List<string> { ConstantVariables.Pending, ConstantVariables.Approved, ConstantVariables.Rejected, ConstantVariables.ActionRequired };
        private string selectedTab { get; set; } = ConstantVariables.Pending;
        public List<IAccCollectionDeposit> accCollectionDeposit { get; set; } = new List<IAccCollectionDeposit>();

        protected override async Task OnInitializedAsync()
        {
            await OnTabSelect(selectedTab);
        }
        private async void ChangeTab(string tab)
        {
            selectedTab = tab;
            await OnTabSelect(selectedTab);
        }
        public async Task OnTabSelect(string Payment)
        {
            accCollectionDeposit = await _collectionDepositViewModel.GetRequestReceipts(selectedTab);
            StateHasChanged();
        }
        public async Task AddNewRequest()
        {
            _navigationManager.NavigateTo("addnewdepositrequest?IsView=false");
        }
        public async Task View(IAccCollectionDeposit collection)
        {
            _navigationManager.NavigateTo("addnewdepositrequest?IsView=true&RequestNo=" + collection.RequestNo + "&Tab=" + selectedTab);
        }
    }
}

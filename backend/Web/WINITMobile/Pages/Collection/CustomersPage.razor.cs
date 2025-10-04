using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using WINITMobile.Models.TopBar;

namespace WINITMobile.Pages.Collection
{
    public partial class CustomersPage : WINITMobile.Pages.Base.BaseComponentBase
    {
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }

        public IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore> _storelist { get; set; }
        private List<Testing> searchList { get; set; } = new List<Testing> {
            new Testing { CustomerNo="407AO789859634", Actual="11.35", Due="11.35"},
            new Testing { CustomerNo="8890AO7898578", Actual="11.35", Due="11.35"},
            new Testing { CustomerNo="7569SP04587598", Actual="11.35", Due="11.35"},
            new Testing { CustomerNo="095UL89893536", Actual="11.35", Due="11.35"},
            new Testing { CustomerNo="94367AWR78977", Actual="11.35", Due="11.35"},
            new Testing { CustomerNo="0812DEFGG996", Actual="11.35", Due="11.35"}
        };
        private DateOnly InvDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        private DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        protected override async void OnInitialized()
        {
            await SetTopBar();
            await _collectionModuleViewModel.PopulateViewModel();
            _storelist = _collectionModuleViewModel._customersList;
            LoadResources(null, _languageService.SelectedCulture);
        }

        async Task SetTopBar()
        {
            MainButtons buttons = new MainButtons()
            {
                
                TopLabel = @Localizer["customers"]
            };
            await Btnname.InvokeAsync(buttons);
        }
        public async void DivClicked(IStore testing)
        {
            string CodeName = "[" + _appUser.SelectedCustomer.Code + "]" + _appUser.SelectedCustomer.Name;
            _collectionModuleViewModel.ConsolidatedReceiptNumber = _appUser.SelectedCustomer.Code + "_" + _collectionModuleViewModel.sixGuidstring() ;
            _collectionModuleViewModel.ReceiptNumber = _appUser.SelectedCustomer.Code + "_" + _collectionModuleViewModel.sixGuidstring() ;
            _collectionModuleViewModel.CustomerName = _appUser.SelectedCustomer.Name;
            _navigationManager.NavigateTo("Payment?UserCode="+ _appUser.SelectedCustomer.StoreUID + "&Name=" + CodeName + "&Code=" + _appUser.SelectedCustomer.Code);
        }
        public class Testing
        {
            public string CustomerNo { get; set; }
            public string Actual { get; set; }
            public string Due { get; set; }
        }
    }
}

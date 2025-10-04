using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITMobile.Pages.Base;
using WINITMobile.Data;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Events;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Shared.Models.Constants;
using WINITMobile.Models.TopBar;
using Winit.UIComponents.Common.CustomControls;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Modules.WHStock.BL.Classes;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Modules.Common.BL;

namespace WINITMobile.Pages.LoadRequestM
{
    public partial class ViewLoadRequest : BaseComponentBase, IDisposable
    {
        [Parameter]
        public string RequestLoadType { get; set; }
        private string Heading => RequestLoadType == "Load" ? @Localizer["view_load_request"] : @Localizer["view_unload_request"];
        //private string ButtonText => RequestLoadType == "Load" ? "LOAD REQ" : "UNLOAD REQ";
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }


        public string ActiveTab = StockRequestStatus.Draft;
        List<DataGridColumn> LoadRequestColumns;
        SelectionManager SelectedTab;
        public bool IsNewRequestAllowed = true;



        public List<ISelectionItem> TabSelectionItems = new List<ISelectionItem>
         {
              new SelectionItem{ Label="Draft", Code=StockRequestStatus.Draft, UID="1"},
              new SelectionItem{ Label="Requested", Code=StockRequestStatus.Requested, UID="2"},
              new SelectionItem{ Label="Approved", Code=StockRequestStatus.Approved, UID="3"},
              new SelectionItem{ Label="Collected", Code=StockRequestStatus.Collected, UID="4"},
              new SelectionItem{ Label="Rejected", Code=StockRequestStatus.Rejected, UID="5"},
         };
        protected override void OnInitialized()
        {

            WINITMobile.Data.SideBarService _sidebar = new WINITMobile.Data.SideBarService();
            //_sidebar.IsBackRestricted = true; //NeedToAsk

        }

        protected override async Task OnInitializedAsync()
        {


            _loadingService.ShowLoading(@Localizer["loading_load_request"]);
            await Task.Run(async () =>
            {
                try
                {
                    if(_appUser.SelectedRoute == null)
                    {
                        IsNewRequestAllowed = false;
                    }
                    await SetViewState();
                    InvokeAsync(() =>
                    {
                        _loadingService.HideLoading();
                       // SetTopBar();
                        StateHasChanged(); // Ensure UI reflects changes
                    });
                }
                catch (Exception ex)
                {

                }
            });
            LoadResources(null, _languageService.SelectedCulture);

        }
        public async Task SetViewState()
        {
            await _LoadRequestView.PopulateViewModel(ActiveTab);
            SetVariales();
        }

        public void SetVariales()
        {
            SelectedTab = new SelectionManager(TabSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
            TabSelectionItems[0].IsSelected = ActiveTab == StockRequestStatus.Draft;
        }
        public void Dispose()
        {
            // Dispose of objects and perform cleanup here
        }
        //async Task SetTopBar()
        //{
        //    MainButtons buttons = new MainButtons()
        //    {


        //        TopLabel = "Stock Request",
        //        BottomLabel = "",

        //        UIButton1 = new Models.TopBar.Buttons
        //        {

        //            Action = () => TakeNewRequest(RequestLoadType),
        //            ButtonText = "LOAD REQ",
        //            ButtonType = Models.TopBar.ButtonType.Text,
        //            IsVisible = true

        //        },
        //         UIButton2 = new Models.TopBar.Buttons
        //         {

        //             Action = () => TakeNewRequest(RequestType.Unload.ToString()),
        //             ButtonText = "UNLOAD REQ",
        //             ButtonType = Models.TopBar.ButtonType.Text,
        //             IsVisible = true

        //         }
        //    };
        //    await Btnname.InvokeAsync(buttons);
        //}
        public List<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemViewUI> AppProcessWHStockRequestItemViewM = new List<IWHStockRequestItemViewUI>();
       
        public async void OnTabSelect(ISelectionItem selectionItem)
        {
            if (!selectionItem.IsSelected)
            {
                SelectedTab.Select(selectionItem);
                ActiveTab = selectionItem.Code;
                if (ActiveTab == StockRequestStatus.Approved)
                {
                    await _LoadRequestView.PopulateViewModel(ActiveTab);


                    AppProcessWHStockRequestItemViewM = new List<IWHStockRequestItemViewUI>(_LoadRequestView.DisplayWHStockRequestItemView);


                    await _LoadRequestView.PopulateViewModel(StockRequestStatus.Processed);

                    AppProcessWHStockRequestItemViewM.AddRange(_LoadRequestView.DisplayWHStockRequestItemView);


                    _LoadRequestView.DisplayWHStockRequestItemView = new List<IWHStockRequestItemViewUI>(AppProcessWHStockRequestItemViewM);
                }
                else
                {
                    await _LoadRequestView.PopulateViewModel(ActiveTab);
                }
                
            }
        }

        public void ViewEditQuantity(IWHStockRequestItemView item)
        {

            var uid = item.UID;
            NavigationManager.NavigateTo($"addeditloadrequest?UID={uid}");
        }
        public void TakeNewRequest( string RequestType)
        {
            NavigationManager.NavigateTo($"addeditloadrequest/{RequestType}?RequestType={RequestType}");
        }
        private void RequestLoad()
        {
            var requestType = RequestLoadType == "Load" ? RequestType.Load.ToString() : RequestType.Unload.ToString();

            TakeNewRequest(requestType);
        }
    }
}



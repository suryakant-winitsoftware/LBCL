using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Summery;

public partial class DeliverySummery : BaseComponentBase, IDisposable
{
    private string SearchString = "";
    private SelectionManager TabselectionManager;
    protected override async Task OnInitializedAsync()
    {
        _backbuttonhandler.ClearCurrentPage();
        _SalesSummeryViewModel.PopulateViewModel(_SalesOrderBL);
        TabselectionManager = new SelectionManager(_SalesSummeryViewModel.TabList, Winit.Shared.Models.Enums.SelectionMode.Single);
        ActiveTabChanged(_SalesSummeryViewModel.TabList[0]);
        LoadResources(null, _languageService.SelectedCulture);

    }
    private void OnDateChange()
    {
        ActiveTabChanged(TabselectionManager.GetSelectedSelectionItems().FirstOrDefault());
    }

    private void OnSearching(string value)
    {
        SearchString = value;
        _SalesSummeryViewModel.ApplySearch(value);
    }

    private void ActiveTabChanged(ISelectionItem selectionItem)
    {
        TabselectionManager.Select(selectionItem);
        _SalesSummeryViewModel.OnTabSelected(selectionItem);
        if (!string.IsNullOrEmpty(SearchString))
        {
            _SalesSummeryViewModel.ApplySearch(SearchString);
        }
    }
    //private void OnItemSelected(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView salesSummaryItemView)
    //{
    //    if (salesSummaryItemView != null)
    //    {
    //        _dataManager.SetData("salesSummaryItemView", salesSummaryItemView);
    //        Navigate("createsalesorder", "", null);
    //    }

    //}

    private void OnItemSelected(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView salesSummaryItemView)
    {
        if (salesSummaryItemView != null && salesSummaryItemView.OrderStatus == SalesOrderStatus.DRAFT)
        {

           // _navigationManager.NavigateTo("createsalesorder",true);
            NavigateTo("createsalesorder");

            _dataManager.SetData("salesSummaryItemView", salesSummaryItemView);


        }
        else
        {
            //_navigationManager.NavigateTo($"previewpageforSummary/{salesSummaryItemView.SalesOrderUID}");
              NavigateTo($"previewpageforSummary/{salesSummaryItemView.SalesOrderUID}");
        }

    }

    public void Dispose()
    {

    }
}

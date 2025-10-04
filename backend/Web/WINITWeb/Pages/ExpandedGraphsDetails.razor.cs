using Microsoft.AspNetCore.Components;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.UIModels.Common.GraphModels;

namespace WinIt.Pages
{
    public partial class ExpandedGraphsDetails : ComponentBase
    {
        public string SelectedGraph { get; set; }

        public List<ClusteredBarModel> ClusteredBarDataSource = new List<ClusteredBarModel>
{
    new ClusteredBarModel { Label = "Primary Qty", Data = new[] { 673, 505, 900, 500,500,500,800 }, Color = "#E56C37" },
    new ClusteredBarModel { Label = "Sell Through Qty", Data = new[] { 764, 625, 700, 400,400,400,820 }, Color = "#2C9E12" }
};
        private string[] BarLabels = { "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov" };

        public List<BarChartModel> BarDataSource = new List<BarChartModel>
{
    new BarChartModel { Label = "Product A", Data = new[] { 70,80,70,80,70,80,80,80,80,70,80,70,80,70}, Color = "#077ADF" }
};
        private string[] SingleBarLabels = { "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star", "24K3Star" };

        private List<DoughNutGraphModel> DoghnutGraphDataSource { get; set; } = new List<DoughNutGraphModel>
{
    new DoughNutGraphModel { Label = "Aman Electronics", Value = 25, Color = "#01A1FF" },
    new DoughNutGraphModel { Label = "Burnwal Telecom", Value = 30, Color = "#BE9502" },
    new DoughNutGraphModel { Label = "Monika Electronics", Value = 20, Color = "#FFAA00" },
    new DoughNutGraphModel { Label = "Electronics Central Pvt.Ltd", Value = 5, Color = "#498CC5" },
    new DoughNutGraphModel { Label = "BB Electronics", Value = 20, Color = "#4CD964" },

};
        protected override async Task OnInitializedAsync()
        {
            SelectedGraph = _commonFunctions.GetParameterValueFromURL("SelectedGraph");
            StateHasChanged();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                StateHasChanged();
            }
        }
        public async Task BackToDashBoard()
        {
            try
            {
                _navigationManager.NavigateTo("DashBoard");
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

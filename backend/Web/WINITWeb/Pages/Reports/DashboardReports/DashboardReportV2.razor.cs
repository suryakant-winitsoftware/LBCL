using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WinIt.Pages.Reports.DashboardReports
{
    public partial class DashboardReportV2 : ComponentBase
    {
        public DateTime startDate { get; set; } = DateTime.Today;
        public bool isLoading { get; set; } = false;

        // ECharts Options
        public object barChartOptions { get; set; }
        public object horizontalBarChartOptions { get; set; }
        public object lineChartOptions { get; set; }
        public object pieChartOptions { get; set; }

        protected override void OnInitialized()
        {
            InitializeCharts();
        }

        private void InitializeCharts()
        {
            // Bar Chart Options
            barChartOptions = new
            {
                tooltip = new
                {
                    trigger = "axis",
                    axisPointer = new { type = "shadow" }
                },
                legend = new
                {
                    data = new[] { "Sales 2023", "Sales 2022", "Target" },
                    top = 10
                },
                grid = new
                {
                    left = "3%",
                    right = "4%",
                    bottom = "15%",
                    containLabel = true
                },
                dataZoom = new[]
                {
                    new
                    {
                        type = "slider",
                        show = true,
                        xAxisIndex = new[] { 0 },
                        start = 0,
                        end = 50,
                        bottom = 0
                    }
                },
                xAxis = new
                {
                    type = "category",
                    data = new[] { 
                        "Electronics", "Clothing", "Food", "Books", "Sports",
                        "Furniture", "Toys", "Jewelry", "Beauty", "Automotive",
                        "Home", "Garden", "Pet", "Office", "Health"
                    }
                },
                yAxis = new
                {
                    type = "value",
                    name = "Sales Amount"
                },
                series = new[]
                {
                    new
                    {
                        name = "Sales 2023",
                        type = "bar",
                        data = new[] { 12, 19, 3, 5, 2, 8, 15, 7, 9, 11, 6, 4, 13, 10, 14 },
                        itemStyle = new { color = "#FF6384" }
                    },
                    new
                    {
                        name = "Sales 2022",
                        type = "bar",
                        data = new[] { 8, 15, 2, 4, 1, 6, 12, 5, 7, 9, 4, 3, 10, 8, 11 },
                        itemStyle = new { color = "#36A2EB" }
                    },
                    new
                    {
                        name = "Target",
                        type = "bar",
                        data = new[] { 15, 20, 5, 7, 3, 10, 18, 9, 12, 14, 8, 6, 16, 13, 17 },
                        itemStyle = new { color = "#FFCE56" }
                    }
                }
            };

            // Horizontal Bar Chart Options
            horizontalBarChartOptions = new
            {
                tooltip = new
                {
                    trigger = "axis",
                    axisPointer = new { type = "shadow" }
                },
                legend = new
                {
                    data = new[] { "Revenue", "Cost", "Profit" },
                    top = 10
                },
                grid = new
                {
                    left = "15%",
                    right = "4%",
                    bottom = "3%",
                    containLabel = true
                },
                dataZoom = new[]
                {
                    new
                    {
                        type = "slider",
                        show = true,
                        yAxisIndex = new[] { 0 },
                        start = 0,
                        end = 50,
                        left = 0
                    }
                },
                xAxis = new
                {
                    type = "value",
                    name = "Amount ($)"
                },
                yAxis = new
                {
                    type = "category",
                    data = new[] { 
                        "Product A", "Product B", "Product C", "Product D", "Product E",
                        "Product F", "Product G", "Product H", "Product I", "Product J",
                        "Product K", "Product L", "Product M", "Product N", "Product O"
                    }
                },
                series = new[]
                {
                    new
                    {
                        name = "Revenue",
                        type = "bar",
                        data = new[] { 150, 120, 90, 80, 60, 140, 110, 85, 75, 55, 130, 100, 95, 70, 50 },
                        itemStyle = new { color = "#FF6384" }
                    },
                    new
                    {
                        name = "Cost",
                        type = "bar",
                        data = new[] { 100, 80, 60, 50, 40, 90, 70, 55, 45, 35, 85, 65, 60, 45, 30 },
                        itemStyle = new { color = "#36A2EB" }
                    },
                    new
                    {
                        name = "Profit",
                        type = "bar",
                        data = new[] { 50, 40, 30, 30, 20, 50, 40, 30, 30, 20, 45, 35, 35, 25, 20 },
                        itemStyle = new { color = "#FFCE56" }
                    }
                }
            };

            // Line Chart Options
            lineChartOptions = new
            {
                tooltip = new
                {
                    trigger = "axis"
                },
                legend = new
                {
                    data = new[] { "Sales 2023", "Sales 2022" },
                    top = 10
                },
                grid = new
                {
                    left = "3%",
                    right = "4%",
                    bottom = "3%",
                    containLabel = true
                },
                xAxis = new
                {
                    type = "category",
                    boundaryGap = false,
                    data = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" }
                },
                yAxis = new
                {
                    type = "value",
                    name = "Sales"
                },
                series = new[]
                {
                    new
                    {
                        name = "Sales 2023",
                        type = "line",
                        data = new[] { 65, 59, 80, 81, 56, 55 },
                        itemStyle = new { color = "#36A2EB" }
                    },
                    new
                    {
                        name = "Sales 2022",
                        type = "line",
                        data = new[] { 45, 49, 60, 71, 46, 45 },
                        itemStyle = new { color = "#FF6384" }
                    }
                }
            };

            // Pie Chart Options
            pieChartOptions = new
            {
                tooltip = new
                {
                    trigger = "item",
                    formatter = "{a} <br/>{b}: {c} ({d}%)"
                },
                legend = new
                {
                    orient = "vertical",
                    left = 10,
                    data = new[] { "Online", "Store", "Wholesale" }
                },
                series = new[]
                {
                    new
                    {
                        name = "Revenue Distribution",
                        type = "pie",
                        radius = new[] { "50%", "70%" },
                        avoidLabelOverlap = false,
                        itemStyle = new
                        {
                            borderRadius = 10,
                            borderColor = "#fff",
                            borderWidth = 2
                        },
                        label = new
                        {
                            show = false,
                            position = "center"
                        },
                        emphasis = new
                        {
                            label = new
                            {
                                show = true,
                                fontSize = "20",
                                fontWeight = "bold"
                            }
                        },
                        labelLine = new
                        {
                            show = false
                        },
                        data = new[]
                        {
                            new { value = 300, name = "Online", itemStyle = new { color = "#FF6384" } },
                            new { value = 50, name = "Store", itemStyle = new { color = "#36A2EB" } },
                            new { value = 100, name = "Wholesale", itemStyle = new { color = "#FFCE56" } }
                        }
                    }
                }
            };
        }

        public async Task LoadReportData()
        {
            try
            {
                isLoading = true;
                // Add your data loading logic here
                await Task.Delay(1000); // Simulated delay
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading report data: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
} 
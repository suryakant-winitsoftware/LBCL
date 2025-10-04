using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace WinIt.Pages.Reports.DashboardReports
{
    public partial class DashboardReport : ComponentBase // Ensure the class inherits from ComponentBase
    {
        private DateTime startDate = DateTime.Today;
        private bool isLoading = false;

        // Sample data for different chart types
        public object barChartData { get; set; } = new
        {
            labels = new[] { 
                "Electronics", "Clothing", "Food", "Books", "Sports",
                "Furniture", "Toys", "Jewelry", "Beauty", "Automotive",
                "Home", "Garden", "Pet", "Office", "Health"
            },
            datasets = new[]
            {
                new
                {
                    label = "Sales 2023",
                    data = new[] { 12, 19, 3, 5, 2, 8, 15, 7, 9, 11, 6, 4, 13, 10, 14 },
                    backgroundColor = "#FF6384"
                },
                new
                {
                    label = "Sales 2022",
                    data = new[] { 8, 15, 2, 4, 1, 6, 12, 5, 7, 9, 4, 3, 10, 8, 11 },
                    backgroundColor = "#36A2EB"
                },
                new
                {
                    label = "Target",
                    data = new[] { 15, 20, 5, 7, 3, 10, 18, 9, 12, 14, 8, 6, 16, 13, 17 },
                    backgroundColor = "#FFCE56"
                }
            }
        };

        public object horizontalBarChartData { get; set; } = new
        {
            labels = new[] { 
                "Product A", "Product B", "Product C", "Product D", "Product E",
                "Product F", "Product G", "Product H", "Product I", "Product J",
                "Product K", "Product L", "Product M", "Product N", "Product O"
            },
            datasets = new[]
            {
                new
                {
                    label = "Revenue",
                    data = new[] { 150, 120, 90, 80, 60, 140, 110, 85, 75, 55, 130, 100, 95, 70, 50 },
                    backgroundColor = "#FF6384"
                },
                new
                {
                    label = "Cost",
                    data = new[] { 100, 80, 60, 50, 40, 90, 70, 55, 45, 35, 85, 65, 60, 45, 30 },
                    backgroundColor = "#36A2EB"
                },
                new
                {
                    label = "Profit",
                    data = new[] { 50, 40, 30, 30, 20, 50, 40, 30, 30, 20, 45, 35, 35, 25, 20 },
                    backgroundColor = "#FFCE56"
                }
            }
        };

        private object lineChartData = new
        {
            labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
            datasets = new[]
            {
                    new
                    {
                        label = "Target 2023",
                        data = new[] { 65, 59, 80, 81, 56, 55 },
                        borderColor = "#36A2EB",
                        tension = 0.1
                    },
                    new
                    {
                        label = "Sales 2023",
                        data = new[] { 60, 70, 100, 20, 80, 60 },
                        borderColor = "#FF6384",
                        tension = 0.1
                    }
                }
        };

        private object pieChartData = new
        {
            labels = new[] { "Online", "Store", "Wholesale" },
            datasets = new[]
            {
                    new
                    {
                        data = new[] { 300, 50, 100 },
                        backgroundColor = new[] { "#FF6384", "#36A2EB", "#FFCE56" }
                    }
                }
        };

        private object doughnutChartData = new
        {
            labels = new[] { "New", "Returning", "Inactive" },
            datasets = new[]
            {
                    new
                    {
                        data = new[] { 40, 35, 25 },
                        backgroundColor = new[] { "#FF6384", "#36A2EB", "#FFCE56" }
                    }
                }
        };

        // Chart options
        public object barChartOptions { get; set; } = new
        {
            responsive = true,
            maintainAspectRatio = false,
            plugins = new
            {
                legend = new { display = true },
                title = new { display = true, text = "Sales Comparison by Category" }
            },
            scales = new
            {
                x = new
                {
                    ticks = new { maxRotation = 45, minRotation = 45 },
                    grid = new { display = false }
                },
                y = new
                {
                    beginAtZero = true,
                    title = new { display = true, text = "Sales Amount" }
                }
            }
        };

        public object horizontalBarChartOptions { get; set; } = new
        {
            indexAxis = "y",
            responsive = true,
            maintainAspectRatio = false,
            plugins = new
            {
                legend = new { display = true },
                title = new { display = true, text = "Product Financial Analysis" }
            },
            scales = new
            {
                x = new
                {
                    beginAtZero = true,
                    title = new { display = true, text = "Amount ($)" }
                },
                y = new
                {
                    grid = new { display = false }
                }
            }
        };

        private object lineChartOptions = new
        {
            responsive = true,
            plugins = new
            {
                legend = new { display = true },
                title = new { display = true, text = "Monthly Sales Trend" }
            }
        };

        private object pieChartOptions = new
        {
            responsive = true,
            plugins = new
            {
                legend = new { display = true },
                title = new { display = true, text = "Revenue Distribution" }
            }
        };

        private object doughnutChartOptions = new
        {
            responsive = true,
            plugins = new
            {
                legend = new { display = true },
                title = new { display = true, text = "Customer Distribution" }
            }
        };

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadReportData();
        }

        private async Task LoadReportData()
        {
            try
            {
                isLoading = true;
                // Add your data loading logic here
                // This is where you would typically:
                // 1. Call your API services
                // 2. Process the data
                // 3. Update the chart data
                await Task.Delay(1000); // Simulated delay
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error loading report data: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
} 
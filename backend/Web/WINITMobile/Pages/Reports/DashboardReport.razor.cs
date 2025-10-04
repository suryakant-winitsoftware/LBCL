using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace WINITMobile.Pages.Reports
{
    public partial class DashboardReport : ComponentBase
    {
        public DateTime startDate { get; set; } = DateTime.Today;
        public bool isLoading { get; set; } = false;

        // ECharts Options
        public object achievementChartOptions { get; set; }

        protected override void OnInitialized()
        {
            InitializeCharts();
        }

        private void InitializeCharts()
        {
            // Achievement Progress Chart Options
            achievementChartOptions = new
            {
                title = new
                {
                    text = $"{((200.0 / 1000) * 100).ToString("0")}%",
                    subtext = "Achieved",
                    left = "center",
                    top = "55%",
                    textStyle = new
                    {
                        fontSize = 20,
                        fontWeight = "bold"
                    },
                    subtextStyle = new
                    {
                        fontSize = 14
                    }
                },
                tooltip = new
                {
                    trigger = "item"
                },
                legend = new
                {
                    top = "5%",
                    left = "center",
                    show = false
                },
                series = new object[]
                {
                    new
                    {
                        name = "Access From",
                        type = "pie",
                        radius = new[] { "40%", "70%" },
                        center = new[] { "50%", "70%" },
                        startAngle = 180,
                        endAngle = 360,
                        labelLine = new
                        {
                            show = false
                        },
                        label = new
                        {
                            show = false
                        },
                        data = new object[]
                        {
                            new 
                            {
                                value = 200,
                                name = "Achieved",
                                itemStyle = new { color = "#4CAF50" },
                                emphasis = new { disabled = true }
                            },
                            new 
                            {
                                value = 800,
                                name = "Remaining",
                                itemStyle = new { color = "#E0E0E0" },
                                tooltip = new { show = false },
                                emphasis = new { disabled = true }
                            }
                        }
                    }
                },
                graphic = new object[]
                {
                    new
                    {
                        type = "text",
                        left = "28%",
                        top = "75%",
                        style = new
                        {
                            text = "0",
                            font = "bold 14px sans-serif",
                            fill = "#666"
                        }
                    },
                    new
                    {
                        type = "text",
                        right = "28%",
                        top = "75%",
                        style = new
                        {
                            text = "1000",
                            font = "bold 14px sans-serif",
                            fill = "#666"
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
                InitializeCharts(); // Refresh chart data
                StateHasChanged(); // Force UI update
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading report data: {ex.Message}");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
    }
} 
let myBarChart;

window.BarChart = function (chartId, dataset, barLabels, backgroundColor) {
    var parsedData = JSON.parse(dataset);
    const data = {
        labels: barLabels,
        datasets: parsedData.map((item) => ({
            label: item.Label,
            data: item.Data,
            backgroundColor: item.Color,// Assign color dynamically
            borderRadius: 5
        }))
    };
    const barConfig = {
        type: 'bar',
        data: data,
        options: {
            plugins: {
                legend: {
                    display: false,
                },
                tooltip: {
                    enabled: true
                },
                datalabels: {
                    display: true, // Show data labels
                    anchor: 'end', // Position of the label on the bar
                    align: 'end', // Align it at the end (top of the bar)
                    color: 'black', // Color of the labels
                    font: {
                        weight: 'normal', // Make the font bold
                        size: 10 // Set the font size
                    },
                    formatter: function (value, context) {
                        return value; // Display the actual value of the data
                    },
                }
            },
            responsive: true,
            scales: {
                x: {
                    ticks: {
                        callback: function (value, index) {
                            return barLabels[index]; // Display the label from barLabels
                        },
                        font: {
                            family: 'Montserrat', // Apply Montserrat font to X-axis ticks
                            size: 10
                        },
                        maxRotation: 45, // Rotate labels at 45 degrees
                        minRotation: 45  // Ensures a cross-aligned position
                    },
                    grid: {
                        display: false // Remove gridlines on X-axis
                    },
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 20, // Step size remains 10000
                        callback: function (value) {
                            return value /*=== 0 ? '0' : (value / 1000) + 'k'*/; // Format values with 'k'
                        },
                        font: {
                            family: 'Montserrat', // Apply Montserrat font to X-axis ticks
                            size: 10
                        }
                    },
                    grid: {
                        display: false // Remove gridlines on X-axis
                    },
                    max: 120 // Optional cap at a specific value
                }
            },
            layout: {
                padding: 10
            },
        },
        plugins: [ChartDataLabels]
    };

    const chartElement = document.getElementById(chartId);
    chartElement.style.backgroundColor = "#FFFFFF";;

    const ctx = chartElement.getContext('2d');

    if (myBarChart) {
        myBarChart.destroy();
    }


    window.myBarChart = new Chart(ctx, barConfig);
};
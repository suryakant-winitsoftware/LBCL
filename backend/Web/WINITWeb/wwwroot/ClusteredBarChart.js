let myClusteredBarChart;

window.ClusteredBarChart = function (chartId, dataset, barLabels, backgroundColor) {
    var parsedData = JSON.parse(dataset);
    const data = {
        labels: barLabels,
        datasets: parsedData.map((item) => ({
            label: item.Label,
            data: item.Data,
            backgroundColor: item.Color, // Assign color dynamically
            borderRadius: 5,
            barPercentage: 0.9,       // Shrinks bar width within the category to create spacing
            categoryPercentage: 0.7,
            pointRadius: 5,
        }))
    };
    const barConfig = {
        type: 'bar',
        data: data,
        options: {
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        fontColor: '#474747',
                        fontFamily: '6px Montserrat',
                    },
                },
                tooltip: {
                    enabled: true
                },
                title: {
                    display: true,
                    text: '',
                    color: 'black',
                    font: {
                        weight: 'normal',
                        size: 20
                    }
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
                    stacked: false,
                    grid: {
                        display: false // Remove gridlines on X-axis
                    },
                    
                    //title: {
                    //    display: true,
                    //    text: 'X-Axis Label', // Add your X-axis label here
                    //    color: 'black',
                    //    font: {
                    //        size: 14,
                    //        weight: 'bold'
                    //    }
                    //}
                },
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 200, // This will set the steps on the y-axis to 200
                        callback: function (value) {
                            return value; // Ensure values are shown as-is
                        },
                        font: {
                            family: 'Montserrat', // Apply Montserrat font to X-axis ticks
                            size: 10
                        }
                    },
                    // Optional: Adjust max if you want to cap it at a specific value
                    max: 1200,
                    //title: {
                    //    display: true,
                    //    text: 'Y-Axis Label', // Add your Y-axis label here
                    //    color: 'black',
                    //    font: {
                    //        size: 14,
                    //        weight: 'bold'
                    //    }
                    //}
                    grid: {
                        display: false // Remove gridlines on Y-axis
                    }
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

    if (myClusteredBarChart) {
        myClusteredBarChart.destroy();
    }


    window.myClusteredBarChart = new Chart(ctx, barConfig);
};
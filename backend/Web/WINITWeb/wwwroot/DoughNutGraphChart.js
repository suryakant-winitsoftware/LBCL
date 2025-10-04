let myDoughNutGraph;

window.PieGraphRender = function (chartId, jsonData, backgroundColor) {
    const dataset = JSON.parse(jsonData);

    const data = {
        labels: dataset.map(item => item.Label),  // This is for the legend
        datasets: [{
            label: 'Doughnut Chart',
            data: dataset.map(item => item.Value),
            backgroundColor: dataset.map(item => item.Color),
            hoverOffset: 4
        }]
    };

   

    const config = {
        type: 'doughnut',
        data: data,
        options: {
            responsive: true,
            plugins: {
                annotation: {
                    annotations: {
                        innerlabel: {
                            type: 'doughnutLabel',
                            display: true,
                            drawTime: 'afterDraw',
                            font: [{ size: 24 }, { size: 16, weight: 'bold' }],
                            click(context) {
                                console.log("click", context);
                            },
                            color: ['blue', 'red'],
                            content: ({ chart }) => ['Utilization'],
                        }
                    }
                },
                legend: {
                    position: 'right',
                    labels: {
                        boxWidth: 20, // Adjust size of legend box
                    },
                    title: {
                        display: true, // Show legend title
                        text: 'Other Dealers', // Title for the legend
                        color: '#333', // Set the color of the legend title
                        font: {
                            size: 16, // Set the font size for the legend title
                            weight: 'bold', // Make legend title bold
                        },
                        padding: 10, // Add padding around the legend title
                    }
                },
                tooltip: {
                    enabled: true,
                },
                datalabels: {
                    display: true,  // Disable default label positioning
                    color: '#ffffff',
                    font: {
                        size: 10,  
                        weight: 'bold'
                    },
                    formatter: (value, ctx) => {
                        const total = ctx.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                        const percentage = ((value / total) * 100).toFixed(1) + '%';
                        return percentage;     // Show percentage on each segment
                    },
                    anchor: 'center',
                    align: 'center',
                    offset: 0
                }
            },
            cutout: '70%',
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                animateScale: true,
                animateRotate: true
            }
        },
        plugins: [ChartDataLabels],
    };

    const chartElement = document.getElementById(chartId);
    chartElement.style.backgroundColor = backgroundColor;

    const ctx = chartElement.getContext('2d');

    // Destroy the chart if it already exists
    if (myDoughNutGraph) {
        myDoughNutGraph.destroy();
    }

    window.myDoughNutGraph = new Chart(ctx, config);
};

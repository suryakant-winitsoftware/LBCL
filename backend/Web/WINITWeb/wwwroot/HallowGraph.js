let myDoughNutGraphHallow;

window.PieGraphRenderHallow = function (chartId, jsonData, backgroundColor) {
    const dataset = JSON.parse(jsonData);

    const data = {
        labels: ['Progressed', 'Total'], // Labels for each part
        datasets: [{
            label: 'Doughnut Chart',
            data: [dataset.CurrentProgress, dataset.TotalTarget], // Data points for Progressed and Remaining
            backgroundColor: ['#ADD8E6 ', '#D3D3D3'], // Colors for Progressed (green) and Remaining (grey)
            hoverOffset: 4
        }]
    };

    // Chart configuration
    const config = {
        type: 'doughnut',
        data: data,
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: "Sum of Qty.", // Title for the chart
                    font: {
                        size: 18, // Set font size of the title
                        weight: 'bold' // Make title bold
                    },
                    padding: {
                        top: 20, // Padding at the top
                    },
                    color: '#333', // Title text color
                    align: 'left' // Left-align the title
                },
                legend: {
                    display: false, // Hide the legend
                },
                tooltip: {
                    enabled: true,
                    callbacks: {
                        label: (tooltipItem) => {
                            // Show the exact value in the tooltip for each section
                            return tooltipItem.raw;
                        }
                    }
                },
                //centerTextPlugin: {
                //    centerText: '',
                //    centerValue: '1632'
                //},
                datalabels: {
                    display: true,
                    color: '#ffffff',
                    font: {
                        weight: 'bold',
                        size: 14
                    },
                    anchor: 'center',
                    align: 'center',
                    offset: 0
                }
            },
            cutout: '70%', // Hollow part in the middle
            rotation: 270, // Start from the top for a half-doughnut
            circumference: 180, // Only half the circle (180 degrees)
            maintainAspectRatio: false,
            animation: {
                animateScale: true,
                animateRotate: true
            }
        },
        plugins: [ChartDataLabels]
    };

    // Get the chart element and set background color
    const chartElement = document.getElementById(chartId);
    chartElement.style.backgroundColor = "#FFFFFF";
    const ctx = chartElement.getContext('2d');

    // Destroy existing chart instance if it exists
    if (myDoughNutGraphHallow) {
        myDoughNutGraphHallow.destroy();
    }

    // Create new chart instance
    myDoughNutGraphHallow = new Chart(ctx, config);

    //Chart.register({
    //    id: 'centerTextPlugin',
    //    beforeDraw: function (chart) {
    //        var ctx = chart.ctx;
    //        var width = chart.width;
    //        var height = chart.height;
    //        ctx.textAlign = 'center';
    //        ctx.textBaseline = 'middle';
    //        var textX = width / 2;
    //        var centerText = chart.config.options.plugins.centerTextPlugin.centerText;
    //        var centerValue = chart.config.options.plugins.centerTextPlugin.centerValue;
    //        var fontSize = (height / 250).toFixed(2);
    //        ctx.font = fontSize + "em sans-serif";
    //        ctx.fillStyle = '#000';
    //        var textY = height / 2 + 20;
    //        ctx.fillText(centerText, textX, textY);
    //        var valueFontSize = (height / 220).toFixed(2);
    //        ctx.font = valueFontSize + "em sans-serif";
    //        ctx.fillStyle = '#333';
    //        var valueY = height / 2 + 45;
    //        ctx.fillText(centerValue, textX, valueY);
    //    }   
    //});
    //myDoughNutGraphHallow.update();
};

let myDoughnutChart;

window.DoughnutChart = function (chartId, jsonData, centerText, centerValue) {
    var parsedData = JSON.parse(jsonData);
    var canvas = document.getElementById(chartId);
    if (!canvas) {
        console.error(`Canvas element with id '${chartId}' not found.`);
        return;
    }
    var ctx = canvas.getContext('2d');

    // Register the custom plugin
    Chart.register({
        id: 'centerTextPlugin',
        beforeDraw: function (chart) {
            var ctx = chart.ctx;
            var width = chart.width;
            var height = chart.height;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            var textX = width / 2;
            var centerText = chart.config.options.plugins.centerTextPlugin.centerText;
            var centerValue = chart.config.options.plugins.centerTextPlugin.centerValue;
            var fontSize = (height / 250).toFixed(2);
            ctx.font = fontSize + "em sans-serif";
            ctx.fillStyle = '#000';
            var textY = height / 2 + 20;
            ctx.fillText(centerText, textX, textY);
            var valueFontSize = (height / 220).toFixed(2);
            ctx.font = valueFontSize + "em sans-serif";
            ctx.fillStyle = '#333';
            var valueY = height / 2 + 45;
            ctx.fillText(centerValue, textX, valueY);
        }
    });

    // Chart configuration
    const config = {
        type: 'doughnut',
        data: {
            labels: parsedData.map(item => item.Label),
            datasets: [{
                data: parsedData.map(item => item.Value),
                backgroundColor: parsedData.map(item => item.Color)
            }]
        },
        options: {
            plugins: {
                legend: {
                    display: true,
                    onClick: null,
                    onHover: null
                },
                tooltip: {
                    enabled: true
                },
                centerTextPlugin: {
                    centerText: centerText,
                    centerValue: centerValue
                }
            },
            cutout: '70%',
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                animateScale: true,
                animateRotate: true
            },
            interaction: {
                mode: 'nearest',
                intersect: false
            }
        }
    };

    // Destroy existing chart if it exists
    if (myDoughnutChart) {
        myDoughnutChart.destroy();
    }

    // Create the new chart instance
    myDoughnutChart = new Chart(ctx, config);
};

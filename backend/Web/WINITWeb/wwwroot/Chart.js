Chart.plugins.register({
    afterDraw: function (chart) {
        if (chart.config.options.elements.center) {
            var ctx = chart.chart.ctx;
            var centerConfig = chart.config.options.elements.center;
            var fontStyle = centerConfig.fontStyle || 'Arial';
            var txt = centerConfig.text;
            var color = centerConfig.color || '#000';
            var sidePadding = centerConfig.sidePadding || 20;
            var sidePaddingCalculated = (sidePadding / 100) * (chart.innerRadius * 2);
            ctx.font = "30px " + fontStyle;
            var stringWidth = ctx.measureText(txt).width;
            var elementWidth = (chart.innerRadius * 2) - sidePaddingCalculated;
            var widthRatio = elementWidth / stringWidth;
            var newFontSize = Math.floor(30 * widthRatio);
            var elementHeight = (chart.innerRadius * 2);
            var fontSizeToUse = Math.min(newFontSize, elementHeight);
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            var centerX = ((chart.chartArea.left + chart.chartArea.right) / 2);
            var centerY = ((chart.chartArea.top + chart.chartArea.bottom) / 2);
            ctx.font = fontSizeToUse + "px " + fontStyle;
            ctx.fillStyle = color;
            ctx.fillText(txt, centerX, centerY);
        }
    }
});

/*Chart.register(ChartjsFunnel);*/
// Doughnut Chart Configuration and Initialization
window.DoughnutChart = function (chartId, jsonData, centerText) {
    var parsedData = JSON.parse(jsonData);
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
                    display: true
                }
            },
            elements: {
                center: {   // Center text configuration
                    text: centerText,
                    color: '#FF6384',  // Customize the color of the center text
                    fontStyle: 'Helvetica',  // Customize the font style
                    sidePadding: 15  // Adjust padding as needed
                }
            },
            layout: {
                padding: 20
            },
        }
    };
    new Chart(
        document.getElementById(chartId).getContext('2d'),
        config
    );
};

//window.FunnelChart = function (chartId, jsonData) {
//    var parsedData = JSON.parse(jsonData);
//    const funnelConfig = {
//        type: 'funnel',  // Funnel chart type
//        data: {
//            labels: parsedData.map(item => item.Label),
//            datasets: [{
//                data: parsedData.map(item => item.Value),
//                backgroundColor: parsedData.map(item => item.Color)
//            }]
//        },
//        options: {
//            plugins: {
//                legend: {
//                    display: false
//                },
//                tooltip: {
//                    enabled: true
//                },
//                title: {
//                    display: true,
//                    text: 'Customer Conversion Funnel',
//                    color: 'black',
//                    font: {
//                        weight: 'normal',
//                        size: 20
//                    }
//                }
//            },
//            responsive: true,
//            maintainAspectRatio: false,
//            layout: {
//                padding: 10
//            }
//        }
//    };

//    new Chart(
//        document.getElementById(chartId).getContext('2d'),
//        funnelConfig
//    );
//};

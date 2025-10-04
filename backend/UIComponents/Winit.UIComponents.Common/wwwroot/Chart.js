//window.PercentageChart = function (chartId, percentage, percentageColor, remainingColor, backgroundColor) {
//    const data = {
//        labels: ['Battery', 'Remaining'],
//        datasets: [{
//            data: [percentage, 100 - percentage],
//            backgroundColor: [percentageColor, remainingColor] // Use individual colors for percentage and remaining portion
//        }]
//    };

//    const percentageLabel = {
//        id: 'percentageLabel',
//        beforeDatasetsDraw(chart, args, pluginOptions) {
//            const { ctx, data } = chart;
//            ctx.save();
//            const xCoor = chart.width / 2;
//            const yCoor = chart.height / 2;

//            ctx.font = 'bold 40px sans-serif'; // Adjusted font size for smaller display
//            ctx.fillStyle = 'white'; // Set the percentage color to white
//            ctx.textAlign = 'center';
//            ctx.textBaseline = 'middle';
//            ctx.fillText(percentage + '%', xCoor, yCoor);
//        }
//    };

//    const percentageConfig = {
//        type: 'doughnut',
//        data: data,
//        options: {
//            plugins: {
//                legend: {
//                    display: false
//                },
//                tooltip: {
//                    enabled: false // Disable tooltips
//                }
//            },
//            layout: {
//                padding: 10 // Adjusted padding for smaller display
//            },
//            cutout: '80%' // Adjusted cutout for smaller display
//        },
//        plugins: [percentageLabel]
//    };

//    // Set background color
//    document.getElementById(chartId).style.backgroundColor = backgroundColor;
//    // Retrieve the existing chart instance from sessionStorage
//    let myPercentageChart = sessionStorage.getItem(chartId);
//    // Create or update the chart instance
//    if (window.myPercentageChart) {
//        window.myPercentageChart.data.datasets[0].data = [percentage, 100 - percentage];
//        window.myPercentageChart.data.datasets[0].backgroundColor = [percentageColor, remainingColor];
//        window.myPercentageChart.update();
//    } else {
//        window.myPercentageChart = new Chart(
//            document.getElementById(chartId).getContext('2d'),
//            percentageConfig
//        );
//    }

//    // Save the chart instance to sessionStorage
//    sessionStorage.setItem(chartId, myPercentageChart);
//};

//window.PercentageChart = function (chartId, percentage, percentageColor, remainingColor, backgroundColor) {
//    const data = {
//        labels: ['Battery', 'Remaining'],
//        datasets: [{
//            data: [percentage, 100 - percentage],
//            backgroundColor: [percentageColor, remainingColor] // Use individual colors for percentage and remaining portion
//        }]
//    };

//    const percentageLabel = {
//        id: 'percentageLabel',
//        beforeDatasetsDraw(chart, args, pluginOptions) {
//            const { ctx, chartArea: { width, height } } = chart;
//            ctx.save();
//            const xCoor = width / 2;
//            const yCoor = height / 2;

//            ctx.font = 'bold 40px sans-serif'; // Adjusted font size for smaller display
//            ctx.fillStyle = 'white'; // Set the percentage color to white
//            ctx.textAlign = 'center';
//            ctx.textBaseline = 'middle';
//            ctx.fillText(percentage + '%', xCoor, yCoor);
//        }
//    };

//    const percentageConfig = {
//        type: 'doughnut',
//        data: data,
//        options: {
//            plugins: {
//                legend: {
//                    display: false
//                },
//                tooltip: {
//                    enabled: false // Disable tooltips
//                }
//            },
//            layout: {
//                padding: 10 // Adjusted padding for smaller display
//            },
//            cutout: '80%' // Adjusted cutout for smaller display
//        },
//        plugins: [percentageLabel]
//    };

//    // Set background color
//    document.getElementById(chartId).style.backgroundColor = backgroundColor;

//    // Retrieve the chart context
//    const ctx = document.getElementById(chartId).getContext('2d');

//    // Check if the chart instance already exists and destroy it if so
//    if (window.myPercentageChart) {
//        window.myPercentageChart.destroy();
//    }

//    // Create a new chart instance
//    window.myPercentageChart = new Chart(ctx, percentageConfig);
//};

window.PercentageChart = function (chartId, percentage, percentageFontSizeInPx, percentageColor, percentageTextColor, remainingColor, backgroundColor) {
    const data = {
        labels: ['Battery', 'Remaining'],
        datasets: [{
            data: [percentage, 100 - percentage],
            backgroundColor: [percentageColor, remainingColor] // Use individual colors for percentage and remaining portion
        }]
    };

    const percentageLabel = {
        id: 'percentageLabel',
        beforeDatasetsDraw(chart, args, pluginOptions) {
            const { ctx, chartArea: { top, bottom, left, right } } = chart;
            ctx.save();
            const xCoor = (left + right) / 2;
            const yCoor = (top + bottom) / 2;

            ctx.font = 'bold '+percentageFontSizeInPx+'px sans-serif'; // Adjusted font size for smaller display
            ctx.fillStyle = percentageTextColor; // Set the percentage color to white
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(percentage + '%', xCoor, yCoor);
            ctx.restore();
        }
    };

    const percentageConfig = {
        type: 'doughnut',
        data: data,
        options: {
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: false // Disable tooltips
                }
            },
            layout: {
                padding: 10 // Adjusted padding for smaller display
            },
            cutout: '80%' // Adjusted cutout for smaller display
        },
        plugins: [percentageLabel]
    };

    // Set background color
    const chartElement = document.getElementById(chartId);
    chartElement.style.backgroundColor = backgroundColor;

    // Retrieve the chart context
    const ctx = chartElement.getContext('2d');

    // Check if the chart instance already exists and destroy it if so
    if (window.myPercentageChart) {
        window.myPercentageChart.destroy();
    }

    // Create a new chart instance
    window.myPercentageChart = new Chart(ctx, percentageConfig);
};


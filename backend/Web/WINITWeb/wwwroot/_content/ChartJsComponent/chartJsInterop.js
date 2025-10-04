export function createChart(canvasId, chartType, data, options) {
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext('2d');
    
    return new Chart(ctx, {
        type: chartType,
        data: data,
        options: options
    });
}

export function updateChart(chart, data, options) {
    chart.data = data;
    chart.options = options;
    chart.update();
}

export function destroyChart(chart) {
    chart.destroy();
} 
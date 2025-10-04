window.echartsInterop = {
    init: function (element) {
        return echarts.init(element);
    },
    setOption: function (chart, options) {
        chart.setOption(options);
    },
    resize: function (chart) {
        chart.resize();
    },
    dispose: function (chart) {
        chart.dispose();
    }
}; 
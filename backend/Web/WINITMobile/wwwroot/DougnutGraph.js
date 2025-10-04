window.DoughnutChart = function (chartId, data) {
    var data = JSON.parse(data);

    const doughnutLabelsLine = {
        id: 'doughnutLabelsLine',
        afterDraw(chart, args, options) {
            const { ctx, chartArea: { left, top, right, bottom, height, width } } = chart;
            chart.data.datasets.forEach((dataset, i) => {
                //console.log(chart)
                chart.getDatasetMeta(i).data.forEach((datapoint, index) => {
                    console.log(dataset)
                    const { x, y } = datapoint.tooltipPosition();
                    // ctx.fillStyle = dataset.borderColor [index]; // ctx.fill();
                    //ctx.fillRect(x, y, 10, 10);
                    //console.log(x)
                    // draw line
                    const halfwidth = width / 2;
                    const halfheight = height / 2;
                    const xLine = x >= halfwidth ? x + 15 : x - 15;//upper line
                    const yLine = y >= halfheight ? y + 10 : y - 10; //turning
                    const extraLine = x >= halfwidth ? 30 : -30; //inside line
                    ctx.beginPath();
                    ctx.moveTo(x, y);
                    ctx.lineTo(xLine, yLine);
                    ctx.lineTo(xLine + extraLine, yLine);
                    //ctx.strokeStyle = dataset.backgroundColor[index];
                    ctx.stroke();
                    const textWidth = ctx.measureText(chart.data.labels[index]).width;
                    //console.log(textWidth)
                    ctx.font = '8px Arial';
                    // control the position
                    const textXPosition = x >= halfwidth ? 'left' : 'right';
                    ctx.textAlign = textXPosition;
                    ctx.textBaseline = 'middle';
                    ctx.fillStyle = dataset.backgroundColor[index];
                    ctx.fillText(chart.data.labels[index], xLine + extraLine, yLine);
                })
            })
        }
    }


    const doughnutLabel = {
        id: 'doughnutLabel',
        beforeDatasetsDraw(chart, args, pluginOptions) {
            const { ctx, data } = chart;
            ctx.save();
            const xCoor = chart.getDatasetMeta(0).data[0].x;
            const yCoor = chart.getDatasetMeta(0).data[0].y;
            const dataTotal = data.datasets[0].data.reduce((a, b) => a + b, 0);
            ctx.font = 'bold 30px sans-serif';
            ctx.fillStyle = 'rgba(54, 162, 235, 1)';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText('Target', xCoor, yCoor - 10);
            ctx.fillText(dataTotal, xCoor, yCoor + 20);
        }
    }
    const config = {
        type: 'doughnut',
        data: {
            labels: data.map(item => item.Label),
            datasets: [{
                //data: data.values,
                data: data.map(item => item.Value),
                backgroundColor: [
                    '#379118', '#FFAA00'
                ],

            }]
        },
        options: {
            plugins: {
                legend: {
                    display: true
                }
            },
            layout:
            {
                padding: 20
            },

        },
        plugins: [doughnutLabelsLine, doughnutLabel]
    };

    const myChart = new Chart(
        document.getElementById(chartId).getContext('2d'),
        config
    );


};
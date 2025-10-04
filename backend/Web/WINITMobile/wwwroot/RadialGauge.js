window.renderDoughnutChart = function (chartId, data, NeedleValue) {
    var data = JSON.parse(data);

    const gaugeNeedle = {
        id: 'gaugeNeedle',
        afterDatasetDraw(chart, args, options) {
            const { ctx, config, data, chartArea: { top, bottom, left, right, width, height } } = chart;
            ctx.save();
            const xCenter = chart.getDatasetMeta(0).data[0].x;
            const yCenter = chart.getDatasetMeta(0).data[0].y;
            const outerRadius = chart.getDatasetMeta(0).data[0].outerRadius;
            const innerRadius = chart.getDatasetMeta(0).data[0].innerRadius;
            const needleValue = data.datasets[0].needleValue;
            const dataTotal = data.datasets[0].data.reduce((a, b) => a + b, 0);
            const angle = Math.PI + (1 / dataTotal * needleValue * Math.PI);
            const radius = 10;
            const circumference = ((chart.getDatasetMeta(0).data[0]
                .circumference / Math.PI) / data.datasets[0].data[0]) * needleValue;
            //needle
            ctx.translate(xCenter, yCenter);
            ctx.rotate(Math.PI * (circumference + 1.5));
            ctx.beginPath();
            ctx.fillStyle = '#1F528E';
            ctx.moveTo(0 - radius, 0);
            ctx.lineTo(0, 0 - outerRadius);
            ctx.lineTo(0 + radius, 0);
            ctx.stroke();
            ctx.fill();


            //dot
            ctx.beginPath();
            //ctx.arc(x,y,angleStart, angleEnd, false);  //counterclockwise false
            ctx.arc(0, 0, radius, 0, angle * 360, false);
            ctx.fill();
            ctx.restore();
        }
    }

    //GaugeLabels
    const gaugeLabels = {
        id: 'gaugeLabels',
        afterDatasetsDraw(chart, args, plugins) {
            const { ctx, chartArea: { left, right } } = chart;
            const xCenter = chart.getDatasetMeta(0).data[0].x;
            const yCenter = chart.getDatasetMeta(0).data[0].y;
            const outerRadius = chart.getDatasetMeta(0).data[0].outerRadius;
            const innerRadius = chart.getDatasetMeta(0).data[0].innerRadius;
            const widthSlice = (outerRadius - innerRadius) / 2;
            ctx.translate(xCenter, yCenter);
            ctx.font = 'bold 15px sans-serif';
            ctx.fillStyle = 'black';
            ctx.textAlign = 'center';
            ctx.fillText('0', 0 - innerRadius - widthSlice, 0 +
                20);
            ctx.fillText('150', 0 + innerRadius + widthSlice, 0
                + 20);
            ctx.restore();
        }
    }

    // gaugeFlowMeter plugin block 
    const gaugeFlowMeter = {
        id: 'gaugeFlowMeter',
        afterDatasetsDraw(chart, args, plugins) {
            const { ctx, data } = chart;
            ctx.save();
            const needleValue = data.datasets[0].needleValue;
            const xCenter = chart.getDatasetMeta(0).data[0].x;
            const yCenter = chart.getDatasetMeta(0).data[0].y;
            // flowMeter
            const circumference = ((chart.getDatasetMeta(0).data[0]
                .circumference / Math.PI) / data.datasets[0].data[0]) * needleValue;

            const circumferencepercentage = circumference * 100;
            ctx.font = 'bold 30px sans-serif';
            ctx.fillStyle = 'grey';
            ctx.textAlign = 'center';
            ctx.fillText(circumferencepercentage.toFixed(1) + "%", xCenter, yCenter + 30);
        }
    }

    const config = {
        type: 'doughnut',
        data: {
            //labels: data.labels,
            datasets: [{
                // data: data.values,
                data: data.map(item => item.Value),
                backgroundColor: [
                    '#FFAA00', '#FFAA00', '#DB7464', '#DB7464', '#DB7464'
                ],
                needleValue: NeedleValue,
                circumference: 180,
                rotation: 270,
            }]
        },
        options: {
            plugins: {
                //tooltip:{
                //    enabled: false
                //}
            },


        },
        plugins: [gaugeNeedle, gaugeFlowMeter, gaugeLabels]
    };

    const myChart = new Chart(
        document.getElementById(chartId).getContext('2d'),
        config
    );

};

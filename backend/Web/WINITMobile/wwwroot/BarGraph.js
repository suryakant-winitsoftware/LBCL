function createChart(jsonData) {
    var data = JSON.parse(jsonData);

    var ctx = document.getElementById('myChart').getContext('2d');

    var chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(item => item.Label),
            datasets: [{
                label: 'Target',
                data: data.map(item => item.Target),
                backgroundColor: 'rgba(255, 99, 132, 0.2)'
            }, {
                label: 'Ach',
                data: data.map(item => item.Ach),
                backgroundColor: 'rgba(54, 162, 235, 0.2)'
            }]
        },
        options: {
            indexAxis: 'y', // Change indexAxis to 'y'
            scales: {
                x: {
                    beginAtZero: true
                },
                y: {
                    beginAtZero: false,
                    min: -40, // Set the minimum value of the y-axis
                    max: 80, // Set the maximum value of the y-axis
                    stepSize: 20 // Set the interval between ticks
                }
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom', // Place legend at the bottom
                    labels: {
                        //usePointStyle: true, // Use point style for legend labels
                        boxWidth: 10 // Set width of legend color boxes
                    }
                }
            }
        }
    });
}

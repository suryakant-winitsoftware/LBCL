let map;
let markers = [];

function initialize(serializedCustomers) {
    const customers = JSON.parse(serializedCustomers);
    const firstlat = parseFloat(customers[0].Latitude);
    const firstlng = parseFloat(customers[0].Longitude);
    map = new google.maps.Map(document.getElementById("map"), {
        center: { lat: firstlat, lng: firstlng },
        zoom: 14,
    });

    addCustomerMarkers(customers);
}

function addCustomerMarkers(customers) {
    customers.forEach(customer => {
        const lat = parseFloat(customer.Latitude);
        const lng = parseFloat(customer.Longitude);

        if (!isNaN(lat) && !isNaN(lng)) {
            const latlng = { lat: lat, lng: lng };
            const geocoder = new google.maps.Geocoder();
            geocoder.geocode({ location: latlng }, function (results, status) {
                if (status === 'OK') {
                    if (results[0]) {
                        const address = results[0].formatted_address;

                        
                        const marker = new google.maps.Marker({
                            position: latlng,
                            map: map,
                            title: customer.Name,
                            icon: {
                                url: '/Images/pngwing.com.png',
                                scaledSize: new google.maps.Size(40, 40)
                            }
                        });

                        
                        const infowindow = new google.maps.InfoWindow({
                            content: generateInfoWindowContent(customer, address)
                        });

                        marker.addListener('click', function () {
                            infowindow.open(map, marker);
                        });
                    } else {
                        console.error('No results found for geocoding');
                    }
                } else {
                    console.error('Geocoder failed due to: ' + status);
                }
            });
        } else {
            console.error('Invalid latitude or longitude:', customer.Latitude, customer.Longitude);
        }
    });
}

function generateInfoWindowContent(customer, address) {
    return `
        <div>
            <p>Customer Code: ${customer.Code}</p>
            <p>Customer Name: ${customer.Name}</p>
            <p>Address: ${customer.Description}</p>
            <p>Google Address: ${address}</p>
            <button id="directionsBtn" onclick="showDirections('${customer.Latitude}', '${customer.Longitude}')">Directions</button>

        </div>
    `;
}
function showDirections(latitude, longitude) {
    const url = `https://www.google.com/maps/dir/?api=1&destination=${latitude},${longitude}`;
    window.location.href = url;
}



let customerMap;
let customerMarker;
let currentLocationMarker;
let originalPosition;

function initializeCustomer(serializedCustomers) {
    const customer = JSON.parse(serializedCustomers);
    const lat = parseFloat(customer.Latitude);
    const lng = parseFloat(customer.Longitude);

    if (!isNaN(lat) && !isNaN(lng)) {
        customerMap = new google.maps.Map(document.getElementById("customerMap"), {
            center: { lat: lat, lng: lng },
            zoom: 13,
        });

        originalPosition = { lat: lat, lng: lng };
        customerMarker = new google.maps.Marker({
            position: originalPosition,
            map: customerMap,
            title: customer.Name,
            icon: {
                url: '/Images/pngwing.com.png',
                scaledSize: new google.maps.Size(40, 40)
            }
        });

        const geocoder = new google.maps.Geocoder();
        geocoder.geocode({ location: originalPosition }, function (results, status) {
            if (status === 'OK') {
                if (results[0]) {
                    const address = results[0].formatted_address;
                    const infowindow = new google.maps.InfoWindow({
                        content: generateInfoWindowContent(customer, address)
                    });

                    customerMarker.addListener('click', function () {
                        infowindow.open(customerMap, customerMarker);
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
}

function initializeCurrentLocation(serializedCurrentLocation, dotNetHelper) {
    const location = JSON.parse(serializedCurrentLocation);
    const lat = parseFloat(location.Latitude);
    const lng = parseFloat(location.Longitude);

    if (!isNaN(lat) && !isNaN(lng)) {
        currentLocationMarker = new google.maps.Marker({
            position: { lat: lat, lng: lng },
            map: customerMap,
            draggable: true,
            title: "Current Location"
        });

        google.maps.event.addListener(currentLocationMarker, 'dragend', function (event) {
            const newPosition = { lat: event.latLng.lat(), lng: event.latLng.lng() };
            confirmLocationChange(newPosition, dotNetHelper);
        });
    } else {
        console.error('Invalid latitude or longitude for current location:', location.Latitude, location.Longitude);
    }
}

function generateInfoWindowContent(customer, address) {
    return `
        <div>
            <p>Customer Code: ${customer.Code}</p>
            <p>Customer Name: ${customer.Name}</p>
            <p>Address: ${customer.Description}</p>
            <p>Google Address: ${address}</p>
            <button onclick="openDirections('${customer.Latitude}', '${customer.Longitude}')">Directions</button>
        </div>
    `;
}

function openDirections(latitude, longitude) {
    const url = `https://www.google.com/maps/dir/?api=1&destination=${latitude},${longitude}`;
    window.location.href = url;
}

function confirmLocationChange(newPosition, dotNetHelper) {
    if (confirm("Do you want to change your location?")) {
        currentLocationMarker.setPosition(newPosition);
        // Call .NET method to update latitude and longitude
        dotNetHelper.invokeMethodAsync('UpdateNewLocation', newPosition.lat.toString(), newPosition.lng.toString())
            .catch(err => console.error('Error calling UpdateNewLocation:', err));
    } else {
        currentLocationMarker.setPosition(originalPosition);
    }
}

function calculateRoadDistance(lat1, lon1, lat2, lon2, dotNetHelper) {
    return new Promise((resolve, reject) => {
        const service = new google.maps.DistanceMatrixService();
        const origin = new google.maps.LatLng(lat1, lon1);
        const destination = new google.maps.LatLng(lat2, lon2);

        service.getDistanceMatrix(
            {
                origins: [origin],
                destinations: [destination],
                travelMode: google.maps.TravelMode.DRIVING,
            },
            (response, status) => {
                if (status === google.maps.DistanceMatrixStatus.OK) {
                    const distance = response.rows[0].elements[0].distance.value; // distance in meters
                    dotNetHelper.invokeMethodAsync('GetRoadDistance', distance.toString())
                        .catch(err => console.error('Error calling GetRoadDistance:', err));
                    resolve(distance);
                } else {
                    console.error('Distance Matrix request failed due to ' + status);
                    reject('Distance Matrix request failed');
                }
            }
        );
    });
}
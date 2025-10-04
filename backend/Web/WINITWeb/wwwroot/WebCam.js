let stream;
let latitude = 0;
let longitude = 0;
var Address = "";
let base64Image = "";


// Function to get geolocation with a Promise
async function getAddressFromLatLon(latitude, longitude) {
    const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${latitude}&lon=${longitude}`;

    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error("Network response was not ok");
        }
        const data = await response.json();
        return data.display_name;
    } catch (error) {
        console.error("There was a problem with the fetch operation:", error);
        return "Unable to fetch address";
    }
}

async function getGeolocation() {
    return new Promise((resolve, reject) => {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(position => {
                latitude = position.coords.latitude;
                longitude = position.coords.longitude;
                console.log(`Latitude: ${latitude}, Longitude: ${longitude}`);
                getAddressFromLatLon(latitude, longitude)
                    .then(address => {
                        console.log("Fetched address:", address);
                        Address = address;
                        // You can use the address here (e.g., display it)
                        resolve(); // Resolve the promise with address (optional)
                    })
                    .catch(error => {
                        console.error("Error fetching address:", error);
                        // Handle address fetching error (optional)
                    });
                // Resolve the promise once geolocation data is retrieved
            }, error => {
                showError(error);
                reject(error); // Reject the promise if there's an error
            });
        } else {
            console.error('Geolocation is not supported by this browser.');
            reject(new Error('Geolocation is not supported.'));
        }
    });
}

async function startCamera() {
    const video = document.getElementById('video');
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        console.error('MediaDevices API is not supported in this browser.');
        return;
    }

    // Stop any existing stream before starting a new one
    if (stream) {
        console.log('Stopping existing stream');
        stream.getTracks().forEach(track => track.stop());
    }

    navigator.mediaDevices.getUserMedia({ video: true })
        .then(newStream => {
            stream = newStream;
            video.srcObject = stream;
            video.onloadedmetadata = () => {
                console.log('Video metadata loaded');
                video.play();
            };
        })
        .catch(err => {
            console.error("Error accessing the camera: ", err);
        });

    // Start the geolocation request and capture image once it's done
    getGeolocation().then(() => {
        // Geolocation data is available, you can now capture the image

    }).catch(err => {
        console.error("Error getting geolocation: ", err);
    });
}

async function showError(error) {
    switch (error.code) {
        case error.PERMISSION_DENIED:
            console.error("User denied the request for Geolocation.");
            break;
        case error.POSITION_UNAVAILABLE:
            console.error("Location information is unavailable.");
            break;
        case error.TIMEOUT:
            console.error("The request to get user location timed out.");
            break;
        case error.UNKNOWN_ERROR:
            console.error("An unknown error occurred.");
            break;
    }
}

async function stopCamera() {
    if (stream) {
        console.log('Stopping camera stream');
        stream.getTracks().forEach(track => track.stop());
        stream = null;
    }
}

async function captureImage() {
    const canvas = document.getElementById('canvas');
    const video = document.getElementById('video');
    const context = canvas.getContext('2d');

    // Ensure the canvas dimensions match the video dimensions
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    // Freeze the video frame for capturing a clear image
    video.pause(); // Pause the video
    context.drawImage(video, 0, 0, canvas.width, canvas.height);
    video.play(); // Resume the video after capturing

    // Get the current date and time
    const currentDate = new Date();
    const date = currentDate.toLocaleDateString();
    const time = currentDate.toLocaleTimeString();

    // Create the overlay text for each line
    const dateText = `${time}, ${date}`;
    const latLonText = `Lat: ${latitude}, Lon: ${longitude}`;
    let addressParts = Address.split(',').map(part => part.trim());

    // Dynamically determine the two-line format
    let firstLine = ""; 
    let secondLine = "";
    let thirdLine = "";

    if (addressParts.length >= 4) {
        // Combine the first two parts and the last two parts for better readability
        firstLine = `${addressParts[0]}, ${addressParts[1]}`;
        secondLine = `${addressParts[2]}, ${addressParts[3]}`;
        thirdLine = `${addressParts.slice(4).join(', ')}`;
    } else if (addressParts.length === 3) {
        // First part on the first line, and second + third part on the second line
        firstLine = `${addressParts[0]}, ${addressParts[1]}`;
        secondLine = `${addressParts[2]}`;
    } else if (addressParts.length === 2) {
        // If only two parts, each part on its own line
        firstLine = addressParts[0];
        secondLine = addressParts[1];
    } else {
        // If only one part, or if something unexpected occurs
        firstLine = Address;
    }

    // Set the font style
    context.font = '20px Arial';
    context.fillStyle = 'rgba(255, 255, 255, 0.9)';
    context.textAlign = 'left';
    context.textBaseline = 'top';
    

    //shadow
    context.shadowColor = 'rgba(0, 0, 0, 0.7)'; // Dark shadow
    context.shadowOffsetX = 2;
    context.shadowOffsetY = 2;
    context.shadowBlur = 4;

    // Calculate positions to avoid overlapping with the button and move the text higher
    const padding = 10;
    const lineHeight = 20; // Line height for each line of text
    const startX = padding; // X position (left)
    const offsetFromBottom = 0; // Increase this value to move text higher
    const startY = canvas.height - lineHeight * 5 - padding - offsetFromBottom; // Adjusted Y position

    // Draw each line of text separately
    context.fillText(dateText, startX, startY);
    context.fillText(latLonText, startX, startY + lineHeight);
    context.fillText(firstLine, startX, startY + lineHeight * 2);
    context.fillText(secondLine, startX, startY + lineHeight * 3);
    context.fillText(thirdLine, startX, startY + lineHeight * 4);

    //get captured image
    base64Image = canvas.toDataURL('image/png');

    await stopCamera();
}

async function recaptureImage() {
    console.log("Re-capturing image");
    await stopCamera();
    // Ensure a short delay to prevent race conditions
    return new Promise(resolve => setTimeout(resolve, 100))
        .then(() => {
            const video = document.getElementById('video');
            const canvas = document.getElementById('canvas');
            const context = canvas.getContext('2d'); 

            video.srcObject = null;
            video.removeAttribute('src');
            video.load();
            context.clearRect(0, 0, canvas.width, canvas.height);

            // Add event listener to handle video playing after reset
            video.addEventListener('playing', () => {
                console.log('Video started playing after reset');
            });
        })
        .then(() => startCamera()); // Start the camera again
}
window.getCapturedImage = async function () {// Ensure captureImage() returns the base64 string
    return base64Image;
};
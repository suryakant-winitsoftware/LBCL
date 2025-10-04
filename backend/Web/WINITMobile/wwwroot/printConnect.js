
//For Overriding to present app
//function startAppAService() {
//    const intentUri = "intent://#Intent;package=com.Winit.Print;component=com.Winit.Print/.SplashScreen;end";
//    window.location.href = intentUri;
//}

// For opening the app like it initialise
function startAppAService() {
    const intentUri = "intent://#Intent;package=com.Winit.Print;component=com.Winit.Print/.SplashScreen;launchFlags=0x10008000;end";
    window.location.href = intentUri;
}

//function startAppAService() {
//    const intentUri = "intent://#Intent;package=com.Winit.Print;component=com.Winit.Print/.SplashScreen;launchFlags=0x10000000;end";
//    window.location.href = intentUri;
//}
//function startAppAService() {
//    // Assuming 'webView' is your WebView reference
//    webView.invokeMethodAsync('startBackgroundService');
//}


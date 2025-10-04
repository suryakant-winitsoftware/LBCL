using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WinIt.Pages.Collection.BalanceConfirmation
{
    public partial class WebCam : ComponentBase
    {
        private bool DisableCam = false;
        private bool IsReCapture = false;
        private bool IsCapture = false;
        private string Camera = "Capture Selfie";
        private string ImageCaptured = "";
        [Parameter]
        public EventCallback<string> CapturedImage { get; set; }

        private async Task StartCamera()
        {
            if (Camera == "Re-Capture")
            {
                await ReCaptureImage();
            }
            else
            {
                Console.WriteLine("Starting camera");
                await jsRuntime.InvokeVoidAsync("startCamera");
                DisableCam = true;
                IsCapture = true;
                StateHasChanged(); // Refresh the UI to hide/show elements
            }

        }

        private async Task ReCaptureImage()
        {
            Console.WriteLine("Re-Capturing image");// Small delay to ensure the video element resets
            await jsRuntime.InvokeVoidAsync("recaptureImage");
            await Task.Delay(100);
            IsReCapture = false;
            IsCapture = true;
            StateHasChanged();
        }

        private async Task CaptureImage()
        {
            Console.WriteLine("Capturing image");
            await jsRuntime.InvokeVoidAsync("captureImage");
            await Task.Delay(100);
            await jsRuntime.InvokeVoidAsync("stopCamera");
            await Task.Delay(100);
            await jsRuntime.InvokeVoidAsync("stopCamera");
            IsCapture = false;
            IsReCapture = true;
            Camera = "Re-Capture";
            ImageCaptured = await SaveImage();
            await CapturedImage.InvokeAsync(ImageCaptured);
            StateHasChanged();
        }

        private async Task<string> SaveImage()
        {
            var base64Image = await jsRuntime.InvokeAsync<string>("getCapturedImage");
            // You can now process the base64Image as needed, such as uploading it to the server.
            Console.WriteLine("Captured Image Base64: " + base64Image);
            return base64Image;
        }
    }
}

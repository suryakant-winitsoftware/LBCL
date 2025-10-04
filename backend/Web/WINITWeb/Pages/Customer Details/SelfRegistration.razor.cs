//using Microsoft.AspNetCore.Components;

//namespace WinIt.Pages.Customer_Details
//{
//    public partial class SelfRegistration
//    {
//        private string generatedOtp = string.Empty;
//        private string message = string.Empty;
//        private bool otpSent = false;

//        // Method to request an OTP
//        private async Task RequestOtp()
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(_viewmodel.selfRegistration.MobileNo))
//                {
//                    message = "Please enter a valid mobile number.";
//                    return;
//                }

//                // Generate OTP
//                generatedOtp = GenerateOtp();
//                _viewmodel.selfRegistration.OTP = generatedOtp;
//                // Save OTP to database and send it to the user
//                bool otpSaved = await _viewmodel.HandleSelfRegistration();

//                if (otpSaved)
//                {
//                    otpSent = true;
//                    message = "OTP has been sent to your mobile number.";
//                }
//                else
//                {
//                    message = "Failed to send OTP. Please try again.";
//                }
//            }
//            catch (Exception ex)
//            {
//                message = $"An error occurred: {ex.Message}";
//            }
//        }

//        private string GenerateOtp()
//        {
//            var random = new Random();
//            return random.Next(100000, 999999).ToString();
//        }
//        private async Task VerifyOtp()
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(_viewmodel.selfRegistration.UserEnteredOtp))
//                {
//                    message = "Please enter the OTP sent to your mobile number.";
//                    return;
//                }

//                // Verify OTP from database
//                bool otpVerified = await _viewmodel.VerifyOTP();

//                if (otpVerified)
//                {
//                    // make this flag true = IsVerified
//                    _navigationManager.NavigateTo($"/next-page/{_viewmodel.selfRegistration.UID}");
//                }
//                else
//                {
//                    message = "Invalid OTP. Please try again.";
//                }
//            }
//            catch (Exception ex)
//            {
//                message = $"An error occurred: {ex.Message}";
//            }
//        }
//    }
//}

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Winit.UIComponents.Common.CustomControles
{
    public partial class Timer : ComponentBase
    {
        private int timerValue = 10;
        private bool timerExpired = false;
        private System.Threading.Timer countdownTimer;

        protected override void OnInitialized()
        {
            countdownTimer = new System.Threading.Timer(UpdateTimer, null, 1000, 1000);
        }

        private void UpdateTimer(object state)
        {
            if (timerValue > 0)
            {
                timerValue--;
            }
            else
            {
                timerExpired = true;
                countdownTimer.Dispose(); // Stop the timer when it reaches zero
            }

            InvokeAsync(StateHasChanged); // Update the UI
        }
    }
}

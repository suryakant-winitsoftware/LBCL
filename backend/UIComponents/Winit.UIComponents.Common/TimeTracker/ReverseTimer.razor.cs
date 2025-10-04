using Microsoft.AspNetCore.Components;

namespace Winit.UIComponents.Common.TimeTracker
{
    public partial class ReverseTimer : ComponentBase
    {
        private TimeSpan _timeLeft;
        private Timer _timer;
        [Parameter] public int CountdownMinutes { get; set; } // Total countdown time in minutes
        [Parameter]
        public EventCallback<bool> Timerstopped { get; set; }

        protected override void OnInitialized()
        {
            // Initialize the timer with 5 minutes
            StartTimer();
        }

        public void StartTimer()
        {
            _timeLeft = TimeSpan.FromMinutes(CountdownMinutes);

            // Set up the timer to tick every second
            _timer = new Timer(UpdateTimer, null, 0, 1000);
        }

        private void UpdateTimer(object state)
        {
            if (_timeLeft.TotalSeconds > 0)
            {
                _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
                InvokeAsync(StateHasChanged); // Update the UI
            }
            else
            {
                _timer?.Dispose(); // Stop the timer
                Timerstopped.InvokeAsync(true);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        private string timeLeft => FormatTimeSpan(_timeLeft);
    }
}

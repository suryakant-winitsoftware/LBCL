using System.Diagnostics;
using WINITMobile.Data;
using WINITMobile.Services;

public class StopwatchService
{
    private DateTime _startTime;
    private readonly SecureStorageHelper _storageHelper;
    private bool _isRunning;
    private bool _isUpdating;
    public TimeSpan RunningTime { get; private set; } = TimeSpan.Zero;
    public event Action? OnTimeUpdated;
    public DateTime? StartTime { get; private set; }
    public bool IsRunning => _isRunning;

    public StopwatchService()
    {
        _storageHelper = new SecureStorageHelper();
        RestoreStopwatchState();
    }
    // Static constructor
    static StopwatchService()
    {
        ClearStartTimeKeyIfPresent();
    }

    // Static method to clear the key if present
    private static void ClearStartTimeKeyIfPresent()
    {
        var storageHelper = new SecureStorageHelper();
        string startTimeStr = storageHelper.GetStringFromPreferences("StopwatchStartTime");
        if (!string.IsNullOrEmpty(startTimeStr))
        {
            storageHelper.RemoveKey("StopwatchStartTime");
            Debug.WriteLine("Static Constructor: Cleared StartTime key from storage.");
        }
    }
    public void StartTimer()
    {
        if (!_isRunning)
        {
            _isRunning = true;
            _startTime = DateTime.UtcNow;

            // Save Start Time if not set
            if (string.IsNullOrEmpty(_storageHelper.GetStringFromPreferences("StopwatchStartTime")))
            {
                StartTime = _startTime;
                _storageHelper.SaveStringToPreference("StopwatchStartTime", StartTime.Value.ToString("O"));
            }
            else
            {
                StartTime = DateTime.Parse(_storageHelper.GetStringFromPreferences("StopwatchStartTime"));
            }
            RunBackgroundTimer();
        }
    }


    public void StopTimer()
    {
        if (_isRunning)
        {
            _isRunning = false;
            _storageHelper.RemoveKey("StopwatchStartTime");
        }
    }

    private async void RunBackgroundTimer()
    {
        if (_isUpdating) return;

        _isUpdating = true;
        while (_isRunning)
        {
            if (StartTime.HasValue)
            {
                RunningTime = DateTime.UtcNow - StartTime.Value;
            }

            OnTimeUpdated?.Invoke();
            await Task.Delay(1000);
        }
        _isUpdating = false;
    }

    public void ResetTimer()
    {
        _isRunning = false;
        RunningTime = TimeSpan.Zero;
        StartTime = null;
        _storageHelper.RemoveKey("StopwatchStartTime");
        OnTimeUpdated?.Invoke();
    }

    private void RestoreStopwatchState()
    {
        string startTimeStr = _storageHelper.GetStringFromPreferences("StopwatchStartTime");
        if (!string.IsNullOrEmpty(startTimeStr))
        {
            var startTime = DateTime.Parse(startTimeStr);
            RunningTime = DateTime.UtcNow - startTime;
            _isRunning = true;
            RunBackgroundTimer();
        }
        else
        {
            _isRunning = false;
            RunningTime = TimeSpan.Zero;
            StartTime = null;
        }
    }
}

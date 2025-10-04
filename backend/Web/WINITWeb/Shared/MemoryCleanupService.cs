namespace WinIt.Shared
{
    public class MemoryCleanupService
    {
        public event Action? OnCleanupRequested;

        public void RequestCleanup()
        {
            OnCleanupRequested?.Invoke();
            GC.Collect();
            //GC.WaitForPendingFinalizers();

        }
    }
}

namespace WINITSyncManager.Constants
{
    public static class CronExpressions
    {
        public const string EveryMinute = "* * * * *";               // Runs every minute
        public const string EveryFiveMinutes = "*/5 * * * *";        // Runs every 5 minutes
        public const string EveryTenMinutes = "*/10 * * * *";        // Runs every 10 minutes
        public const string EveryFifteenMinutes = "*/15 * * * *";    // Runs every 15 minutes
        public const string EveryTwentyMinutes = "*/20 * * * *";     // Runs every 20 minutes
        public const string EveryThirtyMinutes = "*/30 * * * *";     // Runs every 30 minutes
        public const string EveryHour = "0 * * * *";                 // Runs every hour
        public const string EveryThreeHours = "0 */3 * * *";         // Runs every 3 hours
        public const string EveryFiveHours = "0 */5 * * *";          // Runs every 5 hours
        public const string EveryTenHours = "0 */10 * * *";          // Runs every 10 hours
        public const string EveryFifteenHours = "0 */15 * * *";      // Runs every 15 hours
        public const string EveryTwentyHours = "0 */20 * * *";       // Runs every 20 hours
        public const string EveryTwentyFourHours = "0 0 * * *";      // Runs every 24 hours (midnight)
        public const string EveryOneDay = "0 0 0 */1 * *";           // Runs every day
        public const string EveryTwoDays = "0 0 0 */2 * *";          // Runs every two day
    }
}

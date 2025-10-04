namespace Winit.Shared.Models.Common
{
    public interface IAppConfig
    {
        string AppEnvironment { get; }
        string ApiBaseUrl { get; }
        string AuditTrialApiBaseUrl { get; }
        string ApiDataBaseUrl { get; }
        string NotificationApiUrl { get; }
        string ApplicationName { get; }
        string ExternalDownloadFolderPath { get; }
        string BaseFolderPath { get; }
        string BaseFolderPathForMail { get; }
        string DataFolderPathInternal { get; }
        string DataFolderPathExternal { get; }
        string SqliteZipFileNameTemp { get; }
        string SqliteFileName { get; }
        string SqliteTempPath { get; }
        string SqliteFilePath { get; }
        string ErrorLogFileName { get; }
        string ErrorLogPath { get; }
        string ErrorLogFilePath { get; }
        string RabbitMqueueBaseUrl { get; }

        void Initialize(string environment);
    }
}

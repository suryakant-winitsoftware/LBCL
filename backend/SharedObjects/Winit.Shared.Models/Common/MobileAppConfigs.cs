using Winit.Shared.Models.Constants;

namespace Winit.Shared.Models.Common
{
    public class MobileAppConfigs : IAppConfig
    {
        public string AppEnvironment { get; private set; }
        public string RabbitMqueueBaseUrl { get; private set; }
        public string ApiBaseUrl { get; private set; }
        public string AuditTrialApiBaseUrl { get; private set; }
        public string ApiDataBaseUrl { get; private set; }
        public string NotificationApiUrl { get; private set; }
        public string ApplicationName { get; private set; }
        public string ExternalDownloadFolderPath { get; private set; }
        public string BaseFolderPath { get; private set; }
        public string BaseFolderPathForMail { get; private set; }
        public string DataFolderPathInternal { get; private set; }
        public string DataFolderPathExternal { get; private set; }
        public string SqliteZipFileNameTemp { get; private set; }
        public string SqliteFileName { get; private set; }
        public string SqliteTempPath { get; private set; }
        public string SqliteFilePath { get; private set; }
        public string ErrorLogFileName { get; private set; }
        public string ErrorLogPath { get; private set; }
        public string ErrorLogFilePath { get; private set; }

        public MobileAppConfigs(string environment, string baseFolderPath = "/")
        {
            BaseFolderPath = baseFolderPath;
            Initialize(environment);
        }
        public void Initialize(string environment)
        {
            AppEnvironment = environment;
            ApplicationName = "WINIT" + AppEnvironment;
            ExternalDownloadFolderPath = "";
            //BaseFolderPath = baseFolderPath;//Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            SqliteZipFileNameTemp = "SalesmanDbTemp.zip";
            SqliteFileName = "SalesmanDb.sqlite ";
            ErrorLogFileName = "ErrorLog_" + DateTime.Now.Date;

            DataFolderPathInternal = System.IO.Path.Combine(BaseFolderPath, ApplicationName);
            DataFolderPathExternal = System.IO.Path.Combine(ExternalDownloadFolderPath, ApplicationName);
            SqliteTempPath = System.IO.Path.Combine(ExternalDownloadFolderPath, SqliteZipFileNameTemp);
            SqliteFilePath = System.IO.Path.Combine(DataFolderPathInternal, SqliteFileName);
            ErrorLogPath = System.IO.Path.Combine(DataFolderPathExternal, "ErrorLog");
            ErrorLogFilePath = System.IO.Path.Combine(ErrorLogPath, ErrorLogFileName);
            if (environment == EnvironmentName.Local)
            {
                //Local
                ApiBaseUrl = "https://multiplex-promotions-api.winitsoftware.com/api/";
                ApiDataBaseUrl = "http://localhost:8000/";
                RabbitMqueueBaseUrl = "https://localhost:7164/api/";
            }
            else if (environment == EnvironmentName.Development)
            {
                //Dev - Updated to use local API
                ApiBaseUrl = "https://multiplex-promotions-api.winitsoftware.com/api/";
                ApiDataBaseUrl = "http://localhost:8000/";
                RabbitMqueueBaseUrl = "https://farmely-dev.winitsoftware.com/PostingAPI/api/";

            }
            else if (environment == EnvironmentName.LinuxDevelopment)
            {
                //LinuxDevelopment
                ApiBaseUrl = "https://farmely-dev.winitsoftware.com/WINITAPI/api/";
                ApiDataBaseUrl = "https://farmely-dev.winitsoftware.com/WINITAPI/";
                RabbitMqueueBaseUrl = "https://farmely-dev.winitsoftware.com/PostingAPI/api/";

            }
            else
            {
                ApiBaseUrl = "https://farmely.winitsoftware.com/WINITAPI/api/";
                ApiDataBaseUrl = "https://farmely.winitsoftware.com/WINITAPI/";
                RabbitMqueueBaseUrl = "https://farmely.winitsoftware.com/PostingAPI/api/";
            }
        }
    }

}

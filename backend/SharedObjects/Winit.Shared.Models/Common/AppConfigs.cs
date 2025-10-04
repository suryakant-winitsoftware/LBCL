using Microsoft.Extensions.Configuration;
using Winit.Shared.Models.Constants;

namespace Winit.Shared.Models.Common
{
    public class AppConfigs : IAppConfig
    {
        public string RabbitMqueueBaseUrl { get; }
        public string AppEnvironment { get; private set; }
        public string ApiBaseUrl { get; private set; }
        public string AuditTrialApiBaseUrl { get; private set; }
        public string NotificationApiUrl { get; private set; }
        public string ApiDataBaseUrl { get; private set; }
        public string AppImagePath { get; private set; }
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
        private readonly IConfiguration _configuration;
        public AppConfigs(IConfiguration configuration)
        {
            _configuration = configuration;
            Initialize(EnvironmentName.Development);
            // Only override if configuration values are present
            ApiBaseUrl = _configuration["ApiBaseUrl"] ?? ApiBaseUrl;
            AuditTrialApiBaseUrl = _configuration["AuditTrialApiBaseUrl"] ?? AuditTrialApiBaseUrl;
            ApiDataBaseUrl = _configuration["ApiDataBaseUrl"] ?? ApiDataBaseUrl;
            NotificationApiUrl = _configuration["NotificationApiUrl"] ?? NotificationApiUrl;
        }
        public void Initialize(string environment)
        {
            AppEnvironment = environment;
            ApplicationName = "WINIT" + AppEnvironment;
            ExternalDownloadFolderPath = "";
            BaseFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);            
            SqliteZipFileNameTemp = "SalesmanDbTemp.zip";
            SqliteFileName = "SalesmanDb.sqlite ";
            ErrorLogFileName = "ErrorLog_" + DateTime.Now.Date;

            DataFolderPathInternal = System.IO.Path.Combine(BaseFolderPath, ApplicationName);
            DataFolderPathExternal = System.IO.Path.Combine(ExternalDownloadFolderPath, ApplicationName);
            SqliteTempPath = System.IO.Path.Combine(ExternalDownloadFolderPath, SqliteZipFileNameTemp);
            SqliteFilePath = System.IO.Path.Combine(DataFolderPathInternal, SqliteFileName);
            ErrorLogPath = System.IO.Path.Combine(DataFolderPathExternal, "ErrorLog");
            ErrorLogFilePath = System.IO.Path.Combine(ErrorLogPath, ErrorLogFileName);
            //if (environment == EnvironmentName.Development)
            //{
            //    //ApiBaseUrl = "http://192.168.99.2/WINITAPIPUBLISHED/api/";
            //    //ApiDataBaseUrl = "http://192.168.99.2/WINITAPIPUBLISHED/";
            //    //ApiBaseUrl = "https://localhost:5001/api/";
            ApiBaseUrl = "https://multiplex-promotions-api.winitsoftware.com/api/";
                ApiDataBaseUrl = "http://localhost:8000/";
            //    AppImagePath = "https :///M:/prem/app/WINITAPI/";

            //}
            //else
            //{
            //    ApiBaseUrl = "https://netcore7.winitsoftware.com/winitapi/api/";
            //    ApiDataBaseUrl = "https://netcoretest.winitsoftware.com/";
            //    AppImagePath = "https :///M:/prem/app/WINITAPI/";
            //}
        }
    }

}

using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.BL.Classes.BalanceConfirmation
{
    public abstract class BalanceConfirmationBaseViewModel : IBalanceConfirmationViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public List<FilterCriteria> FilterCriterias { get; set; } = new List<FilterCriteria>();
        public List<SortCriteria> SortCriterias { get; set; } = new List<SortCriteria>();
        public List<IStoreStatement> StoreStatementDetails { get; set; }
        public IBalanceConfirmation BalanceConfirmationDetails { get; set; }
        public List<IBalanceConfirmation> BalanceConfirmationListDetails { get; set; }
        public List<IBalanceConfirmationLine> BalanceConfirmationLineDetails { get; set; }
        public IContact ContactDetails { get; set; }
        public string imageSrc { get; set; } = "";
        public BalanceConfirmationBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appUser = appUser;
            _appConfig = appConfig;
            StoreStatementDetails = new List<IStoreStatement>();
            BalanceConfirmationDetails = new Model.Classes.BalanceConfirmation();
            ContactDetails = new Winit.Modules.Contact.Model.Classes.Contact();
            BalanceConfirmationListDetails = new List<IBalanceConfirmation>();
            BalanceConfirmationLineDetails = new List<IBalanceConfirmationLine>();
        }

        public async Task PopulateViewModel()
        {
            try
            {
                await GetBalanceConfirmationTableDetails("159823");
                await GetStoreStatementData();
                await GetContactDetails(_appUser.Emp.Code);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public string GetHtmlContent(string fileName)
        {
            string fullPath = Path.Combine("D:/Carrier/Web/WINITWeb/Data", fileName);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {fullPath}");
            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BalanceConfirmationTemplate.html");
            string htmlContent = File.ReadAllText(path);
            Console.WriteLine(htmlContent);

            return GetFileContent(fullPath);
        }
        public static string GetFileContent(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath); // Simplified method
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}"); // Log the exception
                throw; // Rethrow to handle in the calling method
            }
        }
        public async Task OnSortApply(SortCriteria sortCriteria)
        {
            try
            {
                SortCriterias.Clear();
                SortCriterias.Add(sortCriteria);
                await GetStoreStatementData();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnFilterApply(Dictionary<string, string> keyValuePairs, string pageName)
        {
            string fromDateValue = "";
            string toDateValue = "";

            foreach (var keyValue in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if (keyValue.Value.Contains(","))
                    {
                        string[] values = keyValue.Value.Split(',');
                        FilterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                    }
                    else
                    {
                        if (keyValue.Key == "FromDate" || keyValue.Key == "ToDate")
                        {
                            switch (keyValue.Key)
                            {
                                case "FromDate":
                                    fromDateValue = ConvertIntoFormat(keyValue.Value);
                                    break;
                                case "ToDate":
                                    toDateValue = ConvertIntoFormat(keyValue.Value);
                                    break;
                            }
                        }
                        else
                        {
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
            }
            //if (!string.IsNullOrEmpty(fromDateValue) && !string.IsNullOrEmpty(toDateValue))
            //{
            //    // Create FilterCriteria for Between
            //    string[] dateValues = { fromDateValue, toDateValue };
            //    string ColumnName = "";
            //    switch (pageName)
            //    {
            //        case "NonCashSettlement":
            //            ColumnName = "ChequeDate";
            //            break;
            //        case "CashSettlement":
            //            ColumnName = "CollectedDate";
            //            break;
            //        case "ViewPayments":
            //            ColumnName = "CollectedDate";
            //            break;
            //    }
            //FilterCriterias.Add(new FilterCriteria(ColumnName, dateValues, FilterType.Between));
            //}
            await GetStoreStatementData();
        }
        public string ConvertIntoFormat(string value)
        {
            try
            {
                string dateValueString = value;
                DateTime dateValue;

                if (DateTime.TryParse(dateValueString, out dateValue))
                {
                    return dateValue.ToString("yyyy-MM-dd");
                    // Use the formattedDate as needed
                }
                return value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task SendSms(string Otp, string MobileNumber)
        {
            try
            {
                await InsertSmsIntoRabbitMQ(Otp, MobileNumber);
            }
            catch (Exception ex)
            {
                // Log Exception
            }
        }


        public abstract Task<List<IStoreStatement>> GetStoreStatementData();
        public abstract Task GetContactDetails(string EmpCode);
        public abstract Task<bool> InsertDisputeRecords(List<IBalanceConfirmationLine> balanceConfirmationLine);
        public abstract Task GetBalanceConfirmationTableDetails(string StoreUID);
        public abstract Task GetBalanceConfirmationTableListDetails();
        public abstract Task GetBalanceConfirmationLineTableDetails(string UID);
        public abstract Task<bool> UpdateDisputeResolved(IBalanceConfirmation balanceConfirmation);
        public abstract Task<bool> UpdateBalanceConfirmation(IBalanceConfirmation balanceConfirmation);
        public abstract Task<bool> InsertSmsIntoRabbitMQ(string Otp, string MobileNumber);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.DL.Classes
{
    public class PGSQLStoreAdditionalInfoCMIDL : Base.DL.DBManager.PostgresDBManager, Interfaces.IStoreAdditionalInfoCMIDL
    {
        public PGSQLStoreAdditionalInfoCMIDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }

        public async Task<int> UpdateDistBusinessDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)

        {
            try
            {

                var sql = @"UPDATE store_additional_info_cmi   
                                SET 
    modified_by = @ModifiedBy, 
    modified_time = @ModifiedTime,  
    server_modified_time = @ServerModifiedTime, 


    dof = @DateOfFoundation, 
    dob = @DateOfBirth, 
    wedding_date = @WeddingDate, 
    no_of_manager = @NoOfManager, 
    no_of_sales_team = @NoOfSalesTeam, 
    no_of_commercial = @NoOfCommercial, 
    no_of_service = @NoOfService, 
    no_of_others = @NoOfOthers, 
    total_emp = @TotalEmp, 
    showroom_details = @ShowroomDetails, 
    bank_details = @BankDetails, 
    signatory_details = @SignatoryDetails, 
    brand_dealing_in_details = @BrandDealingInDetails, 
    product_dealing_in = @ProductDealingIn, 
    area_of_operation = @AreaOfOperation, 
    dist_products = @DistProducts, 
    dist_area_of_operation = @DistAreaOfOperation, 
    dist_brands = @DistBrands, 
    dist_monthly_sales = @DistMonthlySales, 
    dist_no_of_sub_dealers = @DistNoOfSubDealers, 
    dist_retailing_city_monthly_sales = @DistRetailingCityMonthlySales, 
    dist_rac_sales_by_year = @DistRacSalesByYear, 
    eww_has_worked_with_cmi = @EwwHasWorkedWithCMI, 
    eww_year_of_operation_and_volume = @EwwYearOfOperationAndVolume, 
    eww_dealer_info = @EwwDealerInfo, 
    eww_name_of_firms = @EwwNameOfFirms, 
    eww_total_investment = @EwwTotalInvestment, 
    aoda_expected_to_1 = @AodaExpectedTo1, 
    aoda_expected_to_2 = @AodaExpectedTo2, 
    aoda_expected_to_3 = @AodaExpectedTo3, 
    aoda_has_office = @AodaHasOffice, 
    aoda_has_godown = @AodaHasGodown, 
    aoda_has_manpower = @AodaHasManpower, 
    aoda_has_service_center = @AodaHasServiceCenter, 
    aoda_has_delivery_van = @AodaHasDeliveryVan, 
    aoda_has_salesman = @AodaHasSalesman, 
    aoda_has_computer = @AodaHasComputer, 
    aoda_has_others = @AodaHasOthers, 
    ap_market_reputation_level1 = @ApMarketReputationLevel1, 
    ap_market_reputation_level2 = @ApMarketReputationLevel2, 
    ap_market_reputation_level3 = @ApMarketReputationLevel3, 
    ap_display_quantity_level1 = @ApDisplayQuantityLevel1, 
    ap_display_quantity_level2 = @ApDisplayQuantityLevel2, 
    ap_display_quantity_level3 = @ApDisplayQuantityLevel3, 
    ap_dist_ret_strength_level1 = @ApDistRetStrengthLevel1, 
    ap_dist_ret_strength_level2 = @ApDistRetStrengthLevel2, 
    ap_dist_ret_strength_level3 = @ApDistRetStrengthLevel3, 
    ap_financial_strength_level1 = @ApFinancialStrengthLevel1, 
    ap_financial_strength_level2 = @ApFinancialStrengthLevel2, 
    ap_financial_strength_level3 = @ApFinancialStrengthLevel3, 
    is_agreed_with_tnc = @IsAgreedWithTNC, 
    store_uid = @StoreUid 
                                WHERE uid = @UID";



                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateEmployeeDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)

        {
            try
            {

                var sql = @"UPDATE store_additional_info_cmi   
                                SET 
    modified_by = @ModifiedBy, 
    modified_time = @ModifiedTime,  
    server_modified_time = @ServerModifiedTime, 


    dof = @DateOfFoundation, 
    dob = @DateOfBirth, 
    wedding_date = @WeddingDate, 
    no_of_manager = @NoOfManager, 
    no_of_sales_team = @NoOfSalesTeam, 
    no_of_commercial = @NoOfCommercial, 
    no_of_service = @NoOfService, 
    no_of_others = @NoOfOthers, 
    total_emp = @TotalEmp, 
    showroom_details = @ShowroomDetails, 
    bank_details = @BankDetails, 
    signatory_details = @SignatoryDetails, 
    brand_dealing_in_details = @BrandDealingInDetails, 
    product_dealing_in = @ProductDealingIn, 
    area_of_operation = @AreaOfOperation, 
    dist_products = @DistProducts, 
    dist_area_of_operation = @DistAreaOfOperation, 
    dist_brands = @DistBrands, 
    dist_monthly_sales = @DistMonthlySales, 
    dist_no_of_sub_dealers = @DistNoOfSubDealers, 
    dist_retailing_city_monthly_sales = @DistRetailingCityMonthlySales, 
    dist_rac_sales_by_year = @DistRacSalesByYear, 
    eww_has_worked_with_cmi = @EwwHasWorkedWithCMI, 
    eww_year_of_operation_and_volume = @EwwYearOfOperationAndVolume, 
    eww_dealer_info = @EwwDealerInfo, 
    eww_name_of_firms = @EwwNameOfFirms, 
    eww_total_investment = @EwwTotalInvestment, 
    aoda_expected_to_1 = @AodaExpectedTo1, 
    aoda_expected_to_2 = @AodaExpectedTo2, 
    aoda_expected_to_3 = @AodaExpectedTo3, 
    aoda_has_office = @AodaHasOffice, 
    aoda_has_godown = @AodaHasGodown, 
    aoda_has_manpower = @AodaHasManpower, 
    aoda_has_service_center = @AodaHasServiceCenter, 
    aoda_has_delivery_van = @AodaHasDeliveryVan, 
    aoda_has_salesman = @AodaHasSalesman, 
    aoda_has_computer = @AodaHasComputer, 
    aoda_has_others = @AodaHasOthers, 
    ap_market_reputation_level1 = @ApMarketReputationLevel1, 
    ap_market_reputation_level2 = @ApMarketReputationLevel2, 
    ap_market_reputation_level3 = @ApMarketReputationLevel3, 
    ap_display_quantity_level1 = @ApDisplayQuantityLevel1, 
    ap_display_quantity_level2 = @ApDisplayQuantityLevel2, 
    ap_display_quantity_level3 = @ApDisplayQuantityLevel3, 
    ap_dist_ret_strength_level1 = @ApDistRetStrengthLevel1, 
    ap_dist_ret_strength_level2 = @ApDistRetStrengthLevel2, 
    ap_dist_ret_strength_level3 = @ApDistRetStrengthLevel3, 
    ap_financial_strength_level1 = @ApFinancialStrengthLevel1, 
    ap_financial_strength_level2 = @ApFinancialStrengthLevel2, 
    ap_financial_strength_level3 = @ApFinancialStrengthLevel3, 
    is_agreed_with_tnc = @IsAgreedWithTNC, 
    store_uid = @StoreUid 
                                WHERE uid = @UID";



                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            try
            {

                                        var sql = @"INSERT INTO public.store_additional_info_cmi 
                        (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                         dof, dob, wedding_date, no_of_manager, no_of_sales_team, no_of_commercial, no_of_service, 
                         no_of_others, total_emp, showroom_details, bank_details, signatory_details, brand_dealing_in_details, 
                         product_dealing_in, area_of_operation, dist_products, dist_area_of_operation, dist_brands, 
                         dist_monthly_sales, dist_no_of_sub_dealers, dist_retailing_city_monthly_sales, dist_rac_sales_by_year, 
                         eww_has_worked_with_cmi, eww_year_of_operation_and_volume, eww_dealer_info, eww_name_of_firms, 
                         eww_total_investment, aoda_expected_to_1, aoda_expected_to_2, aoda_expected_to_3, aoda_has_office, 
                         aoda_has_godown, aoda_has_manpower, aoda_has_service_center, aoda_has_delivery_van, aoda_has_salesman, 
                         aoda_has_computer, aoda_has_others, ap_market_reputation_level1, ap_market_reputation_level2, 
                         ap_market_reputation_level3, ap_display_quantity_level1, ap_display_quantity_level2, 
                         ap_display_quantity_level3, ap_dist_ret_strength_level1, ap_dist_ret_strength_level2, 
                         ap_dist_ret_strength_level3, ap_financial_strength_level1, ap_financial_strength_level2, 
                         ap_financial_strength_level3, is_agreed_with_tnc, store_uid,is_tc_agreed)
                        VALUES 
                        (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                         @DateOfFoundation, @DateOfBirth, @WeddingDate, @NoOfManager, @NoOfSalesTeam, @NoOfCommercial, 
                         @NoOfService, @NoOfOthers, @TotalEmp, @ShowroomDetails::json, @BankDetails::json, @SignatoryDetails::json, 
                         @BrandDealingInDetails::json, @ProductDealingIn, @AreaOfOperation, @DistProducts, @DistAreaOfOperation, 
                         @DistBrands, @DistMonthlySales, @DistNoOfSubDealers, @DistRetailingCityMonthlySales::json, @DistRacSalesByYear::json, 
                         @EwwHasWorkedWithCMI, @EwwYearOfOperationAndVolume, @EwwDealerInfo, @EwwNameOfFirms, @EwwTotalInvestment, 
                         @AodaExpectedTo1, @AodaExpectedTo2, @AodaExpectedTo3, @AodaHasOffice, @AodaHasGodown, @AodaHasManpower, 
                         @AodaHasServiceCenter, @AodaHasDeliveryVan, @AodaHasSalesman, @AodaHasComputer, @AodaHasOthers, 
                         @ApMarketReputationLevel1, @ApMarketReputationLevel2, @ApMarketReputationLevel3, @ApDisplayQuantityLevel1, 
                         @ApDisplayQuantityLevel2, @ApDisplayQuantityLevel3, @ApDistRetStrengthLevel1, @ApDistRetStrengthLevel2, 
                         @ApDistRetStrengthLevel3, @ApFinancialStrengthLevel1, @ApFinancialStrengthLevel2, @ApFinancialStrengthLevel3, 
                         @IsAgreedWithTNC, @StoreUid,@IsTcAgreed)";


                //            var parameters = new Dictionary<string, object>
                //{
                //    {"DateOfFoundation", model.DateOfFoundation},
                //    {"DateOfBirth", model.DateOfBirth},
                //    {"WeddingDate", model.WeddingDate},
                //    {"NoOfManager", model.NoOfManager},
                //    {"NoOfSalesTeam", model.NoOfSalesTeam},
                //    {"NoOfCommercial", model.NoOfCommercial},
                //    {"NoOfService", model.NoOfService},
                //    {"NoOfOthers", model.NoOfOthers},
                //    {"TotalEmp", model.TotalEmp},
                //    {"NoOfStores", model.NoOfStores},
                //    {"BankAccountNo", model.BankAccountNo},
                //    {"SN", model.SN},
                //    {"ProductDealingIn", model.ProductDealingIn},
                //    {"AreaOfOperation", model.AreaOfOperation},
                //    {"Products", model.Products},
                //    {"DistAreaOfOperation", model.DistAreaOfOperation},
                //    {"Brands", model.Brands},
                //    {"MonthlySales", model.MonthlySales},
                //    {"NoOfTotalSubDealers", model.NoOfTotalSubDealers},
                //    {"HasEarlierWorkedWIthCMI", model.HasEarlierWorkedWIthCMI},
                //    {"YearOfOperationAndBusinessVolume", model.YearOfOperationAndBusinessVolume},
                //    {"DealerOrDIstributor", model.DealerOrDIstributor},
                //    {"NameOfTheFirm", model.NameOfTheFirm},
                //    {"TotalInvestment", model.TotalInvestment},
                //    {"ExpectedTO1", model.ExpectedTO1},
                //    {"ExpectedTO2", model.ExpectedTO2},
                //    {"ExpectedTO3", model.ExpectedTO3},
                //    {"OfficeForCMI", model.OfficeForCMI},
                //    {"GoDownForCMI", model.GoDownForCMI},
                //    {"ManPowerForCMI", model.ManPowerForCMI},
                //    {"ServiceCenterForCMI", model.ServiceCenterForCMI},
                //    {"DeliveryVanForCMI", model.DeliveryVanForCMI},
                //    {"SalesManPowerForCMI", model.SalesManPowerForCMI},
                //    {"ComputerForCMI", model.ComputerForCMI},
                //    {"OthersForCMI", model.OthersForCMI},
                //    {"MarketReputationAPLevel1", model.MarketReputationAPLevel1},
                //    {"MarketReputationAPLevel2", model.MarketReputationAPLevel2},
                //    {"MarketReputationAPLevel3", model.MarketReputationAPLevel3},
                //    {"DisplayQualityAPLevel1", model.DisplayQualityAPLevel1},
                //    {"DisplayQualityAPLevel2", model.DisplayQualityAPLevel2},
                //    {"DisplayQualityAPLevel3", model.DisplayQualityAPLevel3},
                //    {"DistributionRetailStrengthAPLevel1", model.DistributionRetailStrengthAPLevel1},
                //    {"DistributionRetailStrengthAPLevel2", model.DistributionRetailStrengthAPLevel2},
                //    {"DistributionRetailStrengthAPLevel3", model.DistributionRetailStrengthAPLevel3},
                //    {"FinancialStrengthAPLevel1", model.FinancialStrengthAPLevel1},
                //    {"FinancialStrengthAPLevel2", model.FinancialStrengthAPLevel2},
                //    {"FinancialStrengthAPLevel3", model.FinancialStrengthAPLevel3},
                //    {"IsAgreedWithTNC", model.IsAgreedWithTNC}
                //};

                return await ExecuteNonQueryAsync(sql, model);

            }

            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)

        {
            try
            {

                var sql = @"
    UPDATE public.store_additional_info_cmi 
    SET 
        dof = @DateOfFoundation, 
        dob = @DateOfBirth, 
        wedding_date = @WeddingDate, 
        no_of_manager = @NoOfManager, 
        no_of_sales_team = @NoOfSalesTeam, 
        no_of_commercial = @NoOfCommercial, 
        no_of_service = @NoOfService, 
        no_of_others = @NoOfOthers, 
        total_emp = @TotalEmp, 
        showroom_details = @ShowroomDetails::json, 
        bank_details = @BankDetails::json, 
        signatory_details = @SignatoryDetails::json, 
        brand_dealing_in_details = @BrandDealingInDetails::json, 
        product_dealing_in = @ProductDealingIn, 
        area_of_operation = @AreaOfOperation, 
        dist_products = @DistProducts, 
        dist_area_of_operation = @DistAreaOfOperation, 
        dist_brands = @DistBrands, 
        dist_monthly_sales = @DistMonthlySales, 
        dist_no_of_sub_dealers = @DistNoOfSubDealers, 
        dist_retailing_city_monthly_sales = @DistRetailingCityMonthlySales::json, 
        dist_rac_sales_by_year = @DistRacSalesByYear::json, 
        eww_has_worked_with_cmi = @EwwHasWorkedWithCMI, 
        eww_year_of_operation_and_volume = @EwwYearOfOperationAndVolume, 
        eww_dealer_info = @EwwDealerInfo, 
        eww_name_of_firms = @EwwNameOfFirms, 
        eww_total_investment = @EwwTotalInvestment, 
        aoda_expected_to_1 = @AodaExpectedTo1, 
        aoda_expected_to_2 = @AodaExpectedTo2, 
        aoda_expected_to_3 = @AodaExpectedTo3, 
        aoda_has_office = @AodaHasOffice, 
        aoda_has_godown = @AodaHasGodown, 
        aoda_has_manpower = @AodaHasManpower, 
        aoda_has_service_center = @AodaHasServiceCenter, 
        aoda_has_delivery_van = @AodaHasDeliveryVan, 
        aoda_has_salesman = @AodaHasSalesman, 
        aoda_has_computer = @AodaHasComputer, 
        aoda_has_others = @AodaHasOthers, 
        ap_market_reputation_level1 = @ApMarketReputationLevel1, 
        ap_market_reputation_level2 = @ApMarketReputationLevel2, 
        ap_market_reputation_level3 = @ApMarketReputationLevel3, 
        ap_display_quantity_level1 = @ApDisplayQuantityLevel1, 
        ap_display_quantity_level2 = @ApDisplayQuantityLevel2, 
        ap_display_quantity_level3 = @ApDisplayQuantityLevel3, 
        ap_dist_ret_strength_level1 = @ApDistRetStrengthLevel1, 
        ap_dist_ret_strength_level2 = @ApDistRetStrengthLevel2, 
        ap_dist_ret_strength_level3 = @ApDistRetStrengthLevel3, 
        ap_financial_strength_level1 = @ApFinancialStrengthLevel1, 
        ap_financial_strength_level2 = @ApFinancialStrengthLevel2, 
        ap_financial_strength_level3 = @ApFinancialStrengthLevel3, 
        is_agreed_with_tnc = @IsAgreedWithTNC, 
        store_uid = @StoreUid,
        is_tc_agreed=@IsTcAgreed
    WHERE uid = @UID";


                return await ExecuteNonQueryAsync(sql, model);
                                        }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateBusinessDetailsInStoreAdditionalInfoCMI1(Model.Interfaces.IStoreAdditionalInfoCMI model)

        {
            try
            {

                var sql = @"UPDATE store_additional_info_cmi   
                                SET 
                  modified_by = @ModifiedBy, 
                  modified_time = @ModifiedTime,  
                  server_modified_time = @ServerModifiedTime, 
                brand_dealing_in_details = @BrandDealingInDetails, 
               product_dealing_in = @ProductDealingIn, 
                area_of_operation = @AreaOfOperation, 
                dist_products = @DistProducts, 
               dist_area_of_operation = @DistAreaOfOperation, 
               dist_brands = @DistBrands, 
              dist_monthly_sales = @DistMonthlySales, 

                                WHERE uid = @UID";

                //var parameters = new Dictionary<string, object>
                //                {
                //                    {"UID", model.UID},
                //                    {"DateOfFoundation", model.DateOfFoundation},
                //                    {"DateOfBirth", model.DateOfBirth},
                //                    {"WeddingDate", model.WeddingDate},
                //                    {"NoOfManager", model.NoOfManager},
                //                    {"NoOfSalesTeam", model.NoOfSalesTeam},
                //                    {"NoOfCommercial", model.NoOfCommercial},
                //                    {"NoOfService", model.NoOfService},
                //                    {"NoOfOthers", model.NoOfOthers},
                //                    {"TotalEmp", model.TotalEmp},
                //                    {"NoOfStores", model.NoOfStores},
                //                    {"BankAccountNo", model.BankAccountNo},
                //                    {"SN", model.SN},
                //                    {"ProductDealingIn", model.ProductDealingIn},
                //                    {"AreaOfOperation", model.AreaOfOperation},
                //                    {"Products", model.Products},
                //                    {"DistAreaOfOperation", model.DistAreaOfOperation},
                //                    {"Brands", model.Brands},
                //                    {"MonthlySales", model.MonthlySales},
                //                    {"NoOfTotalSubDealers", model.NoOfTotalSubDealers},
                //                    {"HasEarlierWorkedWIthCMI", model.HasEarlierWorkedWIthCMI},
                //                    {"YearOfOperationAndBusinessVolume", model.YearOfOperationAndBusinessVolume},
                //                    {"DealerOrDIstributor", model.DealerOrDIstributor},
                //                    {"NameOfTheFirm", model.NameOfTheFirm},
                //                    {"TotalInvestment", model.TotalInvestment},
                //                    {"ExpectedTO1", model.ExpectedTO1},
                //                    {"ExpectedTO2", model.ExpectedTO2},
                //                    {"ExpectedTO3", model.ExpectedTO3},
                //                    {"OfficeForCMI", model.OfficeForCMI},
                //                    {"GoDownForCMI", model.GoDownForCMI},
                //                    {"ManPowerForCMI", model.ManPowerForCMI},
                //                    {"ServiceCenterForCMI", model.ServiceCenterForCMI},
                //                    {"DeliveryVanForCMI", model.DeliveryVanForCMI},
                //                    {"SalesManPowerForCMI", model.SalesManPowerForCMI},
                //                    {"ComputerForCMI", model.ComputerForCMI},
                //                    {"OthersForCMI", model.OthersForCMI},
                //                    {"MarketReputationAPLevel1", model.MarketReputationAPLevel1},
                //                    {"MarketReputationAPLevel2", model.MarketReputationAPLevel2},
                //                    {"MarketReputationAPLevel3", model.MarketReputationAPLevel3},
                //                    {"DisplayQualityAPLevel1", model.DisplayQualityAPLevel1},
                //                    {"DisplayQualityAPLevel2", model.DisplayQualityAPLevel2},
                //                    {"DisplayQualityAPLevel3", model.DisplayQualityAPLevel3},
                //                    {"DistributionRetailStrengthAPLevel1", model.DistributionRetailStrengthAPLevel1},
                //                    {"DistributionRetailStrengthAPLevel2", model.DistributionRetailStrengthAPLevel2},
                //                    {"DistributionRetailStrengthAPLevel3", model.DistributionRetailStrengthAPLevel3},
                //                    {"FinancialStrengthAPLevel1", model.FinancialStrengthAPLevel1},
                //                    {"FinancialStrengthAPLevel2", model.FinancialStrengthAPLevel2},
                //                    {"FinancialStrengthAPLevel3", model.FinancialStrengthAPLevel3},
                //                    {"IsAgreedWithTNC", model.IsAgreedWithTNC}
                //                };

                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateServiceCenterDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            try
            {
                var sql = @"UPDATE store_additional_info_cmi   
                            SET 
                                
                                WHERE uid = @UID;";

                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateAreaofOperationAgreedInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            try
            {
                var sql = @"UPDATE store_additional_info_cmi   
                                        SET 
                                        AooaType = @AooaType,
                                        AooaCategory = @AooaCategory,
                                        AooaAspToClose = @AooaAspToClose,
                                        AooaCode = @AooaCode,
                                        AooaProduct = @AooaProduct,
                                        AooaEval1 = @AooaEval1,
                                        AooaEval2 = @AooaEval2,
                                        AooaEval3 = @AooaEval3,
                                        AooaEval4 = @AooaEval4,
                                        AooaEval5 = @AooaEval5,
                                        AooaEval6 = @AooaEval6,
                                        AooaEval7 = @AooaEval7,
                                        AooaEval8 = @AooaEval8,
                                        AooaEval9 = @AooaEval9,
                                        AooaEval10 = @AooaEval10,
                                        AooaEval11 = @AooaEval11,
                                        AooaEval12 = @AooaEval12,
                                        AooaEval13 = @AooaEval13,
                                        AooaEval14 = @AooaEval14,
                                        AooaEval15 = @AooaEval15,
                                        AooaEval16 = @AooaEval16,
                                        AooaEval17 = @AooaEval17,
                                        AooaEval18 = @AooaEval18,
                                        AooaEval19 = @AooaEval19,
                                        AooaApTechnicalCompLevel1 = @AooaApTechnicalCompLevel1,
                                        AooaApTechnicalCompLevel2 = @AooaApTechnicalCompLevel2,
                                        AooaApTechnicalCompAverage = @AooaApTechnicalCompAverage,
                                        AoaaApManpowerLevel1 = @AoaaApManpowerLevel1,
                                        AoaaApManpowerLevel2 = @AoaaApManpowerLevel2,
                                        AoaaApManpowerAverage = @AoaaApManpowerAverage,
                                        AoaaApWorkshopLevel1 = @AoaaApWorkshopLevel1,
                                        AoaaApWorkshopLevel2 = @AoaaApWorkshopLevel2,
                                        AoaaApWorkshopAverage = @AoaaApWorkshopAverage,
                                        AoaaApToolsLevel1 = @AoaaApToolsLevel1,
                                        AoaaApToolsLevel2 = @AoaaApToolsLevel2,
                                        AoaaApToolsAverage = @AoaaApToolsAverage,
                                        AoaaApComputerLevel1 = @AoaaApComputerLevel1,
                                        AoaaApComputerLevel2 = @AoaaApComputerLevel2,
                                        AoaaApComputerAverage = @AoaaApComputerAverage,
                                        AoaaApFinancialStrengthLevel1 = @AoaaApFinancialStrengthLevel1,
                                        AoaaApFinancialStrengthLevel2 = @AoaaApFinancialStrengthLevel2,
                                        AoaaApFinancialStrengthAverage = @AoaaApFinancialStrengthAverage,
                                        AoaaCcFcContactPerson = @AoaaCcFcContactPerson,
                                        AoaaCcFcSentTime = @AoaaCcFcSentTime,
                                        AoaaCcFcReplyReceivedBy = @AoaaCcFcReplyReceivedBy,
                                        AoaaCcSecWayOfCommu = @AoaaCcSecWayOfCommu,
                                        AoaaCcSecContactPerson = @AoaaCcSecContactPerson,
                                        AoaaCcSecSentTime = @AoaaCcSecSentTime,
                                        AoaaCcSecReplyReceivedBy = @AoaaCcSecReplyReceivedBy
                                            WHERE uid = @UID";

                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Model.Interfaces.IStoreAdditionalInfoCMI> SelectStoreAdditionalInfoCMIByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };

                      var sql = @"SELECT
                                uid AS UID,
                                created_by AS CreatedBy,
                                created_time AS CreatedTime,
                                modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime,
                                server_add_time AS ServerAddTime,
                                server_modified_time AS ServerModifiedTime,
                                dof AS DateOfFoundation,
                                dob AS DateOfBirth,
                                wedding_date AS WeddingDate,
                                no_of_manager AS NoOfManager,
                                no_of_sales_team AS NoOfSalesTeam,
                                no_of_commercial AS NoOfCommercial,
                                no_of_service AS NoOfService,
                                no_of_others AS NoOfOthers,
                                total_emp AS TotalEmp,
                                showroom_details::json AS ShowroomDetails,
                                bank_details::json AS BankDetails,
                                signatory_details::json AS SignatoryDetails,
                                brand_dealing_in_details::json AS BrandDealingInDetails,
                                product_dealing_in AS ProductDealingIn,
                                area_of_operation AS AreaOfOperation,
                                dist_products AS DistProducts,
                                dist_area_of_operation AS DistAreaOfOperation,
                                dist_brands AS DistBrands,
                                dist_monthly_sales AS DistMonthlySales,
                                dist_no_of_sub_dealers AS DistNoOfSubDealers,
                                dist_retailing_city_monthly_sales::json AS DistRetailingCityMonthlySales,
                                dist_rac_sales_by_year::json AS DistRacSalesByYear,
                                eww_has_worked_with_cmi AS EwwHasWorkedWithCMI,
                                eww_year_of_operation_and_volume AS EwwYearOfOperationAndVolume,
                                eww_dealer_info AS EwwDealerInfo,
                                eww_name_of_firms AS EwwNameOfFirms,
                                eww_total_investment AS EwwTotalInvestment,
                                aoda_expected_to_1 AS AodaExpectedTo1,
                                aoda_expected_to_2 AS AodaExpectedTo2,
                                aoda_expected_to_3 AS AodaExpectedTo3,
                                aoda_has_office AS AodaHasOffice,
                                aoda_has_godown AS AodaHasGodown,
                                aoda_has_manpower AS AodaHasManpower,
                                aoda_has_service_center AS AodaHasServiceCenter,
                                aoda_has_delivery_van AS AodaHasDeliveryVan,
                                aoda_has_salesman AS AodaHasSalesman,
                                aoda_has_computer AS AodaHasComputer,
                                aoda_has_others AS AodaHasOthers,
                                ap_market_reputation_level1 AS ApMarketReputationLevel1,
                                ap_market_reputation_level2 AS ApMarketReputationLevel2,
                                ap_market_reputation_level3 AS ApMarketReputationLevel3,
                                ap_display_quantity_level1 AS ApDisplayQuantityLevel1,
                                ap_display_quantity_level2 AS ApDisplayQuantityLevel2,
                                ap_display_quantity_level3 AS ApDisplayQuantityLevel3,
                                ap_dist_ret_strength_level1 AS ApDistRetStrengthLevel1,
                                ap_dist_ret_strength_level2 AS ApDistRetStrengthLevel2,
                                ap_dist_ret_strength_level3 AS ApDistRetStrengthLevel3,
                                ap_financial_strength_level1 AS ApFinancialStrengthLevel1,
                                ap_financial_strength_level2 AS ApFinancialStrengthLevel2,
                                ap_financial_strength_level3 AS ApFinancialStrengthLevel3,
                                is_agreed_with_tnc AS IsAgreedWithTNC,
                                store_uid AS StoreUid,
                                is_tc_agreed As IsTcAgreed
                            FROM public.store_additional_info_cmi where uid=@UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreAdditionalInfoCMI>().GetType();
            Model.Interfaces.IStoreAdditionalInfoCMI StoreList = await ExecuteSingleAsync<Model.Interfaces.IStoreAdditionalInfoCMI>(sql, parameters, type);
            return StoreList;
        }
        public async Task<int> CUDStoreAdditionalInfoCMI(IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            int count = -1;
            try
            {
                string? existingUID = await CheckIfUIDExistsInDB(DbTableName.StoreAdditionalInfoCMI, storeAdditionalInfoCMI.UID);
                if (existingUID != null)
                {
                    count = await UpdateStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
                }
                else
                {
                    count = await CreateStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
                }
                return count;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> UpdateStoreAdditionalInfoCMIAsync(Model.Interfaces.IStoreAdditionalInfoCMI model, string specificUpdates)
        {
            try
            {
                // Build the common part of the SQL query
                var commonPart = GetCommonUpdatePart();

                // Combine the common part with the specific updates
                var sql = $@"
            UPDATE store_additional_info_cmi
            SET 
            {commonPart}
            {specificUpdates}
            WHERE uid = @UID";

                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetCommonUpdatePart()
        {
            return @"
        modified_by = @ModifiedBy, 
        modified_time = @ModifiedTime,  
        server_modified_time = @ServerModifiedTime";
        }
        public async Task<int> UpdateShowroomDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            var specificUpdates = @"
        showroom_details = @ShowroomDetails";

            return await UpdateStoreAdditionalInfoCMIAsync(model, specificUpdates);
        }

        public async Task<int> UpdateBusinessDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            var specificUpdates = @"
        brand_dealing_in_details = @BrandDealingInDetails, 
        product_dealing_in = @ProductDealingIn, 
        area_of_operation = @AreaOfOperation, 
        dist_products = @DistProducts, 
        dist_area_of_operation = @DistAreaOfOperation, 
        dist_brands = @DistBrands, 
        dist_monthly_sales = @DistMonthlySales";

            return await UpdateStoreAdditionalInfoCMIAsync(model, specificUpdates);
        }
        public async Task<int> UpdateAreaOfDistAgreedInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            var specificUpdates = @"
        brand_dealing_in_details = @BrandDealingInDetails, 
        product_dealing_in = @ProductDealingIn, 
        area_of_operation = @AreaOfOperation, 
        dist_products = @DistProducts, 
        dist_area_of_operation = @DistAreaOfOperation, 
        dist_brands = @DistBrands, 
        dist_monthly_sales = @DistMonthlySales";

            return await UpdateStoreAdditionalInfoCMIAsync(model, specificUpdates);
        }
        public async Task<int> UpdateBankersDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            var specificUpdates = @"
        brand_dealing_in_details = @BrandDealingInDetails, 
        product_dealing_in = @ProductDealingIn, 
        area_of_operation = @AreaOfOperation, 
        dist_products = @DistProducts, 
        dist_area_of_operation = @DistAreaOfOperation, 
        dist_brands = @DistBrands, 
        dist_monthly_sales = @DistMonthlySales";

            return await UpdateStoreAdditionalInfoCMIAsync(model, specificUpdates);
        }
        public async Task<int> UpdateEarlierWorkWithCMIInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            var specificUpdates = @"
        brand_dealing_in_details = @BrandDealingInDetails, 
        product_dealing_in = @ProductDealingIn, 
        area_of_operation = @AreaOfOperation, 
        dist_products = @DistProducts, 
        dist_area_of_operation = @DistAreaOfOperation, 
        dist_brands = @DistBrands, 
        dist_monthly_sales = @DistMonthlySales";

            return await UpdateStoreAdditionalInfoCMIAsync(model, specificUpdates);
        }
        public async Task<int> UpdateTermAndCondInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            var specificUpdates = @"
       
        is_tc_agreed = @IsTcAgreed";

            return await UpdateStoreAdditionalInfoCMIAsync(model, specificUpdates);
        }

        Task<int> IStoreAdditionalInfoCMIDL.UpdateKartaInStoreAdditionalInfoCMI(IStoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            throw new NotImplementedException();
        }
    }
}

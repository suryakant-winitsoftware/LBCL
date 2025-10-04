using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Store.DL.Classes
{
    public class MSSQLStoreAdditionalInfoCMIDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IStoreAdditionalInfoCMIDL

    {
        public MSSQLStoreAdditionalInfoCMIDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider , config)
        {

        }
        public async Task<int> CreateStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            try
            {

                var sql = @"INSERT INTO store_additional_info_cmi 
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
                         ap_financial_strength_level3, is_agreed_with_tnc, store_uid,aooa_type, aooa_category, aooa_asp_to_close, aooa_code, aooa_product, 
                         aooa_eval_1, aooa_eval_2, aooa_eval_3, aooa_eval_4, aooa_eval_5, 
                         aooa_eval_6, aooa_eval_7, aooa_eval_8, aooa_eval_9, aooa_eval_10, 
                         aooa_eval_11, aooa_eval_12, aooa_eval_13, aooa_eval_14, aooa_eval_15, 
                         aooa_eval_16, aooa_eval_17, aooa_eval_18, aooa_eval_19, 
                         aooa_ap_technical_comp_level1, aooa_ap_technical_comp_level2, aooa_ap_technical_comp_average, 
                         aoaa_ap_manpower_level1, aoaa_ap_manpower_level2, aoaa_ap_manpower_average, 
                         aoaa_ap_workshop_level1, aoaa_ap_workshop_level2, aoaa_ap_workshop_average, 
                         aoaa_ap_tools_level1, aoaa_ap_tools_level2, aoaa_ap_tools_average, 
                         aoaa_ap_computer_level1, aoaa_ap_computer_level2, aoaa_ap_computer_average, 
                         aoaa_ap_financial_strength_level1, aoaa_ap_financial_strength_level2, aoaa_ap_financial_strength_average, 
                         aoaa_cc_fc_contact_person, aoaa_cc_fc_sent_time, aoaa_cc_fc_reply_received_by, 
                         aoaa_cc_sec_way_of_commu, aoaa_cc_sec_contact_person, aoaa_cc_sec_sent_time, aoaa_cc_sec_reply_received_by, 
                         sc_address, sc_address_type, sc_is_service_center_difference_from_principle_place, 
                         sc_area, sc_current_brand_handled, sc_exp_in_year, sc_no_of_technician, sc_technician_data, 
                         sc_no_of_supervisor, sc_supervisor_data, sc_license_no,is_tc_agreed, partner_details";
                if(!string.IsNullOrEmpty(model.SelfRegistrationUID))
                {
                    sql += ", self_registration_uid";
                }
                sql += @" ) VALUES 
                        (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                         @DateOfFoundation, @DateOfBirth, @WeddingDate, @NoOfManager, @NoOfSalesTeam, @NoOfCommercial, 
                         @NoOfService, @NoOfOthers, @TotalEmp, @ShowroomDetails, @BankDetails, @SignatoryDetails, 
                         @BrandDealingInDetails, @ProductDealingIn, @AreaOfOperation, @DistProducts, @DistAreaOfOperation, 
                         @DistBrands, @DistMonthlySales, @DistNoOfSubDealers, @DistRetailingCityMonthlySales, @DistRacSalesByYear, 
                         @EwwHasWorkedWithCMI, @EwwYearOfOperationAndVolume, @EwwDealerInfo, @EwwNameOfFirms, @EwwTotalInvestment, 
                         @AodaExpectedTo1, @AodaExpectedTo2, @AodaExpectedTo3, @AodaHasOffice, @AodaHasGodown, @AodaHasManpower, 
                         @AodaHasServiceCenter, @AodaHasDeliveryVan, @AodaHasSalesman, @AodaHasComputer, @AodaHasOthers, 
                         @ApMarketReputationLevel1, @ApMarketReputationLevel2, @ApMarketReputationLevel3, @ApDisplayQuantityLevel1, 
                         @ApDisplayQuantityLevel2, @ApDisplayQuantityLevel3, @ApDistRetStrengthLevel1, @ApDistRetStrengthLevel2, 
                         @ApDistRetStrengthLevel3, @ApFinancialStrengthLevel1, @ApFinancialStrengthLevel2, @ApFinancialStrengthLevel3, 
                         @IsAgreedWithTNC, @StoreUid,@AooaType, @AooaCategory, @AooaAspToClose, @AooaCode, @AooaProduct, 
                         @AooaEval1, @AooaEval2, @AooaEval3, @AooaEval4, @AooaEval5, 
                         @AooaEval6, @AooaEval7, @AooaEval8, @AooaEval9, @AooaEval10, 
                         @AooaEval11, @AooaEval12, @AooaEval13, @AooaEval14, @AooaEval15, 
                         @AooaEval16, @AooaEval17, @AooaEval18, @AooaEval19, 
                         @AooaApTechnicalCompLevel1, @AooaApTechnicalCompLevel2, @AooaApTechnicalCompAverage, 
                         @AoaaApManpowerLevel1, @AoaaApManpowerLevel2, @AoaaApManpowerAverage, 
                         @AoaaApWorkshopLevel1, @AoaaApWorkshopLevel2, @AoaaApWorkshopAverage, 
                         @AoaaApToolsLevel1, @AoaaApToolsLevel2, @AoaaApToolsAverage, 
                         @AoaaApComputerLevel1, @AoaaApComputerLevel2, @AoaaApComputerAverage, 
                         @AoaaApFinancialStrengthLevel1, @AoaaApFinancialStrengthLevel2, @AoaaApFinancialStrengthAverage, 
                         @AoaaCcFcContactPerson, @AoaaCcFcSentTime, @AoaaCcFcReplyReceivedBy, 
                         @AoaaCcSecWayOfCommu, @AoaaCcSecContactPerson, @AoaaCcSecSentTime, @AoaaCcSecReplyReceivedBy, 
                         @ScAddress, @ScAddressType, @ScIsServiceCenterDifferenceFromPrinciplePlace, 
                         @ScArea, @ScCurrentBrandHandled, @ScExpInYear, @ScNoOfTechnician, @ScTechnicianData, 
                         @ScNoOfSupervisor, @ScSupervisorData, @ScLicenseNo,@IsTcAgreed, @PartnerDetails";
                if (!string.IsNullOrEmpty(model.SelfRegistrationUID))
                {
                    sql += @",@SelfRegistrationUID";
                }
                sql += ")";

                //var parameters = new Dictionary<string, object>
                //                {
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
        
        public async Task<int> UpdateStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)

        {
            try
            {
                model.ModifiedBy = "ADMIN";
                model.ModifiedTime = DateTime.Now ;
                model.ServerModifiedTime = DateTime.Now;
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
    partner_details = @PartnerDetails,
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
aooa_type = @AooaType, 
    aooa_category = @AooaCategory, 
    aooa_asp_to_close = @AooaAspToClose, 
    aooa_code = @AooaCode, 
    aooa_product = @AooaProduct, 
    aooa_eval_1 = @AooaEval1, 
    aooa_eval_2 = @AooaEval2, 
    aooa_eval_3 = @AooaEval3, 
    aooa_eval_4 = @AooaEval4, 
    aooa_eval_5 = @AooaEval5, 
    aooa_eval_6 = @AooaEval6, 
    aooa_eval_7 = @AooaEval7, 
    aooa_eval_8 = @AooaEval8, 
    aooa_eval_9 = @AooaEval9, 
    aooa_eval_10 = @AooaEval10, 
    aooa_eval_11 = @AooaEval11, 
    aooa_eval_12 = @AooaEval12, 
    aooa_eval_13 = @AooaEval13, 
    aooa_eval_14 = @AooaEval14, 
    aooa_eval_15 = @AooaEval15, 
    aooa_eval_16 = @AooaEval16, 
    aooa_eval_17 = @AooaEval17, 
    aooa_eval_18 = @AooaEval18, 
    aooa_eval_19 = @AooaEval19, 
    aooa_ap_technical_comp_level1 = @AooaApTechnicalCompLevel1, 
    aooa_ap_technical_comp_level2 = @AooaApTechnicalCompLevel2, 
    aooa_ap_technical_comp_average = @AooaApTechnicalCompAverage, 
    aoaa_ap_manpower_level1 = @AoaaApManpowerLevel1, 
    aoaa_ap_manpower_level2 = @AoaaApManpowerLevel2, 
    aoaa_ap_manpower_average = @AoaaApManpowerAverage, 
    aoaa_ap_workshop_level1 = @AoaaApWorkshopLevel1, 
    aoaa_ap_workshop_level2 = @AoaaApWorkshopLevel2, 
    aoaa_ap_workshop_average = @AoaaApWorkshopAverage, 
    aoaa_ap_tools_level1 = @AoaaApToolsLevel1, 
    aoaa_ap_tools_level2 = @AoaaApToolsLevel2, 
    aoaa_ap_tools_average = @AoaaApToolsAverage, 
    aoaa_ap_computer_level1 = @AoaaApComputerLevel1, 
    aoaa_ap_computer_level2 = @AoaaApComputerLevel2, 
    aoaa_ap_computer_average = @AoaaApComputerAverage, 
    aoaa_ap_financial_strength_level1 = @AoaaApFinancialStrengthLevel1, 
    aoaa_ap_financial_strength_level2 = @AoaaApFinancialStrengthLevel2, 
    aoaa_ap_financial_strength_average = @AoaaApFinancialStrengthAverage, 
    aoaa_cc_fc_contact_person = @AoaaCcFcContactPerson, 
    aoaa_cc_fc_sent_time = @AoaaCcFcSentTime, 
    aoaa_cc_fc_reply_received_by = @AoaaCcFcReplyReceivedBy, 
    aoaa_cc_sec_way_of_commu = @AoaaCcSecWayOfCommu, 
    aoaa_cc_sec_contact_person = @AoaaCcSecContactPerson, 
    aoaa_cc_sec_sent_time = @AoaaCcSecSentTime, 
    aoaa_cc_sec_reply_received_by = @AoaaCcSecReplyReceivedBy, 
    sc_address = @ScAddress, 
    sc_address_type = @ScAddressType, 
    sc_is_service_center_difference_from_principle_place = @ScIsServiceCenterDifferenceFromPrinciplePlace, 
    sc_area = @ScArea, 
    sc_current_brand_handled = @ScCurrentBrandHandled, 
    sc_exp_in_year = @ScExpInYear, 
    sc_no_of_technician = @ScNoOfTechnician, 
    sc_technician_data = @ScTechnicianData, 
    sc_no_of_supervisor = @ScNoOfSupervisor, 
    sc_supervisor_data = @ScSupervisorData, 
    sc_license_no = @ScLicenseNo,
    is_tc_agreed=@IsTcAgreed
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
        public async Task<int> UpdateShowroomDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)

        {
            try
            {

                                    var sql = @"UPDATE store_additional_info_cmi   
                                                    SET 
                                                modified_by = @ModifiedBy, 
                                                modified_time = @ModifiedTime,  
                                                server_modified_time = @ServerModifiedTime, 
                                                showroom_details = @ShowroomDetails
                                                WHERE uid = @UID";

                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateKartaInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)

        {
            try
            {

                                    var sql = @"UPDATE store_additional_info_cmi   
                                                    SET 
                                                modified_by = @ModifiedBy, 
                                                modified_time = @ModifiedTime,  
                                                server_modified_time = @ServerModifiedTime, 
                                                partner_details = @PartnerDetails
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
                                                no_of_manager = @NoOfManager, 
                                                no_of_sales_team = @NoOfSalesTeam, 
                                                no_of_commercial = @NoOfCommercial, 
                                                no_of_service = @NoOfService, 
                                                no_of_others = @NoOfOthers, 
                                                total_emp = @TotalEmp
                                                    WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
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
                            dist_no_of_sub_dealers = @DistNoOfSubDealers, 
                            dist_retailing_city_monthly_sales = @DistRetailingCityMonthlySales, 
                            dist_rac_sales_by_year = @DistRacSalesByYear
                            WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public async Task<int> UpdateBankDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)

        //{
        //    try
        //    {

        //        var sql = @"UPDATE store_additional_info_cmi   
        //                                                SET 
        //                    modified_by = @ModifiedBy, 
        //                    modified_time = @ModifiedTime,  
        //                    server_modified_time = @ServerModifiedTime, 
        //                  dist_no_of_sub_dealers = @DistNoOfSubDealers, 
        //                    dist_retailing_city_monthly_sales = @DistRetailingCityMonthlySales, 
        //                    dist_rac_sales_by_year = @DistRacSalesByYear
        //                        WHERE uid = @UID";
        //        return await ExecuteNonQueryAsync(sql, model);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        public async Task<int> UpdateBusinessDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)

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
                           dist_monthly_sales = @DistMonthlySales
                            WHERE uid = @UID";

                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateAreaOfDistAgreedInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            try
            {
                                var sql = @"UPDATE store_additional_info_cmi   
                                                                    SET 
                                        modified_by = @ModifiedBy, 
                                        modified_time = @ModifiedTime,  
                                        server_modified_time = @ServerModifiedTime, 
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
                                        is_agreed_with_tnc = @IsAgreedWithTNC
                                        WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateBankersDetailsInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            try
            {
                                    var sql = @"UPDATE store_additional_info_cmi   
                                                    SET 
                                            modified_by = @ModifiedBy, 
                                            modified_time = @ModifiedTime,  
                                            server_modified_time = @ServerModifiedTime, 
                                            bank_details = @BankDetails, 
                                            signatory_details = @SignatoryDetails 
                                            WHERE uid = @UID";

                var sql1 = @"UPDATE store_additional_info_cmi   
                                                    SET 
                                            modified_by = @ModifiedBy, 
                                            modified_time = @ModifiedTime,  
                                            server_modified_time = @ServerModifiedTime, 
                                            signatory_details = @SignatoryDetails 
                                            WHERE uid = @UID";

                var result  = string.IsNullOrEmpty(model.BankDetails) ?  await ExecuteNonQueryAsync(sql1, model) :  await ExecuteNonQueryAsync(sql, model);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateEarlierWorkWithCMIInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            try
            {
                                var sql = @"UPDATE store_additional_info_cmi   
                                                SET 
                                        modified_by = @ModifiedBy, 
                                        modified_time = @ModifiedTime,  
                                        server_modified_time = @ServerModifiedTime, 
                                        eww_has_worked_with_cmi = @EwwHasWorkedWithCMI, 
                                        eww_year_of_operation_and_volume = @EwwYearOfOperationAndVolume, 
                                        eww_dealer_info = @EwwDealerInfo, 
                                        eww_name_of_firms = @EwwNameOfFirms, 
                                        eww_total_investment = @EwwTotalInvestment
                                        WHERE uid = @UID";

                return await ExecuteNonQueryAsync(sql, model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateTermAndCondInStoreAdditionalInfoCMI(Model.Interfaces.IStoreAdditionalInfoCMI model)
        {
            try
            {
                                        var sql = @"UPDATE store_additional_info_cmi   
                                                                        SET 
                                            modified_by = @ModifiedBy, 
                                            modified_time = @ModifiedTime,  
                                            server_modified_time = @ServerModifiedTime, 
                                            is_tc_agreed = @IsTcAgreed
                                            WHERE uid = @UID";

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
                               sc_address=@ScAddress,
                                sc_address_type =@ScAddressType,
                                sc_is_service_center_difference_from_principle_place=@ScIsServiceCenterDifferenceFromPrinciplePlace,

                                sc_area=@ScArea,
                                sc_current_brand_handled=@ScCurrentBrandHandled,
                                sc_exp_in_year=@ScExpInYear,
                                sc_no_of_technician=@ScNoOfTechnician,
                                sc_technician_data=@ScTechnicianData,
                                sc_no_of_supervisor=@ScNoOfSupervisor,
                                sc_supervisor_data=@ScSupervisorData,
                                sc_license_no=@ScLicenseNo
 
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
                                aooa_type = @AooaType,
                                aooa_category = @AooaCategory,
                                aooa_asp_to_close = @AooaAspToClose,
                                aooa_code = @AooaCode,
                                aooa_product = @AooaProduct,
                                aooa_eval_1 = @AooaEval1,
                                aooa_eval_2 = @AooaEval2,
                                aooa_eval_3 = @AooaEval3,
                                aooa_eval_4 = @AooaEval4,
                                aooa_eval_5 = @AooaEval5,
                                aooa_eval_6 = @AooaEval6,
                                aooa_eval_7 = @AooaEval7,
                                aooa_eval_8 = @AooaEval8,
                                aooa_eval_9 = @AooaEval9,
                                aooa_eval_10 = @AooaEval10,
                                aooa_eval_11 = @AooaEval11,
                                aooa_eval_12 = @AooaEval12,
                                aooa_eval_13 = @AooaEval13,
                                aooa_eval_14 = @AooaEval14,
                                aooa_eval_15 = @AooaEval15,
                                aooa_eval_16 = @AooaEval16,
                                aooa_eval_17 = @AooaEval17,
                                aooa_eval_18 = @AooaEval18,
                                aooa_eval_19 = @AooaEval19,
                                aooa_ap_technical_comp_level1 = @AooaApTechnicalCompLevel1,
                                aooa_ap_technical_comp_level2 = @AooaApTechnicalCompLevel2,
                                aooa_ap_technical_comp_average = @AooaApTechnicalCompAverage,
                                aoaa_ap_manpower_level1 = @AoaaApManpowerLevel1,
                                aoaa_ap_manpower_level2 = @AoaaApManpowerLevel2,
                                aoaa_ap_manpower_average = @AoaaApManpowerAverage,
                                aoaa_ap_workshop_level1 = @AoaaApWorkshopLevel1,
                                aoaa_ap_workshop_level2 = @AoaaApWorkshopLevel2,
                                aoaa_ap_workshop_average = @AoaaApWorkshopAverage,
                                aoaa_ap_tools_level1 = @AoaaApToolsLevel1,
                                aoaa_ap_tools_level2 = @AoaaApToolsLevel2,
                                aoaa_ap_tools_average = @AoaaApToolsAverage,
                                aoaa_ap_computer_level1 = @AoaaApComputerLevel1,
                                aoaa_ap_computer_level2 = @AoaaApComputerLevel2,
                                aoaa_ap_computer_average = @AoaaApComputerAverage,
                                aoaa_ap_financial_strength_level1 = @AoaaApFinancialStrengthLevel1,
                                aoaa_ap_financial_strength_level2 = @AoaaApFinancialStrengthLevel2,
                                aoaa_ap_financial_strength_average = @AoaaApFinancialStrengthAverage,
                                aoaa_cc_fc_contact_person = @AoaaCcFcContactPerson,
                                aoaa_cc_fc_sent_time = @AoaaCcFcSentTime,
                                aoaa_cc_fc_reply_received_by = @AoaaCcFcReplyReceivedBy,
                                aoaa_cc_sec_way_of_commu = @AoaaCcSecWayOfCommu,
                                aoaa_cc_sec_contact_person = @AoaaCcSecContactPerson,
                                aoaa_cc_sec_sent_time = @AoaaCcSecSentTime,
                                aoaa_cc_sec_reply_received_by = @AoaaCcSecReplyReceivedBy
                                WHERE uid = @UID;";

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
                                showroom_details AS ShowroomDetails,
                                bank_details AS BankDetails,
                                signatory_details AS SignatoryDetails,
                                brand_dealing_in_details AS BrandDealingInDetails,
                                product_dealing_in AS ProductDealingIn,
                                area_of_operation AS AreaOfOperation,
                                dist_products AS DistProducts,
                                dist_area_of_operation AS DistAreaOfOperation,
                                dist_brands AS DistBrands,
                                dist_monthly_sales AS DistMonthlySales,
                                dist_no_of_sub_dealers AS DistNoOfSubDealers,
                                dist_retailing_city_monthly_sales AS DistRetailingCityMonthlySales,
                                dist_rac_sales_by_year AS DistRacSalesByYear,
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
                                aooa_type AS AooaType,
                                aooa_category AS AooaCategory,
                                aooa_asp_to_close AS AooaAspToClose,
                                aooa_code AS AooaCode,
                                aooa_product AS AooaProduct,
                                aooa_eval_1 AS AooaEval1,
                                aooa_eval_2 AS AooaEval2,
                                aooa_eval_3 AS AooaEval3,
                                aooa_eval_4 AS AooaEval4,
                                aooa_eval_5 AS AooaEval5,
                                aooa_eval_6 AS AooaEval6,
                                aooa_eval_7 AS AooaEval7,
                                aooa_eval_8 AS AooaEval8,
                                aooa_eval_9 AS AooaEval9,
                                aooa_eval_10 AS AooaEval10,
                                aooa_eval_11 AS AooaEval11,
                                aooa_eval_12 AS AooaEval12,
                                aooa_eval_13 AS AooaEval13,
                                aooa_eval_14 AS AooaEval14,
                                aooa_eval_15 AS AooaEval15,
                                aooa_eval_16 AS AooaEval16,
                                aooa_eval_17 AS AooaEval17,
                                aooa_eval_18 AS AooaEval18,
                                aooa_eval_19 AS AooaEval19,
                                aooa_ap_technical_comp_level1 AS AooaApTechnicalCompLevel1,
                                aooa_ap_technical_comp_level2 AS AooaApTechnicalCompLevel2,
                                aooa_ap_technical_comp_average AS AooaApTechnicalCompAverage,
                                aoaa_ap_manpower_level1 AS AoaaApManpowerLevel1,
                                aoaa_ap_manpower_level2 AS AoaaApManpowerLevel2,
                                aoaa_ap_manpower_average AS AoaaApManpowerAverage,
                                aoaa_ap_workshop_level1 AS AoaaApWorkshopLevel1,
                                aoaa_ap_workshop_level2 AS AoaaApWorkshopLevel2,
                                aoaa_ap_workshop_average AS AoaaApWorkshopAverage,
                                aoaa_ap_tools_level1 AS AoaaApToolsLevel1,
                                aoaa_ap_tools_level2 AS AoaaApToolsLevel2,
                                aoaa_ap_tools_average AS AoaaApToolsAverage,
                                aoaa_ap_computer_level1 AS AoaaApComputerLevel1,
                                aoaa_ap_computer_level2 AS AoaaApComputerLevel2,
                                aoaa_ap_computer_average AS AoaaApComputerAverage,
                                aoaa_ap_financial_strength_level1 AS AoaaApFinancialStrengthLevel1,
                                aoaa_ap_financial_strength_level2 AS AoaaApFinancialStrengthLevel2,
                                aoaa_ap_financial_strength_average AS AoaaApFinancialStrengthAverage,
                                aoaa_cc_fc_contact_person AS AoaaCcFcContactPerson,
                                aoaa_cc_fc_sent_time AS AoaaCcFcSentTime,
                                aoaa_cc_fc_reply_received_by AS AoaaCcFcReplyReceivedBy,
                                aoaa_cc_sec_way_of_commu AS AoaaCcSecWayOfCommu,
                                aoaa_cc_sec_contact_person AS AoaaCcSecContactPerson,
                                aoaa_cc_sec_sent_time AS AoaaCcSecSentTime,
                                aoaa_cc_sec_reply_received_by AS AoaaCcSecReplyReceivedBy,
                                sc_address AS ScAddress,
                                sc_address_type AS ScAddressType,
                                sc_is_service_center_difference_from_principle_place AS ScIsServiceCenterDifferenceFromPrinciplePlace,
                                sc_area AS ScArea,
                                sc_current_brand_handled AS ScCurrentBrandHandled,
                                sc_exp_in_year AS ScExpInYear,
                                sc_no_of_technician AS ScNoOfTechnician,
                                sc_technician_data AS ScTechnicianData,
                                sc_no_of_supervisor AS ScNoOfSupervisor,
                                sc_supervisor_data AS ScSupervisorData,
                                sc_license_no AS ScLicenseNo,
                                is_tc_agreed As IsTcAgreed
                            FROM store_additional_info_cmi where uid=@UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreAdditionalInfoCMI>().GetType();
            return  await ExecuteSingleAsync<Model.Interfaces.IStoreAdditionalInfoCMI>(sql, parameters, type);
             
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
     

        
       
    }
}

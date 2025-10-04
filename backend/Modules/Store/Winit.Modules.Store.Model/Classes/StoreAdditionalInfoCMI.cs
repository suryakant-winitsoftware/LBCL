using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    //public class StoreAdditionalInfoCMI :BaseModel, IStoreAdditionalInfoCMI
    //{
    //    public DateTime? DateOfFoundation { get; set; }
    //    public DateTime? DateOfBirth { get; set; }
    //    public DateTime? WeddingDate { get; set; }
    //    public int? NoOfManager { get; set; }
    //    public int? NoOfSalesTeam { get; set; }
    //    public int? NoOfCommercial { get; set; }
    //    public int? NoOfService { get; set; }
    //    public int? NoOfOthers { get; set; }
    //    public int? NoOfStores { get; set; }

    //    public int? TotalEmp { get; set; }
    //    public string ShowroomDetails { get; set; }
    //    public string BankDetails { get; set; }
    //    public string SignatoryDetails { get; set; }
    //    public string BrandDealingInDetails { get; set; }
    //    public string ProductDealingIn { get; set; }
    //    public string AreaOfOperation { get; set; }
    //    public string DistProducts { get; set; }
    //    public string DistAreaOfOperation { get; set; }
    //    public string DistBrands { get; set; }
    //    public string DistMonthlySales { get; set; }
    //    public int? DistNoOfSubDealers { get; set; }
    //    public string DistRetailingCityMonthlySales { get; set; }
    //    public string DistRacSalesByYear { get; set; }
    //    public bool? EwwHasWorkedWithCMI { get; set; } = true;
    //    public string EwwYearOfOperationAndVolume { get; set; }
    //    public string EwwDealerInfo { get; set; }
    //    public string EwwNameOfFirms { get; set; }
    //    public decimal? EwwTotalInvestment { get; set; }
    //    public decimal? AodaExpectedTo1 { get; set; } 
    //    public decimal? AodaExpectedTo2 { get; set; }
    //    public decimal? AodaExpectedTo3 { get; set; }
    //    public bool? AodaHasOffice { get; set; }
    //    public bool? AodaHasGodown { get; set; }
    //    public bool? AodaHasManpower { get; set; }
    //    public bool? AodaHasServiceCenter { get; set; }
    //    public bool? AodaHasDeliveryVan { get; set; }
    //    public bool? AodaHasSalesman { get; set; }
    //    public bool? AodaHasComputer { get; set; }
    //    public bool? AodaHasOthers { get; set; }
    //    public int? ApMarketReputationLevel1 { get; set; } = 0;
    //    public int? ApMarketReputationLevel2 { get; set; } = 0;
    //    public decimal? ApMarketReputationLevel3 { get; set; } = 0;
    //    public int? ApDisplayQuantityLevel1 { get; set; } = 0;
    //    public int? ApDisplayQuantityLevel2 { get; set; } = 0;
    //    public decimal? ApDisplayQuantityLevel3 { get; set; } = 0;
    //    public int? ApDistRetStrengthLevel1 { get; set; } = 0;
    //    public int? ApDistRetStrengthLevel2 { get; set; } = 0;
    //    public decimal? ApDistRetStrengthLevel3 { get; set; } = 0;
    //    public int? ApFinancialStrengthLevel1 { get; set; } = 0;
    //    public int? ApFinancialStrengthLevel2 { get; set; } = 0;
    //    public decimal? ApFinancialStrengthLevel3 { get; set; } = 0;
    //    public bool IsAgreedWithTNC { get; set; }
    //    public string StoreUid { get; set; }
    //    public int BankAccountNo { get; set; }
    //    public int SN { get; set; }

    //    public string Action { get; set; }

    //    public string SectionName { get; set; }
    //    public string Products { get; set; }
    //    public string Brands { get; set; }
    //    public string MonthlySales { get; set; }
    //    public int? NoOfTotalSubDealers { get; set; }
    //    public bool? HasEarlierWorkedWIthCMI { get; set; }
    //    public string YearOfOperationAndBusinessVolume { get; set; }
    //    public string DealerOrDistributor { get; set; }
    //    public string NameOfTheFirm { get; set; }
    //    public decimal? TotalInvestment { get; set; }
    //    public decimal? ExpectedTO1 { get; set; }
    //    public decimal? ExpectedTO2 { get; set; }
    //    public decimal? ExpectedTO3 { get; set; }
    //    public bool? OfficeForCMI { get; set; }
    //    public bool? GoDownForCMI { get; set; }
    //    public bool? ManPowerForCMI { get; set; }
    //    public bool? ServiceCenterForCMI { get; set; }
    //    public bool? DeliveryVanForCMI { get; set; }
    //    public bool? SalesManPowerForCMI { get; set; }
    //    public bool? ComputerForCMI { get; set; }
    //    public bool? OthersForCMI { get; set; }
    //    public int? MarketReputationAPLevel1 { get; set; } = 0;
    //    public int? MarketReputationAPLevel2 { get; set; } = 0;
    //    public decimal? MarketReputationAPLevel3 { get; set; } = 0;
    //    public int? DisplayQualityAPLevel1 { get; set; } = 0;
    //    public int? DisplayQualityAPLevel2 { get; set; } = 0;
    //    public decimal? DisplayQualityAPLevel3 { get; set; } = 0;
    //    public int? DistributionRetailStrengthAPLevel1 { get; set; } = 0;
    //    public int? DistributionRetailStrengthAPLevel2 { get; set; } = 0;
    //    public decimal? DistributionRetailStrengthAPLevel3 { get; set; } = 0;
    //    public int? FinancialStrengthAPLevel1 { get; set; } = 0;
    //    public int? FinancialStrengthAPLevel2 { get; set; } = 0;
    //    public decimal? FinancialStrengthAPLevel3 { get; set; } = 0;
    //    //Selva
    //    public string AooaType { get; set; }
    //    public string AooaCategory { get; set; }
    //    public string AooaAspToClose { get; set; }
    //    public string AooaCode { get; set; }
    //    public string AooaProduct { get; set; }
    //    public bool AooaEval1 { get; set; }
    //    public bool AooaEval2 { get; set; }
    //    public bool AooaEval3 { get; set; }
    //    public bool AooaEval4 { get; set; }
    //    public bool AooaEval5 { get; set; }
    //    public bool AooaEval6 { get; set; }
    //    public bool AooaEval7 { get; set; }
    //    public bool AooaEval8 { get; set; }
    //    public bool AooaEval9 { get; set; }
    //    public bool AooaEval10 { get; set; }
    //    public bool AooaEval11 { get; set; }
    //    public bool AooaEval12 { get; set; }
    //    public bool AooaEval13 { get; set; }
    //    public bool AooaEval14 { get; set; }
    //    public bool AooaEval15 { get; set; }
    //    public bool AooaEval16 { get; set; }
    //    public bool AooaEval17 { get; set; }
    //    public bool AooaEval18 { get; set; }
    //    public bool AooaEval19 { get; set; }
    //    public int AooaApTechnicalCompLevel1 { get; set; } = 0;
    //    public int AooaApTechnicalCompLevel2 { get; set; } = 0;
    //    public decimal AooaApTechnicalCompAverage { get; set; } = 0;
    //    public int AoaaApManpowerLevel1 { get; set; } = 0;
    //    public int AoaaApManpowerLevel2 { get; set; } = 0;
    //    public decimal AoaaApManpowerAverage { get; set; } = 0;
    //    public int AoaaApWorkshopLevel1 { get; set; } = 0;
    //    public int AoaaApWorkshopLevel2 { get; set; } = 0;
    //    public decimal AoaaApWorkshopAverage { get; set; } = 0;
    //    public int AoaaApToolsLevel1 { get; set; } = 0;
    //    public int AoaaApToolsLevel2 { get; set; } = 0;
    //    public decimal AoaaApToolsAverage { get; set; } = 0;
    //    public int AoaaApComputerLevel1 { get; set; } = 0;
    //    public int AoaaApComputerLevel2 { get; set; } = 0;
    //    public decimal AoaaApComputerAverage { get; set; } = 0;
    //    public int AoaaApFinancialStrengthLevel1 { get; set; } = 0;
    //    public int AoaaApFinancialStrengthLevel2 { get; set; } = 0;
    //    public decimal AoaaApFinancialStrengthAverage { get; set; } = 0;
    //    public string AoaaCcFcContactPerson { get; set; }
    //    public DateTime? AoaaCcFcSentTime { get; set; }
    //    public string AoaaCcFcReplyReceivedBy { get; set; }
    //    public string AoaaCcSecWayOfCommu { get; set; }
    //    public string AoaaCcSecContactPerson { get; set; }
    //    public DateTime? AoaaCcSecSentTime { get; set; }
    //    public string AoaaCcSecReplyReceivedBy { get; set; }
    //    public string ScAddress { get; set; }
    //    public string ScAddressType { get; set; } 
    //    public bool ScIsServiceCenterDifferenceFromPrinciplePlace { get; set; } 
    //    public decimal? ScArea { get; set; }
    //    public string ScCurrentBrandHandled { get; set; }
    //    public decimal? ScExpInYear { get; set; } 
    //    public int ScNoOfTechnician { get; set; }
    //    public string ScTechnicianData { get; set; } // Assuming it's a string representation of JSON
    //    public int ScNoOfSupervisor { get; set; }
    //    public string ScSupervisorData { get; set; } // Assuming it's a string representation of JSON
    //    public string ScLicenseNo { get; set; }
    //    public bool IsTcAgreed { get; set; }
    //    public string SelfRegistrationUID { get; set; }


    //}
    public class StoreAdditionalInfoCMI : BaseModel, IStoreAdditionalInfoCMI
    {
        [Column("aoaa_ap_tools_level1")]
        public int AoaaApToolsLevel1 { get; set; } = 0;

        [Column("aoaa_ap_tools_level2")]
        public int AoaaApToolsLevel2 { get; set; } = 0;

        [Column("aoaa_ap_tools_average")]
        public decimal AoaaApToolsAverage { get; set; } = 0;

        [Column("aoaa_ap_computer_level1")]
        public int AoaaApComputerLevel1 { get; set; } = 0;

        [Column("aoaa_ap_computer_level2")]
        public int AoaaApComputerLevel2 { get; set; } = 0;

        [Column("aoaa_ap_computer_average")]
        public decimal AoaaApComputerAverage { get; set; } = 0;

        [Column("aoaa_ap_financial_strength_level1")]
        public int AoaaApFinancialStrengthLevel1 { get; set; } = 0;

        [Column("aoaa_ap_financial_strength_level2")]
        public int AoaaApFinancialStrengthLevel2 { get; set; } = 0;

        [Column("aoaa_ap_financial_strength_average")]
        public decimal AoaaApFinancialStrengthAverage { get; set; } = 0;

        [Column("aoaa_cc_fc_sent_time")]
        public DateTime? AoaaCcFcSentTime { get; set; }

        [Column("aoaa_cc_sec_contact_person")]
        public string AoaaCcSecContactPerson { get; set; }

        [Column("aoaa_cc_sec_sent_time")]
        public DateTime? AoaaCcSecSentTime { get; set; }

        [Column("aoaa_cc_sec_reply_received_by")]
        public string AoaaCcSecReplyReceivedBy { get; set; }

        [Column("is_tc_agreed")]
        public bool IsTcAgreed { get; set; }

        [Column("sc_license_no")]
        public string ScLicenseNo { get; set; }

        [Column("sc_no_of_supervisor")]
        public int ScNoOfSupervisor { get; set; }

        [Column("sc_no_of_technician")]
        public int ScNoOfTechnician { get; set; }

        [Column("sc_area")]
        public decimal? ScArea { get; set; }

        [Column("sc_is_service_center_difference_from_principle_place")]
        public bool ScIsServiceCenterDifferenceFromPrinciplePlace { get; set; }

        [Column("sc_address_type")]
        public string ScAddressType { get; set; }

        [Column("aooa_type")]
        public string AooaType { get; set; }

        [Column("aooa_category")]
        public string AooaCategory { get; set; }

        [Column("aooa_asp_to_close")]
        public string AooaAspToClose { get; set; }

        [Column("aoaa_cc_sec_way_of_commu")]
        public string AoaaCcSecWayOfCommu { get; set; }

        [Column("aoaa_cc_fc_reply_received_by")]
        public string AoaaCcFcReplyReceivedBy { get; set; }

        [Column("aoaa_cc_fc_contact_person")]
        public string AoaaCcFcContactPerson { get; set; }

        [Column("aooa_code")]
        public string AooaCode { get; set; }

        [Column("sc_current_brand_handled")]
        public string ScCurrentBrandHandled { get; set; }

        [Column("sc_exp_in_year")]
        public decimal? ScExpInYear { get; set; }


        public string SectionName { get; set; }
        [Column("sc_technician_data")]
        public string ScTechnicianData { get; set; }
        public string Action { get; set; }
        [Column("sc_address")]
        public string ScAddress { get; set; }
        [Column("sc_supervisor_data")]
        public string ScSupervisorData { get; set; }
        [Column("store_uid")]
        public string StoreUid { get; set; }

        [Column("dof")]
        public DateTime? DateOfFoundation { get; set; }

        [Column("dob")]
        public DateTime? DateOfBirth { get; set; }

        [Column("wedding_date")]
        public DateTime? WeddingDate { get; set; }

        [Column("no_of_manager")]
        public int? NoOfManager { get; set; }

        [Column("no_of_sales_team")]
        public int? NoOfSalesTeam { get; set; }

        [Column("no_of_commercial")]
        public int? NoOfCommercial { get; set; }

        [Column("no_of_service")]
        public int? NoOfService { get; set; }

        [Column("no_of_others")]
        public int? NoOfOthers { get; set; }

        [Column("total_emp")]
        public int? TotalEmp { get; set; }

        [Column("showroom_details")]
        public string ShowroomDetails { get; set; }

        [Column("bank_details")]
        public string BankDetails { get; set; }

        [Column("signatory_details")]
        public string SignatoryDetails { get; set; }

        [Column("brand_dealing_in_details")]
        public string BrandDealingInDetails { get; set; }
        [Column("partner_details")]
        public string PartnerDetails { get; set; }

        [Column("product_dealing_in")]
        public string ProductDealingIn { get; set; }

        [Column("area_of_operation")]
        public string AreaOfOperation { get; set; }

        [Column("dist_products")]
        public string DistProducts { get; set; }

        [Column("dist_area_of_operation")]
        public string DistAreaOfOperation { get; set; }

        [Column("dist_brands")]
        public string DistBrands { get; set; }

        [Column("dist_monthly_sales")]
        public string DistMonthlySales { get; set; }

        [Column("dist_no_of_sub_dealers")]
        public int? DistNoOfSubDealers { get; set; }

        [Column("dist_retailing_city_monthly_sales")]
        public string DistRetailingCityMonthlySales { get; set; }

        [Column("dist_rac_sales_by_year")]
        public string DistRacSalesByYear { get; set; }

        [Column("eww_has_worked_with_cmi")]
        public bool? EwwHasWorkedWithCMI { get; set; }

        [Column("eww_year_of_operation_and_volume")]
        public string EwwYearOfOperationAndVolume { get; set; }

        [Column("eww_dealer_info")]
        public string EwwDealerInfo { get; set; }

        [Column("eww_name_of_firms")]
        public string EwwNameOfFirms { get; set; }

        [Column("eww_total_investment")]
        public decimal? EwwTotalInvestment { get; set; }

        [Column("aoda_expected_to_1")]
        public decimal? AodaExpectedTo1 { get; set; }

        [Column("aoda_expected_to_2")]
        public decimal? AodaExpectedTo2 { get; set; }

        [Column("aoda_expected_to_3")]
        public decimal? AodaExpectedTo3 { get; set; }

        [Column("aoda_has_office")]
        public bool? AodaHasOffice { get; set; }

        [Column("aoda_has_godown")]
        public bool? AodaHasGodown { get; set; }

        [Column("aoda_has_manpower")]
        public bool? AodaHasManpower { get; set; }

        [Column("aoda_has_service_center")]
        public bool? AodaHasServiceCenter { get; set; }

        [Column("aoda_has_delivery_van")]
        public bool? AodaHasDeliveryVan { get; set; }

        [Column("aoda_has_salesman")]
        public bool? AodaHasSalesman { get; set; }

        [Column("aoda_has_computer")]
        public bool? AodaHasComputer { get; set; }

        [Column("aoda_has_others")]
        public bool? AodaHasOthers { get; set; }

        [Column("ap_market_reputation_level1")]
        public int? ApMarketReputationLevel1 { get; set; }

        [Column("ap_market_reputation_level2")]
        public int? ApMarketReputationLevel2 { get; set; }

        [Column("ap_market_reputation_level3")]
        public decimal? ApMarketReputationLevel3 { get; set; }

        [Column("ap_display_quantity_level1")]
        public int? ApDisplayQuantityLevel1 { get; set; }

        [Column("ap_display_quantity_level2")]
        public int? ApDisplayQuantityLevel2 { get; set; }

        [Column("ap_display_quantity_level3")]
        public decimal? ApDisplayQuantityLevel3 { get; set; }

        [Column("ap_dist_ret_strength_level1")]
        public int? ApDistRetStrengthLevel1 { get; set; }

        [Column("ap_dist_ret_strength_level2")]
        public int? ApDistRetStrengthLevel2 { get; set; }

        [Column("ap_dist_ret_strength_level3")]
        public decimal? ApDistRetStrengthLevel3 { get; set; }

        [Column("ap_financial_strength_level1")]
        public int? ApFinancialStrengthLevel1 { get; set; }

        [Column("ap_financial_strength_level2")]
        public int? ApFinancialStrengthLevel2 { get; set; }

        [Column("ap_financial_strength_level3")]
        public decimal? ApFinancialStrengthLevel3 { get; set; }

        [Column("is_agreed_with_tnc")]
        public bool IsAgreedWithTNC { get; set; }
        [Column("self_registration_uid")]
        public string SelfRegistrationUID { get; set; }

        [Column("aoaa_ap_manpower_level1")]
        public int AoaaApManpowerLevel1 { get; set; } = 0;

        [Column("aoaa_ap_manpower_level2")]
        public int AoaaApManpowerLevel2 { get; set; } = 0;

        [Column("aoaa_ap_manpower_average")]
        public decimal AoaaApManpowerAverage { get; set; } = 0;

        [Column("aoaa_ap_workshop_level1")]
        public int AoaaApWorkshopLevel1 { get; set; } = 0;

        [Column("aoaa_ap_workshop_level2")]
        public int AoaaApWorkshopLevel2 { get; set; } = 0;

        [Column("aoaa_ap_workshop_average")]
        public decimal AoaaApWorkshopAverage { get; set; } = 0;

        [Column("aooa_ap_technical_comp_level1")]
        public int AooaApTechnicalCompLevel1 { get; set; } = 0;

        [Column("aooa_ap_technical_comp_level2")]
        public int AooaApTechnicalCompLevel2 { get; set; } = 0;

        [Column("aooa_ap_technical_comp_average")]
        public decimal AooaApTechnicalCompAverage { get; set; } = 0;

        [Column("aooa_eval1")]
        public bool AooaEval1 { get; set; }

        [Column("aooa_eval2")]
        public bool AooaEval2 { get; set; }

        [Column("aooa_eval3")]
        public bool AooaEval3 { get; set; }

        [Column("aooa_eval4")]
        public bool AooaEval4 { get; set; }

        [Column("aooa_eval5")]
        public bool AooaEval5 { get; set; }

        [Column("aooa_eval6")]
        public bool AooaEval6 { get; set; }

        [Column("aooa_eval7")]
        public bool AooaEval7 { get; set; }

        [Column("aooa_eval8")]
        public bool AooaEval8 { get; set; }

        [Column("aooa_eval9")]
        public bool AooaEval9 { get; set; }

        [Column("aooa_eval10")]
        public bool AooaEval10 { get; set; }

        [Column("aooa_eval11")]
        public bool AooaEval11 { get; set; }

        [Column("aooa_eval12")]
        public bool AooaEval12 { get; set; }

        [Column("aooa_eval13")]
        public bool AooaEval13 { get; set; }

        [Column("aooa_eval14")]
        public bool AooaEval14 { get; set; }

        [Column("aooa_eval15")]
        public bool AooaEval15 { get; set; }

        [Column("aooa_eval16")]
        public bool AooaEval16 { get; set; }

        [Column("aooa_eval17")]
        public bool AooaEval17 { get; set; }

        [Column("aooa_eval18")]
        public bool AooaEval18 { get; set; }

        [Column("aooa_eval19")]
        public bool AooaEval19 { get; set; }

        [Column("aooa_product")]
        public string AooaProduct { get; set; }

        [Column("no_of_stores")]
        public int? NoOfStores { get; set; }

        [Column("bank_account_no")]
        public int BankAccountNo { get; set; }

        [Column("sn")]
        public int SN { get; set; }

        [Column("products")]
        public string Products { get; set; }

        [Column("brands")]
        public string Brands { get; set; }

        [Column("monthly_sales")]
        public string MonthlySales { get; set; }

        [Column("no_of_total_sub_dealers")]
        public int? NoOfTotalSubDealers { get; set; }

        [Column("has_earlier_worked_with_cmi")]
        public bool? HasEarlierWorkedWIthCMI { get; set; }

        [Column("year_of_operation_and_business_volume")]
        public string YearOfOperationAndBusinessVolume { get; set; }

        [Column("dealer_or_distributor")]
        public string DealerOrDistributor { get; set; }

        [Column("name_of_the_firm")]
        public string NameOfTheFirm { get; set; }

        [Column("total_investment")]
        public decimal? TotalInvestment { get; set; }

        [Column("expected_to1")]
        public decimal? ExpectedTO1 { get; set; }

        [Column("expected_to2")]
        public decimal? ExpectedTO2 { get; set; }

        [Column("expected_to3")]
        public decimal? ExpectedTO3 { get; set; }

        [Column("office_for_cmi")]
        public bool? OfficeForCMI { get; set; }

        [Column("go_down_for_cmi")]
        public bool? GoDownForCMI { get; set; }

        [Column("man_power_for_cmi")]
        public bool? ManPowerForCMI { get; set; }

        [Column("service_center_for_cmi")]
        public bool? ServiceCenterForCMI { get; set; }

        [Column("delivery_van_for_cmi")]
        public bool? DeliveryVanForCMI { get; set; }

        [Column("sales_man_power_for_cmi")]
        public bool? SalesManPowerForCMI { get; set; }

        [Column("computer_for_cmi")]
        public bool? ComputerForCMI { get; set; }

        [Column("others_for_cmi")]
        public bool? OthersForCMI { get; set; }

        [Column("market_reputation_ap_level1")]
        public int? MarketReputationAPLevel1 { get; set; } = 0;

        [Column("market_reputation_ap_level2")]
        public int? MarketReputationAPLevel2 { get; set; } = 0;

        [Column("market_reputation_ap_level3")]
        public decimal? MarketReputationAPLevel3 { get; set; } = 0;

        [Column("display_quality_ap_level1")]
        public int? DisplayQualityAPLevel1 { get; set; } = 0;

        [Column("display_quality_ap_level2")]
        public int? DisplayQualityAPLevel2 { get; set; } = 0;

        [Column("display_quality_ap_level3")]
        public decimal? DisplayQualityAPLevel3 { get; set; } = 0;

        [Column("distribution_retail_strength_ap_level1")]
        public int? DistributionRetailStrengthAPLevel1 { get; set; } = 0;

        [Column("distribution_retail_strength_ap_level2")]
        public int? DistributionRetailStrengthAPLevel2 { get; set; } = 0;

        [Column("distribution_retail_strength_ap_level3")]
        public decimal? DistributionRetailStrengthAPLevel3 { get; set; } = 0;

        [Column("financial_strength_ap_level1")]
        public int? FinancialStrengthAPLevel1 { get; set; } = 0;

        [Column("financial_strength_ap_level2")]
        public int? FinancialStrengthAPLevel2 { get; set; } = 0;

        [Column("financial_strength_ap_level3")]
        public decimal? FinancialStrengthAPLevel3 { get; set; } = 0;

    }

}

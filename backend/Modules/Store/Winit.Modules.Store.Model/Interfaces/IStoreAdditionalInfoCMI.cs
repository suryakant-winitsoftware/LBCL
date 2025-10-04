using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreAdditionalInfoCMI:IBaseModel
    {
        [Column("aoaa_ap_tools_level1")]
        public int AoaaApToolsLevel1 { get; set; } 

        [Column("aoaa_ap_tools_level2")]
        public int AoaaApToolsLevel2 { get; set; } 

        [Column("aoaa_ap_tools_average")]
        public decimal AoaaApToolsAverage { get; set; } 

        [Column("aoaa_ap_computer_level1")]
        public int AoaaApComputerLevel1 { get; set; } 

        [Column("aoaa_ap_computer_level2")]
        public int AoaaApComputerLevel2 { get; set; } 

        [Column("aoaa_ap_computer_average")]
        public decimal AoaaApComputerAverage { get; set; } 

        [Column("aoaa_ap_financial_strength_level1")]
        public int AoaaApFinancialStrengthLevel1 { get; set; } 

        [Column("aoaa_ap_financial_strength_level2")]
        public int AoaaApFinancialStrengthLevel2 { get; set; } 

        [Column("aoaa_ap_financial_strength_average")]
        public decimal AoaaApFinancialStrengthAverage { get; set; } 

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
        public int AoaaApManpowerLevel1 { get; set; } 
        public int AoaaApManpowerLevel2 { get; set; } 
        public decimal AoaaApManpowerAverage { get; set; } 
        public int AoaaApWorkshopLevel1 { get; set; } 
        public int AoaaApWorkshopLevel2 { get; set; } 
        public decimal AoaaApWorkshopAverage { get; set; } 
        public int AooaApTechnicalCompLevel1 { get; set; } 
        public int AooaApTechnicalCompLevel2 { get; set; } 
        public decimal AooaApTechnicalCompAverage { get; set; } 
        public bool AooaEval1 { get; set; }
        public bool AooaEval2 { get; set; }
        public bool AooaEval3 { get; set; }
        public bool AooaEval4 { get; set; }
        public bool AooaEval5 { get; set; }
        public bool AooaEval6 { get; set; }
        public bool AooaEval7 { get; set; }
        public bool AooaEval8 { get; set; }
        public bool AooaEval9 { get; set; }
        public bool AooaEval10 { get; set; }
        public bool AooaEval11 { get; set; }
        public bool AooaEval12 { get; set; }
        public bool AooaEval13 { get; set; }
        public bool AooaEval14 { get; set; }
        public bool AooaEval15 { get; set; }
        public bool AooaEval16 { get; set; }
        public bool AooaEval17 { get; set; }
        public bool AooaEval18 { get; set; }
        public bool AooaEval19 { get; set; }
        public string AooaProduct { get; set; }
        public int? NoOfStores { get; set; }
        public int BankAccountNo { get; set; }
        public int SN { get; set; }
        public string Products { get; set; }
        public string Brands { get; set; }
        public string MonthlySales { get; set; }
        public int? NoOfTotalSubDealers { get; set; }
        public bool? HasEarlierWorkedWIthCMI { get; set; }
        public string YearOfOperationAndBusinessVolume { get; set; }
        public string DealerOrDistributor { get; set; }
        public string NameOfTheFirm { get; set; }
        public decimal? TotalInvestment { get; set; }
        public decimal? ExpectedTO1 { get; set; }
        public decimal? ExpectedTO2 { get; set; }
        public decimal? ExpectedTO3 { get; set; }
        public bool? OfficeForCMI { get; set; }
        public bool? GoDownForCMI { get; set; }
        public bool? ManPowerForCMI { get; set; }
        public bool? ServiceCenterForCMI { get; set; }
        public bool? DeliveryVanForCMI { get; set; }
        public bool? SalesManPowerForCMI { get; set; }
        public bool? ComputerForCMI { get; set; }
        public bool? OthersForCMI { get; set; }
        public int? MarketReputationAPLevel1 { get; set; } 
        public int? MarketReputationAPLevel2 { get; set; } 
        public decimal? MarketReputationAPLevel3 { get; set; } 
        public int? DisplayQualityAPLevel1 { get; set; } 
        public int? DisplayQualityAPLevel2 { get; set; } 
        public decimal? DisplayQualityAPLevel3 { get; set; } 
        public int? DistributionRetailStrengthAPLevel1 { get; set; } 
        public int? DistributionRetailStrengthAPLevel2 { get; set; } 
        public decimal? DistributionRetailStrengthAPLevel3 { get; set; } 
        public int? FinancialStrengthAPLevel1 { get; set; } 
        public int? FinancialStrengthAPLevel2 { get; set; } 
        public decimal? FinancialStrengthAPLevel3 { get; set; } 

    }
}

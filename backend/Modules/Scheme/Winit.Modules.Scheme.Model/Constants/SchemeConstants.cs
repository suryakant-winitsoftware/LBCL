using System.Security.Authentication.ExtendedProtection;

namespace Winit.Modules.Scheme.Model.Constants
{
    public class SchemeConstants
    {
        public const string Executed = "Executed";
        public const string Pending_Execution = "Pending Execution";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Expired = "Expired";
        public const string Pending = "Pending";
        public const string FromDate = "FromDate";
        public const string ToDate = "ToDate";
        public const string ShowInactive = "ShowInactive";
        public const string Sell_In = "Sell In";
        public const string Sell_Out = "Sell Out";
        public const string SellOutRealSecondary = "SellOutActualSecondary";
        public const string QPS = "QPS";
        public const string Sales_Promotion = "Sales Promotion";


        public const string ApprovedFilesysType = "PO Invoice";
        public const string SalesPromotion = "SalesPromotion";
        public const string SalesPromotionExecution = "SalesPromotionExecution";

        public static string[] HO = ["BUHS", "CH"];
        public static string[] BM = ["BM", "BSEM"];

        public const string SI = "SI";
        public const string QPS_Code = "QPS";
        public const string SP = "SP";
        public const string SOA = "SOA";
        public const string SO = "SO";
        public const string S = "S";

        public const string NegativeSchemeSuffix = "(N)";
        //Approval Engine Rules
        public const string SARoleCode = "Sales Administration";
        public const string ASMRoleCode = "ASM";
        public const string BUHSRoleCode = "BUHS";

        public const int DistributorRule = 21;
        public const int TradeRule = 22;
        public const int StandingRule = 23;

        public const string RuleNameForSchemeWithPositiveMargin = "BM Created positive Scheme";
        public const int SchemeWithPositiveMargin = 14;
        public const int SchemeWithNegativeMargin = 15;
        public const string RuleNameForSchemeWithNegativeMargin = "BM Created negative Scheme";
        public const string UserTypeWithePositiveScheme = "BMCreatedPositiveScheme";
        public const string UserTypeWitheNegativeScheme = "BMCreatedNegativeScheme";

        public const string RuleNameForASM_SchemeWithPositiveMargin = "Scheme with positive margin";
        public const int ASM_SchemeWithPositiveMargin = 21;
        public const int ASM_SchemeWithNegativeMargin = 19;
        public const string RuleNameForASM_SchemeWithNegativeMargin = "Scheme with negative Margin";
        public const string ASM_UserTypeWithePositiveScheme = "Scheme with positive margin";
        public const string ASM_UserTypeWitheNegativeScheme = "Scheme with negative Margin";

        public const string RuleNameForBUHS_SchemeWithPositiveMargin = "BUHS Created Positive Scheme";
        public const int BUHS_SchemeWithPositiveMargin = 25;
        public const int BUHS_SchemeWithNegativeMargin = 24;
        public const string RuleNameForBUHS_SchemeWithNegativeMargin = "BUHS Created Negative Scheme";
        public const string BUHS_UserTypeWithePositiveScheme = "BUHSCreatedPositiveScheme";
        public const string BUHS_UserTypeWitheNegativeScheme = "BUHSCreatedNegativeScheme";
        public const string RuleNameForSA_SchemeWithPositiveMargin = "SA_scheme with positive margin";
        public const int SA_SchemeWithPositiveMargin = 12;
        public const int SA_SchemeWithNegativeMargin = 13;
        public const string RuleNameForSA_SchemeWithNegativeMargin = "SA_scheme with negative margin";
        public const string SA_UserTypeWithePositiveScheme = "SA_schemeWithPositiveMargin";
        public const string SA_UserTypeWitheNegativeScheme = "SA_schemeWithNegativeMargin";

        public const string RuleNameForSA_StandingProvisionRule = "SA creates Standing Provision Or Scheme";
        public const int SA_StandingProvisionRule = 16;
        public const string SA_StandingProvisionUserType = "SACreatesStandingProvisionOrScheme";


        public const string RuleNameForBUHS_StandingProvisionRule = "BUHS created Standing Provision";
        public const int BUHS_StandingProvisionRule = 17;
        public const string BUHS_StandingProvisionUserType = "BUHSCreatedStandingProvision";


        public const string SellInScheme = "SellInScheme";
        public const string StandingProvision = "StandingProvision";

        public const string Extend = "Extend";
        public const string Expire = "Expire";
    }
}


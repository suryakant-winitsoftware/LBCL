using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Constants
{
    public class FileSysTemplateStructure
    {
        public const string Demo = "Data/Demo/{DemoUID}/{Year}/{Month}/{Date}";
        public const string SKU_Path = "Data/SKU/{SKUUID}/{Year}/{Month}/{Date}";
        public const string EMP_Path = "Data/EMP/{EMPUID}/{Year}/{Month}/{Date}";
        public const string UPLOAD_DEBUG_LOG = "Data/DebugLog/{UserCode}";
        public const string STORE_CHECK = "Data/StoreCheck/{StoreUID}/{Year}/{Month}/{Date}";
        public const string ProductCatalogue = "Data/ProductCatalogue/{SKUCode}/{FileSysUID}";
        public const string SURVEY = "Data/Survey/{JobPositionUID}/{SurveyUID}/{SurveyQuestionUID}/{Year}/{Month}/{Date}";
        public const string Attendence = "Data/Attendance/{EmpUID}/{Year}/{Month}/{Date}";
        public const string STORE = "Data/Store/{StoreUID}/{Year}/{Month}/{Date}";
        public const string SCHEME = "Data/SCHEME/{SCHEMEUID}/{Year}/{Month}/{Date}";
        public const string PROFILE = "Data/Profile/{EmpID}/{Year}/{Month}/{Date}";
        public const string STORE_DOCUMENT = "Data/StoreDocument/{StoreUID}/{Year}/{Month}/{Date}";
        public const string SalesManSignature = "Data/Signature/{Module}/{Year}/{Month}/{Date}/{ModuleUID}";
        public const string DComPOD = "Data/DCom/{Year}/{Month}/{Date}/{OrderUID}";
        public const string ReturnOrder = "Data/ReturnOrder/{Year}/{Month}/{Date}/{OrderUID}";
        public const string ReturnOrderImage = "Data/ReturnOrder/{Year}/{Month}/{Date}/{OrderUID}/{ItemCode}";
        public const string DefaultCamera = "Data/Camera/{Year}/{Month}/{Date}";
        public const string CageCredit = "Data/CageCredit/{Year}/{Month}/{Date}/{ItemUID}";
        public const string ONBOARD_iMAGE = "Data/OnBoardImage/{Year}/{Month}/{Date}";
        public const string Capture_Competitor = "Data/CaptureCompetitor/{Year}/{Month}/{Date}";
        public const string Planogram = "Data/Planogram/{Year}/{Month}/{Date}";
        public const string Survey_Path = "Data/Survey/{Year}/{Month}/{Date}";
        public const string News_Activity = "Data/News_Activity/{UID}/{Year}/{Month}/{Date}";
    }
}

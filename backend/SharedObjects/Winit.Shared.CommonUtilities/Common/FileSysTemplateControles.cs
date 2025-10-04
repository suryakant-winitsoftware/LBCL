using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;

namespace Winit.Shared.CommonUtilities.Common;

public class FileSysTemplateControles
{
    public static string GetDemoFolderPath(string DemoUID)
    {
        return FileSysTemplateStructure.Demo
            .Replace("{DemoUID}", DemoUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetSKUFolderPath(string SKUUID)
    {
        return FileSysTemplateStructure.SKU_Path
            .Replace("{SKUUID}", SKUUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetUploadDebugLogFolderPath(string userCode)
    {
        return FileSysTemplateStructure.UPLOAD_DEBUG_LOG.Replace("{UserCode}", userCode);
    }
    public static string GetStoreCheckFolderPath(string storeUID)
    {
        return FileSysTemplateStructure.STORE_CHECK
            .Replace("{StoreUID}", storeUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetOnBoardImageCheckFolderPath(string storeUID)
    {
        return FileSysTemplateStructure.ONBOARD_iMAGE
            .Replace("{StoreUID}", storeUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetSurveyFolderPath(string jobPositionUID, string surveyUID, string surveyQuestionUID)
    {
        return FileSysTemplateStructure.SURVEY
            .Replace("{JobPositionUID}", jobPositionUID)
            .Replace("{SurveyUID}", surveyUID)
            .Replace("{SurveyQuestionUID}", surveyQuestionUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetPlanogramImageFolderPath(string selectedCustomerCode)
    {
        return FileSysTemplateStructure.Planogram
            .Replace("{SelectedCustomerCode}", selectedCustomerCode)
            .Replace("{Year}", DateTime.Now.Year.ToString())
            .Replace("{Month}", DateTime.Now.Month.ToString())
            .Replace("{Date}", DateTime.Now.Day.ToString());
    }

    public static string GetAttendenceFolderPath(string jobPositionUID, string empUID)
    {
        return FileSysTemplateStructure.Attendence
            .Replace("{EmpUID}", empUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetProductCatalogueFolderPath(string skuCode, string fileSysUID)
    {
        return FileSysTemplateStructure.ProductCatalogue
            .Replace("{SKUCode}", skuCode)
            .Replace("{FileSysUID}", fileSysUID);
    }
    public static string GetStoreFolderPath(string storeUID)
    {
        return FileSysTemplateStructure.STORE
            .Replace("{StoreUID}", storeUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetSchemeeFolderPath(string storeUID)
    {
        return FileSysTemplateStructure.SCHEME
            .Replace("{SCHEMEUID}", storeUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetNewsActivityFolderPath(string storeUID)
    {
        return FileSysTemplateStructure.News_Activity
            .Replace("{UID}", storeUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetStoreOwnerProfilePicFolderPath(string empUid)
    {
        return FileSysTemplateStructure.PROFILE
            .Replace("{EmpID}", empUid)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetStoreDocumentFolderPath(string storeUID)
    {
        return FileSysTemplateStructure.STORE_DOCUMENT
            .Replace("{StoreUID}", storeUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetSignatureFolderPath(string module, string moduleUID)
    {
        return FileSysTemplateStructure.SalesManSignature
            .Replace("{Module}", module)
            .Replace("{ModuleUID}", moduleUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetDComPODFolderPath(string orderUID)
    {
        return FileSysTemplateStructure.DComPOD
            .Replace("{OrderUID}", orderUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetReturnOrderFolderPath(string orderUID)
    {
        return FileSysTemplateStructure.ReturnOrder
            .Replace("{OrderUID}", orderUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetReturnOrderImageFolderPath(string orderUID)
    {
        return FileSysTemplateStructure.ReturnOrder
            .Replace("{OrderUID}", orderUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetCageCreditFolderPath(string ItemUID)
    {
        return FileSysTemplateStructure.CageCredit
            .Replace("{ItemUID}", ItemUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }

    public static string GetEmpFolderPath(string EMPUID)
    {
        return FileSysTemplateStructure.EMP_Path
            .Replace("{EMPUID}", EMPUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }

    public static string GetCaptureCapitatorImageFolderPath(string SelectedCustomerCode)
    {
        return FileSysTemplateStructure.Capture_Competitor
            .Replace("{SelectedCustomerCode}", SelectedCustomerCode)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetSurveyImageFolderPath(string SectionUID)
    {
        return FileSysTemplateStructure.Survey_Path
            .Replace("{SectionUID}", SectionUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    public static string GetSurveyVidoeFolderPath(string SectionUID)
    {
        return FileSysTemplateStructure.SURVEY
            .Replace("{SectionUID}", SectionUID)
            .Replace("{Year}", DateTime.Now.Year + "")
            .Replace("{Month}", DateTime.Now.Month + "")
            .Replace("{Date}", DateTime.Now.Day + "");
    }
    //public static void UpdateUploadStatus(Shared.sFAModel.FileSy fileSy, string s)
    //{
    //    FileSysData fileSysData = new FileSysData(null);
    //    fileSysData.UpdateUploadStatus(fileSy);
    //}
    //public static List<Shared.sFAModel.FileSy> GetPendingFileSyToUpload()
    //{
    //    FileSysData fileSysData = new FileSysData(null);
    //    return fileSysData.GetPendingFileSyToUpload();
    //}

    //public static FileSy CreateFileSys(string uniqueUID, string linkedItemType, string linkedItemUID, string fileSysType, string fileType, string fileName,
    //   string displayName, int fileSize, string relativePath, string latitude, string longitude, string jobPositionUID, string empUID)
    //{
    //    Shared.sFAModel.FileSy fileSy = new Shared.sFAModel.FileSy();
    //    fileSy.UID = uniqueUID;
    //    fileSy.LinkedItemType = linkedItemType;
    //    fileSy.LinkedItemUID = linkedItemUID;
    //    fileSy.FileSysType = fileSysType;
    //    fileSy.FileType = fileType;
    //    fileSy.FileName = fileName;
    //    fileSy.DisplayName = displayName;
    //    fileSy.FileSize = fileSize;
    //    fileSy.RelativePath = relativePath;
    //    fileSy.Latitude = latitude;
    //    fileSy.Longitude = longitude;
    //    fileSy.CreatedByJobPositionUID = jobPositionUID;
    //    fileSy.CreatedByEmpUID = empUID;
    //    return fileSy;
    //}
}

using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using NPOI.SS.Formula.Functions;
using Practice;
using Winit.Modules.BroadClassification.Model.Constant;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Common;
using Winit.UIComponents.Common.FileUploader;
namespace WinIt.Pages.Customer_Details
{
    public partial class DocumentsAppendixA
    {
        public bool Isinitialized { get; set; } = false;
        [Parameter] public bool HideMSME { get; set; }
        [Parameter] public bool HideVendor { get; set; }
        [Parameter]
        public string FirmType { get; set; }
        [Parameter]
        public string BroadClassification { get; set; }
        private Winit.UIComponents.Common.FileUploader.FileUploader? Memo_AssofileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Partnership_DealerPfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? GSTfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? PANfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Shop_EstfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? ESICfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? PFfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Auth_SignfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Broad_resfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Proof_AddressfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Proof_Res_AddressfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Vendor_DecfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Application_LetterfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? MSME_CertificatefileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Bank_MandatefileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? Cancellation_ChequefileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? RMA_DebitLetterfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? OthersfileUploader { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        [Parameter] public EventCallback<List<List<IFileSys>>> SaveOrUpdateDocument { get; set; }
        public List<IFileSys>? Memo_AssofileSysList { get; set; }
        public List<IFileSys>? Partnership_DealerfileSysList { get; set; }
        public List<IFileSys>? GSTfileSysList { get; set; }
        public List<IFileSys>? PANfileSysList { get; set; }
        public List<IFileSys>? Shop_EstfileSysList { get; set; }
        public List<IFileSys>? ESICfileSysList { get; set; }
        public List<IFileSys>? PFfileSysList { get; set; }
        public List<IFileSys>? Auth_SignfileSysList { get; set; }
        public List<IFileSys>? Broad_resfileSysList { get; set; }
        public List<IFileSys>? Proof_AddressfileSysList { get; set; }
        public List<IFileSys>? Proof_Res_AddressfileSysList { get; set; }
        public List<IFileSys>? Vendor_DecfileSysList { get; set; }
        public List<IFileSys>? Application_LetterfileSysList { get; set; }
        public List<IFileSys>? MSME_CertificatefileSysList { get; set; }
        public List<IFileSys>? Bank_MandatefileSysList { get; set; }
        public List<IFileSys>? Cancellation_ChequefileSysList { get; set; }
        public List<IFileSys>? RMA_DebitLetterfileSysList { get; set; }
        public List<IFileSys>? OthersfileSysList { get; set; }
        public List<List<IFileSys>>? DocumentAppendixfileSysList { get; set; } = new List<List<IFileSys>>();
        [Parameter] public List<IFileSys> fileSysList { get; set; } = new List<IFileSys>();
        [Parameter] public bool IsEditOnBoardDetails { get; set; } = false;
        private string? FilePath { get; set; }
        // private string? PDPFilePath { get; set; }
        [Parameter] public IOnBoardCustomerDTO? _onBoardCustomerDTO { get; set; }
        public bool IsSuccess { get; set; } = false;
        [Parameter]
        public string LinkedItemUID { get; set; }
        public string ButtonName { get; set; } = "Save";
        public List<string> RequiredFileNames { get; set; } = new List<string>();

        public bool ShowRequiredFieldNames { get; set; } = false;
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            _onBoardCustomerDTO = _serviceProvider.CreateInstance<IOnBoardCustomerDTO>();
            FilePath = FileSysTemplateControles.GetOnBoardImageCheckFolderPath(LinkedItemUID);
            // PDPFilePath = FileSysTemplateControles.GetOnBoardImageCheckFolderPath(LinkedItemUID);
            if (IsEditOnBoardDetails)
            {
                await MapFiles();
                ButtonName = "Update";
            }
            Isinitialized = true;
            _loadingService.HideLoading();
        }
        public async Task MapFiles()
        {
            try
            {
                DocumentAppendixfileSysList.Clear();
                DocumentAppendixfileSysList.Add(Memo_AssofileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.MemorandumofAssociation).ToList());
                DocumentAppendixfileSysList.Add(Partnership_DealerfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.PartnershipDealerAndPartnershipRegistration).ToList());
                DocumentAppendixfileSysList.Add(GSTfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.GST).ToList());
                DocumentAppendixfileSysList.Add(PANfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.PAN).ToList());
                DocumentAppendixfileSysList.Add(Shop_EstfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.Shop_EstablishmentCertificate_MunicipalLicence).ToList());
                DocumentAppendixfileSysList.Add(ESICfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.ESICRegistration).ToList());
                DocumentAppendixfileSysList.Add(PFfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.PFRegistration).ToList());
                DocumentAppendixfileSysList.Add(Auth_SignfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.ListofAuthorizedSignature).ToList());
                DocumentAppendixfileSysList.Add(Broad_resfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.BroadResolution).ToList());
                DocumentAppendixfileSysList.Add(Proof_AddressfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.ProofofAddressofPlaceofBusiness).ToList());
                DocumentAppendixfileSysList.Add(Proof_Res_AddressfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.ProofofPermanentandCurrentResidentialAddressofPropertier_Partner).ToList());
                DocumentAppendixfileSysList.Add(Vendor_DecfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.VenderDeclaration).ToList());
                DocumentAppendixfileSysList.Add(Application_LetterfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.ApplicationLetteronYourLetterHeadAsperFormat).ToList());
                DocumentAppendixfileSysList.Add(MSME_CertificatefileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.MSMECertificate).ToList());
                DocumentAppendixfileSysList.Add(Bank_MandatefileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.UploadBankMandateForm).ToList());
                DocumentAppendixfileSysList.Add(Cancellation_ChequefileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.UploadCancellationCheque).ToList());
                DocumentAppendixfileSysList.Add(RMA_DebitLetterfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.RMADebitLetter).ToList());
                DocumentAppendixfileSysList.Add(OthersfileSysList = fileSysList.Where(f => f.FileSysType == FileSysTypeConstants.Others).ToList());
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnClear()
        {
            try
            {
                if (Memo_AssofileSysList != null && Memo_AssofileSysList.Any()) Memo_AssofileSysList.Clear();
                if (Partnership_DealerfileSysList != null && Partnership_DealerfileSysList.Any()) Partnership_DealerfileSysList.Clear();
                if (GSTfileSysList != null && GSTfileSysList.Any()) GSTfileSysList.Clear();
                if (PANfileSysList != null && PANfileSysList.Any()) PANfileSysList.Clear();
                if (Shop_EstfileSysList != null && Shop_EstfileSysList.Any()) Shop_EstfileSysList.Clear();
                if (ESICfileSysList != null && ESICfileSysList.Any()) ESICfileSysList.Clear();
                if (PFfileSysList != null && PFfileSysList.Any()) PFfileSysList.Clear();
                if (Auth_SignfileSysList != null && Auth_SignfileSysList.Any()) Auth_SignfileSysList.Clear();
                if (Broad_resfileSysList != null && Broad_resfileSysList.Any()) Broad_resfileSysList.Clear();
                if (Proof_AddressfileSysList != null && Proof_AddressfileSysList.Any()) Proof_AddressfileSysList.Clear();
                if (Proof_Res_AddressfileSysList != null && Proof_Res_AddressfileSysList.Any()) Proof_Res_AddressfileSysList.Clear();
                if (Vendor_DecfileSysList != null && Vendor_DecfileSysList.Any()) Vendor_DecfileSysList.Clear();
                if (Application_LetterfileSysList != null && Application_LetterfileSysList.Any()) Application_LetterfileSysList.Clear();
                if (MSME_CertificatefileSysList != null && MSME_CertificatefileSysList.Any()) MSME_CertificatefileSysList.Clear();
                if (Bank_MandatefileSysList != null && Bank_MandatefileSysList.Any()) Bank_MandatefileSysList.Clear();
                if (Cancellation_ChequefileSysList != null && Cancellation_ChequefileSysList.Any()) Cancellation_ChequefileSysList.Clear();
                if (RMA_DebitLetterfileSysList != null && RMA_DebitLetterfileSysList.Any()) RMA_DebitLetterfileSysList.Clear();
                if (OthersfileSysList != null && OthersfileSysList.Any()) OthersfileSysList.Clear();
                ButtonName = "Save";
                IsEditOnBoardDetails = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        private void GetMoASavedImagePath(List<IFileSys> ImagePath)
        {
            Memo_AssofileSysList = ImagePath;
        }

        private void GetPDPSavedImagePath(List<IFileSys> ImagePath)
        {
            Partnership_DealerfileSysList = ImagePath;
        }

        private void GetGSTSavedImagePath(List<IFileSys> ImagePath)
        {
            GSTfileSysList = ImagePath;
        }

        private void GetPANSavedImagePath(List<IFileSys> ImagePath)
        {
            PANfileSysList = ImagePath;
        }

        private void GetShopEstSavedImagePath(List<IFileSys> ImagePath)
        {
            Shop_EstfileSysList = ImagePath;
        }

        private void GetESICSavedImagePath(List<IFileSys> ImagePath)
        {
            ESICfileSysList = ImagePath;
        }

        private void GetPFSavedImagePath(List<IFileSys> ImagePath)
        {
            PFfileSysList = ImagePath;
        }

        private void GetAuthSignSavedImagePath(List<IFileSys> ImagePath)
        {
            Auth_SignfileSysList = ImagePath;
        }

        private void GetBroadResSavedImagePath(List<IFileSys> ImagePath)
        {
            Broad_resfileSysList = ImagePath;
        }

        private void GetProofAddressSavedImagePath(List<IFileSys> ImagePath)
        {
            Proof_AddressfileSysList = ImagePath;
        }

        private void GetProofResAddressSavedImagePath(List<IFileSys> ImagePath)
        {
            Proof_Res_AddressfileSysList = ImagePath;
        }

        private void GetVendorDecSavedImagePath(List<IFileSys> ImagePath)
        {
            Vendor_DecfileSysList = ImagePath;
        }
        private void GetApplicationLetterSavedImagePath(List<IFileSys> ImagePath)
        {
            Application_LetterfileSysList = ImagePath;
        }
        private void GetMSMECertificateSavedImagePath(List<IFileSys> ImagePath)
        {
            MSME_CertificatefileSysList = ImagePath;
        }
        private void GetBankMandateSavedImagePath(List<IFileSys> ImagePath)
        {
            Bank_MandatefileSysList = ImagePath;
        }
        private void GetCancellationChequeSavedImagePath(List<IFileSys> ImagePath)
        {
            Cancellation_ChequefileSysList = ImagePath;
        }
        private void GetRMADebitLetterSavedImagePath(List<IFileSys> ImagePath)
        {
            RMA_DebitLetterfileSysList = ImagePath;
        }
        private void GetOthersSavedImagePath(List<IFileSys> ImagePath)
        {
            OthersfileSysList = ImagePath;
        }
        private void AfterDeleteImage()
        {

        }

        //protected async Task SaveFileSys()
        //{
        //    if (Memo_AssofileSysList == null || !Memo_AssofileSysList.Any())
        //    {
        //        await _alertService.ShowErrorAlert("Error","Please Upload Files!");
        //        //_tost.Add("SKU Image", "SKU Image Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        //        return;
        //    }
        //    if (Partnership_DealerfileSysList == null || !Partnership_DealerfileSysList.Any())
        //    {
        //        await _alertService.ShowErrorAlert("Error", "Please Upload Files!");
        //        //_tost.Add("SKU Image", "SKU Image Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        //        return;
        //    }
        //    Winit.Shared.Models.Common.ApiResponse<string> apiResponse = await Memo_AssofileUploader.MoveFiles();
        //    Winit.Shared.Models.Common.ApiResponse<string> apiResponse1 = await Partnership_DealerPfileUploader.MoveFiles();
        //    if (apiResponse.IsSuccess)
        //    {
        //        await SaveOrUpdateDocument.InvokeAsync(Memo_AssofileSysList);
        //    }
        //    if(apiResponse1.IsSuccess)
        //    {
        //        await SaveOrUpdateDocument.InvokeAsync(Partnership_DealerfileSysList);
        //    }
        //    IsSuccess = true;
        //}
        private void AddToDictionaryIfNotNull(Dictionary<Winit.UIComponents.Common.FileUploader.FileUploader?, List<IFileSys>?> fileUploaders,
                                                Winit.UIComponents.Common.FileUploader.FileUploader? uploader, List<IFileSys>? fileSysList)
        {
            if (uploader != null && fileSysList != null && fileSysList.Any())
            {
                fileUploaders.Add(uploader, fileSysList);
            }
        }
        public async Task SaveFileSys()
        {
            try
            {
                if (!string.IsNullOrEmpty(LinkedItemUID))
                {
                    var fileUploaders = new Dictionary<Winit.UIComponents.Common.FileUploader.FileUploader?, List<IFileSys>?>();
                    
                    // Add file uploaders to the dictionary only if their corresponding lists are not null or empty
                    AddToDictionaryIfNotNull(fileUploaders, Memo_AssofileUploader, Memo_AssofileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Partnership_DealerPfileUploader, Partnership_DealerfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, GSTfileUploader, GSTfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, PANfileUploader, PANfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Shop_EstfileUploader, Shop_EstfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, ESICfileUploader, ESICfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, PFfileUploader, PFfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Auth_SignfileUploader, Auth_SignfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Broad_resfileUploader, Broad_resfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Proof_AddressfileUploader, Proof_AddressfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Proof_Res_AddressfileUploader, Proof_Res_AddressfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Vendor_DecfileUploader, Vendor_DecfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Application_LetterfileUploader, Application_LetterfileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, MSME_CertificatefileUploader, MSME_CertificatefileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Bank_MandatefileUploader, Bank_MandatefileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, Cancellation_ChequefileUploader, Cancellation_ChequefileSysList);
                    AddToDictionaryIfNotNull(fileUploaders, RMA_DebitLetterfileUploader, RMA_DebitLetterfileSysList);

                    // Iterate over the dictionary to move files and save each corresponding list
                    if (await ValidateFiles(fileUploaders))
                    {
                        DocumentAppendixfileSysList.Clear();
                        foreach (var uploader in fileUploaders)
                        {
                            //if (uploader.Value != null)
                            var apiResponse = await uploader.Key.MoveFiles();
                            if (apiResponse.IsSuccess)
                            {
                                DocumentAppendixfileSysList.Add(uploader.Value);
                            }
                            else
                            {
                                await _alertService.ShowErrorAlert("Error", "Moving files failed");
                            }
                        }
                        await SaveOrUpdateDocument.InvokeAsync(DocumentAppendixfileSysList);
                        IsSuccess = true;
                        ButtonName = "Update";
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", "Please fill Customer Information");
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<bool> ValidateFiles(Dictionary<Winit.UIComponents.Common.FileUploader.FileUploader?, List<IFileSys>> fileUploaders)
        {
            try
            {
                var requiredUploaders = new List<string>();
                if (FirmType == null)
                {
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.MemorandumofAssociation);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.PartnershipDealerAndPartnershipRegistration);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.GST);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.PAN);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ListofAuthorizedSignature);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ProofofAddressofPlaceofBusiness);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ProofofPermanentandCurrentResidentialAddressofPropertier_Partner);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ApplicationLetteronYourLetterHeadAsperFormat);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.VenderDeclaration);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.UploadBankMandateForm);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.UploadCancellationCheque);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.RMADebitLetter);
                }
                if (HideVendor)
                {
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.VenderDeclaration);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.UploadBankMandateForm);
                }
                if (BroadClassification == BroadClassificationConst.SSD)
                {
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.RMADebitLetter);
                }
                if (FirmType == FirmTypeConstants.Company)
                {
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.GST);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.MemorandumofAssociation);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.PAN);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ProofofAddressofPlaceofBusiness);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ApplicationLetteronYourLetterHeadAsperFormat);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.UploadCancellationCheque);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ListofAuthorizedSignature);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ProofofPermanentandCurrentResidentialAddressofPropertier_Partner);
                }
                if (FirmType == FirmTypeConstants.Partnership)
                {
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.GST);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.PartnershipDealerAndPartnershipRegistration);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.PAN);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ProofofAddressofPlaceofBusiness);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ApplicationLetteronYourLetterHeadAsperFormat);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.UploadCancellationCheque);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ListofAuthorizedSignature);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ProofofPermanentandCurrentResidentialAddressofPropertier_Partner);
                }
                if (FirmType == FirmTypeConstants.Propertier)
                {
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.GST);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.PAN);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ProofofAddressofPlaceofBusiness);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ApplicationLetteronYourLetterHeadAsperFormat);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.UploadCancellationCheque);
                    requiredUploaders.Add(Winit.Modules.Store.Model.Constants.FileSysTypeConstants.ProofofPermanentandCurrentResidentialAddressofPropertier_Partner);
                }


                // Check if any required file uploader is empty
                var emptyFileUploaderNames = fileUploaders
                                                .Where(u => requiredUploaders.Contains(u.Key?.FileSysType) && (u.Value == null || !u.Value.Any()))
                                                .Select(u => u.Key?.FileSysType ?? "Unknown")
                                                .ToList();


                // Check if the count of file uploaders differs from requiredUploaders
                if (fileUploaders.Count != requiredUploaders.Count)
                {
                    emptyFileUploaderNames.Clear();
                    foreach (var list in requiredUploaders)
                    {
                        if (!fileUploaders.Any(p => p.Key?.FileSysType == list))
                        {
                            emptyFileUploaderNames.Add(list);
                        }
                    }
                }
                if (!fileUploaders.Any())
                {
                    emptyFileUploaderNames = requiredUploaders;
                }
                if (emptyFileUploaderNames.Any())
                {
                    // Format the message with newline characters
                    ShowRequiredFieldNames = true;
                    RequiredFileNames = emptyFileUploaderNames;
                    return false;
                }

                return true; // Return true if all required files are uploaded
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task Close()
        {
            ShowRequiredFieldNames = false;
        }
    }
}


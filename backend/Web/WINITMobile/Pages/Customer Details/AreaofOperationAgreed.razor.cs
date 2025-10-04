using Microsoft.AspNetCore.Components;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIModels.Common.GST;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Customer_Details
{
    public partial class AreaofOperationAgreed : BaseComponentBase
    {
        [Parameter] public EventCallback<IStoreAdditionalInfoCMI> SaveandUpdate { get; set; }
        [Parameter] public IOnBoardCustomerDTO? _onBoardCustomerDTO { get; set; }
        [Parameter] public EventCallback<List<List<IFileSys>>> SaveOrUpdateDocument { get; set; }

        [Parameter]
        public List<ISelectionItem> AooaTypeselectionItems { get; set; } = new List<ISelectionItem>
        {
            new SelectionItem { UID = "1", Label = "ASP"},
            new SelectionItem { UID = "2", Label = "ASP Plus"},
            new SelectionItem { UID = "3", Label = "Installation Partner" },

            };
        public bool IsSuccess { get; set; } = false;
        [Parameter] public GSTINDetailsModel GSTINDetailsModel { get; set; }
        [Parameter] public string StoreAdditionalInfoCMIUid { get; set; }

        public bool Isinitialized { get; set; } = false;
        private bool IsSaveAttempted { get; set; } = false;
        private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploaderEvidence1 { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploaderEvidence2 { get; set; } = new Winit.UIComponents.Common.FileUploader.FileUploader();
        private List<IFileSys>? fileSysListEvidence1 { get; set; }
        private List<IFileSys>? fileSysListEvidence2 { get; set; }
        private string? FilePath { get; set; }
        private string? ButtonName { get; set; } = "Save";
        [Parameter] public EventCallback<string> AooaTypeSelection { get; set; }
        [Parameter] public bool IsEditOnBoardDetails { get; set; }
        [Parameter] public string LinkedItemUID { get; set; }
        private List<List<IFileSys>>? DocumentAppendixfileSysList { get; set; } = new List<List<IFileSys>>();
        [Parameter]
        public List<ISelectionItem> DummyData { get; set; } = new List<ISelectionItem>
        {
            new SelectionItem { UID = "1", Label = FirmTypeConstants.Propertier},
            new SelectionItem { UID = "2", Label = FirmTypeConstants.Partnership},
            new SelectionItem { UID = "3", Label = FirmTypeConstants.Company },

            };
        [Parameter] public EventCallback<string> InsertDataInChangeRequest { get; set; }
        [Parameter] public bool CustomerEditApprovalRequired { get; set; }
        [Parameter] public string TabName { get; set; }
        public IStoreAdditionalInfoCMI OriginalStoreAdditionalInfoCMI { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            try
            {
                await SaveInitialValuesAsync();
                SetEditForTypeDD(_onBoardCustomerDTO);
                FilePath = FileSysTemplateControles.GetOnBoardImageCheckFolderPath(LinkedItemUID);
                Isinitialized = true;
                if (TabName == StoreConstants.Confirmed)
                {
                    var concreteAddress = _onBoardCustomerDTO?.StoreAdditionalInfoCMI as StoreAdditionalInfoCMI;
                    OriginalStoreAdditionalInfoCMI = concreteAddress.DeepCopy()!;

                }
                await Task.CompletedTask;
                base.OnInitialized();
            }
            catch (Exception ex)
            {

            }
            StateHasChanged();
            _loadingService.HideLoading();
        }
        public void SetEditForTypeDD(IOnBoardCustomerDTO onBoardCustomerDTO)
        {

            foreach (var item in AooaTypeselectionItems)
            {
                if (item.Label == _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaType)
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }
        }
        protected override void OnInitialized()
        {
            if (IsEditOnBoardDetails)
            {
                ButtonName = "Update";
                InitializedRadioCategoriesVariable();
                MapFiles();
            }
            base.OnInitialized();
        }
        public void MapFiles()
        {
            try
            {
                fileSysListEvidence1 = _onBoardCustomerDTO.FileSys.Where(p => p.FileSysType == FileSysTypeConstants.AreaOfOperationEvidence1).ToList();
                fileSysListEvidence2 = _onBoardCustomerDTO.FileSys.Where(p => p.FileSysType == FileSysTypeConstants.AreaOfOperationEvidence2).ToList();
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnWayOfSelectionSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaCcSecWayOfCommu = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                // sku.Code = selecetedValue?.Code;
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaCcSecWayOfCommu = selecetedValue?.Label;

            }
        }
        public class Criteria
        {
            public string Name { get; set; }
            public Dictionary<string, int> Scores { get; set; } = new Dictionary<string, int> { { "BM", 0 }, { "FSM", 0 } };
            public double AverageScore
            {
                get
                {
                    int count = Scores.Values.Count(v => v > 0);
                    if (count == 0)
                        return 0;

                    int sum = Scores.Values.Sum();
                    return (double)sum / count;
                }
            }
        }

        public async Task OnAooaTypeSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaType = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                // sku.Code = selecetedValue?.Code;
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaType = selecetedValue?.Label;
                await AooaTypeSelection.InvokeAsync(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaType);
                StateHasChanged();
            }
        }
        private void OnCategoryChange(ChangeEventArgs e, string type)
        {
            // Update the StoreAdditionalInfo property directly
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaCategory = type;
        }
        private void OnProductChange(ChangeEventArgs e, string type)
        {
            // Update the StoreAdditionalInfo property directly
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaProduct = type;
        }
        private class RadioCategory
        {
            public string Key { get; set; }
            public string Label { get; set; }
            public string Name { get; set; }
            public string IdYes { get; set; }
            public string IdNo { get; set; }
            public bool IsYesChecked { get; set; }
            public bool IsNoChecked { get; set; }
        }
        private List<RadioCategory> RadioCategorys { get; set; } = new()
                    {
                        new RadioCategory
                        {
                            Key = "backgroundCheck",
                            Label = RadioLabels.BackgroundCheckOfProposedChannelPartnerFromMarketRegardingBusiness,
                            Name = "backgroundCheckYesNo",
                            IdYes = "backgroundCheckYes",
                            IdNo = "backgroundCheckNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "physicalVerification",
                            Label = RadioLabels.PhysicalVerificationRegardingOfficeAndResidence,
                            Name = "physicalVerificationYesNo",
                            IdYes = "physicalVerificationYes",
                            IdNo = "physicalVerificationNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "meetParents",
                            Label = RadioLabels.MeetAllTheParentsOrDirectorsOfTheChannelPartner,
                            Name = "meetParentsYesNo",
                            IdYes = "meetParentsYes",
                            IdNo = "meetParentsNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "orderPlacement",
                            Label = RadioLabels.IsOrderPlacedByNewBuyerOrThroughTradingPlatformOrEmailEnquiry,
                            Name = "orderPlacementYesNo",
                            IdYes = "orderPlacementYes",
                            IdNo = "orderPlacementNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "customerContact",
                            Label = RadioLabels.HasCustomerBeenContactedThroughOfficialContactInformation,
                            Name = "customerContactYesNo",
                            IdYes = "customerContactYes",
                            IdNo = "customerContactNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "customerVisit",
                            Label = RadioLabels.HasCustomerBeenVisitedAtOfficialAddress,
                            Name = "customerVisitYesNo",
                            IdYes = "customerVisitYes",
                            IdNo = "customerVisitNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "registrationNumber",
                            Label = RadioLabels.IsRegistrationNumberTheSameInAllFiles,
                            Name = "registrationNumberYesNo",
                            IdYes = "registrationNumberYes",
                            IdNo = "registrationNumberNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "payerName",
                            Label = RadioLabels.IsPayerNameExactlyTheSameAsBuyerNameInAllFiles,
                            Name = "payerNameYesNo",
                            IdYes = "payerNameYes",
                            IdNo = "payerNameNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "payingBankLocation",
                            Label = RadioLabels.IsLocationOfPayingBankSameAsBuyerNameInAllFiles,
                            Name = "payingBankLocationYesNo",
                            IdYes = "payingBankLocationYes",
                            IdNo = "payingBankLocationNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "taxNumber",
                            Label = RadioLabels.IsTaxNumberConsistentInAllFiles,
                            Name = "taxNumberYesNo",
                            IdYes = "taxNumberYes",
                            IdNo = "taxNumberNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "emailProvider",
                            Label = RadioLabels.DoesBuyerUseHotmailOrGmail,
                            Name = "emailProviderYesNo",
                            IdYes = "emailProviderYes",
                            IdNo = "emailProviderNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "buyersEmailAddress",
                            Label = RadioLabels.IsBuyersEmailAddressSameAsInCreditReport,
                            Name = "buyersEmailAddressYesNo",
                            IdYes = "buyersEmailAddressYes",
                            IdNo = "buyersEmailAddressNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "buyersAddress",
                            Label = RadioLabels.IsBuyersAddressSameAsInCreditReport,
                            Name = "buyersAddressYesNo",
                            IdYes = "buyersAddressYes",
                            IdNo = "buyersAddressNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "consigneeAddress",
                            Label = RadioLabels.IsConsigneeAddressSameAsBuyersLocationAndWarehouseAddressInCreditReport,
                            Name = "consigneeAddressYesNo",
                            IdYes = "consigneeAddressYes",
                            IdNo = "consigneeAddressNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "goodsShipped",
                            Label = RadioLabels.AreGoodsShippedToThirdPartyWithDifferentDestinationPort,
                            Name = "goodsShippedYesNo",
                            IdYes = "goodsShippedYes",
                            IdNo = "goodsShippedNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "consigneeOnBillOfLading",
                            Label = RadioLabels.IsConsigneeAddressOnBillOfLadingSameAsBuyersDomicileOrPlaceOfBusiness,
                            Name = "consigneeOnBillOfLadingYesNo",
                            IdYes = "consigneeOnBillOfLadingYes",
                            IdNo = "consigneeOnBillOfLadingNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "consigneeSameAsBuyer",
                            Label = RadioLabels.IsConsigneeOnBillOfLadingSameAsBuyer,
                            Name = "consigneeSameAsBuyerYesNo",
                            IdYes = "consigneeSameAsBuyerYes",
                            IdNo = "consigneeSameAsBuyerNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "temporaryChanges",
                            Label = RadioLabels.AreThereAnyTemporaryChangesToConsigneeAddressOrPaymentAccount,
                            Name = "temporaryChangesYesNo",
                            IdYes = "temporaryChangesYes",
                            IdNo = "temporaryChangesNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        },
                        new RadioCategory
                        {
                            Key = "multipleIdentities",
                            Label = RadioLabels.DoesBuyerProvideMultipleCorporateIdentitiesForDecentralizedProcurement,
                            Name = "multipleIdentitiesYesNo",
                            IdYes = "multipleIdentitiesYes",
                            IdNo = "multipleIdentitiesNo",
                            IsYesChecked = true,
                            IsNoChecked = false
                        }
                    };


        public class RadioLabels
        {
            public const string BackgroundCheckOfProposedChannelPartnerFromMarketRegardingBusiness = "Background Check of Proposed Channel Partner from Market regarding Business creditability & financial health";
            public const string PhysicalVerificationRegardingOfficeAndResidence = "Physical verification regarding office & Residence";
            public const string MeetAllTheParentsOrDirectorsOfTheChannelPartner = "Meet All the Parents/ directors of the channel Partner";
            public const string IsOrderPlacedByNewBuyerOrThroughTradingPlatformOrEmailEnquiry = "Is the order placed by a buyer newly met at trade fair or from some trading platform or through email enquiry (without site visit) or third party?";
            public const string HasCustomerBeenContactedThroughOfficialContactInformation = "Has the customer been contacted through their official contact information?";
            public const string HasCustomerBeenVisitedAtOfficialAddress = "Has the customer been visited at their official address?";
            public const string IsRegistrationNumberTheSameInAllFiles = "Is the registration number the same in all the files?";
            public const string IsPayerNameExactlyTheSameAsBuyerNameInAllFiles = "Is the payer name exactly the same as the buyer name in all the files?";
            public const string IsLocationOfPayingBankSameAsBuyerNameInAllFiles = "Is the location of the paying bank the same as the buyer's location in all the files?";
            public const string IsTaxNumberConsistentInAllFiles = "Is the tax number consistent in all the files?";
            public const string DoesBuyerUseHotmailOrGmail = "Does the buyer use Hotmail or Gmail?";
            public const string IsBuyersEmailAddressSameAsInCreditReport = "Is the buyer's email address the same with the one shown in the credit report?";
            public const string IsBuyersAddressSameAsInCreditReport = "Is the buyer's address the same with the one in the credit report?";
            public const string IsConsigneeAddressSameAsBuyersLocationAndWarehouseAddressInCreditReport = "Is the consignee address  same with the buyer's location & warehouse address in the credit report?";
            public const string AreGoodsShippedToThirdPartyWithDifferentDestinationPort = "Are the goods shipped to a third party and the actual destination port is far from buyer's location (e.g., a Western European buyer whose goods will be delivered to Africa)?";
            public const string IsConsigneeAddressOnBillOfLadingSameAsBuyersDomicileOrPlaceOfBusiness = "Is the consignee address on the bill of lading the same as the buyer's domicile/place of business?";
            public const string IsConsigneeOnBillOfLadingSameAsBuyer = "Is the consignee on BL same with buyer?";
            public const string AreThereAnyTemporaryChangesToConsigneeAddressOrPaymentAccount = "Are there any temporary changes to consignee address or payment account?";
            public const string DoesBuyerProvideMultipleCorporateIdentitiesForDecentralizedProcurement = "Does the buyer provide multiple corporate identities for decentralized procurement?";
        }
        private async Task SaveInitialValuesAsync()
        {
            foreach (var category in RadioCategorys)
            {
                await OnRadioChanged1(category.Key, category.IsYesChecked);
            }
        }
        private async Task OnRadioChanged1(string key, bool selected)
        {
            // Update the state of the selected category
            var selectedCategory = RadioCategorys.FirstOrDefault(c => c.Key == key);
            if (selectedCategory != null)
            {
                selectedCategory.IsYesChecked = selected;
                selectedCategory.IsNoChecked = !selected;
            }

            if (key == "backgroundCheck")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval1 = selected;
            }
            else if (key == "physicalVerification")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval2 = selected;
            }
            else if (key == "meetParents")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval3 = selected;
            }
            else if (key == "orderPlacement")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval4 = selected;
            }
            else if (key == "customerContact")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval5 = selected;
            }
            else if (key == "customerVisit")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval6 = selected;
            }
            else if (key == "registrationNumber")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval7 = selected;
            }
            else if (key == "payerName")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval8 = selected;
            }
            else if (key == "payingBankLocation")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval9 = selected;
            }
            else if (key == "taxNumber")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval10 = selected;
            }
            else if (key == "emailProvider")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval11 = selected;
            }
            else if (key == "buyersEmailAddress")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval12 = selected;
            }
            else if (key == "buyersAddress")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval13 = selected;
            }
            else if (key == "consigneeAddress")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval14 = selected;
            }
            else if (key == "goodsShipped")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval15 = selected;
            }
            else if (key == "consigneeOnBillOfLading")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval16 = selected;
            }
            else if (key == "consigneeSameAsBuyer")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval17 = selected;
            }
            else if (key == "temporaryChanges")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval18 = selected;
            }
            else if (key == "multipleIdentities")
            {
                _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaEval19 = selected;
            }

        }
        public void InitializedRadioCategoriesVariable()
        {
            foreach (var item in RadioCategorys)
            {
                if (item.Key == "backgroundCheck")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval1 ?? true;
                }
                else if (item.Key == "physicalVerification")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval2 ?? true;
                }
                else if (item.Key == "meetParents")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval3 ?? true;
                }
                else if (item.Key == "orderPlacement")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval4 ?? true;
                }
                else if (item.Key == "customerContact")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval5 ?? true;
                }
                else if (item.Key == "customerVisit")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval6 ?? true;
                }
                else if (item.Key == "registrationNumber")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval7 ?? true;
                }
                else if (item.Key == "payerName")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval8 ?? true;
                }
                else if (item.Key == "payingBankLocation")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval9 ?? true;
                }
                else if (item.Key == "taxNumber")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval10 ?? true;
                }
                else if (item.Key == "emailProvider")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval11 ?? true;
                }
                else if (item.Key == "buyersEmailAddress")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval12 ?? true;
                }
                else if (item.Key == "buyersAddress")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval13 ?? true;
                }
                else if (item.Key == "consigneeAddress")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval14 ?? true;
                }
                else if (item.Key == "goodsShipped")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval15 ?? true;
                }
                else if (item.Key == "consigneeOnBillOfLading")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval16 ?? true;
                }
                else if (item.Key == "consigneeSameAsBuyer")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval17 ?? true;
                }
                else if (item.Key == "temporaryChanges")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval18 ?? true;
                }
                else if (item.Key == "multipleIdentities")
                {
                    item.IsYesChecked = _onBoardCustomerDTO?.StoreAdditionalInfoCMI?.AooaEval19 ?? true;
                }
            }

        }
        private void GetSavedImagePathEvidence1(List<IFileSys> ImagePath)
        {
            fileSysListEvidence1 = ImagePath;
        }
        private void GetSavedImagePathEvidence2(List<IFileSys> ImagePath)
        {
            fileSysListEvidence2 = ImagePath;
        }
        private void AfterDeleteImage()
        {

        }
        private Dictionary<string, decimal> points = new Dictionary<string, decimal>();
        public string ValidationMessage;
        private void HandleRadioChange(ChangeEventArgs e, string category)
        {
            // Extract the value from ChangeEventArgs
            var selectedValue = e.Value?.ToString();

            // Update the points based on the category
            if (selectedValue != null)
            {
                points[category] = CalculatePoints(selectedValue);
                Mapping(points, category);
                RecalculateAverage();
                StateHasChanged();
            }
        }
        public int? Mapping(Dictionary<string, decimal> mappingPoints, string category)
        {
            try
            {
                switch (category)
                {
                    case "bm1":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaApTechnicalCompLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "fsm1":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaApTechnicalCompLevel2 = Convert.ToInt32(mappingPoints[category]);
                    case "bm2":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApManpowerLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "fsm2":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApManpowerLevel2 = Convert.ToInt32(mappingPoints[category]);
                    case "bm3":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApWorkshopLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "fsm3":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApWorkshopLevel2 = Convert.ToInt32(mappingPoints[category]);
                    case "bm4":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApToolsLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "fsm4":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApToolsLevel2 = Convert.ToInt32(mappingPoints[category]);
                    case "bm5":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApComputerLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "fsm5":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApComputerLevel2 = Convert.ToInt32(mappingPoints[category]);
                    case "bm6":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApFinancialStrengthLevel1 = Convert.ToInt32(mappingPoints[category]);
                    case "fsm6":
                        return _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApFinancialStrengthLevel2 = Convert.ToInt32(mappingPoints[category]);
                    default:
                        return 0;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        //private async void RecalculateAverage()
        //{
        //    _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaApTechnicalCompAverage = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaApTechnicalCompLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaApTechnicalCompLevel2) / 2;
        //    _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApManpowerAverage = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApManpowerLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApManpowerLevel2) / 2;
        //    _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApWorkshopAverage = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApWorkshopLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApWorkshopLevel2) / 2;
        //    _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApToolsAverage = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApToolsLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApToolsLevel2) / 2;
        //    _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApComputerAverage = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApComputerLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApComputerLevel2) / 2;
        //    _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApFinancialStrengthAverage = ((decimal)_onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApFinancialStrengthLevel1 + _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApFinancialStrengthLevel2) / 2;
        //    TotalAverageScore();
        //    StateHasChanged();
        //}
        private async void RecalculateAverage()
        {
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaApTechnicalCompAverage = _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaApTechnicalCompLevel1;
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApManpowerAverage = _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApManpowerLevel1;
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApWorkshopAverage = _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApWorkshopLevel1;
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApToolsAverage = _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApToolsLevel1;
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApComputerAverage = _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApComputerLevel1;
            _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApFinancialStrengthAverage = _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApFinancialStrengthLevel1;
            TotalAverageScore();
            StateHasChanged();
        }
        public decimal? TotalAverageScore()
        {
            decimal? sum = _onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaApTechnicalCompAverage +
                           _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApManpowerAverage +
                           _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApWorkshopAverage +
                           _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApToolsAverage +
                           _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApComputerAverage +
                           _onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaApFinancialStrengthAverage;

            // Check if the sum is not null
            if (sum.HasValue)
            {
                return Math.Round(sum.Value / 6, 2);
            }

            return null;
        }

        private decimal CalculatePoints(string selectedValue)
        {
            return selectedValue switch
            {
                "Good" => 3,
                "Average" => 2,
                "Poor" => 1,
                _ => 0
            };
        }
        public async Task SaveOrUpdate()
        {
            IsSaveAttempted = true;
            ValidateAllFields();
            if (string.IsNullOrWhiteSpace(ValidationMessage))
            {
                try
                {
                    var fileUploaders = new Dictionary<Winit.UIComponents.Common.FileUploader.FileUploader?, List<IFileSys>?>();
                    AddToDictionaryIfNotNull(fileUploaders, fileUploaderEvidence1, fileSysListEvidence1);
                    AddToDictionaryIfNotNull(fileUploaders, fileUploaderEvidence2, fileSysListEvidence2);
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

                    }
                    await SaveOrUpdateDocument.InvokeAsync(DocumentAppendixfileSysList);
                    _onBoardCustomerDTO.StoreAdditionalInfoCMI.SectionName = OnboardingScreenConstant.AreaofOperationAgreed;
                    if (TabName == StoreConstants.Confirmed && !CustomerEditApprovalRequired)
                    {
                        await RequestChange();
                        await SaveandUpdate.InvokeAsync(_onBoardCustomerDTO.StoreAdditionalInfoCMI);
                    }
                    else if (TabName == StoreConstants.Confirmed && CustomerEditApprovalRequired)
                    {
                        await RequestChange();
                    }
                    else
                    {
                        await SaveandUpdate.InvokeAsync(_onBoardCustomerDTO.StoreAdditionalInfoCMI);
                    }
                    IsSuccess = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }
        private void AddToDictionaryIfNotNull(Dictionary<Winit.UIComponents.Common.FileUploader.FileUploader?, List<IFileSys>?> fileUploaders,
                                                Winit.UIComponents.Common.FileUploader.FileUploader? uploader, List<IFileSys>? fileSysList)
        {
            if (uploader != null && fileSysList != null && fileSysList.Any())
            {
                fileUploaders.Add(uploader, fileSysList);
            }
        }
        public async Task<bool> ValidateFiles(Dictionary<Winit.UIComponents.Common.FileUploader.FileUploader?, List<IFileSys>> fileUploaders)
        {
            try
            {
                //if (fileSysListEvidence1.Count > 0 && fileSysListEvidence2.Count > 0)
                //{
                //    return true;
                //}

                //return false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void ValidateAllFields()
        {
            ValidationMessage = null;

            if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaCode) ||
                string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaCcFcContactPerson) ||
                string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaCcFcReplyReceivedBy))
            {
                ValidationMessage = "The Following fields have invalid field(s)" + ": ";

                if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AooaCode))
                {
                    ValidationMessage += "Code, ";
                }

                if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaCcFcContactPerson))
                {
                    ValidationMessage += "Contact Person, ";
                }
                if (string.IsNullOrWhiteSpace(_onBoardCustomerDTO.StoreAdditionalInfoCMI.AoaaCcFcReplyReceivedBy))
                {
                    ValidationMessage += "ReceivedBy, ";
                }

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');
            }
        }
        #region Change RequestLogic

        public async Task RequestChange()
        {
            List<IChangeRecordDTO> ChangeRecordDTOs = new List<IChangeRecordDTO>
            {
                new ChangeRecordDTO
                {
                     Action= OnboardingScreenConstant.Update,
                    ScreenModelName = OnboardingScreenConstant.AreaofOperationAgreed,
                    UID = StoreAdditionalInfoCMIUid,
                    ChangeRecords = CommonFunctions.GetChangedData(CommonFunctions.CompareObjects(OriginalStoreAdditionalInfoCMI!, _onBoardCustomerDTO?.StoreAdditionalInfoCMI!)!)
                }
            }
            .Where(c => c.ChangeRecords.Count > 0)
            .ToList();

            if (ChangeRecordDTOs.Count > 0)
            {
                var ChangeRecordDTOInJson = CommonFunctions.ConvertToJson(ChangeRecordDTOs);
                await InsertDataInChangeRequest.InvokeAsync(ChangeRecordDTOInJson);
            }
            ChangeRecordDTOs.Clear();
        }
        public object GetModifiedObject(IStoreAdditionalInfoCMI storeAdditionalinfoCMI)
        {
            var modifiedObject = new
            {
                storeAdditionalinfoCMI.AooaType,
                storeAdditionalinfoCMI.AooaCategory,
                storeAdditionalinfoCMI.AooaAspToClose,
                storeAdditionalinfoCMI.AooaCode,
                storeAdditionalinfoCMI.AooaProduct,
                storeAdditionalinfoCMI.AooaEval1,
                storeAdditionalinfoCMI.AooaEval2,
                storeAdditionalinfoCMI.AooaEval3,
                storeAdditionalinfoCMI.AooaEval4,
                storeAdditionalinfoCMI.AooaEval5,
                storeAdditionalinfoCMI.AooaEval6,
                storeAdditionalinfoCMI.AooaEval7,
                storeAdditionalinfoCMI.AooaEval8,
                storeAdditionalinfoCMI.AooaEval9,
                storeAdditionalinfoCMI.AooaEval10,
                storeAdditionalinfoCMI.AooaEval11,
                storeAdditionalinfoCMI.AooaEval12,
                storeAdditionalinfoCMI.AooaEval13,
                storeAdditionalinfoCMI.AooaEval14,
                storeAdditionalinfoCMI.AooaEval15,
                storeAdditionalinfoCMI.AooaEval16,
                storeAdditionalinfoCMI.AooaEval17,
                storeAdditionalinfoCMI.AooaEval18,
                storeAdditionalinfoCMI.AooaEval19,
                storeAdditionalinfoCMI.AooaApTechnicalCompLevel1,
                storeAdditionalinfoCMI.AooaApTechnicalCompLevel2,
                storeAdditionalinfoCMI.AooaApTechnicalCompAverage,
                storeAdditionalinfoCMI.AoaaApManpowerLevel1,
                storeAdditionalinfoCMI.AoaaApManpowerLevel2,
                storeAdditionalinfoCMI.AoaaApManpowerAverage,
                storeAdditionalinfoCMI.AoaaApWorkshopLevel1,
                storeAdditionalinfoCMI.AoaaApWorkshopLevel2,
                storeAdditionalinfoCMI.AoaaApWorkshopAverage,
                storeAdditionalinfoCMI.AoaaApToolsLevel1,
                storeAdditionalinfoCMI.AoaaApToolsLevel2,
                storeAdditionalinfoCMI.AoaaApToolsAverage,
                storeAdditionalinfoCMI.AoaaApComputerLevel1,
                storeAdditionalinfoCMI.AoaaApComputerLevel2,
                storeAdditionalinfoCMI.AoaaApComputerAverage,
                storeAdditionalinfoCMI.AoaaApFinancialStrengthLevel1,
                storeAdditionalinfoCMI.AoaaApFinancialStrengthLevel2,
                storeAdditionalinfoCMI.AoaaApFinancialStrengthAverage,
                storeAdditionalinfoCMI.AoaaCcFcContactPerson,
                storeAdditionalinfoCMI.AoaaCcFcSentTime,
                storeAdditionalinfoCMI.AoaaCcFcReplyReceivedBy,
                storeAdditionalinfoCMI.AoaaCcSecWayOfCommu,
                storeAdditionalinfoCMI.AoaaCcSecContactPerson,
                storeAdditionalinfoCMI.AoaaCcSecSentTime,
                storeAdditionalinfoCMI.AoaaCcSecReplyReceivedBy
            };


            return modifiedObject;
        }
        #endregion
    }

}




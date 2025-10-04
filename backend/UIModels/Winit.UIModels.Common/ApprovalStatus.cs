using System.Text.Json.Serialization;

namespace Winit.UIModels.Common
{
    public class ApprovalStatus
    {
        public long RequestId { get; set; }
        public List<ApprovalStatusResponse>? approvalStatus { get; set; }
        public List<ApprovalLogDetail>? approvalLog { get; set; }
    }


    public class ApprovalStatusResponse
    {
        public string RoleName { get; set; }
        public string ApproverId { get; set; }
        public int ApproverLevel { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string ApproverType { get; set; }
    }
    public class ApprovalApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? data { get; set; }
        public string? Action { get; set; }
    }
    public class Customer
    {
        [JsonPropertyName("CustomerId")]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        [JsonPropertyName("IsNew")]
        public bool IsNew { get; set; }
        [JsonPropertyName("OrderCount")]
        public int OrderCount { get; set; }
        [JsonPropertyName("CreditLimit")]
        public decimal CreditLimit { get; set; }
        //new
        [JsonPropertyName("BroadCustomerClassification")]
        public string BroadCustomerClassification { get; set; }
        [JsonPropertyName("ClassificationType")]
        public string ClassificationType { get; set; }
        [JsonPropertyName("FirmType")]
        public string FirmType { get; set; }
        [JsonPropertyName("City")]
        public string City { get; set; }
        [JsonPropertyName("IsMSME")]
        public bool IsMSME { get; set; }
        [JsonPropertyName("CreatedBy")]
        public string CreatedBy { get; set; }
    }
    public class ApprovalRequest
    {
        public long Id { get; set; }
        public int RuleId { get; set; }
        public string RequesterId { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }

    }
    public class ApprovalLogs
    {
        public long requestId { get; set; }
        public string ApproverId { get; set; }
        public int level { get; set; }
        public string Status { get; set; }
        public string modifiedBy { get; set; }
        public string reassignTo { get; set; }
        public string Comments { get; set; }

    }
    public class ApprovalLogDetail
    {
        public string? ApproverId { get; set; }
        public int? Level { get; set; }
        public string? Status { get; set; }
        public string? Comments { get; set; }
        public string? ModifiedBy { get; set; }
        public string? ReassignTo { get; set; }
        public string? CreatedOn { get; set; }
    }
}

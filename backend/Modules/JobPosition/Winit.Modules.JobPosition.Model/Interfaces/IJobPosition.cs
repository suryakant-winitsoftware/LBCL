using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.JobPosition.Model.Interfaces
{
    public interface IJobPosition : IBaseModel
    {
        public string CompanyUID { get; set; }
        public string Designation { get; set; }
        public string EmpUID { get; set; }
        [AuditTrail]
        public string Department { get; set; }
        [AuditTrail("Role")]
        public string UserRoleUID { get; set; }
        [AuditTrail("ReportsTo")]
        public string ReportsToUID { get; set; }
        public string LocationMappingTemplateUID { get; set; }
        public string LocationMappingTemplateName { get; set; }
        [AuditTrail]
        public string OrgUID { get; set; }
        public string SeqCode { get; set; }
        public bool HasEOT { get; set; }
        public decimal CollectionLimit { get; set; }
        public string SKUMappingTemplateUID { get; set; }
        public string SKUMappingTemplateName { get; set; }
        [AuditTrail("Location Type")]
        public string LocationType { get; set; }
        [AuditTrail("Location Value")]
        public string LocationValue { get; set; }
        [AuditTrail("Branch")]
        public string BranchUID { get; set; }
        [AuditTrail("Sales Office")]
        public string SalesOfficeUID { get; set; }
        public string EmpCode { get; set; }
    }
}

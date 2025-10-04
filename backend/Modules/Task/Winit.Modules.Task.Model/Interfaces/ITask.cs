using Winit.Modules.Base.Model;

namespace Winit.Modules.Task.Model.Interfaces
{
    public interface ITask : IBaseModel
    {
        string Code { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        int TaskTypeId { get; set; }
        string TaskTypeName { get; set; }
        int? TaskSubTypeId { get; set; }
        int SalesOrgId { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        bool IsActive { get; set; }
        string Priority { get; set; }
        string Status { get; set; }
        string TaskData { get; set; }
    }
}
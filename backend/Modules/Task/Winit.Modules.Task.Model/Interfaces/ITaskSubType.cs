using Winit.Modules.Base.Model;

namespace Winit.Modules.Task.Model.Interfaces
{
    public interface ITaskSubType : IBaseModel
    {
        string Code { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        int TaskTypeId { get; set; }
        bool IsActive { get; set; }
        int SortOrder { get; set; }
    }
}
using Winit.Modules.Base.Model;

namespace Winit.Modules.Task.Model.Interfaces
{
    public interface ITaskType : IBaseModel
    {
        string Code { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        bool IsActive { get; set; }
        int SortOrder { get; set; }
    }
}
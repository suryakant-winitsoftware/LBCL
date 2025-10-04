namespace Winit.Modules.Task.Model.Enums
{
    public enum TaskStatus
    {
        Draft = 0,
        Active = 1,
        Completed = 2,
        Cancelled = 3,
        OnHold = 4
    }

    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    public enum AssignmentStatus
    {
        Pending = 0,
        Assigned = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        Overdue = 5
    }

    public enum AssignedToType
    {
        User = 0,
        UserGroup = 1
    }
}
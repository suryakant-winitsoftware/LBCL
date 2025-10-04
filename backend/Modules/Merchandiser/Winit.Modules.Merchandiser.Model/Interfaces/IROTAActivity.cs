using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Merchandiser.Model.Interfaces
{
    public interface IROTAActivity : IBaseModel
    {
        string JobPositionUID { get; set; }
        DateTime RotaDate { get; set; }
        string RotaActivityName { get; set; }
    }
} 
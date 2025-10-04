using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Planogram.Model.Interfaces
{
    public interface IPlanogramSetupV1 : IBaseModel
    {
        string Description { get; set; }
        string ImageUrl { get; set; }
    }
} 
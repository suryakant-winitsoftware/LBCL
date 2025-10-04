using System;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromotionVolumeCapView : PromotionVolumeCap
    {
        public ActionType ActionType { get; set; }
    }
}
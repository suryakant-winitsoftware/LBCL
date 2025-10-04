namespace Winit.Modules.Vehicle.Model.Interfaces
{
    public interface IVehicleStatus : IVehicle
    {
        public bool IsStarted { get; set; }
        public bool IsRunningRoute { get; set; }
        public string UserJourneyVehicleUID { get; set; }
        public bool IsStopped { get; set; }
        public System.DateTime? CurrentRunningDate { get; set; }
    }
}

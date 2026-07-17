namespace FleetPulse.SignalRHub.Contracts.Response
{
    public class LastestDriverStateResponse
    {
        public string DriverId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
        public string LastSeen { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}

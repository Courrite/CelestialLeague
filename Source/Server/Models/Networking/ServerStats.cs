namespace CelestialLeague.Server.Models
{
    public class ServerStats
    {
        public int TotalConnections { get; set; }
        public int AuthenticatedUsers { get; set; }
        public int TotalSessions { get; set; }
        public int UptimeSeconds { get; set; }
        public bool IsRunning { get; set; }
        public long TotalBytesSent { get; set; }
        public long TotalBytesReceived { get; set; }
        public int TotalPacketsSent { get; set; }
        public int TotalPacketsReceived { get; set; }
    }
}
namespace CTrue.DcsConnect
{
    public class DcsServerInfo
    {
        public string Theatre { get; set; }
        public string MissionName { get; set; }
        public string MissionDescription { get; set;}
        public bool IsServer { get; set; }
        public bool IsMultiplayer { get; set; }
        
        /// <summary>
        /// Gets the last mission time received from the server via the streaming APIs.
        /// </summary>
        public double MissionTime { get; set; }
    }
}
using RurouniJones.Dcs.Grpc.V0.Common;

namespace CTrue.DcsConnect
{
    /// <summary>
    /// Contains information about a player connected to the DCS Server.
    /// </summary>
    public class PlayerInfo
    {
        public PlayerType PlayerType { get; set; }

        public uint PlayerId { get; private set; }

        public string PlayerName { get; private set; }

        /// <summary>
        /// The unique player id, based on DCS account.
        /// </summary>
        public string Ucid { get; private set; }

        public string NetworkAddress { get; set; }
       
        public bool Connected { get; set; }
        
        /// <summary>
        /// A general status text, such as disconnect reason.
        /// </summary>
        public string Status { get; set; }

        public Coalition Coalition { get; set; }

        public string SlotId { get; set; }

        public PlayerInfo(uint playerId, string playerName, string ucid)
        {
            PlayerType = PlayerType.Player;
            PlayerId = playerId;
            PlayerName = playerName;
            Ucid = ucid;
        }

        public PlayerInfo(string playerName)
        {
            PlayerType = PlayerType.Client;
            PlayerId = uint.MaxValue; 
            PlayerName = playerName;
        }


    }

    public enum PlayerType
    {
        /// <summary>
        /// Player connected to a multiplayer session.
        /// </summary>
        Player,
        /// <summary>
        /// Local client
        /// </summary>
        Client
    }
}
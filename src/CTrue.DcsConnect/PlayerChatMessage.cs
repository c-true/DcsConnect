using System;

namespace CTrue.DcsConnect
{
    public class PlayerChatMessage : EventArgs
    {
        public uint PlayerId { get; private set; }
        public string Message { get; private set; }

        public PlayerChatMessage(uint playerId, string message)
        {
            PlayerId = playerId;
            Message = message;
        }
    }
}
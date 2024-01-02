namespace CTrue.DcsConnect
{
    public class PlayerInUnitChangedEventArgs
    {
        public uint UnitId { get; private set; }
        
        public PlayerInfo PlayerInfo { get; private set; }

        public PlayerInUnitChangeType ChangeType { get; private set; }

        public PlayerInUnitChangedEventArgs(uint unitId, PlayerInUnitChangeType changeType, PlayerInfo playerInfo)
        {
            UnitId = unitId;
            PlayerInfo = playerInfo;
            ChangeType = changeType;
        }
    }

    public enum PlayerInUnitChangeType
    {
        PlayerEnteredUnit,
        PlayerLeavedUnit
    }
}
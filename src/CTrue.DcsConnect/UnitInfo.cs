using System.Diagnostics;
using RurouniJones.Dcs.Grpc.V0.Common;

namespace CTrue.DcsConnect
{
    [DebuggerDisplay("N:{Name} / CS: {Callsign} / P:{PlayerName} / {Type}")]
    public class UnitInfo
    {
        public UnitInfo(uint unitId)
        {
            
        }

        public uint Id { get; private set; }
        
        public string Symbology { get; set; }
        
        public string Name { get; private set; }
        
        public string PlayerName { get; private set; }
        public string Callsign { get; private set; }
        public uint GroupId { get; private set; }
        public string GroupName { get; private set; }
        public int GroupCategory { get; private set; }
        public uint NumberInGroup { get; private set; }
        public int Coalition { get; private set; }
        public string Type { get; private set; }
        /// <summary>
        /// Latitude in decimal degrees.
        /// </summary>
        public double Latitude { get; private set; }
        /// <summary>
        /// Longitude in decimal degrees.
        /// </summary>
        public double Longitude { get; private set; }
        /// <summary>
        /// Elevation in meters AMSL.
        /// </summary>
        public double Elevation { get; private set; }
        public double Heading { get; private set; }
        public double Speed { get; private set; }
        public bool Deleted { get; private set; }

        private UnitInfo()
        {
        }

        public UnitInfo(Unit unit)
        {
            Id = unit.Id;

            Coalition = (int)unit.Coalition;
            Name = unit.Name;
            Latitude = unit.Position.Lat;
            Longitude = unit.Position.Lon;
            Elevation = unit.Position.Alt;
            Callsign = unit.Callsign;
            Type = unit.Type;
            PlayerName = unit.PlayerName;
            GroupId = unit.Group.Id;
            GroupName = unit.Group.Name;
            GroupCategory = (int)unit.Group.Category;
            NumberInGroup = unit.NumberInGroup;
            Speed = unit.Velocity.Speed;
            Heading = unit.Orientation.Heading;
        }

        public void Update(Unit unit)
        {
            Coalition = (int)unit.Coalition;
            Name = unit.Name;
            Latitude = unit.Position.Lat;
            Longitude = unit.Position.Lon;
            Elevation = unit.Position.Alt;
            Callsign = unit.Callsign;
            Type = unit.Type;
            PlayerName = unit.PlayerName;
            GroupId = unit.Group.Id;
            GroupName = unit.Group.Name;
            GroupCategory = (int)unit.Group.Category;
            NumberInGroup = unit.NumberInGroup;
            Speed = unit.Velocity.Speed;
            Heading = unit.Orientation.Heading;
        }
    }
}
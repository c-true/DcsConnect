using System.Diagnostics;

namespace CTrue.DcsConnect
{
    /// <summary>
    /// Represents an update for a unit in DCS.
    /// </summary>
    [DebuggerDisplay("N:{Name} / CS: {Callsign} / P:{PlayerName} / {Type}")]
    public class DcsUnitUpdate
    {
        public double Time { get; private set; }
        public UnitInfo Unit { get; private set; }

        public bool Deleted => DeletedUnitId.HasValue;

        public uint? DeletedUnitId { get; private set; }
        public string DeletedUnitName { get; private set; }

        public static DcsUnitUpdate UpdateUnit(double time, UnitInfo updatedUnit)
        {
            return new DcsUnitUpdate(time, updatedUnit);
        }

        public static DcsUnitUpdate DeleteUnit(double time, uint deletedUnitId, string deletedUnitName)
        {
            return new DcsUnitUpdate(time, deletedUnitId, deletedUnitName);
        }

        private DcsUnitUpdate(double time, UnitInfo unit)
        {
            Time = time;
            Unit = unit;
        }

        private DcsUnitUpdate(double time, uint unitId, string unitName)
        {
            Time = time;
            DeletedUnitId = unitId;
            DeletedUnitName = unitName;
        }
    }
}
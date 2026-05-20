using UnityEngine;

namespace StructrualPatternIdentifier
{
    [AddComponentMenu("SPI/Relation Attribute/Locked")]
    public class Locked : RelationAttribute
    {
        public enum LockStates
        {
            Locked,
            Unlocked,
        }

        [SerializeField] private LockStates locked = LockStates.Locked;

        public LockStates LockedStates
        {
            get => locked;
            set => locked = value;
        }

        public bool IsLocked => locked == LockStates.Locked;
        public bool IsUnlocked => locked == LockStates.Unlocked;
    }
}

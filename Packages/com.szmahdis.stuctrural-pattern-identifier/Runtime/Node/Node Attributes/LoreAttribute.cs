using UnityEngine;

namespace StructrualPatternIdentifier
{
    [AddComponentMenu("SPI/Node Attributes/LoreAttribute")]
    public class LoreAttribute : NodeAttribute
    {
        public enum ClanType { None, Human, Dwarf, Orc, Elf }

        [Header("Clan Type")]
        [SerializeField] private ClanType clan = ClanType.None;

        public ClanType Clan => clan;

        public bool IsHuman => clan == ClanType.Human;
        public bool IsDwarf => clan == ClanType.Dwarf;
        public bool IsOrc => clan == ClanType.Orc;
        public bool IsElf => clan == ClanType.Elf;
        public bool IsNone => clan == ClanType.None;
    }
}

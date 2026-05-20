using UnityEngine;

namespace StructrualPatternIdentifier
{
    [AddComponentMenu("SPI/Node Attributes/Technical Attribute")]
    public class TechnicalAttribute : NodeAttribute
    {
        [Header("Technical State")]
        [Range(0, 10)] public float difficultyWeight;
        public string requiredKeyID;
    }
}
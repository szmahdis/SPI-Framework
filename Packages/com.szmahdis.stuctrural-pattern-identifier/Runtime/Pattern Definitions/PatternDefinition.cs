using System.Collections.Generic;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    public abstract class PatternDefinition : ScriptableObject

    {
        [Header("General Info")]
        public string patternName;

        public abstract bool Validate(List<SPINode> cluster);
    }
}
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    public class PatternIdentifier : MonoBehaviour
    {
        public PatternGraphManager graphManager;
        public List<PatternDefinition> patternDefinitions;

        [ContextMenu("ScanForPatterns")]
        public void ScanForPatterns()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<SPINode> allNodes = graphManager.GetNodes();
            List<string> detectedPatterns = new List<string>();

            foreach (var definition in patternDefinitions)
            {
                if (definition.Validate(allNodes))
                {
                    detectedPatterns.Add(definition.patternName);
                    UnityEngine.Debug.Log($"<color=green>Pattern Detected: {definition.patternName}</color>");
                }
            }

            sw.Stop();

            if (detectedPatterns.Count == 0)
            {
                UnityEngine.Debug.Log("<color=red>No patterns detected.</color>");
            }

            UnityEngine.Debug.Log($"<b>Scan completed in:</b> {sw.Elapsed.TotalMilliseconds} ms");

        }
    }
}

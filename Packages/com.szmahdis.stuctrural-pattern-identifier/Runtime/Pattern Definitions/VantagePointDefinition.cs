using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    [CreateAssetMenu(fileName = "Vantage Point", menuName = "SPI Patterns/Vantage Point")]
    public class VantagePointDefinition : PatternDefinition
    {
        [SerializeField] private int MinLoSRequired = 6;
        public override bool Validate(List<SPINode> cluster)
        {
            if (cluster == null || cluster.Count <= MinLoSRequired) return false;

            int totalVantagePointsFound = 0;

            foreach (var node in cluster)
            {

                var visibleNodes = cluster
                      .Where(n => n.Id != node.Id)
                      .Select(n => new { Node = n, LoS = node.GetLoSRelation(n.Id) })
                      .Where(x => x.LoS != null)
                      .ToList();

                if (visibleNodes.Count >= MinLoSRequired)
                {
                    totalVantagePointsFound++;
                    var visibleIds = string.Join(",", visibleNodes.Select(n => n.Node.Id));
                    Debug.Log($"[Vantage Point] Node {node.Id} is a Vantage Point with Line of Sight to {visibleNodes.Count} nodes:[{visibleIds}]");
                }

            }

            if (totalVantagePointsFound > 0)
            {
                Debug.Log($"[Vantage Point] Search complete. Total Vantage Point patterns: {totalVantagePointsFound}");
                return true;
            }

            return false;
        }
    }
}

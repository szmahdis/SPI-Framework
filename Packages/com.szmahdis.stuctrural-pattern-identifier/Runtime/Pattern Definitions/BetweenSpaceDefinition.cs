using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    [CreateAssetMenu(fileName = "Between Space", menuName = "SPI Patterns/Between Space")]
    public class BetweenSpaceDefinition : PatternDefinition
    {
        public override bool Validate(List<SPINode> cluster)
        {

            if (cluster == null || cluster.Count < 3) return false;

            var clusterMap = cluster.ToDictionary(n => n.Id);
            int totalBetweenSpacesFound = 0;

            foreach (var node in cluster)
            {
                var nodeLore = node.GetAttribute<LoreAttribute>();
                if (nodeLore == null) continue;

                if (!nodeLore.IsNone) continue;

                var neighbors = node.walkableRelations
                  .Where(r => clusterMap.ContainsKey(r.NodeId))
                  .Select(r => clusterMap[r.NodeId])
                  .Where(n =>
                  {
                      var nLore = n.GetAttribute<LoreAttribute>();
                      if (nLore == null || nLore.IsNone) return false;

                      var los = node.GetLoSRelation(n.Id);
                      return los != null;
                  })
                  .ToList();

                for (int i = 0; i < neighbors.Count; i++)
                {
                    var loreI = neighbors[i].GetAttribute<LoreAttribute>();
                    if (loreI == null) continue;

                    for (int j = i + 1; j < neighbors.Count; j++)
                    {
                        var loreJ = neighbors[j].GetAttribute<LoreAttribute>();
                        if (loreJ == null) continue;

                        if (loreI.Clan != loreJ.Clan)
                        {
                            Debug.Log($"[BetweenSpace] Node {node.Id} connects {loreI.Clan} and {loreJ.Clan}");
                            totalBetweenSpacesFound++;
                            goto NextNode;
                        }
                    }
                }
            NextNode:;
            }

            if (totalBetweenSpacesFound > 0)
            {
                Debug.Log($"[BetweenSpace] Search complete. Total Between Space patterns: {totalBetweenSpacesFound}");
                return true;
            }

            return false;
        }
    }
}

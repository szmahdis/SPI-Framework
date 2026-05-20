using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    [CreateAssetMenu(fileName = "Rhizome", menuName = "SPI Patterns/Rhizome")]
    public class RhizomeDefinition : PatternDefinition
    {
        private List<HashSet<SPINode>> allCliques = new List<HashSet<SPINode>>();
        [SerializeField] private int MinRhizomeLength = 3;
        public override bool Validate(List<SPINode> cluster)
        {
            if (cluster == null) return false;

            allCliques.Clear();

            int totalRhizomesFound = 0;

            BronKerboschAlg(new HashSet<SPINode>(), new HashSet<SPINode>(cluster), new HashSet<SPINode>());

            foreach (var clique in allCliques)
            {
                ;
                if (clique.Count >= MinRhizomeLength)
                {
                    Debug.Log("Found a complete graph with " + clique.Count + " nodes!");
                    totalRhizomesFound++;
                    string nodeNames = string.Join(", ", clique.Select(n => n.Id));
                    Debug.Log($"Found a complete graph (Rhizome) with {clique.Count} nodes: [{nodeNames}]");
                }
            }

            if (totalRhizomesFound > 0)
            {
                Debug.Log($"[Rhizome] Search complete. Total Rhizomes found: {totalRhizomesFound}");
                return true;
            }
            return false;
        }
        private void BronKerboschAlg(HashSet<SPINode> R, HashSet<SPINode> P, HashSet<SPINode> X)
        {
            if (P.Count == 0 && X.Count == 0)
            {
                allCliques.Add(new HashSet<SPINode>(R));
            }

            SPINode pivotU = P.Concat(X).FirstOrDefault();
            if (pivotU == null) return;

            HashSet<SPINode> neighborsOfU = GetNeighborSet(pivotU);

            List<SPINode> candidates = new List<SPINode>(P);

            foreach (var v in candidates)
            {
                if (neighborsOfU.Contains(v)) continue;

                HashSet<SPINode> nextR = new HashSet<SPINode>(R) { v };

                HashSet<SPINode> vNeighbors = GetNeighborSet(v);
                HashSet<SPINode> nextP = new HashSet<SPINode>(P);
                nextP.IntersectWith(vNeighbors);

                HashSet<SPINode> nextX = new HashSet<SPINode>(X);
                nextX.IntersectWith(vNeighbors);

                BronKerboschAlg(nextR, nextP, nextX);

                P.Remove(v);
                X.Add(v);
            }
        }

        private HashSet<SPINode> GetNeighborSet(SPINode node)
        {
            HashSet<SPINode> neighbors = new HashSet<SPINode>();
            node.walkableRelations.ForEach(rel => neighbors.Add(rel.Node));
            return neighbors;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    [CreateAssetMenu(fileName = "Labyrinth", menuName = "SPI Patterns/Labyrinth")]
    public class LabyrinthDefinition : PatternDefinition
    {
        [SerializeField] private int MinLabyrinthLength = 4;
        public override bool Validate(List<SPINode> cluster)
        {
            if (cluster == null || cluster.Count < MinLabyrinthLength) return false;

            var nodeDegree = cluster.ToDictionary(node => node.Id, node => 0);

            foreach (SPINode node in cluster)
            {
                int degree = 0;
                foreach (SPIWalkableRelation rel in node.walkableRelations)
                {
                    if (rel.NodeId != null
                        && nodeDegree.ContainsKey(rel.NodeId))
                    {
                        degree++;
                    }
                }
                nodeDegree[node.Id] = degree;
            }

            List<SPINode> boundaryNodes = cluster
                .Where(n => nodeDegree[n.Id] == 1 || nodeDegree[n.Id] > 2)
                .ToList();

            HashSet<SPINode> baseIgnored = new HashSet<SPINode>(
                cluster.Where(n => nodeDegree[n.Id] == 0)
            );

            int totalPathsFound = 0;
            HashSet<SPINode> ignoredNodes = new HashSet<SPINode>();

            for (int i = 0; i < boundaryNodes.Count; i++)
            {
                for (int j = i + 1; j < boundaryNodes.Count; j++)
                {
                    SPINode nodeA = boundaryNodes[i];
                    SPINode nodeB = boundaryNodes[j];

                    ignoredNodes.Clear();
                    ignoredNodes.UnionWith(baseIgnored);

                    foreach (var bNode in boundaryNodes)
                    {
                        if (bNode != nodeA && bNode != nodeB)
                            ignoredNodes.Add(bNode);
                    }

                    FindAllPathsWithDFS(
                        nodeA,
                        nodeB,
                        ignoredNodes,
                        nodeDegree,
                        (path) =>
                        {
                            totalPathsFound++;
                            Debug.Log($"[Labyrinth] Path found: {PathToString(path)}");
                        }
                    );

                }
            }

            if (totalPathsFound > 0)
            {
                Debug.Log($"[Labyrinth] Search complete. Total labyrinth paths: {totalPathsFound}");
                return true;
            }


            return false;

        }

        private void FindAllPathsWithDFS(
               SPINode start,
                SPINode goal,
                HashSet<SPINode> ignoredNodes,
                Dictionary<string, int> nodeDegree,
                Action<List<SPINode>> onPathFound)
        {

            Stack<(SPINode node, bool isBacktracking)> stack = new Stack<(SPINode, bool)>();

            HashSet<SPINode> visited = new HashSet<SPINode>(ignoredNodes);
            List<SPINode> currentPath = new List<SPINode>();

            stack.Push((start, false));

            while (stack.Count > 0)
            {
                var (current, isBacktracking) = stack.Pop();

                if (isBacktracking)
                {
                    visited.Remove(current);
                    currentPath.RemoveAt(currentPath.Count - 1);
                    continue;
                }

                currentPath.Add(current);
                visited.Add(current);

                stack.Push((current, true));

                if (current.Id == goal.Id)
                {
                    if (currentPath.Count >= MinLabyrinthLength)
                    {
                        onPathFound.Invoke(currentPath);
                    }
                    continue;
                }

                foreach (var relation in current.walkableRelations)
                {
                    if (relation.NodeId != null
                        && !visited.Contains(relation.Node))
                    {
                        stack.Push((relation.Node, false));
                    }
                }
            }
        }

        private string PathToString(List<SPINode> path)
        {
            if (path == null || path.Count == 0) return "No Path Found";
            return string.Join(" -> ", path.Select(node => node.Id));
        }
    }
}
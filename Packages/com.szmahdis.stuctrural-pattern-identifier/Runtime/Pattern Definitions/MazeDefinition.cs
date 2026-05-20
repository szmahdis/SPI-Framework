using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StructrualPatternIdentifier
{

    [CreateAssetMenu(fileName = "Maze", menuName = "SPI Patterns/Maze")]
    public class MazeDefinition : PatternDefinition
    {
        private struct BFSResult
        {
            public HashSet<SPINode> reachable;
            public List<SPINode> pathToGoal;
            public Dictionary<SPINode, SPINode> cameFrom;
        }

        public override bool Validate(List<SPINode> cluster)
        {
            if (cluster == null) return false;


            int totalMazesFound = 0;
            Dictionary<int, (SPINode start, SPINode goal)> pairs = GetMazePairs(cluster);

            foreach (var kvp in pairs)
            {
                int id = kvp.Key;
                SPINode startNode = kvp.Value.start;
                SPINode goalNode = kvp.Value.goal;

                Debug.Log($"[Maze] Validating mazeID {id}: start={startNode.Id} -> goal={goalNode.Id}");

                BFSResult initialBFS = RunBFS(startNode, goalNode);

                if (initialBFS.pathToGoal == null)
                {
                    Debug.Log("[Maze] Goal unreachable from start, skipping.");
                    continue;
                }

                bool hasMultiplePaths = ValidateEdgeDisjointPaths(startNode, goalNode, initialBFS);
                bool hasDeadEndBranch = ValidateDeadEndBranch(startNode, goalNode, initialBFS);

                if (hasMultiplePaths)
                    Debug.Log("[Maze] Mode A passed: multiple edge-disjoint paths to goal.");

                if (hasDeadEndBranch)
                    Debug.Log("[Maze] Mode B passed: at least one dead-end branch exists off the solution path.");

                if (hasMultiplePaths || hasDeadEndBranch)
                {
                    totalMazesFound++;
                }
                else
                {
                    Debug.Log("[Maze] Validation Failed: no alternative paths or dead-end branches found.");
                }
            }

            if (totalMazesFound > 0)
            {
                Debug.Log($"[Maze] Search complete. Total mazes found: {totalMazesFound}");
                return true;
            }
            return false;
        }

        private Dictionary<int, (SPINode start, SPINode goal)> GetMazePairs(List<SPINode> allNodes)
        {
            var starts = new Dictionary<int, SPINode>();
            var goals = new Dictionary<int, SPINode>();

            foreach (var node in allNodes)
            {
                var gpAttr = node.GetAttribute<GameplayAttribute>();
                if (gpAttr == null) continue;

                int id = gpAttr.clusterId;

                if (id == -1 && !gpAttr.IsNone)
                {
                    Debug.LogWarning($"[Maze] Node {node.Id} is set to Auto-assign (-1). Maze pairs require a clusterId > 0.");
                    continue;
                }

                if (id >= 0)
                {
                    if (gpAttr.IsStart) starts[id] = node;
                    if (gpAttr.IsGoal) goals[id] = node;
                }
            }

            return starts
                .Where(kvp =>
                {
                    if (goals.ContainsKey(kvp.Key)) return true;
                    Debug.LogWarning($"[Maze] clusterId {kvp.Key} has a Start but no matching Goal.");
                    return false;
                })
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (start: kvp.Value, goal: goals[kvp.Key])
                );
        }
        private bool ValidateEdgeDisjointPaths(SPINode startNode, SPINode goalNode, BFSResult initialBFS)
        {
            List<SPINode> pathA = initialBFS.pathToGoal;
            string pathAString = PathToString(pathA);
            Debug.Log("[Maze] Path A: " + pathAString);

            for (int i = 0; i < pathA.Count - 1; i++)
            {
                HashSet<(SPINode, SPINode)> singleEdgeToBlock = new HashSet<(SPINode, SPINode)>();
                singleEdgeToBlock.Add((pathA[i], pathA[i + 1]));
                singleEdgeToBlock.Add((pathA[i + 1], pathA[i]));

                BFSResult altBFS = RunBFS(startNode, goalNode, singleEdgeToBlock);

                if (altBFS.pathToGoal != null && PathToString(altBFS.pathToGoal) != pathAString)
                {
                    Debug.Log("[Maze] Alternative Path B found: " + PathToString(altBFS.pathToGoal));
                    return true;
                }
            }

            return false;
        }

        private bool ValidateDeadEndBranch(SPINode startNode, SPINode goalNode, BFSResult initialBFS)
        {
            bool foundAnyJunction = false;

            foreach (SPINode junction in initialBFS.reachable)
            {
                int degree = junction.walkableRelations.Count;

                if (junction == goalNode)
                    continue;


                if (junction == startNode)
                {
                    if (degree < 2)
                        continue;
                }
                else
                {
                    if (degree < 3)
                        continue;
                }

                bool hasDeadEndPath = false;
                bool hasPathToGoal = false;

                foreach (var relation in junction.walkableRelations)
                {
                    SPINode neighbor = relation.Node;

                    HashSet<(SPINode, SPINode)> singleEdgeToBlock = new HashSet<(SPINode, SPINode)>();
                    singleEdgeToBlock.Add((junction, neighbor));
                    singleEdgeToBlock.Add((neighbor, junction));

                    BFSResult altBFS = RunBFS(neighbor, goalNode, singleEdgeToBlock);

                    if (altBFS.pathToGoal != null)
                    {
                        hasPathToGoal = true;
                    }
                    else
                    {
                        hasDeadEndPath = true;
                    }

                    if (hasDeadEndPath && hasPathToGoal)
                    {
                        Debug.Log($"[Maze] DeadEnd Path found at junction {junction.Id}");
                        foundAnyJunction = true;
                        break;
                    }
                }
            }

            return foundAnyJunction;
        }

        private BFSResult RunBFS(SPINode start, SPINode goal, HashSet<(SPINode, SPINode)> ignoreEdges = null)
        {
            var result = new BFSResult
            {
                reachable = new HashSet<SPINode>(),
                cameFrom = new Dictionary<SPINode, SPINode>(),
                pathToGoal = null
            };

            Queue<SPINode> queue = new Queue<SPINode>();
            queue.Enqueue(start);
            result.cameFrom[start] = null;
            result.reachable.Add(start);

            while (queue.Count > 0)
            {
                SPINode current = queue.Dequeue();

                foreach (var relation in current.walkableRelations)
                {
                    SPINode neighbor = relation.Node;

                    if (ignoreEdges != null && ignoreEdges.Contains((current, neighbor)))
                        continue;

                    if (!result.cameFrom.ContainsKey(neighbor))
                    {
                        result.cameFrom[neighbor] = current;
                        result.reachable.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (result.cameFrom.ContainsKey(goal))
            {
                var path = new List<SPINode>();
                SPINode current = goal;
                while (current != null)
                {
                    path.Add(current);
                    current = result.cameFrom[current];
                }
                path.Reverse();
                result.pathToGoal = path;
            }

            return result;
        }

        private string PathToString(List<SPINode> path)
        {
            List<string> ids = new List<string>();
            foreach (var node in path)
            {
                ids.Add(node.Id);
            }
            return string.Join(" -> ", ids);
        }

    }
}
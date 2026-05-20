using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    public class SFSAdapter : EditorWindow
    {

        private ScriptableObject sourceData;

        [MenuItem("SPI/SFS Sync Window")]
        public static void ShowWindow()
        {
            GetWindow<SFSAdapter>("SFS Sync");
        }

        private void OnGUI()
        {
            GUILayout.Label("SFS Sync Adapter", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            sourceData = (ScriptableObject)EditorGUILayout.ObjectField(
                "Source Data Asset:",
                sourceData,
                typeof(ScriptableObject),
                false);

            EditorGUILayout.Space();

            GUI.enabled = sourceData != null;

            if (GUILayout.Button("Sync Data", GUILayout.Height(30)))
            {
                SyncData(sourceData);
            }

            GUI.enabled = true;

            if (sourceData == null)
            {
                EditorGUILayout.HelpBox("Please assign a SpaceFoundationData to sync SPI with SFS Data.", MessageType.Info);
            }

        }

        private void SyncData(ScriptableObject data)
        {
            PatternGraphManager graphManager = Object.FindAnyObjectByType<PatternGraphManager>();

            if (graphManager == null)
            {
                Debug.LogError("SFSAdapter: No PatternGraphManager found in the scene!");
                return;
            }

            // Requires SFS Package
            SpaceFoundationData sfsData = data as SpaceFoundationData;
            if (sfsData == null)
            {
                Debug.LogError("SFSAdapter: Provided data is not of type SpaceFoundationData!");
                return;
            }

            graphManager.ClearGraph();

            var sceneAnchors = Object.FindObjectsByType<Anchor>(FindObjectsSortMode.None)
                             .ToDictionary(a => a.GetUniqueId(), a => a.gameObject);

            Dictionary<string, SPINode> nodeMap = new Dictionary<string, SPINode>();

            ProcessSPINodes(sfsData, nodeMap, sceneAnchors);

            ProcessWalkableRelations(nodeMap);

            ProcessLineOfSightRelations(nodeMap, sceneAnchors);

            List<SPINode> finalizedNodes = new List<SPINode>(nodeMap.Values);
            graphManager.SetNodes(finalizedNodes);

            EditorUtility.SetDirty(graphManager);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(graphManager.gameObject.scene);

            Debug.Log($"SFSAdapter: Successfully poured {finalizedNodes.Count} nodes into PatternGraphManager.");
        }

        private void ProcessWalkableRelations(Dictionary<string, SPINode> nodeMap)
        {
            var sceneWalkables = Object.FindObjectsByType<WalkableRelation>(FindObjectsSortMode.None);

            foreach (var walkRel in sceneWalkables)
            {
                if (walkRel.anchorA == null || walkRel.anchorB == null) continue;

                string idA = walkRel.anchorA.GetUniqueId();
                string idB = walkRel.anchorB.GetUniqueId();

                if (nodeMap.ContainsKey(idA) && nodeMap.ContainsKey(idB))
                {
                    var relationAttributes = walkRel.GetComponents<RelationAttribute>();

                    AddWalkableRelation(idA, idB, relationAttributes, nodeMap);
                    AddWalkableRelation(idB, idA, relationAttributes, nodeMap);
                }
                else
                {
                    Debug.LogWarning($"SFSAdapter: WalkableRelation {walkRel.GetUniqueId()} links missing anchors!");
                }
            }
        }

        private void AddWalkableRelation(string sourceId, string targetId, RelationAttribute[] attributes, Dictionary<string, SPINode> nodeMap)
        {
            var sourceNode = nodeMap[sourceId];

            if (!sourceNode.walkableRelations.Exists(r => r.NodeId == targetId))
            {
                var rel = new SPIWalkableRelation(targetId);
                if (attributes != null)
                {
                    rel.attributes.AddRange(attributes);
                }
                sourceNode.walkableRelations.Add(rel);
            }
        }

        private void ProcessLineOfSightRelations(Dictionary<string, SPINode> nodeMap, Dictionary<string, GameObject> sceneAnchors)
        {
            foreach (var entry in nodeMap)
            {
                string anchorId = entry.Key;
                SPINode node = entry.Value;

                if (!sceneAnchors.TryGetValue(anchorId, out GameObject anchorGO)) continue;

                var losComponents = anchorGO.GetComponents<LineOfSightRelation>();

                foreach (var losComp in losComponents)
                {
                    foreach (var targetId in losComp.GetRelations())
                    {
                        if (!nodeMap.ContainsKey(targetId))
                        {
                            Debug.LogWarning($"SFSAdapter: LoS target {targetId} on anchor {anchorId} has no matching node!");
                            continue;
                        }

                        var entryData = losComp.GetLoSEntry(targetId);

                        var existingRel = node.lineOfSightRelations.Find(r => r.NodeId == targetId);

                        if (existingRel == null)
                        {
                            node.lineOfSightRelations.Add(new SPILoSRelation(targetId));
                        }
                    }
                }
            }
        }

        private void ProcessSPINodes(SpaceFoundationData sfsData, Dictionary<string, SPINode> nodeMap, Dictionary<string, GameObject> sceneAnchors)
        {
            if (sfsData.anchors == null || sfsData.anchors.entries == null) return;

            foreach (var entry in sfsData.anchors.entries)
            {
                string anchorId = entry.Key;

                if (nodeMap.ContainsKey(anchorId)) continue;

                SPINode newNode = new SPINode(anchorId);

                if (sceneAnchors.TryGetValue(anchorId, out GameObject anchorGO))
                {
                    var foundAttributes = anchorGO.GetComponents<NodeAttribute>();
                    newNode.attributes.AddRange(foundAttributes);
                }
                else
                {
                    Debug.LogWarning($"SFSAdapter: Anchor ID {anchorId} exists in data but no matching GameObject found in scene!");
                }

                nodeMap.Add(anchorId, newNode);
            }
        }

        private void ProcessDelimiterCollection(SerializableDictionary<string, StringArrayWrapper> delimiterDict, Dictionary<string, SPINode> nodeMap)
        {
            if (delimiterDict == null || delimiterDict.entries == null) return;

            var sceneDelimiters = Object.FindObjectsByType<Delimiter>(FindObjectsSortMode.None)
                          .ToDictionary(d => d.GetUniqueId(), d => d.gameObject);


            foreach (var entry in delimiterDict.entries)
            {
                string delimiterId = entry.Key;
                string[] connectedAnchors = entry.Value.Array;

                if (connectedAnchors == null || connectedAnchors.Length < 2) continue;

                RelationAttribute[] delimiterAttributes = null;
                if (sceneDelimiters.TryGetValue(delimiterId, out GameObject delimiterGO))
                    delimiterAttributes = delimiterGO.GetComponents<RelationAttribute>();
                else
                    Debug.LogWarning($"SFSAdapter: Delimiter ID {delimiterId} exists in data but no matching GameObject found in scene!");

                for (int i = 0; i < connectedAnchors.Length; i++)
                {
                    for (int j = i + 1; j < connectedAnchors.Length; j++)
                    {
                        if (i == j) continue;

                        string sourceId = connectedAnchors[i];
                        string targetId = connectedAnchors[j];

                        if (!nodeMap.ContainsKey(sourceId) || !nodeMap.ContainsKey(targetId)) continue;

                        if (!nodeMap[sourceId].walkableRelations.Exists(r => r.NodeId == targetId))
                        {
                            var rel = new SPIWalkableRelation(targetId);
                            if (delimiterAttributes != null) rel.attributes.AddRange(delimiterAttributes);
                            nodeMap[sourceId].walkableRelations.Add(rel);
                        }

                        if (!nodeMap[targetId].walkableRelations.Exists(r => r.NodeId == sourceId))
                        {
                            var rel = new SPIWalkableRelation(sourceId);
                            if (delimiterAttributes != null) rel.attributes.AddRange(delimiterAttributes);
                            nodeMap[targetId].walkableRelations.Add(rel);
                        }

                    }
                }
            }
        }


    }
}

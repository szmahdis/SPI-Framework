using System.Collections.Generic;
using UnityEngine;


namespace StructrualPatternIdentifier
{
    public class PatternGraphManager : MonoBehaviour
    {
        public static PatternGraphManager Instance { get; private set; }

        [Header("Formalized Gamespace")]
        [SerializeField] private List<SPINode> nodes = new List<SPINode>();

        private Dictionary<string, SPINode> _nodeCache = new Dictionary<string, SPINode>();

        public List<SPINode> Nodes => nodes;

        private void Awake()
        {
            Instance = this;
            BuildCache();
        }
        public void SetNodes(List<SPINode> newNodes)
        {
            nodes = newNodes;
            BuildCache();
        }

        private void BuildCache()
        {
            _nodeCache.Clear();
            foreach (var node in nodes)
            {
                if (node != null && !string.IsNullOrEmpty(node.Id))
                    _nodeCache[node.Id] = node;
            }
        }

        public SPINode FindNodeById(string id)
        {
            if (_nodeCache.TryGetValue(id, out SPINode node)) return node;
            return null;
        }

        public List<SPINode> GetNodes()
        {
            return nodes;
        }

        public void ClearGraph()
        {
            nodes.Clear();
        }
    }
}

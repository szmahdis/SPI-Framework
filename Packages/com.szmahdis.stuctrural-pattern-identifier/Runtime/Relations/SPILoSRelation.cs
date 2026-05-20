using System;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    [Serializable]
    public class SPILoSRelation
    {
        [Tooltip("The ID of the target SPINode this LoS relation points to.")]
        [SerializeField] private string _nodeId;

        [NonSerialized] private SPINode _cachedNode;


        public SPILoSRelation(string nodeId)
        {
            _nodeId = nodeId;
        }

        public string NodeId => _nodeId;

        public SPINode Node
        {
            get
            {
                if (_cachedNode == null && !string.IsNullOrEmpty(_nodeId))
                    _cachedNode = PatternGraphManager.Instance.FindNodeById(_nodeId);
                return _cachedNode;
            }
        }
    }
}
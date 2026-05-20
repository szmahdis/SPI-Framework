using System;
using System.Collections.Generic;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    [Serializable]
    public class SPIWalkableRelation
    {
        [Tooltip("The ID of the target SPINode this relation connects to.")]
        [SerializeField] private string _nodeId;
        [NonSerialized] private SPINode _cachedNode;

        [Header("Attributes")]
        public List<RelationAttribute> attributes = new List<RelationAttribute>();

        public T GetAttribute<T>() where T : RelationAttribute
        {
            foreach (var attr in attributes)
                if (attr is T target) return target;
            return null;
        }
        public SPIWalkableRelation(string nodeId)
        {
            _nodeId = nodeId;
        }

        public SPINode Node
        {
            get
            {
                if (_cachedNode == null && !string.IsNullOrEmpty(_nodeId))
                    _cachedNode = PatternGraphManager.Instance.FindNodeById(_nodeId);
                return _cachedNode;
            }
        }

        public string NodeId => _nodeId;

    }
}


using System;
using System.Collections.Generic;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    [Serializable]
    public class SPINode
    {
        [Header("Node Identifier")]
        [SerializeField] private string id;

        public string Id
        {
            get => id;
            set => id = value;
        }
        public SPINode(string id) => this.id = id;

        [Header("Node")]
        public List<NodeAttribute> attributes = new List<NodeAttribute>();

        [Header("Relations")]
        public List<SPIWalkableRelation> walkableRelations = new List<SPIWalkableRelation>();
        public List<SPILoSRelation> lineOfSightRelations = new List<SPILoSRelation>();

        public T GetAttribute<T>() where T : NodeAttribute
        {
            foreach (var attr in attributes)
            {
                if (attr is T target) return target;
            }
            return null;
        }

        public SPILoSRelation GetLoSRelation(string targetId)
        {
            foreach (var los in lineOfSightRelations)
                if (los.NodeId == targetId) return los;
            return null;
        }
    }
}
using UnityEngine;

namespace StructrualPatternIdentifier
{
    [AddComponentMenu("SPI/Node Attributes/Gameplay Attribute")]
    public class GameplayAttribute : NodeAttribute
    {
        private enum NodeRole { None, Start, Goal, Reward }

        [Header("Cluster Assignment")]
        [Tooltip("-1 = auto-assign. Required for Start and Goal nodes, when sharing the same ID they are paired.")]
        public int clusterId = -1;

        [Header("Node Role")]
        [SerializeField] private NodeRole role = NodeRole.None;

        public bool IsStart => role == NodeRole.Start;
        public bool IsGoal => role == NodeRole.Goal;
        public bool IsReward => role == NodeRole.Reward;
        public bool IsNone => role == NodeRole.None;
    }
}
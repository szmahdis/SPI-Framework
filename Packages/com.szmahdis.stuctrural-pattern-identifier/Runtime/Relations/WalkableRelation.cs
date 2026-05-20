using UnityEngine;

namespace StructrualPatternIdentifier
{
    [ExecuteAlways]
    public class WalkableRelation : MonoBehaviour
    {

        [Header("Visualization")]
        [SerializeField] private bool _showGizmos = true;

        [SerializeField] private string id;
        public Anchor anchorA; // Requires SFS Package
        public Anchor anchorB;

        private Color edgeColor = Color.yellowGreen;

        private void Update()
        {
            PositionAtMidpoint();
        }

        private void PositionAtMidpoint()
        {
            if (anchorA == null || anchorB == null) return;

            Vector3 midpoint = (anchorA.transform.position + anchorB.transform.position) * 0.5f;

            if (transform.position != midpoint)
            {
                transform.position = midpoint;
            }
        }

        private void OnDrawGizmos()
        {
            if (anchorA == null || anchorB == null || !_showGizmos) return;

            edgeColor = Color.yellowGreen;

            Gizmos.color = edgeColor;
            Gizmos.DrawLine(anchorA.transform.position, anchorB.transform.position);
        }

        public string GetUniqueId()
        {
            return id;
        }

        public void SetUniqueId(string id)
        {
            this.id = id;
        }
    }
}
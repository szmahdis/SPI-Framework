using System.Collections.Generic;
using UnityEngine;

namespace StructrualPatternIdentifier
{
    [System.Serializable]
    public class LineOfSightEntry
    {
        //public Anchor target; // Requires SFS Package
    }

    [AddComponentMenu("SPI/Relations/Line of Sight Relation")]
    public class LineOfSightRelation : MonoBehaviour
    {
        [Header("Visualization")]
        [SerializeField] private bool _showGizmos = true;

        [Header("Data")]
        [SerializeField] private List<LineOfSightEntry> _entries = new();

        [Header("Gizmo Settings")]
        [SerializeField] private float _heightOffset = 0.5f;

        public List<string> GetRelations()
        {
            var result = new List<string>();
            foreach (var entry in _entries)
            {
                //if (entry.target == null) continue;
                //result.Add((entry.target.GetUniqueId()));
            }
            return result;
        }

        public LineOfSightEntry GetLoSEntry(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return null;
            return _entries.Find(e => e.target != null && e.target.GetUniqueId() == targetId);
        }

        private void OnDrawGizmos()
        {
            if (!_showGizmos || _entries == null || _entries.Count == 0) return;

            Vector3 offset = new Vector3(0, _heightOffset, 0);

            Vector3 startPos = transform.position + offset;

            foreach (var entry in _entries)
            {
                if (entry.target == null) continue;

                Gizmos.color = Color.cyan;

                Vector3 targetPos = entry.target.transform.position + offset;

                Gizmos.DrawLine(startPos, targetPos);

                Gizmos.DrawWireSphere(targetPos, 0.5f);
            }
        }
    }
}
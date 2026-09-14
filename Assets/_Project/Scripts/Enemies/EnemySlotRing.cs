using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Reservable positions around the player. Each chaser walks to its own slot instead of the
    // player's position, so a group spreads around the player rather than queueing on one path.
    // Enemies that don't fit on the inner ring wait on the outer ring until an inner slot frees up.
    public class EnemySlotRing : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("innerSlots")] int _innerSlots = 6;
        [SerializeField, FormerlySerializedAs("innerRadius")] float _innerRadius = 1.6f;
        [SerializeField, FormerlySerializedAs("outerSlots")] int _outerSlots = 10;
        [SerializeField, FormerlySerializedAs("outerRadius")] float _outerRadius = 3.8f;
        [Tooltip("A free inner slot must be this much closer before a chaser switches to it.")]
        [SerializeField, FormerlySerializedAs("switchMargin")] float _switchMargin = 1f;

        Vector3[] _offsets;
        EnemyChaser[] _ownerComponents;

        void Awake()
        {
            int total = _innerSlots + _outerSlots;
            _offsets = new Vector3[total];
            _ownerComponents = new EnemyChaser[total];
            for (int i = 0; i < _innerSlots; i++) _offsets[i] = RingOffset(i, _innerSlots, _innerRadius, 0f);
            for (int i = 0; i < _outerSlots; i++) _offsets[_innerSlots + i] = RingOffset(i, _outerSlots, _outerRadius, 0.5f);
        }

        static Vector3 RingOffset(int index, int count, float radius, float phase)
        {
            float angle = (index + phase) * Mathf.PI * 2f / count;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        }

        // Called on every repath. Keeps, upgrades or assigns the chaser's slot and returns where to walk.
        public Vector3 GetDestination(EnemyChaser chaser)
        {
            Vector3 from = chaser.transform.position;
            int current = Array.IndexOf(_ownerComponents, chaser);
            Vector3 currentPos = default;
            bool hasCurrent = current >= 0 && TryGetSlotPosition(current, out currentPos);
            if (current >= 0 && !hasCurrent)
            {
                _ownerComponents[current] = null; // slot ended up inside a wall
                current = -1;
            }

            bool onInner = hasCurrent && current < _innerSlots;
            float currentDist = hasCurrent ? Vector3.Distance(from, currentPos) : float.MaxValue;

            int best = FindNearestFree(0, _innerSlots, from, out Vector3 bestPos, out float bestDist);
            bool take = best >= 0 && (!onInner || bestDist < currentDist - _switchMargin);
            if (!take && !hasCurrent)
            {
                best = FindNearestFree(_innerSlots, _ownerComponents.Length, from, out bestPos, out _);
                take = best >= 0;
            }

            if (!take) return hasCurrent ? currentPos : transform.position;

            if (current >= 0) _ownerComponents[current] = null;
            _ownerComponents[best] = chaser;
            return bestPos;
        }

        public void Release(EnemyChaser chaser)
        {
            int index = Array.IndexOf(_ownerComponents, chaser);
            if (index < 0) return;
            _ownerComponents[index] = null;
        }

        int FindNearestFree(int start, int end, Vector3 from, out Vector3 position, out float distance)
        {
            int best = -1;
            position = default;
            distance = float.MaxValue;
            for (int i = start; i < end; i++)
            {
                if (_ownerComponents[i] != null) continue; // destroyed owners compare equal to null, so they count as free
                if (!TryGetSlotPosition(i, out Vector3 p)) continue;
                float d = Vector3.Distance(from, p);
                if (d >= distance) continue;
                best = i;
                position = p;
                distance = d;
            }
            return best;
        }

        bool TryGetSlotPosition(int index, out Vector3 position)
        {
            position = transform.position + _offsets[index];
            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 0.6f, NavMesh.AllAreas)) return false;
            position = hit.position;
            return true;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.8f);
            for (int i = 0; i < _innerSlots; i++)
                Gizmos.DrawWireSphere(transform.position + RingOffset(i, _innerSlots, _innerRadius, 0f), 0.2f);
            Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.5f);
            for (int i = 0; i < _outerSlots; i++)
                Gizmos.DrawWireSphere(transform.position + RingOffset(i, _outerSlots, _outerRadius, 0.5f), 0.2f);
        }
    }
}

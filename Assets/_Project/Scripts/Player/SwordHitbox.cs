using System;
using System.Collections.Generic;
using Branded.Combat;
using UnityEngine;

namespace Branded.Player
{
    // On the swing's hit frame, scans an arc in front of the player with OverlapSphere (no trigger tunneling).
    public class SwordHitbox : MonoBehaviour
    {
        [SerializeField] PlayerCombat combat;
        [SerializeField] PlayerAim aim;

        [Header("Hit")]
        [SerializeField] float damage = 25f;
        [SerializeField] LayerMask targetLayers;

        [Header("Shape")]
        [SerializeField] float reach = 1.3f;
        [SerializeField] float radius = 1.6f;
        [SerializeField, Range(0f, 360f)] float arcAngle = 170f;
        [SerializeField] float heightOffset = 1f;

        // Boons scale damage through this, never by touching damage.
        public float DamageMultiplier { get; set; } = 1f;

        // (targets hit, swing direction)
        public event Action<int, Vector3> HitLanded;
        // Once per target hit, for on-hit effects such as burn.
        public event Action<IDamageable> TargetHit;

        readonly HashSet<IDamageable> _hitThisSwing = new();

        void Awake()
        {
            if (!combat) combat = GetComponent<PlayerCombat>();
            if (!aim) aim = GetComponent<PlayerAim>();
        }

        void OnEnable()
        {
            combat.SwingStarted += ResetSwing;
            combat.SwingActive += Scan;
        }

        void OnDisable()
        {
            combat.SwingStarted -= ResetSwing;
            combat.SwingActive -= Scan;
        }

        void ResetSwing() => _hitThisSwing.Clear();

        void Scan()
        {
            Vector3 direction = aim.AimDirection;
            Vector3 origin = transform.position + Vector3.up * heightOffset;
            Collider[] hits = Physics.OverlapSphere(origin + direction * reach, radius, targetLayers, QueryTriggerInteraction.Collide);

            int landed = 0;
            foreach (var hit in hits)
            {
                if (hit.transform.IsChildOf(transform)) continue;

                Vector3 toTarget = hit.transform.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f && Vector3.Angle(direction, toTarget) > arcAngle * 0.5f) continue;

                var target = hit.GetComponentInParent<IDamageable>();
                if (target == null || !_hitThisSwing.Add(target)) continue;

                target.TakeDamage(damage * DamageMultiplier, toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : direction);
                TargetHit?.Invoke(target);
                landed++;
            }

            if (landed > 0) HitLanded?.Invoke(landed, direction);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 direction = aim ? aim.AimDirection : transform.forward;
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * heightOffset + direction * reach, radius);
        }
    }
}

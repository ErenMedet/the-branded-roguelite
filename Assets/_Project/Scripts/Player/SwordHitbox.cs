using System.Collections.Generic;
using Branded.Combat;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // On the swing's hit frame, scans an arc in front of the player with OverlapSphere (no trigger tunneling).
    public class SwordHitbox : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("combat")] PlayerCombat _combatComponent;
        [SerializeField, FormerlySerializedAs("aim")] PlayerAim _aimComponent;

        [Header("Hit")]
        [SerializeField, FormerlySerializedAs("damage")] float _damage = 25f;
        [SerializeField, FormerlySerializedAs("targetLayers")] LayerMask _targetLayers;

        [Header("Shape")]
        [SerializeField, FormerlySerializedAs("reach")] float _reach = 1.3f;
        [SerializeField, FormerlySerializedAs("radius")] float _radius = 1.6f;
        [SerializeField, Range(0f, 360f), FormerlySerializedAs("arcAngle")] float _arcAngle = 170f;
        [SerializeField, FormerlySerializedAs("heightOffset")] float _heightOffset = 1f;

        // Godo's forge raises the base damage through this; boons stack on top through DamageMultiplier.
        public float BaseDamageMultiplier { get; set; } = 1f;
        // Boons scale damage through this, never by touching _damage.
        public float DamageMultiplier { get; set; } = 1f;

        // (targets hit, swing direction)
        public event UnityAction<int, Vector3> HitLanded;
        // Once per target hit, for on-hit effects such as burn.
        public event UnityAction<IDamageable> TargetHit;

        readonly HashSet<IDamageable> _hitThisSwing = new();

        void Awake()
        {
            if (!_combatComponent) _combatComponent = GetComponent<PlayerCombat>();
            if (!_aimComponent) _aimComponent = GetComponent<PlayerAim>();
        }

        void OnEnable()
        {
            _combatComponent.SwingStarted += OnSwingStarted;
            _combatComponent.SwingActive += OnSwingActive;
        }

        void OnDisable()
        {
            _combatComponent.SwingStarted -= OnSwingStarted;
            _combatComponent.SwingActive -= OnSwingActive;
        }

        void OnSwingStarted() => _hitThisSwing.Clear();

        void OnSwingActive()
        {
            Vector3 direction = _aimComponent.AimDirection;
            Vector3 origin = transform.position + Vector3.up * _heightOffset;
            Collider[] hits = Physics.OverlapSphere(origin + direction * _reach, _radius, _targetLayers, QueryTriggerInteraction.Collide);

            int landed = 0;
            foreach (var hit in hits)
            {
                if (hit.transform.IsChildOf(transform)) continue;

                Vector3 toTarget = hit.transform.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f && Vector3.Angle(direction, toTarget) > _arcAngle * 0.5f) continue;

                var target = hit.GetComponentInParent<IDamageable>();
                if (target == null || !_hitThisSwing.Add(target)) continue;

                target.TakeDamage(_damage * BaseDamageMultiplier * DamageMultiplier, toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : direction);
                TargetHit?.Invoke(target);
                landed++;
            }

            if (landed > 0) HitLanded?.Invoke(landed, direction);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 direction = _aimComponent ? _aimComponent.AimDirection : transform.forward;
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * _heightOffset + direction * _reach, _radius);
        }
    }
}

using System.Collections.Generic;
using Branded.Combat;
using Branded.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // On the swing's hit frame, scans an arc in front of the player with OverlapSphere (no trigger tunneling).
    // Reach, radius, arc, knockback and impact come from the swing being played, so each combo step hits differently.
    // A spin scans a full circle every frame of its active phase, so a dash spin hits along the whole path.
    public class SwordHitbox : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("combat")] PlayerCombat _combatComponent;
        [SerializeField, FormerlySerializedAs("aim")] PlayerAim _aimComponent;

        [Header("Hit")]
        [SerializeField, FormerlySerializedAs("damage")] float _damage = 25f;
        [SerializeField, FormerlySerializedAs("targetLayers")] LayerMask _targetLayers;

        [Header("Shape")]
        [SerializeField, FormerlySerializedAs("heightOffset")] float _heightOffset = 1f;
        [SerializeField] float _spinRadius = 2.2f;

        // Godot's forge raises the base damage through this; boons stack on top through DamageMultiplier.
        public float BaseDamageMultiplier { get; set; } = 1f;
        // Boons scale damage through this, never by touching _damage.
        public float DamageMultiplier { get; set; } = 1f;

        // (targets hit, swing direction, impact). Raised once per swing, so a spin's repeated scans don't stack hitstops.
        public event UnityAction<int, Vector3, float> HitLanded;
        // Once per target hit, for on-hit effects such as burn.
        public event UnityAction<IDamageable> TargetHit;

        readonly HashSet<IDamageable> _hitThisSwing = new();
        bool _landedThisSwing;

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

        void Update()
        {
            if (!_combatComponent.IsSpin || _combatComponent.Phase != ESwingPhase.Active) return;
            Scan();
        }

        void OnSwingStarted()
        {
            _hitThisSwing.Clear();
            _landedThisSwing = false;
        }

        void OnSwingActive() => Scan();

        void Scan()
        {
            bool spin = _combatComponent.IsSpin;
            SwingData swing = _combatComponent.CurrentSwing;
            if (!spin && swing == null) return;

            Vector3 direction = _aimComponent.AimDirection;
            Vector3 origin = transform.position + Vector3.up * _heightOffset;
            Vector3 center = spin ? origin : origin + direction * swing.Reach;
            Collider[] hits = Physics.OverlapSphere(center, spin ? _spinRadius : swing.Radius, _targetLayers, QueryTriggerInteraction.Collide);

            float damage = _damage * BaseDamageMultiplier * DamageMultiplier * _combatComponent.AttackDamageMultiplier;
            float knockback = spin ? 1f : swing.Knockback;

            int landed = 0;
            foreach (var hit in hits)
            {
                if (hit.transform.IsChildOf(transform)) continue;

                if (!spin && !FlatMath.WithinArc(direction, hit.transform.position - transform.position, swing.ArcAngle)) continue;

                var target = hit.GetComponentInParent<IDamageable>();
                if (target == null || !_hitThisSwing.Add(target)) continue;

                target.TakeDamage(damage, FlatMath.FlatDirection(transform.position, hit.transform.position, direction), knockback);
                TargetHit?.Invoke(target);
                landed++;
            }

            if (landed == 0 || _landedThisSwing) return;
            _landedThisSwing = true;
            HitLanded?.Invoke(landed, direction, spin ? 1f : swing.Impact);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 direction = _aimComponent ? _aimComponent.AimDirection : transform.forward;
            Vector3 origin = transform.position + Vector3.up * _heightOffset;

            SwingData swing = _combatComponent ? _combatComponent.CurrentSwing : null;
            if (swing != null)
            {
                Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.6f);
                Gizmos.DrawWireSphere(origin + direction * swing.Reach, swing.Radius);
            }

            Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(origin, _spinRadius);
        }
    }
}

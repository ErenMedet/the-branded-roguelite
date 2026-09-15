using System.Collections;
using Branded.Combat;
using Branded.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Simple telegraphed melee: stop, show the warning, then hit if the player is still in reach.
    public class EnemyMeleeAttack : EnemyAttack
    {
        [SerializeField, FormerlySerializedAs("telegraph")] GameObject _telegraph;
        [SerializeField, FormerlySerializedAs("damage")] float _damage = 10f;
        [SerializeField, FormerlySerializedAs("windup")] float _windup = 0.55f;
        [SerializeField, FormerlySerializedAs("cooldown")] float _cooldown = 1.2f;
        [SerializeField, FormerlySerializedAs("hitRange")] float _hitRange = 2.1f;
        [SerializeField, Range(0f, 360f)] float _hitArc = 140f;
        [Tooltip("Hits still deal damage but don't stagger or cancel this attack (troll).")]
        [SerializeField] bool _superArmor;

        public override bool CanBeInterrupted => !_superArmor;

        protected override void Awake()
        {
            base.Awake();
            if (_telegraph) _telegraph.SetActive(false);
        }

        void Update()
        {
            TickCooldown();
            if (!IsReady || !_chaserComponent.InRange) return;
            StartCoroutine(Attack());
        }

        IEnumerator Attack()
        {
            StartAttack();
            if (_telegraph) _telegraph.SetActive(true);

            float t = 0f;
            while (t < _windup)
            {
                _chaserComponent.FaceTarget();
                t += Time.deltaTime;
                yield return null;
            }

            if (_telegraph) _telegraph.SetActive(false);
            TryHitTarget();
            FinishAttack(_cooldown);
        }

        void TryHitTarget()
        {
            Transform target = _chaserComponent.TargetComponent;
            if (!target) return;

            Vector3 toTarget = FlatMath.Flat(target.position - transform.position);
            if (toTarget.sqrMagnitude > _hitRange * _hitRange) return;
            if (!FlatMath.WithinArc(transform.forward, toTarget, _hitArc)) return;

            var damageable = target.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage, toTarget.normalized);
        }

        // Called when the enemy gets staggered.
        public override void Interrupt()
        {
            if (!IsAttacking) return;
            StopAllCoroutines();
            if (_telegraph) _telegraph.SetActive(false);
            FinishAttack(_cooldown);
        }

        void OnDisable() => Interrupt();
    }
}

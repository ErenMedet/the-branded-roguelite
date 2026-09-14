using System.Collections;
using Branded.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Simple telegraphed melee: stop, show the warning, then hit if the player is still in reach.
    public class EnemyMeleeAttack : EnemyAttack
    {
        [SerializeField, FormerlySerializedAs("chaser")] EnemyChaser _chaserComponent;
        [SerializeField, FormerlySerializedAs("telegraph")] GameObject _telegraph;
        [SerializeField, FormerlySerializedAs("damage")] float _damage = 10f;
        [SerializeField, FormerlySerializedAs("windup")] float _windup = 0.55f;
        [SerializeField, FormerlySerializedAs("cooldown")] float _cooldown = 1.2f;
        [SerializeField, FormerlySerializedAs("hitRange")] float _hitRange = 2.1f;
        [SerializeField, Range(0f, 360f)] float _hitArc = 140f;
        [Tooltip("Hits still deal damage but don't stagger or cancel this attack (troll).")]
        [SerializeField] bool _superArmor;

        public override bool CanBeInterrupted => !_superArmor;

        float _cooldownTimer;

        void Awake()
        {
            if (!_chaserComponent) _chaserComponent = GetComponent<EnemyChaser>();
            if (_telegraph) _telegraph.SetActive(false);
        }

        void Update()
        {
            _cooldownTimer -= Time.deltaTime;
            if (IsAttacking || _cooldownTimer > 0f || _chaserComponent.Halted || !_chaserComponent.InRange) return;
            StartCoroutine(Attack());
        }

        IEnumerator Attack()
        {
            IsAttacking = true;
            _chaserComponent.Halted = true;
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
            Finish();
        }

        void TryHitTarget()
        {
            Transform target = _chaserComponent.TargetComponent;
            if (!target) return;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > _hitRange * _hitRange) return;
            if (Vector3.Angle(transform.forward, toTarget) > _hitArc * 0.5f) return;

            var damageable = target.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage, toTarget.normalized);
        }

        // Called when the enemy gets staggered.
        public override void Interrupt()
        {
            if (!IsAttacking) return;
            StopAllCoroutines();
            if (_telegraph) _telegraph.SetActive(false);
            Finish();
        }

        void Finish()
        {
            IsAttacking = false;
            _chaserComponent.Halted = false;
            _cooldownTimer = _cooldown;
        }

        void OnDisable() => Interrupt();
    }
}

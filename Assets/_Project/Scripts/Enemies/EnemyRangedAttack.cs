using System.Collections;
using Branded.Combat;
using Branded.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Stops, charges a shot for a moment (glow at the hand), then fires a projectile at where the player stands.
    public class EnemyRangedAttack : EnemyAttack
    {
        [SerializeField, FormerlySerializedAs("projectilePrefab")] Projectile _projectilePrefabComponent;
        [SerializeField, FormerlySerializedAs("muzzle")] Transform _muzzleComponent;
        [SerializeField, FormerlySerializedAs("telegraph")] GameObject _telegraph;
        [SerializeField, FormerlySerializedAs("range")] float _range = 12f;
        [SerializeField, FormerlySerializedAs("windup")] float _windup = 0.6f;
        [SerializeField, FormerlySerializedAs("cooldown")] float _cooldown = 2.2f;
        [SerializeField, FormerlySerializedAs("damage")] float _damage = 10f;

        protected override void Awake()
        {
            base.Awake();
            if (_telegraph) _telegraph.SetActive(false);
            CooldownTimer = _cooldown * Random.Range(0.3f, 1f);
        }

        void Update()
        {
            TickCooldown();
            if (!IsReady || _chaserComponent.DistanceToTarget > _range || !_chaserComponent.HasLineOfSight()) return;
            StartCoroutine(Shoot());
        }

        IEnumerator Shoot()
        {
            StartAttack();
            if (_telegraph) _telegraph.SetActive(true);
            for (float t = 0f; t < _windup; t += Time.deltaTime)
            {
                _chaserComponent.FaceTarget();
                yield return null;
            }
            if (_telegraph) _telegraph.SetActive(false);

            Vector3 aim = FlatMath.Flat(_chaserComponent.TargetComponent.position - _muzzleComponent.position);
            if (aim.sqrMagnitude > 0.01f)
            {
                var projectile = Instantiate(_projectilePrefabComponent, _muzzleComponent.position, Quaternion.LookRotation(aim));
                projectile.Launch(_damage);
            }
            FinishAttack(_cooldown);
        }

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

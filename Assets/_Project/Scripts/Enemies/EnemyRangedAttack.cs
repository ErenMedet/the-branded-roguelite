using System.Collections;
using Branded.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Stops, charges a shot for a moment (glow at the hand), then fires a projectile at where the player stands.
    public class EnemyRangedAttack : EnemyAttack
    {
        [SerializeField, FormerlySerializedAs("chaser")] EnemyChaser _chaserComponent;
        [SerializeField, FormerlySerializedAs("projectilePrefab")] Projectile _projectilePrefabComponent;
        [SerializeField, FormerlySerializedAs("muzzle")] Transform _muzzleComponent;
        [SerializeField, FormerlySerializedAs("telegraph")] GameObject _telegraph;
        [SerializeField, FormerlySerializedAs("range")] float _range = 12f;
        [SerializeField, FormerlySerializedAs("windup")] float _windup = 0.6f;
        [SerializeField, FormerlySerializedAs("cooldown")] float _cooldown = 2.2f;
        [SerializeField, FormerlySerializedAs("damage")] float _damage = 10f;

        float _cooldownTimer;

        void Awake()
        {
            if (!_chaserComponent) _chaserComponent = GetComponent<EnemyChaser>();
            if (_telegraph) _telegraph.SetActive(false);
            _cooldownTimer = _cooldown * Random.Range(0.3f, 1f);
        }

        void Update()
        {
            _cooldownTimer -= Time.deltaTime;
            if (IsAttacking || _cooldownTimer > 0f || _chaserComponent.Halted || !_chaserComponent.TargetComponent) return;
            if (_chaserComponent.DistanceToTarget > _range || !_chaserComponent.HasLineOfSight()) return;
            StartCoroutine(Shoot());
        }

        IEnumerator Shoot()
        {
            IsAttacking = true;
            _chaserComponent.Halted = true;
            if (_telegraph) _telegraph.SetActive(true);
            for (float t = 0f; t < _windup; t += Time.deltaTime)
            {
                _chaserComponent.FaceTarget();
                yield return null;
            }
            if (_telegraph) _telegraph.SetActive(false);

            Vector3 aim = _chaserComponent.TargetComponent.position - _muzzleComponent.position;
            aim.y = 0f;
            if (aim.sqrMagnitude > 0.01f)
            {
                var projectile = Instantiate(_projectilePrefabComponent, _muzzleComponent.position, Quaternion.LookRotation(aim));
                projectile.Launch(_damage);
            }
            Finish();
        }

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

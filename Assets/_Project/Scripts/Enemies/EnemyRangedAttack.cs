using System.Collections;
using UnityEngine;

namespace Branded.Enemies
{
    // Stops, charges a shot for a moment (glow at the hand), then fires a projectile at where the player stands.
    public class EnemyRangedAttack : EnemyAttack
    {
        [SerializeField] EnemyChaser chaser;
        [SerializeField] EnemyProjectile projectilePrefab;
        [SerializeField] Transform muzzle;
        [SerializeField] GameObject telegraph;
        [SerializeField] float range = 12f;
        [SerializeField] float windup = 0.6f;
        [SerializeField] float cooldown = 2.2f;
        [SerializeField] float damage = 10f;

        float _cooldownTimer;

        void Awake()
        {
            if (!chaser) chaser = GetComponent<EnemyChaser>();
            if (telegraph) telegraph.SetActive(false);
            _cooldownTimer = cooldown * Random.Range(0.3f, 1f);
        }

        void Update()
        {
            _cooldownTimer -= Time.deltaTime;
            if (IsAttacking || _cooldownTimer > 0f || chaser.Halted || !chaser.Target) return;
            if (chaser.DistanceToTarget <= range && chaser.HasLineOfSight()) StartCoroutine(Shoot());
        }

        IEnumerator Shoot()
        {
            IsAttacking = true;
            chaser.Halted = true;
            if (telegraph) telegraph.SetActive(true);
            for (float t = 0f; t < windup; t += Time.deltaTime)
            {
                chaser.FaceTarget();
                yield return null;
            }
            if (telegraph) telegraph.SetActive(false);

            Vector3 aim = chaser.Target.position - muzzle.position;
            aim.y = 0f;
            if (aim.sqrMagnitude > 0.01f)
            {
                var projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(aim));
                projectile.Launch(damage);
            }
            Finish();
        }

        public override void Interrupt()
        {
            if (!IsAttacking) return;
            StopAllCoroutines();
            if (telegraph) telegraph.SetActive(false);
            Finish();
        }

        void Finish()
        {
            IsAttacking = false;
            chaser.Halted = false;
            _cooldownTimer = cooldown;
        }

        void OnDisable() => Interrupt();
    }
}

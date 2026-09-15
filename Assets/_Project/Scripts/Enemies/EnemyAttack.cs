using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Base for enemy attacks, so hit reactions can cancel whichever one is running.
    // Also owns the bookkeeping every attack shares: halting the chaser while attacking, and the cooldown.
    public abstract class EnemyAttack : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("chaser")] protected EnemyChaser _chaserComponent;

        public bool IsAttacking { get; private set; }

        // False while the attack has super armor: hits still deal damage but don't stagger or cancel it.
        public virtual bool CanBeInterrupted => true;

        protected float CooldownTimer { get; set; }

        // Not attacking, off cooldown, not halted by something else (e.g. a stagger), and has a target.
        protected bool IsReady => !IsAttacking && CooldownTimer <= 0f && !_chaserComponent.Halted && _chaserComponent.TargetComponent;

        protected virtual void Awake()
        {
            if (!_chaserComponent) _chaserComponent = GetComponent<EnemyChaser>();
        }

        public abstract void Interrupt();

        protected void TickCooldown() => CooldownTimer -= Time.deltaTime;

        protected void StartAttack()
        {
            IsAttacking = true;
            _chaserComponent.Halted = true;
        }

        protected void FinishAttack(float cooldown)
        {
            IsAttacking = false;
            _chaserComponent.Halted = false;
            CooldownTimer = cooldown;
        }
    }
}

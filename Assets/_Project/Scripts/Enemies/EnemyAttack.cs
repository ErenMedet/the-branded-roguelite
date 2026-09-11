using UnityEngine;

namespace Branded.Enemies
{
    // Base for enemy attacks so hit reactions can cancel whichever one is running.
    public abstract class EnemyAttack : MonoBehaviour
    {
        public bool IsAttacking { get; protected set; }

        // False while the attack has super armor: hits still deal damage but don't stagger or cancel it.
        public virtual bool CanBeInterrupted => true;

        public abstract void Interrupt();
    }
}

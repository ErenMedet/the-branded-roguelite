using UnityEngine;

namespace Branded.Combat
{
    // Anything that can be hit: enemies, breakable pots, explosive barrels.
    // The sword never needs to know which one it hit.
    public interface IDamageable
    {
        // knockback scales how far the hit shoves the target: a heavy finisher sends it further than a light swing.
        void TakeDamage(float amount, Vector3 hitDirection, float knockback = 1f);
    }
}

using UnityEngine;

namespace Branded.Combat
{
    // Anything that can be hit: enemies, breakable pots, explosive barrels.
    // The sword never needs to know which one it hit.
    public interface IDamageable
    {
        void TakeDamage(float amount, Vector3 hitDirection);
    }
}

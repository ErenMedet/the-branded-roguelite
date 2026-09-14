using UnityEngine;

namespace Branded.Combat
{
    // Lets a component on the same GameObject turn a hit away based on where it came from (e.g. a shield).
    // Unlike IInvulnerabilitySource it sees the hit, so it can also react to it, like a shield breaking.
    public interface IDamageBlocker
    {
        bool Blocks(float amount, Vector3 hitDirection);
    }
}

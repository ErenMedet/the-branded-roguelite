using Branded.Combat;
using Branded.Meta;
using UnityEngine;

namespace Branded.Enemies
{
    // Demon ash for a kill, banked at once so it is kept even if the player dies later.
    // Evaporating at dawn isn't a kill and gives nothing.
    public class EnemyAshReward : MonoBehaviour
    {
        [SerializeField] HealthComponent health;
        [SerializeField] int ashes = 1;

        void Awake()
        {
            if (!health) health = GetComponent<HealthComponent>();
        }

        void OnEnable() => health.OnDeath += Reward;
        void OnDisable() => health.OnDeath -= Reward;

        void Reward() => Progress.AddAshes(ashes);
    }
}

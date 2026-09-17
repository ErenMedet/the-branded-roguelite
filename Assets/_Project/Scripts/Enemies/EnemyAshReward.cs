using Branded.Combat;
using Branded.Meta;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Enemies
{
    // Demon ash for a kill, banked at once so it is kept even if the player dies later.
    // Evaporating at dawn isn't a kill and gives nothing.
    public class EnemyAshReward : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("health")] HealthComponent _healthComponent;
        [SerializeField, FormerlySerializedAs("ashes")] int _ashes = 1;

        void Awake()
        {
            if (!_healthComponent) _healthComponent = GetComponent<HealthComponent>();
        }

        // The spawner pays a pack's whole purse out of its threat, so the serialized value is only
        // a fallback for enemies placed by hand in a scene.
        public void SetAshes(int ashes) => _ashes = ashes;

        void OnEnable() => _healthComponent.Died += OnDied;
        void OnDisable() => _healthComponent.Died -= OnDied;

        void OnDied() => Progress.AddAshes(_ashes);
    }
}

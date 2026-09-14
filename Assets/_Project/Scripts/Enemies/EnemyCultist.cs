using Branded.Combat;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Branded.Enemies
{
    // Stays behind the crowd and brings back only the escorts it spawned with. A fallen escort's soul drifts to
    // the cultist and, if it arrives, that enemy rises again beside it. Killing the soul or the cultist ends it.
    public class EnemyCultist : MonoBehaviour
    {
        [SerializeField] ReviveSoul _soulPrefabComponent;
        [SerializeField] float _soulHeight = 1f;
        [SerializeField] float _reviveSpread = 1.5f;

        // The revived enemy, so the spawner can count it again.
        public event UnityAction<GameObject> EscortRevived;

        public void Bind(HealthComponent escort, GameObject prefab)
        {
            // The escort is destroyed right after dying, so this lambda never needs removing.
            escort.Died += () => OnEscortDied(escort.transform.position, prefab);
        }

        void OnEscortDied(Vector3 corpse, GameObject prefab)
        {
            // The escort can die after the cultist is already gone or shut down; then it stays dead.
            if (this == null || !isActiveAndEnabled || !_soulPrefabComponent) return;
            var soul = Instantiate(_soulPrefabComponent, corpse + Vector3.up * _soulHeight, Quaternion.identity);
            soul.Launch(this, prefab);
        }

        public void Revive(GameObject prefab)
        {
            if (!isActiveAndEnabled || !prefab) return;

            Vector3 point = transform.position + new Vector3(Random.Range(-_reviveSpread, _reviveSpread), 0f, Random.Range(-_reviveSpread, _reviveSpread));
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 3f, NavMesh.AllAreas)) point = hit.position;

            var escort = Instantiate(prefab, point, Quaternion.identity);
            if (escort.TryGetComponent(out HealthComponent health)) Bind(health, prefab);
            EscortRevived?.Invoke(escort);
        }
    }
}

using System.Collections;
using Branded.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Test-only: keeps the given enemy types alive (a dead enemy respawns as the same type) so combat can be tuned.
    // Replaced by waves in Aşama 3.
    public class TestEnemySpawner : MonoBehaviour
    {
        [SerializeField] GameObject[] enemyPrefabs;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] int copiesEach = 1;
        [SerializeField] float respawnDelay = 1.5f;

        void Start()
        {
            for (int i = 0; i < copiesEach; i++)
                foreach (var prefab in enemyPrefabs)
                    Spawn(prefab);
        }

        void Spawn(GameObject prefab)
        {
            if (!prefab || spawnPoints == null || spawnPoints.Length == 0) return;

            Vector3 point = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            point += new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 3f, NavMesh.AllAreas)) point = hit.position;

            var enemy = Instantiate(prefab, point, Quaternion.identity);
            if (enemy.TryGetComponent(out HealthComponent health))
                health.OnDeath += () => StartCoroutine(RespawnLater(prefab));
        }

        IEnumerator RespawnLater(GameObject prefab)
        {
            yield return new WaitForSeconds(respawnDelay);
            Spawn(prefab);
        }
    }
}

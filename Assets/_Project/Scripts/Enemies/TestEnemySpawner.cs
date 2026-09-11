using System.Collections;
using Branded.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Test-only: keeps a fixed number of enemies alive so combat feel can be tuned. Replaced by waves in Aşama 3.
    public class TestEnemySpawner : MonoBehaviour
    {
        [SerializeField] GameObject enemyPrefab;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] int aliveCount = 4;
        [SerializeField] float respawnDelay = 1.5f;

        void Start()
        {
            for (int i = 0; i < aliveCount; i++) Spawn();
        }

        void Spawn()
        {
            if (!enemyPrefab || spawnPoints == null || spawnPoints.Length == 0) return;

            Vector3 point = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            point += new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 3f, NavMesh.AllAreas)) point = hit.position;

            var enemy = Instantiate(enemyPrefab, point, Quaternion.identity);
            if (enemy.TryGetComponent(out HealthComponent health))
                health.OnDeath += () => StartCoroutine(RespawnLater());
        }

        IEnumerator RespawnLater()
        {
            yield return new WaitForSeconds(respawnDelay);
            Spawn();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Branded.Combat;
using Branded.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Loop
{
    // Spawns a night's waves at spawn points away from the player. A wave comes after its delay, or sooner once
    // the field has been clear for a moment; after the last wave it keeps repeating that one until dawn.
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] float minPlayerDistance = 9f;
        [SerializeField] float clearedWaveDelay = 3f;
        [SerializeField] float spawnInterval = 0.3f;
        [SerializeField] int maxAlive = 12;

        readonly List<HealthComponent> _alive = new();
        Coroutine _run;
        Transform _player;

        public int AliveCount => _alive.Count;

        void Awake()
        {
            var player = GameObject.FindWithTag("Player");
            if (player) _player = player.transform;
        }

        public void Begin(NightData night)
        {
            Stop();
            if (night.waves == null || night.waves.Length == 0) return;
            _run = StartCoroutine(Run(night));
        }

        public void Stop()
        {
            if (_run != null) StopCoroutine(_run);
            _run = null;
        }

        // Dawn: whatever is still alive leaves without dying, along with shots in flight.
        public void EvaporateAll()
        {
            foreach (var health in _alive)
                if (health && health.TryGetComponent(out EnemyDeath death)) death.Evaporate(Random.Range(0f, 0.6f));
            _alive.Clear();
            foreach (var projectile in FindObjectsByType<EnemyProjectile>())
                Destroy(projectile.gameObject);
        }

        IEnumerator Run(NightData night)
        {
            for (int i = 0; ; i = Mathf.Min(i + 1, night.waves.Length - 1))
            {
                Wave wave = night.waves[i];
                float wait = wave.delay;
                float clearFor = 0f;
                while (wait > 0f && clearFor < clearedWaveDelay)
                {
                    wait -= Time.deltaTime;
                    clearFor = _alive.Count == 0 ? clearFor + Time.deltaTime : 0f;
                    yield return null;
                }

                foreach (var group in wave.groups)
                    for (int n = 0; n < group.count; n++)
                    {
                        while (_alive.Count >= maxAlive) yield return null;
                        Spawn(group.prefab);
                        yield return new WaitForSeconds(spawnInterval);
                    }
            }
        }

        void Spawn(GameObject prefab)
        {
            if (!prefab || spawnPoints == null || spawnPoints.Length == 0) return;

            Vector3 point = PickSpawnPoint() + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 3f, NavMesh.AllAreas)) point = hit.position;

            var enemy = Instantiate(prefab, point, Quaternion.identity);
            if (!enemy.TryGetComponent(out HealthComponent health)) return;
            _alive.Add(health);
            health.OnDeath += () => _alive.Remove(health);
        }

        // Random start so the same point isn't always picked; the first one far enough from the player wins.
        Vector3 PickSpawnPoint()
        {
            int start = Random.Range(0, spawnPoints.Length);
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform point = spawnPoints[(start + i) % spawnPoints.Length];
                if (!_player || Vector3.Distance(point.position, _player.position) >= minPlayerDistance) return point.position;
            }
            return spawnPoints[start].position;
        }
    }
}

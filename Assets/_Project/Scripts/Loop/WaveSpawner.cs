using System.Collections;
using System.Collections.Generic;
using Branded.Combat;
using Branded.Enemies;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    // Spawns a night's waves at spawn points away from the player. A wave comes after its delay, or sooner once
    // the field has been clear for a moment; after the last wave it keeps repeating that one until dawn.
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("spawnPoints")] Transform[] _spawnPointComponents;
        [SerializeField, FormerlySerializedAs("minPlayerDistance")] float _minPlayerDistance = 9f;
        [SerializeField, FormerlySerializedAs("clearedWaveDelay")] float _clearedWaveDelay = 3f;
        [SerializeField, FormerlySerializedAs("spawnInterval")] float _spawnInterval = 0.3f;
        [SerializeField, FormerlySerializedAs("maxAlive")] int _maxAlive = 12;

        readonly List<HealthComponent> _aliveHealthComponents = new();
        Coroutine _run;
        Transform _playerTransformComponent;

        public int AliveCount => _aliveHealthComponents.Count;

        void Awake()
        {
            var player = GameObject.FindWithTag("Player");
            if (!player) return;
            _playerTransformComponent = player.transform;
        }

        public void Begin(NightData night)
        {
            Stop();
            if (night.Waves == null || night.Waves.Length == 0) return;
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
            foreach (var health in _aliveHealthComponents)
                if (health && health.TryGetComponent(out EnemyDeath death)) death.Evaporate(Random.Range(0f, 0.6f));
            _aliveHealthComponents.Clear();
            foreach (var projectile in FindObjectsByType<Projectile>())
                Destroy(projectile.gameObject);
            foreach (var zone in FindObjectsByType<DamageZone>())
                Destroy(zone.gameObject);
        }

        IEnumerator Run(NightData night)
        {
            for (int i = 0; ; i = Mathf.Min(i + 1, night.Waves.Length - 1))
            {
                Wave wave = night.Waves[i];
                float wait = wave.Delay;
                float clearFor = 0f;
                while (wait > 0f && clearFor < _clearedWaveDelay)
                {
                    wait -= Time.deltaTime;
                    clearFor = _aliveHealthComponents.Count == 0 ? clearFor + Time.deltaTime : 0f;
                    yield return null;
                }

                foreach (var group in wave.Groups)
                    for (int n = 0; n < group.Count; n++)
                    {
                        while (_aliveHealthComponents.Count >= _maxAlive) yield return null;
                        if (_spawnPointComponents == null || _spawnPointComponents.Length == 0) yield break;
                        var leader = Spawn(group.Prefab, PickSpawnPoint());
                        if (leader && group.Escort) SpawnEscorts(leader, group);
                        yield return new WaitForSeconds(_spawnInterval);
                    }
            }
        }

        GameObject Spawn(GameObject prefab, Vector3 around)
        {
            if (!prefab) return null;

            Vector3 point = around + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 3f, NavMesh.AllAreas)) point = hit.position;

            var enemy = Instantiate(prefab, point, Quaternion.identity);
            Track(enemy);
            return enemy;
        }

        // Escorts arrive with their leader even past the alive cap, so a group never shows up split.
        void SpawnEscorts(GameObject leader, SpawnGroup group)
        {
            leader.TryGetComponent(out EnemyCultist cultist);
            // The cultist is destroyed along with its event, so this never needs removing.
            if (cultist) cultist.EscortRevived += Track;

            for (int i = 0; i < group.EscortCount; i++)
            {
                var escort = Spawn(group.Escort, leader.transform.position);
                if (cultist && escort && escort.TryGetComponent(out HealthComponent health)) cultist.Bind(health, group.Escort);
            }
        }

        void Track(GameObject enemy)
        {
            if (!enemy.TryGetComponent(out HealthComponent health)) return;
            _aliveHealthComponents.Add(health);
            // The enemy is destroyed right after dying, so this lambda never needs removing.
            health.Died += () => _aliveHealthComponents.Remove(health);
        }

        // Random start so the same point isn't always picked; the first one far enough from the player wins.
        Vector3 PickSpawnPoint()
        {
            int start = Random.Range(0, _spawnPointComponents.Length);
            for (int i = 0; i < _spawnPointComponents.Length; i++)
            {
                Transform point = _spawnPointComponents[(start + i) % _spawnPointComponents.Length];
                if (!_playerTransformComponent || Vector3.Distance(point.position, _playerTransformComponent.position) >= _minPlayerDistance) return point.position;
            }
            return _spawnPointComponents[start].position;
        }
    }
}

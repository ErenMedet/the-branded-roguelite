using System.Collections;
using System.Collections.Generic;
using Branded.Combat;
using Branded.Enemies;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    // Keeps the field's total threat near a target instead of dropping fixed waves, like Hades' encounter budget
    // (base + ramp per depth, capped active enemies). The target grows every night, builds up through the night
    // and drifts up and down, so the fight swells and eases without emptying or piling past the alive cap.
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("spawnPoints")] Transform[] _spawnPointComponents;
        [SerializeField, FormerlySerializedAs("minPlayerDistance")] float _minPlayerDistance = 9f;
        [SerializeField, FormerlySerializedAs("spawnInterval")] float _spawnInterval = 0.3f; // between members of a pack
        [SerializeField] float _packInterval = 1.2f;

        [Header("Difficulty per night")]
        [SerializeField] float _threatBase = 8f;
        [SerializeField] float _threatRamp = 6f;
        [SerializeField] int _maxAliveBase = 12;
        [SerializeField] int _maxAliveRamp = 4;
        [SerializeField] int _maxAliveCap = 40;

        [Header("Pacing within a night")]
        [SerializeField, Range(0f, 1f)] float _openingShare = 0.6f; // the target starts at this share of the night's threat and reaches it by dawn
        [SerializeField, Range(0f, 1f)] float _swellAmount = 0.25f;
        [SerializeField] float _swellPeriod = 20f;

        readonly Dictionary<HealthComponent, float> _threatByHealthComponents = new();
        Coroutine _run;
        Transform _playerTransformComponent;

        public int AliveCount => _threatByHealthComponents.Count;
        public float AliveThreat { get; private set; }
        public float TargetThreat { get; private set; }

        void Awake()
        {
            var player = GameObject.FindWithTag("Player");
            if (!player) return;
            _playerTransformComponent = player.transform;
        }

        public void Begin(NightData night, int nightNumber)
        {
            Stop();
            if (night.Pool == null || night.Pool.Length == 0) return;
            if (_spawnPointComponents == null || _spawnPointComponents.Length == 0) return;
            _run = StartCoroutine(Run(night, nightNumber));
        }

        public void Stop()
        {
            if (_run != null) StopCoroutine(_run);
            _run = null;
        }

        // Dawn: whatever is still alive leaves without dying, along with shots in flight.
        public void EvaporateAll()
        {
            foreach (var health in _threatByHealthComponents.Keys)
                if (health && health.TryGetComponent(out EnemyDeath death)) death.Evaporate(Random.Range(0f, 0.6f));
            _threatByHealthComponents.Clear();
            AliveThreat = 0f;
            foreach (var projectile in FindObjectsByType<Projectile>())
                Destroy(projectile.gameObject);
            foreach (var zone in FindObjectsByType<DamageZone>())
                Destroy(zone.gameObject);
        }

        IEnumerator Run(NightData night, int nightNumber)
        {
            float startedAt = Time.time;
            float noiseSeed = Random.Range(0f, 1000f); // a different swell every night
            float nightThreat = _threatBase + (nightNumber - 1) * _threatRamp;
            int maxAlive = Mathf.Min(_maxAliveBase + (nightNumber - 1) * _maxAliveRamp, _maxAliveCap);

            while (true)
            {
                float elapsed = Time.time - startedAt;
                float buildUp = Mathf.Lerp(_openingShare, 1f, elapsed / night.Duration);
                float swell = 1f + _swellAmount * (Mathf.PerlinNoise(elapsed / _swellPeriod, noiseSeed) * 2f - 1f);
                TargetThreat = nightThreat * buildUp * swell;

                SpawnGroup group = AliveThreat < TargetThreat ? PickGroup(night.Pool, TargetThreat - AliveThreat, maxAlive - AliveCount) : null;
                if (group == null)
                {
                    yield return null;
                    continue;
                }

                yield return SpawnPack(group);
                // The emptier the field, the sooner the next pack, so killing fast never opens a long lull.
                yield return new WaitForSeconds(_packInterval * Mathf.Lerp(0.3f, 1f, AliveThreat / TargetThreat));
            }
        }

        // Weighted pick among packs within the missing threat. When none is that cheap the cheapest still comes,
        // so a gap smaller than every pack never leaves the field waiting.
        SpawnGroup PickGroup(SpawnGroup[] pool, float missingThreat, int freeSlots)
        {
            SpawnGroup cheapest = null;
            float totalWeight = 0f;
            foreach (var group in pool)
            {
                if (!Fits(group, freeSlots)) continue;
                if (cheapest == null || group.Threat < cheapest.Threat) cheapest = group;
                if (group.Threat <= missingThreat) totalWeight += group.Weight;
            }
            if (totalWeight <= 0f) return cheapest;

            float roll = Random.Range(0f, totalWeight);
            foreach (var group in pool)
            {
                if (!Fits(group, freeSlots) || group.Threat > missingThreat) continue;
                roll -= group.Weight;
                if (roll <= 0f) return group;
            }
            return cheapest;
        }

        static bool Fits(SpawnGroup group, int freeSlots) => group.Prefab && group.Count > 0 && group.Count <= freeSlots;

        // A pack comes from one spawn point so it reads as a group; the next pack picks its own.
        IEnumerator SpawnPack(SpawnGroup group)
        {
            Vector3 point = PickSpawnPoint();
            int members = group.Count * (1 + (group.Escort ? group.EscortCount : 0));
            float threatEach = group.Threat / members;

            for (int n = 0; n < group.Count; n++)
            {
                var leader = Spawn(group.Prefab, point, threatEach);
                if (leader && group.Escort) SpawnEscorts(leader, group, threatEach);
                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        GameObject Spawn(GameObject prefab, Vector3 around, float threat)
        {
            if (!prefab) return null;

            Vector3 point = around + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 3f, NavMesh.AllAreas)) point = hit.position;

            var enemy = Instantiate(prefab, point, Quaternion.identity);
            Track(enemy, threat);
            return enemy;
        }

        // Escorts arrive with their leader even past the alive cap, so a group never shows up split.
        void SpawnEscorts(GameObject leader, SpawnGroup group, float threat)
        {
            leader.TryGetComponent(out EnemyCultist cultist);
            // The cultist is destroyed along with its event, so this never needs removing.
            if (cultist) cultist.EscortRevived += revived => Track(revived, threat);

            for (int i = 0; i < group.EscortCount; i++)
            {
                var escort = Spawn(group.Escort, leader.transform.position, threat);
                if (cultist && escort && escort.TryGetComponent(out HealthComponent health)) cultist.Bind(health, group.Escort);
            }
        }

        void Track(GameObject enemy, float threat)
        {
            if (!enemy.TryGetComponent(out HealthComponent health)) return;
            _threatByHealthComponents[health] = threat;
            AliveThreat += threat;
            // The enemy is destroyed right after dying, so this lambda never needs removing.
            health.Died += () => Untrack(health);
        }

        void Untrack(HealthComponent health)
        {
            if (!_threatByHealthComponents.Remove(health, out float threat)) return;
            AliveThreat = Mathf.Max(0f, AliveThreat - threat);
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

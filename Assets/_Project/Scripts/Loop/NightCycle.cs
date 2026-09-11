using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Loop
{
    public enum CyclePhase { Night, Dawn, Morning, Dusk }

    // The GDD rhythm: a night of waves, dawn clears the field, the morning campfire waits, and resting at it
    // lets the next night fall. Aşama 4 hooks dialogue and the boon choice into the morning.
    public class NightCycle : MonoBehaviour
    {
        [SerializeField] NightData[] nights; // past the last entry, the last night repeats
        [SerializeField] WaveSpawner spawner;
        [SerializeField] DayNightLighting lighting;
        [SerializeField] Campfire campfirePrefab;
        [SerializeField] float campfireDistance = 3f;
        [SerializeField] float dawnDuration = 4f;
        [SerializeField] float duskDuration = 2.5f;

        public CyclePhase Phase { get; private set; }
        public int NightNumber { get; private set; }
        public float TimeUntilDawn => Phase == CyclePhase.Night ? Mathf.Max(0f, _dawnAt - Time.time) : 0f;

        public event Action<CyclePhase> PhaseChanged;

        Transform _player;
        float _dawnAt;

        void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player) _player = player.transform;
            lighting.Apply(0f);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            for (NightNumber = 1; ; NightNumber++)
            {
                NightData night = nights[Mathf.Min(NightNumber, nights.Length) - 1];
                _dawnAt = Time.time + night.duration;
                SetPhase(CyclePhase.Night);
                spawner.Begin(night);
                yield return new WaitUntil(() => Time.time >= _dawnAt);

                SetPhase(CyclePhase.Dawn);
                spawner.Stop();
                spawner.EvaporateAll();
                yield return lighting.BlendTo(1f, dawnDuration);

                SetPhase(CyclePhase.Morning);
                var campfire = Instantiate(campfirePrefab, CampfirePosition(), Quaternion.identity);
                bool rested = false;
                campfire.Rested += () => rested = true;
                yield return new WaitUntil(() => rested);

                SetPhase(CyclePhase.Dusk);
                yield return lighting.BlendTo(0f, duskDuration);
                Destroy(campfire.gameObject);
            }
        }

        void SetPhase(CyclePhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        // A few steps from the player, toward the middle of the map so it never lands against a wall.
        Vector3 CampfirePosition()
        {
            Vector3 origin = _player ? _player.position : transform.position;
            Vector3 toCenter = transform.position - origin;
            toCenter.y = 0f;
            Vector3 direction = toCenter.sqrMagnitude > 1f ? toCenter.normalized : Vector3.forward;
            Vector3 spot = origin + direction * campfireDistance;
            return NavMesh.SamplePosition(spot, out NavMeshHit hit, 2f, NavMesh.AllAreas) ? hit.position : spot;
        }
    }
}

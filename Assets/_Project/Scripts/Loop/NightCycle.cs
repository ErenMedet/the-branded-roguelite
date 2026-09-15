using System.Collections;
using Branded.Boons;
using Branded.Core;
using Branded.Dialogue;
using Branded.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    // The GDD rhythm: a night of waves, dawn clears the field, then the morning camp: rest at the campfire,
    // talk, pick a boon, and the next night falls.
    public class NightCycle : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("nights")] NightData[] _nights; // past the last entry its pool repeats, while the spawner's difficulty keeps ramping
        [SerializeField, FormerlySerializedAs("spawner")] WaveSpawner _spawnerComponent;
        [SerializeField, FormerlySerializedAs("lighting")] DayNightLighting _lightingComponent;
        [SerializeField, FormerlySerializedAs("campfirePrefab")] Campfire _campfirePrefabComponent;
        [SerializeField, FormerlySerializedAs("dialogue")] DialogueManager _dialogueComponent;
        [SerializeField, FormerlySerializedAs("boonChoice")] BoonChoiceUI _boonChoiceComponent;
        [SerializeField, FormerlySerializedAs("hud")] GameObject _hud; // hidden during the camp so the portrait gets the corner
        [SerializeField, FormerlySerializedAs("campfireDistance")] float _campfireDistance = 3f;
        [SerializeField, FormerlySerializedAs("dawnDuration")] float _dawnDuration = 4f;
        [SerializeField, FormerlySerializedAs("duskDuration")] float _duskDuration = 2.5f;

        public ECyclePhase Phase { get; private set; }
        public int NightNumber { get; private set; }
        public float TimeUntilDawn => Phase == ECyclePhase.Night ? Mathf.Max(0f, _dawnAt - Time.time) : 0f;

        public event UnityAction<ECyclePhase> PhaseChanged;

        Transform _playerTransformComponent;
        PlayerBoons _boonsComponent;
        float _dawnAt;
        bool _rested;

        void OnEnable() => GameEvents.CampfireRested += OnCampfireRested;
        void OnDisable() => GameEvents.CampfireRested -= OnCampfireRested;

        void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player)
            {
                _playerTransformComponent = player.transform;
                _boonsComponent = player.GetComponent<PlayerBoons>();
            }
            _lightingComponent.Apply(0f);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            for (NightNumber = 1; ; NightNumber++)
            {
                NightData night = _nights[Mathf.Min(NightNumber, _nights.Length) - 1];
                _dawnAt = Time.time + night.Duration;
                SetPhase(ECyclePhase.Night);
                _spawnerComponent.Begin(night, NightNumber);
                yield return new WaitUntil(() => Time.time >= _dawnAt);

                SetPhase(ECyclePhase.Dawn);
                _spawnerComponent.Stop();
                _spawnerComponent.EvaporateAll();
                yield return _lightingComponent.BlendTo(1f, _dawnDuration);

                SetPhase(ECyclePhase.Morning);
                var campfire = Instantiate(_campfirePrefabComponent, CampfirePosition(), Quaternion.identity);
                _rested = false;
                yield return new WaitUntil(() => _rested);
                yield return Camp(night);

                SetPhase(ECyclePhase.Dusk);
                yield return _lightingComponent.BlendTo(0f, _duskDuration);
                Destroy(campfire.gameObject);
            }
        }

        void OnCampfireRested() => _rested = true;

        // Dialogue, then the boon choice. CampStarted locks the player's input meanwhile.
        IEnumerator Camp(NightData night)
        {
            GameEvents.RaiseCampStarted();
            if (_hud) _hud.SetActive(false);
            if (_dialogueComponent) yield return _dialogueComponent.Play(night.MorningDialogue);
            if (_boonChoiceComponent && _boonsComponent)
                yield return _boonChoiceComponent.Choose(_boonsComponent.PickOffers(night.BoonPool, _boonChoiceComponent.Capacity), _boonsComponent.Add);
            if (_hud) _hud.SetActive(true);
            GameEvents.RaiseCampEnded();
        }

        void SetPhase(ECyclePhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        // A few steps from the player, toward the middle of the map so it never lands against a wall.
        Vector3 CampfirePosition()
        {
            Vector3 origin = _playerTransformComponent ? _playerTransformComponent.position : transform.position;
            Vector3 toCenter = FlatMath.Flat(transform.position - origin);
            Vector3 direction = toCenter.sqrMagnitude > 1f ? toCenter.normalized : Vector3.forward;
            Vector3 spot = origin + direction * _campfireDistance;
            return NavMesh.SamplePosition(spot, out NavMeshHit hit, 2f, NavMesh.AllAreas) ? hit.position : spot;
        }
    }
}

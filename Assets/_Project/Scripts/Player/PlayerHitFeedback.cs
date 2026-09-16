using Branded.Combat;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Hit feel for the Dragonslayer: hitstop + camera shake whenever the sword connects.
    public class PlayerHitFeedback : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("hitbox")] SwordHitbox _hitboxComponent;
        [SerializeField, FormerlySerializedAs("impulse")] CinemachineImpulseSource _impulseComponent;
        [SerializeField, FormerlySerializedAs("hitStopDuration")] float _hitStopDuration = 0.07f;
        [Tooltip("How long time takes to climb back to normal after the freeze.")]
        [SerializeField] float _hitStopRamp = 0.06f;
        [SerializeField, FormerlySerializedAs("shakeForce")] float _shakeForce = 0.4f;

        [Header("Crowd")]
        [Tooltip("Extra share of the freeze and shake for every target past the first.")]
        [SerializeField] float _perExtraTarget = 0.25f;
        [Tooltip("Ceiling on that bonus, so a wide sweep never stalls the fight.")]
        [SerializeField] float _maxCrowdScale = 1.75f;

        void Awake()
        {
            if (!_hitboxComponent) _hitboxComponent = GetComponent<SwordHitbox>();
            if (!_impulseComponent) _impulseComponent = GetComponent<CinemachineImpulseSource>();
            // Let the shake play during the hitstop freeze instead of after it.
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        }

        void OnEnable() => _hitboxComponent.HitLanded += OnHitLanded;
        void OnDisable() => _hitboxComponent.HitLanded -= OnHitLanded;

        // impact comes from the swing: the heavy finisher freezes and shakes harder than a light chain hit.
        // Cleaving a group adds to that, so a crowded sweep lands heavier than a single clip.
        void OnHitLanded(int targetCount, Vector3 direction, float impact)
        {
            float crowd = Mathf.Min(1f + (targetCount - 1) * _perExtraTarget, _maxCrowdScale);
            float weight = impact * crowd;

            HitStop.Trigger(_hitStopDuration * weight, _hitStopRamp);
            if (!_impulseComponent) return;
            _impulseComponent.GenerateImpulseWithVelocity(direction * (_shakeForce * weight));
        }
    }
}

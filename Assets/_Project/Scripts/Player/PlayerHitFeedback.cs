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
        [SerializeField, FormerlySerializedAs("shakeForce")] float _shakeForce = 0.4f;

        void Awake()
        {
            if (!_hitboxComponent) _hitboxComponent = GetComponent<SwordHitbox>();
            if (!_impulseComponent) _impulseComponent = GetComponent<CinemachineImpulseSource>();
            // Let the shake play during the hitstop freeze instead of after it.
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        }

        void OnEnable() => _hitboxComponent.HitLanded += OnHitLanded;
        void OnDisable() => _hitboxComponent.HitLanded -= OnHitLanded;

        void OnHitLanded(int targetCount, Vector3 direction)
        {
            HitStop.Trigger(_hitStopDuration);
            if (!_impulseComponent) return;
            _impulseComponent.GenerateImpulseWithVelocity(direction * _shakeForce);
        }
    }
}

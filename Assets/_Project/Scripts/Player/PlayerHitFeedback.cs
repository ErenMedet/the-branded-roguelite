using Branded.Combat;
using Unity.Cinemachine;
using UnityEngine;

namespace Branded.Player
{
    // Hit feel for the Dragonslayer: hitstop + camera shake whenever the sword connects.
    public class PlayerHitFeedback : MonoBehaviour
    {
        [SerializeField] SwordHitbox hitbox;
        [SerializeField] CinemachineImpulseSource impulse;
        [SerializeField] float hitStopDuration = 0.07f;
        [SerializeField] float shakeForce = 0.4f;

        void Awake()
        {
            if (!hitbox) hitbox = GetComponent<SwordHitbox>();
            if (!impulse) impulse = GetComponent<CinemachineImpulseSource>();
            // Let the shake play during the hitstop freeze instead of after it.
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        }

        void OnEnable() => hitbox.HitLanded += OnHitLanded;
        void OnDisable() => hitbox.HitLanded -= OnHitLanded;

        void OnHitLanded(int targetCount, Vector3 direction)
        {
            HitStop.Trigger(hitStopDuration);
            if (impulse) impulse.GenerateImpulseWithVelocity(direction * shakeForce);
        }
    }
}

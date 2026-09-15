using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Greybox only: sweeps the sword pivot through an arc (or a full circle for the spin) following PlayerCombat's phases.
    // Replaced by real animation once the character model arrives.
    public class SwordSwingVisual : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("combat")] PlayerCombat _combatComponent;
        [SerializeField, FormerlySerializedAs("swordPivot")] Transform _swordPivotComponent;
        [SerializeField, FormerlySerializedAs("restAngle")] float _restAngle = 110f;
        [SerializeField, FormerlySerializedAs("arcAngle")] float _arcAngle = 160f;

        void Awake()
        {
            if (!_combatComponent) _combatComponent = GetComponentInParent<PlayerCombat>();
        }

        void LateUpdate()
        {
            if (!_combatComponent || !_swordPivotComponent) return;

            float yaw = _combatComponent.IsSpin ? SpinYaw() : SwingYaw();
            _swordPivotComponent.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        float SwingYaw()
        {
            float start = -_arcAngle * 0.5f;
            float end = _arcAngle * 0.5f;
            float t = _combatComponent.PhaseProgress;

            return _combatComponent.Phase switch
            {
                ESwingPhase.Windup => Mathf.Lerp(_restAngle, start, t),
                ESwingPhase.Active => Mathf.Lerp(start, end, t),
                ESwingPhase.Recovery => Mathf.Lerp(end, _restAngle, t * t),
                _ => _restAngle,
            };
        }

        // A full turn ends back at rest, so windup and recovery just hold the rest angle.
        float SpinYaw()
        {
            if (_combatComponent.Phase != ESwingPhase.Active) return _restAngle;
            return _restAngle + 360f * _combatComponent.PhaseProgress;
        }
    }
}

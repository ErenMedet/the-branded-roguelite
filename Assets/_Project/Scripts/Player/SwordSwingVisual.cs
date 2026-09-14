using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Greybox only: sweeps the sword pivot through an arc following PlayerCombat's phases.
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

            float start = -_arcAngle * 0.5f;
            float end = _arcAngle * 0.5f;
            float t = _combatComponent.PhaseProgress;

            float yaw = _combatComponent.Phase switch
            {
                PlayerCombat.ESwingPhase.Windup => Mathf.Lerp(_restAngle, start, t),
                PlayerCombat.ESwingPhase.Active => Mathf.Lerp(start, end, t),
                PlayerCombat.ESwingPhase.Recovery => Mathf.Lerp(end, _restAngle, t * t),
                _ => _restAngle,
            };

            _swordPivotComponent.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}

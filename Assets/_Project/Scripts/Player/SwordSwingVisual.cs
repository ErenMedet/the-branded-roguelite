using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Greybox only: sweeps the sword pivot through the angles of the swing PlayerCombat is playing.
    // Sideways swings turn the pivot (yaw), the finisher falls from overhead (pitch), a charge pulls it back.
    // Replaced by real animation once the character model arrives.
    public class SwordSwingVisual : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("combat")] PlayerCombat _combatComponent;
        [SerializeField, FormerlySerializedAs("swordPivot")] Transform _swordPivotComponent;
        [SerializeField, FormerlySerializedAs("restAngle")] float _restAngle = 110f;

        [Header("Charge")]
        [SerializeField] float _chargeAngle = 175f;
        [SerializeField] float _chargeShakeAngle = 3f;

        [Header("Overhead")]
        [SerializeField] float _overheadRaise = -100f;
        [SerializeField] float _overheadEnd = 25f;

        void Awake()
        {
            if (!_combatComponent) _combatComponent = GetComponentInParent<PlayerCombat>();
        }

        void LateUpdate()
        {
            if (!_combatComponent || !_swordPivotComponent) return;
            _swordPivotComponent.localRotation = CurrentRotation();
        }

        Quaternion CurrentRotation()
        {
            if (_combatComponent.IsSpin) return Quaternion.Euler(0f, SpinYaw(), 0f);
            if (_combatComponent.Phase == ESwingPhase.Charging) return Quaternion.Euler(0f, ChargeYaw(), 0f);

            SwingData swing = _combatComponent.CurrentSwing;
            if (swing == null) return Quaternion.Euler(0f, _restAngle, 0f);

            return swing.Overhead
                ? Quaternion.Euler(OverheadPitch(swing), 0f, 0f)
                : Quaternion.Euler(0f, SwingYaw(swing), 0f);
        }

        float SwingYaw(SwingData swing)
        {
            float t = _combatComponent.PhaseProgress;

            return _combatComponent.Phase switch
            {
                ESwingPhase.Windup => Mathf.Lerp(_restAngle, swing.VisualFrom, t),
                ESwingPhase.Active => Mathf.Lerp(swing.VisualFrom, swing.VisualTo, t),
                ESwingPhase.Recovery => Mathf.Lerp(swing.VisualTo, _restAngle, t * t),
                _ => _restAngle,
            };
        }

        // The blade rises behind the head, then falls in front along the aim.
        float OverheadPitch(SwingData swing)
        {
            float t = _combatComponent.PhaseProgress;

            return _combatComponent.Phase switch
            {
                ESwingPhase.Windup => Mathf.Lerp(0f, _overheadRaise, t),
                ESwingPhase.Active => Mathf.Lerp(_overheadRaise, _overheadEnd, t),
                ESwingPhase.Recovery => Mathf.Lerp(_overheadEnd, 0f, t * t),
                _ => 0f,
            };
        }

        // Pulled further back the longer it is held, trembling once it is full.
        float ChargeYaw()
        {
            float t = _combatComponent.ChargeProgress;
            float yaw = Mathf.Lerp(_restAngle, _chargeAngle, t);
            if (t < 1f) return yaw;
            return yaw + Mathf.Sin(Time.unscaledTime * 40f) * _chargeShakeAngle;
        }

        // A full turn ends back at rest, so windup and recovery just hold the rest angle.
        float SpinYaw()
        {
            if (_combatComponent.Phase != ESwingPhase.Active) return _restAngle;
            return _restAngle + 360f * _combatComponent.PhaseProgress;
        }
    }
}

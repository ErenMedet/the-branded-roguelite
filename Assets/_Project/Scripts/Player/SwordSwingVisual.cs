using Branded.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Greybox only: poses the sword pivot for the swing PlayerCombat is playing.
    // The pose is carried between phases and between swings instead of being rebuilt from a rest angle,
    // so a chain flows as one motion: each phase starts from where the last one left the blade.
    // Replaced by real animation once the character model arrives.
    public class SwordSwingVisual : MonoBehaviour
    {
        // Share of the sweep the blade carries past its end angle at an overshoot of 1.
        const float OvershootShare = 0.12f;
        // Opening speed of each curve as a multiple of its average, so a windup and its strike
        // are compared at the moment the eye actually judges them: the frame each one starts.
        const float OutCubicPeakRate = 3f;
        const float OutExpoPeakRate = 6.93f;

        [SerializeField, FormerlySerializedAs("combat")] PlayerCombat _combatComponent;
        [SerializeField, FormerlySerializedAs("swordPivot")] Transform _swordPivotComponent;

        [Header("Guard")]
        [SerializeField, FormerlySerializedAs("restAngle")] float _restAngle = 110f;
        [Tooltip("Tip lowered at rest, so the drift back to guard does not read as a flat sideways sweep.")]
        [SerializeField] float _restPitch = 20f;
        [Tooltip("How long the blade takes to drift back to guard once the chain is over.")]
        [SerializeField] float _returnDuration = 0.45f;

        [Header("Windup")]
        [Tooltip("Ceiling on the windup's opening speed as a share of the strike's. Above 1 the windup outruns its own swing.")]
        [SerializeField, Range(0.1f, 1f)] float _windupSpeedRatio = 0.35f;

        [Header("Charge")]
        [SerializeField] float _chargeAngle = 175f;
        [SerializeField] float _chargeShakeAngle = 3f;
        [Tooltip("How fast the blade travels into the charge pose, in degrees per second.")]
        [SerializeField] float _chargeRaiseSpeed = 600f;

        [Header("Overhead")]
        [SerializeField] float _overheadRaise = -100f;
        [SerializeField] float _overheadEnd = 25f;

        Quaternion _pose;
        Quaternion _phaseFrom;
        ESwingPhase _lastPhase = ESwingPhase.None;
        SwingData _lastSwing;
        bool _lastSpin;
        float _returnTimer;

        void Awake()
        {
            if (!_combatComponent) _combatComponent = GetComponentInParent<PlayerCombat>();
            _pose = RestPose();
            _phaseFrom = _pose;
        }

        void LateUpdate()
        {
            if (!_combatComponent || !_swordPivotComponent) return;

            TrackPhaseChange();
            _pose = CurrentPose(Time.deltaTime);
            _swordPivotComponent.localRotation = _pose;
        }

        // Every phase carries on from the pose the last one left, so the blade never jumps.
        void TrackPhaseChange()
        {
            ESwingPhase phase = _combatComponent.Phase;
            SwingData swing = _combatComponent.CurrentSwing;
            bool spin = _combatComponent.IsSpin;
            if (phase == _lastPhase && swing == _lastSwing && spin == _lastSpin) return;

            _phaseFrom = _pose;
            _lastPhase = phase;
            _lastSwing = swing;
            _lastSpin = spin;
            if (phase == ESwingPhase.None) _returnTimer = 0f;
        }

        Quaternion CurrentPose(float dt)
        {
            if (_combatComponent.IsSpin) return SpinPose();
            if (_combatComponent.Phase == ESwingPhase.Charging) return ChargePose(dt);

            SwingData swing = _combatComponent.CurrentSwing;
            if (swing == null) return ReturnToGuard(dt);

            float t = _combatComponent.PhaseProgress;
            switch (_combatComponent.Phase)
            {
                case ESwingPhase.Windup:
                    return WindupPose(swing, t);
                case ESwingPhase.Active:
                    return StrikePose(swing, t);
                case ESwingPhase.Recovery:
                    // Only the overshoot is reeled back in; the guard is not reclaimed until the chain drops.
                    return Quaternion.Slerp(_phaseFrom, SwingPose(swing, EndAngle(swing)), Easing.OutCubic(t));
                default:
                    return ReturnToGuard(dt);
            }
        }

        // A windup may never outrun the strike it is loading, or the blade reads as teleporting before it swings.
        // The ceiling shortens how far the blade loads instead of clipping how fast it travels: a clipped
        // windup crosses its whole arc at one flat rate, which is the robotic look the easing exists to kill.
        Quaternion WindupPose(SwingData swing, float t)
        {
            Quaternion loaded = SwingPose(swing, StartAngle(swing));
            float share = ReachableShare(swing, loaded);
            if (share < 1f) loaded = Quaternion.Slerp(_phaseFrom, loaded, share);

            // Falling short is safe: the strike starts from wherever the windup actually reached.
            return Quaternion.Slerp(_phaseFrom, loaded, Easing.OutCubic(t));
        }

        // The sweep is applied as a signed delta rather than slerped to an end pose, because Slerp always
        // takes the short way round: a swing wider than half a turn would cut behind the player while the
        // hitbox scanned in front of him. Starting from _phaseFrom also keeps a shortened windup honest.
        Quaternion StrikePose(SwingData swing, float t)
        {
            float sweep = (FollowThroughAngle(swing) - StartAngle(swing)) * Easing.OutExpo(t);
            return _phaseFrom * SwingPose(swing, sweep);
        }

        // How much of the load the windup can take on before its opening speed passes the ceiling.
        float ReachableShare(SwingData swing, Quaternion loaded)
        {
            if (swing.Windup <= 0f || swing.Active <= 0f) return 1f;
            float travel = Quaternion.Angle(_phaseFrom, loaded);
            if (travel <= 0.01f) return 1f;

            float strikePeak = Mathf.Abs(FollowThroughAngle(swing) - StartAngle(swing)) / swing.Active * OutExpoPeakRate;
            float windupPeak = travel / swing.Windup * OutCubicPeakRate;
            return Mathf.Clamp01(strikePeak * _windupSpeedRatio / windupPeak);
        }

        // Between chains the blade drifts back slowly enough never to read as another swing, and any
        // new swing interrupts the drift from wherever it got to.
        Quaternion ReturnToGuard(float dt)
        {
            if (_returnDuration <= 0f) return RestPose();
            _returnTimer += dt;
            return Quaternion.Slerp(_phaseFrom, RestPose(), Easing.InOutQuad(Mathf.Clamp01(_returnTimer / _returnDuration)));
        }

        // Pulled further back the longer it is held, trembling once it is full. The charge can begin
        // anywhere in a chain, so the blade travels into the pose instead of appearing in it.
        Quaternion ChargePose(float dt)
        {
            float t = _combatComponent.ChargeProgress;
            float eased = Easing.OutCubic(t);
            Quaternion target = Quaternion.Euler(Mathf.Lerp(_restPitch, 0f, eased), Mathf.Lerp(_restAngle, _chargeAngle, eased), 0f);
            if (t >= 1f) target *= Quaternion.Euler(0f, Mathf.Sin(Time.unscaledTime * 40f) * _chargeShakeAngle, 0f);
            return Quaternion.RotateTowards(_pose, target, _chargeRaiseSpeed * dt);
        }

        // A full turn cannot be reached with Slerp, since 360 degrees is the identity rotation: it is applied as a delta.
        Quaternion SpinPose()
        {
            float t = _combatComponent.PhaseProgress;
            if (_combatComponent.Phase != ESwingPhase.Active) return Quaternion.Slerp(_phaseFrom, RestPose(), Easing.OutCubic(t));
            return _phaseFrom * Quaternion.Euler(0f, 360f * Easing.InOutQuad(t), 0f);
        }

        // An overhead swing falls on the centre line, so its angle is a pitch where a level one is a yaw.
        // Used both as an absolute pose and, in StrikePose, as a delta along the same axis.
        Quaternion SwingPose(SwingData swing, float angle) =>
            swing.Overhead ? Quaternion.Euler(angle, 0f, 0f) : Quaternion.Euler(0f, angle, 0f);

        float StartAngle(SwingData swing) => swing.Overhead ? _overheadRaise : swing.VisualFrom;
        float EndAngle(SwingData swing) => swing.Overhead ? _overheadEnd : swing.VisualTo;

        // A real swing does not stop on its target angle: it carries past and the recovery reels it back in.
        float FollowThroughAngle(SwingData swing)
        {
            float from = StartAngle(swing);
            float to = EndAngle(swing);
            return to + (to - from) * OvershootShare * swing.VisualOvershoot;
        }

        Quaternion RestPose() => Quaternion.Euler(_restPitch, _restAngle, 0f);
    }
}

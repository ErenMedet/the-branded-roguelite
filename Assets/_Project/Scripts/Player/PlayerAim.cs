using Branded.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Aims along the third-person camera's line of sight and turns Visual_Holder.
    // Souls-style: the body follows where it walks, and only commits to the camera's
    // direction when combat calls TurnToAim on an attack or SnapToAim on a shot.
    public class PlayerAim : MonoBehaviour
    {
        const float MinMoveSqrSpeed = 0.04f;
        const float SnapAngle = 3f; // below this the committed turn is not worth easing
        static readonly Vector3 ViewportCentre = new Vector3(0.5f, 0.5f, 0f);

        [SerializeField, FormerlySerializedAs("motor")] PlayerMotor _motorComponent;
        [Tooltip("Visual_Holder: only the visuals rotate, never Player_Root.")]
        [SerializeField, FormerlySerializedAs("visual")] Transform _visualComponent;
        [Tooltip("Hard cap on how fast the body may turn, in degrees per second.")]
        [SerializeField, FormerlySerializedAs("turnSpeed")] float _turnSpeed = 1440f;
        [Tooltip("How eagerly free facing chases the walk direction. Higher snaps sooner, lower drifts.")]
        [SerializeField] float _turnSharpness = 16f;

        [Header("Aim ray")]
        [Tooltip("Height above the player the shots fly at. Match the muzzle, or the crosshair lies about range.")]
        [SerializeField] float _aimHeight = 1.2f;
        [SerializeField] float _maxAimDistance = 60f;

        public Vector3 AimPoint { get; private set; }
        public Vector3 AimDirection { get; private set; } = Vector3.forward;

        // Set by combat while a swing is committed to a direction.
        public bool RotationLocked { get; set; }

        Camera _cameraComponent;
        Quaternion _turnFrom;
        Quaternion _turnTo;
        float _turnTimer;
        float _turnDuration;

        void Awake()
        {
            if (!_motorComponent) _motorComponent = GetComponent<PlayerMotor>();
        }

        void Update()
        {
            UpdateAimPoint();
            if (!_visualComponent) return;
            if (TickCommittedTurn()) return;
            if (RotationLocked) return;

            Vector3 facing = FreeFacing();
            if (facing.sqrMagnitude < 0.0001f) return;

            // Eased approach with a hard cap: the body decelerates as it lines up instead of
            // sweeping at one rate and stopping dead.
            Quaternion target = Quaternion.LookRotation(facing, Vector3.up);
            float blend = 1f - Mathf.Exp(-_turnSharpness * Time.deltaTime);
            Quaternion eased = Quaternion.Slerp(_visualComponent.rotation, target, blend);
            _visualComponent.rotation = Quaternion.RotateTowards(_visualComponent.rotation, eased, _turnSpeed * Time.deltaTime);
        }

        // Commits to the aim over a duration (an attack's windup) instead of teleporting the body.
        public void TurnToAim(float duration)
        {
            if (!_visualComponent) return;

            Quaternion target = Quaternion.LookRotation(AimDirection, Vector3.up);
            if (duration <= 0f || Quaternion.Angle(_visualComponent.rotation, target) <= SnapAngle)
            {
                _visualComponent.rotation = target;
                _turnDuration = 0f;
                return;
            }

            _turnFrom = _visualComponent.rotation;
            _turnTo = target;
            _turnTimer = 0f;
            _turnDuration = duration;
        }

        public void SnapToAim()
        {
            if (!_visualComponent) return;
            _turnDuration = 0f;
            _visualComponent.rotation = Quaternion.LookRotation(AimDirection, Vector3.up);
        }

        // True while a committed turn owns the rotation this frame.
        bool TickCommittedTurn()
        {
            if (_turnDuration <= 0f) return false;

            _turnTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_turnTimer / _turnDuration);
            _visualComponent.rotation = Quaternion.Slerp(_turnFrom, _turnTo, Easing.OutCubic(t));
            if (t >= 1f) _turnDuration = 0f;
            return true;
        }

        // Flat direction from an off-centre point (the arm muzzle) to the aim point, so shots land on the crosshair.
        // A cursor right on the muzzle (within half a unit) gives no usable direction, so the body's aim is used.
        public Vector3 DirectionFrom(Vector3 origin)
        {
            Vector3 direction = FlatMath.Flat(AimPoint - origin);
            return direction.sqrMagnitude > 0.25f ? direction.normalized : AimDirection;
        }

        // Standing still keeps the current facing, so the body doesn't snap back while the camera orbits.
        Vector3 FreeFacing()
        {
            if (!_motorComponent) return AimDirection;
            if (_motorComponent.IsDashing) return _motorComponent.DashDirection;

            Vector3 velocity = FlatMath.Flat(_motorComponent.PlanarVelocity);
            return velocity.sqrMagnitude > MinMoveSqrSpeed ? velocity.normalized : Vector3.zero;
        }

        void UpdateAimPoint()
        {
            if (!_cameraComponent) _cameraComponent = Camera.main;
            if (!_cameraComponent) return;

            Transform cameraTransform = _cameraComponent.transform;
            AimDirection = FlatMath.FlatDirection(transform.position, transform.position + cameraTransform.forward, AimDirection);

            // The aim point is taken in the plane the shots fly in, not off the ground the camera happens
            // to see through the crosshair: with the camera angled down, that ground point sits metres
            // past where the bolt actually passes, so the reticle would promise a range nothing keeps.
            Vector3 lineHeight = new Vector3(0f, transform.position.y + _aimHeight, 0f);
            var weaponLine = new Plane(Vector3.up, lineHeight);
            Ray ray = _cameraComponent.ViewportPointToRay(ViewportCentre);
            if (weaponLine.Raycast(ray, out float distance) && distance <= _maxAimDistance)
            {
                AimPoint = ray.GetPoint(distance);
                return;
            }

            // Looking along the plane or away from it leaves nothing to cross, so the aim runs straight out.
            AimPoint = new Vector3(transform.position.x, lineHeight.y, transform.position.z) + AimDirection * _maxAimDistance;
        }
    }
}

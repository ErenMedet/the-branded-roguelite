using Branded.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Aims along the third-person camera's line of sight and turns Visual_Holder.
    // Souls-style: the body follows where it walks, and only commits to the camera's
    // direction when combat calls SnapToAim on an attack or a shot.
    public class PlayerAim : MonoBehaviour
    {
        const float MinMoveSqrSpeed = 0.04f;
        static readonly Vector3 ViewportCentre = new Vector3(0.5f, 0.5f, 0f);

        [SerializeField, FormerlySerializedAs("motor")] PlayerMotor _motorComponent;
        [Tooltip("Visual_Holder: only the visuals rotate, never Player_Root.")]
        [SerializeField, FormerlySerializedAs("visual")] Transform _visualComponent;
        [SerializeField, FormerlySerializedAs("turnSpeed")] float _turnSpeed = 1440f;

        [Header("Aim ray")]
        [Tooltip("What the camera's centre ray can lock onto.")]
        [SerializeField] LayerMask _aimMask = ~0;
        [Tooltip("How far past the player the aim ray starts, so the body is never the aim target.")]
        [SerializeField] float _aimStartOffset = 1f;
        [SerializeField] float _maxAimDistance = 60f;

        public Vector3 AimPoint { get; private set; }
        public Vector3 AimDirection { get; private set; } = Vector3.forward;

        // Set by combat while a swing is committed to a direction.
        public bool RotationLocked { get; set; }

        Camera _cameraComponent;

        void Awake()
        {
            if (!_motorComponent) _motorComponent = GetComponent<PlayerMotor>();
        }

        void Update()
        {
            UpdateAimPoint();
            if (!_visualComponent || RotationLocked) return;

            Vector3 facing = FreeFacing();
            if (facing.sqrMagnitude < 0.0001f) return;

            Quaternion target = Quaternion.LookRotation(facing, Vector3.up);
            _visualComponent.rotation = Quaternion.RotateTowards(_visualComponent.rotation, target, _turnSpeed * Time.deltaTime);
        }

        public void SnapToAim()
        {
            if (!_visualComponent) return;
            _visualComponent.rotation = Quaternion.LookRotation(AimDirection, Vector3.up);
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

            // The ray starts past the player so the body itself never counts as the thing being aimed at.
            Ray ray = _cameraComponent.ViewportPointToRay(ViewportCentre);
            Vector3 origin = ray.GetPoint(FlatMath.FlatDistance(cameraTransform.position, transform.position) + _aimStartOffset);

            AimPoint = Physics.Raycast(origin, ray.direction, out RaycastHit hit, _maxAimDistance, _aimMask, QueryTriggerInteraction.Ignore)
                ? hit.point
                : origin + ray.direction * _maxAimDistance;
        }
    }
}

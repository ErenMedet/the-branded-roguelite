using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Finds the mouse point on the ground plane and turns Visual_Holder toward it.
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("input")] PlayerInputReader _inputComponent;
        [SerializeField, FormerlySerializedAs("motor")] PlayerMotor _motorComponent;
        [Tooltip("Visual_Holder: only the visuals rotate, never Player_Root.")]
        [SerializeField, FormerlySerializedAs("visual")] Transform _visualComponent;
        [SerializeField, FormerlySerializedAs("turnSpeed")] float _turnSpeed = 1440f;

        public Vector3 AimPoint { get; private set; }
        public Vector3 AimDirection { get; private set; } = Vector3.forward;

        // Set by combat while a swing is committed to a direction.
        public bool RotationLocked { get; set; }

        Camera _cameraComponent;

        void Awake()
        {
            if (!_inputComponent) _inputComponent = GetComponent<PlayerInputReader>();
            if (!_motorComponent) _motorComponent = GetComponent<PlayerMotor>();
        }

        void Update()
        {
            UpdateAimPoint();
            if (!_visualComponent || RotationLocked) return;

            Vector3 facing = _motorComponent && _motorComponent.IsDashing ? _motorComponent.DashDirection : AimDirection;
            Quaternion target = Quaternion.LookRotation(facing, Vector3.up);
            _visualComponent.rotation = Quaternion.RotateTowards(_visualComponent.rotation, target, _turnSpeed * Time.deltaTime);
        }

        public void SnapToAim()
        {
            if (!_visualComponent) return;
            _visualComponent.rotation = Quaternion.LookRotation(AimDirection, Vector3.up);
        }

        // Flat direction from an off-centre point (the arm muzzle) to the aim point, so shots land on the cursor.
        public Vector3 DirectionFrom(Vector3 origin)
        {
            Vector3 direction = AimPoint - origin;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.25f ? direction.normalized : AimDirection;
        }

        void UpdateAimPoint()
        {
            if (!_cameraComponent) _cameraComponent = Camera.main;
            if (!_cameraComponent) return;

            Ray ray = _cameraComponent.ScreenPointToRay(_inputComponent.PointerScreenPosition);
            Plane ground = new Plane(Vector3.up, transform.position);
            if (!ground.Raycast(ray, out float enter)) return;

            AimPoint = ray.GetPoint(enter);
            Vector3 direction = AimPoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f) AimDirection = direction.normalized;
        }
    }
}

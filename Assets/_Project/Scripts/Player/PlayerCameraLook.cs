using Unity.Cinemachine;
using UnityEngine;

namespace Branded.Player
{
    // Drives the orbital camera from mouse look. Lives on the CinemachineCamera: the rig orbits the
    // player's head, and the Rotation Composer keeps them framed however far the orbit swings around.
    // It also binds the rig to the player, so the camera ships as one prefab that needs no per-scene wiring.
    [RequireComponent(typeof(CinemachineOrbitalFollow), typeof(CinemachineCamera))]
    public class PlayerCameraLook : MonoBehaviour
    {
        [Tooltip("Child of the player the orbit centres on, found by name under the Player-tagged object.")]
        [SerializeField] string _targetName = "CameraTarget";

        [Header("References")]
        [Tooltip("Empty = found once by the Player tag.")]
        [SerializeField] PlayerInputReader _inputComponent;

        [Header("Sensitivity")]
        [SerializeField] float _horizontalSensitivity = 0.15f;
        [SerializeField] float _verticalSensitivity = 0.1f;
        [SerializeField] bool _invertVertical;

        CinemachineOrbitalFollow _orbitComponent;
        CinemachineCamera _cameraComponent;

        void Awake()
        {
            _orbitComponent = GetComponent<CinemachineOrbitalFollow>();
            _cameraComponent = GetComponent<CinemachineCamera>();

            GameObject player = GameObject.FindWithTag("Player");
            if (!player) return;

            if (!_inputComponent) _inputComponent = player.GetComponent<PlayerInputReader>();

            // A prefab cannot hold a scene reference, so the orbit centre is bound here.
            // A target already wired in the scene wins, so a scene can still override the framing.
            if (_cameraComponent.Follow) return;

            Transform target = player.transform.Find(_targetName);
            if (!target) return;

            _cameraComponent.Follow = target;
            _cameraComponent.LookAt = target;
        }

        void Update()
        {
            if (!_inputComponent) return;

            Vector2 look = _inputComponent.Look;
            if (look.sqrMagnitude < 0.0001f) return;

            // Pushing the mouse up should look up, which lowers the camera's place on the orbit.
            float verticalDelta = look.y * _verticalSensitivity * (_invertVertical ? 1f : -1f);

            _orbitComponent.HorizontalAxis.Value = _orbitComponent.HorizontalAxis.ClampValue(
                _orbitComponent.HorizontalAxis.Value + look.x * _horizontalSensitivity);
            _orbitComponent.VerticalAxis.Value = _orbitComponent.VerticalAxis.ClampValue(
                _orbitComponent.VerticalAxis.Value + verticalDelta);
        }
    }
}

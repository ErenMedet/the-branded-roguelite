using Branded.Enemies;
using UnityEngine;

namespace Branded.Player
{
    // Spirits latch onto the player through this. Each one slows walking a little more;
    // a dash or a finished swing shakes them all off.
    public class PlayerLatchReceiver : MonoBehaviour
    {
        [SerializeField] PlayerMotor _motorComponent;
        [SerializeField] PlayerCombat _combatComponent;
        [SerializeField] int _maxLatched = 5;
        [SerializeField] float _slowPerLatch = 0.2f;
        [SerializeField] float _minSlowMultiplier = 0.3f;
        [SerializeField] float _latchRadius = 0.55f;

        public int LatchedCount { get; private set; }

        EnemyLatch[] _latchComponents;

        void Awake()
        {
            if (!_motorComponent) _motorComponent = GetComponent<PlayerMotor>();
            if (!_combatComponent) _combatComponent = GetComponent<PlayerCombat>();
            _latchComponents = new EnemyLatch[Mathf.Max(1, _maxLatched)];
        }

        void OnEnable()
        {
            _motorComponent.DashStarted += OnDashStarted;
            _combatComponent.SwingEnded += OnSwingEnded;
        }

        void OnDisable()
        {
            _motorComponent.DashStarted -= OnDashStarted;
            _combatComponent.SwingEnded -= OnSwingEnded;
        }

        // Each slot sits at its own spot on a small ring, so latched spirits don't stack on one point.
        public bool TryAttach(EnemyLatch latch, out Vector3 offset)
        {
            offset = default;
            for (int i = 0; i < _latchComponents.Length; i++)
            {
                if (_latchComponents[i] != null) continue; // destroyed spirits compare equal to null, so they count as free
                _latchComponents[i] = latch;
                float angle = i * Mathf.PI * 2f / _latchComponents.Length;
                offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _latchRadius;
                RefreshSlow();
                return true;
            }
            return false;
        }

        public void Detach(EnemyLatch latch)
        {
            for (int i = 0; i < _latchComponents.Length; i++)
                if (_latchComponents[i] == latch) _latchComponents[i] = null;
            RefreshSlow();
        }

        void OnDashStarted() => ShakeOffAll();
        void OnSwingEnded() => ShakeOffAll();

        void ShakeOffAll()
        {
            for (int i = 0; i < _latchComponents.Length; i++)
            {
                EnemyLatch latch = _latchComponents[i];
                _latchComponents[i] = null;
                if (latch) latch.ShakeOff();
            }
            RefreshSlow();
        }

        void RefreshSlow()
        {
            int count = 0;
            foreach (var latch in _latchComponents)
                if (latch != null) count++;
            LatchedCount = count;
            _motorComponent.SlowMultiplier = Mathf.Max(_minSlowMultiplier, 1f - _slowPerLatch * count);
        }
    }
}

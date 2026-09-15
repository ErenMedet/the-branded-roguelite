using Branded.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Branded.Enemies
{
    // Shadow spirit: instead of hitting, it clings to the player and slows them until a dash or a swing shakes it off.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyLatch : EnemyAttack
    {
        const float RetryDelay = 0.5f; // every latch slot was taken

        [SerializeField] float _relatchCooldown = 1.5f;
        [Tooltip("How far from the player a shaken-off spirit lands.")]
        [SerializeField] float _shakeOffDistance = 1.8f;

        NavMeshAgent _navMeshAgentComponent;
        Collider[] _colliderComponents;
        PlayerLatchReceiver _receiverComponent;
        Vector3 _offset;

        protected override void Awake()
        {
            base.Awake();
            _navMeshAgentComponent = GetComponent<NavMeshAgent>();
            _colliderComponents = GetComponents<Collider>();
        }

        void Update()
        {
            TickCooldown();
            if (!IsReady || !_chaserComponent.InRange) return;
            TryLatch();
        }

        void TryLatch()
        {
            if (!_receiverComponent) _receiverComponent = _chaserComponent.TargetComponent.GetComponent<PlayerLatchReceiver>();
            if (!_receiverComponent || !_receiverComponent.TryAttach(this, out _offset))
            {
                CooldownTimer = RetryDelay;
                return;
            }

            StartAttack();
            _chaserComponent.ReleaseSlot();
            _navMeshAgentComponent.enabled = false;
            // A solid collider stuck to the player would shove the CharacterController; the sword still finds triggers.
            SetTrigger(true);
        }

        void LateUpdate()
        {
            if (!IsAttacking || !_receiverComponent) return;
            transform.position = _receiverComponent.transform.position + _offset;
            _chaserComponent.FaceTarget();
        }

        // Called by the receiver, which has already let go of this spirit.
        public void ShakeOff()
        {
            if (!IsAttacking) return;
            Release();

            Vector3 away = _offset.sqrMagnitude > 0.0001f ? _offset.normalized : -transform.forward;
            Vector3 spot = _receiverComponent.transform.position + away * _shakeOffDistance;
            if (NavMesh.SamplePosition(spot, out NavMeshHit hit, 2f, NavMesh.AllAreas)) transform.position = hit.position;

            _navMeshAgentComponent.enabled = true;
            if (_navMeshAgentComponent.isOnNavMesh) _navMeshAgentComponent.Warp(transform.position);
        }

        // Staying on is the whole point: hits never pry it loose, only the receiver's shake-off does.
        public override void Interrupt() { }

        void Release()
        {
            FinishAttack(_relatchCooldown);
            SetTrigger(false);
        }

        void SetTrigger(bool trigger)
        {
            foreach (var col in _colliderComponents) col.isTrigger = trigger;
        }

        // Death or dawn while latched: let go so the player's slow ends.
        void OnDisable()
        {
            if (!IsAttacking) return;
            if (_receiverComponent) _receiverComponent.Detach(this);
            Release();
        }
    }
}

using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Wander around home, chase the player if they get close, lunge when in range.</summary>
    public class CreatureAI : MonoBehaviour
    {
        enum State { Wander, Chase, Lunge, Cooldown, Dead }

        public Transform body;
        public Vector3 homePosition;
        public Health health;

        public float wanderRadius = 3f;
        public float wanderSpeed = 0.8f;
        public float aggroRange = 4f;
        public float chaseSpeed = 2.2f;
        public float lungeRange = 1.4f;
        public float lungeSpeed = 7f;
        public float lungeDuration = 0.25f;
        public float cooldownDuration = 1f;
        public float leashRange = 6f;

        State _state = State.Wander;
        float _stateTimer;
        Vector3 _wanderTarget;
        Vector3 _lungeStart, _lungeTarget;

        void Start()
        {
            if (health != null) health.OnDeath += () => _state = State.Dead;
            PickNewWanderTarget();
        }

        void Update()
        {
            if (_state == State.Dead) return;

            var player = GameManager.Instance != null ? GameManager.Instance.player : null;
            _stateTimer += Time.deltaTime;
            float bob = Mathf.Sin(Time.time * 3f + homePosition.x) * 0.04f;

            switch (_state)
            {
                case State.Wander:
                    body.position = Vector3.MoveTowards(body.position, _wanderTarget + Vector3.up * bob, wanderSpeed * Time.deltaTime);
                    if (Vector3.Distance(body.position, _wanderTarget) < 0.15f) PickNewWanderTarget();
                    if (player != null && Vector3.Distance(body.position, player.position) < aggroRange) _state = State.Chase;
                    break;

                case State.Chase:
                    if (player == null) { _state = State.Wander; break; }
                    if (Vector3.Distance(homePosition, player.position) > leashRange) { _state = State.Wander; break; }
                    Vector3 target = player.position + Vector3.up * 0.35f;
                    body.position = Vector3.MoveTowards(body.position, target, chaseSpeed * Time.deltaTime);
                    if (Vector3.Distance(body.position, target) < lungeRange)
                    {
                        _state = State.Lunge;
                        _stateTimer = 0f;
                        _lungeStart = body.position;
                        _lungeTarget = target;
                    }
                    break;

                case State.Lunge:
                    float t = Mathf.Clamp01(_stateTimer / lungeDuration);
                    body.position = Vector3.Lerp(_lungeStart, _lungeTarget, t);
                    if (t >= 1f) { _state = State.Cooldown; _stateTimer = 0f; }
                    break;

                case State.Cooldown:
                    if (_stateTimer > cooldownDuration) _state = State.Wander;
                    break;
            }
        }

        void PickNewWanderTarget()
        {
            Vector2 offset = Random.insideUnitCircle * wanderRadius;
            _wanderTarget = homePosition + new Vector3(offset.x, 0, offset.y);
        }
    }
}

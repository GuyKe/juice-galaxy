using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Simple idle-sway / notice / lunge state machine driving Mrs. Slithers' head.</summary>
    public class MrsSlithersAI : MonoBehaviour
    {
        enum State { Idle, Chase, Lunge, Cooldown, Fainted }

        public Transform head;
        public Vector3 homePosition;
        public Health health;

        public float aggroRange = 4.5f;
        public float lungeRange = 2.2f;
        public float chaseSpeed = 2.5f;
        public float lungeSpeed = 9f;
        public float lungeDuration = 0.35f;
        public float cooldownDuration = 1.2f;
        public float leashRange = 3.5f;

        State _state = State.Idle;
        float _stateTimer;
        Vector3 _lungeStart;
        Vector3 _lungeTarget;

        void Start()
        {
            if (health != null) health.OnDeath += OnDeath;
        }

        void Update()
        {
            var player = GameManager.Instance != null ? GameManager.Instance.player : null;
            _stateTimer += Time.deltaTime;

            float bob = Mathf.Sin(Time.time * 1.8f) * 0.06f;

            switch (_state)
            {
                case State.Idle:
                    head.position = homePosition + new Vector3(0, bob, 0);
                    head.rotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 0.6f) * 25f, 0);
                    if (player != null && Vector3.Distance(head.position, player.position) < aggroRange)
                        _state = State.Chase;
                    break;

                case State.Chase:
                    if (player == null) { _state = State.Idle; break; }
                    Vector3 targetPos = player.position + Vector3.up * 1.3f;
                    if (Vector3.Distance(homePosition, targetPos) > leashRange + aggroRange)
                    {
                        _state = State.Idle;
                        break;
                    }
                    head.position = Vector3.MoveTowards(head.position, targetPos, chaseSpeed * Time.deltaTime);
                    FaceTarget(targetPos);
                    if (Vector3.Distance(head.position, targetPos) < lungeRange)
                    {
                        _state = State.Lunge;
                        _stateTimer = 0f;
                        _lungeStart = head.position;
                        _lungeTarget = targetPos;
                    }
                    break;

                case State.Lunge:
                    float t = Mathf.Clamp01(_stateTimer / lungeDuration);
                    head.position = Vector3.Lerp(_lungeStart, _lungeTarget, EaseOutQuad(t));
                    if (t >= 1f)
                    {
                        _state = State.Cooldown;
                        _stateTimer = 0f;
                    }
                    break;

                case State.Cooldown:
                    head.position = Vector3.MoveTowards(head.position, homePosition, chaseSpeed * 0.5f * Time.deltaTime);
                    if (_stateTimer > cooldownDuration) _state = State.Idle;
                    break;

                case State.Fainted:
                    head.position = Vector3.Lerp(head.position, homePosition + Vector3.down * 0.3f, Time.deltaTime);
                    break;
            }
        }

        void FaceTarget(Vector3 targetPos)
        {
            Vector3 dir = targetPos - head.position;
            if (dir.sqrMagnitude > 0.001f) head.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        void OnDeath()
        {
            _state = State.Fainted;
        }
    }
}

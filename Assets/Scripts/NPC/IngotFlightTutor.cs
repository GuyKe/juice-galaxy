using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>"Hold A to learn to fly." Tracks how long the player holds the A button near Ingot.</summary>
    public class IngotFlightTutor : MonoBehaviour
    {
        public WorldSpaceLabel promptLabel;
        public float requiredHoldSeconds = 1.5f;
        public float celebrationSeconds = 3f;

        bool _playerInside;
        float _holdTimer;
        float _celebrationTimer;
        XRInputRig _rig;
        JuiceSystem _juice;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = true;
            CachePlayerRefs();
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = false;
            _holdTimer = 0f;
        }

        void CachePlayerRefs()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.player == null) return;
            if (_rig == null) _rig = gm.player.GetComponentInChildren<XRInputRig>();
            if (_juice == null) _juice = gm.playerJuice;
        }

        void Update()
        {
            if (_juice == null) CachePlayerRefs();
            if (_juice == null) return;

            if (_juice.flightUnlocked)
            {
                if (_celebrationTimer > 0f)
                {
                    _celebrationTimer -= Time.deltaTime;
                    if (_celebrationTimer <= 0f) promptLabel.SetVisible(false);
                }
                return;
            }

            if (!_playerInside)
            {
                promptLabel.SetVisible(false);
                return;
            }

            promptLabel.SetVisible(true);

            bool holding = _rig != null && _rig.flyButtonAction.IsPressed();
            if (holding)
            {
                _holdTimer += Time.deltaTime;
                promptLabel.SetText($"Hold A to fly... {Mathf.CeilToInt(requiredHoldSeconds - _holdTimer)}");
                if (_holdTimer >= requiredHoldSeconds)
                {
                    _juice.UnlockFlight();
                    promptLabel.SetText("Great! You can fly now!");
                    _celebrationTimer = celebrationSeconds;
                }
            }
            else
            {
                _holdTimer = Mathf.Max(0f, _holdTimer - Time.deltaTime * 2f);
                promptLabel.SetText("Hold the A button to learn to fly!");
            }
        }
    }
}

using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// The first time the player nears Ingot, he delivers one line for a few seconds; holding the
    /// right controller's A button near him (for requiredHoldSeconds, any time after that) unlocks
    /// flight.
    /// </summary>
    public class IngotFlightTutor : MonoBehaviour
    {
        public WorldSpaceLabel promptLabel;
        // TextMesh doesn't word-wrap on its own, so the line is split by hand to keep it readable
        // instead of rendering as one very wide (or very tiny) strip of text.
        public string introLine = "There's a cool toy on top of\nthe school, hold A to fly.";
        public float introDuration = 6f;
        public float requiredHoldSeconds = 1.5f;

        bool _playerInside;
        bool _hasIntroduced;
        float _introTimer;
        float _holdTimer;
        XRInputRig _rig;
        JuiceSystem _juice;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInside = true;
            CachePlayerRefs();

            if (!_hasIntroduced && _juice != null && !_juice.flightUnlocked)
            {
                _hasIntroduced = true;
                _introTimer = introDuration;
                promptLabel.SetText(introLine);
                promptLabel.SetVisible(true);
            }
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
                promptLabel.SetVisible(false);
                return;
            }

            if (_introTimer > 0f)
            {
                _introTimer -= Time.deltaTime;
                if (_introTimer <= 0f) promptLabel.SetVisible(false);
            }

            bool holding = _playerInside && _rig != null && _rig.flyButtonAction.IsPressed();
            if (holding)
            {
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= requiredHoldSeconds) _juice.UnlockFlight();
            }
            else
            {
                _holdTimer = Mathf.Max(0f, _holdTimer - Time.deltaTime * 2f);
            }
        }
    }
}

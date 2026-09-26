using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace JuiceGalaxy
{
    /// <summary>
    /// The first time the player nears Ingot, he delivers one line for a few seconds while the
    /// camera cuts to a close-up on his face; holding the right controller's A button near him
    /// (for requiredHoldSeconds, any time after that) unlocks flight.
    /// </summary>
    public class IngotFlightTutor : MonoBehaviour
    {
        public WorldSpaceLabel promptLabel;
        public Transform facePoint;
        // TextMesh doesn't word-wrap on its own, so the line is split by hand to keep it readable
        // instead of rendering as one very wide (or very tiny) strip of text.
        public string introLine = "There's a cool toy on top of\nthe school, hold A to fly.";
        public float introDuration = 6f;
        public float requiredHoldSeconds = 1.5f;

        // How fast the camera cuts to/from Ingot's face. Kept fast (a near-instant cut rather than
        // a slow dolly) since a lingering artificial camera move is a real VR discomfort risk.
        public float cameraBlendSpeed = 6f;
        public float closeUpDistance = 1.1f;

        bool _playerInside;
        bool _hasIntroduced;
        float _introTimer;
        float _holdTimer;
        XRInputRig _rig;
        JuiceSystem _juice;

        TrackedPoseDriver _headDriver;
        Transform _playerRoot;
        float _cameraBlend;

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
            if (_headDriver == null && _rig != null)
            {
                _headDriver = _rig.head.GetComponent<TrackedPoseDriver>();
                _playerRoot = gm.player;
            }
        }

        void Update()
        {
            if (_juice == null) CachePlayerRefs();
            if (_juice == null) return;

            bool talking = UpdateDialogueState();
            UpdateCameraFocus(talking);
        }

        bool UpdateDialogueState()
        {
            if (_juice.flightUnlocked)
            {
                promptLabel.SetVisible(false);
                return false;
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

            // The camera cutscene runs exactly as long as his one line is on screen - a short,
            // bounded window rather than the open-ended waiting-for-A period, since freezing real
            // head tracking indefinitely would be a much worse VR comfort problem than a brief cut.
            return _introTimer > 0f;
        }

        void UpdateCameraFocus(bool talking)
        {
            if (_headDriver == null || facePoint == null || _playerRoot == null) return;

            _cameraBlend = Mathf.MoveTowards(_cameraBlend, talking ? 1f : 0f, Time.deltaTime * cameraBlendSpeed);

            if (_cameraBlend <= 0.0001f)
            {
                _headDriver.enabled = true;
                return;
            }

            // Read the live tracked pose directly from the underlying actions (they keep updating
            // even while the driver component is disabled) so we can blend smoothly in both
            // directions without ever losing track of where the player's real head actually is.
            Transform head = _headDriver.transform;
            Vector3 livePos = _headDriver.positionInput.action != null
                ? _headDriver.positionInput.action.ReadValue<Vector3>() : head.localPosition;
            Quaternion liveRot = _headDriver.rotationInput.action != null
                ? _headDriver.rotationInput.action.ReadValue<Quaternion>() : head.localRotation;

            _headDriver.enabled = false;

            Vector3 worldCamPos = facePoint.position + facePoint.forward * closeUpDistance;
            Quaternion worldCamRot = Quaternion.LookRotation(facePoint.position - worldCamPos, Vector3.up);
            Vector3 targetLocalPos = _playerRoot.InverseTransformPoint(worldCamPos);
            Quaternion targetLocalRot = Quaternion.Inverse(_playerRoot.rotation) * worldCamRot;

            head.localPosition = Vector3.Lerp(livePos, targetLocalPos, _cameraBlend);
            head.localRotation = Quaternion.Slerp(liveRot, targetLocalRot, _cameraBlend);
        }
    }
}

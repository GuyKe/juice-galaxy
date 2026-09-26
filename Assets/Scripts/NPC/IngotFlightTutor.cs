using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace JuiceGalaxy
{
    /// <summary>
    /// "Hold A to learn to fly." Tracks how long the player holds the A button near Ingot, and
    /// cuts to a close-up on his face while he delivers his "you can fly now" line.
    /// </summary>
    public class IngotFlightTutor : MonoBehaviour
    {
        public WorldSpaceLabel promptLabel;
        public Transform facePoint;
        public float requiredHoldSeconds = 1.5f;
        public float celebrationSeconds = 3f;

        // How fast the camera cuts to/from Ingot's face. Kept fast (a near-instant cut rather than
        // a slow dolly) since a lingering artificial camera move is a real VR discomfort risk.
        public float cameraBlendSpeed = 6f;
        public float closeUpDistance = 1.1f;

        bool _playerInside;
        float _holdTimer;
        float _celebrationTimer;
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
                if (_celebrationTimer > 0f)
                {
                    _celebrationTimer -= Time.deltaTime;
                    if (_celebrationTimer <= 0f) promptLabel.SetVisible(false);
                    return _celebrationTimer > 0f;
                }
                return false;
            }

            if (!_playerInside)
            {
                promptLabel.SetVisible(false);
                return false;
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

            // The camera cutscene is scoped to just this bounded "he's actually saying his line"
            // window (celebrationSeconds long), not the open-ended waiting prompt above - freezing
            // the player's real head tracking for an indefinite amount of time would be a much
            // worse VR comfort problem than this brief scripted moment.
            return false;
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

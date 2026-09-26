using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace JuiceGalaxy
{
    /// <summary>
    /// Builds the head + hand tracking rig entirely at runtime via the Input System's XR device
    /// layouts (fed by OpenXR), so no hand-authored prefab or .inputactions asset is required.
    /// Exposes the raw actions other scripts (locomotion, flight, melee, grabbing) read from.
    /// </summary>
    public class XRInputRig : MonoBehaviour
    {
        public Camera headCamera { get; private set; }
        public Transform head { get; private set; }
        public Transform leftHand { get; private set; }
        public Transform rightHand { get; private set; }

        public InputAction moveAction { get; private set; }
        public InputAction turnAction { get; private set; }
        public InputAction flyButtonAction { get; private set; }
        public InputAction leftTriggerAction { get; private set; }
        public InputAction rightTriggerAction { get; private set; }
        public InputAction leftHandTrackedAction { get; private set; }
        public InputAction rightHandTrackedAction { get; private set; }

        public static XRInputRig Build(Transform parent)
        {
            var go = new GameObject("XR Input Rig");
            go.transform.SetParent(parent, false);
            var rig = go.AddComponent<XRInputRig>();
            rig.Construct();
            return rig;
        }

        void Construct()
        {
            head = CreateTracked("Head", "<XRHMD>/centerEyePosition", "<XRHMD>/centerEyeRotation", "Vector3", "Quaternion");
            headCamera = head.gameObject.AddComponent<Camera>();
            headCamera.nearClipPlane = 0.05f;
            headCamera.farClipPlane = 500f;
            head.gameObject.AddComponent<AudioListener>();
            head.gameObject.tag = "MainCamera";

            leftHand = CreateTracked("LeftHand", "<XRController>{LeftHand}/devicePosition", "<XRController>{LeftHand}/deviceRotation", "Vector3", "Quaternion");
            rightHand = CreateTracked("RightHand", "<XRController>{RightHand}/devicePosition", "<XRController>{RightHand}/deviceRotation", "Vector3", "Quaternion");

            // Quest's inside-out controller tracking can briefly lose a hand that swings out of the
            // headset's camera view (behind the back, down low, fast motion). When that happens the
            // devicePosition binding above just holds its last known value, which otherwise reads as
            // the floppy arm being rigidly "frozen". Exposing isTracked lets the arm go limp instead.
            leftHandTrackedAction = new InputAction("LeftHandTracked", InputActionType.Button, "<XRController>{LeftHand}/isTracked");
            rightHandTrackedAction = new InputAction("RightHandTracked", InputActionType.Button, "<XRController>{RightHand}/isTracked");
            leftHandTrackedAction.Enable();
            rightHandTrackedAction.Enable();

            moveAction = new InputAction("Move", InputActionType.Value, "<XRController>{LeftHand}/primary2DAxis", expectedControlType: "Vector2");
            turnAction = new InputAction("Turn", InputActionType.Value, "<XRController>{RightHand}/primary2DAxis", expectedControlType: "Vector2");
            flyButtonAction = new InputAction("Fly", InputActionType.Button, "<XRController>{RightHand}/primary2DAxisClick");
            leftTriggerAction = new InputAction("LeftTrigger", InputActionType.Value, "<XRController>{LeftHand}/trigger", expectedControlType: "Axis");
            rightTriggerAction = new InputAction("RightTrigger", InputActionType.Value, "<XRController>{RightHand}/trigger", expectedControlType: "Axis");

            moveAction.Enable();
            turnAction.Enable();
            flyButtonAction.Enable();
            leftTriggerAction.Enable();
            rightTriggerAction.Enable();
        }

        Transform CreateTracked(string name, string posBinding, string rotBinding, string posControlType, string rotControlType)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var posAction = new InputAction(name + "Position", InputActionType.Value, posBinding, expectedControlType: posControlType);
            var rotAction = new InputAction(name + "Rotation", InputActionType.Value, rotBinding, expectedControlType: rotControlType);

            var driver = go.AddComponent<TrackedPoseDriver>();
            driver.positionInput = new InputActionProperty(posAction);
            driver.rotationInput = new InputActionProperty(rotAction);
            driver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            driver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

            posAction.Enable();
            rotAction.Enable();

            return go.transform;
        }

        void OnDestroy()
        {
            moveAction?.Disable();
            turnAction?.Disable();
            flyButtonAction?.Disable();
            leftTriggerAction?.Disable();
            rightTriggerAction?.Disable();
            leftHandTrackedAction?.Disable();
            rightHandTrackedAction?.Disable();
        }
    }
}

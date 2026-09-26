using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>
    /// "Ingot tasks you with learning how to fly by holding the right thumbstick." Once
    /// JuiceSystem.flightUnlocked is set, holding the right thumbstick down lifts and propels the
    /// player forward in their look direction; releasing lets gravity resume smoothly. While airborne, the
    /// player's floppy limbs get blown backward by "wind" so the whole body ragdolls/flails.
    /// Flight doesn't cost Juice - Juice doubles as your health pool, so making flight also
    /// consume it meant getting hit (which drains Juice) could silently ground you, making flight
    /// feel randomly broken.
    /// </summary>
    public class PlayerFlight : MonoBehaviour
    {
        public XRInputRig rig;
        public FloppyPlayerController controller;
        public JuiceSystem juice;

        public float lift = 3.2f;
        public float forwardThrust = 2.4f;
        public float maxFlightSpeed = 6f;

        /// <summary>The player's floppy limb chains - blown backward by "wind" while flying so the
        /// whole floppy body ragdolls/flails instead of just trailing limply.</summary>
        public FloppyChain[] windChains;
        public float windStrength = 3.5f;

        Vector3 _flightVelocity;

        public void Init(XRInputRig inputRig, FloppyPlayerController playerController, JuiceSystem juiceSystem)
        {
            rig = inputRig;
            controller = playerController;
            juice = juiceSystem;
        }

        void Update()
        {
            if (rig == null || controller == null) return;

            bool wantsToFly = juice != null && juice.flightUnlocked && rig.flyButtonAction.IsPressed();

            if (wantsToFly)
            {
                controller.isFlying = true;
                Vector3 dir = rig.head.forward;
                Vector3 targetVelocity = dir * forwardThrust + Vector3.up * lift;
                _flightVelocity = Vector3.Lerp(_flightVelocity, targetVelocity, Time.deltaTime * 3f);
                _flightVelocity = Vector3.ClampMagnitude(_flightVelocity, maxFlightSpeed);

                controller.ApplyFlightMotion(_flightVelocity * Time.deltaTime);
                controller.verticalVelocity = 0f;
            }
            else if (controller.isFlying)
            {
                controller.isFlying = false;
                controller.verticalVelocity = Mathf.Min(0f, _flightVelocity.y);
                _flightVelocity = Vector3.zero;
            }
        }

        void FixedUpdate()
        {
            if (windChains == null || controller == null || !controller.isFlying) return;

            // Blow every floppy segment backward relative to flight velocity, in physics time so
            // the force is frame-rate independent.
            Vector3 windForce = -_flightVelocity * windStrength;
            foreach (var chain in windChains)
            {
                if (chain == null || chain.segments == null) continue;
                foreach (var segment in chain.segments)
                {
                    if (segment != null) segment.AddForce(windForce, ForceMode.Acceleration);
                }
            }
        }
    }
}

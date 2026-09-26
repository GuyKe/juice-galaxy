using System.Collections.Generic;
using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Builds the whole player rig - XR tracking, locomotion, floppy visuals, fists, juice/health.</summary>
    public static class PlayerFactory
    {
        public static Transform Spawn(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            var playerGo = new GameObject("Player");
            playerGo.tag = "Player";
            playerGo.transform.position = spawnPosition;
            playerGo.transform.rotation = spawnRotation;

            var controller = playerGo.AddComponent<CharacterController>();
            controller.radius = 0.3f;
            controller.height = 1.7f;
            controller.center = new Vector3(0, 0.85f, 0);
            controller.skinWidth = 0.02f;
            controller.stepOffset = 0.3f;

            var xrRig = XRInputRig.Build(playerGo.transform);

            var health = playerGo.AddComponent<Health>();
            health.maxHealth = 100f;
            var juice = playerGo.AddComponent<JuiceSystem>();
            juice.maxJuice = 100f;
            juice.currentJuice = 60f;

            var floppyController = playerGo.AddComponent<FloppyPlayerController>();
            floppyController.Init(xrRig);

            var flight = playerGo.AddComponent<PlayerFlight>();
            flight.Init(xrRig, floppyController, juice);

            flight.windChains = BuildFloppyVisuals(playerGo.transform, xrRig);
            BuildFists(xrRig, playerGo);
            BuildHandGrabbers(xrRig, playerGo);
            DisableSelfCollisions(controller, flight.windChains);

            var gm = GameManager.Instance;
            if (gm != null) gm.RegisterPlayer(playerGo.transform, juice, health, xrRig.headCamera);

            return playerGo.transform;
        }

        static FloppyChain[] BuildFloppyVisuals(Transform playerRoot, XRInputRig rig)
        {
            var bodyMat = MaterialUtil.CreateLit(new Color(0.85f, 0.25f, 0.65f),
                MaterialUtil.CreateMottleTexture(new Color(0.85f, 0.25f, 0.65f), new Color(1f, 0.55f, 0.1f), 32, 7));

            // Torso: a simple stretched blob that kinematically follows the headset's horizontal
            // position at a fixed body height - not full-body IK, just enough presence in mirrors/shadows.
            var torsoHolder = new GameObject("TorsoFollow");
            torsoHolder.transform.SetParent(playerRoot, false);
            var follow = torsoHolder.AddComponent<HeadFollowVisual>();
            follow.head = rig.head;
            follow.bodyRoot = playerRoot;
            follow.heightOffset = -0.55f;

            var torso = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            torso.name = "TorsoVisual";
            torso.transform.SetParent(torsoHolder.transform, false);
            torso.transform.localScale = new Vector3(0.5f, 0.65f, 0.4f);
            Object.Destroy(torso.GetComponent<Collider>());
            torso.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

            // Floppy tentacle sleeves dangling from each hand.
            var leftSleeve = FloppyChain.Build(playerRoot, "LeftSleeve", rig.leftHand, 4, 0.09f, 0.06f, 0.02f, bodyMat);
            var rightSleeve = FloppyChain.Build(playerRoot, "RightSleeve", rig.rightHand, 4, 0.09f, 0.06f, 0.02f, bodyMat);

            // Floppy tail dangling from the torso.
            var tail = FloppyChain.Build(playerRoot, "Tail", torsoHolder.transform, 5, 0.1f, 0.07f, 0.015f, bodyMat);

            return new[] { leftSleeve, rightSleeve, tail };
        }

        static void BuildFists(XRInputRig rig, GameObject owner)
        {
            CreateFist(rig.leftHand, owner);
            CreateFist(rig.rightHand, owner);
        }

        static void CreateFist(Transform hand, GameObject owner)
        {
            var fist = new GameObject("Fist");
            fist.transform.SetParent(hand, false);
            var col = fist.AddComponent<SphereCollider>();
            col.radius = 0.08f;
            col.isTrigger = true;

            // A kinematic Rigidbody is required for trigger events against other kinematic/static
            // colliders since neither the hand-tracked fist nor those targets are driven by normal
            // physics forces.
            var rb = fist.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var hitbox = fist.AddComponent<MomentumMeleeHitbox>();
            hitbox.owner = owner;
        }

        static void BuildHandGrabbers(XRInputRig rig, GameObject owner)
        {
            var left = owner.AddComponent<HandGrabber>();
            left.Init(rig.leftHand, rig.leftTriggerAction, owner);

            var right = owner.AddComponent<HandGrabber>();
            right.Init(rig.rightHand, rig.rightTriggerAction, owner);
        }

        /// <summary>
        /// The floppy sleeve/tail segments have solid colliders so they can bump world geometry,
        /// but without this they'd also collide with the player's own CharacterController and each
        /// other every frame - the constant self-pushback is what made the arms look "glitched",
        /// getting shoved into tangled poses under the torso.
        /// </summary>
        static void DisableSelfCollisions(CharacterController controller, FloppyChain[] chains)
        {
            if (chains == null) return;

            var colliders = new List<Collider>();
            foreach (var chain in chains)
            {
                if (chain == null || chain.segments == null) continue;
                foreach (var segment in chain.segments)
                {
                    var col = segment != null ? segment.GetComponent<Collider>() : null;
                    if (col != null) colliders.Add(col);
                }
            }

            foreach (var col in colliders)
                Physics.IgnoreCollision(controller, col);

            for (int i = 0; i < colliders.Count; i++)
                for (int j = i + 1; j < colliders.Count; j++)
                    Physics.IgnoreCollision(colliders[i], colliders[j]);
        }
    }
}

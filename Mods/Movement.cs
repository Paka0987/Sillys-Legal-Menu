using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using POpusCodec.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Valve.VR;
using static Unity.Burst.Intrinsics.X86.Avx;
using Application = UnityEngine.Application;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Text = UnityEngine.UI.Text;

namespace Juul
{
    public class Movement
    {
        public static int Speedinde = 0;
        public static string[] platInputNames = { "Grip", "Trigger" };

        private static bool wasNoClipFly = false;
        public static void NoClipFly()
        {
            if (Inputs.RightPrimary)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * flyspeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                if (!wasNoClipFly)
                {
                    foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                        v.enabled = false;
                    wasNoClipFly = true;
                }
            }
            else
            {
                if (wasNoClipFly)
                {
                    foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                        v.enabled = true;
                    wasNoClipFly = false;
                }
            }
        }
        public static float flyspeed = 5f; 
        public static int speedIndex = 1; 
        public static float[] speedOptions = new float[] { 2f, 5f, 10f, 20f, 50f };
        public static string[] speedNames = new string[] { "Slow", "Normal", "Fast", "Very Fast", "Extreme" };

        public static void Fly()
        {
            var group = ButtonConfigs.Get("Flight");
            bool active = group != null && group.Binds.Count > 0 ? group.Binds[0].IsPressed() : Inputs.RightPrimary;
            if (active)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * flyspeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }
        public static void UpAndDown()
        {
            if (Inputs.RightTrigger)
            {
                GTPlayer.Instance.transform.position += Vector3.up * (Time.deltaTime * flyspeed);
            }
            if (Inputs.LeftTrigger)
            {
                GTPlayer.Instance.transform.position += Vector3.down * (Time.deltaTime * flyspeed);
            }
        }
        public static void LeftAndRight()
        {
            if (Inputs.RightTrigger)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.right * (Time.deltaTime * flyspeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
            if (Inputs.LeftTrigger)
            {
                GTPlayer.Instance.transform.position -= GorillaTagger.Instance.headCollider.transform.right * (Time.deltaTime * flyspeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }
        public static void BackAndForth()
        {
            if (Inputs.RightTrigger)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * flyspeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
            if (Inputs.LeftTrigger)
            {
                GTPlayer.Instance.transform.position -= GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * flyspeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }
        private static bool wasNoclipOnly = false;
        public static void Noclip()
        {
            var group = ButtonConfigs.Get("Noclip");
            bool active = group != null && group.Binds.Count > 0 ? group.Binds[0].IsPressed() : Inputs.RightPrimary;
            if (active)
            {
                if (!wasNoclipOnly)
                {
                    foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                        v.enabled = false;
                    wasNoclipOnly = true;
                }
            }
            else
            {
                if (wasNoclipOnly)
                {
                    foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                        v.enabled = true;
                    wasNoclipOnly = false;
                }
            }
        }

        public static void ZeroGravity()
        {
            GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.up * 9.81f, ForceMode.Acceleration);
        }

        public static void LowGravity()
        {
            GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.up * 6.66f, ForceMode.Acceleration);
        }

        public static void HighGravity()
        {
            GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.down * 7.77f, ForceMode.Acceleration);
        }

        public static void Bouncy()
        {
            GorillaTagger.Instance.bodyCollider.material.bounciness = 1f;
            GorillaTagger.Instance.bodyCollider.material.bounceCombine = (PhysicsMaterialCombine)3;
            GorillaTagger.Instance.bodyCollider.material.dynamicFriction = 0f;
        }

        public static void ResetBouncy()
        {
            GorillaTagger.Instance.bodyCollider.material.bounciness = 0f;
            GorillaTagger.Instance.bodyCollider.material.bounceCombine = (PhysicsMaterialCombine)3;
            GorillaTagger.Instance.bodyCollider.material.dynamicFriction = 0f;
        }

        public static bool isTped = false;

        public static void TeleportGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!isTped)
                {
                    isTped = true;
                    TeleportPlayer(GunLib.spherepointer.transform.position);
                }
            }, false);

            if (!GunLib.trigger)
            {
                isTped = false;
            }
        }

        public static void TeleportPlayer(Vector3 pos)
        {
            GTPlayer.Instance.TeleportTo(World2Player(pos), GTPlayer.Instance.transform.rotation, false);
        }

        public static Vector3 World2Player(Vector3 world)
        {
            return world - GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.transform.position;
        }
        public static Vector3 pos;

        public static float jumpspeed = 20f;
        public static float jumpmultiplier = 20f;

        public static float[] speedOptions2 = new float[] { 1f, 7f, 8f, 25f };
        public static float[] multiplierOptions = new float[] { 1f, 2.1f, 3f, 15f };
        public static string[] speedNames2 = new string[] { "Slow", "Mosa", "Normal", "Insane" };
        public static int speedIndex2 = 2; 

        public static void SpeedBoost()
        {
            var group = ButtonConfigs.Get("Speed Boost");
            bool hasKeybind = group != null && group.Binds.Count > 0 && group.Binds[0].VRInput != VRButton.None;
            if (hasKeybind)
            {
                if (group.Binds[0].IsPressed())
                {
                    GTPlayer.Instance.maxJumpSpeed = jumpspeed;
                    GTPlayer.Instance.jumpMultiplier = jumpmultiplier;
                }
            }
            else
            {
                GTPlayer.Instance.maxJumpSpeed = jumpspeed;
                GTPlayer.Instance.jumpMultiplier = jumpmultiplier;
            }
        }
      
        public static void WASDFly()
        {
            float sped = 5f;
            float multipl = 2.5f;
            float sens = 0.3f;
            Transform playerCamera = Camera.main.transform;
            Rigidbody playerRigidBody = GorillaTagger.Instance.rigidbody;
            playerRigidBody.useGravity = false;
            playerRigidBody.linearVelocity = Vector3.zero;
            float actualSpeed = UnityInput.Current.GetKey(KeyCode.LeftShift)
                ? sped * multipl
                : sped;
            float deltaMovement = actualSpeed * Time.deltaTime;
            Vector3 movementVector = Vector3.zero;
            if (UnityInput.Current.GetKey(KeyCode.W) || UnityInput.Current.GetKey(KeyCode.UpArrow))
                movementVector += playerCamera.forward;
            if (UnityInput.Current.GetKey(KeyCode.S) || UnityInput.Current.GetKey(KeyCode.DownArrow))
                movementVector -= playerCamera.forward;
            if (UnityInput.Current.GetKey(KeyCode.D) || UnityInput.Current.GetKey(KeyCode.RightArrow))
                movementVector += playerCamera.right;
            if (UnityInput.Current.GetKey(KeyCode.A) || UnityInput.Current.GetKey(KeyCode.LeftArrow))
                movementVector -= playerCamera.right;
            if (UnityInput.Current.GetKey(KeyCode.Space))
                movementVector += playerCamera.up;
            if (UnityInput.Current.GetKey(KeyCode.LeftControl))
                movementVector -= playerCamera.up;
            playerCamera.position += movementVector * deltaMovement;
            if (UnityInput.Current.GetMouseButton(1))
            {
                Vector3 mouseDelta = UnityInput.Current.mousePosition - pos;
                float pitchRotation = playerCamera.localEulerAngles.x - (mouseDelta.y * sens);
                float yawRotation = playerCamera.localEulerAngles.y + (mouseDelta.x * sens);
                playerCamera.localEulerAngles = new Vector3(pitchRotation, yawRotation, 0f);
            }
            pos = UnityInput.Current.mousePosition;
        }
        public static GameObject platL, platR;
        public static int platMode = 1;
        public static int platInput = 0;

        public static void Platforms()
        {
            var group = ButtonConfigs.Get("Platforms");
            bool leftInput, rightInput;
            if (group != null && group.Binds.Count >= 2)
            {
                leftInput = group.Binds[0].IsPressed();
                rightInput = group.Binds[1].IsPressed();
            }
            else
            {
                leftInput = (platInput == 0) ? Inputs.LeftGrip : Inputs.LeftTrigger;
                rightInput = (platInput == 0) ? Inputs.RightGrip : Inputs.RightTrigger;
            }
            if (leftInput)
            {
                if (platL == null)
                {
                    platL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platL.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platL.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    platL.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                    platL.GetComponent<Renderer>().material.shader = Shader.Find("Sprites/Default");
                }
                Renderer rendL = platL.GetComponent<Renderer>();
                if (platMode == 0) rendL.enabled = false;
                else
                {
                    rendL.enabled = true;
                    rendL.material.color = new Color(Core.BaseColor.r, Core.BaseColor.g, Core.BaseColor.b, (platMode == 1) ? 0.5f : 1f);
                }
            }
            else if (platL != null) { if (platL.GetComponent<Renderer>() != null) { UnityEngine.Object.Destroy(platL.GetComponent<Renderer>().material); } UnityEngine.Object.Destroy(platL); platL = null; }
            if (rightInput)
            {
                if (platR == null)
                {
                    platR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platR.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platR.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    platR.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                    platR.GetComponent<Renderer>().material.shader = Shader.Find("Sprites/Default");
                }
                Renderer rendR = platR.GetComponent<Renderer>();
                if (platMode == 0) rendR.enabled = false;
                else
                {
                    rendR.enabled = true;
                    rendR.material.color = new Color(Core.BaseColor.r, Core.BaseColor.g, Core.BaseColor.b, (platMode == 1) ? 0.5f : 1f);
                }
            }
            else if (platR != null) { if (platR.GetComponent<Renderer>() != null) { UnityEngine.Object.Destroy(platR.GetComponent<Renderer>().material); } UnityEngine.Object.Destroy(platR); platR = null; }
        }
        private static string[] modeNames = new string[] {"Normal"};
   
        public static void ChangeFlySpeed(bool forward)
        {
            if (forward)
                speedIndex = (speedIndex + 1) % speedOptions.Length;
            else
                speedIndex = (speedIndex - 1 + speedOptions.Length) % speedOptions.Length;

            flyspeed = speedOptions[speedIndex];
            if (ExtraButtons.FlySpeedButton != null)
                ExtraButtons.FlySpeedButton.Name = $"Fly Speed: {speedNames[speedIndex]}";
        }

        public static void ChangeSpeedBoostSpeed(bool forward)
        {
            if (forward)
                speedIndex2 = (speedIndex2 + 1) % speedOptions2.Length;
            else
                speedIndex2 = (speedIndex2 - 1 + speedOptions2.Length) % speedOptions2.Length;

            jumpspeed = speedOptions2[speedIndex2];
            jumpmultiplier = multiplierOptions[speedIndex2];
            GTPlayer.Instance.maxJumpSpeed = jumpspeed;
            GTPlayer.Instance.jumpMultiplier = jumpmultiplier;
            
            if (ExtraButtons.SpeedBoostSpeedButton != null)
                ExtraButtons.SpeedBoostSpeedButton.Name = $"Boost: {speedNames2[speedIndex2]}";
        }

        public static void ChangePlatformType(bool forward)
        {
            if (forward)
                platMode = (platMode + 1) % modeNames.Length;
            else
                platMode = (platMode - 1 + modeNames.Length) % modeNames.Length;

            string message = $"Platform Type Changed To: {modeNames[platMode]}";
            NotifiLib.SendNotification("", message, 2.5f, NotifiLib.NotifiReason.Success);
        }

        public static Vector3? checkpointPos = null;
        public static GameObject orb = null;
        public static void Checkpoint()
        {
            var group = ButtonConfigs.Get("Check Point");
            bool setInput = group != null && group.Binds.Count >= 1 ? group.Binds[0].IsPressed() : Inputs.RightPrimary;
            bool tpInput = group != null && group.Binds.Count >= 2 ? group.Binds[1].IsPressed() : Inputs.RightSecondary;

            if (setInput)
            {
                checkpointPos = GorillaTagger.Instance.rightHandTransform.transform.position;
                if (orb == null)
                {
                    orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    orb.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    orb.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    Object.Destroy(orb.GetComponent<SphereCollider>());
                }
                orb.transform.position = checkpointPos.Value;
            }

            if (orb != null)
                orb.GetComponent<Renderer>().material.color = Core.BaseColor;

            if (tpInput && checkpointPos != null)
                Movement.TeleportPlayer(checkpointPos.Value);
        }

        public static void DestroyCheckpoint()
        {
            if (orb != null && orb.GetComponent<Renderer>() != null) { Object.Destroy(orb.GetComponent<Renderer>().material); }
            Object.Destroy(orb);
            orb = null;
            checkpointPos = null;
        }
      
        public static void ChangePullStrength()
        {
            pullStrength += 2f;
            if (pullStrength > 20f) pullStrength = 2f;
            NotifiLib.SendNotification("", $"Pull Strength: {pullStrength}", 1.5f, NotifiLib.NotifiReason.Info);
        }

        public static float wallAssistAmount = 0.69f;
        public static void WallAssist()
        {
            
                if (GTPlayer.Instance.rightHand.wasColliding)
                {
                    GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity += -GTPlayer.Instance.rightHand.controllerTransform.up * wallAssistAmount;
                    RaycastHit raycastHit;
                    Physics.Raycast(GTPlayer.Instance.rightHand.controllerTransform.position, -GTPlayer.Instance.rightHand.controllerTransform.up, out raycastHit);
                }
                if (GTPlayer.Instance.leftHand.wasColliding)
                {
                    GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity += -GTPlayer.Instance.leftHand.controllerTransform.up * wallAssistAmount;
                    RaycastHit raycastHit2;
                    Physics.Raycast(GTPlayer.Instance.leftHand.controllerTransform.position, -GTPlayer.Instance.leftHand.controllerTransform.up, out raycastHit2);
                }
            
        }
    

        public static int wallForceIndex = 2;
        public static float activeWallForce = 9.81f;
        public static float[] wallForces = { 2f, 5f, 9.81f, 15f, 50f };
        public static string[] wallForceNames = { "Feeble", "Soft", "Default", "Firm", "Intense" };
        private static Vector3 currentWallNormal = Vector3.up;
        private static bool contactSaved = false;

        public static void AdjustWallWalkStrength(bool increase)
        {
            if (increase)
                wallForceIndex = (wallForceIndex + 1) % wallForces.Length;
            else
                wallForceIndex = (wallForceIndex - 1 + wallForces.Length) % wallForces.Length;

            activeWallForce = wallForces[wallForceIndex];
            if (ExtraButtons.WallWalkStrengthButton != null)
                ExtraButtons.WallWalkStrengthButton.Name = $"Wall Walk: {wallForceNames[wallForceIndex]}";
        }

        public static void WallWalk()
        {
            var group = ButtonConfigs.Get("Wall Walk");
            bool grabInput;
            if (group != null && group.Binds.Count > 0 && group.Binds[0].VRInput != VRButton.None)
            {
                grabInput = group.Binds[0].IsPressed();
            }
            else
            {
                grabInput = Inputs.RightGrip || Inputs.LeftGrip;
            }
            
            if (GTPlayer.Instance.rightHand.wasColliding || GTPlayer.Instance.leftHand.wasColliding)
            {
                Transform hand = GTPlayer.Instance.rightHand.wasColliding ? GTPlayer.Instance.rightHand.controllerTransform : GTPlayer.Instance.leftHand.controllerTransform;
                if (Physics.Raycast(hand.position, -hand.up, out RaycastHit hit, 0.5f) || 
                    Physics.Raycast(hand.position, hand.forward, out hit, 0.5f))
                {
                    currentWallNormal = hit.normal;
                    contactSaved = true;
                }
            }

            if (!grabInput)
                contactSaved = false;

            if (contactSaved && grabInput)
            {
                GorillaTagger.Instance.rigidbody.AddForce(-currentWallNormal * activeWallForce, ForceMode.Acceleration);
                ZeroGravity();
            }
        }

        public static void LegitimateWallWalk()
        {
            float maxRange = 0.25f;
            float legitPull = 2.5f;

            if (Inputs.LeftGrip)
            {
                Transform leftHand = GTPlayer.Instance.leftHand.controllerTransform;
                if (Physics.Raycast(leftHand.position, -leftHand.up, out RaycastHit hitL, maxRange))
                {
                    GorillaTagger.Instance.rigidbody.AddForce(-hitL.normal * legitPull, ForceMode.Acceleration);
                }
            }

            if (Inputs.RightGrip)
            {
                Transform rightHand = GTPlayer.Instance.rightHand.controllerTransform;
                if (Physics.Raycast(rightHand.position, -rightHand.up, out RaycastHit hitR, maxRange))
                {
                    GorillaTagger.Instance.rigidbody.AddForce(-hitR.normal * legitPull, ForceMode.Acceleration);
                }
            }
        }
        public static void FastSwim()
        {
            GTPlayer.Instance.swimmingParams.swimmingVelocityOutOfWaterDrainRate = 25f;
        }
        public static void FixWater()
        {
            GTPlayer.Instance.swimmingParams.swimmingVelocityOutOfWaterDrainRate = 5f;
        }
        public static void PlayspaceAbuse()
        {
            if (Inputs.RightPrimary)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.bodyCollider.transform.forward * (Time.deltaTime * 5f);
                if (!GTPlayer.Instance.IsGroundedHand)
                {
                    if (GTPlayer.Instance.transform.position.y > 1f)
                    {
                        GorillaTagger.Instance.rigidbody.linearVelocity = new Vector3(
                            GorillaTagger.Instance.rigidbody.linearVelocity.x,
                            -15f,
                            GorillaTagger.Instance.rigidbody.linearVelocity.z
                        );
                    }
                }
                else
                {
                    GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                }
            }
        }
        private static float pullStrength = 15f;
        private static float uphillBypass = 0.8f;
        private static float downhillBypass = 1.5f;

        public static void PullBoost()
        {
            GTPlayer player = GTPlayer.Instance;
            if (player == null) return;
            if (!Inputs.RightPrimary) return;
            if (Inputs.LeftGrip && Inputs.RightTrigger && Inputs.LeftTrigger &&
                Inputs.LeftPrimary && Inputs.RightSecondary && Inputs.LeftSecondary)
                return;
            if (!player.leftHand.wasColliding && !player.rightHand.wasColliding) return;
            Vector3 moveDir = player.bodyCollider.transform.forward;
            float currentStrength = pullStrength;
            RaycastHit groundHit;
            if (Physics.Raycast(player.transform.position + Vector3.up * 0.5f, Vector3.down, out groundHit, 2f))
            {
                float angle = Vector3.Angle(groundHit.normal, Vector3.up);
                if (angle > 5f)
                {
                    Vector3 slopeDir = Vector3.ProjectOnPlane(moveDir, groundHit.normal).normalized;
                    bool goingUp = Vector3.Dot(moveDir, groundHit.normal) < 0;
                    if (goingUp)
                    {
                        currentStrength = pullStrength * uphillBypass;
                        moveDir = slopeDir;
                    }
                    else
                    {
                        currentStrength = pullStrength * downhillBypass;
                        moveDir = slopeDir;
                    }
                }
            }
            Rigidbody rb = player.bodyCollider.attachedRigidbody;
            Vector3 originalVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
            player.transform.position += moveDir * (Time.deltaTime * currentStrength);
            rb.linearVelocity = originalVelocity;
        }

        private static float pullStrength2 = 0.15f;

        public static void PullMod()
        {
            GTPlayer player = GTPlayer.Instance;
            if (player == null) return;

            if (!Inputs.RightGrip) return;

            if (Inputs.LeftGrip && Inputs.RightTrigger && Inputs.LeftTrigger &&
                Inputs.RightPrimary && Inputs.LeftPrimary && Inputs.RightSecondary && Inputs.LeftSecondary)
                return;

            if (player.leftHand.wasColliding || player.rightHand.wasColliding)
            {
                Rigidbody rb = GorillaTagger.Instance.rigidbody;
                Vector3 originalVelocity = rb.linearVelocity;

                rb.linearVelocity = Vector3.zero;

                Vector3 velocity = originalVelocity;
                velocity.x *= pullStrength2;
                velocity.y = 0f;
                velocity.z *= pullStrength2;

                Vector3 newPos = player.transform.position + velocity;
                player.transform.position = newPos;

                rb.linearVelocity = originalVelocity;
            }
        }
        public static float delay = 0f;
        public static void Dash()
        {
            if (Inputs.RightPrimary && Time.time > delay)
            {
                delay = Time.time + 1f;
                Vector3 dashDirection = GorillaTagger.Instance.headCollider.transform.forward;
                GTPlayer.Instance.transform.position += dashDirection * flyspeed;
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }
        public static void WalkSim()
        {
            if (GTPlayer.Instance == null) return;

            // Mouse look
            if (UnityInput.Current.GetMouseButton(1))
            {
                var look = UnityInput.Current.mousePosition - pos;
                Camera.main.transform.localEulerAngles += new Vector3(-look.y * .3f, look.x * .3f, 0);
            }
            pos = UnityInput.Current.mousePosition;

            var tr = GTPlayer.Instance.bodyCollider.transform;
            var rb = GTPlayer.Instance.bodyCollider.attachedRigidbody;

            // Get input
            Vector2 joystick = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis(SteamVR_Input_Sources.LeftHand);
            Vector2 stick = joystick.magnitude > .05f ? joystick : new Vector2(
                (UnityInput.Current.GetKey(KeyCode.D) ? 1 : 0) - (UnityInput.Current.GetKey(KeyCode.A) ? 1 : 0),
                (UnityInput.Current.GetKey(KeyCode.W) ? 1 : 0) - (UnityInput.Current.GetKey(KeyCode.S) ? 1 : 0));

            // Normalize stick input
            if (stick.magnitude > 1f) stick.Normalize();

            var armLength = 0.56f;
            var walkSpeed = 6f;
            var direction = (tr.forward * stick.y + tr.right * stick.x).normalized;

            // Sprint
            bool isSprinting = Inputs.LeftJoystick || UnityInput.Current.GetKey(KeyCode.LeftShift);
            if (isSprinting) walkSpeed *= 2.5f;

            // Movement with ground collision
            if (stick.magnitude > 0.05f)
            {
                // Calculate new position with collision
                Vector3 newPosition = tr.position + direction * walkSpeed * Time.deltaTime;

                // Ground raycast to maintain floor contact
                RaycastHit groundHit;
                if (Physics.Raycast(newPosition + Vector3.up * 0.5f, Vector3.down, out groundHit, 1.5f))
                {
                    newPosition.y = groundHit.point.y + 0.1f;
                    rb.linearVelocity = new Vector3(direction.x * walkSpeed, rb.linearVelocity.y, direction.z * walkSpeed);
                    tr.position = newPosition;
                }
                else
                {
                    // Fall if no ground
                    rb.linearVelocity = new Vector3(direction.x * walkSpeed, rb.linearVelocity.y - 9.81f * Time.deltaTime, direction.z * walkSpeed);
                }

                // Realistic VR-like arm swing animation
                float stepSpeed = walkSpeed * (isSprinting ? 1.5f : 1f);
                float swingTime = Time.time * stepSpeed;

                // Calculate swing intensity based on movement speed
                float swingIntensity = Mathf.Lerp(0.5f, 1.2f, Mathf.Clamp01(walkSpeed / 15f));

                // Right hand animation (follows movement rhythm)
                float rightHandSwingX = Mathf.Sin(swingTime) * (stick.y * armLength * swingIntensity);
                float rightHandSwingZ = Mathf.Sin(swingTime + Mathf.PI) * (Mathf.Abs(stick.x) * 0.15f);
                float rightHandVertical = -.3f + (Mathf.Cos(swingTime * 2f) * 0.15f);

                // Left hand animation (opposite rhythm)
                float leftHandSwingX = -Mathf.Sin(swingTime) * (stick.y * armLength * swingIntensity);
                float leftHandSwingZ = Mathf.Sin(swingTime) * (Mathf.Abs(stick.x) * 0.15f);
                float leftHandVertical = -.3f + (Mathf.Cos(swingTime * 2f + Mathf.PI) * 0.15f);

                // Add sideways arm movement for turning
                float turnSwing = direction.x * 0.2f;

                // Apply hand positions
                GTPlayer.Instance.rightHand.controllerTransform.position = tr.position +
                    tr.forward * (rightHandSwingX + turnSwing) +
                    tr.right * (rightHandSwingZ - 0.25f + (stick.x * 0.1f)) +
                    new Vector3(0, rightHandVertical, 0);

                GTPlayer.Instance.leftHand.controllerTransform.position = tr.position +
                    tr.forward * (leftHandSwingX - turnSwing) +
                    tr.right * (leftHandSwingZ + 0.25f + (stick.x * 0.1f)) +
                    new Vector3(0, leftHandVertical, 0);

                // Body tilt based on movement direction
                float bodyTilt = Mathf.Lerp(-5f, 5f, (stick.x + 1f) / 2f);
                tr.rotation = Quaternion.Euler(0, tr.eulerAngles.y, bodyTilt * Mathf.Sin(swingTime * 2f) * 0.5f);

                // Head bob for realism
                float headBob = Mathf.Sin(swingTime * 2f) * 0.03f;
                Camera.main.transform.localPosition = new Vector3(0, headBob, 0);
            }
            else
            {
                // Idle animation - slight breathing movement
                float idleBreath = Mathf.Sin(Time.time * 2f) * 0.01f;

                GTPlayer.Instance.rightHand.controllerTransform.localPosition = new Vector3(0.25f, -0.3f + idleBreath, -0.2f);
                GTPlayer.Instance.leftHand.controllerTransform.localPosition = new Vector3(-0.25f, -0.3f + idleBreath, -0.2f);

                // Reset head bob
                Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, Vector3.zero, Time.deltaTime * 5f);

                // Reset body tilt
                tr.rotation = Quaternion.Euler(0, tr.eulerAngles.y, Mathf.Lerp(tr.eulerAngles.z, 0, Time.deltaTime * 10f));

                // Slow down when not moving
                rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.95f, rb.linearVelocity.y, rb.linearVelocity.z * 0.95f);
            }

            // Jump with ground check
            if ((UnityInput.Current.GetKeyDown(KeyCode.Space) || Inputs.RightPrimary) && IsGroundedPC())
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * 12f, ForceMode.Impulse);

                // Jump animation - arms throw up
                GTPlayer.Instance.rightHand.controllerTransform.position += Vector3.up * 0.3f;
                GTPlayer.Instance.leftHand.controllerTransform.position += Vector3.up * 0.3f;
            }

            // Gravity for when not grounded
            if (!IsGroundedPC())
            {
                rb.linearVelocity += Vector3.down * 9.81f * Time.deltaTime;
            }
        }
        private static bool IsGroundedPC()
        {
            if (GTPlayer.Instance == null) return false;

            RaycastHit hit;
            float checkDistance = 0.2f;
            Vector3 feetPos = GTPlayer.Instance.bodyCollider.transform.position;

            return Physics.Raycast(feetPos + Vector3.up * 0.1f, Vector3.down, out hit, checkDistance + 0.2f);
        }
        public static void BarkFly()
        {
            GTPlayer player = GTPlayer.Instance;
            if (player == null) return;
            Rigidbody rb = player.bodyCollider.attachedRigidbody;
            rb.AddForce(-Physics.gravity * rb.mass * player.scale);
            Vector2 leftAxis = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.axis;
            float rightY = SteamVR_Actions.gorillaTag_RightJoystick2DAxis.axis.y;
            Vector3 movementVector = new Vector3(leftAxis.x, rightY, leftAxis.y);
            Vector3 forward = player.bodyCollider.transform.forward;
            forward.y = 0f;
            Vector3 right = player.bodyCollider.transform.right;
            right.y = 0f;
            Vector3 velocity = (movementVector.x * right) + (movementVector.y * Vector3.up) + (movementVector.z * forward);
            velocity *= player.scale * 10f;
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, velocity, 0.01f);
        }




    }
}





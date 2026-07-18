using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using static Juul.Patches.TelemetryPatches;
using Application = UnityEngine.Application;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Text = UnityEngine.UI.Text;

namespace Juul
{
    public class Players
    {
        public static int Longinde = 0;

        private static bool LRI;
        private static bool GS;
        private static bool IS;
        private static bool LLI;
        private static bool WRE;
        private static int? noInvisLayerMask;
        private static Vector3? oldLocalPosition;

        public static void InvisibleMonke()
        {
            bool inputPressed = ExtraButtons.ghostview
                ? ControllerInputPoller.instance.leftControllerPrimaryButton
                : ControllerInputPoller.instance.leftControllerSecondaryButton;

            inputPressed = inputPressed || UnityInput.Current.GetKey(KeyCode.T);

            if (inputPressed && !LLI)
            {
                IS = !IS;

                if (IS)
                {
                    WRE = VRRig.LocalRig.enabled;
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position =
                        GorillaTagger.Instance.bodyCollider.transform.position - Vector3.up * 99999f;
                }
                else
                {
                    VRRig.LocalRig.enabled = WRE;
                    Visual.RigColorFix();
                }
            }

            if (IS)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position =
                    GorillaTagger.Instance.bodyCollider.transform.position - Vector3.up * 99999f;
            }

            LLI = inputPressed;
        }

        public static void GhostMonke()
        {
            bool input = Inputs.RightSecondary;

            if (input && !LRI)
            {
                GS = !GS;

                if (GS)
                {
                    if (ExtraButtons.ghostview)
                    {
                        Renderer rigRenderer = VRRig.LocalRig.mainSkin.GetComponent<Renderer>();
                        rigRenderer.material.shader = Shader.Find("GUI/Text Shader");
                        Color hollowColor = Color.white;
                        hollowColor.a = 0.3f;
                        rigRenderer.material.color = hollowColor;
                    }
                    VRRig.LocalRig.enabled = false;
                }
                else
                {
                    Visual.RigColorFix();
                    VRRig.LocalRig.enabled = true;
                }
            }
            LRI = input;
        }

        public static void GhostviewClean()
        {
            ExtraButtons.ghostview = false;
            GS = false;
            Visual.RigColorFix();
            VRRig.LocalRig.enabled = true;
        }

        public static void SSRgbMonkey()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(NetworkSystem.Instance.LocalPlayer.UserId))
                {
                    Color color = Color.Lerp(Color.red, Color.blue, Mathf.PingPong(Time.time, 1));

                    VRRig.LocalRig.netView.GetView.RPC("RPC_InitializeNoobMaterial", RpcTarget.All, new object[]
                    {
                        color.r,
                        color.g,
                        color.b
                    });
                }
            }
        }

        private static bool wasNoclipSpam = false;
        public static void Noclip()
        {
            if (Inputs.RightPrimary)
            {
                if (!wasNoclipSpam)
                {
                    foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                        v.enabled = false;
                    wasNoclipSpam = true;
                }
            }
            else
            {
                if (wasNoclipSpam)
                {
                    foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                        v.enabled = true;
                    wasNoclipSpam = false;
                }
            }
        }

        public static void DisableLongArms()
        {
            GTPlayer.Instance.transform.localScale = Vector3.one * VRRig.LocalRig.NativeScale;
        }

        public static void SArms()
        {
            GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(0.77f, 0.77f, 0.77f);
        }

        public static void RArms()
        {
            GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
        }

        public static void TArms()
        {
            GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        public static float armLength = 1f;
        public static float minArmLength = 0.5f;
        public static float maxArmLength = 3f;
        public static void ChangeArmLenth()
        {
            if (Inputs.RightGrip)
            {
                armLength += Time.deltaTime * 2f;
                if (armLength > maxArmLength) armLength = maxArmLength;
                GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f) * armLength;
                if (GorillaTagger.Instance.offlineVRRig != null)
                {
                    GorillaTagger.Instance.offlineVRRig.transform.localScale = new Vector3(1f, 1f, 1f) * armLength;
                }
            }
            if (Inputs.LeftGrip)
            {
                armLength -= Time.deltaTime * 2f;
                if (armLength < minArmLength) armLength = minArmLength;

                GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f) * armLength;

                if (GorillaTagger.Instance.offlineVRRig != null)
                {
                    GorillaTagger.Instance.offlineVRRig.transform.localScale = new Vector3(1f, 1f, 1f) * armLength;
                }
            }
            if (Inputs.RightPrimary)
            {
                armLength = 1f;
                GorillaLocomotion.GTPlayer.Instance.transform.localScale = Vector3.one;

                if (GorillaTagger.Instance.offlineVRRig != null)
                {
                    GorillaTagger.Instance.offlineVRRig.transform.localScale = Vector3.one;
                }
            }
        }

        public static void Spinbot()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GTPlayer.Instance.bodyCollider.transform.position + new Vector3(0f, 0.15f, 0f);
                VRRig.LocalRig.transform.Rotate(new Vector3(0f, 10f, 0f));
            }
            else
            {
                VRRig.LocalRig.enabled = true;
            }
        }
        public static void BayBlade()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GTPlayer.Instance.bodyCollider.transform.position + new Vector3(0f, 0.5f, 0f);
                VRRig.LocalRig.transform.Rotate(new Vector3(0f, 15f, 0f));
                VRRig.LocalRig.leftHandTransform.localPosition = new Vector3(-1f, 0f, 0f);
                VRRig.LocalRig.rightHandTransform.localPosition = new Vector3(1f, 0f, 0f);
            }
            else
            {
                VRRig.LocalRig.enabled = true;
                VRRig.LocalRig.leftHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.rightHandTransform.localPosition = Vector3.zero;
            }
        }
        public static void TPose()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.leftHandTransform.localPosition = new Vector3(-0.8f, 0.5f, 0f);
                VRRig.LocalRig.rightHandTransform.localPosition = new Vector3(0.8f, 0.5f, 0f);
            }
            else
            {
                VRRig.LocalRig.enabled = true;
                VRRig.LocalRig.leftHandTransform.localPosition = new Vector3(-0.35f, -0.2f, -0.1f);
                VRRig.LocalRig.rightHandTransform.localPosition = new Vector3(0.35f, -0.2f, -0.1f);
            }
        }
        public static void Ragdoll()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                Rigidbody rb = VRRig.LocalRig.gameObject.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = VRRig.LocalRig.gameObject.AddComponent<Rigidbody>();
                }
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            else
            {
                VRRig.LocalRig.enabled = true;
                Rigidbody rb = VRRig.LocalRig.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
            }
        }
        public static void SpazRig()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;

                float time = Time.time * 10f;
                VRRig.LocalRig.transform.position = GTPlayer.Instance.bodyCollider.transform.position + new Vector3(
                    Mathf.Sin(time) * 0.5f,
                    Mathf.Cos(time * 1.5f) * 0.5f,
                    Mathf.Sin(time * 1.3f) * 0.5f
                );
            }
            else
            {
                VRRig.LocalRig.enabled = true;
            }
        }
        public static void SpazHands()
        {
            if (Inputs.RightGrip)
            {
                float time = Time.time * 10f;

                VRRig.LocalRig.leftHandTransform.localPosition = new Vector3(
                    Mathf.Sin(time * 2f) * 0.5f,
                    Mathf.Cos(time * 2.5f) * 0.5f,
                    Mathf.Sin(time * 1.8f) * 0.3f
                );

                VRRig.LocalRig.rightHandTransform.localPosition = new Vector3(
                    Mathf.Cos(time * 2.2f) * 0.5f,
                    Mathf.Sin(time * 2.7f) * 0.5f,
                    Mathf.Sin(time * 2f) * 0.3f
                );
            }
            else
            {
                VRRig.LocalRig.leftHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.rightHandTransform.localPosition = Vector3.zero;
            }
        }
        private static float fakeLagTimer = 0f;
        private static bool fakeLagEnabled = false;

        public static void FakeLag()
        {
            if (Inputs.RightGrip)
            {
                if (!fakeLagEnabled)
                {
                    fakeLagEnabled = true;
                    fakeLagTimer = 0f;
                }

                fakeLagTimer += Time.deltaTime;

                if (fakeLagTimer >= 1f)
                {
                    fakeLagTimer = 0f;
                    VRRig.LocalRig.enabled = !VRRig.LocalRig.enabled;
                }
            }
            else
            {
                if (fakeLagEnabled)
                {
                    fakeLagEnabled = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void PCButtonClick()
        {
            if (Mouse.current.leftButton.isPressed)
            {
                noInvisLayerMask ??= ~(
                    1 << LayerMask.NameToLayer("TransparentFX") |
                    1 << LayerMask.NameToLayer("Ignore Raycast") |
                    1 << LayerMask.NameToLayer("Zone") |
                    1 << LayerMask.NameToLayer("Gorilla Boundary") |
                    1 << LayerMask.NameToLayer("GorillaParticle"));
                int layerMask = noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
                if (Core.TPC != null)
                {
                    Ray ray = Core.TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out var Ray, 512f, layerMask))
                    {
                        oldLocalPosition ??= GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition;
                        var follow = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent("TransformFollow");
                        if (follow != null) { ((MonoBehaviour)follow).enabled = false; }
                        GorillaTagger.Instance.rightHandTriggerCollider.transform.position = Ray.point;
                    }
                }
            }
            else
            {
                if (oldLocalPosition != null)
                {
                    GorillaTagger.Instance.rightHandTriggerCollider.transform.localPosition = oldLocalPosition.Value;
                    oldLocalPosition = null;
                }
                var follow = GorillaTagger.Instance.rightHandTriggerCollider.GetComponent("TransformFollow");
                if (follow != null) { ((MonoBehaviour)follow).enabled = true; }
            }
        }
        public static void StareAtClosestPlayer()
        {
            VRRig closestRig = GetClosestPlayer();
            if (closestRig != null)
            {
                VRRig.LocalRig.headConstraint.LookAt(closestRig.transform.position + new Vector3(0f, 0.4f, 0f));
            }
        }
        public static void FixHead()
        {
            VRRig.LocalRig.head.trackingRotationOffset = Vector3.zero;
            VRRig.LocalRig.headConstraint.rotation = GTPlayer.Instance.headCollider.transform.rotation;
        }
        private static VRRig GetClosestPlayer()
        {
            VRRig closestRig = null;
            float closestDistance = float.MaxValue;
            Vector3 myPosition = VRRig.LocalRig.transform.position;
            foreach (VRRig rig in VRRigCache.ActiveRigs.Where(r => r != VRRig.LocalRig))
            {
                if (rig != null)
                {
                    float distance = Vector3.Distance(myPosition, rig.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestRig = rig;
                    }
                }
            }
            return closestRig;
        }
        public static void SpinHeadX()
        {
            VRMap head = VRRig.LocalRig.head;
            head.trackingRotationOffset.x = head.trackingRotationOffset.x + 10f;
        }
        public static void SpinHeadY()
        {
            VRMap head = VRRig.LocalRig.head;
            head.trackingRotationOffset.y = head.trackingRotationOffset.y + 10f;
        }
        public static void SpinHeadZ()
        {
            VRMap head = VRRig.LocalRig.head;
            head.trackingRotationOffset.z = head.trackingRotationOffset.z + 10f;
        }
        public static void SpazHead()
        {
            VRMap head = VRRig.LocalRig.head;
            head.trackingRotationOffset.x = head.trackingRotationOffset.x + 15f;
            head.trackingRotationOffset.y = head.trackingRotationOffset.y + 15f;
            head.trackingRotationOffset.z = head.trackingRotationOffset.z + 15f;
        }
        public static void BackwardsHead()
        {
            VRRig.LocalRig.head.trackingRotationOffset.y = 180f;
        }
        public static void BreakNeck()
        {
            VRRig.LocalRig.head.trackingRotationOffset.y = 75f;
        }
        public static void UpsidedownHead()
        {
            VRRig.LocalRig.head.trackingRotationOffset.x = 180f;
        }
        private static float currentSize = 1f;

        public static void SizeChanger()
        {
            if (Inputs.RightSecondary)
            {
                Players.currentSize = 1f;
            }
            Players.currentSize += (ControllerInputPoller.instance.rightControllerIndexFloat - ControllerInputPoller.instance.rightControllerGripFloat) * 0.04f;
            if (Players.currentSize < 0.01f)
            {
                Players.currentSize = 0.01f;
            }
            float clampedSize = Mathf.Clamp(Players.currentSize, 0.1f, 10f);
            VRRig.LocalRig.transform.localScale = Vector3.one * clampedSize;
            GTPlayer.Instance.transform.localScale = Vector3.one * clampedSize;
        }

        public static void GrabRig()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GTPlayer.Instance.rightHand.controllerTransform.position;
            }
            else if (Inputs.LeftGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GTPlayer.Instance.leftHand.controllerTransform.position;
            }
            else
            {
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void TeleportToRandomPlayer()
        {
            if (Inputs.RightGrip)
            {
                List<VRRig> otherRigs = VRRigCache.ActiveRigs.Where(r => r != VRRig.LocalRig && r != null).ToList();
                if (otherRigs.Count > 0)
                {
                    VRRig target = otherRigs[Random.Range(0, otherRigs.Count)];
                    GTPlayer.Instance.TeleportTo(target.transform.position, target.transform.rotation);
                }
            }
        }

        public static void TeleportToClosestPlayer()
        {
            if (Inputs.RightGrip)
            {
                VRRig closest = GetClosestPlayer();
                if (closest != null)
                {
                    GTPlayer.Instance.TeleportTo(closest.transform.position, closest.transform.rotation);
                }
            }
        }
        private static Quaternion fakefullbodyTrackinggRefRot = Quaternion.identity;
        private static Quaternion fakefullbodyTrackinggCurRot = Quaternion.identity;
        private static Quaternion fakefullbodyTrackinggTgtRot = Quaternion.identity;
        private static bool fakefullbodyTrackinggInputLast;
        private static bool isFakeFBTCamHooked = false;
        private static Quaternion originalCamRot;

        public static void FakefullbodyTrackingg()
        {
            if (!isFakeFBTCamHooked)
            {
                Camera.onPreCull += OnPreCullFakeFBT;
                Camera.onPostRender += OnPostRenderFakeFBT;
                isFakeFBTCamHooked = true;
            }

            Transform head = GTPlayer.Instance.headCollider.transform;
            var group = ButtonConfigs.Get("Fake FBT");
            bool button = group != null && group.Binds.Count > 0 ? group.Binds[0].IsPressed() : ControllerInputPoller.instance.leftControllerPrimaryButton;

            if (button && !fakefullbodyTrackinggInputLast)
                fakefullbodyTrackinggRefRot = head.rotation;

            fakefullbodyTrackinggInputLast = button;

            fakefullbodyTrackinggTgtRot = button
                ? head.rotation * Quaternion.Inverse(fakefullbodyTrackinggRefRot)
                : Quaternion.identity;

            float t = 1f - Mathf.Exp(-5f * Time.deltaTime);
            fakefullbodyTrackinggCurRot = Quaternion.Slerp(fakefullbodyTrackinggCurRot, fakefullbodyTrackinggTgtRot, t);

            Transform rig = GorillaTagger.Instance.offlineVRRig.transform;
            rig.position = head.position + (fakefullbodyTrackinggCurRot * (rig.position - head.position));
            rig.rotation = fakefullbodyTrackinggCurRot * rig.rotation;
        }

        public static void DisableFakefullbodyTrackingg()
        {
            fakefullbodyTrackinggCurRot = Quaternion.identity;
            fakefullbodyTrackinggTgtRot = Quaternion.identity;
            
            if (isFakeFBTCamHooked)
            {
                Camera.onPreCull -= OnPreCullFakeFBT;
                Camera.onPostRender -= OnPostRenderFakeFBT;
                isFakeFBTCamHooked = false;
            }
        }

        private static void OnPreCullFakeFBT(Camera cam)
        {
            if (cam == GorillaTagger.Instance.mainCamera)
            {
                originalCamRot = cam.transform.rotation;
                cam.transform.rotation = fakefullbodyTrackinggCurRot * originalCamRot;
            }
        }

        private static void OnPostRenderFakeFBT(Camera cam)
        {
            if (cam == GorillaTagger.Instance.mainCamera)
            {
                cam.transform.rotation = originalCamRot;
            }
        }
    }
}

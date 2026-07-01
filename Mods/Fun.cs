using BepInEx;
using com.AnotherAxiom.Paddleball;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag.Gravity;
using GorillaTagScripts;
using Liv.Lck.GorillaTag;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using static GorillaNetworking.CosmeticsController;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using JoinType = GorillaNetworking.JoinType;

namespace Juul
{
    internal class Fun
    {

        private static Hashtable rpcFilterByViewId = new Hashtable();
        public static void FlushRPCS()
        {
            rpcFilterByViewId[0] = GorillaTagger.Instance.myVRRig.ViewID;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                CachingOption = EventCaching.RemoveFromRoomCache,
                TargetActors = new int[]
                {
                        PhotonNetwork.LocalPlayer.ActorNumber
                }
            };
            MonkeAgent.instance.rpcErrorMax = int.MaxValue;
            MonkeAgent.instance.rpcCallLimit = int.MaxValue;
            MonkeAgent.instance.logErrorMax = int.MaxValue;
            PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
            PhotonNetwork.QuickResends = int.MaxValue;
            PhotonNetwork.SendAllOutgoingCommands();
            PhotonNetwork.NetworkingClient.OpRaiseEvent(200, rpcFilterByViewId, raiseEventOptions, SendOptions.SendReliable);
        }

        public static void DropHoverBoard(Vector3 pos, Quaternion rot, Vector3 vel)
        {
            if (PhotonNetwork.IsConnected)
            {
                FreeHoverboardManager.instance.SendDropBoardRPC(pos, rot, vel, vel, Color.clear);
                FlushRPCS();
            }
        }
        public static float delay;
        public static void HoverboardSpammer()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.5f;
                    DropHoverBoard(GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.rightHandTransform.transform.up * 1f);
                }
            }
            if (ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.5f;
                    DropHoverBoard(GorillaTagger.Instance.leftHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.leftHandTransform.transform.up * 1f);
                }
            }
        }
        public static void HoverboardMinigun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.5f;
                    DropHoverBoard(GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.rightHandTransform.transform.forward * 10f);
                }
            }
            if (ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.5f;
                    DropHoverBoard(GorillaTagger.Instance.leftHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.leftHandTransform.transform.forward * 10f);
                }
            }
        }
        public static void HoverboardSniper()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.5f;
                    DropHoverBoard(GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.rightHandTransform.transform.forward * 50f);
                }
            }
            if (ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.5f;
                    DropHoverBoard(GorillaTagger.Instance.leftHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.leftHandTransform.transform.forward * 50f);
                }
            }
        }
        public static void HoverboardSniper2()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.5f;
                    DropHoverBoard(GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.rightHandTransform.transform.forward * 100f);
                }
            }
            if (ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.5f;
                    DropHoverBoard(GorillaTagger.Instance.leftHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.leftHandTransform.transform.forward * 100f);
                }
            }
        }
        public static void HoverboardGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.spherepointer != null && Time.time > delay)
                {
                    delay = Time.time + 0.5f;
                    if (PhotonNetwork.IsConnected && FreeHoverboardManager.instance != null)
                    {
                        FreeHoverboardManager.instance.SendDropBoardRPC(
                            GunLib.spherepointer.transform.position,
                            Quaternion.identity,
                            Vector3.zero,
                            Vector3.zero,
                            Color.clear);
                    }
                }
            }, false);
        }
        public static void PlayRandomSounds()
        {
            if (PhotonNetwork.IsConnected)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, UnityEngine.Random.Range(0, GTPlayer.Instance.materialData.Count), false, 999999f);
                FlushRPCS();
            }
        }
        public static void PlayJmanYell()
        {
            if (PhotonNetwork.IsConnected)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 337, false, 1f);
                FlushRPCS();
            }
        }
        public static void SpamJmanYell()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.25f;
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 337, false, 1f);
                }
                FlushRPCS();
            }
        }

        public static void SIUnlockAll()
        {
            foreach (bool[] gadget in SIProgression.Instance.unlockedTechTreeData)
            {
                Array.Fill(gadget, true);
            }
        }
        public static void YoinkTerms()
        {
            foreach (SICombinedTerminal term in SuperInfectionManager.activeSuperInfectionManager.zoneSuperInfection.siTerminals)
            {
                term.PlayerHandScanned(NetworkSystem.Instance.LocalPlayer.ActorNumber);
            }
        }
        public static void GiveAllResources()
        {
            var prog = SIProgression.Instance;
            foreach (SIResource.ResourceType type in Enum.GetValues(typeof(SIResource.ResourceType)))
            {
                prog.resourceDict[type] = 999999;
            }
            SIPlayer.SetAndBroadcastProgression();
        }
        public static void CompleteAllQuests()
        {
            var prog = SIProgression.Instance;
            for (int i = 0; i < prog.ActiveQuestIds.Length; i++)
            {
                if (prog.ActiveQuestIds[i] != -1)
                {
                    prog.AttemptRedeemCompletedQuest(i);
                }
            }
        }
        public static void AlwaysOwnTerminals()
        {
            var manager = SuperInfectionManager.activeSuperInfectionManager;
            foreach (var term in manager.zoneSuperInfection.siTerminals)
            {
                term.activePlayer = SIPlayer.LocalPlayer;
                term.isOccupiedByLocalPlayer = true;
            }
        }
        public static void DisableTerminalTimeout()
        {
            var manager = SuperInfectionManager.activeSuperInfectionManager;
            foreach (var term in manager.zoneSuperInfection.siTerminals)
            {
                term.foldupDelay = float.MaxValue;
            }
        }

        public static void SoundSpammer(int num)
        {
            if (Inputs.RightGrip || Inputs.LeftGrip)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.1f;
                    if (PhotonNetwork.InRoom)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, num, false, 999999f);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(num, false, 999999f);
                    }
                }
            }
            FlushRPCS();
        }
        public static void SqueakSoundSpam2() => SoundSpammer(187);
        public static void SqueakSoundSpam3() => SoundSpammer(222);
        public static void SqueakSoundSpam4() => SoundSpammer(275);
        public static void StaticSoundSpam() => SoundSpammer(64);
        public static void StaticSoundSpam2() => SoundSpammer(72);
        public static void StaticSoundSpam3() => SoundSpammer(196);
        public static void StaticSoundSpam4() => SoundSpammer(210);
        public static void StaticSoundSpam5() => SoundSpammer(259);
        public static void WoodSoundSpam() => SoundSpammer(7);
        public static void WoodSoundSpam2() => SoundSpammer(101);
        public static void WoodSoundSpam3() => SoundSpammer(112);
        public static void WoodSoundSpam4() => SoundSpammer(1);
        public static void BassSoundSpam() => SoundSpammer(68);
        public static void MetalSoundSpam() => SoundSpammer(18);
        public static void MetalSoundSpam2() => SoundSpammer(98);
        public static void MetalSoundSpam3() => SoundSpammer(57);
        public static void PopSoundSpam() => SoundSpammer(84);
        public static void WolfSoundSpam() => SoundSpammer(195);
        public static void CatSoundSpam() => SoundSpammer(236);
        public static void TurkeySoundSpam() => SoundSpammer(83);
        public static void FrogSoundSpam() => SoundSpammer(91);
        public static void BeeSoundSpam() => SoundSpammer(191);
        public static void SqueakSoundSpam() => SoundSpammer(215);
        public static void EarrapeSoundSpam() => SoundSpammer(215);
        public static void DingSoundSpam() => SoundSpammer(244);
        public static void DingSoundSpam2() => SoundSpammer(269);
        public static void BigCrystalSoundSpam() => SoundSpammer(213);
        public static void CrystalSoundSpam() => SoundSpammer(20);
        public static void PanSoundSpam() => SoundSpammer(248);
        public static void AK47SoundSpam() => SoundSpammer(203);
        public static void TickSoundSpam() => SoundSpammer(148);
        public static void CarpetSoundSpam() => SoundSpammer(93);
        public static void ScarySoundSpam() => SoundSpammer(283);
        public static void PianoSoundSpam()
        {
            if (Inputs.RightGrip || Inputs.LeftGrip)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.1f;
                    int randomPiano = UnityEngine.Random.Range(295, 308);
                    if (PhotonNetwork.InRoom)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, randomPiano, false, 999999f);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(randomPiano, false, 999999f);
                    }
                }
            }
            FlushRPCS();
        }
        public static void WideFOV()
        {
            Camera.main.fieldOfView = 130f;
        }
        public static void NormalFOV()
        {
            Camera.main.fieldOfView = 90f;
        }
        public static void LookFOV()
        {
            Camera.main.fieldOfView = 1f;
        }
        private static bool isShaking = false;
        public static void SeizureCamera()
        {
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f && !isShaking)
            {
                GorillaTagger.Instance.StartCoroutine(CameraShake());
            }
        }
        private static IEnumerator CameraShake()
        {
            isShaking = true;
            Vector3 originalPos = Camera.main.transform.position;
            for (int i = 0; i < 10; i++)
            {
                Camera.main.transform.position = originalPos + new Vector3(
                    UnityEngine.Random.Range(-0.2f, 0.2f),
                    UnityEngine.Random.Range(-0.1f, 0.1f),
                    0
                );
                yield return new WaitForSeconds(0.02f);
            }
            Camera.main.transform.position = originalPos;
            isShaking = false;
        }
        public static void RandomSoundSpam()
        {
            if (Inputs.RightGrip || Inputs.LeftGrip)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.1f;
                    int randomSound = UnityEngine.Random.Range(0, 350);
                    if (PhotonNetwork.InRoom)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, randomSound, false, 999999f);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(randomSound, false, 999999f);
                    }
                }
            }
            FlushRPCS();
        }

        public static void SirenSoundSpam()
        {
            int[] sirenSounds = new int[] { 250, 251, 252, 253 };
            if (Inputs.RightGrip || Inputs.LeftGrip)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.2f;
                    int sirenSound = sirenSounds[UnityEngine.Random.Range(0, sirenSounds.Length)];
                    if (PhotonNetwork.InRoom)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, sirenSound, false, 999999f);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(sirenSound, false, 999999f);
                    }
                }
            }
            FlushRPCS();
        }

        public static bool nettrigsoff = false, qboff = false;

        public static void nettriggers()
        {
            if (nettrigsoff) { GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(false); }
            else { GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(true); }
        }
        public static void quitbox()
        {
            if (qboff) { GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(false); }
            else { GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(true); }
        }
        public static void FlashMonkey()
        {
            float speed = 15f;
            float t = Mathf.Sin(Time.time * speed);
            Color c = Color.HSVToRGB(Mathf.Abs(t), 1f, 1f);
            if (PhotonNetwork.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, c.r, c.g, c.b);
            }
        }
        public static void FadeMonkey()
        {
            float h = Mathf.Repeat(Time.time * 0.2f, 1f);
            Color c = Color.HSVToRGB(h, 1f, 1f);
            if (PhotonNetwork.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, c.r, c.g, c.b);
            }
        }

        public static void FadeMonkeyHardRGB()
        {
            float t = Time.time * 0.5f;
            float r = Mathf.PingPong(t * 2f, 1f);
            float g = Mathf.PingPong(t * 2f + 0.66f, 1f);
            float b = Mathf.PingPong(t * 2f + 1.33f, 1f);
            r = Mathf.Clamp01(r);
            g = Mathf.Clamp01(g);
            b = Mathf.Clamp01(b);

            if (PhotonNetwork.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, r, g, b);
            }
        }
        public static void BAWFlashMonkey()
        {
            float t = Time.time * 5f;
            float val = Mathf.PingPong(t, 1f) > 0.5f ? 1f : 0f;

            if (PhotonNetwork.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, val, val, val);
            }
        }

        public static void ForgetAllCredentials()
        {
            PlayFabSettings.staticPlayer.ForgetAllCredentials();
        }
        public static void GrabBug()
        {
            if (Inputs.RightGrip)
            {
                GameObject targetObject = GameObject.Find("Floating Bug Holdable");
                Vector3 handPos = GTPlayer.Instance.rightHand.controllerTransform.position;
                targetObject.transform.position = handPos;
                targetObject.transform.SetParent(GTPlayer.Instance.rightHand.controllerTransform);
            }
            if (Inputs.LeftGrip)
            {
                GameObject targetObject2 = GameObject.Find("Floating Bug Holdable");
                Vector3 handPos2 = GTPlayer.Instance.leftHand.controllerTransform.position;
                targetObject2.transform.position = handPos2;
                targetObject2.transform.SetParent(GTPlayer.Instance.leftHand.controllerTransform);
            }
        }
        public static void GrabBat()
        {
            if (Inputs.RightGrip)
            {
                GameObject targetObject = GameObject.Find("Cave Bat Holdable");
                Vector3 handPos = GTPlayer.Instance.rightHand.controllerTransform.position;
                targetObject.transform.position = handPos;
                targetObject.transform.SetParent(GTPlayer.Instance.rightHand.controllerTransform);
            }
            if (Inputs.LeftGrip)
            {
                GameObject targetObject2 = GameObject.Find("Cave Bat Holdable");
                Vector3 handPos2 = GTPlayer.Instance.leftHand.controllerTransform.position;
                targetObject2.transform.position = handPos2;
                targetObject2.transform.SetParent(GTPlayer.Instance.leftHand.controllerTransform);
            }
        }
        public static void GrabFirefly()
        {
            if (Inputs.RightGrip)
            {
                GameObject targetObject = GameObject.Find("Firefly");
                Vector3 handPos = GTPlayer.Instance.rightHand.controllerTransform.position;
                targetObject.transform.position = handPos;
                targetObject.transform.SetParent(GTPlayer.Instance.rightHand.controllerTransform);
            }
            if (Inputs.LeftGrip)
            {
                GameObject targetObject2 = GameObject.Find("Firefly");
                Vector3 handPos2 = GTPlayer.Instance.leftHand.controllerTransform.position;
                targetObject2.transform.position = handPos2;
                targetObject2.transform.SetParent(GTPlayer.Instance.leftHand.controllerTransform);
            }
        }
        /*public static void GrabCamera()
        {
            if (Inputs.RightGrip)
            {
                LckSocialCamera rightCamera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
                rightCamera.visible = true;
                rightCamera.recording = true;
                rightCamera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                rightCamera.m_CameraVisuals.SetRecordingState(true);
                rightCamera.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                rightCamera.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                rightCamera.transform.SetParent(GorillaTagger.Instance.rightHandTransform);
            }
            if (Inputs.LeftGrip)
            {
                LckSocialCamera leftCamera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
                leftCamera.visible = true;
                leftCamera.recording = true;
                leftCamera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                leftCamera.m_CameraVisuals.SetRecordingState(true);
                leftCamera.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                leftCamera.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                leftCamera.transform.SetParent(GorillaTagger.Instance.leftHandTransform);
            }
        }*/
        public static void OrbitBug()
        {
            GameObject targetObject = GameObject.Find("Floating Bug Holdable");
            float angle = Time.time * 15f;
            Vector3 orbitPos = GTPlayer.Instance.transform.position +
            new Vector3(Mathf.Cos(angle) * 1.5f, 0.5f, Mathf.Sin(angle) * 1.5f);
            targetObject.transform.position = orbitPos;
            targetObject.transform.Rotate(Vector3.up, 15f * 50f * Time.deltaTime);
        }
        public static void OrbitBat()
        {
            GameObject targetObject = GameObject.Find("Cave Bat Holdable");
            float angle = Time.time * 15f;
            Vector3 orbitPos = GTPlayer.Instance.transform.position +
            new Vector3(Mathf.Cos(angle) * 1.5f, 0.5f, Mathf.Sin(angle) * 1.5f);
            targetObject.transform.position = orbitPos;
            targetObject.transform.Rotate(Vector3.up, 15f * 50f * Time.deltaTime);
        }
        public static void OrbitFirefly()
        {
            GameObject targetObject = GameObject.Find("Firefly");
            float angle = Time.time * 15f;
            Vector3 orbitPos = GTPlayer.Instance.transform.position +
            new Vector3(Mathf.Cos(angle) * 1.5f, 0.5f, Mathf.Sin(angle) * 1.5f);
            targetObject.transform.position = orbitPos;
            targetObject.transform.Rotate(Vector3.up, 15f * 50f * Time.deltaTime);
        }
        /*public static void OrbitCamera()
        {
            LckSocialCamera camera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
            camera.visible = true;
            camera.recording = true;
            camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
            camera.m_CameraVisuals.SetRecordingState(true);
            float angle = Time.time * 10f;
            Vector3 orbitPos = GTPlayer.Instance.transform.position +
            new Vector3(Mathf.Cos(angle) * 2f, 1f, Mathf.Sin(angle) * 2f);
            camera.transform.position = orbitPos;
            camera.transform.LookAt(GTPlayer.Instance.transform.position);
        }*/
        public static void SpazBug()
        {
            GameObject targetObject = GameObject.Find("Floating Bug Holdable");
            Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
            new Vector3(
                Mathf.Sin(Time.time * 20f) * 1f,
                Mathf.Cos(Time.time * 25f) * 1f,
                Mathf.Sin(Time.time * 15f) * 1f
            );
            targetObject.transform.position = spazPos;
            targetObject.transform.rotation = Quaternion.Euler(
                Time.time * 200f,
                Time.time * 300f,
                Time.time * 100f
            );
        }
        public static void SpazBat()
        {
            GameObject targetObject = GameObject.Find("Cave Bat Holdable");
            Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
            new Vector3(
                Mathf.Sin(Time.time * 20f) * 1f,
                Mathf.Cos(Time.time * 25f) * 1f,
                Mathf.Sin(Time.time * 15f) * 1f
            );
            targetObject.transform.position = spazPos;
            targetObject.transform.rotation = Quaternion.Euler(
                Time.time * 200f,
                Time.time * 300f,
                Time.time * 100f
            );
        }
        public static void SpazFirefly()
        {
            GameObject targetObject = GameObject.Find("Firefly");
            Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
            new Vector3(
                Mathf.Sin(Time.time * 20f) * 1f,
                Mathf.Cos(Time.time * 25f) * 1f,
                Mathf.Sin(Time.time * 15f) * 1f
            );
            targetObject.transform.position = spazPos;
            targetObject.transform.rotation = Quaternion.Euler(
                Time.time * 200f,
                Time.time * 300f,
                Time.time * 100f
            );
        }
        /*public static void SpazCamera()
        {
            LckSocialCamera camera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
            camera.visible = true;
            camera.recording = true;
            camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
            camera.m_CameraVisuals.SetRecordingState(true);
            Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
            new Vector3(
                Mathf.Sin(Time.time * 30f) * 2f,
                Mathf.Cos(Time.time * 40f) * 1.5f,
                Mathf.Sin(Time.time * 25f) * 2f
            );
            camera.transform.position = spazPos;
            camera.transform.rotation = Quaternion.Euler(
                Mathf.Sin(Time.time * 20f) * 360f,
                Mathf.Cos(Time.time * 15f) * 360f,
                Mathf.Sin(Time.time * 10f) * 360f
            );
        }*/
        public static void DestroyBug()
        {
            GameObject targetObject = GameObject.Find("Floating Bug Holdable");
            targetObject.transform.position = new Vector3(999f, 999f, 999f);
        }
        public static void DestroyBat()
        {
            GameObject targetObject = GameObject.Find("Cave Bat Holdable");
            targetObject.transform.position = new Vector3(999f, 999f, 999f);
        }
        public static void DestroyFirefly()
        {
            GameObject targetObject = GameObject.Find("Firefly");
            targetObject.transform.position = new Vector3(999f, 999f, 999f);
        }
        /*public static void DestroyCamera()
        {
            LckSocialCamera camera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
            camera.visible = false;
            camera.recording = false;
            camera.m_CameraVisuals.SetNetworkedVisualsActive(false);
            camera.m_CameraVisuals.SetRecordingState(false);
            camera.transform.position = new Vector3(999f, 999f, 999f);
        }*/

        private static SocialCoconutCamera coconutCamera;
        private static float cameraDelay = 0f;

        private static void FindCoconutCamera()
        {
            if (coconutCamera == null)
            {
                coconutCamera = GameObject.FindObjectOfType<SocialCoconutCamera>();
            }
        }

        public static void GrabCamera()
        {
            FindCoconutCamera();
            if (coconutCamera == null) return;

            if (Inputs.RightGrip)
            {
                coconutCamera.SetVisualsActive(true);
                coconutCamera.SetRecordingState(true);
                coconutCamera.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                coconutCamera.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                coconutCamera.transform.SetParent(GorillaTagger.Instance.rightHandTransform);
            }
            if (Inputs.LeftGrip)
            {
                coconutCamera.SetVisualsActive(true);
                coconutCamera.SetRecordingState(true);
                coconutCamera.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                coconutCamera.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                coconutCamera.transform.SetParent(GorillaTagger.Instance.leftHandTransform);
            }
        }

        public static void OrbitCamera()
        {
            FindCoconutCamera();
            if (coconutCamera == null) return;

            coconutCamera.SetVisualsActive(true);
            coconutCamera.SetRecordingState(true);
            float angle = Time.time * 10f;
            Vector3 orbitPos = GTPlayer.Instance.transform.position +
                new Vector3(Mathf.Cos(angle) * 2f, 1f, Mathf.Sin(angle) * 2f);
            coconutCamera.transform.position = orbitPos;
            coconutCamera.transform.LookAt(GTPlayer.Instance.transform.position);
        }

        public static void SpazCamera()
        {
            FindCoconutCamera();
            if (coconutCamera == null) return;

            coconutCamera.SetVisualsActive(true);
            coconutCamera.SetRecordingState(true);
            Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
                new Vector3(
                    Mathf.Sin(Time.time * 30f) * 2f,
                    Mathf.Cos(Time.time * 40f) * 1.5f,
                    Mathf.Sin(Time.time * 25f) * 2f
                );
            coconutCamera.transform.position = spazPos;
            coconutCamera.transform.rotation = Quaternion.Euler(
                Mathf.Sin(Time.time * 20f) * 360f,
                Mathf.Cos(Time.time * 15f) * 360f,
                Mathf.Sin(Time.time * 10f) * 360f
            );
        }

        public static void DestroyCamera()
        {
            FindCoconutCamera();
            if (coconutCamera == null) return;

            coconutCamera.SetVisualsActive(false);
            coconutCamera.SetRecordingState(false);
            coconutCamera.transform.position = new Vector3(999f, 999f, 999f);
            coconutCamera.transform.SetParent(null);
        }
        public static void FlashCameraRecording()
        {
            FindCoconutCamera();
            if (coconutCamera == null) return;

            if (Time.time > cameraDelay)
            {
                cameraDelay = Time.time + 0.5f;
                var isRecordingField = typeof(SocialCoconutCamera).GetField("_isActive",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (isRecordingField != null)
                {
                    bool current = (bool)isRecordingField.GetValue(coconutCamera);
                    coconutCamera.SetRecordingState(!current);
                    coconutCamera.SetVisualsActive(true);
                }
            }
        }

        public static void GrabTablet()
        {
            if (Inputs.RightGrip)
            {
                LckSocialCamera camera = LckSocialCameraManager.Instance._networkedTablet;
                if (camera != null)
                {
                    camera.visible = true;
                    camera.recording = true;
                    camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                    camera.m_CameraVisuals.SetRecordingState(true);
                    camera.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    camera.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                    camera.transform.SetParent(GorillaTagger.Instance.rightHandTransform);
                }
            }
            if (Inputs.LeftGrip)
            {
                LckSocialCamera camera = LckSocialCameraManager.Instance._networkedTablet;
                if (camera != null)
                {
                    camera.visible = true;
                    camera.recording = true;
                    camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                    camera.m_CameraVisuals.SetRecordingState(true);
                    camera.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    camera.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                    camera.transform.SetParent(GorillaTagger.Instance.leftHandTransform);
                }
            }
        }
        public static void OrbitTablet()
        {
            LckSocialCamera tablet = LckSocialCameraManager.Instance._networkedTablet;
            if (tablet != null)
            {
                tablet.visible = true;
                tablet.recording = true;
                tablet.m_CameraVisuals.SetNetworkedVisualsActive(true);
                tablet.m_CameraVisuals.SetRecordingState(true);
                float angle = Time.time * 10f;
                Vector3 orbitPos = GTPlayer.Instance.transform.position +
                new Vector3(Mathf.Cos(angle) * 2f, 1f, Mathf.Sin(angle) * 2f);
                tablet.transform.position = orbitPos;
                tablet.transform.LookAt(GTPlayer.Instance.transform.position);
            }
        }

        public static void SpazTablet()
        {
            LckSocialCamera tablet = LckSocialCameraManager.Instance._networkedTablet;
            if (tablet != null)
            {
                tablet.visible = true;
                tablet.recording = true;
                tablet.m_CameraVisuals.SetNetworkedVisualsActive(true);
                tablet.m_CameraVisuals.SetRecordingState(true);
                Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
                new Vector3(
                    Mathf.Sin(Time.time * 30f) * 2f,
                    Mathf.Cos(Time.time * 40f) * 1.5f,
                    Mathf.Sin(Time.time * 25f) * 2f
                );
                tablet.transform.position = spazPos;
                tablet.transform.rotation = Quaternion.Euler(
                    Mathf.Sin(Time.time * 20f) * 360f,
                    Mathf.Cos(Time.time * 15f) * 360f,
                    Mathf.Sin(Time.time * 10f) * 360f
                );
            }
        }

        public static void DestroyTablet()
        {
            LckSocialCamera tablet = LckSocialCameraManager.Instance._networkedTablet;
            if (tablet != null)
            {
                tablet.visible = false;
                tablet.recording = false;
                tablet.m_CameraVisuals.SetNetworkedVisualsActive(false);
                tablet.m_CameraVisuals.SetRecordingState(false);
                tablet.transform.position = new Vector3(999f, 999f, 999f);
                tablet.transform.SetParent(null);
            }
        }
        private static List<GliderHoldable> cachedGliders = new List<GliderHoldable>();
        private static List<BalloonHoldable> cachedBalloons = new List<BalloonHoldable>();
        private static float lastGliderRefresh = 0f;
        private static float lastBalloonRefresh = 0f;
        private static float lastOrbitTime = 0f;
        private static float lastSpazTime = 0f;

        private static void RefreshGliders()
        {
            if (Time.time - lastGliderRefresh > 1f)
            {
                cachedGliders.Clear();
                cachedGliders.AddRange(Resources.FindObjectsOfTypeAll<GliderHoldable>());
                lastGliderRefresh = Time.time;
            }
        }

        private static void RefreshBalloons()
        {
            if (Time.time - lastBalloonRefresh > 1f)
            {
                cachedBalloons.Clear();
                cachedBalloons.AddRange(Resources.FindObjectsOfTypeAll<BalloonHoldable>());
                lastBalloonRefresh = Time.time;
            }
        }

        public static List<GliderHoldable> GetAllGliders()
        {
            RefreshGliders();
            return cachedGliders;
        }

        public static List<BalloonHoldable> GetAllBalloons()
        {
            RefreshBalloons();
            return cachedBalloons;
        }

        public static void GrabGlider()
        {
            if (!Inputs.RightGrip && !Inputs.LeftGrip) return;

            RefreshGliders();
            bool isRightGrip = Inputs.RightGrip;
            Transform handTransform = isRightGrip ?
                GorillaTagger.Instance.rightHandTransform :
                GorillaTagger.Instance.leftHandTransform;

            foreach (GliderHoldable glider in cachedGliders)
            {
                if (glider == null) continue;

                if (glider.IsMine)
                {
                    glider.transform.position = handTransform.position;
                    glider.transform.rotation = handTransform.rotation;

                    if (glider.transform.parent != handTransform)
                    {
                        glider.transform.SetParent(handTransform);
                    }
                }
                else if (NetworkSystem.Instance.InRoom)
                {
                    glider.OnHover(null, null);
                }
            }
        }

        private static float orbitAngle = 0f;
        public static void OrbitGlider()
        {
            if (Time.time - lastOrbitTime < 0.033f) return; 
            lastOrbitTime = Time.time;

            RefreshGliders();
            if (GTPlayer.Instance == null) return;

            orbitAngle += Time.deltaTime * 8f; 
            Vector3 playerPos = GTPlayer.Instance.transform.position;

            foreach (GliderHoldable glider in cachedGliders)
            {
                if (glider?.IsMine != true) continue;

                float cos = Mathf.Cos(orbitAngle);
                float sin = Mathf.Sin(orbitAngle);

                Vector3 orbitPos = playerPos + new Vector3(cos * 2f, 1f, sin * 2f);
                glider.transform.position = orbitPos;
                glider.transform.LookAt(playerPos);
            }
        }

        private static float spazSeedX, spazSeedY, spazSeedZ, spazRotX, spazRotY, spazRotZ;
        private static bool spazInitialized = false;

        public static void SpazGlider()
        {
            if (Time.time - lastSpazTime < 0.033f) return;
            lastSpazTime = Time.time;

            RefreshGliders();
            if (GTPlayer.Instance == null) return;

            if (!spazInitialized)
            {
                spazSeedX = UnityEngine.Random.Range(10f, 50f);
                spazSeedY = UnityEngine.Random.Range(10f, 50f);
                spazSeedZ = UnityEngine.Random.Range(10f, 50f);
                spazRotX = UnityEngine.Random.Range(10f, 30f);
                spazRotY = UnityEngine.Random.Range(10f, 30f);
                spazRotZ = UnityEngine.Random.Range(10f, 30f);
                spazInitialized = true;
            }

            Vector3 playerPos = GTPlayer.Instance.transform.position;
            float time = Time.time;

            foreach (GliderHoldable glider in cachedGliders)
            {
                if (glider?.IsMine != true) continue;

                Vector3 spazPos = playerPos + Vector3.up + new Vector3(
                    Mathf.Sin(time * spazSeedX) * 2f,
                    Mathf.Cos(time * spazSeedY) * 1.5f,
                    Mathf.Sin(time * spazSeedZ) * 2f
                );

                glider.transform.position = spazPos;
                glider.transform.rotation = Quaternion.Euler(
                    Mathf.Sin(time * spazRotX) * 360f,
                    Mathf.Cos(time * spazRotY) * 360f,
                    Mathf.Sin(time * spazRotZ) * 360f
                );
            }
        }

        public static void DestroyGlider()
        {
            RefreshGliders();
            Vector3 voidPos = new Vector3(999f, 999f, 999f);

            foreach (GliderHoldable glider in cachedGliders)
            {
                if (glider?.IsMine == true)
                {
                    glider.transform.position = voidPos;
                    glider.Respawn();
                }
            }
        }

        public static void GrabBalloons()
        {
            if (!Inputs.RightGrip && !Inputs.LeftGrip) return;

            RefreshBalloons();
            bool isRightGrip = Inputs.RightGrip;
            Transform handTransform = isRightGrip ?
                GorillaTagger.Instance.rightHandTransform :
                GorillaTagger.Instance.leftHandTransform;

            foreach (BalloonHoldable balloon in cachedBalloons)
            {
                if (balloon == null) continue;

                if (balloon.ownerRig.isLocal)
                {
                    balloon.gameObject.transform.position = handTransform.position;
                    balloon.gameObject.transform.rotation = handTransform.rotation;

                    if (balloon.gameObject.transform.parent != handTransform)
                    {
                        balloon.gameObject.transform.SetParent(handTransform);
                    }
                }
                else
                {
                    balloon.WorldShareableRequestOwnership();
                }
            }
        }

        private static float balloonOrbitAngle = 0f;
        public static void OrbitBalloons()
        {
            if (Time.time - lastOrbitTime < 0.033f) return;

            RefreshBalloons();
            if (GTPlayer.Instance == null) return;

            balloonOrbitAngle += Time.deltaTime * 4f;
            Vector3 playerPos = GTPlayer.Instance.transform.position;

            foreach (BalloonHoldable balloon in cachedBalloons)
            {
                if (balloon?.ownerRig.isLocal != true) continue;

                float cos = Mathf.Cos(balloonOrbitAngle);
                float sin = Mathf.Sin(balloonOrbitAngle);

                Vector3 orbitPos = playerPos + new Vector3(
                    cos * 2f,
                    Mathf.Sin(balloonOrbitAngle * 1.5f) * 1.5f + 1.5f,
                    sin * 2f
                );

                balloon.transform.position = orbitPos;
                balloon.transform.LookAt(playerPos);
            }
        }

        public static void SpazBalloons()
        {
            if (Time.time - lastSpazTime < 0.033f) return;

            RefreshBalloons();
            if (GTPlayer.Instance == null) return;

            float time = Time.time;
            Vector3 playerPos = GTPlayer.Instance.transform.position;

            foreach (BalloonHoldable balloon in cachedBalloons)
            {
                if (balloon?.ownerRig.isLocal != true) continue;

                Vector3 spazPos = playerPos + Vector3.up + new Vector3(
                    Mathf.Sin(time * 25f) * 1.5f,
                    Mathf.Cos(time * 30f) * 1.5f,
                    Mathf.Sin(time * 20f) * 1.5f
                );

                balloon.transform.position = spazPos;
                balloon.transform.rotation = Quaternion.Euler(
                    time * 200f,
                    time * 250f,
                    time * 150f
                );
            }
        }

        public static void DestroyBalloons()
        {
            RefreshBalloons();
            Vector3 voidPos = new Vector3(999f, 999f, 999f);

            foreach (BalloonHoldable balloon in cachedBalloons)
            {
                if (balloon?.ownerRig.isLocal == true)
                {
                    balloon.transform.position = voidPos;
                    balloon.WorldShareableRequestOwnership();
                }
            }
        }

        public static void PopAllBalloons()
        {
            RefreshBalloons();

            foreach (BalloonHoldable balloon in cachedBalloons)
            {
                if (balloon != null)
                {
                    if (balloon.ownerRig.isLocal)
                    {
                        balloon.PopBalloon();
                    }
                    else
                    {
                        balloon.PopBalloonRemote();
                    }
                }
            }
        }

        public static void EnableGoldNameTag()
        {
            VRRig.LocalRig.ShowGoldNameTag = true;
            VRRig.LocalRig.playerText1.color = SubscriptionManager.SUBSCRIBER_NAME_COLOR;
        }

        public static void DisableGoldNameTag()
        {
            VRRig.LocalRig.ShowGoldNameTag = false;
        }

        public static float flashTimer = 0f;

        public static void FlashGoldNameTag()
        {
            if (Time.time > flashTimer)
            {
                flashTimer = Time.time + 0.1f;

                if (VRRig.LocalRig.ShowGoldNameTag)
                {
                    VRRig.LocalRig.ShowGoldNameTag = false;
                    VRRig.LocalRig.playerText1.color = Color.white;
                }
                else
                {
                    VRRig.LocalRig.ShowGoldNameTag = true;
                    VRRig.LocalRig.playerText1.color = SubscriptionManager.SUBSCRIBER_NAME_COLOR;
                }
            }
        }

        public static void BypassNameChange(string newName)
        {
            var computer = GorillaComputer.instance;
            computer.currentName = newName;
            computer.savedName = newName;
            NetworkSystem.Instance.SetMyNickName(newName);
            PlayerPrefs.SetString("playerName", newName);
            PlayerPrefs.Save();
            VRRig.LocalRig.SetNameTagText(newName);
            if (NetworkSystem.Instance.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All,
                    computer.redValue, computer.greenValue, computer.blueValue);
            }
        }




        public static void ChangeNameTo(string newName)
        {
            var computer = GorillaComputer.instance;
            if (computer == null) return;
            computer.currentName = newName;
            computer.savedName = newName;
            NetworkSystem.Instance.SetMyNickName(newName);
            PlayerPrefs.SetString("playerName", newName);
            PlayerPrefs.Save();
            VRRig.LocalRig.SetNameTagText(newName);
        }
        public static void ChangeColor(float r, float g, float b)
        {
            var computer = GorillaComputer.instance;
            if (computer == null) return;
            computer.redValue = Mathf.Clamp(r, 0f, 1f);
            computer.greenValue = Mathf.Clamp(g, 0f, 1f);
            computer.blueValue = Mathf.Clamp(b, 0f, 1f);
            PlayerPrefs.SetFloat("redValue", computer.redValue);
            PlayerPrefs.SetFloat("greenValue", computer.greenValue);
            PlayerPrefs.SetFloat("blueValue", computer.blueValue);
            PlayerPrefs.Save();
            GorillaTagger.Instance.UpdateColor(computer.redValue, computer.greenValue, computer.blueValue);
            if (NetworkSystem.Instance.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All,
                    computer.redValue, computer.greenValue, computer.blueValue);
            }
        }
        public static void CopyColorGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer == null) return;
                VRRig targetRig = GunLib.LockedPlayer;
                if (targetRig == null) return;
                float r = targetRig.playerColor.r;
                float g = targetRig.playerColor.g;
                float b = targetRig.playerColor.b;
                var computer = GorillaComputer.instance;
                if (computer != null)
                {
                    computer.redValue = r;
                    computer.greenValue = g;
                    computer.blueValue = b;
                    PlayerPrefs.SetFloat("redValue", r);
                    PlayerPrefs.SetFloat("greenValue", g);
                    PlayerPrefs.SetFloat("blueValue", b);
                    PlayerPrefs.Save();
                }
                if (PhotonNetwork.InRoom)
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, r, g, b);
                }

            }, true);
        }
        public static CosmeticsController GetCosmetics()
        {
            return CosmeticsController.instance;
        }
        public static void UnlockAllCosmetics()
        {
            var cosmetics = GetCosmetics();
            if (cosmetics == null) return;
            foreach (var item in cosmetics.allCosmetics)
            {
                if (!item.isNullItem && !cosmetics.unlockedCosmetics.Contains(item))
                {
                    cosmetics.unlockedCosmetics.Add(item);
                    switch (item.itemCategory)
                    {
                        case CosmeticsController.CosmeticCategory.Hat:
                            if (!cosmetics.unlockedHats.Contains(item))
                                cosmetics.unlockedHats.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Face:
                            if (!cosmetics.unlockedFaces.Contains(item))
                                cosmetics.unlockedFaces.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Badge:
                            if (!cosmetics.unlockedBadges.Contains(item))
                                cosmetics.unlockedBadges.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Paw:
                            if (!item.isThrowable)
                            {
                                if (!cosmetics.unlockedPaws.Contains(item))
                                    cosmetics.unlockedPaws.Add(item);
                            }
                            else
                            {
                                if (!cosmetics.unlockedThrowables.Contains(item))
                                    cosmetics.unlockedThrowables.Add(item);
                            }
                            break;
                        case CosmeticsController.CosmeticCategory.Fur:
                            if (!cosmetics.unlockedFurs.Contains(item))
                                cosmetics.unlockedFurs.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Shirt:
                            if (!cosmetics.unlockedShirts.Contains(item))
                                cosmetics.unlockedShirts.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Back:
                            if (!cosmetics.unlockedBacks.Contains(item))
                                cosmetics.unlockedBacks.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Arms:
                            if (!cosmetics.unlockedArms.Contains(item))
                                cosmetics.unlockedArms.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Chest:
                            if (!cosmetics.unlockedChests.Contains(item))
                                cosmetics.unlockedChests.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Pants:
                            if (!cosmetics.unlockedPants.Contains(item))
                                cosmetics.unlockedPants.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.TagEffect:
                            if (!cosmetics.unlockedTagFX.Contains(item))
                                cosmetics.unlockedTagFX.Add(item);
                            break;
                    }
                }
            }
            cosmetics.UpdateWardrobeModelsAndButtons();
            cosmetics.OnCosmeticsUpdated?.Invoke();
        }
        public static void GiveUnlimitedShinyRocks()
        {
            var cosmetics = GetCosmetics();
            if (cosmetics == null) return;
            cosmetics.currencyBalance = 999999;
            cosmetics.UpdateCurrencyBoards();
        }
        private static List<TransferrableObject> cachedHoldables = new List<TransferrableObject>();
        private static float lastCacheTime = 0f;
        private static float cacheInterval = 0.5f;

        private static void RefreshHoldablesCache()
        {
            if (Time.time - lastCacheTime > cacheInterval)
            {
                cachedHoldables.Clear();
                var found = Resources.FindObjectsOfTypeAll<TransferrableObject>();
                foreach (var obj in found)
                {
                    if (obj != null)
                    {
                        cachedHoldables.Add(obj);
                    }
                }
                lastCacheTime = Time.time;
            }
        }

        public static void StickHoldables()
        {
            try
            {
                RefreshHoldablesCache();
                foreach (var holdable in cachedHoldables)
                {
                    try
                    {
                        if (holdable == null || holdable.gameObject == null || !holdable.gameObject.activeInHierarchy)
                            continue;

                        if (holdable.currentState == TransferrableObject.PositionState.InLeftHand ||
                            holdable.currentState == TransferrableObject.PositionState.InRightHand)
                        {
                            Transform handTransform = holdable.currentState == TransferrableObject.PositionState.InLeftHand ?
                                GTPlayer.Instance.leftHand.controllerTransform :
                                GTPlayer.Instance.rightHand.controllerTransform;

                            holdable.transform.position = handTransform.position;
                            holdable.transform.rotation = handTransform.rotation;

                            if (holdable.grabAnchor != null)
                            {
                                holdable.grabAnchor.position = handTransform.position;
                                holdable.grabAnchor.rotation = handTransform.rotation;
                            }

                            holdable.interpState = TransferrableObject.InterpolateState.None;

                            if (holdable.rigidbodyInstance != null)
                            {
                                holdable.rigidbodyInstance.isKinematic = true;
                                holdable.rigidbodyInstance.linearVelocity = Vector3.zero;
                                holdable.rigidbodyInstance.angularVelocity = Vector3.zero;
                            }

                            if (holdable.anchor != null)
                            {
                                holdable.anchor.parent = null;
                            }

                            if (holdable.targetDockPositions != null)
                            {
                                holdable.startInterpolation = false;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static void SpinHoldables()
        {
            try
            {
                RefreshHoldablesCache();
                foreach (var holdable in cachedHoldables)
                {
                    try
                    {
                        if (holdable == null || holdable.transform == null)
                            continue;

                        if (holdable.currentState == TransferrableObject.PositionState.InLeftHand ||
                            holdable.currentState == TransferrableObject.PositionState.InRightHand)
                        {
                            holdable.transform.Rotate(Vector3.up, 360f * Time.deltaTime);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static float nextJuggleTime = 0f;
        private static float juggleInterval = 0.3f;
        private static int positionIndex = 0;
        private static TransferrableObject.PositionState[] allPositions = new TransferrableObject.PositionState[]
        {
            TransferrableObject.PositionState.InLeftHand,
            TransferrableObject.PositionState.InRightHand,
            TransferrableObject.PositionState.OnLeftArm,
            TransferrableObject.PositionState.OnRightArm,
            TransferrableObject.PositionState.OnLeftShoulder,
            TransferrableObject.PositionState.OnRightShoulder,
            TransferrableObject.PositionState.OnChest,
            TransferrableObject.PositionState.Dropped
        };

        public static void JuggleHoldables()
        {
            try
            {
                if (Time.time < nextJuggleTime) return;
                nextJuggleTime = Time.time + juggleInterval;
                RefreshHoldablesCache();
                positionIndex = (positionIndex + 1) % allPositions.Length;
                foreach (var holdable in cachedHoldables)
                {
                    try
                    {
                        if (holdable == null || !holdable.gameObject.activeInHierarchy) continue;
                        if (holdable.currentState != TransferrableObject.PositionState.None)
                        {
                            holdable.currentState = allPositions[positionIndex];
                            if (allPositions[positionIndex] == TransferrableObject.PositionState.InLeftHand && holdable.canAutoGrabLeft)
                            {
                                holdable.OnGrab(holdable.gripInteractor, EquipmentInteractor.instance.leftHand);
                            }
                            else if (allPositions[positionIndex] == TransferrableObject.PositionState.InRightHand && holdable.canAutoGrabRight)
                            {
                                holdable.OnGrab(holdable.gripInteractor, EquipmentInteractor.instance.rightHand);
                            }
                            else if (allPositions[positionIndex] == TransferrableObject.PositionState.Dropped)
                            {
                                holdable.DropItem();
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
        public static VirtualStumpSerializer GetTerminalNetwork()
        {
            if (CustomMapsTerminal.instance == null) return null;
            return CustomMapsTerminal.instance.mapTerminalNetworkObject;
        }
        public static void VirtualStumpKickGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                var network = GetTerminalNetwork();
                VRRig targetRig = GunLib.LockedPlayer;
                NetPlayer netPlayer = targetRig.Creator ?? NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(targetRig.rigSerializer.gameObject));
                Photon.Realtime.Player targetPlayer = netPlayer.GetPlayerRef();
                network.photonView.RPC("SetRoomMap_RPC", targetPlayer, -1);
            }, true);
        }
        public static void VirtualStumpKickAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var network = GetTerminalNetwork();
            foreach (var rig in VRRigCache.ActiveRigs)
            {
                if (rig.isOfflineVRRig) continue;
                NetPlayer netPlayer = rig.Creator ?? NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(rig.rigSerializer.gameObject));
                Photon.Realtime.Player targetPlayer = netPlayer.GetPlayerRef();
                network.photonView.RPC("SetRoomMap_RPC", targetPlayer, -1);
            }
        }

        private static Paddleball GetPaddleball()
        {
            return UnityEngine.Object.FindObjectOfType<Paddleball>();
        }
        public static void WinPaddleballLeft()
        {
            Paddleball game = GetPaddleball();
            if (game == null) return;
            Type gameType = typeof(Paddleball);
            FieldInfo scoreLField = gameType.GetField("scoreL", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo scoreRField = gameType.GetField("scoreR", BindingFlags.NonPublic | BindingFlags.Instance);
            if (scoreLField != null && scoreRField != null)
            {
                scoreLField.SetValue(game, 10);
                scoreRField.SetValue(game, 0);
                MethodInfo updateScoreMethod = gameType.GetMethod("UpdateScore",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                updateScoreMethod?.Invoke(game, null);
                MethodInfo changeScreenMethod = gameType.GetMethod("ChangeScreen",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Type screenModeType = gameType.GetNestedType("ScreenMode", BindingFlags.NonPublic);
                if (screenModeType != null && changeScreenMethod != null)
                {
                    Array values = Enum.GetValues(screenModeType);
                    object whiteWin = values.GetValue(2);
                    changeScreenMethod.Invoke(game, new object[] { whiteWin });
                }
            }
        }
        public static void WinPaddleballRight()
        {
            Paddleball game = GetPaddleball();
            if (game == null) return;
            Type gameType = typeof(Paddleball);
            FieldInfo scoreLField = gameType.GetField("scoreL", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo scoreRField = gameType.GetField("scoreR", BindingFlags.NonPublic | BindingFlags.Instance);
            if (scoreLField != null && scoreRField != null)
            {
                scoreLField.SetValue(game, 0);
                scoreRField.SetValue(game, 10);
                MethodInfo updateScoreMethod = gameType.GetMethod("UpdateScore",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                updateScoreMethod?.Invoke(game, null);
                MethodInfo changeScreenMethod = gameType.GetMethod("ChangeScreen",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Type screenModeType = gameType.GetNestedType("ScreenMode", BindingFlags.NonPublic);
                if (screenModeType != null && changeScreenMethod != null)
                {
                    Array values = Enum.GetValues(screenModeType);
                    object blackWin = values.GetValue(3);
                    changeScreenMethod.Invoke(game, new object[] { blackWin });
                }
            }
        }
        public static void SuperFastPaddleballBall()
        {
            Paddleball game = GetPaddleball();
            if (game == null) return;
            Type gameType = typeof(Paddleball);
            FieldInfo gameBallSpeedField = gameType.GetField("gameBallSpeed",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (gameBallSpeedField != null)
            {
                gameBallSpeedField.SetValue(game, 25f);
            }
        }
        public static void MaxQuestScore()
        {
            VRRig.LocalRig.SetQuestScore(int.MaxValue);
        }
        public static void SetCustomQuestScore()
        {
            KeyboardManager.IsSettingQuestScore = true;
            KeyboardManager.QuestScoreQuery = "";
            KeyboardManager.ToggleKeyboard(true);
            KeyboardManager.KeyboardJustOpened = true;
            if (ExtraButtons.QuestScoreButton != null)
                ExtraButtons.QuestScoreButton.Name = "Set Quest Score: " + KeyboardManager.QuestScoreQuery;
        }
        public static void UnlockSubscription()
        {
            if (SubscriptionManager.Instance == null) return;

            Type subscriptionDetailsType = typeof(SubscriptionManager.SubscriptionDetails);

            object details = Activator.CreateInstance(subscriptionDetailsType);

            subscriptionDetailsType.GetField("active").SetValue(details, true);
            subscriptionDetailsType.GetField("daysAccrued").SetValue(details, int.MaxValue);
            subscriptionDetailsType.GetField("tier").SetValue(details, int.MaxValue);
            subscriptionDetailsType.GetField("autoRenew").SetValue(details, true);
            subscriptionDetailsType.GetField("autoRenewMonths").SetValue(details, int.MaxValue);
            subscriptionDetailsType.GetField("subscriptionActiveUntilDate").SetValue(details, DateTime.MaxValue);

            FieldInfo subscriptionFeatureSettingsField = subscriptionDetailsType.GetField("subscriptionFeatureSettings");
            if (subscriptionFeatureSettingsField != null)
            {
                subscriptionFeatureSettingsField.SetValue(details, new[] { true, true });
            }

            typeof(SubscriptionManager).GetField("localSubscriptionDetails", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, details);
            typeof(SubscriptionManager).GetField("_localSubscriptionDataInitialized", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, true);

            if (NetworkSystem.Instance != null && NetworkSystem.Instance.LocalPlayer != null)
            {
                MethodInfo updateMethod = typeof(SubscriptionManager).GetMethod("UpdatePlayerSubsDetails", BindingFlags.NonPublic | BindingFlags.Instance);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(SubscriptionManager.Instance, new object[] { NetworkSystem.Instance.LocalPlayer, true, int.MaxValue });
                }
            }
        }

        private static List<PaperPlaneThrowable> cachedPaperPlanes = new List<PaperPlaneThrowable>();
        private static float lastPaperPlaneRefresh = 0f;
        private static float lastPlaneActionTime = 0f;
        private static float lastPlaneSpamTime = 0f;
        private static float lastBarrageTime = 0f;

        private static List<PaperPlaneThrowable> GetAllPaperPlaneThrowables()
        {
            if (Time.time - lastPaperPlaneRefresh > 0.5f)
            {
                cachedPaperPlanes.Clear();
                cachedPaperPlanes.AddRange(Resources.FindObjectsOfTypeAll<PaperPlaneThrowable>());
                lastPaperPlaneRefresh = Time.time;
            }
            return cachedPaperPlanes;
        }

        public static void SpamPaperPlanes()
        {
            if (Time.time - lastPlaneSpamTime < 0.1f) return;
            lastPlaneSpamTime = Time.time;

            var planes = GetAllPaperPlaneThrowables();
            if (planes.Count == 0) return;

            var interactor = EquipmentInteractor.instance;
            if (interactor == null) return;

            foreach (var plane in planes)
            {
                if (plane != null && plane.gameObject.activeInHierarchy && plane.IsLocalObject())
                {
                    if (plane.currentState == TransferrableObject.PositionState.InLeftHand)
                    {
                        plane.OnRelease(null, interactor.leftHand);
                    }
                    else if (plane.currentState == TransferrableObject.PositionState.InRightHand)
                    {
                        plane.OnRelease(null, interactor.rightHand);
                    }
                }
            }
        }

        public static void RapidPaperPlanes()
        {
            if (!Inputs.RightGrip && !Inputs.LeftGrip) return;
            if (Time.time - lastPlaneActionTime < 0.05f) return;
            lastPlaneActionTime = Time.time;

            var planes = GetAllPaperPlaneThrowables();
            if (planes.Count == 0) return;

            var interactor = EquipmentInteractor.instance;
            if (interactor == null) return;

            bool rightGrip = Inputs.RightGrip;
            bool leftGrip = Inputs.LeftGrip;

            foreach (var plane in planes)
            {
                if (plane?.IsLocalObject() != true) continue;

                if (rightGrip && plane.currentState == TransferrableObject.PositionState.InRightHand)
                {
                    plane.OnRelease(null, interactor.rightHand);
                }
                else if (leftGrip && plane.currentState == TransferrableObject.PositionState.InLeftHand)
                {
                    plane.OnRelease(null, interactor.leftHand);
                }
            }
        }

        public static void PaperPlaneGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (Time.time - lastPlaneSpamTime < 0.1f) return;
                lastPlaneSpamTime = Time.time;

                var planes = GetAllPaperPlaneThrowables();
                if (planes.Count == 0) return;

                var interactor = EquipmentInteractor.instance;
                if (interactor == null) return;

                foreach (var plane in planes)
                {
                    if (plane?.IsLocalObject() != true) continue;

                    if (plane.currentState == TransferrableObject.PositionState.InLeftHand)
                    {
                        plane.OnRelease(null, interactor.leftHand);
                    }
                    else if (plane.currentState == TransferrableObject.PositionState.InRightHand)
                    {
                        plane.OnRelease(null, interactor.rightHand);
                    }
                }
            }, true);
        }

        public static void InfinitePaperPlanes()
        {
            var planes = GetAllPaperPlaneThrowables();
            if (planes.Count == 0) return;

            foreach (var plane in planes)
            {
                if (plane != null && !plane.gameObject.activeInHierarchy)
                {
                    plane.gameObject.SetActive(true);
                    if (plane._renderer != null)
                    {
                        plane._renderer.forceRenderingOff = false;
                    }
                }
            }
        }

        private static System.Reflection.FieldInfo projectileField;
        private static bool projectileFieldInitialized = false;

        public static void PaperPlaneBarrage()
        {
            if (Time.time - lastBarrageTime < 0.2f) return;
            lastBarrageTime = Time.time;

            var planes = GetAllPaperPlaneThrowables();
            if (planes.Count == 0) return;

            if (!projectileFieldInitialized)
            {
                projectileField = typeof(PaperPlaneThrowable).GetField("_projectilePrefab",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                projectileFieldInitialized = true;
            }

            if (projectileField == null) return;

            var player = GTPlayer.Instance;
            if (player == null) return;

            Vector3 center = player.transform.position + Vector3.up * 2f;
            var objectPool = ObjectPools.instance;
            if (objectPool == null) return;

            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0.3f, Mathf.Sin(angle)).normalized;

                foreach (var plane in planes)
                {
                    if (plane?.IsLocalObject() != true) continue;

                    GameObject projectile = projectileField.GetValue(plane) as GameObject;
                    if (projectile == null) continue;

                    GameObject spawned = objectPool.Instantiate(projectile, center, true);
                    if (spawned == null) continue;

                    var projectileComp = spawned.GetComponent<PaperPlaneProjectile>();
                    if (projectileComp != null)
                    {
                        projectileComp.Launch(center, Quaternion.identity, dir * 25f);
                    }
                }
            }
        }

        private static List<RCShip> cachedRCShips = new List<RCShip>();
        private static float lastRCShipRefresh = 0f;
        private static float lastRCFireTime = 0f;
        private static float lastRCSwitchTime = 0f;
        private static float lastRCBarrageTime = 0f;

        private static System.Reflection.FieldInfo cannonToLeftField;
        private static bool cannonFieldInitialized = false;

        private static List<RCShip> GetAllRCShips()
        {
            if (Time.time - lastRCShipRefresh > 0.5f)
            {
                cachedRCShips.Clear();
                cachedRCShips.AddRange(Resources.FindObjectsOfTypeAll<RCShip>());
                lastRCShipRefresh = Time.time;
            }
            return cachedRCShips;
        }

        public static void ForceFireRCShip()
        {
            var ships = GetAllRCShips();
            if (ships.Count == 0) return;

            foreach (var ship in ships)
            {
                if (ship?.gameObject.activeInHierarchy == true && ship.OnFire != null)
                {
                    ship.OnFire.Invoke();
                }
            }
        }

        public static void RapidFireRCShip()
        {
            if (Time.time - lastRCFireTime < 0.1f) return;
            lastRCFireTime = Time.time;

            var ships = GetAllRCShips();
            if (ships.Count == 0) return;

            foreach (var ship in ships)
            {
                if (ship?.OnFire != null)
                {
                    ship.OnFire.Invoke();
                }
            }
        }

        public static void RCShipGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var ships = GetAllRCShips();
                if (ships.Count == 0) return;

                foreach (var ship in ships)
                {
                    if (ship?.OnFire != null)
                    {
                        ship.OnFire.Invoke();
                    }
                }
            }, true);
        }

        public static void BoostRCShipSpeed()
        {
            var ships = GetAllRCShips();
            if (ships.Count == 0) return;

            foreach (var ship in ships)
            {
                if (ship?.rb != null)
                {
                    ship.rb.linearVelocity += ship.transform.forward * 20f;
                }
            }
        }

        public static void BoostRCShipGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                Vector3 direction = GunLib.spherepointer.transform.forward;
                var ships = GetAllRCShips();
                if (ships.Count == 0) return;

                foreach (var ship in ships)
                {
                    if (ship?.rb != null)
                    {
                        ship.rb.linearVelocity = direction * 30f;
                    }
                }
            }, true);
        }

        public static void LaunchRCShipUp()
        {
            var ships = GetAllRCShips();
            if (ships.Count == 0) return;

            foreach (var ship in ships)
            {
                if (ship?.rb != null)
                {
                    ship.rb.linearVelocity = Vector3.up * 25f;
                }
            }
        }

        public static void RCShipBarrage()
        {
            if (Time.time - lastRCBarrageTime < 0.2f) return;
            lastRCBarrageTime = Time.time;

            var ships = GetAllRCShips();
            if (ships.Count == 0) return;

            foreach (var ship in ships)
            {
                if (ship?.OnFire != null)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        ship.OnFire.Invoke();
                    }
                }
            }
        }

        public static void EnableBodySlide()
        {
            GTPlayer.Instance.IsBodySliding = true;
        }

        public static void DisableBodySlide()
        {
            GTPlayer.Instance.IsBodySliding = false;
        }
        private static GorillaComputer computer;
        private static GorillaComputer GetComputer()
        {
            if (computer == null)
            {
                computer = GorillaComputer.instance;
            }
            return computer;
        }
        public static void UnlockCompetitiveQueue()
        {
            var comp = GetComputer();
            if (comp == null) return;
            comp.allowedInCompetitive = true;
            PlayerPrefs.SetInt("allowedInCompetitive", 1);
            PlayerPrefs.Save();
        }
        public static void ForceQueueDefault()
        {
            var comp = GetComputer();
            if (comp == null) return;
            comp.JoinDefaultQueue();
        }

        public static void ForceQueueCompetitive()
        {
            var comp = GetComputer();
            if (comp == null) return;
            comp.JoinQueue("COMPETITIVE", false);
        }

        public static void ForceQueueMinigames()
        {
            var comp = GetComputer();
            if (comp == null) return;
            comp.JoinQueue("MINIGAMES", false);
        }
        private static PhotonNetworkController networkController;
        private static PhotonNetworkController GetNetworkController()
        {
            if (networkController == null)
            {
                networkController = PhotonNetworkController.Instance;
            }
            return networkController;
        }
        public static void GetTotalPlayersOnline()
        {
            var controller = GetNetworkController();
            if (controller == null) return;

            int total = controller.TotalUsers();
            NotifiLib.SendNotification($"Total players online: {total}");
        }












































    }
}





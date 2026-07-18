using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using JetBrains.Annotations;
using Liv.Lck.Telemetry;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EventsModels;
using PlayFab.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using ExitGames.Client.Photon;
using GorillaExtensions;

namespace Juul
{
    internal class Patches
    {
        public class SafetyPatches
        {
            [HarmonyPatch(typeof(MonkeAgent), "SendReport")]
            public class SendReportPatch
            {
                public static bool AntiCheatSelf = false;
                public static bool AntiCheatAll = false;
                public static bool AntiCheatReasonHide = false;
                public static bool AntiACReport = false;

                private static bool Prefix(string susReason, string susId, string susNick)
                {
                    if (susReason.ToLower() == "empty rig")
                        return false;

                    if (AntiACReport)
                    {
                        PhotonNetwork.Disconnect();
                    }

                    return false;
                }
            }

            [HarmonyPatch(typeof(MonkeAgent), "CloseInvalidRoom")]
            public class CloseInvalidRoomPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(MonkeAgent), "CheckReports")]
            public class CheckReportsPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(MonkeAgent), "DispatchReport")]
            public class DispatchReportPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(MonkeAgent), "GetRPCCallTracker")]
            internal class GetRPCCallTrackerPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(MonkeAgent), "LogErrorCount")]
            public class LogErrorCountPatch
            {
                private static bool Prefix(string logString, string stackTrace, LogType type) =>
                    false;
            }

            [HarmonyPatch(typeof(MonkeAgent), "QuitDelay", MethodType.Enumerator)]
            public class QuitDelayPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(GorillaGameManager), "ForceStopGame_DisconnectAndDestroy")]
            public class ForceStopGame_DisconnectAndDestroyPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(CosmeticsController), "ReauthOrBan")]
            public class ReauthOrBanPatch
            {
                private static bool Prefix(PlayFabError error) =>
                    false;
            }

            [HarmonyPatch(typeof(MonkeAgent), "ShouldDisconnectFromRoom")]
            public class ShouldDisconnectFromRoomPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(PlayFabDeviceUtil), "SendDeviceInfoToPlayFab")]
            public class SendDeviceInfoToPlayFabPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(PlayFabClientInstanceAPI), "ReportDeviceInfo")]
            public class ReportDeviceInfoPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(PlayFabClientAPI), "ReportDeviceInfo")]
            public class ReportDeviceInfoPatch2
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(PlayFabDeviceUtil), "GetAdvertIdFromUnity")]
            public class GetAdvertIdFromUnityPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(PlayFabClientAPI), "AttributeInstall")]
            public class AttributeInstallPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(PlayFabHttp), "InitializeScreenTimeTracker")]
            public class InitializeScreenTimeTrackerPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(VRRig), "IncrementRPC", typeof(PhotonMessageInfoWrapped), typeof(string))]
            public class IncrementRPCPatch
            {
                private static bool Prefix(PhotonMessageInfoWrapped info, string sourceCall) =>
                    false;
            }

            [HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCall", typeof(PhotonMessageInfo), typeof(string))]
            public class IncrementRPCCallPatch
            {
                private static bool Prefix(PhotonMessageInfo info, string callingMethod = "") =>
                    false;
            }

            [HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCallLocal")]
            public class IncrementRPCCallLocalPatch
            {
                private static bool Prefix(PhotonMessageInfoWrapped infoWrapped, string rpcFunction) =>
                    false;
            }

            [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.OnEvent))]
            internal class BlockScoreBoardReports
            {
                static bool Prefix(EventData photonEvent)
                {
                    if (photonEvent.Code == 50)
                        if (((object[])photonEvent.CustomData)[0] == PhotonNetwork.LocalPlayer.UserId)
                            return false;
                        else return true;
                    else
                        return true;
                }
            }

            [HarmonyPatch(typeof(MothershipClientApiUnity), nameof(MothershipClientApiUnity.CreateReport))]
            internal class BlockUnityAAReports
            {
                static bool Prefix(string reportedUserId, int category, bool moddedClient, string metadata, Action<CreateReportResponse> successAction, Action<MothershipError, int> errorAction) => false;
            }

            [HarmonyPatch(typeof(MothershipApiPINVOKE), nameof(MothershipApiPINVOKE.MothershipClientApiClient_CreateReport))]
            internal class PatchCreateReport
            {
                private static bool Prefix()
                {
                    return false;
                }
            }

            [HarmonyPatch(typeof(MothershipApiPINVOKE), nameof(MothershipApiPINVOKE.MothershipServerApiClient_ServerCreateBan))]
            internal class PatchServerCreateBan
            {
                private static bool Prefix()
                {
                    return false;
                }
            }

            [HarmonyPatch(typeof(MothershipApiPINVOKE), nameof(MothershipApiPINVOKE.MothershipAutomationApiClient_CreateBan))]
            internal class PatchAutomationCreateBan
            {
                private static bool Prefix()
                {
                    return false;
                }
            }

            [HarmonyPatch(typeof(MothershipApiPINVOKE), nameof(MothershipApiPINVOKE.new_CreateReportRequest))]
            internal class PatchNewCreateReportRequest
            {
                private static bool Prefix()
                {
                    return false;
                }
            }

            [HarmonyPatch(typeof(MothershipApiPINVOKE), nameof(MothershipApiPINVOKE.CreateReportRequest_ToHttpRequest))]
            internal class PatchCreateReportToHttpRequest
            {
                private static bool Prefix()
                {
                    return false;
                }
            }

            [HarmonyPatch(typeof(MothershipApiPINVOKE), nameof(MothershipApiPINVOKE.new_ServerCreateBanRequest))]
            internal class PatchNewServerCreateBanRequest
            {
                private static bool Prefix()
                {
                    return false;
                }
            }

            [HarmonyPatch(typeof(MothershipApiPINVOKE), nameof(MothershipApiPINVOKE.ServerCreateBanRequest_ToHttpRequest))]
            internal class PatchServerCreateBanToHttpRequest
            {
                private static bool Prefix()
                {
                    return false;
                }
            }

            [HarmonyPatch(typeof(MothershipApiPINVOKE), nameof(MothershipApiPINVOKE.new_CreateBanRequest))]
            internal class PatchNewCreateBanRequest
            {
                private static bool Prefix()
                {
                    return false;
                }
            }

            [HarmonyPatch(typeof(MothershipApiPINVOKE), nameof(MothershipApiPINVOKE.CreateBanRequest_ToHttpRequest))]
            internal class PatchCreateBanToHttpRequest
            {
                private static bool Prefix()
                {
                    return false;
                }
            }
        }

        public class GameplayPatches
        {
            [HarmonyPatch(typeof(VRRig), nameof(VRRig.UpdateFriendshipBracelet))]
            public class BraceletPatch
            {
                public static bool enabled;

                public static bool Prefix(VRRig __instance) =>
                    !enabled;
            }

            [HarmonyPatch(typeof(VRRig), nameof(VRRig.SetHandEffectData))]
            public class EffectDataPatch
            {
                public static bool enabled;
                public static bool tapsEnabled = true;
                public static bool doOverride;
                public static float overrideVolume = 99999f;
                public static int tapMultiplier = 1;
                public static int material = -1;

                private static bool Prefix(VRRig __instance, HandEffectContext effectContext, int audioClipIndex, bool isDownTap, bool isLeftHand, float handTapVolume, float handTapSpeed, Vector3 dirFromHitToHand)
                {
                    if (enabled)
                    {
                        if (__instance.isLocal)
                        {
                            if (doOverride)
                            {
                                effectContext.soundFX = VRRig.LocalRig.GetHandSurfaceData(audioClipIndex).audio;
                                effectContext.speed = overrideVolume;
                                effectContext.soundVolume = overrideVolume;

                                if (PhotonNetwork.InRoom)
                                {
                                    if (tapMultiplier > 1)
                                    {
                                        for (int i = 0; i < tapMultiplier; i++)
                                        {
                                            GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, audioClipIndex, isLeftHand, handTapSpeed);
                                        }
                                    }
                                }

                                return false;
                            }

                            if (!tapsEnabled)
                            {
                                effectContext.speed = 0f;
                                effectContext.soundVolume = 0f;

                                GorillaTagger.Instance.handTapVolume = 0f;
                                GorillaTagger.Instance.handTapSpeed = 0f;
                                GorillaTagger.Instance.audioClipIndex = -1;

                                return false;
                            }

                            if (material > 0)
                            {
                                GorillaTagger.Instance.audioClipIndex = material;
                                audioClipIndex = material;

                                if (isLeftHand)
                                    GTPlayer.Instance.leftHand.materialTouchIndex = material;
                                else
                                    GTPlayer.Instance.rightHand.materialTouchIndex = material;

                                effectContext.soundFX = VRRig.LocalRig.GetHandSurfaceData(material).audio;

                                return false;
                            }
                        }
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(VRRig), "IsPositionInRange")]
            public class IsPositionInRangePatch
            {
                public static bool enabled = false;

                public static void Postfix(VRRig __instance, ref bool __result, Vector3 position, float range)
                {
                    NetPlayer player = Rigs.GetPlayerFromVRRig(__instance) ?? null;
                    if ((enabled && __instance.isLocal) || (player != null && player == NetworkSystem.Instance.LocalPlayer))
                        __result = true;
                }
            }

            [HarmonyPatch(typeof(GrowingSnowballThrowable), "SnowballThrowEventReceiver")]
            public class SnowballThrowEventReceiverPatch
            {
                public static bool Prefix(GrowingSnowballThrowable __instance, int sender, int receiver, object[] args, PhotonMessageInfoWrapped info)
                {
                    if (info.Sender != null && NetworkSystem.Instance.LocalPlayer != null)
                    {
                        object obj = args[0];
                        if (obj is Vector3)
                        {
                            Vector3 vector = (Vector3)obj;
                            obj = args[1];
                            if (obj is Vector3)
                            {
                                Vector3 vector2 = (Vector3)obj;
                                obj = args[2];
                                if (obj is int)
                                    __instance.LaunchSnowballRemote(vector, vector2, __instance.snowballModelTransform.lossyScale.x, (int)obj, info);
                            }
                        }
                        return false;
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(SlingshotProjectile), "CheckForAOEKnockback")]
            public class CheckForAOEKnockbackPatch
            {
                public static bool Fling = true;
                private static bool Prefix(Vector3 impactPosition, float impactSpeed)
                {
                    return Fling;
                }
            }

            [HarmonyPatch(typeof(PhotonNetworkController), "OnJoinedRoom")]
            public class OnJoinedRoomPatch
            {
                public static bool enabled = false;

                private static void Prefix()
                {
                    if (enabled)
                        PhotonNetworkController.Instance.currentJoinType = GorillaNetworking.JoinType.FollowingParty;
                }
            }

            /*[HarmonyPatch(typeof(GorillaLocomotion.GTPlayer), "AntiTeleportTechnology", MethodType.Normal)]
            public class AntiTeleportTechnologyPatch
            {
                static bool Prefix()
                {
                    return false;
                }
            }*/

            [HarmonyPatch(typeof(GameObject), "CreatePrimitive", MethodType.Normal)]
            public class CreatePrimitivePatch
            {
                private static Shader uberShader;
                public static void Postfix(GameObject __result)
                {
                    if (__result.GetComponent<Renderer>() != null)
                    {
                        if (uberShader == null) uberShader = Shader.Find("GorillaTag/UberShader");
                        __result.GetComponent<Renderer>().material.shader = uberShader;
                    }
                }
            }

            [HarmonyPatch(typeof(VRRig), nameof(VRRig.OnDisable))]
            public class OnDisable
            {
                public static bool Prefix(VRRig __instance) =>
                    !__instance.isLocal;
            }

            [HarmonyPatch(typeof(VRRig), nameof(VRRig.Awake))]
            public class Awake
            {
                public static bool Prefix(VRRig __instance) =>
                    __instance.gameObject.name != "Local Gorilla Player(Clone)";
            }

            [HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
            public class PostTick
            {
                public static bool Prefix(VRRig __instance) =>
                    !__instance.isLocal || __instance.enabled;
            }

            [HarmonyPatch(typeof(ProjectileWeapon), "LaunchProjectile")]
            public class LaunchProjectilePatch
            {
                public static bool enabled = false;

                public static void Prefix(ProjectileWeapon __instance)
                {
                    if (enabled)
                        GorillaTagger.Instance.rigidbody.linearVelocity = __instance.GetLaunchVelocity();

                }
            }

            [HarmonyPatch(typeof(SIGadgetWristJet), "OnUpdateAuthority")]
            public class OnUpdateAuthorityJetPatch
            {
                public static bool enabled = false;

                public static void Postfix(SIGadgetWristJet __instance, float dt)
                {
                    if (enabled)
                        __instance.currentFuel = __instance.fuelSize;
                }

            }

            [HarmonyPatch(typeof(SIGadgetPlatformDeployer), "OnUpdateAuthority")]
            public class OnUpdateAuthorityPlatformPatch
            {
                public static bool enabled = false;

                public static void Prefix(SIGadgetPlatformDeployer __instance, float dt)
                {
                    if (enabled)
                        __instance.remainingRechargeTime = 0f;
                }
            }

            [HarmonyPatch(typeof(SIGadgetChargeBlaster), "OnUpdateAuthority")]
            public class OnUpdateAuthorityBlasterPatch
            {
                public static bool enabled = false;

                public static void Prefix(SIGadgetChargeBlaster __instance, float dt)
                {
                    if (enabled)
                        __instance.fireCooldown = 0f;
                }
            }

            [HarmonyPatch(typeof(SIGadgetDashYoyo), "OnUpdateAuthority")]
            public class OnUpdateAuthorityYoyoPatch
            {
                public static void Prefix(SIGadgetDashYoyo __instance, float dt)
                {
                    if (enabled)
                    {
                        __instance._cooldownDuration = 0f;
                        __instance.m_postYankCooldown = 0f;
                    }
                }

                public static bool enabled = false;
            }
        }

        public class MiscellaneousPatches
        {
            [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerEnteredRoom")]
            public class OnPlayerEnteredRoomPatch : MonoBehaviour
            {
                private static void Prefix(Player newPlayer)
                {
                    if (newPlayer != oldnewplayer)
                    {
                        NotifiLib.SendNotification($"{newPlayer.NickName}", NotifiLib.NotifiReason.RoomJoined);
                        oldnewplayer = newPlayer;
                    }
                }

                private static Player oldnewplayer;
            }

            [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerLeftRoom")]
            public class OnPlayerLeftRoomPatch : MonoBehaviour
            {
                private static void Prefix(Player otherPlayer)
                {
                    if (otherPlayer != PhotonNetwork.LocalPlayer && otherPlayer != a)
                    {
                        NotifiLib.SendNotification($"{otherPlayer.NickName}", NotifiLib.NotifiReason.RoomLeft);
                        a = otherPlayer;
                    }
                }

                private static Player a;
            }

            [HarmonyPatch(typeof(GorillaNetworkPublicTestsJoin), "GracePeriod")]
            public class GracePeriodPatch
            {
                private static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(GorillaNetworkPublicTestJoin2), "GracePeriod")]
            public class GracePeriodPatch2
            {
                private static bool Prefix() =>
                    false;
            }
        }

        public class TelemetryPatches
        {
            [HarmonyPatch(typeof(GorillaTelemetry), "IsConnected")]
            internal class IsConnectedPatch
            {
                public static bool Prefix() =>
                    false;
            }

            [HarmonyPatch(typeof(GorillaTelemetry), "EnqueueTelemetryEvent")]
            public class EnqueueTelemetryEventPatch
            {
                private static bool Prefix(string eventName, object content, [CanBeNull] string[] customTags = null) =>
                    !enabled;
            }

            [HarmonyPatch(typeof(GorillaTelemetry), "EnqueueTelemetryEventPlayFab")]
            public class EnqueueTelemetryEventPlayFabPatch
            {
                private static bool Prefix(EventContents eventContent) =>
                    !enabled;
            }

            [HarmonyPatch(typeof(GorillaTelemetry), "FlushPlayFabTelemetry")]
            public class FlushPlayFabTelemetryPatch
            {
                private static bool Prefix() =>
                    !enabled;
            }

            [HarmonyPatch(typeof(GorillaTelemetry), "FlushMothershipTelemetry")]
            public class FlushMothershipTelemetryPatch
            {
                private static bool Prefix() =>
                    !enabled;
            }

            [HarmonyPatch(typeof(ILckTelemetryClient), "SendTelemetry")]
            public class SendTelemetryPatch
            {
                private static bool Prefix(LckTelemetryEvent lckTelemetryEvent) =>
                    !enabled;
            }

            public static bool enabled = true;
        
           
        }
        public class GrabPatches
        {
            [HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.TakeMyHand_ProcessMovement))]
            public class GrabPatch
            {
                public static bool enabled;

                public static bool Prefix(GTPlayer __instance) => !enabled;
            }
        }

        
    }
}


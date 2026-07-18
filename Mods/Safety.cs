using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.Events;
using PlayFab.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using Debug = UnityEngine.Debug;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Object = UnityEngine.Object;

namespace Juul
{
    internal class Safety
    {
        public static void BypassVCBan()
        {
            GorillaTagger.moderationMutedTime = -1f;
            GorillaTelemetry.PostNotificationEvent("Unmute");
            GorillaTagger.Instance.myRecorder.TransmitEnabled = true;
            if (KIDManager.Instance != null)
            {
                GameObject.Destroy(KIDManager.Instance);
            }
        }
        public static void SetTick(float tickMultiplier)
        {
            var photonMono = GameObject.Find("PhotonMono")?.GetComponent<PhotonHandler>();
            if (photonMono != null)
            {
                Traverse.Create(photonMono).Field("nextSendTickCountOnSerialize").SetValue((int)(Time.realtimeSinceStartup * tickMultiplier));
                PhotonHandler.SendAsap = true;
            }
        }

        public static void FlushNetwork()
        {
            try
            {
                PhotonNetwork.SendAllOutgoingCommands();
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            }
            catch { }
        }

        public static object RunViewUpdate()
        {
            return typeof(PhotonNetwork).GetMethod("RunViewUpdate", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
        }
        private static DateTime lastAntiBanCall = DateTime.MinValue;
        private static readonly TimeSpan antiBanInterval = TimeSpan.FromSeconds(5);
        private static bool initialized;
        private static FieldInfo authContextField;
        private static FieldInfo photonViewListField;
        private static FieldInfo userRPCCallsField;
        private static FieldInfo reportedPlayersField;
        private static FieldInfo sendReportField;
        private static FieldInfo suspiciousPlayerIdField;
        private static FieldInfo suspiciousReasonField;
        private static FieldInfo suspiciousPlayerNameField;
        private static FieldInfo cachedDataField;
        private static FieldInfo monoRPCMethodsCacheField;
        private static MethodInfo clearAllEventsMethod;
        private static FieldInfo staticPlayerField;
        private static FieldInfo requestTimeoutField;
        private static FieldInfo compressApiDataField;
        private static FieldInfo disableFocusTimeCollectionField;
        private static FieldInfo sentCountAllowanceField;
        private static FieldInfo quickResendAttemptsField;
        private static FieldInfo outgoingStreamQueueField;
        public static void InitializeAntiBanHelper()
        {
            if (initialized) return;
            try
            {
                var playFabHttpType = typeof(PlayFabHttp);
                clearAllEventsMethod = playFabHttpType.GetMethod("ClearAllEvents", BindingFlags.Public | BindingFlags.Static);
                authContextField = typeof(PlayFabAuthenticationAPI).GetField("_authenticationContext",
                    BindingFlags.Static | BindingFlags.NonPublic);
                staticPlayerField = typeof(PlayFabSettings).GetField("staticPlayer",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                requestTimeoutField = typeof(PlayFabSettings).GetField("RequestTimeout",
                    BindingFlags.Static | BindingFlags.Public);
                compressApiDataField = typeof(PlayFabSettings).GetField("CompressApiData",
                    BindingFlags.Static | BindingFlags.Public);
                disableFocusTimeCollectionField = typeof(PlayFabSettings).GetField("DisableFocusTimeCollection",
                    BindingFlags.Static | BindingFlags.Public);
                var monkeAgentType = typeof(MonkeAgent);
                userRPCCallsField = monkeAgentType.GetField("userRPCCalls",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                reportedPlayersField = monkeAgentType.GetField("reportedPlayers",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                sendReportField = monkeAgentType.GetField("_sendReport",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                suspiciousPlayerIdField = monkeAgentType.GetField("_suspiciousPlayerId",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                suspiciousPlayerNameField = monkeAgentType.GetField("_suspiciousPlayerName",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                suspiciousReasonField = monkeAgentType.GetField("_suspiciousReason",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var photonNetworkType = typeof(Photon.Pun.PhotonNetwork);
                photonViewListField = photonNetworkType.GetField("photonViewList",
                    BindingFlags.Static | BindingFlags.NonPublic);
                cachedDataField = photonNetworkType.GetField("cachedData",
                    BindingFlags.Static | BindingFlags.NonPublic);
                monoRPCMethodsCacheField = photonNetworkType.GetField("monoRPCMethodsCache",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var peerType = typeof(LoadBalancingPeer);
                sentCountAllowanceField = peerType.GetField("SentCountAllowance",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                quickResendAttemptsField = peerType.GetField("QuickResendAttempts",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                outgoingStreamQueueField = peerType.GetField("outgoingStreamQueue",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                initialized = true;
            }
            catch { }
        }

        public static void AntiBan()
        {
            InitializeAntiBanHelper();
            if (!PhotonNetwork.InRoom) return;
            try
            {
                var instance = MonkeAgent.instance;
                if (instance != null)
                {
                    instance.rpcErrorMax = int.MaxValue;
                    instance.rpcCallLimit = int.MaxValue;
                    instance.logErrorMax = int.MaxValue;
                    userRPCCallsField?.SetValue(instance, new Dictionary<string, Dictionary<string, object>>());
                    reportedPlayersField?.SetValue(instance, new List<string>());
                    sendReportField?.SetValue(instance, false);
                    suspiciousPlayerIdField?.SetValue(instance, "");
                    suspiciousPlayerNameField?.SetValue(instance, "");
                    suspiciousReasonField?.SetValue(instance, "");
                }
                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.QuickResends = int.MaxValue;
                PhotonNetwork.NetworkStatisticsEnabled = false;
                var peer = PhotonNetwork.NetworkingClient?.LoadBalancingPeer;
                if (peer != null)
                {
                    sentCountAllowanceField?.SetValue(peer, int.MaxValue);
                    quickResendAttemptsField?.SetValue(peer, (byte)3);
                    var queue = outgoingStreamQueueField?.GetValue(peer) as System.Collections.IList;
                    queue?.Clear();
                    var resentField = peer.GetType().GetField("resentCommandsCount", BindingFlags.NonPublic | BindingFlags.Instance);
                    resentField?.SetValue(peer, 0);
                    peer.SendOutgoingCommands();
                }
                PhotonNetwork.SendAllOutgoingCommands();
                photonViewListField?.SetValue(null, Activator.CreateInstance(photonViewListField.FieldType));
                cachedDataField?.SetValue(null, new Dictionary<int, Dictionary<int, Queue<object[]>>>());
                monoRPCMethodsCacheField?.SetValue(null, new Dictionary<Type, List<MethodInfo>>());
                if (DateTime.UtcNow - lastAntiBanCall < antiBanInterval) return;
                lastAntiBanCall = DateTime.UtcNow;
                if (!PhotonNetwork.IsConnected)
                {
                    authContextField?.SetValue(null, null);
                    clearAllEventsMethod?.Invoke(null, null);
                    return;
                }
                if (!PlayFabAuthenticationAPI.IsEntityLoggedIn()) return;
                try
                {
                    clearAllEventsMethod?.Invoke(null, null);
                    requestTimeoutField?.SetValue(null, 30000);
                    compressApiDataField?.SetValue(null, true);
                    disableFocusTimeCollectionField?.SetValue(null, true);
                    var staticPlayer = staticPlayerField?.GetValue(null) as PlayFabAuthenticationContext;
                    staticPlayer?.ForgetAllCredentials();
                }
                catch { }
            }
            catch { }
        }
        public static void NoFinger()
        {
            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.rightControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
            ControllerInputPoller.instance.rightControllerIndexFloat = 0f;
        }

        public static void RestartGame()
        {
            Process.Start("steam://rungameid/1533390");
            Application.Quit();
        }

        public static void QuitGame()
        {
            Application.Quit();
        }

        public static void DisconnectLT()
        {
            if (PhotonNetwork.InRoom)
            {
                if (ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f || Mouse.current.leftButton.isPressed)
                {
                    PhotonNetwork.Disconnect();
                }
            }
        }

        public static void DisconnectRT()
        {
            if (PhotonNetwork.InRoom)
            {
                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f || Mouse.current.rightButton.isPressed)
                {
                    PhotonNetwork.Disconnect();
                }
            }
        }

        public static void DisableNetworkTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(false);
        }

        public static void EnableNetworkTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(true);
        }

        public static void DisableMapTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab").SetActive(false);
        }

        public static void EnableMapTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab").SetActive(true);
        }

        public static void DisableQuitBox()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(false);
        }

        public static void EnableQuitBox()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(true);
        }

        public static void EnableAntiAFK()
        {
            PhotonNetworkController.Instance.disableAFKKick = true;
        }

        public static void DisableAntiAFK()
        {
            PhotonNetworkController.Instance.disableAFKKick = false;
        }

        public static void JoinRandom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.Disconnect();
            }
            else
            {
                string text = PhotonNetworkController.Instance.currentJoinTrigger == null ? "forest" : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.GetJoinTriggerForZone(text), 0);
            }
        }

        public static void JoinRoom(string RoomCode)
        {
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(RoomCode, 0);
        }

        public static VRRig reportRig;
        public static float antiReportRadius = 0.65f;
        public static GameObject antiReportSphere;
        
        public static void AntiReport(System.Action<VRRig, Vector3> onReport)
        {
            if (!NetworkSystem.Instance.InRoom) return;

            if (reportRig != null)
            {
                onReport?.Invoke(reportRig, reportRig.transform.position);
                reportRig = null;
                return;
            }

            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                Transform report = line.reportButton.gameObject.transform;

                foreach (var vrrig in from vrrig in VRRigCache.ActiveRigs where !vrrig.isLocal let D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position) let D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position) where D1 < antiReportRadius || D2 < antiReportRadius select vrrig)
                    onReport?.Invoke(vrrig, report.transform.position);
            }
        }

        public static void VisualizeAntiReportRadius()
        {
            if (!NetworkSystem.Instance.InRoom)
            {
                CleanupAntiReportVisualization();
                return;
            }

            bool foundButton = false;
            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line == null || line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                
                if (line.reportButton == null || line.reportButton.gameObject == null)
                {
                    continue;
                }
                
                Transform report = line.reportButton.gameObject.transform;
                foundButton = true;

                if (antiReportSphere == null)
                {
                    antiReportSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Object.Destroy(antiReportSphere.GetComponent<Collider>());
                    
                    Renderer renderer = antiReportSphere.GetComponent<Renderer>();
                    
                    // Use GUI/Text Shader
                    renderer.material.shader = Shader.Find("GUI/Text Shader");
                    
                    // Use menu theme color
                    Color sphereColor = Core.BaseColor;
                    sphereColor.a = 0.4f;
                    renderer.material.color = sphereColor;
                    
                    antiReportSphere.layer = 0; // Default layer
                    
                    NotifiLib.SendNotification("<color=green>[SUCCESS]</color> Anti-Report visualization created!");
                }
                else
                {
                    // Update color with pulsing gradient using menu theme color
                    Renderer renderer = antiReportSphere.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        Color themeColor = Core.BaseColor;
                        Color lightThemeColor = Color.Lerp(themeColor, Color.white, 0.5f);
                        Color sphereColor = Color.Lerp(themeColor, lightThemeColor, Mathf.PingPong(Time.time * 0.8f, 1f));
                        sphereColor.a = 0.4f;
                        renderer.material.color = sphereColor;
                    }
                }

                antiReportSphere.transform.position = report.position;
                antiReportSphere.transform.localScale = Vector3.one * (antiReportRadius * 2f);
                antiReportSphere.SetActive(true);
                break; // Only need to update once per frame
            }
        }

        public static void CleanupAntiReportVisualization()
        {
            if (antiReportSphere != null)
            {
                GameObject.Destroy(antiReportSphere);
                antiReportSphere = null;
            }
        }

        public static string GetAntiReportRadiusName()
        {
            return antiReportRadius.ToString("F2");
        }

        public static void ChangeAntiReportRadius(bool increase)
        {
            if (increase)
            {
                antiReportRadius += 0.05f;
                if (antiReportRadius > 2f) antiReportRadius = 2f;
            }
            else
            {
                antiReportRadius -= 0.05f;
                if (antiReportRadius < 0.1f) antiReportRadius = 0.1f;
            }

            if (ExtraButtons.AntiReportRadiusButton != null)
                ExtraButtons.AntiReportRadiusButton.Name = $"Anti Report Size: {GetAntiReportRadiusName()}";
        }

        public static float antiReportDelay;
        public static void AntiReportDisconnect()
        {
            AntiReport((vrrig, position) =>
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();
            });
        }
        public static void AntiReportQuit()
        {
            AntiReport((vrrig, position) =>
            {
                Application.Quit();
            });
        }
        public static void AntiReportNotify()
        {
            AntiReport((vrrig, position) =>
            {
                NotifiLib.SendNotification("You Have Been Reported");
            });
        }
        public static void AntiReportReconnect()
        {
            AntiReport((vrrig, position) =>
            {
                string name = PhotonNetwork.CurrentRoom.Name;
                NetworkSystem.Instance.ReturnToSinglePlayer();
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(name, GorillaNetworking.JoinType.Solo);
            });
        }

        static float rpcDel;
        public static bool IsRPCPatched = false;
        public static bool visAntiReport = false;
        public static void UncapFPS()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = int.MaxValue;
        }
        public static void SetFPS144()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 144;
        }
        public static void SetFPS120()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
        }

        public static void SetFPS90()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 90;
        }

        public static void SetFPS80()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 80;
        }

        public static void SetFPS72()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 72;
        }

        public static void SetFPS60()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        public static void SetFPS45()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 45;
        }

        public static void SetFPS15()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 15;
        }

        public static void SetFPS1()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 1;
        }

        public static void AntiAFKKick()
        {
            PhotonNetworkController.Instance.disableAFKKick = true;
        }
        private static bool spoofingActive = false;
        private static string spoofedPlayFabId;
        private static string spoofedEntityId;
        private static string spoofedEntityToken;
        private static string spoofedSessionTicket;
        private static FieldInfo nicknameField;
        private static FieldInfo userIdField;
        private static Type networkSystemType;
        private static PropertyInfo networkSystemInstanceProperty;
        private static MethodInfo returnToSinglePlayerMethod;
        private static System.Random random = new System.Random();

        public static void InitializePlayerSpoofHelper()
        {
            if (initialized) return;
            try
            {
                authContextField = typeof(PlayFabAuthenticationAPI).GetField("_authenticationContext",
                    BindingFlags.Static | BindingFlags.NonPublic);
                staticPlayerField = typeof(PlayFabSettings).GetField("staticPlayer",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var photonNetworkType = typeof(PhotonNetwork);
                var playerType = typeof(Player);
                nicknameField = playerType.GetField("nickName",
                    BindingFlags.Instance | BindingFlags.Public);
                userIdField = playerType.GetField("userId",
                    BindingFlags.Instance | BindingFlags.Public);
                networkSystemType = Type.GetType("NetworkSystem, Assembly-CSharp");
                if (networkSystemType != null)
                {
                    networkSystemInstanceProperty = networkSystemType.GetProperty("Instance",
                        BindingFlags.Static | BindingFlags.Public);
                    returnToSinglePlayerMethod = networkSystemType.GetMethod("ReturnToSinglePlayer",
                        BindingFlags.Instance | BindingFlags.Public);
                }
                initialized = true;
            }
            catch { }
        }
        private static void ForgetAllPlayFabCredentials()
        {
            try
            {
                var staticPlayer = staticPlayerField?.GetValue(null) as PlayFabAuthenticationContext;
                staticPlayer?.ForgetAllCredentials();
                authContextField?.SetValue(null, null);
                var clearEventsMethod = typeof(PlayFabHttp).GetMethod("ClearAllEvents",
                    BindingFlags.Public | BindingFlags.Static);
                clearEventsMethod?.Invoke(null, null);
                typeof(PlayFabSettings).GetField("DisableFocusTimeCollection",
                    BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
                typeof(PlayFabSettings).GetField("DisableAdvertising",
                    BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
                typeof(PlayFabSettings).GetField("DisableDeviceInfo",
                    BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
            }
            catch { }
        }
        private static void GenerateSpoofedIdentities()
        {
            spoofedPlayFabId = RandomPlayfabID();
            spoofedEntityId = RandomEntityID();
            spoofedEntityToken = RandomToken();
            spoofedSessionTicket = RandomTicket();
        }
        private static void ApplySpoofedIdentity()
        {
            try
            {
                var spoofedContext = new PlayFabAuthenticationContext
                {
                    PlayFabId = spoofedPlayFabId,
                    EntityId = spoofedEntityId,
                    EntityToken = spoofedEntityToken,
                    ClientSessionTicket = spoofedSessionTicket,
                    EntityType = "_GorillaPlayer"
                };
                authContextField?.SetValue(null, spoofedContext);
                staticPlayerField?.SetValue(null, spoofedContext);
                if (PhotonNetwork.LocalPlayer != null)
                {
                    nicknameField?.SetValue(PhotonNetwork.LocalPlayer, "Player_" + UnityEngine.Random.Range(1000, 9999));
                    userIdField?.SetValue(PhotonNetwork.LocalPlayer, spoofedPlayFabId);
                }
            }
            catch { }
        }
        public static string GetCurrentSpoofedPlayFabId() => spoofedPlayFabId;
        public static bool IsSpoofingActive() => spoofingActive;
        private static string RandomPlayfabID()
        {
            const string chars = "0123456789ABCDEF";
            string id = "";
            for (int i = 0; i < 16; i++)
            {
                id += chars[random.Next(chars.Length)];
            }
            return id;
        }
        private static string RandomEntityID()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
        }
        private static string RandomToken()
        {
            byte[] bytes = new byte[32];
            random.NextBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
        private static string RandomTicket()
        {
            return "TICKET_" + Guid.NewGuid().ToString("N").ToUpper();
        }
        public static void SpoofPlayer()
        {
            InitializePlayerSpoofHelper();
            try
            {
                if (networkSystemInstanceProperty != null && returnToSinglePlayerMethod != null)
                {
                    var instance = networkSystemInstanceProperty.GetValue(null);
                    returnToSinglePlayerMethod.Invoke(instance, null);
                }
                ForgetAllPlayFabCredentials();
                GenerateSpoofedIdentities();
                ApplySpoofedIdentity();
                spoofingActive = true;
            }
            catch { }
        }

        public static void AcceptTOS()
        {
            GameObject.Find("Miscellaneous Scripts/PopUpMessage").SetActive(false);
        }
        private static float lastRpcClear = 0f;
        private static float rpcClearInterval = 5f;

        public static void AntiCrash()
        {
            try
            {
                if (Time.time > lastRpcClear + rpcClearInterval)
                {
                    lastRpcClear = Time.time;
                    if (PhotonNetwork.NetworkingClient != null)
                    {
                        var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
                        var outgoingQueueField = peer.GetType().GetField("outgoingStreamQueue",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        var queue = outgoingQueueField?.GetValue(peer) as System.Collections.IList;
                        if (queue != null && queue.Count > 1000)
                        {
                            queue.Clear();
                            Debug.Log("Cleared outgoing RPC queue to prevent crash");
                        }
                    }
                }
                if (GorillaTagger.Instance == null || GorillaTagger.Instance.myVRRig == null)
                    return;
            }
            catch { }
        }
        public static void FakeOculusMenu()
        {
            if (Inputs.RightPrimary)
            {
                NoFinger();
                ConnectedControllerHandler.Instance.SetRightHandOffsets(
                    new Vector3(0f, -0.2f, 0.1f),
                    Quaternion.Euler(275f, 270f, -5f)
                );
                ConnectedControllerHandler.Instance.SetLeftHandOffsets(
                    new Vector3(0f, -0.2f, 0.1f),
                    Quaternion.Euler(275f, 90f, 5f)
                );
                ConnectedControllerHandler.Instance.rightHandFollower.UpdatePositionRotation();
                ConnectedControllerHandler.Instance.leftHandFollower.UpdatePositionRotation();
            }
            else
            {
                ConnectedControllerHandler.Instance.SetOculusOffsets(true, true);
                ConnectedControllerHandler.Instance.rightHandFollower.UpdatePositionRotation();
                ConnectedControllerHandler.Instance.leftHandFollower.UpdatePositionRotation();
            }
        }
        public static void FakeReportMenu()
        {
            if (Inputs.LeftSecondary)
            {
                NoFinger();
                GTPlayer.Instance.InReportMenu = true;
            }
            else
            {
                GTPlayer.Instance.InReportMenu = false;
            }
        }
        public static void FakeBrokenControllerRight()
        {
            if (Inputs.RightPrimary)
            {
                NoFinger();
                ConnectedControllerHandler.Instance.overriddenControllers |= OverrideControllers.RightController;
                ConnectedControllerHandler.Instance.UpdateControllerStates();
            }
            else
            {
                ConnectedControllerHandler.Instance.overriddenControllers &= ~OverrideControllers.RightController;
                ConnectedControllerHandler.Instance.UpdateControllerStates();
            }
        }
        public static void FakeBrokenControllerLeft()
        {
            if (Inputs.LeftSecondary)
            {
                NoFinger();
                ConnectedControllerHandler.Instance.overriddenControllers |= OverrideControllers.LeftController;
                ConnectedControllerHandler.Instance.UpdateControllerStates();
            }
            else
            {
                ConnectedControllerHandler.Instance.overriddenControllers &= ~OverrideControllers.LeftController;
                ConnectedControllerHandler.Instance.UpdateControllerStates();
            }
        }
        public static void FakeBadTracking()
        {
            if (Inputs.RightSecondary)
            {
                NoFinger();
                ConnectedControllerHandler.Instance.overrideRightEnable = true;
                ConnectedControllerHandler.Instance.overrideLeftEnable = true;
            }
            else
            {
                ConnectedControllerHandler.Instance.overrideRightEnable = false;
                ConnectedControllerHandler.Instance.overrideLeftEnable = false;
            }
        }
        public static async void CreatePublicLobby10()
        {
            PhotonNetworkController controller = PhotonNetworkController.Instance;
            if (controller == null) return;
            if (NetworkSystem.Instance.InRoom)
            {
                await NetworkSystem.Instance.ReturnToSinglePlayer();
                await Task.Delay(500);
            }
            string roomName = GenerateRoomName();
            RoomConfig roomConfig = new RoomConfig();
            roomConfig.isPublic = true;
            roomConfig.isJoinable = true;
            roomConfig.createIfMissing = true;
            roomConfig.MaxPlayers = 10;
            roomConfig.CustomProps = new ExitGames.Client.Photon.Hashtable();
            roomConfig.CustomProps.Add("gameMode", "DEFAULT");
            roomConfig.CustomProps.Add("platform", "PC");
            await NetworkSystem.Instance.ConnectToRoom(roomName, roomConfig);
        }

        public static async void CreatePublicLobby20()
        {
            PhotonNetworkController controller = PhotonNetworkController.Instance;
            if (controller == null) return;
            if (NetworkSystem.Instance.InRoom)
            {
                await NetworkSystem.Instance.ReturnToSinglePlayer();
                await Task.Delay(500);
            }
            string roomName = GenerateRoomName();
            RoomConfig roomConfig = new RoomConfig();
            roomConfig.isPublic = true;
            roomConfig.isJoinable = true;
            roomConfig.createIfMissing = true;
            roomConfig.MaxPlayers = 20;
            roomConfig.CustomProps = new ExitGames.Client.Photon.Hashtable();
            roomConfig.CustomProps.Add("gameMode", "DEFAULT");
            roomConfig.CustomProps.Add("platform", "PC");
            await NetworkSystem.Instance.ConnectToRoom(roomName, roomConfig);
        }

        private static string GenerateRoomName()
        {
            string chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789";
            string roomName = "";
            for (int i = 0; i < 4; i++)
            {
                roomName += chars[UnityEngine.Random.Range(0, chars.Length)];
            }
            return roomName;
        }





    }
}


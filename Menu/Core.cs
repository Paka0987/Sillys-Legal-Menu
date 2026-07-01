using BepInEx;
using ExitGames.Client.Photon;
using g3;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTagScripts;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR;
using Valve.Newtonsoft.Json;
using Valve.VR;
using static gs.PointSetHashtable;
using CommonUsages = UnityEngine.XR.CommonUsages;
using JoinType = GorillaNetworking.JoinType;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Juul
{
    [HarmonyPatch(typeof(GTPlayer))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public partial class Core : MonoBehaviour
    {
        public static bool isPCMenuOpen = false;
        public static bool menuWasOpenedViaPC = false;
        public static GameObject Menu = null;
        public static GameObject Canvas = null;
        public static GameObject Pointer = null;
        public static GameObject Pointer2 = null;
        public static GameObject Frame = null;
        public static GameObject Sidebar = null;
        public static float SmFl = 0.0035f;
        public static Category ActiveCategory = null;
        public static int BtnIndex = 0;
        public static int CatIndex = 0;
        public static int MaxCatsPerPage = 11;
        public static int CurrentCatPage = 0;
        public static int MaxButtons = 8;
        public static int CurrentPage = 0;
        public static int PageBtnVer = 2;
        public static float ButtonCooldown = 0f;
        public static float IncrementCooldown = 0f;
        public static bool IsOutlined = true;
        public static bool MenuSmoothing = true;
        public static float MenuSmoothingSpeed = 20f;
        private static float rigCacheTimer = 0f;

        private static string[] layoutNames =
        {
            "JUUL"
        };
        public static int LayoutValue = 0;

        public static void RefreshMenuLayout()
        {
            ResetLayoutDefaults();
            if (LayoutValue == 0)
            {
                MenuWidth = 0.8f;
                BtnInset = 0.1f;
                BtnUpset = 0.3f;
                BtnHeight = 0.07f;
                BtnSpace = 0.005f;
                TextSize = 0.5f;
                GradVal = 0.05f;
                MaxButtons = 8;
            }
        }
        private static void ResetLayoutDefaults()
        {
            MenuWidth = 0.8f;
            BtnInset = 0.1f;
            BtnUpset = 0.3f;
            BtnHeight = 0.07f;
            BtnSpace = 0.005f;
            TextSize = 0.5f;
            GradVal = 0.05f;
            MaxButtons = 8;
        }
        public static string GetCurrentLayoutName()
        {
            if (ThemeValue >= 0 && ThemeValue < layoutNames.Length)
                return layoutNames[ThemeValue];
            return "JUUL";
        }
        public static void ChangeLayout(bool forward)
        {
            LayoutValue = 0;
            RefreshMenuLayout();
            RebuildMenu();
        }



        public static Font Arial = Font.CreateDynamicFontFromOSFont("Arial", 14);
        public static Font Verdana = Font.CreateDynamicFontFromOSFont("Verdana", 14);
        public static Font SFPro = Font.CreateDynamicFontFromOSFont("SF Pro", 14);
        public static Font Consolas = Font.CreateDynamicFontFromOSFont("Consolas", 14);
        public static Font Impact = Font.CreateDynamicFontFromOSFont("Impact", 14);
        public static Font ComicSans = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 14);
        public static Font TimesNewRoman = Font.CreateDynamicFontFromOSFont("Times New Roman", 14);
        public static Font Georgia = Font.CreateDynamicFontFromOSFont("Georgia", 14);
        public static Font CourierNew = Font.CreateDynamicFontFromOSFont("Courier New", 14);
        public static Font Tahoma = Font.CreateDynamicFontFromOSFont("Tahoma", 14);
        public static Font Trebuchet = Font.CreateDynamicFontFromOSFont("Trebuchet MS", 14);
        public static Font Lucida = Font.CreateDynamicFontFromOSFont("Lucida Console", 14);
        public static Font SegoeUI = Font.CreateDynamicFontFromOSFont("Segoe UI", 14);
        public static Font Roboto = Font.CreateDynamicFontFromOSFont("Roboto", 14);
        public static Font OpenSans = Font.CreateDynamicFontFromOSFont("Open Sans", 14);

        public static int FontValue = 0;

        public static Font[] FontList = new Font[]
        {
            Verdana,  
            Arial,
            SFPro,
            Consolas,
            Impact,
            ComicSans,
            TimesNewRoman,
            Georgia,
            CourierNew,
            Tahoma,
            Trebuchet,
            Lucida,
            SegoeUI,
            Roboto,
            OpenSans
        };

        public static string[] FontNames = new string[]
        {
            "Verdana",
            "Arial",
            "SF Pro",
            "Consolas",
            "Impact",
            "Comic Sans MS",
            "Times New Roman",
            "Georgia",
            "Courier New",
            "Tahoma",
            "Trebuchet MS",
            "Lucida Console",
            "Segoe UI",
            "Roboto",
            "Open Sans"
        };
        public static string GetCurrentFontName()
        {
            return FontNames[FontValue];
        }
        public static void ChangeFont(bool forward)
        {
            if (forward && FontValue >= (FontList.Length - 1))
                FontValue = 0;
            else if (!forward && FontValue <= 0)
                FontValue = (FontList.Length - 1);
            else
                FontValue = FontValue + (forward ? 1 : -1);
            MenuFont = FontList[FontValue];

            if (ExtraButtons.FontButton != null)
                ExtraButtons.FontButton.Name = $"Font: {GetCurrentFontName()}";
            RebuildMenu();
            PlayerPrefs.SetInt("JuulFonts", FontValue);
            PlayerPrefs.Save();
        }

        public static void SetFont(int value)
        {
            FontValue = Mathf.Clamp(value, 0, FontList.Length - 1);
            MenuFont = FontList[FontValue];
            if (ExtraButtons.FontButton != null)
                ExtraButtons.FontButton.Name = $"Font: {GetCurrentFontName()}";
            RebuildMenu();
            PlayerPrefs.SetInt("JuulFont", FontValue);
            PlayerPrefs.Save();
        }
        public static void LoadFontPreference()
        {
            if (PlayerPrefs.HasKey("JuulFont"))
            {
                int savedFont = PlayerPrefs.GetInt("JuulFont");
                if (savedFont >= 0 && savedFont < FontList.Length)
                {
                    FontValue = savedFont;
                    MenuFont = FontList[FontValue];
                }
            }
        }

        public static void ChangeMenuSmoothingSpeed(bool up)
        {
            if (up) MenuSmoothingSpeed -= 2f;
            else MenuSmoothingSpeed += 2f;
            
            if (MenuSmoothingSpeed < 2f) MenuSmoothingSpeed = 2f;
            if (MenuSmoothingSpeed > 30f) MenuSmoothingSpeed = 30f;
            
            if (ExtraButtons.MenuSmoothingSpeedButton != null)
                ExtraButtons.MenuSmoothingSpeedButton.Name = $"Smoothing Speed: {MenuSmoothingSpeed}";
            
            RebuildMenu();
        }
        public static bool IsAnimated = true;
        public static bool IsMenuParticles = false;
        public static string LastClickedButtonID = "";
        public static Vector3 SearchButtonLastPos = Vector3.zero;
        public static bool IsRounded = true;
        public static bool IsCatLeft = true;
        public static bool IsCatRotated = true;
        public static bool MenuStart = false;
        public static bool IsMenuOpen = false;
        public static bool IsRightHanded = false;
        public static Font MenuFont = Verdana;
        public static float OffBrightness = 0.5f;
        public static float OnBrightness = 0.33f;
        public static int ThemeValue = 0;
        public static Color BaseColor = Color.red;
        public static Material BoardMat;
        public static GameObject BoardGradientObject = null;
        public static bool IsBoardGradientEnabled = true;
        private static Material _origMonitorScreenMat;
        private static Material _origWallMonitorMat;
        private static Dictionary<Renderer, Material> _origOtherBoardMats = new Dictionary<Renderer, Material>();
        private static bool _origBoardMatsCaptured = false;
        private static Shader _guiTextShader;
        private static Shader _uberShader;
        public static Shader GuiTextShader => _guiTextShader ??= Shader.Find("GUI/Text Shader");
        public static Shader UberShader => _uberShader ??= Shader.Find("GorillaTag/UberShader");
        public static VRRig[] CachedActiveRigs = Array.Empty<VRRig>();
        public static Camera CachedMainCamera;
        public static Color GetCurrentThemeColor()
        {
            return Themes.List[ThemeValue].Color;
        }
        public static string GetCurrentThemeName()
        {
            return Themes.List[ThemeValue].Name;
        }
        public static void ChangeTheme(bool forward)
        {
            if (forward && ThemeValue >= (Themes.List.Length - 1)) ThemeValue = 0;
            else if (!forward && ThemeValue <= 0) ThemeValue = (Themes.List.Length - 1);
            else ThemeValue = ThemeValue + (forward ? 1 : -1);
            if (ExtraButtons.ThemeButton != null)
                ExtraButtons.ThemeButton.Name = $"Theme: {GetCurrentThemeName()}";
        }
        public static void SetTheme(int value)
        {
            ThemeValue = value;
            if (ExtraButtons.ThemeButton != null)
                ExtraButtons.ThemeButton.Name = $"Theme: {GetCurrentThemeName()}";
            RebuildMenu();
        }
        public static string Folder
        {
            get
            {
                return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Juul");
            }
        }
        public static void ChangePageButtons(bool forward)
        {
            PageBtnVer = PageBtnVer + (forward ? 1 : -1);
            if (PageBtnVer < 0) PageBtnVer = 3;
            if (PageBtnVer > 3) PageBtnVer = 0;
            RebuildMenu();
        }
        public static Color CycleColors(Color[] colors, float cycleSpeed)
        {
            if (colors == null || colors.Length == 0)
                return Color.white;
            if (colors.Length == 1)
                return colors[0];
            float totalRange = colors.Length - 1;
            float t = Mathf.PingPong(Time.time * cycleSpeed, totalRange);
            int indexA = Mathf.FloorToInt(t);
            int indexB = Mathf.Clamp(indexA + 1, 0, colors.Length - 1);
            float localT = t - indexA;
            float easedT = localT < 0.5f
                ? 2f * localT * localT
                : 1f - Mathf.Pow(-2f * localT + 2f, 2f) / 2f;
            return Color.Lerp(colors[indexA], colors[indexB], easedT);
        }
        public static void OutlineGradient(GameObject toOutline)
        {
            if (IsOutlined && toOutline != null && Menu != null)
            {
                GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Cube);
                outline.transform.parent = toOutline.transform.parent;
                outline.transform.rotation = Quaternion.identity;
                outline.transform.localScale = toOutline.transform.localScale - new Vector3(SmFl / 2f, 0f, 0f);
                outline.transform.localPosition = toOutline.transform.localPosition;
                outline.transform.rotation = toOutline.transform.rotation;
                GameObject.Destroy(outline.GetComponent<Rigidbody>());
                GameObject.Destroy(outline.GetComponent<BoxCollider>());
                GradientSetter cs1 = outline.AddComponent<GradientSetter>();
                var sourceGradient = toOutline.GetComponent<GradientSetter>();
                var sourceColor = toOutline.GetComponent<ColorSetter>();
                if (sourceGradient != null)
                {
                    cs1.brightness = sourceGradient.brightness - 0.3f;
                    cs1.gradientOffset = sourceGradient.gradientOffset;
                    cs1.startOffset = sourceGradient.startOffset;
                    cs1.buttonID = sourceGradient.buttonID;
                }
                else if (sourceColor != null)
                {
                    cs1.brightness = sourceColor.brightness - 0.3f;
                    cs1.startOffset = sourceColor.colorOffset;
                    cs1.gradientOffset = 0f;
                    cs1.buttonID = sourceColor.buttonID;
                }
                if (IsRounded)
                {
                    var sourceCorners = toOutline.GetComponent<RoundedCorners>();
                    if (sourceCorners != null)
                    {
                        RoundedCorners corners = outline.AddComponent<RoundedCorners>();
                        corners.bevel = sourceCorners.bevel;
                    }
                }
                toOutline.transform.localScale = toOutline.transform.localScale - new Vector3(0f, 0.01f, 0.01f);
            }
        }
        public static Vector3 ServerPos;
        public static Vector3 ServerLeftHandPos;
        public static Vector3 ServerRightHandPos;
        public static Vector3 ServerSyncPos;
        public static Vector3 ServerSyncLeftHandPos;
        public static Vector3 ServerSyncRightHandPos;
        public static void OnSerialize()
        {
            ServerSyncPos = VRRig.LocalRig?.transform.position ?? ServerSyncPos;
            ServerSyncLeftHandPos = VRRig.LocalRig?.leftHand?.rigTarget?.transform.position ?? ServerSyncLeftHandPos;
            ServerSyncRightHandPos = VRRig.LocalRig?.rightHand?.rigTarget?.transform.position ?? ServerSyncRightHandPos;
        }
        public static bool inroomrel = false;
        private static readonly Dictionary<string, GameObject> objectPool = new Dictionary<string, GameObject>();
        private const int MAX_POOL_SIZE = 100;
        private static float poolCleanupTimer = 0f;
        public static GameObject GetObject(string find)
        {
            if (objectPool.TryGetValue(find, out GameObject go))
            {
                if (go != null)
                    return go;
                else
                    objectPool.Remove(find);
            }
            GameObject tgo = GameObject.Find(find);
            if (tgo != null && objectPool.Count < MAX_POOL_SIZE)
                objectPool.Add(find, tgo);
            return tgo;
        }
        private static void CleanupObjectPool()
        {
            poolCleanupTimer += Time.deltaTime;
            if (poolCleanupTimer > 60f)
            {
                poolCleanupTimer = 0f;
                List<string> keysToRemove = new List<string>();
                foreach (var kvp in objectPool)
                {
                    if (kvp.Value == null)
                        keysToRemove.Add(kvp.Key);
                }
                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    objectPool.Remove(keysToRemove[i]);
                }
            }
        }
        public static Camera TPC;
        private static float tpcSearchTimer = 0f;
        private static RaycastHit[] raycastHits = new RaycastHit[10];
        private static int uiLayerMask = 1 << 2;
        private static int? noInvisLayerMask;
        private static DateTime menuLoadTime = DateTime.Now;
        public static int NoInvisLayerMask()
        {
            noInvisLayerMask ??= ~(
                1 << LayerMask.NameToLayer("TransparentFX") |
                1 << LayerMask.NameToLayer("Ignore Raycast") |
                1 << LayerMask.NameToLayer("Zone") |
                1 << LayerMask.NameToLayer("Gorilla Trigger") |
                1 << LayerMask.NameToLayer("Gorilla Boundary") |
                1 << LayerMask.NameToLayer("GorillaCosmetics") |
                1 << LayerMask.NameToLayer("GorillaParticle"));
            return noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
        }
        public static void GetOtherBoards()
        {
            var treeRoom = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom");
            if (treeRoom == null) return;
            var stumpChildren = treeRoom.transform
                .Cast<Transform>()
                .Where(x => x.name.Contains("UnityTempFile"))
                .ToList();
            if (stumpChildren.Count <= 3) return;
            Renderer ren = stumpChildren[3].GetComponent<Renderer>();
            if (ren == null) return;
            if (!_origOtherBoardMats.ContainsKey(ren))
                _origOtherBoardMats[ren] = ren.material;
            if (IsBoardGradientEnabled && BoardMat != null)
                ren.material = BoardMat;
            else if (!IsBoardGradientEnabled && _origOtherBoardMats.ContainsKey(ren))
                ren.material = _origOtherBoardMats[ren];
        }
        public static void ChangeMapInfoText()
        {
            try
            {
                var mapInfo = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/MapInfo_TMP").GetComponent<TextMeshPro>();
                if (mapInfo != null)
                {
                    mapInfo.richText = true;
                    mapInfo.text = "JUUL ON TOP";
                    mapInfo.color = Color.white;
                    mapInfo.fontSize = 42;
                    mapInfo.alignment = TextAlignmentOptions.Center;
                }
            }
            catch { }
        }
        private static TextMeshPro motdBody;
        private static TextMeshPro motdHeading;
        private static TextMeshPro cocHeading;
        private static TextMeshPro mapInfo;
        private static TextMeshPro cocBody;
        private static Renderer monitorScreen;
        private static Renderer wallMonitor;
        private static float _boardsTimer = 0f;
        private static bool _boardsCacheInit = false;
        private static readonly string[] _spinChars = { "-", "/", "|", "\\"};
        private static void InitBoardsCache()
        {
            if (_boardsCacheInit) return;
            try { motdBody = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText")?.GetComponent<TextMeshPro>(); } catch { }
            try { motdHeading = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText")?.GetComponent<TextMeshPro>(); } catch { }
            try { cocHeading = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText")?.GetComponent<TextMeshPro>(); } catch { }
            try { mapInfo = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/MapInfo_TMP")?.GetComponent<TextMeshPro>(); } catch { }
            try { cocBody = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData")?.GetComponent<TextMeshPro>(); } catch { }
            try { monitorScreen = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen")?.GetComponent<Renderer>(); } catch { }
            try { wallMonitor = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomBoundaryStones/BoundaryStoneSet_Forest/wallmonitorforestbg")?.GetComponent<Renderer>(); } catch { }
            _boardsCacheInit = true;
        }
        public static void InvalidateBoardsCache()
        {
            _boardsCacheInit = false;
            motdBody = null;
            motdHeading = null;
            cocHeading = null;
            mapInfo = null;
            cocBody = null;
            monitorScreen = null;
            wallMonitor = null;
        }
        private static string _lastSpinner = "";
        public static void Boards()
        {
            InitBoardsCache();
            string spinner = _spinChars[Mathf.FloorToInt(Time.time * 3f) % _spinChars.Length];
            if (spinner != _lastSpinner)
            {
                _lastSpinner = spinner;
                if (motdHeading != null) motdHeading.text = $"[{spinner}] JUUL INFO BOARD [{spinner}]";
                if (cocHeading != null) cocHeading.text = $"[{spinner}] JUUL [{spinner}]";
                if (mapInfo != null) mapInfo.text = $"[{spinner}] JUUL ON TOP [{spinner}]";
            }

            _boardsTimer += Time.deltaTime;
            if (_boardsTimer < 1.0f) return;
            _boardsTimer = 0f;
            try
            {
                if (motdBody != null)
                {
                    motdBody.richText = true;
                    TimeSpan uptime = DateTime.Now - menuLoadTime;
                    string uptimeStr = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)uptime.TotalHours, uptime.Minutes, uptime.Seconds);
                    string playerName = PhotonNetwork.LocalPlayer.NickName;
                    string room = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "Not In Room";
                    int players = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
                    motdBody.text = "=========================================================================\nName: " + playerName + "\nRoom: " + room + "\nPlayers: " + players + "\nUptime: " + uptimeStr + "\nStatus: <#00FF00>Undetected</color>\n\n<#FF0000>If You Want To Open The Menu On Pc Press Q</color>\n=========================================================================";
                }
            }
            catch { }

            try
            {
                if (cocBody != null)
                {
                    cocBody.richText = true;
                    cocBody.text = "==============================================\n\nWelcome To JUUL Mod Menu! We Are A Free And Open Source Mod Menu\n<#FF0000>If You Have Problem On The Menu Or You Have A Suggestion, Check Out Our Discord !\nIf You Get Banned With 1 Mod On This Menu, Please Report The Detected Mod In The Discord !</color>\nYou Know Everything About The Menu\nNow Have Fun With Juul\n\n==============================================";
                }
            }
            catch { }
            if (!_origBoardMatsCaptured)
            {
                try { if (monitorScreen != null) _origMonitorScreenMat = monitorScreen.material; } catch { }
                try { if (wallMonitor != null) _origWallMonitorMat = wallMonitor.material; } catch { }
                _origBoardMatsCaptured = true;
            }
            if (IsBoardGradientEnabled && BoardMat != null)
            {
                try { if (monitorScreen != null) monitorScreen.material = BoardMat; } catch { }
                try { if (wallMonitor != null) wallMonitor.material = BoardMat; } catch { }
            }
            else if (!IsBoardGradientEnabled)
            {
                try { if (monitorScreen != null && _origMonitorScreenMat != null) monitorScreen.material = _origMonitorScreenMat; } catch { }
                try { if (wallMonitor != null && _origWallMonitorMat != null) wallMonitor.material = _origWallMonitorMat; } catch { }
            }
            GetOtherBoards();
        }
        public static bool pcFlipped = false;

        public static void Prefix()
        {
            if (Buttons.Modules == null) return;

            CleanupObjectPool();
            if (CachedMainCamera == null) CachedMainCamera = Camera.main;

            rigCacheTimer += Time.deltaTime;
            if (rigCacheTimer > 0.5f)
            {
                if (VRRigCache.ActiveRigs != null)
                    CachedActiveRigs = VRRigCache.ActiveRigs.ToArray();
                else
                    CachedActiveRigs = Array.Empty<VRRig>();
                rigCacheTimer = 0f;
            }

            Boards();
            PlayerMenu.Tick();
            PlayerMenu.UpdateSpectate();
            Soundboard.UpdateSoundboard();

            for (int i = ActiveButtons.Count - 1; i >= 0; i--)
            {
                if (i < ActiveButtons.Count)
                {
                    Button btn = ActiveButtons[i];
                    if (btn != null && btn.Enabled) btn.OnUpdate?.Invoke();
                }
            }

            if (TPC == null)
            {
                tpcSearchTimer += Time.deltaTime;
                if (tpcSearchTimer > 1.5f)
                {
                    tpcSearchTimer = 0f;
                    GameObject camObj = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
                    if (camObj == null) camObj = GameObject.Find("Shoulder Camera");
                    if (camObj != null) TPC = camObj.GetComponent<Camera>();
                }
            }
            else
            {
                tpcSearchTimer = 0f;
            }

            if (VRRig.LocalRig == null) return;
            ServerPos = ServerPos == Vector3.zero ? ServerSyncPos : Vector3.Lerp(ServerPos, VRRig.LocalRig.SanitizeVector3(ServerSyncPos), VRRig.LocalRig.lerpValueBody * 0.66f);
            ServerLeftHandPos = ServerLeftHandPos == Vector3.zero ? ServerSyncLeftHandPos : Vector3.Lerp(ServerLeftHandPos, VRRig.LocalRig.SanitizeVector3(ServerSyncLeftHandPos), VRRig.LocalRig.lerpValueBody);
            ServerRightHandPos = ServerRightHandPos == Vector3.zero ? ServerSyncRightHandPos : Vector3.Lerp(ServerRightHandPos, VRRig.LocalRig.SanitizeVector3(ServerSyncRightHandPos), VRRig.LocalRig.lerpValueBody);
            if (PhotonNetwork.InRoom && !inroomrel)
            {
                inroomrel = true;
            }
            if (!MenuStart)
            {
                MenuStart = true;
                Buttons.Initialize();
                Configs.LoadConfig();


            }
            if (BoardGradientObject == null)
            {
                BoardGradientObject = new GameObject("JuulBoardGradient");
                GameObject.DontDestroyOnLoad(BoardGradientObject);
                BoardGradientObject.AddComponent<MeshRenderer>();
                GradientSetter gs = BoardGradientObject.AddComponent<GradientSetter>();
                gs.gradientOffset = 0f;
            }
            if (ActiveCategory == null && Buttons.Modules != null && Buttons.Modules.Length > 0)
            {
                ActiveCategory = Buttons.Modules[0];
            }
            if (Themes.List != null)
            {
                for (int i = 0; i < Themes.List.Length; i++)
                {
                    Themes.List[i].Color = CycleColors(Themes.List[i].Colors, Themes.List[i].Speed);
                }
            }
            BaseColor = Color.Lerp(BaseColor, GetCurrentThemeColor(), Time.deltaTime * 5.5f);
            bool tabPressed = false;
            try
            {
                if (UnityEngine.InputSystem.Keyboard.current != null)
                {
                    if (UnityEngine.InputSystem.Keyboard.current.qKey.isPressed)
                    {
                        isPCMenuOpen = true;
                    }
                    else
                    {
                        isPCMenuOpen = false;
                    }
                    tabPressed = isPCMenuOpen;
                }
            }
            catch { }
            bool isVR = UnityEngine.XR.XRSettings.isDeviceActive;
            bool vRSearchStayOpen = isVR && (SearchManager.IsSearching || KeyboardManager.IsJoiningRoom || KeyboardManager.IsSavingPreset || KeyboardManager.IsSettingName || ButtonConfigs.IsListening);
            bool isPopoutRunning = Menu != null && Menu.GetComponent<PopoutAnimation>() != null;
            bool shouldOpenMenu = (IsRightHanded ? Inputs.RightSecondary : Inputs.LeftSecondary) || tabPressed || vRSearchStayOpen || SearchManager.WasSearchingLastFrame || KeyboardManager.WasJoiningRoomLastFrame || isPopoutRunning || ButtonConfigs.KeepMenuOpen;
            bool menuJustOpened = false;
            if (shouldOpenMenu)
            {
                if (Menu == null)
                {
                    menuJustOpened = true;
                    CreateFrame();
                    if (SearchManager.IsSearching || KeyboardManager.IsJoiningRoom || KeyboardManager.IsSavingPreset || KeyboardManager.IsSettingName)
                        KeyboardManager.ToggleKeyboard(true);
                    Audios.Play("Home", 0.35f);
                    menuWasOpenedViaPC = tabPressed;
                    Transform hand = IsRightHanded ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
                    if (tabPressed && TPC != null)
                    {
                        bool isSearchingActiveOpen = (!UnityEngine.XR.XRSettings.isDeviceActive && (SearchManager.IsSearching || KeyboardManager.IsJoiningRoom || KeyboardManager.IsSavingPreset || KeyboardManager.IsSettingName || ButtonConfigs.IsListening));
                        float initDist = isSearchingActiveOpen ? 1.0f : 0.6f;
                        Vector3 normalTargetPos = TPC.transform.position + TPC.transform.forward * initDist;
                        float flipAngle = pcFlipped ? 180f : 0f;
                        Quaternion normalTargetRot = Quaternion.LookRotation(TPC.transform.position - normalTargetPos) * Quaternion.Euler(0f, flipAngle, 0f) * Quaternion.Euler(-90f, 0f, -90f);
                        Menu.transform.position = normalTargetPos;
                        Menu.transform.rotation = normalTargetRot;
                    }
                    else
                    {
                        Menu.transform.position = hand.position;
                        Menu.transform.rotation = hand.rotation * (IsRightHanded ? Quaternion.Euler(180f, 180f, 0f) : Quaternion.identity);
                    }
                    Menu.AddComponent<ScaleInAnimation>();
                    IsMenuOpen = true;
                    if (Pointer == null)
                    {
                        Pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Pointer.transform.parent = IsRightHanded ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
                        Pointer.GetComponent<Renderer>().material.color = Color.white;
                        Pointer.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                        Pointer.transform.localScale = Vector3.one * 0.0075f;
                        Pointer.layer = 2;
                    }
                    else
                    {
                        Pointer.transform.parent = IsRightHanded ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
                    }

                }
                if (Menu != null)
                {

                    bool keyboardVisible = UnityEngine.XR.XRSettings.isDeviceActive &&
                        (KeyboardManager.IsJoiningRoom || KeyboardManager.IsSavingPreset || SearchManager.IsSearching);
                    if (keyboardVisible)
                    {
                        if (Pointer2 == null)
                        {
                            Pointer2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            Pointer2.transform.SetParent(IsRightHanded ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform, false);
                            Pointer2.GetComponent<Renderer>().material.color = Color.white;
                            Pointer2.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                            Pointer2.transform.localScale = Vector3.one * 0.0075f;
                            Pointer2.layer = 2;
                        }
                    }
                    else if (Pointer2 != null)
                    {
                        if (Pointer2.GetComponent<Renderer>() != null) GameObject.Destroy(Pointer2.GetComponent<Renderer>().material);
                        GameObject.Destroy(Pointer2);
                        Pointer2 = null;
                    }
                    if ((tabPressed || menuWasOpenedViaPC) && TPC != null)
                    {
                        bool isSearchingActive = (!UnityEngine.XR.XRSettings.isDeviceActive && (SearchManager.IsSearching || KeyboardManager.IsJoiningRoom || KeyboardManager.IsSavingPreset || KeyboardManager.IsSettingName || ButtonConfigs.IsListening));
                        float zoomDist = isSearchingActive ? 1.0f : 0.6f;
                        Vector3 targetPos = TPC.transform.position + TPC.transform.forward * zoomDist;
                        float flipAngle = pcFlipped ? 180f : 0f;
                        Quaternion targetRot = Quaternion.LookRotation(TPC.transform.position - targetPos) * Quaternion.Euler(0f, flipAngle, 0f) * Quaternion.Euler(-90f, 0f, -90f);

                        if (isSearchingActive && !(SearchManager.WasSearchingLastFrame || KeyboardManager.WasJoiningRoomLastFrame || ButtonConfigs.WasListeningLastFrame))
                        {
                            if (menuJustOpened)
                            {

                            }
                            else if (Core.IsAnimated)
                            {
                                ScaleInAnimation oldAnim = Menu.GetComponent<ScaleInAnimation>();
                                if (oldAnim != null) GameObject.Destroy(oldAnim);
                                PopoutAnimation pop = Menu.AddComponent<PopoutAnimation>();
                                pop.doScale = false;
                                pop.targetPosition = targetPos;
                                pop.targetRotation = targetRot;
                                pop.originPosition = Menu.transform.position;
                                pop.originRotation = Menu.transform.rotation;
                            }
                            SearchManager.WasSearchingLastFrame = true;
                            KeyboardManager.WasJoiningRoomLastFrame = true;
                            ButtonConfigs.WasListeningLastFrame = true;
                        }
                        else if (!isSearchingActive && (SearchManager.WasSearchingLastFrame || KeyboardManager.WasJoiningRoomLastFrame || ButtonConfigs.WasListeningLastFrame))
                        {
                            if (Core.IsAnimated)
                            {
                                ScaleInAnimation oldAnim = Menu.GetComponent<ScaleInAnimation>();
                                if (oldAnim != null) GameObject.Destroy(oldAnim);
                                PopoutAnimation pop = Menu.AddComponent<PopoutAnimation>();
                                pop.doScale = false;
                                pop.targetPosition = targetPos;
                                pop.targetRotation = targetRot;
                                pop.originPosition = Menu.transform.position;
                                pop.originRotation = Menu.transform.rotation;
                            }
                            SearchManager.WasSearchingLastFrame = false;
                            KeyboardManager.WasJoiningRoomLastFrame = false;
                            ButtonConfigs.WasListeningLastFrame = false;
                        }

                        if (Menu.GetComponent<PopoutAnimation>() != null)
                        {
                            PopoutAnimation pop = Menu.GetComponent<PopoutAnimation>();
                            pop.targetPosition = targetPos;
                            pop.targetRotation = targetRot;
                        }
                        else
                        {
                            Menu.transform.position = targetPos;
                            Menu.transform.rotation = targetRot;
                        }
                        
                        if (UnityEngine.InputSystem.Mouse.current != null)
                        {
                            Ray ray = TPC.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
                            if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                            {
                                int hitCount = Physics.RaycastNonAlloc(ray, raycastHits, 512f, uiLayerMask);
                                if (hitCount > 0)
                                {
                                    System.Array.Sort(raycastHits, 0, hitCount, Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));
                                    for (int i = 0; i < hitCount; i++)
                                    {
                                        IncrementalButtonCollider incrementalCollider = raycastHits[i].collider.GetComponent<IncrementalButtonCollider>();
                                        if (incrementalCollider != null)
                                        {
                                            if (Time.time > IncrementCooldown)
                                            {
                                                IncrementCooldown = Time.time + 0.15f;
                                                incrementalCollider.onClick?.Invoke();
                                                Audios.Play("Select");
                                                RebuildMenu();
                                            }
                                            break;
                                        }
                                        ButtonCollider buttonCollider = raycastHits[i].collider.GetComponent<ButtonCollider>();
                                        if (buttonCollider != null)
                                        {
                                            if (Time.time > ButtonCooldown)
                                            {
                                                ButtonCooldown = Time.time + 0.2345f;
                                                buttonCollider.onClick?.Invoke();
                                                Audios.Play("Select");
                                                RebuildMenu();
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (vRSearchStayOpen)
                    {
                        if (!(SearchManager.WasSearchingLastFrame || KeyboardManager.WasJoiningRoomLastFrame || ButtonConfigs.WasListeningLastFrame) && GorillaTagger.Instance != null && GorillaTagger.Instance.headCollider != null)
                        {
                            Transform head = GorillaTagger.Instance.headCollider.transform;
                            Vector3 targetPos = head.position + head.forward * 0.8f;
                            Quaternion targetRot = Quaternion.LookRotation(head.position - targetPos) * Quaternion.Euler(0f, 0f, 0f) * Quaternion.Euler(-90f, 0f, -90f);

                            if (Core.IsAnimated)
                            {
                                ScaleInAnimation oldAnim = Menu.GetComponent<ScaleInAnimation>();
                                if (oldAnim != null) GameObject.Destroy(oldAnim);
                                PopoutAnimation pop = Menu.AddComponent<PopoutAnimation>();
                                pop.doScale = false;
                                pop.targetPosition = targetPos;
                                pop.targetRotation = targetRot;
                                pop.originPosition = Menu.transform.position;
                                pop.originRotation = Menu.transform.rotation;
                            }
                            else
                            {
                                Menu.transform.position = targetPos;
                                Menu.transform.rotation = targetRot;
                            }
                            SearchManager.WasSearchingLastFrame = true;
                            KeyboardManager.WasJoiningRoomLastFrame = true;
                            ButtonConfigs.WasListeningLastFrame = true;
                        }
                    }
                    else
                    {
                        Transform hand = IsRightHanded ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
                        Quaternion targetRot = hand.rotation * (IsRightHanded ? Quaternion.Euler(180f, 180f, 0f) : Quaternion.identity);

                        if (SearchManager.WasSearchingLastFrame || KeyboardManager.WasJoiningRoomLastFrame || ButtonConfigs.WasListeningLastFrame)
                        {
                            if (!menuWasOpenedViaPC && Core.IsAnimated)
                            {
                                ScaleInAnimation oldAnim = Menu.GetComponent<ScaleInAnimation>();
                                if (oldAnim != null) GameObject.Destroy(oldAnim);
                                PopoutAnimation pop = Menu.AddComponent<PopoutAnimation>();
                                pop.doScale = false;
                                pop.targetPosition = hand.position;
                                pop.targetRotation = targetRot;
                                pop.originPosition = Menu.transform.position;
                                pop.originRotation = Menu.transform.rotation;
                            }
                        }
                        SearchManager.WasSearchingLastFrame = false;
                        KeyboardManager.WasJoiningRoomLastFrame = false;
                        ButtonConfigs.WasListeningLastFrame = false;

                        if (!menuWasOpenedViaPC)
                        {
                            if (Menu.GetComponent<PopoutAnimation>() != null)
                            {
                                PopoutAnimation pop = Menu.GetComponent<PopoutAnimation>();
                                pop.targetPosition = hand.position;
                                pop.targetRotation = targetRot;
                            }
                            else
                            {
                                if (MenuSmoothing)
                                {
                                    Menu.transform.position = Vector3.Lerp(Menu.transform.position, hand.position, Time.deltaTime * MenuSmoothingSpeed);
                                    Menu.transform.rotation = Quaternion.Lerp(Menu.transform.rotation, targetRot, Time.deltaTime * MenuSmoothingSpeed);
                                }
                                else
                                {
                                    Menu.transform.position = hand.position;
                                    Menu.transform.rotation = targetRot;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                SearchManager.WasSearchingLastFrame = false;
                KeyboardManager.WasJoiningRoomLastFrame = false;
                ButtonConfigs.WasListeningLastFrame = false;
                if (Menu != null && Menu.GetComponent<ScaleInAnimation>() == null)
                {
                    var scaleInAnimation = Menu.AddComponent<ScaleInAnimation>();
                    scaleInAnimation.reverse = true;

                    if (menuWasOpenedViaPC && TPC != null)
                    {
                        Vector3 targetPos = TPC.transform.position + TPC.transform.forward * 0.6f;
                        float flipAngle = pcFlipped ? 180f : 0f;
                        Quaternion targetRot = Quaternion.LookRotation(TPC.transform.position - targetPos) * Quaternion.Euler(0f, flipAngle, 0f) * Quaternion.Euler(-90f, 0f, -90f);

                        if (Vector3.Distance(Menu.transform.position, targetPos) > 0.05f)
                        {
                            PopoutAnimation pop = Menu.AddComponent<PopoutAnimation>();
                            pop.doScale = false;
                            pop.speed = 1.5f;
                            pop.targetPosition = targetPos;
                            pop.targetRotation = targetRot;
                            pop.originPosition = Menu.transform.position;
                            pop.originRotation = Menu.transform.rotation;
                        }
                    }

                    if (Pointer != null && Pointer.GetComponent<Renderer>() != null) { GameObject.Destroy(Pointer.GetComponent<Renderer>().material); }
                    if (Pointer != null) GameObject.Destroy(Pointer);
                    Pointer = null;
                    if (Pointer2 != null && Pointer2.GetComponent<Renderer>() != null) { GameObject.Destroy(Pointer2.GetComponent<Renderer>().material); }
                    if (Pointer2 != null) GameObject.Destroy(Pointer2);
                    Pointer2 = null;
                    IsMenuOpen = false;
                }
            }
            if (Buttons.Modules != null)
            {
                for (int i = 0; i < Buttons.Modules.Length; i++)
                {
                    Category category = Buttons.Modules[i];
                    if (category == ExtraButtons.EnabledCategory) continue;
                    List<Button> allButtons = new List<Button>();
                    ExtraButtons.GetButtonsRecursive(category, allButtons);
                    for (int j = 0; j < allButtons.Count; j++)
                    {
                        Button button = allButtons[j];
                        if (button.Enabled)
                            button.OnEnable();
                    }
                }
            }
        }
        public static void RebuildMenu()
        {
            Vector3? lastPos = null;
            Quaternion? lastRot = null;
            Vector3? kbLocalPos = null;
            Quaternion? kbLocalRot = null;
            Vector3? kbLocalScale = null;

            if (KeyboardManager.KeyboardObj != null && !KeyboardManager.KeyboardObj.Equals(null))
            {
                kbLocalPos = KeyboardManager.KeyboardObj.transform.localPosition;
                kbLocalRot = KeyboardManager.KeyboardObj.transform.localRotation;
                kbLocalScale = KeyboardManager.KeyboardObj.transform.localScale;
                KeyboardManager.KeyboardObj.transform.SetParent(null, true);
            }

            if (Menu != null)
            {
                lastPos = Menu.transform.position;
                lastRot = Menu.transform.rotation;
                CleanupMenu();
                GameObject.Destroy(Menu);
                Menu = null;
            }
            CreateFrame();
            if (lastPos.HasValue)
            {
                Menu.transform.position = lastPos.Value;
                Menu.transform.rotation = lastRot.Value;
            }

            if (KeyboardManager.KeyboardObj != null && !KeyboardManager.KeyboardObj.Equals(null))
            {
                KeyboardManager.KeyboardObj.transform.SetParent(Menu.transform, true);
                if (kbLocalPos.HasValue)
                {
                    KeyboardManager.KeyboardObj.transform.localPosition = kbLocalPos.Value;
                    KeyboardManager.KeyboardObj.transform.localRotation = kbLocalRot.Value;
                    KeyboardManager.KeyboardObj.transform.localScale = kbLocalScale.Value;
                }
            }

            if (SearchManager.IsSearching || KeyboardManager.IsJoiningRoom || KeyboardManager.IsSavingPreset || KeyboardManager.IsSettingName)
                KeyboardManager.ToggleKeyboard(true);
            else
                KeyboardManager.ToggleKeyboard(false);
        }
        private static void CleanupMenu()
        {
            if (Menu == null) return;
            GradientSetter[] gradients = Menu.GetComponentsInChildren<GradientSetter>();
            foreach (var gradient in gradients)
            {
                if (gradient != null)
                    GameObject.Destroy(gradient);
            }
            ColorSetter[] colors = Menu.GetComponentsInChildren<ColorSetter>();
            foreach (var color in colors)
            {
                if (color != null)
                    GameObject.Destroy(color);
            }
            Renderer[] renderers = Menu.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                    GameObject.Destroy(renderer.material);
            }
        }
        public static void AddText(string Text, float Size, Vector3 Position, Vector3 Rotation = default(Vector3), bool Bold = false)
        {
            if (Canvas == null) return;
            GameObject gameObject = new GameObject();
            gameObject.transform.SetParent(Canvas.transform, false);
            Text text = gameObject.AddComponent<Text>();
            text.font = MenuFont;
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            text.text = Text;
            text.color = Color.white;
            text.fontStyle = Bold ? FontStyle.Bold : FontStyle.Normal;
            text.material.renderQueue = 4000;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.24f, 0.035f * Size);
            component.localPosition = Position;
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f) + Rotation);
        }
        private static Texture2D _searchIconTexture;
        public static Texture2D GetSearchIconTexture()
        {
            if (_searchIconTexture != null) return _searchIconTexture;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = null;
            foreach (string name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith("search.png", System.StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }
            if (resourceName != null)
            {
                using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                        ImageConversion.LoadImage(tex, data);
                        tex.filterMode = FilterMode.Trilinear;
                        tex.anisoLevel = 8;
                        tex.Apply(true, false);
                        _searchIconTexture = tex;
                    }
                }
            }
            return _searchIconTexture;
        }
        private static Texture2D _gearIconTexture;
        public static Texture2D GetGearIconTexture()
        {
            if (_gearIconTexture != null) return _gearIconTexture;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = null;
            foreach (string name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith("Gear Settings.png", System.StringComparison.OrdinalIgnoreCase) || name.EndsWith("Gear_Settings.png", System.StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }
            if (resourceName != null)
            {
                using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                        ImageConversion.LoadImage(tex, data);
                        tex.filterMode = FilterMode.Trilinear;
                        tex.anisoLevel = 8;
                        tex.Apply(true, false);
                        _gearIconTexture = tex;
                    }
                }
            }
            return _gearIconTexture;
        }
        private static Texture2D _xIconTexture;
        public static Texture2D GetXIconTexture()
        {
            if (_xIconTexture != null) return _xIconTexture;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = null;
            foreach (string name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith("X.png", System.StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }
            if (resourceName != null)
            {
                using (System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                        ImageConversion.LoadImage(tex, data);
                        tex.filterMode = FilterMode.Trilinear;
                        tex.anisoLevel = 8;
                        tex.Apply(true, false);
                        _xIconTexture = tex;
                    }
                }
            }
            return _xIconTexture;
        }
        public static void AddImage(Texture2D texture, float Size, Vector3 Position, Vector3 Rotation = default(Vector3), Vector2 canvasOffset = default(Vector2))
        {
            if (Canvas == null || texture == null) return;
            GameObject gameObject = new GameObject();
            gameObject.transform.SetParent(Canvas.transform, false);
            UnityEngine.UI.RawImage image = gameObject.AddComponent<UnityEngine.UI.RawImage>();
            image.texture = texture;
            image.color = Color.white;
            if (image.material != null) image.material.renderQueue = 4000;
            RectTransform component = image.GetComponent<RectTransform>();
            component.pivot = new Vector2(0.5f, 0.5f);
            component.anchorMin = new Vector2(0.5f, 0.5f);
            component.anchorMax = new Vector2(0.5f, 0.5f);
            component.localPosition = Vector3.zero;
            float targetSize = 0.035f * Size * 0.9f;
            component.sizeDelta = new Vector2(targetSize, targetSize);
            component.localPosition = Position;
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f) + Rotation);
            component.Translate(new Vector3(0f, 0f, 0.000f), Space.Self);
            component.anchoredPosition += canvasOffset;
        }
        public static Light MenuLight;
        public static bool BlockRebuild = false;
        public static void DelayAction(float delay, System.Action action)
        {
            GameObject dl = new GameObject("Delayer");
            Delayer d = dl.AddComponent<Delayer>();
            d.delay = delay;
            d.action = action;
        }
        public class Delayer : MonoBehaviour
        {
            public float delay;
            public System.Action action;
            private float t;
            void Update() {
               t += Time.deltaTime; 
               if(t >= delay) { action?.Invoke(); Destroy(gameObject); }
            }
        }
        public static float MenuWidth = 0.8f;
        public static float BtnInset = 0.1f;
        public static float BtnUpset = 0.3f;
        public static float BtnHeight = 0.07f;
        public static float BtnSpace = 0.005f;
        public static float TextSize = 0.5f;
        public static float GradVal = 0.05f;

        public static List<Button> ActiveButtons = new List<Button>();
        public static void ChangeMenuScale(bool forward)
        {
            if (forward && MenuWidth >= 2f) MenuWidth = 0.45f;
            if (!forward && MenuWidth <= 0.45f) MenuWidth = 2f;
            MenuWidth = MenuWidth + (forward ? 0.025f : -0.025f);
            RebuildMenu();
        }
        public static void ChangeButtonInset(bool forward)
        {
            if (forward && BtnInset >= 0.5f) BtnInset = 0f;
            if (!forward && BtnInset <= 0f) BtnInset = 0.4f;
            BtnInset = BtnInset + (forward ? 0.025f : -0.025f);
            RebuildMenu();
        }
        public static void ChangeTextSize(bool forward)
        {
            if (forward && TextSize >= 1.1f) TextSize = 0.3f;
            if (!forward && TextSize <= 0.3f) TextSize = 0.9f;
            TextSize = TextSize + (forward ? 0.025f : -0.025f);
            RebuildMenu();
        }

        private static Vector3 GetCatNavTextPos(GameObject obj, Vector3 rotEuler)
        {
            Vector3 localFaceNormal = Quaternion.Euler(rotEuler) * new Vector3(1f, 0f, 0f);
            Vector3 worldFaceNormal = Menu.transform.TransformDirection(localFaceNormal);
            return obj.transform.position + worldFaceNormal * SmFl;
        }

        public static void CreateFrame()
        {
            Menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Menu.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            Rigidbody menuRigid = Menu.GetComponent<Rigidbody>();
            if (menuRigid == null) menuRigid = Menu.AddComponent<Rigidbody>();
            menuRigid.isKinematic = true;
            menuRigid.useGravity = false;
            GameObject.Destroy(Menu.GetComponent<Collider>());
            GameObject.Destroy(Menu.GetComponent<Renderer>());
            Canvas = new GameObject();
            Canvas.transform.SetParent(Menu.transform, true);
            Canvas canvas = Canvas.AddComponent<Canvas>();
            CanvasScaler canvasScaler = Canvas.AddComponent<CanvasScaler>();
            Canvas.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 2000f;
            Frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Frame.name = "Menu Frame";
            Frame.transform.parent = Menu.transform;
            Frame.transform.rotation = Quaternion.identity;
            Frame.transform.localScale = new Vector3(SmFl, MenuWidth, 0.9f);
            Frame.transform.localPosition = new Vector3(SmFl * 40f, 0f, 0f);
            GameObject.Destroy(Frame.GetComponent<Rigidbody>());
            GameObject.Destroy(Frame.GetComponent<BoxCollider>());
            GradientSetter frameGradient = Frame.AddComponent<GradientSetter>();
            if (IsRounded) Frame.AddComponent<RoundedCorners>();
            OutlineGradient(Frame);

            if (IsMenuParticles)
            {
                GameObject particleSys = new GameObject("Menu Particles");
                particleSys.transform.parent = Menu.transform;
                particleSys.transform.localPosition = new Vector3(SmFl * 40f, 0f, 0f);
                particleSys.transform.localRotation = Quaternion.identity;
                
                UnityEngine.ParticleSystem ps = particleSys.AddComponent<UnityEngine.ParticleSystem>();
                var main = ps.main;
                main.loop = true;
                main.startLifetime = new UnityEngine.ParticleSystem.MinMaxCurve(1.5f, 2.5f);
                main.startSpeed = 0f;
                main.startSize = new UnityEngine.ParticleSystem.MinMaxCurve(0.01f, 0.025f);
                main.maxParticles = 80;
                main.gravityModifier = -0.005f; 
                main.simulationSpace = UnityEngine.ParticleSystemSimulationSpace.World;
                
                var emission = ps.emission;
                emission.rateOverTime = 20f;

                var shape = ps.shape;
                shape.shapeType = UnityEngine.ParticleSystemShapeType.Box;
                shape.scale = new Vector3(0.01f, 0.45f, 0.9f);

                var sol = ps.sizeOverLifetime;
                sol.enabled = true;
                UnityEngine.AnimationCurve curve = new UnityEngine.AnimationCurve();
                curve.AddKey(0f, 0f);
                curve.AddKey(0.5f, 1f);
                curve.AddKey(1f, 0f);
                sol.size = new UnityEngine.ParticleSystem.MinMaxCurve(1f, curve);

                var vel = ps.velocityOverLifetime;
                vel.enabled = true;
                vel.x = new UnityEngine.ParticleSystem.MinMaxCurve(-0.04f, 0.04f);
                vel.y = new UnityEngine.ParticleSystem.MinMaxCurve(0.01f, 0.05f);
                vel.z = new UnityEngine.ParticleSystem.MinMaxCurve(-0.04f, 0.04f);

                var renderer = particleSys.GetComponent<UnityEngine.ParticleSystemRenderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                
                particleSys.AddComponent<ParticleThemeUpdater>();
            }

            Sidebar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Sidebar.transform.parent = Menu.transform;
            Sidebar.transform.localScale = new Vector3(SmFl, 0.45f, 0.9f);
            Sidebar.transform.localPosition = new Vector3(SmFl * 40f + (IsCatRotated ? Sidebar.transform.localScale.y / 2f : 0f), (IsCatLeft ? -((Frame.transform.localScale.y / 2f) + (Sidebar.transform.localScale.y / 2f)) : ((Frame.transform.localScale.y / 2f) + (Sidebar.transform.localScale.y / 2f))) + (IsCatRotated ? 0f : (IsCatLeft ? -(SmFl * 20f) : (SmFl * 20f))), 0f);
            Sidebar.transform.localRotation = Quaternion.Euler(0f, 0f, IsCatRotated ? (IsCatLeft ? 45f : (-45f)) : 0f);
            GameObject.Destroy(Sidebar.GetComponent<Rigidbody>());
            GameObject.Destroy(Sidebar.GetComponent<BoxCollider>());
            GradientSetter sidebarGradient = Sidebar.AddComponent<GradientSetter>();
            if (IsRounded) Sidebar.AddComponent<RoundedCorners>();
            OutlineGradient(Sidebar);
            if (Core.IsAnimated && (Core.LastClickedButtonID == "Btn_Sidebar Position" || Core.LastClickedButtonID == "Btn_Rotated Sidebar"))
            {
                Sidebar.AddComponent<ScaleInAnimation>();
            }
            GameObject disconnectButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
            disconnectButton.name = "Disconnect Button";
            disconnectButton.layer = 2;
            disconnectButton.transform.parent = Menu.transform;
            disconnectButton.transform.localScale = new Vector3(SmFl, MenuWidth, 0.075f);
            disconnectButton.transform.localPosition = new Vector3(SmFl * 40f, 0f, 0.5f);
            GameObject.Destroy(disconnectButton.GetComponent<Rigidbody>());
            GradientSetter disconnectColor = disconnectButton.AddComponent<GradientSetter>();
            disconnectColor.gradientOffset = 0f;
            disconnectColor.buttonID = "DisconnectBtn";
            BoxCollider disconnectComponent = disconnectButton.GetComponent<BoxCollider>();
            disconnectComponent.isTrigger = true;
            ButtonCollider disButtonCollider = disconnectButton.AddComponent<ButtonCollider>();
            disButtonCollider.onClick = () => { LastClickedButtonID = disconnectColor.buttonID; PhotonNetwork.Disconnect(); };
            if (IsRounded)
            {
                RoundedCorners disconnectCorners = disconnectButton.AddComponent<RoundedCorners>();
                disconnectCorners.bevel = disconnectCorners.bevel / 2f;
            }
            OutlineGradient(disconnectButton);
            AddText("Disconnect", TextSize, disconnectButton.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
            string titleText = SearchManager.IsSearching ? ("Search: " + SearchManager.SearchQuery) : Plugin.title;
            AddText(titleText, 1f, Frame.transform.position + new Vector3(SmFl, 0f, 0.1625f), default(Vector3), true);
            BtnIndex = 0;
            CatIndex = 0;
            if (Buttons.Modules != null)
            {
                ExtraButtons.RefreshEnabledCategory();
                int startCat = CurrentCatPage * MaxCatsPerPage;
                int endCat = Mathf.Min(startCat + MaxCatsPerPage, Buttons.Modules.Length);
                int count = endCat - startCat;
                float maxTotalH = MaxCatsPerPage * BtnHeight + Mathf.Max(0, MaxCatsPerPage - 1) * BtnSpace;
                float catStartZ = (maxTotalH / 2f) - (BtnHeight / 2f);
                for (int i = startCat; i < endCat; i++)
                {
                    AddCategory(Buttons.Modules[i].Name, catStartZ);
                }
            }
            if (Buttons.Modules != null && Buttons.Modules.Length > MaxCatsPerPage)
            {
                Vector3 rotEuler = new Vector3(0f, 0f, IsCatRotated ? (IsCatLeft ? 45f : -45f) : 0f);
                Vector3 fNormal = Quaternion.Euler(rotEuler) * Vector3.right;
                Vector3 baseP = Sidebar.transform.localPosition + fNormal * SmFl;
                float offsetOuter = 0.225f - (BtnHeight / 2f) - 0.02f;
                float offsetInner = offsetOuter - (BtnHeight + 0.005f);
                float offsetSearch = offsetInner - (BtnHeight + 0.005f);
                Vector3 rotationEuler = rotEuler;
                Vector3 slideDir = Quaternion.Euler(rotationEuler) * Vector3.up;
                Vector3 centerPos = new Vector3(baseP.x, baseP.y, 0.45f + (BtnHeight / 2f) + 0.015f);
                Vector3 posForPrev;
                Vector3 posForNext;
                Vector3 posForSearchBtn;
                if (IsCatLeft)
                {
                    posForPrev = centerPos + slideDir * (-offsetInner);
                    posForNext = centerPos + slideDir * (-offsetOuter);
                    posForSearchBtn = centerPos + slideDir * (-offsetSearch);
                }
                else
                {
                    posForPrev = centerPos + slideDir * (offsetOuter);
                    posForNext = centerPos + slideDir * (offsetInner);
                    posForSearchBtn = centerPos + slideDir * (offsetSearch);
                }
                Vector3 textRotation = new Vector3(IsCatRotated ? (IsCatLeft ? (-45f) : 45f) : 0f, 0f, 0f);
                float btnGradLen = BtnHeight / 0.9f;
                float btnGradStart = 0f;

                GameObject searchCatObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                searchCatObj.layer = 2;
                searchCatObj.transform.parent = Menu.transform;
                searchCatObj.transform.rotation = Quaternion.identity;
                searchCatObj.transform.localScale = new Vector3(SmFl, BtnHeight, BtnHeight);
                searchCatObj.transform.localPosition = posForSearchBtn;
                searchCatObj.transform.localRotation = Quaternion.Euler(rotationEuler);
                GameObject.Destroy(searchCatObj.GetComponent<Rigidbody>());
                GradientSetter searchCs = searchCatObj.AddComponent<GradientSetter>();
                searchCs.gradientOffset = btnGradLen;
                searchCs.startOffset = btnGradStart;
                searchCs.buttonID = "SearchCatBtn";
                if (IsRounded)
                {
                    RoundedCorners corners = searchCatObj.AddComponent<RoundedCorners>();
                    corners.bevel = 0.015f;
                }
                OutlineGradient(searchCatObj);
                BoxCollider searchBox = searchCatObj.GetComponent<BoxCollider>();
                searchBox.isTrigger = true;
                ButtonCollider searchCol = searchCatObj.AddComponent<ButtonCollider>();
                searchCol.onClick = () =>
                {
                    LastClickedButtonID = searchCs.buttonID;
                    SearchButtonLastPos = searchCatObj.transform.position;
                    
                    if (KeyboardManager.IsSettingName)
                    {
                        KeyboardManager.IsSettingName = false;
                        KeyboardManager.NameQuery = "";
                        try
                        {
                            if (ExtraButtons.SetNameButton != null)
                            {
                                ExtraButtons.SetNameButton.Enabled = false;
                                ExtraButtons.SetNameButton.Name = "[Can Bypass] Set Name:";
                            }
                        }
                        catch { }
                    }
                    else if (KeyboardManager.IsJoiningRoom)
                    {
                        KeyboardManager.IsJoiningRoom = false;
                        KeyboardManager.JoinRoomQuery = "";
                        try { ExtraButtons.RoomJoinerButton.Enabled = false; ExtraButtons.RoomJoinerButton.Name = "Join Room: "; } catch { }
                    }
                    else if (KeyboardManager.IsSavingPreset)
                    {
                        KeyboardManager.IsSavingPreset = false;
                        KeyboardManager.PresetSaveQuery = "";
                        try { ExtraButtons.SavePresetButton.Enabled = false; } catch { }
                    }
                    else if (KeyboardManager.IsSettingQuestScore)
                    {
                        KeyboardManager.IsSettingQuestScore = false;
                        KeyboardManager.QuestScoreQuery = "";
                        try { ExtraButtons.QuestScoreButton.Enabled = false; } catch { }
                    }
                    else
                    {
                        SearchManager.IsSearching = !SearchManager.IsSearching;
                        if (SearchManager.IsSearching)
                        {
                            SearchManager.SearchQuery = "";
                            SearchManager.PerformSearch();
                            KeyboardManager.KeyboardJustOpened = true;
                        }
                        else
                        {
                            if (Buttons.Modules != null && Buttons.Modules.Length > 0)
                                ActiveCategory = Buttons.Modules[0];
                        }
                    }
                };

                Vector3 searchTextPos = GetCatNavTextPos(searchCatObj, rotationEuler);
                if (SearchManager.IsSearching || KeyboardManager.IsJoiningRoom || KeyboardManager.IsSavingPreset || KeyboardManager.IsSettingQuestScore || KeyboardManager.IsSettingName)
                {
                    AddImage(GetXIconTexture(), TextSize * 1.35f, searchTextPos, textRotation, Vector2.zero);
                }
                else
                {
                    AddImage(GetSearchIconTexture(), TextSize * 1.35f, searchTextPos, textRotation, Vector2.zero);
                }

                GameObject prevCatObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prevCatObj.layer = 2;
                prevCatObj.transform.parent = Menu.transform;
                prevCatObj.transform.rotation = Quaternion.identity;
                prevCatObj.transform.localScale = new Vector3(SmFl, BtnHeight, BtnHeight);
                prevCatObj.transform.localPosition = posForPrev;
                prevCatObj.transform.localRotation = Quaternion.Euler(rotationEuler);
                GameObject.Destroy(prevCatObj.GetComponent<Rigidbody>());
                GradientSetter prevCs = prevCatObj.AddComponent<GradientSetter>();
                prevCs.gradientOffset = btnGradLen;
                prevCs.startOffset = btnGradStart;
                prevCs.buttonID = "PrevCatBtn";
                if (IsRounded)
                {
                    RoundedCorners corners = prevCatObj.AddComponent<RoundedCorners>();
                    corners.bevel = 0.015f;
                }
                OutlineGradient(prevCatObj);
                BoxCollider prevBox = prevCatObj.GetComponent<BoxCollider>();
                prevBox.isTrigger = true;
                ButtonCollider prevCol = prevCatObj.AddComponent<ButtonCollider>();
                prevCol.onClick = () => { LastClickedButtonID = prevCs.buttonID; PreviousCatPage(); };

                Vector3 prevTextPos = GetCatNavTextPos(prevCatObj, rotationEuler);
                AddText("<", TextSize * 1.1f, prevTextPos + new Vector3(0f, 0f, 0.005f), textRotation);

                GameObject nextCatObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nextCatObj.layer = 2;
                nextCatObj.transform.parent = Menu.transform;
                nextCatObj.transform.rotation = Quaternion.identity;
                nextCatObj.transform.localScale = new Vector3(SmFl, BtnHeight, BtnHeight);
                nextCatObj.transform.localPosition = posForNext;
                nextCatObj.transform.localRotation = Quaternion.Euler(rotationEuler);
                GameObject.Destroy(nextCatObj.GetComponent<Rigidbody>());
                GradientSetter nextCs = nextCatObj.AddComponent<GradientSetter>();
                nextCs.gradientOffset = btnGradLen;
                nextCs.startOffset = btnGradStart;
                nextCs.buttonID = "NextCatBtn";
                if (IsRounded)
                {
                    RoundedCorners corners = nextCatObj.AddComponent<RoundedCorners>();
                    corners.bevel = 0.015f;
                }
                OutlineGradient(nextCatObj);
                BoxCollider nextBox = nextCatObj.GetComponent<BoxCollider>();
                nextBox.isTrigger = true;
                ButtonCollider nextCol = nextCatObj.AddComponent<ButtonCollider>();
                nextCol.onClick = () => { LastClickedButtonID = nextCs.buttonID; NextCatPage(); };

                Vector3 nextTextPos = GetCatNavTextPos(nextCatObj, rotationEuler);
                AddText(">", TextSize * 1.1f, nextTextPos + new Vector3(0f, 0f, 0.005f), textRotation);
            }
            if (ActiveCategory == null && Buttons.Modules != null && Buttons.Modules.Length > 0)
            {
                ActiveCategory = Buttons.Modules[0];
            }
            if (PageBtnVer == 2)
            {
                AddCustomButton("<<<<<<", () => PreviousPage());
                AddCustomButton(">>>>>>", () => NextPage());
            }
            if (PageBtnVer == 3)
            {
                BtnIndex++;
            }
            if (ActiveCategory != null)
            {
                RefreshButtons();
            }
            if (PageBtnVer == 0 || PageBtnVer == 3)
            {
                GameObject prevObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prevObj.layer = 2;
                prevObj.transform.parent = Menu.transform;
                prevObj.transform.rotation = Quaternion.identity;
                prevObj.transform.localScale = new Vector3(SmFl, ((MenuWidth - BtnInset) / 2f) - 0.005f, BtnHeight);
                prevObj.transform.localPosition = new Vector3(SmFl * 40f + SmFl, (((MenuWidth - BtnInset) / 2f) + 0.005f) / 2f, BtnUpset - (PageBtnVer == 3 ? 0f : ((BtnHeight + BtnSpace) * (MaxButtons + 1))));
                GameObject.Destroy(prevObj.GetComponent<Rigidbody>());
                BoxCollider boxColP = prevObj.GetComponent<BoxCollider>();
                boxColP.isTrigger = true;
                ColorSetter cs1 = prevObj.AddComponent<ColorSetter>();
                cs1.brightness = OffBrightness;
                cs1.colorOffset = -GradVal * 2;
                cs1.buttonID = "PageLeft";
                ButtonCollider buttonCollider = prevObj.AddComponent<ButtonCollider>();
                buttonCollider.onClick = () => { LastClickedButtonID = cs1.buttonID; PreviousPage(); };
                AddText("<", TextSize, prevObj.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
                GameObject nextObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nextObj.layer = 2;
                nextObj.transform.parent = Menu.transform;
                nextObj.transform.rotation = Quaternion.identity;
                nextObj.transform.localScale = new Vector3(SmFl, ((MenuWidth - BtnInset) / 2f) - 0.005f, BtnHeight);
                nextObj.transform.localPosition = new Vector3(SmFl * 40f + SmFl, -((((MenuWidth - BtnInset) / 2f) + 0.005f) / 2f), BtnUpset - (PageBtnVer == 3 ? 0f : ((BtnHeight + BtnSpace) * (MaxButtons + 1))));
                GameObject.Destroy(nextObj.GetComponent<Rigidbody>());
                BoxCollider boxColN = nextObj.GetComponent<BoxCollider>();
                boxColN.isTrigger = true;
                ColorSetter cs2 = nextObj.AddComponent<ColorSetter>();
                cs2.brightness = OffBrightness;
                cs2.colorOffset = -GradVal * 2;
                cs2.buttonID = "PageRight";
                ButtonCollider buttonCollider2 = nextObj.AddComponent<ButtonCollider>();
                buttonCollider2.onClick = () => { LastClickedButtonID = cs2.buttonID; NextPage(); };
                AddText(">", TextSize, nextObj.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
                if (PageBtnVer == 3) BtnIndex++;
            }
            else if (PageBtnVer == 1)
            {
                BtnIndex = MaxButtons;
                AddCustomButton("<<<<<<", PreviousPage);
                AddCustomButton(">>>>>>", NextPage);
            }
        }
        public static int BtnCount()
        {
            if (PageBtnVer == 1 || PageBtnVer == 2) return MaxButtons;
            else return MaxButtons + 1;
        }
        public static void RefreshButtons()
        {
            if (ActiveCategory == null) return;

            bool hasParent = ActiveCategory.ParentCategory != null;
            int totalItems = (hasParent ? 1 : 0) + ActiveCategory.Buttons.Count + ActiveCategory.Subcategories.Count;
            int buttonLimit = BtnCount();
            int startIndex = CurrentPage * buttonLimit;
            int endIndex = Mathf.Min(startIndex + buttonLimit, totalItems);

            for (int i = startIndex; i < endIndex; i++)
            {
                int currentPos = i;
                if (hasParent)
                {
                    if (currentPos == 0)
                    {
                        AddCustomButton("<<Back", () => {
                            LastClickedButtonID = "CustBtn_<<Back";
                            ActiveCategory = ActiveCategory.ParentCategory;
                            CurrentPage = 0;
                            RebuildMenu();
                        });
                        continue;
                    }
                    currentPos--;
                }

                if (currentPos < ActiveCategory.Buttons.Count)
                {
                    AddButton(ActiveCategory.Buttons[currentPos].Name);
                }
                else
                {
                    int subIndex = currentPos - ActiveCategory.Buttons.Count;
                    Category sub = ActiveCategory.Subcategories[subIndex];
                    AddCustomButton(sub.Name, () => {
                        LastClickedButtonID = "CustBtn_" + sub.Name;
                        sub.ParentCategory = ActiveCategory;
                        ActiveCategory = sub;
                        CurrentPage = 0;
                        RebuildMenu();
                    });
                }
            }
        }
        public static void NextPage()
        {
            if (ActiveCategory == null) return;
            int totalItems = (ActiveCategory.ParentCategory != null ? 1 : 0) + ActiveCategory.Buttons.Count + ActiveCategory.Subcategories.Count;
            int num = Mathf.CeilToInt((float)totalItems / (float)BtnCount());
            if (CurrentPage < num - 1)
            {
                CurrentPage++;
                RebuildMenu();
            }
            else
            {
                CurrentPage = 0;
                RebuildMenu();
            }
        }
        public static void PreviousPage()
        {
            if (ActiveCategory == null) return;
            if (CurrentPage > 0)
            {
                CurrentPage--;
                RebuildMenu();
            }
            else
            {
                int totalItems = (ActiveCategory.ParentCategory != null ? 1 : 0) + ActiveCategory.Buttons.Count + ActiveCategory.Subcategories.Count;
                int num = Mathf.Max(1, Mathf.CeilToInt((float)totalItems / (float)BtnCount()));
                CurrentPage = num - 1;
                RebuildMenu();
            }
        }
        public static void NextCatPage()
        {
            if (Buttons.Modules == null) return;
            int num = Mathf.CeilToInt((float)Buttons.Modules.Length / (float)MaxCatsPerPage);
            if (CurrentCatPage < num - 1)
            {
                CurrentCatPage++;
                RebuildMenu();
            }
            else
            {
                CurrentCatPage = 0;
                RebuildMenu();
            }
        }
        public static void PreviousCatPage()
        {
            if (Buttons.Modules == null) return;
            if (CurrentCatPage > 0)
            {
                CurrentCatPage--;
                RebuildMenu();
            }
            else
            {
                int num = Mathf.Max(1, Mathf.CeilToInt((float)Buttons.Modules.Length / (float)MaxCatsPerPage));
                CurrentCatPage = num - 1;
                RebuildMenu();
            }
        }
        public static Button GetButtonFromCategory(string Category, string Button)
        {
            if (Buttons.Modules == null) return null;
            for (int i = 0; i < Buttons.Modules.Length; i++)
            {
                if (Buttons.Modules[i].Name == Category || Buttons.Modules[i].Name.Contains(Category))
                {
                    List<Button> buttons = Buttons.Modules[i].Buttons;
                    for (int j = 0; j < buttons.Count; j++)
                    {
                        if (buttons[j].Name == Button || buttons[j].Name.Contains(Button))
                        {
                            return buttons[j];
                        }
                    }
                }
            }
            return null;
        }
        public static Button GetButtonByName(string Name)
        {
            if (Buttons.Modules == null) return null;
            for (int i = 0; i < Buttons.Modules.Length; i++)
            {
                List<Button> buttons = Buttons.Modules[i].Buttons;
                for (int j = 0; j < buttons.Count; j++)
                {
                    if (buttons[j].Name == Name || buttons[j].Name.Contains(Name))
                    {
                        return buttons[j];
                    }
                }
            }
            return null;
        }
        public static void AddCategory(string name, float startZ)
        {
            if (Menu == null || string.IsNullOrEmpty(name)) return;
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.layer = 2;
            gameObject.transform.parent = Menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(SmFl, 0.4f, BtnHeight);
            Vector3 rotEulerCat = new Vector3(0f, 0f, IsCatRotated ? (IsCatLeft ? 45f : (-45f)) : 0f);
            Vector3 fNormalCat = Quaternion.Euler(rotEulerCat) * Vector3.right;
            Vector3 catPos = Sidebar.transform.localPosition + fNormalCat * SmFl;
            gameObject.transform.localPosition = new Vector3(catPos.x, catPos.y, startZ - ((BtnHeight + BtnSpace) * CatIndex));
            gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, IsCatRotated ? (IsCatLeft ? 45f : (-45f)) : 0f);
            GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
            BoxCollider component = gameObject.GetComponent<BoxCollider>();
            component.isTrigger = true;
            Category category = null;
            if (Buttons.Modules != null)
            {
                for (int i = 0; i < Buttons.Modules.Length; i++)
                {
                    if (Buttons.Modules[i].Name == name)
                    {
                        category = Buttons.Modules[i];
                        break;
                    }
                }
            }
            float brightness = ((ActiveCategory == category) ? OnBrightness : OffBrightness);
            ColorSetter cs1 = gameObject.AddComponent<ColorSetter>();
            cs1.brightness = brightness;
            cs1.colorOffset = (CatIndex * GradVal) - GradVal;
            cs1.buttonID = "Cat_" + name;
            string categoryName = name;
            ButtonCollider buttonCollider = gameObject.AddComponent<ButtonCollider>();
            buttonCollider.onClick = () =>
            {
                LastClickedButtonID = cs1.buttonID;
                CurrentPage = 0;
                if (Buttons.Modules != null)
                {
                    for (int i = 0; i < Buttons.Modules.Length; i++)
                    {
                        if (Buttons.Modules[i].Name == categoryName)
                        {
                            ActiveCategory = Buttons.Modules[i];
                            break;
                        }
                    }
                }
                RebuildMenu();
            };
            AddText(name, TextSize, gameObject.transform.position + new Vector3(SmFl, 0f, SmFl), new Vector3(IsCatRotated ? (IsCatLeft ? (-45f) : 45f) : 0f, 0f, 0f));
            CatIndex++;
        }
        public static void AddCustomButton(string name, Action callback)
        {
            if (Menu == null) return;
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.layer = 2;
            gameObject.transform.parent = Menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(SmFl, (MenuWidth - BtnInset), BtnHeight);
            gameObject.transform.localPosition = new Vector3(SmFl * 40f + SmFl, 0f, BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
            GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
            BoxCollider component = gameObject.GetComponent<BoxCollider>();
            component.isTrigger = true;
            ColorSetter cs1 = gameObject.AddComponent<ColorSetter>();
            cs1.brightness = OffBrightness;
            cs1.colorOffset = (BtnIndex * GradVal) - GradVal;
            cs1.buttonID = "CustBtn_" + name;
            ButtonCollider buttonCollider = gameObject.AddComponent<ButtonCollider>();
            buttonCollider.onClick = () => { LastClickedButtonID = cs1.buttonID; callback(); };
            Vector3 textLocalPos = new Vector3(SmFl * 40f + SmFl, 0f, BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
            Vector3 textWorldPos = Menu.transform.TransformPoint(textLocalPos);
            AddText(name, TextSize, textWorldPos + new Vector3(SmFl, 0f, SmFl), default(Vector3));
            BtnIndex++;
        }
        public static void AddButton(string name)
        {
            if (ActiveCategory == null || Menu == null) return;
            Button button = null;
            for (int i = 0; i < ActiveCategory.Buttons.Count; i++)
            {
                if (ActiveCategory.Buttons[i].Name == name)
                {
                    button = ActiveCategory.Buttons[i];
                    break;
                }
            }
            if (button == null) return;
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.layer = 2;
            gameObject.transform.parent = Menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            float keybindShrink = (button.HasKeybinds && button.KeybindCategory != null) ? 0.0875f : 0f;
            gameObject.transform.localScale = new Vector3(SmFl, (MenuWidth - BtnInset) - (button.Incremental ? 0.175f : 0f) - keybindShrink, BtnHeight);
            gameObject.transform.localPosition = new Vector3(SmFl * 40f + SmFl, keybindShrink / 2f, BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
            GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
            BoxCollider component = gameObject.GetComponent<BoxCollider>();
            component.isTrigger = true;
            float brightness = (button.Enabled ? OnBrightness : OffBrightness);
            ColorSetter cs1 = gameObject.AddComponent<ColorSetter>();
            cs1.brightness = brightness;
            cs1.colorOffset = (BtnIndex * GradVal) - GradVal;
            cs1.buttonID = "Btn_" + name;
            if (!button.Label)
            {
                if (button.Incremental)
                {
                    GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gameObject2.layer = 2;
                    gameObject2.transform.parent = Menu.transform;
                    gameObject2.transform.rotation = Quaternion.identity;
                    gameObject2.transform.localScale = new Vector3(SmFl, 0.08f, BtnHeight);
                    gameObject2.transform.localPosition = new Vector3(SmFl * 40f + SmFl, ((MenuWidth - BtnInset) / 2f) - (gameObject2.transform.localScale.y / 2f), BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
                    ColorSetter cs2 = gameObject2.AddComponent<ColorSetter>();
                    cs2.brightness = OffBrightness;
                    cs2.colorOffset = (BtnIndex * GradVal) - GradVal;
                    cs2.buttonID = "IncDown_" + BtnIndex;
                    GameObject.Destroy(gameObject2.GetComponent<Rigidbody>());
                    GameObject gameObject3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gameObject3.layer = 2;
                    gameObject3.transform.parent = Menu.transform;
                    gameObject3.transform.rotation = Quaternion.identity;
                    gameObject3.transform.localScale = new Vector3(SmFl, 0.08f, BtnHeight);
                    gameObject3.transform.localPosition = new Vector3(SmFl * 40f + SmFl, -(((MenuWidth - BtnInset) / 2f) - (gameObject2.transform.localScale.y / 2f)), BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
                    ColorSetter cs3 = gameObject3.AddComponent<ColorSetter>();
                    cs3.brightness = OffBrightness;
                    cs3.colorOffset = (BtnIndex * GradVal) - GradVal;
                    cs3.buttonID = "IncUp_" + BtnIndex;
                    GameObject.Destroy(gameObject3.GetComponent<Rigidbody>());
                    AddText("-", TextSize, gameObject2.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
                    AddText("+", TextSize, gameObject3.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
                    BoxCollider component2 = gameObject2.GetComponent<BoxCollider>();
                    component2.isTrigger = true;
                    BoxCollider component3 = gameObject3.GetComponent<BoxCollider>();
                    component3.isTrigger = true;
                    IncrementalButtonCollider downCol = gameObject2.AddComponent<IncrementalButtonCollider>();
                    downCol.onClick = () => { LastClickedButtonID = cs2.buttonID; button.Down(); };
                    IncrementalButtonCollider upCol = gameObject3.AddComponent<IncrementalButtonCollider>();
                    upCol.onClick = () => { LastClickedButtonID = cs3.buttonID; button.Up(); };
                }
                if (button.HasKeybinds && button.KeybindCategory != null)
                {
                    float gearBtnWidth = 0.08f;
                    GameObject gearObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gearObj.layer = 2;
                    gearObj.transform.parent = Menu.transform;
                    gearObj.transform.rotation = Quaternion.identity;
                    gearObj.transform.localScale = new Vector3(SmFl, gearBtnWidth, BtnHeight);
                    gearObj.transform.localPosition = new Vector3(SmFl * 40f + SmFl, -(((MenuWidth - BtnInset) / 2f) - (gearBtnWidth / 2f)), BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
                    ColorSetter gearCs = gearObj.AddComponent<ColorSetter>();
                    gearCs.brightness = OffBrightness;
                    gearCs.colorOffset = (BtnIndex * GradVal) - GradVal;
                    gearCs.buttonID = "Gear_" + name;
                    GameObject.Destroy(gearObj.GetComponent<Rigidbody>());
                    BoxCollider gearBox = gearObj.GetComponent<BoxCollider>();
                    gearBox.isTrigger = true;
                    Category keybindCat = button.KeybindCategory;
                    ButtonCollider gearCol = gearObj.AddComponent<ButtonCollider>();
                    gearCol.onClick = () =>
                    {
                        LastClickedButtonID = gearCs.buttonID;
                        keybindCat.ParentCategory = ActiveCategory;
                        ActiveCategory = keybindCat;
                        CurrentPage = 0;
                        Core.ButtonCooldown = Time.time + 0.4f;
                    };
                    Texture2D gearTex = GetGearIconTexture();
                    if (gearTex != null)
                    {
                        AddImage(gearTex, TextSize * 1.35f, gearObj.transform.position + new Vector3(SmFl, 0f, SmFl - 0.005f), default(Vector3), Vector2.zero);
                    }
                    else
                    {
                        AddText("⚙", TextSize, gearObj.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
                    }
                }
                var DefaultCallback = () => { };
                if (button.OnEnable != DefaultCallback || button.OnDisable != DefaultCallback)
                {
                    ButtonCollider buttonCollider = gameObject.AddComponent<ButtonCollider>();
                    buttonCollider.onClick = () =>
                    {
                        LastClickedButtonID = cs1.buttonID;
                        if (button.Toggle)
                        {
                            button.Enabled = !button.Enabled;
                            if (button.Enabled)
                            {
                                button.OnceEnable();
                                button.OnEnable();
                            }
                            else
                            {
                                button.OnceDisable();
                                button.OnDisable();
                            }
                        }
                        else
                        {
                            button.OnEnable();
                        }
                        if (!BlockRebuild) RebuildMenu();
                        BlockRebuild = false;
                    };
                }
            }
            else
            {
                GameObject.Destroy(gameObject.GetComponent<Renderer>());
            }
            Vector3 textLocalPos = new Vector3(SmFl * 40f + SmFl, 0f, BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
            Vector3 textWorldPos = Menu.transform.TransformPoint(textLocalPos);
            AddText(name, TextSize, textWorldPos + new Vector3(SmFl, 0f, SmFl), default(Vector3));
            BtnIndex++;
        }
        public class ScaleInAnimation : MonoBehaviour
        {
            [Header("Settings")]
            [SerializeField] public bool reverse = false;
            [SerializeField] public float duration = 0.4f;
            [SerializeField] public System.Action onComplete;
            private Vector3 startScale;
            private Vector3 targetScale;
            private float elapsed;
            private bool initialized;
            private void Start() { Initialize(); }
            private void Initialize()
            {
                if (initialized) return;
                if (!reverse)
                {
                    targetScale = transform.localScale;
                    startScale = Vector3.zero;
                    transform.localScale = startScale;
                }
                else
                {
                    startScale = transform.localScale;
                    targetScale = Vector3.zero;
                }
                elapsed = 0f;
                initialized = true;
            }
            private void Update()
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = EaseInOutCubic(t);
                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, easedT);
                if (t >= 1f)
                {
                    transform.localScale = targetScale;
                    onComplete?.Invoke();
                    if (reverse) Destroy(gameObject);
                    else Destroy(this);
                }
            }
            private float EaseInOutCubic(float t)
            {
                return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
            }
        }
        public static Vector3 ParentScale(Vector3 baseVector)
        {
            return new Vector3(
                (baseVector.x * Menu.transform.localScale.x) * (1f / Menu.transform.localScale.x),
                (baseVector.y * Menu.transform.localScale.y) * (1f / Menu.transform.localScale.y),
                (baseVector.z * Menu.transform.localScale.z) * (1f / Menu.transform.localScale.z)
            );
        }
        public class RoundedCorners : MonoBehaviour
        {
            [Range(0f, 0.5f)] public float bevel = 0.04f;
            public bool topLeft = true;
            public bool topRight = true;
            public bool bottomLeft = true;
            public bool bottomRight = true;
            public float multX = 0f;
            public float multY = 0f;
            public float bevelX = 0f;
            public float bevelY = 0f;
            private Renderer sourceRenderer;
            private GradientSetter gradientSetter;
            private ColorSetter colorSetter;
            void Start()
            {
                sourceRenderer = GetComponent<Renderer>();
                if (!sourceRenderer) return;
                gradientSetter = GetComponent<GradientSetter>();
                colorSetter = GetComponent<ColorSetter>();
                float sx = Mathf.Max(transform.localScale.y, 0.001f);
                float sy = Mathf.Max(transform.localScale.z, 0.001f);
                multX = (1f / sx) * (1f + Mathf.Log(sx + 1f));
                multY = (1f / sy) * (1f + Mathf.Log(sy + 1f));
                bevelX = bevel * multX;
                bevelY = bevel * multY;
                CreateGeometry();
                sourceRenderer.enabled = false;
            }
            void CreateGeometry()
            {
                Transform parent = transform;
                CreateCube(parent, Vector3.zero, new Vector3(1f, 1f - bevelX * 2f, 1f), false, -1);
                CreateCube(parent, Vector3.zero, new Vector3(1f, 1f, 1f - bevelY * 2f), false, -1);
                bool[] enabled = { topLeft, bottomLeft, topRight, bottomRight };
                Vector3[] offsets =
                {
                    new Vector3(0f, -0.5f + bevelX, -0.5f + bevelY),
                    new Vector3(0f, 0.5f - bevelX, -0.5f + bevelY),
                    new Vector3(0f, -0.5f + bevelX, 0.5f - bevelY),
                    new Vector3(0f, 0.5f - bevelX, 0.5f - bevelY)
                };
                for (int i = 0; i < 4; i++)
                {
                    bool isTop = (i == 2 || i == 3);
                    if (enabled[i])
                    {
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        Destroy(c.GetComponent<Collider>());
                        c.transform.SetParent(parent, false);
                        c.transform.localRotation = Quaternion.Euler(0, 0, 90);
                        c.transform.localScale = new Vector3(bevelX * 2f, 0.5f, bevelY * 2f);
                        c.transform.localPosition = offsets[i];
                        ConfigureRenderer(c.GetComponent<Renderer>(), true, isTop ? 0 : 1);
                    }
                    else
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Destroy(cube.GetComponent<Collider>());
                        cube.transform.SetParent(parent, false);
                        cube.transform.localScale = new Vector3(1f, bevelX * 2f, bevelY * 2f);
                        cube.transform.localPosition = offsets[i];
                        ConfigureRenderer(cube.GetComponent<Renderer>(), true, isTop ? 0 : 1);
                    }
                }
            }
            void CreateCube(Transform parent, Vector3 pos, Vector3 scale, bool isCorner, int cornerType)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(g.GetComponent<Collider>());
                g.transform.SetParent(parent, false);
                g.transform.localPosition = pos;
                g.transform.localScale = scale;
                ConfigureRenderer(g.GetComponent<Renderer>(), isCorner, cornerType);
            }
            void ConfigureRenderer(Renderer r, bool isCorner, int cornerType)
            {
                Material oldMaterial = r.material;
                if (oldMaterial != null) Destroy(oldMaterial);
                if (gradientSetter != null)
                {
                    GradientSetter gs = r.gameObject.AddComponent<GradientSetter>();
                    gs.brightness = gradientSetter.brightness;
                    gs.isVertical = gradientSetter.isVertical;
                    if (isCorner)
                    {
                        float bevelOffset = bevel * gradientSetter.gradientOffset;
                        if (cornerType == 0)
                        {
                            gs.startOffset = gradientSetter.startOffset;
                            gs.gradientOffset = bevelOffset;
                        }
                        else
                        {
                            gs.startOffset = gradientSetter.startOffset + gradientSetter.gradientOffset - bevelOffset;
                            gs.gradientOffset = bevelOffset;
                        }
                    }
                    else
                    {
                        gs.gradientOffset = gradientSetter.gradientOffset;
                        gs.startOffset = gradientSetter.startOffset;
                    }
                }
                else if (colorSetter != null)
                {
                    ColorSetter cs = r.gameObject.AddComponent<ColorSetter>();
                    cs.brightness = colorSetter.brightness;
                    cs.colorOffset = colorSetter.colorOffset;
                }
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }
        }
        public class GradientSetter : MonoBehaviour
        {
            [Header("Color Settings")]
            [SerializeField, Range(0f, 2f)] public float brightness = 1f;
            [SerializeField] public bool isVertical = false;
            [SerializeField, Range(0f, 10f)] public float gradientOffset = 1f;
            [SerializeField, Range(0f, 10f)] public float startOffset = 0f;
            public string buttonID = "";
            private Vector3 targetScale;
            private Renderer rend;
            public Material cachedMaterial;
            private Texture2D gradientTexture;
            private Color[] pixels;
            private Color lastColor1;
            private Color lastColor2;
            private bool needsUpdate = true;
            private float updateTimer = 0f;
            private const float updateInterval = 0.033f;
            private bool initialized = false;
            private bool isCylinder = false;
            public bool isAura = false;
            private void Start()
            {
                rend = GetComponent<Renderer>();
                if (rend == null) return;
                isCylinder = GetComponent<MeshFilter>()?.sharedMesh.name.Contains("Cylinder") ?? false;
                MeshFilter mf = GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    Mesh m = Instantiate(mf.sharedMesh);
                    Vector3[] verts = m.vertices;
                    Vector2[] uvs = m.uv;
                    Vector3 min = m.bounds.min;
                    Vector3 size = m.bounds.size;
                    for (int i = 0; i < verts.Length; i++)
                    {
                        float u = size.x > 0.001f ? (verts[i].x - min.x) / size.x : 0f;
                        float v = size.y > 0.001f ? (verts[i].y - min.y) / size.y : 0f;
                        float z = size.z > 0.001f ? (verts[i].z - min.z) / size.z : 0f; 
                        if (isCylinder)
                        {
                            uvs[i] = new Vector2(u, z);
                        }
                        else
                        {
                            uvs[i] = new Vector2(z, v);
                        }
                    }
                    m.uv = uvs;
                    mf.mesh = m;
                }
                if (isAura)
                {
                    Shader transparentShader = Shader.Find("Sprites/Default");
                    if (transparentShader == null) transparentShader = Shader.Find("Universal Render Pipeline/Unlit");
                    cachedMaterial = new Material(transparentShader);
                }
                else
                {
                    cachedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                }
                rend.material = cachedMaterial;
                CreateGradientTexture();
                initialized = true;
                targetScale = transform.localScale;
                if (!string.IsNullOrEmpty(buttonID) && Core.IsAnimated && buttonID == Core.LastClickedButtonID)
                {
                    transform.localScale = targetScale * 0.8f;
                }
                lastColor1 = GetOffsetColor(startOffset) * brightness;
                lastColor2 = GetOffsetColor(startOffset + gradientOffset) * brightness;
                UpdateGradientTexture();
            }
            private void Update()
            {
                if (gameObject == Core.BoardGradientObject)
                    Core.BoardMat = cachedMaterial;
                if (!initialized || !isActiveAndEnabled) return;
                if (rend != null && rend.material != cachedMaterial)
                    rend.material = cachedMaterial;

                if (!string.IsNullOrEmpty(buttonID))
                {
                    if (Core.IsAnimated && buttonID == Core.LastClickedButtonID)
                        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 12f);
                    else if (transform.localScale != targetScale)
                        transform.localScale = targetScale;
                }

                updateTimer += Time.deltaTime;
                if (updateTimer >= updateInterval)
                {
                    updateTimer = 0f;
                    Color color1 = GetOffsetColor(startOffset) * brightness;
                    Color color2 = GetOffsetColor(startOffset + gradientOffset) * brightness;
                    if (Vector4.Distance(lastColor1, color1) > 0.02f || Vector4.Distance(lastColor2, color2) > 0.02f)
                    {
                        lastColor1 = color1;
                        lastColor2 = color2;
                        needsUpdate = true;
                    }
                    if (needsUpdate)
                    {
                        UpdateGradientTexture();
                        needsUpdate = false;
                    }
                }
            }
            private void CreateGradientTexture()
            {
                int w = (isCylinder || isVertical) ? 1 : 64;
                int h = (isCylinder || isVertical) ? 64 : 1;
                gradientTexture = new Texture2D(w, h, TextureFormat.RGB24, false);
                gradientTexture.filterMode = FilterMode.Bilinear;
                gradientTexture.wrapMode = isCylinder ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                pixels = new Color[w * h];
                cachedMaterial.color = Color.white;
                cachedMaterial.mainTexture = gradientTexture;
            }
            private Color GetOffsetColor(float timeOffsetSeconds)
            {
                Theme currentTheme = Themes.List[Core.ThemeValue];
                if (currentTheme.Colors == null || currentTheme.Colors.Length == 0)
                    return Color.white;
                if (currentTheme.Colors.Length == 1)
                    return currentTheme.Colors[0];
                float totalRange = currentTheme.Colors.Length - 1;
                float t = Mathf.PingPong((Time.time + timeOffsetSeconds) * currentTheme.Speed, totalRange);
                int indexA = Mathf.FloorToInt(t);
                int indexB = Mathf.Clamp(indexA + 1, 0, currentTheme.Colors.Length - 1);
                float localT = t - indexA;
                float easedT = localT < 0.5f
                    ? 2f * localT * localT
                    : 1f - Mathf.Pow(-2f * localT + 2f, 2f) / 2f;
                return Color.Lerp(currentTheme.Colors[indexA], currentTheme.Colors[indexB], easedT);
            }
            private void UpdateGradientTexture()
            {
                if (gradientTexture == null) return;
                Color color1 = lastColor2;
                Color color2 = lastColor1;
                
                color1.a = isAura ? 0.35f : 1f;
                color2.a = isAura ? 0.35f : 1f;

                int w = gradientTexture.width;
                int h = gradientTexture.height;
                int index = 0;
                if (isCylinder || isVertical)
                {
                    for (int y = 0; y < h; y++)
                    {
                        float t = (float)y / Mathf.Max(1, h - 1);
                        pixels[index++] = Color.Lerp(color1, color2, t);
                    }
                }
                else
                {
                    for (int x = 0; x < w; x++)
                    {
                        float t = (float)x / Mathf.Max(1, w - 1);
                        pixels[index++] = Color.Lerp(color1, color2, t);
                    }
                }
                gradientTexture.SetPixels(pixels);
                gradientTexture.Apply(false);
            }
            public void SetBrightness(float value)
            {
                brightness = Mathf.Max(0f, value);
                needsUpdate = true;
            }
            private void OnDestroy()
            {
                if (gradientTexture != null) Destroy(gradientTexture);
                if (cachedMaterial != null) Destroy(cachedMaterial);
            }
        }
        public class ColorSetter : MonoBehaviour
        {
            [Header("Color Settings")]
            [SerializeField, Range(0f, 1f)] public float brightness = 1f;
            [SerializeField, Range(0f, 10f)] public float colorOffset = 0f;
            public string buttonID = "";
            private Renderer rend;
            private Material instanceMaterial;
            private Color lastAppliedColor;
            private Vector3 targetScale;
            private float updateTimer = 0f;
            private const float updateInterval = 0.033f;
            private void Start()
            {
                rend = GetComponent<Renderer>();
                if (rend == null) return;
                instanceMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                instanceMaterial.SetFloat("_Surface", 1);
                instanceMaterial.SetFloat("_Blend", 0);
                instanceMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                instanceMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                instanceMaterial.SetFloat("_ZWrite", 0);
                instanceMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                instanceMaterial.renderQueue = (int)RenderQueue.Transparent;
                rend.material = instanceMaterial;

                targetScale = transform.localScale;

                if (Core.IsAnimated && buttonID == Core.LastClickedButtonID && !string.IsNullOrEmpty(buttonID))
                {
                    float oldBrightness = Mathf.Approximately(brightness, Core.OnBrightness) ? Core.OffBrightness : Core.OnBrightness;
                    lastAppliedColor = new Color(0f, 0f, 0f, Mathf.Clamp01(1f - oldBrightness));
                    transform.localScale = targetScale * 0.8f; 
                }
                else
                {
                    lastAppliedColor = new Color(0f, 0f, 0f, Mathf.Clamp01(1f - brightness));
                }
                instanceMaterial.color = lastAppliedColor;
            }
            private void Update()
            {
                if (rend == null || instanceMaterial == null || !isActiveAndEnabled) return;
                Color targetColor = new Color(0f, 0f, 0f, Mathf.Clamp01(1f - brightness));

                if (Core.IsAnimated && buttonID == Core.LastClickedButtonID && !string.IsNullOrEmpty(buttonID))
                {
                    lastAppliedColor = Color.Lerp(lastAppliedColor, targetColor, Time.deltaTime * 12f);
                    transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 12f);
                    instanceMaterial.color = lastAppliedColor;
                }
                else
                {
                    updateTimer += Time.deltaTime;
                    if (updateTimer >= updateInterval)
                    {
                        updateTimer = 0f;
                        if (Mathf.Abs(lastAppliedColor.a - targetColor.a) > 0.02f)
                        {
                            lastAppliedColor = targetColor;
                            instanceMaterial.color = targetColor;
                            transform.localScale = targetScale;
                        }
                    }
                }
            }
            public void SetBrightness(float value)
            {
                brightness = Mathf.Clamp01(value);
            }
            private void OnDestroy()
            {
                if (instanceMaterial != null) Destroy(instanceMaterial);
            }
        }
        
        public class ParticleThemeUpdater : MonoBehaviour
        {
            private UnityEngine.ParticleSystem ps;
            private void Start()
            {
                ps = GetComponent<UnityEngine.ParticleSystem>();
            }
            private void Update()
            {
                if (ps == null) return;
                Theme currentTheme = Themes.List[Core.ThemeValue];
                if (currentTheme.Colors != null && currentTheme.Colors.Length > 0)
                {
                    var main = ps.main;
                    main.startColor = currentTheme.Colors[0];
                }
            }
        }
        
        public class PopoutAnimation : MonoBehaviour
        {
            public Vector3 originPosition;
            public Vector3 targetPosition;
            public Quaternion originRotation;
            public Quaternion targetRotation;
            public bool doScale = true;
        
            public float speed = 1.5f;
            private float progress = 0f;
            private Vector3 initialScale;
        
            void Start()
            {
                initialScale = transform.localScale;
                transform.position = originPosition;
                transform.rotation = originRotation;
                if (doScale) transform.localScale = initialScale * 0.1f; 
            }
        
            void Update()
            {
                progress += Time.deltaTime * speed;
                float t = Mathf.Clamp01(progress);
                float easeT = 1f - Mathf.Pow(1f - t, 3f);
                
                transform.position = Vector3.Lerp(originPosition, targetPosition, easeT);
                transform.rotation = Quaternion.Slerp(originRotation, targetRotation, easeT);
                if (doScale) transform.localScale = Vector3.Lerp(initialScale * 0.1f, initialScale, easeT);
                
                if (progress >= 1f)
                {
                    transform.position = targetPosition;
                    transform.rotation = targetRotation;
                    transform.localScale = initialScale;
                    Destroy(this);
                }
            }
        }
    }
}






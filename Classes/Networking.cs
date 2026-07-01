using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Juul
{
    public static class JUUL
    {
        public static bool IsNetworkingEnabled = true;

        private static float syncTimer = 0f;
        private static Dictionary<Player, NetworkedMenu> networkMenus = new Dictionary<Player, NetworkedMenu>();
        private static Dictionary<Player, NetworkedKeyboard> networkKeyboards = new Dictionary<Player, NetworkedKeyboard>();
        private static Dictionary<Player, GameObject> playerNameTags = new Dictionary<Player, GameObject>();
        private static string lastButtonStatesHash = "";

        static JUUL()
        {
            GameObject go = new GameObject("JUULNetwork");
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<JUULInitializer>();

        }

        public static void ToggleNetworking(bool state)
        {
            IsNetworkingEnabled = state;
            if (!state && PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
            {
                ClearAllJuulProperties();
                lastButtonStatesHash = "";
            }
        }

        public static void ClearAllJuulProperties()
        {
            if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null) return;

            string[] keysToRemove = new string[]
            {
                "Juul_V", "Juul_T", "Juul_P", "Juul_O", "Juul_H", "Juul_R",
                "Juul_CL", "Juul_CR", "Juul_MW", "Juul_BI", "Juul_TS", "Juul_VR",
                "Juul_BTN", "Juul_KB", "Juul_KBQ", "Juul_SRCH", "Juul_SRCHQ",
                "Juul_CATPG", "Juul_PG", "Juul_PGV", "Juul_CATS", "Juul_PBTNS"
            };

            Hashtable currentProps = PhotonNetwork.LocalPlayer.CustomProperties;
            Hashtable propsToRemove = new Hashtable();
            foreach (string key in keysToRemove)
            {
                if (currentProps.ContainsKey(key))
                    propsToRemove[key] = null;
            }
            if (propsToRemove.Count > 0)
                PhotonNetwork.LocalPlayer.SetCustomProperties(propsToRemove);

            foreach (string key in keysToRemove)
                currentProps.Remove(key);
        }

        private static string GetButtonStates()
        {
            if (Core.ActiveButtons == null || Core.ActiveButtons.Count == 0) return "";

            List<string> enabled = new List<string>();
            foreach (var btn in Core.ActiveButtons)
            {
                if (btn != null && btn.Enabled && !string.IsNullOrEmpty(btn.Name))
                {
                    string name = btn.Name.Replace("|", "").Replace(":", "");
                    if (name.Length > 30) name = name.Substring(0, 30);
                    enabled.Add(name);
                }
            }
            return enabled.Count == 0 ? "" : string.Join("|", enabled.ToArray());
        }

        public static void SyncProperties()
        {
            if (!IsNetworkingEnabled || !PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null) return;

            string category = Core.ActiveCategory != null ? Core.ActiveCategory.Name : "Home";
            if (category.Length > 20) category = category.Substring(0, 20);

            Hashtable props = new Hashtable();
            props["Juul_V"] = Plugin.version;
            props["Juul_T"] = Core.ThemeValue;
            props["Juul_P"] = category;
            props["Juul_O"] = Core.IsMenuOpen;
            props["Juul_H"] = Core.IsRightHanded;
            props["Juul_R"] = Core.IsRounded;
            props["Juul_CL"] = Core.IsCatLeft;
            props["Juul_CR"] = Core.IsCatRotated;
            props["Juul_MW"] = Core.MenuWidth;
            props["Juul_BI"] = Core.BtnInset;
            props["Juul_TS"] = Core.TextSize;
            props["Juul_VR"] = UnityEngine.XR.XRSettings.isDeviceActive;
            props["Juul_SRCH"] = SearchManager.IsSearching;
            props["Juul_SRCHQ"] = SearchManager.IsSearching ? SearchManager.SearchQuery : "";
            props["Juul_CATPG"] = Core.CurrentCatPage;
            props["Juul_PG"] = Core.CurrentPage;
            props["Juul_PGV"] = Core.PageBtnVer;

            string buttonStates = GetButtonStates();
            if (buttonStates != lastButtonStatesHash)
            {
                props["Juul_BTN"] = buttonStates;
                lastButtonStatesHash = buttonStates;
            }

            props["Juul_CATS"] = GetCategories();
            props["Juul_PBTNS"] = GetCurrentPageButtons();

            bool kbVisible = KeyboardManager.KeyboardObj != null && !KeyboardManager.KeyboardObj.Equals(null);
            props["Juul_KB"] = kbVisible;
            string kbQuery = "";
            if (kbVisible)
            {
                if (KeyboardManager.IsSavingPreset) kbQuery = KeyboardManager.PresetSaveQuery ?? "";
                else if (KeyboardManager.IsJoiningRoom) kbQuery = KeyboardManager.JoinRoomQuery ?? "";
                else if (KeyboardManager.IsSettingName) kbQuery = KeyboardManager.NameQuery ?? "";
                else kbQuery = SearchManager.SearchQuery ?? "";
                if (kbQuery.Length > 64) kbQuery = kbQuery.Substring(0, 64);
            }
            props["Juul_KBQ"] = kbQuery;

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        private static string GetCategories()
        {
            if (Buttons.Modules == null) return "";

            List<string> categoryNames = new List<string>();
            int startCat = Core.CurrentCatPage * Core.MaxCatsPerPage;
            int endCat = Mathf.Min(startCat + Core.MaxCatsPerPage, Buttons.Modules.Length);

            for (int i = startCat; i < endCat; i++)
            {
                string catName = Buttons.Modules[i].Name;
                if (catName.Length > 15) catName = catName.Substring(0, 15);
                categoryNames.Add(catName.Replace("|", ""));
            }

            return string.Join("|", categoryNames.ToArray());
        }

        private static string GetCurrentPageButtons()
        {
            if (Core.ActiveCategory == null) return "";

            List<string> buttonData = new List<string>();

            bool hasParent = Core.ActiveCategory.ParentCategory != null;
            int totalItems = (hasParent ? 1 : 0) + Core.ActiveCategory.Buttons.Count + Core.ActiveCategory.Subcategories.Count;
            int buttonLimit = Core.BtnCount();
            int startIndex = Core.CurrentPage * Core.BtnCount();
            int endIndex = Mathf.Min(startIndex + buttonLimit, totalItems);

            for (int i = startIndex; i < endIndex; i++)
            {
                int currentPos = i;
                if (hasParent)
                {
                    if (currentPos == 0)
                    {
                        buttonData.Add("<<Back:0:0:0");
                        continue;
                    }
                    currentPos--;
                }

                if (currentPos < Core.ActiveCategory.Buttons.Count)
                {
                    Button btn = Core.ActiveCategory.Buttons[currentPos];
                    string btnName = btn.Name;
                    if (btnName.Length > 25) btnName = btnName.Substring(0, 25);
                    btnName = btnName.Replace("|", "").Replace(":", "");
                    int hasInc = btn.Incremental ? 1 : 0;
                    int hasKey = (btn.HasKeybinds && btn.KeybindCategory != null) ? 1 : 0;
                    int isLabel = btn.Label ? 1 : 0;
                    buttonData.Add($"{btnName}:{hasInc}:{hasKey}:{isLabel}");
                }
                else
                {
                    int subIndex = currentPos - Core.ActiveCategory.Buttons.Count;
                    if (subIndex < Core.ActiveCategory.Subcategories.Count)
                    {
                        string subName = Core.ActiveCategory.Subcategories[subIndex].Name;
                        if (subName.Length > 25) subName = subName.Substring(0, 25);
                        subName = subName.Replace("|", "").Replace(":", "");
                        buttonData.Add($"{subName}:0:0:0");
                    }
                }
            }

            while (buttonData.Count < Core.BtnCount()) buttonData.Add(":0:0:0");
            if (Core.PageBtnVer == 1)
            {
                buttonData.Add("<<<<<<:0:0:0");
                buttonData.Add(">>>>>>:0:0:0");
            }
            else if (Core.PageBtnVer == 2)
            {
                buttonData.Insert(0, ">>>>>>:0:0:0");
                buttonData.Insert(0, "<<<<<<:0:0:0");
            }

            return "PGV" + Core.PageBtnVer + "|" + string.Join("|", buttonData.ToArray());
        }

        public static bool HasJUULProperty(Player player)
        {
            if (player == null || !player.CustomProperties.ContainsKey("Juul_V"))
                return false;

            if (!player.CustomProperties.ContainsKey("Juul_O") || 
                !player.CustomProperties.ContainsKey("Juul_T"))
                return false;

            return true;
        }

        public static void UpdatePlayerNamesAndMenus()
        {
            if (VRRigCache.ActiveRigs == null) return;

            List<Player> currentPlayers = new List<Player>();

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isOfflineVRRig || rig.isMyPlayer) continue;
                if (rig.netView == null) continue;

                Player player = null;
                try
                {
                    PhotonView photonView = Rigs.GetPhotonViewFromVRRig(rig);
                    if (photonView != null && photonView.Owner != null)
                        player = photonView.Owner;
                }
                catch { }

                if (player == null) continue;
                currentPlayers.Add(player);

                if (HasJUULProperty(player))
                {
                    UpdateOrCreateNameTag(player, rig);
                    bool isMenuOpen = player.CustomProperties.ContainsKey("Juul_O") && (bool)player.CustomProperties["Juul_O"];

                    if (isMenuOpen && IsNetworkingEnabled)
                    {
                        UpdateNetworkMenu(player, rig);
                        UpdateNetworkKeyboard(player);
                    }
                    else
                    {
                        RemoveNetworkKeyboard(player);
                        RemoveNetworkMenu(player);
                    }
                }
                else
                {
                    RemoveNetworkKeyboard(player);
                    RemoveNetworkMenu(player);
                    RemoveNameTag(player);
                }
            }

            List<Player> toRemove = new List<Player>();
            foreach (var kvp in networkMenus)
            {
                if (!currentPlayers.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);
            }
            foreach (Player p in toRemove)
            {
                RemoveNetworkKeyboard(p);
                RemoveNetworkMenu(p);
                RemoveNameTag(p);
            }
        }

        private static void UpdateNetworkKeyboard(Player player)
        {
            bool kbVisible = player.CustomProperties.ContainsKey("Juul_KB") && (bool)player.CustomProperties["Juul_KB"];
            string kbQuery = player.CustomProperties.ContainsKey("Juul_KBQ") ? player.CustomProperties["Juul_KBQ"] as string ?? "" : "";

            if (!kbVisible)
            {
                RemoveNetworkKeyboard(player);
                return;
            }

            if (!networkMenus.ContainsKey(player) || networkMenus[player] == null || networkMenus[player].Root == null) return;

            int themeIndex = player.CustomProperties.ContainsKey("Juul_T") ? (int)player.CustomProperties["Juul_T"] : 0;
            bool isRounded = player.CustomProperties.ContainsKey("Juul_R") && (bool)player.CustomProperties["Juul_R"];

            if (networkKeyboards.ContainsKey(player) && networkKeyboards[player] != null && networkKeyboards[player].IsClosing)
            {
                networkKeyboards.Remove(player);
            }

            if (!networkKeyboards.ContainsKey(player) || networkKeyboards[player] == null)
            {
                networkKeyboards[player] = new NetworkedKeyboard(networkMenus[player].Root, themeIndex, isRounded);
            }
            networkKeyboards[player].UpdateState(themeIndex, kbQuery);
        }

        private static void RemoveNetworkKeyboard(Player player)
        {
            if (networkKeyboards.ContainsKey(player))
            {
                NetworkedKeyboard kb = networkKeyboards[player];
                if (kb == null)
                {
                    networkKeyboards.Remove(player);
                    return;
                }
                if (!kb.IsClosing)
                {
                    Player capturedPlayer = player;
                    kb.StartClose(() =>
                    {
                        if (networkKeyboards.ContainsKey(capturedPlayer) && networkKeyboards[capturedPlayer] == kb)
                            networkKeyboards.Remove(capturedPlayer);
                    });
                }
            }
        }

        private static void UpdateOrCreateNameTag(Player player, VRRig rig)
        {
            try
            {
                Color themeColor = Color.white;
                if (player.CustomProperties.ContainsKey("Juul_T"))
                {
                    int themeIndex = (int)player.CustomProperties["Juul_T"];
                    if (themeIndex >= 0 && themeIndex < Themes.List.Length)
                        themeColor = Themes.List[themeIndex].Color;
                }

                string userVersion = "";
                if (player.CustomProperties.ContainsKey("Juul_V"))
                {
                    string ver = player.CustomProperties["Juul_V"] as string;
                    if (!string.IsNullOrEmpty(ver))
                        userVersion = " " + ver;
                }

                if (!playerNameTags.ContainsKey(player) || playerNameTags[player] == null)
                {
                    GameObject tag = new GameObject("JuulNameTag");
                    tag.transform.SetParent(rig.transform, false);
                    tag.transform.localPosition = new Vector3(0f, 0.4f, 0f);

                    TextMesh text = tag.AddComponent<TextMesh>();
                    text.text = "[Juul User" + userVersion + "]";
                    text.fontSize = 80;
                    text.characterSize = 0.008f;
                    text.anchor = TextAnchor.MiddleCenter;
                    text.alignment = TextAlignment.Center;
                    text.color = themeColor;
                    text.fontStyle = FontStyle.Bold;

                    tag.AddComponent<Billboard>();
                    playerNameTags[player] = tag;
                }
                else
                {
                    TextMesh text = playerNameTags[player].GetComponent<TextMesh>();
                    if (text != null)
                    {
                        text.color = themeColor;
                        text.text = "[Juul User" + userVersion + "]";
                    }
                }
            }
            catch { }
        }

        private static void RemoveNameTag(Player player)
        {
            if (playerNameTags.ContainsKey(player))
            {
                if (playerNameTags[player] != null) GameObject.Destroy(playerNameTags[player]);
                playerNameTags.Remove(player);
            }
        }

        private static void UpdateNetworkMenu(Player player, VRRig rig)
        {
            if (networkMenus.ContainsKey(player) && networkMenus[player] != null && networkMenus[player].IsClosing)
            {
                networkMenus.Remove(player);
            }
            if (!networkMenus.ContainsKey(player))
            {
                NetworkedMenu menu = new NetworkedMenu(player, rig);
                networkMenus[player] = menu;
            }
            else
            {
                networkMenus[player].Update(player, rig);
            }
        }

        private static void RemoveNetworkMenu(Player player)
        {
            if (networkMenus.ContainsKey(player))
            {
                NetworkedMenu menu = networkMenus[player];
                if (menu == null)
                {
                    networkMenus.Remove(player);
                    return;
                }
                if (!menu.IsClosing)
                {
                    Player capturedPlayer = player;
                    menu.StartClose(() =>
                    {
                        if (networkMenus.ContainsKey(capturedPlayer) && networkMenus[capturedPlayer] == menu)
                            networkMenus.Remove(capturedPlayer);
                    });
                }
            }
        }

        public class Billboard : MonoBehaviour
        {
            void Update()
            {
                if (Camera.main != null)
                    transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }
        }

        private class JUULInitializer : MonoBehaviour
        {
            private void Update()
            {
                if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
                {
                    syncTimer += Time.deltaTime;
                    if (syncTimer >= 0.25f)
                    {
                        syncTimer = 0f;
                        JUUL.SyncProperties();
                    }
                    JUUL.UpdatePlayerNamesAndMenus();
                }
            }
        }
    }

    public class NetworkedGradientUpdater : MonoBehaviour
    {
        public int themeIndex;
        public float brightness = 1f;
        public bool isVertical = false;
        public float gradientOffset = 1f;
        public float startOffset = 0f;
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

        private void Start()
        {
            rend = GetComponent<Renderer>();
            if (rend == null) return;
            isCylinder = GetComponent<MeshFilter>()?.sharedMesh?.name?.Contains("Cylinder") ?? false;
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
                    uvs[i] = isCylinder ? new Vector2(u, z) : new Vector2(z, v);
                }
                m.uv = uvs;
                mf.mesh = m;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            cachedMaterial = new Material(shader);
            cachedMaterial.color = Color.white;
            rend.material = cachedMaterial;

            CreateGradientTexture();
            initialized = true;
            lastColor1 = GetOffsetColor(startOffset) * brightness;
            lastColor2 = GetOffsetColor(startOffset + gradientOffset) * brightness;
            UpdateGradientTexture();
        }

        private void Update()
        {
            if (!initialized || !isActiveAndEnabled) return;
            if (rend != null && rend.material != cachedMaterial)
                rend.material = cachedMaterial;

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
            if (themeIndex < 0 || themeIndex >= Themes.List.Length) return Color.white;
            Theme theme = Themes.List[themeIndex];
            if (theme.Colors == null || theme.Colors.Length == 0) return Color.white;
            if (theme.Colors.Length == 1) return theme.Colors[0];

            float totalRange = theme.Colors.Length - 1;
            float t = Mathf.PingPong((Time.time + timeOffsetSeconds) * theme.Speed, totalRange);
            int indexA = Mathf.FloorToInt(t);
            int indexB = Mathf.Clamp(indexA + 1, 0, theme.Colors.Length - 1);
            float localT = t - indexA;
            float easedT = localT < 0.5f ? 2f * localT * localT : 1f - Mathf.Pow(-2f * localT + 2f, 2f) / 2f;
            return Color.Lerp(theme.Colors[indexA], theme.Colors[indexB], easedT);
        }

        private void UpdateGradientTexture()
        {
            if (gradientTexture == null) return;
            Color color1 = lastColor2;
            Color color2 = lastColor1;
            color1.a = 1f; color2.a = 1f;

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

        private void OnDestroy()
        {
            if (gradientTexture != null) Destroy(gradientTexture);
            if (cachedMaterial != null) Destroy(cachedMaterial);
        }
    }

    public class NetworkedRoundedCorners : MonoBehaviour
    {
        public float bevel = 0.04f;
        public bool topLeft = true;
        public bool topRight = true;
        public bool bottomLeft = true;
        public bool bottomRight = true;
        private Renderer sourceRenderer;
        private NetworkedGradientUpdater gradientSetter;

        void Start()
        {
            sourceRenderer = GetComponent<Renderer>();
            if (!sourceRenderer) return;
            gradientSetter = GetComponent<NetworkedGradientUpdater>();
            float sx = Mathf.Max(transform.localScale.y, 0.001f);
            float sy = Mathf.Max(transform.localScale.z, 0.001f);
            float multX = (1f / sx) * (1f + Mathf.Log(sx + 1f));
            float multY = (1f / sy) * (1f + Mathf.Log(sy + 1f));
            float bevelX = bevel * multX;
            float bevelY = bevel * multY;
            CreateGeometry(bevelX, bevelY);
            sourceRenderer.enabled = false;
        }

        void CreateGeometry(float bevelX, float bevelY)
        {
            Transform parent = transform;
            CreateCube(parent, Vector3.zero, new Vector3(1f, 1f - bevelX * 2f, 1f), false, -1, bevelX, bevelY);
            CreateCube(parent, Vector3.zero, new Vector3(1f, 1f, 1f - bevelY * 2f), false, -1, bevelX, bevelY);
            bool[] enabled = { topLeft, bottomLeft, topRight, bottomRight };
            Vector3[] offsets =
            {
                new Vector3(0f, -0.5f + bevelX, -0.5f + bevelY),
                new Vector3(0f,  0.5f - bevelX, -0.5f + bevelY),
                new Vector3(0f, -0.5f + bevelX,  0.5f - bevelY),
                new Vector3(0f,  0.5f - bevelX,  0.5f - bevelY)
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
                    ConfigureRenderer(c.GetComponent<Renderer>(), true, isTop ? 0 : 1, bevelX, bevelY);
                }
                else
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(cube.GetComponent<Collider>());
                    cube.transform.SetParent(parent, false);
                    cube.transform.localScale = new Vector3(1f, bevelX * 2f, bevelY * 2f);
                    cube.transform.localPosition = offsets[i];
                    ConfigureRenderer(cube.GetComponent<Renderer>(), true, isTop ? 0 : 1, bevelX, bevelY);
                }
            }
        }

        void CreateCube(Transform parent, Vector3 pos, Vector3 scale, bool isCorner, int cornerType, float bevelX, float bevelY)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(g.GetComponent<Collider>());
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            ConfigureRenderer(g.GetComponent<Renderer>(), isCorner, cornerType, bevelX, bevelY);
        }

        void ConfigureRenderer(Renderer r, bool isCorner, int cornerType, float bevelX, float bevelY)
        {
            Material oldMaterial = r.material;
            if (oldMaterial != null) Destroy(oldMaterial);
            if (gradientSetter != null)
            {
                NetworkedGradientUpdater gs = r.gameObject.AddComponent<NetworkedGradientUpdater>();
                gs.themeIndex = gradientSetter.themeIndex;
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
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
    }

    public class NetworkedMenu
    {
        private GameObject menuRoot;
        private GameObject canvas;
        private Text titleText;
        private int themeIndex;
        private Player player;
        private bool isRounded;
        private bool isCatLeft;
        private bool isCatRotated;
        private float menuWidth;
        private float btnInset;
        private float textSize;
        private bool isVR;
        private Dictionary<string, GameObject> uiElements = new Dictionary<string, GameObject>();
        private Dictionary<string, Component> textElements = new Dictionary<string, Component>();
        private bool wasKbVisible = false;
        private Vector3 kbFixedPosition;
        private Quaternion kbFixedRotation;
        private bool isAnimatingToKb = false;
        private float kbAnimationProgress = 0f;
        private Vector3 kbAnimStartPos;
        private Quaternion kbAnimStartRot;

        public NetworkedMenu(Player player, VRRig rig)
        {
            this.player = player;
            LoadPlayerSettings(player);
            CreateMenu(player, rig);
        }

        private void LoadPlayerSettings(Player player)
        {
            if (player.CustomProperties.ContainsKey("Juul_T"))
                themeIndex = (int)player.CustomProperties["Juul_T"];

            isRounded = player.CustomProperties.ContainsKey("Juul_R") && (bool)player.CustomProperties["Juul_R"];
            isCatLeft = player.CustomProperties.ContainsKey("Juul_CL") && (bool)player.CustomProperties["Juul_CL"];
            isCatRotated = player.CustomProperties.ContainsKey("Juul_CR") && (bool)player.CustomProperties["Juul_CR"];
            menuWidth = player.CustomProperties.ContainsKey("Juul_MW") ? (float)player.CustomProperties["Juul_MW"] : 0.8f;
            btnInset = player.CustomProperties.ContainsKey("Juul_BI") ? (float)player.CustomProperties["Juul_BI"] : 0.1f;
            textSize = player.CustomProperties.ContainsKey("Juul_TS") ? (float)player.CustomProperties["Juul_TS"] : 0.5f;
            isVR = player.CustomProperties.ContainsKey("Juul_VR") && (bool)player.CustomProperties["Juul_VR"];
        }

        private void CreateMenu(Player player, VRRig rig)
        {
            menuRoot = new GameObject("NetworkMenu_" + player.NickName);
            menuRoot.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            float smFl = Core.SmFl;

            canvas = new GameObject("Canvas");
            canvas.transform.SetParent(menuRoot.transform, false);
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            c.sortingOrder = 1000;
            CanvasScaler cs = canvas.AddComponent<CanvasScaler>();
            cs.dynamicPixelsPerUnit = 2000f;
            GraphicRaycaster gr = canvas.AddComponent<GraphicRaycaster>();

            CreateFrame(smFl);
            CreateSidebar(smFl);
            CreateDisconnectButton(smFl);
            CreateTitle(smFl);
            CreateCategoryButtons(smFl);
            CreateMainButtons(smFl);

            bool isRightHanded = player.CustomProperties.ContainsKey("Juul_H") && (bool)player.CustomProperties["Juul_H"];
            Transform hand = isRightHanded ? rig.rightHandTransform : rig.leftHandTransform;
            if (hand != null)
            {
                menuRoot.transform.position = hand.position;
                menuRoot.transform.rotation = hand.rotation * (isRightHanded ? Quaternion.Euler(180f, 180f, 0f) : Quaternion.identity);
            }

            menuRoot.AddComponent<Core.ScaleInAnimation>();
        }

        private Text CreateTextComponent(GameObject parent, string name, float sizeMultiplier = 1f)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(canvas.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.font = Core.MenuFont;
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            text.resizeTextMaxSize = 300;
            text.text = "";
            text.color = Color.white;
            text.material.renderQueue = 4000;
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0.6f, 0.0875f * textSize * sizeMultiplier);
            rect.localPosition = Vector3.zero;
            return text;
        }

        private void AddOutline(GameObject toOutline)
        {
            GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Cube);
            outline.transform.parent = toOutline.transform.parent;
            outline.transform.localScale = toOutline.transform.localScale - new Vector3(Core.SmFl / 2f, 0f, 0f);
            outline.transform.localPosition = toOutline.transform.localPosition;
            outline.transform.localRotation = toOutline.transform.localRotation;
            GameObject.Destroy(outline.GetComponent<Rigidbody>());
            GameObject.Destroy(outline.GetComponent<BoxCollider>());
            NetworkedGradientUpdater src = toOutline.GetComponent<NetworkedGradientUpdater>();
            NetworkedGradientUpdater dst = outline.AddComponent<NetworkedGradientUpdater>();
            dst.themeIndex = themeIndex;
            if (src != null)
            {
                dst.brightness = Mathf.Max(0f, src.brightness - 0.3f);
                dst.gradientOffset = src.gradientOffset;
                dst.startOffset = src.startOffset;
            }
            else
            {
                dst.brightness = 0.7f;
                dst.gradientOffset = 0f;
            }
            NetworkedRoundedCorners srcCorners = toOutline.GetComponent<NetworkedRoundedCorners>();
            if (srcCorners != null)
            {
                NetworkedRoundedCorners c = outline.AddComponent<NetworkedRoundedCorners>();
                c.bevel = srcCorners.bevel;
            }
            toOutline.transform.localScale = toOutline.transform.localScale - new Vector3(0f, 0.01f, 0.01f);
        }

        private void CreateFrame(float smFl)
        {
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Menu Frame";
            frame.transform.parent = menuRoot.transform;
            frame.transform.localScale = new Vector3(smFl, menuWidth, 0.9f);
            frame.transform.localPosition = new Vector3(smFl * 40f, 0f, 0f);
            GameObject.Destroy(frame.GetComponent<Rigidbody>());
            GameObject.Destroy(frame.GetComponent<BoxCollider>());
            NetworkedGradientUpdater frameGrad = frame.AddComponent<NetworkedGradientUpdater>();
            frameGrad.themeIndex = themeIndex;
            frameGrad.brightness = 1f;
            frameGrad.gradientOffset = 1f;
            frameGrad.startOffset = 0f;
            if (isRounded) frame.AddComponent<NetworkedRoundedCorners>();
            AddOutline(frame);
            uiElements["Frame"] = frame;
        }

        private void CreateSidebar(float smFl)
        {
            GameObject sidebar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sidebar.name = "Sidebar";
            sidebar.transform.parent = menuRoot.transform;
            sidebar.transform.localScale = new Vector3(smFl, 0.45f, 0.9f);
            sidebar.transform.localPosition = new Vector3(
                smFl * 40f + (isCatRotated ? sidebar.transform.localScale.y / 2f : 0f),
                (isCatLeft ? -((menuWidth / 2f) + (sidebar.transform.localScale.y / 2f)) : ((menuWidth / 2f) + (sidebar.transform.localScale.y / 2f))) + (isCatRotated ? 0f : (isCatLeft ? -(smFl * 20f) : (smFl * 20f))),
                0f);
            sidebar.transform.localRotation = Quaternion.Euler(0f, 0f, isCatRotated ? (isCatLeft ? 45f : (-45f)) : 0f);
            GameObject.Destroy(sidebar.GetComponent<Rigidbody>());
            GameObject.Destroy(sidebar.GetComponent<BoxCollider>());
            NetworkedGradientUpdater sidebarGrad = sidebar.AddComponent<NetworkedGradientUpdater>();
            sidebarGrad.themeIndex = themeIndex;
            sidebarGrad.brightness = 1f;
            sidebarGrad.gradientOffset = 1f;
            sidebarGrad.startOffset = 0f;
            if (isRounded) sidebar.AddComponent<NetworkedRoundedCorners>();
            AddOutline(sidebar);
            uiElements["Sidebar"] = sidebar;
        }

        private void CreateDisconnectButton(float smFl)
        {
            GameObject disconnectBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            disconnectBtn.name = "Disconnect Button";
            disconnectBtn.transform.parent = menuRoot.transform;
            disconnectBtn.transform.localScale = new Vector3(smFl, menuWidth, 0.075f);
            disconnectBtn.transform.localPosition = new Vector3(smFl * 40f, 0f, 0.5f);
            GameObject.Destroy(disconnectBtn.GetComponent<Rigidbody>());
            GameObject.Destroy(disconnectBtn.GetComponent<BoxCollider>());
            NetworkedGradientUpdater btnGrad = disconnectBtn.AddComponent<NetworkedGradientUpdater>();
            btnGrad.themeIndex = themeIndex;
            btnGrad.brightness = 1f;
            btnGrad.gradientOffset = 0f;
            btnGrad.startOffset = 0f;
            if (isRounded)
            {
                NetworkedRoundedCorners corners = disconnectBtn.AddComponent<NetworkedRoundedCorners>();
                corners.bevel = corners.bevel / 2f;
            }
            AddOutline(disconnectBtn);

            GameObject textObj = new GameObject("DisconnectText");
            textObj.transform.SetParent(canvas.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.font = Core.MenuFont;
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            text.resizeTextMaxSize = 300;
            text.text = "Disconnect";
            text.color = Color.white;
            text.material.renderQueue = 4000;
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0.6f, 0.0875f * textSize);
            rect.localPosition = new Vector3(smFl * 40f + smFl, 0f, 0.5f) + new Vector3(smFl * 2.5f, 0f, smFl * 2.5f);
            rect.localRotation = Quaternion.Euler(180f, 90f, 90f);

            uiElements["DisconnectButton"] = disconnectBtn;
            textElements["DisconnectText"] = text;
        }

        private void CreateTitle(float smFl)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(canvas.transform, false);
            titleText = titleObj.AddComponent<Text>();
            titleText.font = Core.MenuFont;
            titleText.fontSize = 1;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.resizeTextForBestFit = true;
            titleText.resizeTextMinSize = 0;
            titleText.resizeTextMaxSize = 300;
            titleText.text = "Juul";
            titleText.color = Color.white;
            titleText.fontStyle = FontStyle.Bold;
            titleText.material.renderQueue = 4000;
            RectTransform titleRect = titleText.GetComponent<RectTransform>();
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(0.6f, 0.0875f);
            titleRect.localPosition = new Vector3(smFl * 40f + smFl, 0f, 0.4f) + new Vector3(smFl * 2.5f, 0f, smFl * 2.5f);
            titleRect.localRotation = Quaternion.Euler(180f, 90f, 90f);

            textElements["Title"] = titleText;
        }

        private void CreateCategoryButtons(float smFl)
        {
            float maxTotalH = Core.MaxCatsPerPage * Core.BtnHeight + Mathf.Max(0, Core.MaxCatsPerPage - 1) * Core.BtnSpace;
            float catStartZ = (maxTotalH / 2f) - (Core.BtnHeight / 2f);

            Vector3 rotEuler = new Vector3(0f, 0f, isCatRotated ? (isCatLeft ? 45f : -45f) : 0f);
            Vector3 fNormal = Quaternion.Euler(rotEuler) * Vector3.right;
            Vector3 sidebarPos = uiElements["Sidebar"].transform.localPosition;
            Vector3 catPos = sidebarPos + fNormal * smFl;

            for (int i = 0; i < Core.MaxCatsPerPage; i++)
            {
                GameObject catBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                catBtn.name = "CategoryButton_" + i;
                catBtn.transform.SetParent(menuRoot.transform, false);
                catBtn.transform.localScale = new Vector3(smFl, 0.4f, Core.BtnHeight);
                catBtn.transform.localPosition = new Vector3(catPos.x, catPos.y, catStartZ - ((Core.BtnHeight + Core.BtnSpace) * i));
                catBtn.transform.localRotation = Quaternion.Euler(rotEuler);
                GameObject.Destroy(catBtn.GetComponent<BoxCollider>());
                GameObject.Destroy(catBtn.GetComponent<Rigidbody>());
                Renderer catRend = catBtn.GetComponent<Renderer>();
                catRend.material = new Material(Shader.Find("Sprites/Default"));
                Core.ColorSetter catColor = catBtn.AddComponent<Core.ColorSetter>();
                catColor.brightness = Core.OffBrightness;
                catColor.colorOffset = (i * Core.GradVal) - Core.GradVal;

                Text text = CreateTextComponent(catBtn, "CategoryText_" + i);
                RectTransform rect = text.GetComponent<RectTransform>();
                Vector3 localFaceNormal = Quaternion.Euler(rotEuler) * new Vector3(1f, 0f, 0f);
                rect.localPosition = new Vector3(catPos.x, catPos.y, catStartZ - ((Core.BtnHeight + Core.BtnSpace) * i)) + localFaceNormal * (smFl * 2.5f) + new Vector3(0f, 0f, smFl * 2.5f);
                rect.localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f) + new Vector3(isCatRotated ? (isCatLeft ? -45f : 45f) : 0f, 0f, 0f));

                uiElements["CategoryButton_" + i] = catBtn;
                textElements["CategoryText_" + i] = text;
            }

            CreateSearchButton(smFl, sidebarPos, rotEuler);
            CreateCategoryNavButtons(smFl, sidebarPos, rotEuler);
        }

        private void CreateSearchButton(float smFl, Vector3 sidebarPos, Vector3 rotEuler)
        {
            Vector3 fNormal = Quaternion.Euler(rotEuler) * Vector3.right;
            Vector3 baseP = sidebarPos + fNormal * smFl;
            float offsetSearch = 0.225f - (Core.BtnHeight / 2f) - 0.02f - (Core.BtnHeight + 0.005f) - (Core.BtnHeight + 0.005f);
            Vector3 slideDir = Quaternion.Euler(rotEuler) * Vector3.up;
            Vector3 centerPos = new Vector3(baseP.x, baseP.y, 0.45f + (Core.BtnHeight / 2f) + 0.015f);
            Vector3 posForSearchBtn = isCatLeft ? centerPos + slideDir * (-offsetSearch) : centerPos + slideDir * offsetSearch;

            GameObject searchBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            searchBtn.name = "SearchButton";
            searchBtn.transform.parent = menuRoot.transform;
            searchBtn.transform.localScale = new Vector3(smFl, Core.BtnHeight, Core.BtnHeight);
            searchBtn.transform.localPosition = posForSearchBtn;
            searchBtn.transform.localRotation = Quaternion.Euler(rotEuler);
            GameObject.Destroy(searchBtn.GetComponent<Rigidbody>());
            GameObject.Destroy(searchBtn.GetComponent<BoxCollider>());
            NetworkedGradientUpdater searchGrad = searchBtn.AddComponent<NetworkedGradientUpdater>();
            searchGrad.themeIndex = themeIndex;
            searchGrad.brightness = 1f;
            searchGrad.gradientOffset = Core.BtnHeight / 0.9f;
            searchGrad.startOffset = 0f;
            if (isRounded)
            {
                NetworkedRoundedCorners corners = searchBtn.AddComponent<NetworkedRoundedCorners>();
                corners.bevel = 0.015f;
            }
            AddOutline(searchBtn);

            GameObject searchText = new GameObject("SearchImage");
            searchText.transform.SetParent(canvas.transform, false);

            Vector3 searchFaceNormal = Quaternion.Euler(rotEuler) * new Vector3(1f, 0f, 0f);
            Vector3 searchIconLocalPos = posForSearchBtn + searchFaceNormal * smFl;

            Texture2D searchIcon = Core.GetSearchIconTexture();
            if (searchIcon != null)
            {
                RawImage image = searchText.AddComponent<RawImage>();
                image.texture = searchIcon;
                image.color = Color.white;
                if (image.material != null) image.material.renderQueue = 4000;
                RectTransform rect = image.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                float targetSize = 0.0875f * textSize * 1.35f * 0.9f;
                rect.sizeDelta = new Vector2(targetSize, targetSize);
                rect.localPosition = searchIconLocalPos;
                rect.localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f) + new Vector3(isCatRotated ? (isCatLeft ? -45f : 45f) : 0f, 0f, 0f));
                textElements["SearchImage"] = image;
            }
            else
            {
                Text text = searchText.AddComponent<Text>();
                text.font = Core.MenuFont;
                text.fontSize = 1;
                text.alignment = TextAnchor.MiddleCenter;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;
                text.resizeTextMaxSize = 300;
                text.text = "S";
                text.color = Color.white;
                text.material.renderQueue = 4000;
                RectTransform rect = text.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(0.6f, 0.0875f * textSize * 1.35f);
                rect.localPosition = searchIconLocalPos;
                rect.localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f) + new Vector3(isCatRotated ? (isCatLeft ? -45f : 45f) : 0f, 0f, 0f));
                textElements["SearchImage"] = text;
            }

            uiElements["SearchButton"] = searchBtn;
        }

        private void CreateCategoryNavButtons(float smFl, Vector3 sidebarPos, Vector3 rotEuler)
        {
            Vector3 fNormal = Quaternion.Euler(rotEuler) * Vector3.right;
            Vector3 baseP = sidebarPos + fNormal * smFl;
            float offsetOuter = 0.225f - (Core.BtnHeight / 2f) - 0.02f;
            float offsetInner = offsetOuter - (Core.BtnHeight + 0.005f);
            Vector3 slideDir = Quaternion.Euler(rotEuler) * Vector3.up;
            Vector3 centerPos = new Vector3(baseP.x, baseP.y, 0.45f + (Core.BtnHeight / 2f) + 0.015f);

            Vector3 posForPrev = isCatLeft ? centerPos + slideDir * (-offsetInner) : centerPos + slideDir * offsetOuter;
            Vector3 posForNext = isCatLeft ? centerPos + slideDir * (-offsetOuter) : centerPos + slideDir * offsetInner;

            GameObject prevBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prevBtn.name = "PrevCatButton";
            prevBtn.transform.parent = menuRoot.transform;
            prevBtn.transform.localScale = new Vector3(smFl, Core.BtnHeight, Core.BtnHeight);
            prevBtn.transform.localPosition = posForPrev;
            prevBtn.transform.localRotation = Quaternion.Euler(rotEuler);
            GameObject.Destroy(prevBtn.GetComponent<Rigidbody>());
            GameObject.Destroy(prevBtn.GetComponent<BoxCollider>());
            NetworkedGradientUpdater prevGrad = prevBtn.AddComponent<NetworkedGradientUpdater>();
            prevGrad.themeIndex = themeIndex;
            prevGrad.brightness = 1f;
            prevGrad.gradientOffset = Core.BtnHeight / 0.9f;
            prevGrad.startOffset = 0f;
            if (isRounded)
            {
                NetworkedRoundedCorners corners = prevBtn.AddComponent<NetworkedRoundedCorners>();
                corners.bevel = 0.015f;
            }
            AddOutline(prevBtn);

            GameObject prevText = new GameObject("PrevCatText");
            prevText.transform.SetParent(canvas.transform, false);
            Text prevTextComp = prevText.AddComponent<Text>();
            prevTextComp.font = Core.MenuFont;
            prevTextComp.fontSize = 1;
            prevTextComp.alignment = TextAnchor.MiddleCenter;
            prevTextComp.resizeTextForBestFit = true;
            prevTextComp.resizeTextMinSize = 0;
            prevTextComp.resizeTextMaxSize = 300;
            prevTextComp.text = "<";
            prevTextComp.color = Color.white;
            prevTextComp.material.renderQueue = 4000;
            RectTransform prevRect = prevTextComp.GetComponent<RectTransform>();
            prevRect.pivot = new Vector2(0.5f, 0.5f);
            prevRect.anchorMin = new Vector2(0.5f, 0.5f);
            prevRect.anchorMax = new Vector2(0.5f, 0.5f);
            prevRect.sizeDelta = new Vector2(0.6f, 0.0875f * textSize * 1.1f);
            Vector3 localFaceNormal = Quaternion.Euler(rotEuler) * new Vector3(1f, 0f, 0f);
            prevRect.localPosition = posForPrev + localFaceNormal * (smFl * 2.5f) + new Vector3(0f, 0f, smFl * 2.5f);
            prevRect.localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f) + new Vector3(isCatRotated ? (isCatLeft ? -45f : 45f) : 0f, 0f, 0f));

            GameObject nextBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nextBtn.name = "NextCatButton";
            nextBtn.transform.parent = menuRoot.transform;
            nextBtn.transform.localScale = new Vector3(smFl, Core.BtnHeight, Core.BtnHeight);
            nextBtn.transform.localPosition = posForNext;
            nextBtn.transform.localRotation = Quaternion.Euler(rotEuler);
            GameObject.Destroy(nextBtn.GetComponent<Rigidbody>());
            GameObject.Destroy(nextBtn.GetComponent<BoxCollider>());
            NetworkedGradientUpdater nextGrad = nextBtn.AddComponent<NetworkedGradientUpdater>();
            nextGrad.themeIndex = themeIndex;
            nextGrad.brightness = 1f;
            nextGrad.gradientOffset = Core.BtnHeight / 0.9f;
            nextGrad.startOffset = 0f;
            if (isRounded)
            {
                NetworkedRoundedCorners corners = nextBtn.AddComponent<NetworkedRoundedCorners>();
                corners.bevel = 0.015f;
            }
            AddOutline(nextBtn);

            GameObject nextText = new GameObject("NextCatText");
            nextText.transform.SetParent(canvas.transform, false);
            Text nextTextComp = nextText.AddComponent<Text>();
            nextTextComp.font = Core.MenuFont;
            nextTextComp.fontSize = 1;
            nextTextComp.alignment = TextAnchor.MiddleCenter;
            nextTextComp.resizeTextForBestFit = true;
            nextTextComp.resizeTextMinSize = 0;
            nextTextComp.resizeTextMaxSize = 300;
            nextTextComp.text = ">";
            nextTextComp.color = Color.white;
            nextTextComp.material.renderQueue = 4000;
            RectTransform nextRect = nextTextComp.GetComponent<RectTransform>();
            nextRect.pivot = new Vector2(0.5f, 0.5f);
            nextRect.anchorMin = new Vector2(0.5f, 0.5f);
            nextRect.anchorMax = new Vector2(0.5f, 0.5f);
            nextRect.sizeDelta = new Vector2(0.6f, 0.0875f * textSize * 1.1f);
            nextRect.localPosition = posForNext + localFaceNormal * (smFl * 2.5f) + new Vector3(0f, 0f, smFl * 2.5f);
            nextRect.localRotation = Quaternion.Euler(new Vector3(180f, 90f, 90f) + new Vector3(isCatRotated ? (isCatLeft ? -45f : 45f) : 0f, 0f, 0f));

            uiElements["PrevCatButton"] = prevBtn;
            uiElements["NextCatButton"] = nextBtn;
            textElements["PrevCatText"] = prevTextComp;
            textElements["NextCatText"] = nextTextComp;
        }

        private void CreateMainButtons(float smFl)
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                btn.name = "Button_" + i;
                btn.transform.SetParent(menuRoot.transform, false);
                btn.transform.localScale = new Vector3(smFl, menuWidth - btnInset, Core.BtnHeight);
                btn.transform.localPosition = new Vector3(smFl * 40f + smFl, 0f, Core.BtnUpset - (i * (Core.BtnHeight + Core.BtnSpace)));
                GameObject.Destroy(btn.GetComponent<BoxCollider>());
                GameObject.Destroy(btn.GetComponent<Rigidbody>());
                Renderer btnRend = btn.GetComponent<Renderer>();
                btnRend.material = new Material(Shader.Find("Sprites/Default"));
                Core.ColorSetter btnColor = btn.AddComponent<Core.ColorSetter>();
                btnColor.brightness = Core.OffBrightness;
                btnColor.colorOffset = (i * Core.GradVal) - Core.GradVal;

                Text text = CreateTextComponent(btn, "ButtonText_" + i);
                RectTransform rect = text.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(smFl * 40f + smFl + smFl * 2.5f, 0f, Core.BtnUpset - (i * (Core.BtnHeight + Core.BtnSpace)) + smFl * 2.5f);
                rect.localRotation = Quaternion.Euler(180f, 90f, 90f);

                uiElements["Button_" + i] = btn;
                textElements["ButtonText_" + i] = text;
            }

            for (int s = 0; s < 2; s++)
            {
                string key = s == 0 ? "PageSplitPrev" : "PageSplitNext";
                GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                btn.name = key;
                btn.transform.SetParent(menuRoot.transform, false);
                float halfH = ((menuWidth - btnInset) / 2f) - 0.005f;
                float yOff = (((menuWidth - btnInset) / 2f) + 0.005f) / 2f;
                btn.transform.localScale = new Vector3(smFl, halfH, Core.BtnHeight);
                btn.transform.localPosition = new Vector3(smFl * 40f + smFl, s == 0 ? yOff : -yOff, Core.BtnUpset - ((Core.BtnHeight + Core.BtnSpace) * (Core.MaxButtons + 1)));
                GameObject.Destroy(btn.GetComponent<BoxCollider>());
                GameObject.Destroy(btn.GetComponent<Rigidbody>());
                Renderer rend = btn.GetComponent<Renderer>();
                rend.material = new Material(Shader.Find("Sprites/Default"));
                Core.ColorSetter cs = btn.AddComponent<Core.ColorSetter>();
                cs.brightness = Core.OffBrightness;
                cs.colorOffset = ((Core.MaxButtons + s) * Core.GradVal) - Core.GradVal;

                Text text = CreateTextComponent(btn, key + "Text");
                RectTransform rect = text.GetComponent<RectTransform>();
                rect.localPosition = btn.transform.localPosition + new Vector3(smFl * 2.5f, 0f, smFl * 2.5f);
                rect.localRotation = Quaternion.Euler(180f, 90f, 90f);
                text.text = s == 0 ? "<" : ">";

                btn.SetActive(false);
                text.gameObject.SetActive(false);
                uiElements[key] = btn;
                textElements[key + "Text"] = text;
            }
        }

        public void Update(Player player, VRRig rig)
        {
            if (menuRoot == null) return;

            bool newIsVR = player.CustomProperties.ContainsKey("Juul_VR") && (bool)player.CustomProperties["Juul_VR"];
            if (newIsVR != isVR)
            {
                isVR = newIsVR;
            }

            bool isRightHanded = player.CustomProperties.ContainsKey("Juul_H") && (bool)player.CustomProperties["Juul_H"];
            bool isSearching = player.CustomProperties.ContainsKey("Juul_SRCH") && (bool)player.CustomProperties["Juul_SRCH"];
            bool kbVisible = player.CustomProperties.ContainsKey("Juul_KB") && (bool)player.CustomProperties["Juul_KB"];

            if (textElements.ContainsKey("SearchImage"))
            {
                RawImage searchImage = textElements["SearchImage"] as RawImage;
                if (searchImage != null)
                {
                    Texture2D targetTexture = isSearching ? Core.GetXIconTexture() : Core.GetSearchIconTexture();
                    if (targetTexture != null && searchImage.texture != targetTexture)
                    {
                        searchImage.texture = targetTexture;
                    }
                }
            }

            if (isVR)
            {
                Transform hand = isRightHanded ? rig.rightHandTransform : rig.leftHandTransform;
                if (hand != null && rig.head != null && rig.head.rigTarget != null)
                {
                    if (kbVisible)
                    {
                        if (!wasKbVisible)
                        {
                            Transform head = rig.head.rigTarget.transform;
                            kbFixedPosition = head.position + head.forward * 0.8f;
                            kbFixedRotation = Quaternion.LookRotation(head.position - kbFixedPosition) * Quaternion.Euler(-90f, 0f, -90f);
                            kbAnimStartPos = menuRoot.transform.position;
                            kbAnimStartRot = menuRoot.transform.rotation;
                            isAnimatingToKb = true;
                            kbAnimationProgress = 0f;
                            wasKbVisible = true;
                        }

                        if (isAnimatingToKb)
                        {
                            kbAnimationProgress += Time.deltaTime * 3f;
                            if (kbAnimationProgress >= 1f)
                            {
                                kbAnimationProgress = 1f;
                                isAnimatingToKb = false;
                            }
                            float t = kbAnimationProgress < 0.5f 
                                ? 2f * kbAnimationProgress * kbAnimationProgress 
                                : 1f - Mathf.Pow(-2f * kbAnimationProgress + 2f, 2f) / 2f;
                            menuRoot.transform.position = Vector3.Lerp(kbAnimStartPos, kbFixedPosition, t);
                            menuRoot.transform.rotation = Quaternion.Lerp(kbAnimStartRot, kbFixedRotation, t);
                        }
                        else
                        {
                            menuRoot.transform.position = kbFixedPosition;
                            menuRoot.transform.rotation = kbFixedRotation;
                        }
                    }
                    else
                    {
                        if (wasKbVisible)
                        {
                            Transform localController = isRightHanded ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
                            Transform localWrist = isRightHanded ? GorillaTagger.Instance.offlineVRRig.rightHandTransform : GorillaTagger.Instance.offlineVRRig.leftHandTransform;

                            Vector3 localOffset = localWrist.InverseTransformPoint(localController.position);
                            Quaternion localRotOffset = Quaternion.Inverse(localWrist.rotation) * localController.rotation;

                            Vector3 handPos = hand.TransformPoint(localOffset);
                            Quaternion handRot = hand.rotation * localRotOffset * (isRightHanded ? Quaternion.Euler(180f, 180f, 0f) : Quaternion.identity);

                            kbAnimStartPos = menuRoot.transform.position;
                            kbAnimStartRot = menuRoot.transform.rotation;
                            kbFixedPosition = handPos;
                            kbFixedRotation = handRot;
                            isAnimatingToKb = true;
                            kbAnimationProgress = 0f;
                            wasKbVisible = false;
                        }

                        if (isAnimatingToKb)
                        {
                            Transform localController = isRightHanded ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
                            Transform localWrist = isRightHanded ? GorillaTagger.Instance.offlineVRRig.rightHandTransform : GorillaTagger.Instance.offlineVRRig.leftHandTransform;

                            Vector3 localOffset = localWrist.InverseTransformPoint(localController.position);
                            Quaternion localRotOffset = Quaternion.Inverse(localWrist.rotation) * localController.rotation;

                            Vector3 handPos = hand.TransformPoint(localOffset);
                            Quaternion handRot = hand.rotation * localRotOffset * (isRightHanded ? Quaternion.Euler(180f, 180f, 0f) : Quaternion.identity);

                            kbFixedPosition = handPos;
                            kbFixedRotation = handRot;

                            kbAnimationProgress += Time.deltaTime * 3f;
                            if (kbAnimationProgress >= 1f)
                            {
                                kbAnimationProgress = 1f;
                                isAnimatingToKb = false;
                            }
                            float t = kbAnimationProgress < 0.5f 
                                ? 2f * kbAnimationProgress * kbAnimationProgress 
                                : 1f - Mathf.Pow(-2f * kbAnimationProgress + 2f, 2f) / 2f;
                            menuRoot.transform.position = Vector3.Lerp(kbAnimStartPos, kbFixedPosition, t);
                            menuRoot.transform.rotation = Quaternion.Lerp(kbAnimStartRot, kbFixedRotation, t);
                        }
                        else
                        {
                            Transform localController = isRightHanded ? GorillaTagger.Instance.rightHandTransform : GorillaTagger.Instance.leftHandTransform;
                            Transform localWrist = isRightHanded ? GorillaTagger.Instance.offlineVRRig.rightHandTransform : GorillaTagger.Instance.offlineVRRig.leftHandTransform;

                            Vector3 localOffset = localWrist.InverseTransformPoint(localController.position);
                            Quaternion localRotOffset = Quaternion.Inverse(localWrist.rotation) * localController.rotation;

                            Vector3 handPos = hand.TransformPoint(localOffset);
                            Quaternion handRot = hand.rotation * localRotOffset * (isRightHanded ? Quaternion.Euler(180f, 180f, 0f) : Quaternion.identity);

                            menuRoot.transform.position = Vector3.Lerp(menuRoot.transform.position, handPos, Time.deltaTime * Core.MenuSmoothingSpeed);
                            menuRoot.transform.rotation = Quaternion.Lerp(menuRoot.transform.rotation, handRot, Time.deltaTime * Core.MenuSmoothingSpeed);
                        }
                    }
                }
            }
            else
            {
                if (rig.head != null && rig.head.rigTarget != null)
                {
                    Transform head = rig.head.rigTarget.transform;
                    Vector3 targetPos = head.position + head.forward * 0.6f;
                    Quaternion targetRot = Quaternion.LookRotation(head.position - targetPos) * Quaternion.Euler(-90f, 0f, -90f);

                    menuRoot.transform.position = Vector3.Lerp(menuRoot.transform.position, targetPos, Time.deltaTime * Core.MenuSmoothingSpeed);
                    menuRoot.transform.rotation = Quaternion.Lerp(menuRoot.transform.rotation, targetRot, Time.deltaTime * Core.MenuSmoothingSpeed);
                }
            }

            if (player.CustomProperties.ContainsKey("Juul_T"))
            {
                int newTheme = (int)player.CustomProperties["Juul_T"];
                if (newTheme != themeIndex)
                {
                    themeIndex = newTheme;
                    NetworkedGradientUpdater[] gradients = menuRoot.GetComponentsInChildren<NetworkedGradientUpdater>(true);
                    foreach (var grad in gradients)
                    {
                        if (grad != null)
                            grad.themeIndex = themeIndex;
                    }
                }
            }

            if (titleText != null && player.CustomProperties.ContainsKey("Juul_P"))
            {
                string category = player.CustomProperties["Juul_P"] as string ?? "Home";
                bool playerSearching = player.CustomProperties.ContainsKey("Juul_SRCH") && (bool)player.CustomProperties["Juul_SRCH"];

                if (playerSearching)
                {
                    string searchQuery = player.CustomProperties.ContainsKey("Juul_SRCHQ") ? player.CustomProperties["Juul_SRCHQ"] as string : "";
                    titleText.text = $"Search: {searchQuery}";
                }
                else
                    titleText.text = "Juul";
            }

            UpdateCategories(player);
            UpdateButtons(player);
        }

        private void UpdateCategories(Player player)
        {
            string[] categories = new string[0];
            if (player.CustomProperties.ContainsKey("Juul_CATS"))
            {
                string catsData = player.CustomProperties["Juul_CATS"] as string;
                if (!string.IsNullOrEmpty(catsData))
                    categories = catsData.Split('|');
            }

            string currentCategory = player.CustomProperties.ContainsKey("Juul_P") ? player.CustomProperties["Juul_P"] as string : "Home";

            for (int i = 0; i < Core.MaxCatsPerPage; i++)
            {
                if (uiElements.ContainsKey("CategoryButton_" + i))
                {
                    bool hasCategory = i < categories.Length && !string.IsNullOrEmpty(categories[i]);
                    uiElements["CategoryButton_" + i].SetActive(hasCategory);
                    
                    if (textElements.ContainsKey("CategoryText_" + i))
                    {
                        GameObject textObj = (textElements["CategoryText_" + i] as Text)?.gameObject;
                        if (textObj != null)
                            textObj.SetActive(hasCategory);
                    }
                }

                if (textElements.ContainsKey("CategoryText_" + i))
                {
                    Text text = textElements["CategoryText_" + i] as Text;
                    if (text != null && i < categories.Length && !string.IsNullOrEmpty(categories[i]))
                    {
                        text.text = categories[i];

                        if (uiElements.ContainsKey("CategoryButton_" + i))
                        {
                            Core.ColorSetter colorSetter = uiElements["CategoryButton_" + i].GetComponent<Core.ColorSetter>();
                            if (colorSetter != null)
                                colorSetter.brightness = (categories[i] == currentCategory) ? Core.OnBrightness : Core.OffBrightness;
                        }
                    }
                    else if (text != null)
                    {
                        text.text = "";
                    }
                }
            }
        }

        private void UpdateButtons(Player player)
        {
            string[] pageButtons = new string[0];
            int pbtnsPgv = -1;
            if (player.CustomProperties.ContainsKey("Juul_PBTNS"))
            {
                string pageButtonsData = player.CustomProperties["Juul_PBTNS"] as string;
                if (!string.IsNullOrEmpty(pageButtonsData))
                {
                    pageButtons = pageButtonsData.Split('|');
                    if (pageButtons.Length > 0 && pageButtons[0].StartsWith("PGV"))
                    {
                        int.TryParse(pageButtons[0].Substring(3), out pbtnsPgv);
                        string[] trimmed = new string[pageButtons.Length - 1];
                        System.Array.Copy(pageButtons, 1, trimmed, 0, trimmed.Length);
                        pageButtons = trimmed;
                    }
                }
            }

            Dictionary<string, bool> enabledStates = new Dictionary<string, bool>();
            if (player.CustomProperties.ContainsKey("Juul_BTN"))
            {
                string btnData = player.CustomProperties["Juul_BTN"] as string;
                if (!string.IsNullOrEmpty(btnData))
                {
                    string[] buttons = btnData.Split('|');
                    foreach (string btn in buttons)
                    {
                        if (!string.IsNullOrEmpty(btn))
                            enabledStates[btn] = true;
                    }
                }
            }

            float smFl = Core.SmFl;

            int pgv = (pbtnsPgv >= 0 && pbtnsPgv <= 3) ? pbtnsPgv : 2;
            bool splitNav = (pgv == 0 || pgv == 3);
            if (uiElements.ContainsKey("PageSplitPrev") && uiElements.ContainsKey("PageSplitNext"))
            {
                uiElements["PageSplitPrev"].SetActive(splitNav);
                uiElements["PageSplitNext"].SetActive(splitNav);
                if (textElements.ContainsKey("PageSplitPrevText"))
                    ((Text)textElements["PageSplitPrevText"]).gameObject.SetActive(splitNav);
                if (textElements.ContainsKey("PageSplitNextText"))
                    ((Text)textElements["PageSplitNextText"]).gameObject.SetActive(splitNav);

                if (splitNav)
                {
                    float zPos = Core.BtnUpset - (pgv == 3 ? 0f : ((Core.BtnHeight + Core.BtnSpace) * (Core.MaxButtons + 1)));
                    foreach (string key in new[] { "PageSplitPrev", "PageSplitNext" })
                    {
                        GameObject b = uiElements[key];
                        Vector3 p = b.transform.localPosition;
                        b.transform.localPosition = new Vector3(p.x, p.y, zPos);
                        if (textElements.ContainsKey(key + "Text"))
                        {
                            RectTransform tr = ((Text)textElements[key + "Text"]).GetComponent<RectTransform>();
                            tr.localPosition = new Vector3(b.transform.localPosition.x + smFl * 2.5f, p.y, zPos + smFl * 2.5f);
                        }
                    }
                }
            }

            int btnOffset = (pgv == 3) ? 1 : 0;
            string[] slotData = new string[10];
            for (int si = 0; si < 10; si++) slotData[si] = "";
            for (int j = 0; j < pageButtons.Length && j + btnOffset < 10; j++)
            {
                slotData[j + btnOffset] = pageButtons[j];
            }
            for (int i = 0; i < 10; i++)
            {
                if (textElements.ContainsKey("ButtonText_" + i))
                {
                    Text text = textElements["ButtonText_" + i] as Text;
                    bool hasButton = !string.IsNullOrEmpty(slotData[i]) && !slotData[i].StartsWith(":");

                    if (hasButton && text != null)
                    {
                        string[] parts = slotData[i].Split(':');
                        string buttonName = parts.Length > 0 ? parts[0] : "";
                        bool hasIncrement = parts.Length > 1 && parts[1] == "1";
                        bool hasKeybind = parts.Length > 2 && parts[2] == "1";
                        bool isLabel = parts.Length > 3 && parts[3] == "1";
                        bool isEnabled = enabledStates.ContainsKey(buttonName) && enabledStates[buttonName];

                        text.text = buttonName;

                        if (uiElements.ContainsKey("Button_" + i))
                        {
                            GameObject mainBtn = uiElements["Button_" + i];
                            Core.ColorSetter colorSetter = mainBtn.GetComponent<Core.ColorSetter>();
                            if (colorSetter != null)
                                colorSetter.brightness = isEnabled ? Core.OnBrightness : Core.OffBrightness;

                            float keybindShrink = (hasKeybind && !isLabel) ? 0.0875f : 0f;
                            float incrementShrink = (hasIncrement && !isLabel) ? 0.175f : 0f;
                            mainBtn.transform.localScale = new Vector3(smFl, (menuWidth - btnInset) - incrementShrink - keybindShrink, Core.BtnHeight);
                            mainBtn.transform.localPosition = new Vector3(smFl * 40f + smFl, keybindShrink / 2f, Core.BtnUpset - (i * (Core.BtnHeight + Core.BtnSpace)));

                            Renderer rend = mainBtn.GetComponent<Renderer>();
                            if (rend != null) rend.enabled = !isLabel;

                            RectTransform textRect = text.GetComponent<RectTransform>();
                            textRect.localPosition = new Vector3(smFl * 40f + smFl + smFl * 2.5f, 0f, Core.BtnUpset - (i * (Core.BtnHeight + Core.BtnSpace)) + smFl * 2.5f);
                        }

                        if (hasIncrement && !isLabel)
                        {
                            CreateOrUpdateIncrementIndicators(i, smFl);
                        }
                        else
                        {
                            RemoveIncrementIndicators(i);
                        }

                        if (hasKeybind && !isLabel)
                        {
                            CreateOrUpdateKeybindIndicator(i, smFl);
                        }
                        else
                        {
                            RemoveKeybindIndicator(i);
                        }
                    }
                    else if (text != null)
                    {
                        text.text = "";
                        if (uiElements.ContainsKey("Button_" + i))
                        {
                            Renderer rend = uiElements["Button_" + i].GetComponent<Renderer>();
                            if (rend != null) rend.enabled = false;
                        }
                        RemoveIncrementIndicators(i);
                        RemoveKeybindIndicator(i);
                    }
                }
            }
        }

        private void CreateOrUpdateIncrementIndicators(int index, float smFl)
        {
            string downKey = "IncDown_" + index;
            string upKey = "IncUp_" + index;
            string downTextKey = "IncDownText_" + index;
            string upTextKey = "IncUpText_" + index;

            if (!uiElements.ContainsKey(downKey))
            {
                GameObject downBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                downBtn.name = downKey;
                downBtn.transform.SetParent(menuRoot.transform, false);
                downBtn.transform.localScale = new Vector3(smFl, 0.08f, Core.BtnHeight);
                downBtn.transform.localPosition = new Vector3(smFl * 40f + smFl, ((menuWidth - btnInset) / 2f) - (0.08f / 2f), Core.BtnUpset - (index * (Core.BtnHeight + Core.BtnSpace)));
                GameObject.Destroy(downBtn.GetComponent<BoxCollider>());
                GameObject.Destroy(downBtn.GetComponent<Rigidbody>());
                Renderer downRend = downBtn.GetComponent<Renderer>();
                downRend.material = new Material(Shader.Find("Sprites/Default"));
                Core.ColorSetter downColor = downBtn.AddComponent<Core.ColorSetter>();
                downColor.brightness = Core.OffBrightness;
                downColor.colorOffset = (index * Core.GradVal) - Core.GradVal;

                GameObject downText = new GameObject(downTextKey);
                downText.transform.SetParent(canvas.transform, false);
                Text dText = downText.AddComponent<Text>();
                dText.font = Core.MenuFont;
                dText.fontSize = 1;
                dText.alignment = TextAnchor.MiddleCenter;
                dText.resizeTextForBestFit = true;
                dText.resizeTextMinSize = 0;
                dText.resizeTextMaxSize = 300;
                dText.text = "-";
                dText.color = Color.white;
                dText.material.renderQueue = 4000;
                RectTransform dRect = dText.GetComponent<RectTransform>();
                dRect.pivot = new Vector2(0.5f, 0.5f);
                dRect.anchorMin = new Vector2(0.5f, 0.5f);
                dRect.anchorMax = new Vector2(0.5f, 0.5f);
                dRect.sizeDelta = new Vector2(0.6f, 0.0875f * textSize);
                dRect.localPosition = downBtn.transform.localPosition + new Vector3(smFl * 2.5f, 0f, smFl * 2.5f);
                dRect.localRotation = Quaternion.Euler(180f, 90f, 90f);

                uiElements[downKey] = downBtn;
                textElements[downTextKey] = dText;
            }

            if (!uiElements.ContainsKey(upKey))
            {
                GameObject upBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                upBtn.name = upKey;
                upBtn.transform.SetParent(menuRoot.transform, false);
                upBtn.transform.localScale = new Vector3(smFl, 0.08f, Core.BtnHeight);
                upBtn.transform.localPosition = new Vector3(smFl * 40f + smFl, -(((menuWidth - btnInset) / 2f) - (0.08f / 2f)), Core.BtnUpset - (index * (Core.BtnHeight + Core.BtnSpace)));
                GameObject.Destroy(upBtn.GetComponent<BoxCollider>());
                GameObject.Destroy(upBtn.GetComponent<Rigidbody>());
                Renderer upRend = upBtn.GetComponent<Renderer>();
                upRend.material = new Material(Shader.Find("Sprites/Default"));
                Core.ColorSetter upColor = upBtn.AddComponent<Core.ColorSetter>();
                upColor.brightness = Core.OffBrightness;
                upColor.colorOffset = (index * Core.GradVal) - Core.GradVal;

                GameObject upText = new GameObject(upTextKey);
                upText.transform.SetParent(canvas.transform, false);
                Text uText = upText.AddComponent<Text>();
                uText.font = Core.MenuFont;
                uText.fontSize = 1;
                uText.alignment = TextAnchor.MiddleCenter;
                uText.resizeTextForBestFit = true;
                uText.resizeTextMinSize = 0;
                uText.resizeTextMaxSize = 300;
                uText.text = "+";
                uText.color = Color.white;
                uText.material.renderQueue = 4000;
                RectTransform uRect = uText.GetComponent<RectTransform>();
                uRect.pivot = new Vector2(0.5f, 0.5f);
                uRect.anchorMin = new Vector2(0.5f, 0.5f);
                uRect.anchorMax = new Vector2(0.5f, 0.5f);
                uRect.sizeDelta = new Vector2(0.6f, 0.0875f * textSize);
                uRect.localPosition = upBtn.transform.localPosition + new Vector3(smFl * 2.5f, 0f, smFl * 2.5f);
                uRect.localRotation = Quaternion.Euler(180f, 90f, 90f);

                uiElements[upKey] = upBtn;
                textElements[upTextKey] = uText;
            }
        }

        private void RemoveIncrementIndicators(int index)
        {
            string downKey = "IncDown_" + index;
            string upKey = "IncUp_" + index;
            string downTextKey = "IncDownText_" + index;
            string upTextKey = "IncUpText_" + index;

            if (uiElements.ContainsKey(downKey))
            {
                GameObject.Destroy(uiElements[downKey]);
                uiElements.Remove(downKey);
            }
            if (uiElements.ContainsKey(upKey))
            {
                GameObject.Destroy(uiElements[upKey]);
                uiElements.Remove(upKey);
            }
            if (textElements.ContainsKey(downTextKey))
            {
                GameObject.Destroy(textElements[downTextKey].gameObject);
                textElements.Remove(downTextKey);
            }
            if (textElements.ContainsKey(upTextKey))
            {
                GameObject.Destroy(textElements[upTextKey].gameObject);
                textElements.Remove(upTextKey);
            }
        }

        private void CreateOrUpdateKeybindIndicator(int index, float smFl)
        {
            string gearKey = "Gear_" + index;
            string gearImageKey = "GearImage_" + index;

            if (!uiElements.ContainsKey(gearKey))
            {
                float gearBtnWidth = 0.08f;
                GameObject gearBtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gearBtn.name = gearKey;
                gearBtn.transform.SetParent(menuRoot.transform, false);
                gearBtn.transform.localScale = new Vector3(smFl, gearBtnWidth, Core.BtnHeight);
                gearBtn.transform.localPosition = new Vector3(smFl * 40f + smFl, -(((menuWidth - btnInset) / 2f) - (gearBtnWidth / 2f)), Core.BtnUpset - (index * (Core.BtnHeight + Core.BtnSpace)));
                GameObject.Destroy(gearBtn.GetComponent<BoxCollider>());
                GameObject.Destroy(gearBtn.GetComponent<Rigidbody>());
                Renderer gearRend = gearBtn.GetComponent<Renderer>();
                gearRend.material = new Material(Shader.Find("Sprites/Default"));
                Core.ColorSetter gearColor = gearBtn.AddComponent<Core.ColorSetter>();
                gearColor.brightness = Core.OffBrightness;
                gearColor.colorOffset = (index * Core.GradVal) - Core.GradVal;

                GameObject gearImage = new GameObject(gearImageKey);
                gearImage.transform.SetParent(canvas.transform, false);

                Vector3 gearIconLocalPos = gearBtn.transform.localPosition + new Vector3(smFl, 0f, smFl - 0.005f);

                Texture2D gearIcon = Core.GetGearIconTexture();
                if (gearIcon != null)
                {
                    RawImage image = gearImage.AddComponent<RawImage>();
                    image.texture = gearIcon;
                    image.color = Color.white;
                    if (image.material != null) image.material.renderQueue = 4000;
                    RectTransform rect = image.GetComponent<RectTransform>();
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    float targetSize = 0.0875f * textSize * 1.35f * 0.9f;
                    rect.sizeDelta = new Vector2(targetSize, targetSize);
                    rect.localPosition = gearIconLocalPos;
                    rect.localRotation = Quaternion.Euler(180f, 90f, 90f);
                    textElements[gearImageKey] = image;
                }
                else
                {
                    Text gText = gearImage.AddComponent<Text>();
                    gText.font = Core.MenuFont;
                    gText.fontSize = 1;
                    gText.alignment = TextAnchor.MiddleCenter;
                    gText.resizeTextForBestFit = true;
                    gText.resizeTextMinSize = 0;
                    gText.resizeTextMaxSize = 300;
                    gText.text = "*";
                    gText.color = Color.white;
                    gText.material.renderQueue = 4000;
                    RectTransform gRect = gText.GetComponent<RectTransform>();
                    gRect.pivot = new Vector2(0.5f, 0.5f);
                    gRect.anchorMin = new Vector2(0.5f, 0.5f);
                    gRect.anchorMax = new Vector2(0.5f, 0.5f);
                    gRect.sizeDelta = new Vector2(0.6f, 0.0875f * textSize);
                    gRect.localPosition = gearIconLocalPos;
                    gRect.localRotation = Quaternion.Euler(180f, 90f, 90f);
                    textElements[gearImageKey] = gText;
                }

                uiElements[gearKey] = gearBtn;
            }
        }

        private void RemoveKeybindIndicator(int index)
        {
            string gearKey = "Gear_" + index;
            string gearImageKey = "GearImage_" + index;

            if (uiElements.ContainsKey(gearKey))
            {
                GameObject.Destroy(uiElements[gearKey]);
                uiElements.Remove(gearKey);
            }
            if (textElements.ContainsKey(gearImageKey))
            {
                if (textElements[gearImageKey] is RawImage)
                    GameObject.Destroy(((RawImage)textElements[gearImageKey]).gameObject);
                else if (textElements[gearImageKey] is Text)
                    GameObject.Destroy(((Text)textElements[gearImageKey]).gameObject);
                textElements.Remove(gearImageKey);
            }
        }

        private Color GetThemeColor(int index)
        {
            if (index < 0 || index >= Themes.List.Length) return Color.white;
            return Themes.List[index].Color;
        }

        private void UpdateColors(Color themeColor)
        {
            if (menuRoot == null) return;

            Core.GradientSetter[] gradients = menuRoot.GetComponentsInChildren<Core.GradientSetter>(true);
            foreach (var gs in gradients)
            {
                if (gs != null)
                {
                }
            }

            Core.ColorSetter[] colors = menuRoot.GetComponentsInChildren<Core.ColorSetter>(true);
            foreach (var cs in colors)
            {
                if (cs != null)
                {
                    Renderer r = cs.GetComponent<Renderer>();
                    if (r != null && r.material != null)
                    {
                        float brightness = cs.brightness;
                        Color offsetColor = Color.Lerp(themeColor, Color.black, cs.colorOffset / 10f);
                        r.material.color = offsetColor * brightness;
                    }
                }
            }
        }


        public bool IsClosing { get; private set; }
        public Transform Root => menuRoot != null ? menuRoot.transform : null;

        public void StartClose(System.Action onComplete)
        {
            if (IsClosing) return;
            IsClosing = true;
            if (menuRoot == null)
            {
                onComplete?.Invoke();
                return;
            }
            Core.ScaleInAnimation existing = menuRoot.GetComponent<Core.ScaleInAnimation>();
            if (existing != null) GameObject.Destroy(existing);
            Core.ScaleInAnimation anim = menuRoot.AddComponent<Core.ScaleInAnimation>();
            anim.reverse = true;
            anim.duration = 0.4f;
            anim.onComplete = onComplete;
        }

        public void Destroy()
        {
            if (menuRoot != null)
                GameObject.Destroy(menuRoot);
        }
    }

    public class NetworkedKeyboard
    {
        private GameObject root;
        private GameObject canvas;
        private Text queryText;
        private int themeIndex;
        private bool isRounded;

        private static readonly string[][] KeyLayout = new string[][]
        {
        new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" },
        new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
        new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
        new string[] { "Z", "X", "C", "V", "B", "N", "M" },
        new string[] { "Space", "Back", "Enter" }
        };

        public bool IsClosing { get; private set; }

        public NetworkedKeyboard(Transform parent, int themeIndex, bool isRounded)
        {
            this.themeIndex = themeIndex;
            this.isRounded = isRounded;
            Build(parent);
        }

        private void Build(Transform parent)
        {
            root = new GameObject("NetworkKeyboard");
            root.transform.SetParent(parent, false);
            root.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

            canvas = new GameObject("KbCanvas");
            canvas.transform.SetParent(root.transform, false);
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            c.sortingOrder = 1001;
            CanvasScaler cs = canvas.AddComponent<CanvasScaler>();
            cs.dynamicPixelsPerUnit = 2000f;
            canvas.AddComponent<GraphicRaycaster>();

            float keySize = 0.035f;
            float spacing = 0.005f;
            float kbWidth = (10 * keySize) + (11 * spacing) + 0.04f;
            float totalKeysHeight = (5 * keySize) + (4 * spacing);
            float kbHeight = totalKeysHeight + 0.035f;

            GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.transform.SetParent(root.transform, false);
            bg.transform.localScale = new Vector3(Core.SmFl, kbWidth, kbHeight);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localRotation = Quaternion.identity;
            GameObject.Destroy(bg.GetComponent<BoxCollider>());
            GameObject.Destroy(bg.GetComponent<Rigidbody>());
            NetworkedGradientUpdater bgGrad = bg.AddComponent<NetworkedGradientUpdater>();
            bgGrad.themeIndex = themeIndex;
            bgGrad.brightness = 1f;
            bgGrad.startOffset = 1.0f;
            bgGrad.gradientOffset = 0.2f;
            if (isRounded)
            {
                NetworkedRoundedCorners corners = bg.AddComponent<NetworkedRoundedCorners>();
                corners.bevel = 0.015f;
            }

            GameObject bgOutline = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bgOutline.transform.SetParent(root.transform, false);
            bgOutline.transform.localScale = new Vector3(Core.SmFl * 0.5f, kbWidth + 0.008f, kbHeight + 0.008f);
            bgOutline.transform.localPosition = new Vector3(-Core.SmFl * 0.25f, 0f, 0f);
            bgOutline.transform.localRotation = Quaternion.identity;
            GameObject.Destroy(bgOutline.GetComponent<BoxCollider>());
            GameObject.Destroy(bgOutline.GetComponent<Rigidbody>());
            NetworkedGradientUpdater outlineGrad = bgOutline.AddComponent<NetworkedGradientUpdater>();
            outlineGrad.themeIndex = themeIndex;
            outlineGrad.brightness = 0.7f;
            outlineGrad.startOffset = 1.0f;
            outlineGrad.gradientOffset = 0.2f;
            if (isRounded)
            {
                NetworkedRoundedCorners corners = bgOutline.AddComponent<NetworkedRoundedCorners>();
                corners.bevel = 0.015f;
            }

            float startY = (totalKeysHeight / 2f) - (keySize / 2f);
            for (int r = 0; r < KeyLayout.Length; r++)
            {
                string[] row = KeyLayout[r];
                float rowWidth = 0f;
                float[] keyWidths = new float[row.Length];
                for (int col = 0; col < row.Length; col++)
                {
                    float width = keySize;
                    if (row[col] == "Space") width = keySize * 6f;
                    else if (row[col] == "Back") width = keySize * 1.5f;
                    else if (row[col] == "Enter") width = keySize * 2.5f;
                    keyWidths[col] = width;
                    rowWidth += width;
                }
                rowWidth += (row.Length - 1) * spacing;
                float currentX = rowWidth / 2f;
                for (int col = 0; col < row.Length; col++)
                {
                    string key = row[col];
                    float kWidth = keyWidths[col];
                    float xPos = currentX - (kWidth / 2f);
                    float yPos = startY - (r * (keySize + spacing));
                    Vector3 pos = new Vector3(Core.SmFl + 0.002f, xPos, yPos);
                    CreateKey(key, pos, keySize, kWidth);
                    currentX -= (kWidth + spacing);
                }
            }

            float targetX = (Core.SmFl * 40f) + 0.14f;
            float targetZ = -0.45f - (kbHeight / 2f) - 0.13f;
            root.transform.localPosition = new Vector3(targetX, 0f, targetZ);
            root.transform.localRotation = Quaternion.Euler(0f, -40f, 0f);

            root.AddComponent<Core.ScaleInAnimation>();
        }

        private void CreateKey(string keyChar, Vector3 localPos, float height, float width)
        {
            GameObject keyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keyObj.transform.SetParent(root.transform, false);
            keyObj.transform.localScale = new Vector3(Core.SmFl, width, height);
            keyObj.transform.localPosition = localPos;
            keyObj.transform.localRotation = Quaternion.identity;
            GameObject.Destroy(keyObj.GetComponent<BoxCollider>());
            GameObject.Destroy(keyObj.GetComponent<Rigidbody>());
            Core.ColorSetter cs = keyObj.AddComponent<Core.ColorSetter>();
            cs.brightness = Core.OffBrightness;

            ButtonCollider buttonCol = keyObj.AddComponent<ButtonCollider>();
            buttonCol.onClick = () => OnKeyPressed(keyChar);

            GameObject txt = new GameObject("KbKeyText");
            txt.transform.SetParent(canvas.transform, false);
            Text t = txt.AddComponent<Text>();
            t.font = Core.MenuFont;
            t.fontSize = 1;
            t.alignment = TextAnchor.MiddleCenter;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 0;
            t.resizeTextMaxSize = 300;

            string displayText = keyChar;
            if (keyChar == "Back") displayText = "Back";
            if (keyChar == "Space") displayText = "Space";
            if (keyChar == "Enter") displayText = "Enter";
            t.text = displayText;

            t.color = Color.white;
            t.material.renderQueue = 4000;
            RectTransform rect = t.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width * 0.9f, height * 0.7f);
            rect.localPosition = localPos + new Vector3((Core.SmFl / 2f) + 0.001f, 0f, Core.SmFl);
            rect.localRotation = Quaternion.Euler(180f, 90f, 90f);
        }

        private void OnKeyPressed(string keyChar)
        {
            Debug.Log($"Key pressed: {keyChar}");
        }

        public void UpdateState(int newThemeIndex, string query)
        {
            if (root == null) return;
            if (newThemeIndex != themeIndex)
            {
                themeIndex = newThemeIndex;
                NetworkedGradientUpdater[] gradients = root.GetComponentsInChildren<NetworkedGradientUpdater>(true);
                foreach (var g in gradients) if (g != null) g.themeIndex = themeIndex;
            }
            if (queryText != null) queryText.text = string.IsNullOrEmpty(query) ? "_" : query;
        }

        public void StartClose(System.Action onComplete)
        {
            if (IsClosing) return;
            IsClosing = true;
            if (root == null) { onComplete?.Invoke(); return; }
            Core.ScaleInAnimation existing = root.GetComponent<Core.ScaleInAnimation>();
            if (existing != null) GameObject.Destroy(existing);
            Core.ScaleInAnimation anim = root.AddComponent<Core.ScaleInAnimation>();
            anim.reverse = true;
            anim.duration = 0.4f;
            anim.onComplete = onComplete;
        }
    }
}




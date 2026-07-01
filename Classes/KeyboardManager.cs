using System;
using UnityEngine;

namespace Juul
{
    public class KeyboardManager
    {
        public static GameObject KeyboardObj = null;
        public static bool IsJoiningRoom = false;
        public static bool WasJoiningRoomLastFrame = false;
        public static string JoinRoomQuery = "";
        public static bool IsSavingPreset = false;
        public static string PresetSaveQuery = "";
        public static bool KeyboardJustOpened = false;

        public static bool IsSettingQuestScore = false;
        public static string QuestScoreQuery = "";

        public static bool IsSettingName = false;
        public static string NameQuery = "";

        private static readonly string[][] KeyLayout = new string[][]
        {
            new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" },
            new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },  
            new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" },     
            new string[] { "Z", "X", "C", "V", "B", "N", "M" },               
            new string[] { "Space", "\b", "Enter" }
        };

        public static void ToggleKeyboard(bool show)
        {
            if (!show)
            {
                if (KeyboardObj != null)
                {
                    if (!KeyboardObj.Equals(null))
                    {
                        var anim = KeyboardObj.AddComponent<Core.ScaleInAnimation>();
                        anim.reverse = true;
                        anim.duration = 0.4f;
                    }
                    KeyboardObj = null;
                }
                return;
            }

            if (KeyboardObj != null)
            {
                if (KeyboardObj.Equals(null))
                {
                    KeyboardObj = null;
                }
                else
                {
                    return;
                }
            }

            KeyboardObj = new GameObject("SearchKeyboard");

            if (Core.Menu != null)
            {
                KeyboardObj.transform.SetParent(Core.Menu.transform, true);
            }
            if (KeyboardJustOpened && Core.IsAnimated)
            {
                KeyboardJustOpened = false;
                var anim = KeyboardObj.AddComponent<Core.ScaleInAnimation>();
                anim.duration = 0.4f;
            }
            Canvas kbCanvas = KeyboardObj.AddComponent<Canvas>();
            kbCanvas.renderMode = RenderMode.WorldSpace;
            KeyboardObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.transform.parent = KeyboardObj.transform;
            GameObject.Destroy(bg.GetComponent<Rigidbody>());
            GameObject.Destroy(bg.GetComponent<BoxCollider>());
            float keySize = 0.035f;
            float spacing = 0.005f;
            float kbWidth = (10 * keySize) + (11 * spacing) + 0.04f;
            float totalKeysHeight = (5 * keySize) + (4 * spacing);
            float kbHeight = totalKeysHeight + 0.035f;
            bg.transform.localScale = new Vector3(Core.SmFl, kbWidth, kbHeight);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localRotation = Quaternion.identity;
            Core.GradientSetter bgGrad = bg.AddComponent<Core.GradientSetter>();
            bgGrad.startOffset = 1.0f;
            bgGrad.gradientOffset = 0.2f;
            if (Core.IsRounded)
            {
                Core.RoundedCorners corners = bg.AddComponent<Core.RoundedCorners>();
                corners.bevel = 0.015f;
            }
            Core.OutlineGradient(bg);
            float startY = (totalKeysHeight / 2f) - (keySize / 2f);
            for (int r = 0; r < KeyLayout.Length; r++)
            {
                string[] row = KeyLayout[r];

                float rowWidth = 0f;
                float[] keyWidths = new float[row.Length];
                for (int c = 0; c < row.Length; c++)
                {
                    float width = keySize;
                    if (row[c] == "Space") width = keySize * 6f;
                    else if (row[c] == "\b") width = keySize * 1.5f;
                    else if (row[c] == "Enter") width = keySize * 2.5f;
                    keyWidths[c] = width;
                    rowWidth += width;
                }
                rowWidth += (row.Length - 1) * spacing;
                float currentX = rowWidth / 2f;
                for (int c = 0; c < row.Length; c++)
                {
                    string key = row[c];
                    float kWidth = keyWidths[c];
                    float xPos = currentX - (kWidth / 2f);
                    float yPos = startY - (r * (keySize + spacing));

                    Vector3 pos = new Vector3(Core.SmFl + 0.002f, xPos, yPos);
                    CreateKey(KeyboardObj.transform, key, pos, keySize, kWidth);

                    currentX -= (kWidth + spacing);
                }
            }
            float targetX = (Core.SmFl * 40f) + 0.14f;
            float targetZ = -0.45f - (kbHeight / 2f) - 0.13f;
            KeyboardObj.transform.localPosition = new Vector3(targetX, 0f, targetZ);
            KeyboardObj.transform.localRotation = Quaternion.Euler(0f, -40f, 0f);
        }

        private static void CreateKey(Transform parent, string keyChar, Vector3 localPos, float height, float width)
        {
            GameObject keyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keyObj.layer = 2;
            keyObj.transform.parent = parent;
            keyObj.transform.localScale = new Vector3(Core.SmFl, width, height);
            keyObj.transform.localPosition = localPos;
            keyObj.transform.localRotation = Quaternion.identity;
            GameObject.Destroy(keyObj.GetComponent<Rigidbody>());
            BoxCollider col = keyObj.GetComponent<BoxCollider>();
            col.isTrigger = true;
            Core.ColorSetter cs = keyObj.AddComponent<Core.ColorSetter>();
            cs.brightness = Core.OffBrightness;

            string displayText = keyChar;
            if (keyChar == "\b") displayText = "Back";
            if (keyChar == "Space") displayText = "Space";
            if (keyChar == "Enter") displayText = "Enter";

            AddTextObj(KeyboardObj.transform, () =>
            {
                return displayText;
            }, localPos + new Vector3((Core.SmFl / 2f) + 0.001f, 0f, 0f), height * 0.8f);

            ButtonCollider buttonCol = keyObj.AddComponent<ButtonCollider>();
            buttonCol.onClick = () =>
            {
                if (keyChar == "Enter")
                {
                    if (IsSettingQuestScore)
                    {
                        if (int.TryParse(QuestScoreQuery, out int score))
                        {
                            VRRig.LocalRig.SetQuestScore(score);
                        }
                        else if (QuestScoreQuery == "")
                        {
                            VRRig.LocalRig.SetQuestScore(0);
                        }
                        IsSettingQuestScore = false;
                        QuestScoreQuery = "";
                        try { ExtraButtons.QuestScoreButton.Enabled = false; } catch { }
                        ToggleKeyboard(false);
                    }
                    else if (IsSavingPreset)
                    {
                        Configs.SaveModPresetWithName(PresetSaveQuery);
                        IsSavingPreset = false;
                        try { ExtraButtons.SavePresetButton.Enabled = false; } catch { }
                    }
                    else if (IsJoiningRoom)
                    {
                        GorillaNetworking.PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(JoinRoomQuery, GorillaNetworking.JoinType.Solo);
                        IsJoiningRoom = false;
                        try { ExtraButtons.RoomJoinerButton.Enabled = false; } catch { }
                    }
                    else if (IsSettingName)
                    {
                        if (NameQuery.Length > 0)
                        {
                            Fun.BypassNameChange(NameQuery);
                        }
                        IsSettingName = false;
                        NameQuery = "";
                        try
                        {
                            if (ExtraButtons.SetNameButton != null)
                            {
                                ExtraButtons.SetNameButton.Name = "[Can Bypass] Set Name:";
                            }
                        }
                        catch { }
                        ToggleKeyboard(false);
                    }
                    else
                    {
                        SearchManager.PerformSearch();
                        SearchManager.IsSearching = false;
                        try { ExtraButtons.GetCategory("Settings").Buttons.Find(b => b.Name == "Configure Search").Enabled = false; } catch { }
                    }

                    return;
                }

                string charToAdd = "";
                if (keyChar == "\b")
                {
                    charToAdd = "\b";
                }
                else if (keyChar == "Space")
                {
                    charToAdd = " ";
                }
                else
                {
                    charToAdd = keyChar.ToUpper();
                }

                if (IsSettingQuestScore)
                {
                    if (charToAdd == "\b")
                    {
                        if (QuestScoreQuery.Length > 0)
                            QuestScoreQuery = QuestScoreQuery.Substring(0, QuestScoreQuery.Length - 1);
                    }
                    else if (char.IsDigit(charToAdd[0]))
                    {
                        QuestScoreQuery += charToAdd;
                    }

                    if (ExtraButtons.QuestScoreButton != null)
                        ExtraButtons.QuestScoreButton.Name = "Set Quest Score: " + (QuestScoreQuery == "" ? "0" : QuestScoreQuery);
                }
                else if (IsSavingPreset)
                {
                    if (charToAdd == "\b")
                    {
                        if (PresetSaveQuery.Length > 0)
                            PresetSaveQuery = PresetSaveQuery.Substring(0, PresetSaveQuery.Length - 1);
                    }
                    else if (charToAdd == " ") PresetSaveQuery += " ";
                    else PresetSaveQuery += charToAdd;

                    if (ExtraButtons.SavePresetButton != null)
                        ExtraButtons.SavePresetButton.Name = "Save Preset: " + PresetSaveQuery;
                }
                else if (IsJoiningRoom)
                {
                    if (charToAdd == "\b")
                    {
                        if (JoinRoomQuery.Length > 0)
                            JoinRoomQuery = JoinRoomQuery.Substring(0, JoinRoomQuery.Length - 1);
                    }
                    else if (charToAdd == " ") JoinRoomQuery += " ";
                    else JoinRoomQuery += charToAdd.ToUpper();

                    if (ExtraButtons.RoomJoinerButton != null)
                        ExtraButtons.RoomJoinerButton.Name = "Join Room: " + JoinRoomQuery;
                }
                else if (IsSettingName)
                {
                    if (charToAdd == "\b")
                    {
                        if (NameQuery.Length > 0)
                            NameQuery = NameQuery.Substring(0, NameQuery.Length - 1);
                    }
                    else if (charToAdd == " ") NameQuery += " ";
                    else NameQuery += charToAdd;

                    try
                    {
                        if (ExtraButtons.SetNameButton != null)
                            ExtraButtons.SetNameButton.Name = "[Can Bypass] Set Name: " + NameQuery;
                    }
                    catch { }
                }
                else
                {
                    if (charToAdd == "\b")
                    {
                        if (SearchManager.SearchQuery.Length > 0)
                            SearchManager.SearchQuery = SearchManager.SearchQuery.Substring(0, SearchManager.SearchQuery.Length - 1);
                    }
                    else if (charToAdd == " ") SearchManager.SearchQuery += " ";
                    else SearchManager.SearchQuery += charToAdd;  
                    SearchManager.PerformSearch();
                }
            };
        }

        private static void AddTextObj(Transform parent, Func<string> textGetter, Vector3 localPos, float size)
        {
            GameObject textObj = new GameObject("Keyboardtext");
            textObj.transform.SetParent(parent, false);
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.font = Core.MenuFont;
            text.fontSize = 40;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = false;
            text.color = Color.white;
            text.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
            text.verticalOverflow = UnityEngine.VerticalWrapMode.Overflow;
            text.material.renderQueue = 4000;
            float scaleTweak = 0.0005f;
            RectTransform component = text.GetComponent<RectTransform>();
            component.sizeDelta = new Vector2(0.35f / scaleTweak, (0.035f * size * 20f) / scaleTweak);
            component.transform.localScale = Vector3.one * scaleTweak;
            component.localPosition = localPos;
            component.localRotation = Quaternion.Euler(180f, 90f, 90f);
            Updater updater = textObj.AddComponent<Updater>();
            updater.getter = textGetter;
            updater.textComponent = text;
        }

        public class Updater : MonoBehaviour
        {
            public Func<string> getter;
            public UnityEngine.UI.Text textComponent;
            void Update()
            {
                if (textComponent != null && getter != null)
                {
                    string newText = getter();
                    if (textComponent.text != newText)
                    {
                        textComponent.text = newText;
                    }
                }
            }
        }
    }
}




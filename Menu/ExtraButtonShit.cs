using GorillaGameModes;
using GorillaLocomotion;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Juul.Mods;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static Juul.NotifiLib;
using static Juul.Patches;
using static Juul.Patches.GameplayPatches;
using static Mono.Security.X509.X520;

namespace Juul
{
    public class ExtraButtons
    {
        public static Button ThemeButton;
        public static Button GunStyleButton;
        public static Button GunLineSizeButton;
        public static Button GunSphereSizeButton;
        public static Button RoomJoinerButton;
        public static Button MenuSmoothingSpeedButton;
        public static Button SavePresetButton;
        public static Button LoadPresetButton;
        public static Button AutoLoadPresetButton;
        public static Button FlySpeedButton;
        public static Button SpeedBoostSpeedButton;
        public static Button WallWalkStrengthButton;
        public static Button MenuScaleButton;
        public static Button QuestScoreButton;
        public static Button SetNameButton;
        public static Button BarrelMethodButton;
        public static Button AntiReportRadiusButton;
        public static Button LagMethodButton;


        public static Category speedBoostConfig;
        public static Category wallWalkConfig;
        public static Category platformsConfig;
        public static Category flightConfig;
        public static Category nameTagsConfig;
        public static Category fakeFBTConfig;

        public static Category EnabledCategory;
        public static bool ghostview = true;

        public static int CurrentPresetIndex = 0;

        public static void Initialize()
        {
            EnabledCategory = new Category { Name = "Enabled" };

            ThemeButton = new Button { Name = $"Theme: {Core.GetCurrentThemeName()}", Toggle = false, Incremental = true, Up = () => Core.ChangeTheme(true), Down = () => Core.ChangeTheme(false) };
            MenuScaleButton = new Button { Name = $"Menu Scale: {Core.MenuWidth.ToString()}", Toggle = false, Incremental = true, Up = () => Core.ChangeMenuScale(true), Down = () => Core.ChangeMenuScale(false) };
            GunStyleButton = new Button { Name = "Gun Style: " + GunLib.currentLineStyle.ToString(), Toggle = false, Incremental = true, Up = () => GunLib.ChangeGunStyle(true), Down = () => GunLib.ChangeGunStyle(false) };
            GunLineSizeButton = new Button { Name = string.Format("Gun Line Size: {0}", GunLib.GunLineWidth), Toggle = false, Incremental = true, Up = () => GunLib.ChangeGunLineSize(true), Down = () => GunLib.ChangeGunLineSize(false) };
            GunSphereSizeButton = new Button { Name = string.Format("Gun Sphere Size: {0}", GunLib.SphereSize), Toggle = false, Incremental = true, Up = () => GunLib.ChangeGunSphereScale(true), Down = () => GunLib.ChangeGunSphereScale(false) };
            RoomJoinerButton = new Button { Name = "Join Room: ", Toggle = true, OnceEnable = () => { KeyboardManager.IsJoiningRoom = true; KeyboardManager.JoinRoomQuery = ""; KeyboardManager.KeyboardJustOpened = true; }, OnceDisable = () => KeyboardManager.IsJoiningRoom = false };
            MenuSmoothingSpeedButton = new Button { Name = $"Smoothing Speed: {Core.MenuSmoothingSpeed}", Toggle = false, Incremental = true, Up = () => Core.ChangeMenuSmoothingSpeed(true), Down = () => Core.ChangeMenuSmoothingSpeed(false) };

            FlySpeedButton = new Button { Name = $"Fly Speed: {Movement.speedNames[Movement.speedIndex]}", Toggle = false, Incremental = true, Up = () => Movement.ChangeFlySpeed(true), Down = () => Movement.ChangeFlySpeed(false) };
            SpeedBoostSpeedButton = new Button { Name = $"Boost: {Movement.speedNames2[Movement.speedIndex2]}", Toggle = false, Incremental = true, Up = () => Movement.ChangeSpeedBoostSpeed(true), Down = () => Movement.ChangeSpeedBoostSpeed(false) };
            WallWalkStrengthButton = new Button { Name = $"Wall Walk: {Movement.wallForceNames[Movement.wallForceIndex]}", Toggle = false, Incremental = true, Up = () => Movement.AdjustWallWalkStrength(true), Down = () => Movement.AdjustWallWalkStrength(false) };

            BarrelMethodButton = new Button { Name = $"Barrel Strength: {Overpowered.GetBarrelFlingMethodName()}", Toggle = false, Incremental = true, Up = () => Overpowered.ChangeBarrelMethod(true), Down = () => Overpowered.ChangeBarrelMethod(false) };

            AntiReportRadiusButton = new Button { Name = $"Anti Report Size: {Safety.GetAntiReportRadiusName()}", Toggle = false, Incremental = true, Up = () => Safety.ChangeAntiReportRadius(true), Down = () => Safety.ChangeAntiReportRadius(false) };

            LagMethodButton = new Button { Name = $"Lag Method: {Overpowered.GetLagMethodName()}", Toggle = false, Incremental = true, Up = () => Overpowered.ChangeLagMethod(true), Down = () => Overpowered.ChangeLagMethod(false) };


            SavePresetButton = new Button { Name = "Save Preset: ", Toggle = true, OnceEnable = () => { KeyboardManager.IsSavingPreset = true; KeyboardManager.PresetSaveQuery = ""; KeyboardManager.KeyboardJustOpened = true; }, OnceDisable = () => KeyboardManager.IsSavingPreset = false };

            QuestScoreButton = new Button { Name = "Set Quest Score: 0", Toggle = true, OnceEnable = () => { KeyboardManager.IsSettingQuestScore = true; KeyboardManager.QuestScoreQuery = ""; KeyboardManager.KeyboardJustOpened = true; }, OnceDisable = () => { KeyboardManager.IsSettingQuestScore = false; KeyboardManager.QuestScoreQuery = ""; } };

            SetNameButton = new Button { Name = "[Can Bypass] Set Name:", Toggle = true, OnceEnable = () => { KeyboardManager.IsSettingName = true; KeyboardManager.NameQuery = ""; KeyboardManager.KeyboardJustOpened = true; }, OnceDisable = () => KeyboardManager.IsSettingName = false };
         
            Action refreshPresetBtn = () =>
            {
                string[] presets = Configs.GetAvailablePresets();
                if (presets.Length == 0)
                {
                    LoadPresetButton.Name = "Load Preset: [None]";
                    if (AutoLoadPresetButton != null)
                        AutoLoadPresetButton.Name = "Auto-Load: [None]";
                    return;
                }
                if (CurrentPresetIndex >= presets.Length) CurrentPresetIndex = 0;
                if (CurrentPresetIndex < 0) CurrentPresetIndex = presets.Length - 1;
                LoadPresetButton.Name = "Load Preset: " + presets[CurrentPresetIndex];

                if (AutoLoadPresetButton != null)
                {
                    if (string.IsNullOrEmpty(Configs.AutoLoadPresetName))
                        AutoLoadPresetButton.Name = "Auto-Load: [None]";
                    else
                        AutoLoadPresetButton.Name = "Auto-Load: " + Configs.AutoLoadPresetName;
                }
            };

            LoadPresetButton = new Button
            {
                Name = "Load Preset: [None]",
                Toggle = false,
                Incremental = true,
                Up = () => { CurrentPresetIndex++; refreshPresetBtn(); Core.RebuildMenu(); },
                Down = () => { CurrentPresetIndex--; refreshPresetBtn(); Core.RebuildMenu(); },
                OnEnable = () =>
                {
                    string[] presets = Configs.GetAvailablePresets();
                    if (presets.Length > 0 && CurrentPresetIndex >= 0 && CurrentPresetIndex < presets.Length)
                    {
                        Configs.LoadModPresetWithName(presets[CurrentPresetIndex]);
                    }
                }
            };
            
            AutoLoadPresetButton = new Button
            {
                Name = "Auto-Load: [None]",
                Toggle = false,
                OnEnable = () =>
                {
                    if (!string.IsNullOrEmpty(Configs.AutoLoadPresetName))
                    {
                        Configs.AutoLoadPresetName = "";
                        Configs.SaveSettingsConfig();
                    }
                    else
                    {
                        string[] presets = Configs.GetAvailablePresets();
                        if (presets.Length > 0 && CurrentPresetIndex >= 0 && CurrentPresetIndex < presets.Length)
                        {
                            Configs.AutoLoadPresetName = presets[CurrentPresetIndex];
                            Configs.SaveSettingsConfig();
                        }
                    }
                    refreshPresetBtn();
                    Core.RebuildMenu();
                }
            };
            refreshPresetBtn();

            ButtonConfigs.Register("Noclip",
                new KeyBind { Name = "Noclip", PCKey = KeyCode.N, VRInput = VRButton.RightPrimary, HasPCBind = true }
            );
            ButtonConfigs.Register("Speed Boost",
                new KeyBind { Name = "Boost", PCKey = KeyCode.None, VRInput = VRButton.None, HasPCBind = false }
            );
            ButtonConfigs.Register("Platforms",
                new KeyBind { Name = "Left Hand", PCKey = KeyCode.None, VRInput = VRButton.LeftGrip, HasPCBind = false },
                new KeyBind { Name = "Right Hand", PCKey = KeyCode.None, VRInput = VRButton.RightGrip, HasPCBind = false }
            );
            ButtonConfigs.Register("Wall Walk",
                new KeyBind { Name = "Grab", PCKey = KeyCode.None, VRInput = VRButton.None, HasPCBind = false }
            );
            ButtonConfigs.Register("Check Point",
                new KeyBind { Name = "Set", PCKey = KeyCode.None, VRInput = VRButton.RightPrimary, HasPCBind = false },
                new KeyBind { Name = "Teleport", PCKey = KeyCode.None, VRInput = VRButton.RightSecondary, HasPCBind = false }
            );
            ButtonConfigs.Register("Flight",
                new KeyBind { Name = "Fly", PCKey = KeyCode.None, VRInput = VRButton.RightPrimary, HasPCBind = false, IsSingleBind = true }
            );

            ButtonConfigs.Register("Fake FBT",
                new KeyBind { Name = "Action", PCKey = KeyCode.None, VRInput = VRButton.LeftPrimary, HasPCBind = false, IsSingleBind = true }
            );

            speedBoostConfig = ButtonConfigs.Get("Speed Boost").GenerateCategory();
            speedBoostConfig.Buttons.Add(SpeedBoostSpeedButton);

            fakeFBTConfig = ButtonConfigs.Get("Fake FBT").GenerateCategory();

            wallWalkConfig = ButtonConfigs.Get("Wall Walk").GenerateCategory();
            wallWalkConfig.Buttons.Add(WallWalkStrengthButton);

            var platGroup = ButtonConfigs.Get("Platforms");
            platformsConfig = new Category { Name = "Platforms Settings" };

            if (platGroup != null && platGroup.Binds.Count > 0)
            {
                Button platModeBtn = new Button
                {
                    Name = $"Mode: {platGroup.Binds[0].GetModeName()}",
                    Toggle = false,
                    OnEnable = () =>
                    {
                        bool newState = !platGroup.Binds[0].IsToggle;
                        foreach (var bind in platGroup.Binds)
                            bind.IsToggle = newState;
                        ButtonConfigs.RefreshAllBindButtons();
                        Core.RebuildMenu();
                    }
                };
                platformsConfig.Buttons.Add(platModeBtn);
                foreach (var bind in platGroup.Binds)
                    bind.BtnMode = platModeBtn;
            }

            if (platGroup != null && platGroup.Binds.Count >= 1)
            {
                var leftBind = platGroup.Binds[0];
                Button leftVRBtn = null;
                leftVRBtn = new Button
                {
                    Name = $"Left Hand: {leftBind.GetVRInputName()}",
                    Toggle = false,
                    Incremental = true,
                    Up = () =>
                    {
                        int idx = Array.IndexOf(ButtonConfigs.VRButtons, leftBind.VRInput);
                        leftBind.VRInput = ButtonConfigs.VRButtons[(idx + 1) % ButtonConfigs.VRButtons.Length];
                        leftVRBtn.Name = $"Left Hand: {leftBind.GetVRInputName()}";
                        Core.RebuildMenu();
                    },
                    Down = () =>
                    {
                        int idx = Array.IndexOf(ButtonConfigs.VRButtons, leftBind.VRInput);
                        leftBind.VRInput = ButtonConfigs.VRButtons[(idx - 1 + ButtonConfigs.VRButtons.Length) % ButtonConfigs.VRButtons.Length];
                        leftVRBtn.Name = $"Left Hand: {leftBind.GetVRInputName()}";
                        Core.RebuildMenu();
                    }
                };
                leftBind.BtnVR = leftVRBtn;
                platformsConfig.Buttons.Add(leftVRBtn);
            }

            if (platGroup != null && platGroup.Binds.Count >= 2)
            {
                var rightBind = platGroup.Binds[1];
                Button rightVRBtn = null;
                rightVRBtn = new Button
                {
                    Name = $"Right Hand: {rightBind.GetVRInputName()}",
                    Toggle = false,
                    Incremental = true,
                    Up = () =>
                    {
                        int idx = Array.IndexOf(ButtonConfigs.VRButtons, rightBind.VRInput);
                        rightBind.VRInput = ButtonConfigs.VRButtons[(idx + 1) % ButtonConfigs.VRButtons.Length];
                        rightVRBtn.Name = $"Right Hand: {rightBind.GetVRInputName()}";
                        Core.RebuildMenu();
                    },
                    Down = () =>
                    {
                        int idx = Array.IndexOf(ButtonConfigs.VRButtons, rightBind.VRInput);
                        rightBind.VRInput = ButtonConfigs.VRButtons[(idx - 1 + ButtonConfigs.VRButtons.Length) % ButtonConfigs.VRButtons.Length];
                        rightVRBtn.Name = $"Right Hand: {rightBind.GetVRInputName()}";
                        Core.RebuildMenu();
                    }
                };
                rightBind.BtnVR = rightVRBtn;
                platformsConfig.Buttons.Add(rightVRBtn);
            }

            Category flightKeybinds = ButtonConfigs.Get("Flight").GenerateCategory();
            flightKeybinds.Buttons.Add(FlySpeedButton);
            flightConfig = flightKeybinds;

            nameTagsConfig = new Category { 
                Name = "Name Tags Settings", 
                Buttons = { 
                    new Button { Name = "Advanced Nametags", Toggle = true, OnceEnable = () => Visual.AdvancedNametags = true, OnceDisable = () => Visual.AdvancedNametags = false } 
                } 
            };
        }

        public static Category GetCategory(string name)
        {
            if (Buttons.Modules == null) return null;
            return Buttons.Modules.FirstOrDefault(c => c.Name == name);
        }

        public static void GetButtonsRecursive(Category category, List<Button> allButtons)
        {
            if (category == null) return;
            if (category.Buttons != null)
                allButtons.AddRange(category.Buttons);
            if (category.Subcategories != null)
            {
                foreach (var sub in category.Subcategories)
                {
                    GetButtonsRecursive(sub, allButtons);
                }
            }
        }

        public static Button GetButton(string name)
        {
            if (Buttons.Modules == null) return null;
            List<Button> allButtons = new List<Button>();
            foreach (var category in Buttons.Modules)
            {
                GetButtonsRecursive(category, allButtons);
            }
            return allButtons.FirstOrDefault(b => b.Name == name);
        }

        public static int CountAllButtons()
        {
            if (Buttons.Modules == null) return 0;
            List<Button> allButtons = new List<Button>();
            foreach (var category in Buttons.Modules)
            {
                GetButtonsRecursive(category, allButtons);
            }
            return allButtons.Count;
        }

        public static void RefreshEnabledCategory()
        {
            if (Buttons.Modules == null || EnabledCategory == null) return;
            EnabledCategory.Buttons.Clear();
            for (int i = 0; i < Buttons.Modules.Length; i++)
            {
                if (Buttons.Modules[i] == EnabledCategory) continue;
                List<Button> catButtons = new List<Button>();
                GetButtonsRecursive(Buttons.Modules[i], catButtons);
                for (int j = 0; j < catButtons.Count; j++)
                {
                    Button btn = catButtons[j];
                    if (btn.Toggle && btn.Enabled)
                    {
                        EnabledCategory.Buttons.Add(btn);
                    }
                }
            }
        }
    }
}

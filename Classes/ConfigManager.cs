using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Juul
{
    public class SettingsConfig
    {
        public int ThemeIndex;
        public int PageButtons;
        public float MenuScale;
        public float ButtonInset;
        public float TextSize;
        public bool IsOutlined;
        public bool IsCatRotated;
        public bool IsCatLeft;
        public bool IsRightHanded;
        public bool IsRounded;
        public bool IsAnimated;
        public bool IsMenuParticles;
        public bool IsBoardGradientEnabled;
        public bool MenuSmoothing;
        public int GunStyle;
        public float GunLineWidth;
        public float GunSphereSize;
        public bool AdvancedNametags;
        public bool DisableNotifications;
        public bool CsProjectiles;
        public string AutoLoadPresetName;
    }

    [Serializable]
    public class ModPreset
    {
        public List<string> EnabledMods = new List<string>();
        public int FlySpeedIndex = 1;
        public int BoostSpeedIndex = 2;
        public int WallWalkIndex = 2;
        public List<KeyBindData> Keybinds = new List<KeyBindData>();
    }

    [Serializable]
    public class KeyBindData
    {
        public string ModName;
        public string BindName;
        public int PCKey;
        public int VRInput;
        public bool IsToggle;
        public bool Enabled;
    }

    public static class Configs
    {
        public static string AutoLoadPresetName = "";

        public static string ConfigPath =>
            Path.Combine(Directory.GetParent(Application.dataPath).FullName, Core.Folder, "Configs");

        private static readonly string SettingsPath =
            Path.Combine(ConfigPath, "settings.cfg");

        private static readonly string PresetsPath =
            Path.Combine(ConfigPath, "modpreset.cfg");


        private static string PathCfg => SettingsPath;

        public static bool ConfigExists() =>
            Directory.Exists(ConfigPath) && File.Exists(SettingsPath);

        public static void SaveConfig()
        {
            SaveSettingsConfig();
        }

        public static void LoadConfig()
        {
            LoadSettingsConfig();
        }

        public static void ResetToDefault()
        {
            Core.ThemeValue = 0;
            Core.PageBtnVer = 2;
            Core.MenuWidth = 0.8f;
            Core.BtnInset = 0.1f;
            Core.TextSize = 0.5f;
            Core.IsOutlined = true;
            Core.IsCatRotated = true;
            Core.IsCatLeft = true;
            Core.IsRightHanded = false;
            Core.IsRounded = true;
            Core.IsAnimated = true;
            Core.IsMenuParticles = false;
            Core.IsBoardGradientEnabled = true;
            Core.MenuSmoothing = true;
            GunLib.currentLineStyle = GunLib.GunLineStyle.Normal;
            GunLib.GunLineWidth = 0.01f;
            GunLib.SphereSize = 0.04f;
            Visual.AdvancedNametags = false;
            NotifiLib.Disablenotifcations = false;
            AutoLoadPresetName = "";

            if (ExtraButtons.ThemeButton != null)
                ExtraButtons.ThemeButton.Name = $"Theme: {Core.GetCurrentThemeName()}";
            if (ExtraButtons.GunStyleButton != null)
                ExtraButtons.GunStyleButton.Name = "Gun Style: " + GunLib.currentLineStyle.ToString();
            if (ExtraButtons.GunLineSizeButton != null)
                ExtraButtons.GunLineSizeButton.Name = string.Format("Gun Line Size: {0}", GunLib.GunLineWidth);
            if (ExtraButtons.GunSphereSizeButton != null)
                ExtraButtons.GunSphereSizeButton.Name = string.Format("Gun Sphere Size: {0}", GunLib.SphereSize);

            Core.RebuildMenu();
            NotifiLib.SendNotification("<color=yellow>[CONFIGS]</color> Reset to defaults.");
        }

        public static void SaveSettingsConfig()
        {
            if (!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);

            var cfg = new SettingsConfig
            {
                ThemeIndex = Core.ThemeValue,
                PageButtons = Core.PageBtnVer,
                MenuScale = Core.MenuWidth,
                ButtonInset = Core.BtnInset,
                TextSize = Core.TextSize,
                IsOutlined = Core.IsOutlined,
                IsCatRotated = Core.IsCatRotated,
                IsCatLeft = Core.IsCatLeft,
                IsRightHanded = Core.IsRightHanded,
                IsRounded = Core.IsRounded,
                IsAnimated = Core.IsAnimated,
                IsMenuParticles = Core.IsMenuParticles,
                IsBoardGradientEnabled = Core.IsBoardGradientEnabled,
                MenuSmoothing = Core.MenuSmoothing,
                GunStyle = (int)GunLib.currentLineStyle,
                GunLineWidth = GunLib.GunLineWidth,
                GunSphereSize = GunLib.SphereSize,
                AdvancedNametags = Visual.AdvancedNametags,
                DisableNotifications = NotifiLib.Disablenotifcations,
                AutoLoadPresetName = Configs.AutoLoadPresetName,
            };
            File.WriteAllText(SettingsPath, JsonUtility.ToJson(cfg, true));
            NotifiLib.SendNotification("<color=green>[CONFIGS]</color> Settings saved.");
        }

        public static void LoadSettingsConfig()
        {
            if (!File.Exists(SettingsPath)) return;
            string json = File.ReadAllText(SettingsPath);
            if (string.IsNullOrEmpty(json)) return;
            SettingsConfig cfg = JsonUtility.FromJson<SettingsConfig>(json);
            if (cfg == null) return;

            Core.ThemeValue = cfg.ThemeIndex;
            Core.PageBtnVer = cfg.PageButtons;
            Core.MenuWidth = cfg.MenuScale;
            Core.BtnInset = cfg.ButtonInset;
            Core.TextSize = cfg.TextSize;
            Core.IsOutlined = cfg.IsOutlined;
            Core.IsCatRotated = cfg.IsCatRotated;
            Core.IsCatLeft = cfg.IsCatLeft;
            Core.IsRightHanded = cfg.IsRightHanded;
            Core.IsRounded = cfg.IsRounded;
            Core.IsAnimated = cfg.IsAnimated;
            Core.IsMenuParticles = cfg.IsMenuParticles;
            Core.IsBoardGradientEnabled = cfg.IsBoardGradientEnabled;
            Core.MenuSmoothing = cfg.MenuSmoothing;
            GunLib.currentLineStyle = (GunLib.GunLineStyle)cfg.GunStyle;
            GunLib.GunLineWidth = cfg.GunLineWidth;
            GunLib.SphereSize = cfg.GunSphereSize;
            Visual.AdvancedNametags = cfg.AdvancedNametags;
            NotifiLib.Disablenotifcations = cfg.DisableNotifications;
            Configs.AutoLoadPresetName = cfg.AutoLoadPresetName;

            if (!string.IsNullOrEmpty(Configs.AutoLoadPresetName))
            {
                Configs.LoadModPresetWithName(Configs.AutoLoadPresetName);
            }

            if (ExtraButtons.ThemeButton != null)
                ExtraButtons.ThemeButton.Name = $"Theme: {Core.GetCurrentThemeName()}";
            if (ExtraButtons.GunStyleButton != null)
                ExtraButtons.GunStyleButton.Name = "Gun Style: " + GunLib.currentLineStyle.ToString();
            if (ExtraButtons.GunLineSizeButton != null)
                ExtraButtons.GunLineSizeButton.Name = string.Format("Gun Line Size: {0}", GunLib.GunLineWidth);
            if (ExtraButtons.GunSphereSizeButton != null)
                ExtraButtons.GunSphereSizeButton.Name = string.Format("Gun Sphere Size: {0}", GunLib.SphereSize);

            Core.RebuildMenu();
            NotifiLib.SendNotification("<color=green>[CONFIGS]</color> Settings loaded.");
        }
        public static string[] GetAvailablePresets()
        {
            if (!Directory.Exists(ConfigPath)) return new string[0];
            return Directory.GetFiles(ConfigPath, "Preset_*.cfg")
                            .Select(Path.GetFileNameWithoutExtension)
                            .Select(name => name.Length > 7 ? name.Substring(7) : name)
                            .ToArray();
        }

        public static void SaveModPresetWithName(string name)
        {
            if (Buttons.Modules == null || string.IsNullOrEmpty(name)) return;
            if (!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);

            var preset = new ModPreset();
            foreach (Category cat in Buttons.Modules)
            {
                CollectEnabledButtons(cat, preset.EnabledMods);
            }

            preset.FlySpeedIndex = Movement.speedIndex;
            preset.BoostSpeedIndex = Movement.speedIndex2;
            preset.WallWalkIndex = Movement.wallForceIndex;

            preset.Keybinds = CollectKeybinds();

            string path = Path.Combine(ConfigPath, $"Preset_{name}.cfg");
            File.WriteAllText(path, JsonUtility.ToJson(preset, true));
            NotifiLib.SendNotification($"<color=green>[PRESETS]</color> Saved preset '{name}' with {preset.EnabledMods.Count} mod(s).");
        }

        public static void LoadModPresetWithName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            string path = Path.Combine(ConfigPath, $"Preset_{name}.cfg");
            if (!File.Exists(path)) return;
            
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return;
            ModPreset preset = JsonUtility.FromJson<ModPreset>(json);
            if (preset == null || Buttons.Modules == null) return;

            int loaded = 0;
            foreach (Category cat in Buttons.Modules)
            {
                loaded += ApplyPresetToCategory(cat, preset.EnabledMods);
            }

            RestoreSpeedIndex(preset.FlySpeedIndex, preset.BoostSpeedIndex, preset.WallWalkIndex);
            RestoreKeybinds(preset.Keybinds);

            Core.RebuildMenu();
            NotifiLib.SendNotification($"<color=green>[PRESETS]</color> Restored '{name}' with {loaded} mod(s).");
        }

        private static void CollectEnabledButtons(Category cat, List<string> list)
        {
            if (cat.Name == "Configs" || cat.Name == "Menu Settings" || cat.Name == "Room" || cat.Name == "Plugins" || cat.Name == "Credits" || cat.Name == "Enabled" || cat.Name == "Players") return;

            foreach (Button btn in cat.Buttons)
            {
                if (btn.Toggle && btn.Enabled && !btn.Label)
                    list.Add(cat.Name + "/" + btn.Name);
            }
            foreach (Category sub in cat.Subcategories)
                CollectEnabledButtons(sub, list);
        }

        private static int ApplyPresetToCategory(Category cat, List<string> enabledMods)
        {
            if (cat.Name == "Configs" || cat.Name == "Menu Settings" || cat.Name == "Room" || cat.Name == "Plugins" || cat.Name == "Credits" || cat.Name == "Enabled" || cat.Name == "Players") return 0;

            int count = 0;
            foreach (Button btn in cat.Buttons)
            {
                if (!btn.Toggle || btn.Label) continue;
                string key = cat.Name + "/" + btn.Name;
                bool shouldEnable = enabledMods.Contains(key);
                if (shouldEnable && !btn.Enabled)
                {
                    btn.Enabled = true;
                    btn.OnceEnable?.Invoke();
                    count++;
                }
                else if (!shouldEnable && btn.Enabled)
                {
                    btn.Enabled = false;
                    btn.OnceDisable?.Invoke();
                }
            }
            foreach (Category sub in cat.Subcategories)
                count += ApplyPresetToCategory(sub, enabledMods);
            return count;
        }

        private static List<KeyBindData> CollectKeybinds()
        {
            var list = new List<KeyBindData>();
            var allGroups = new string[] { "Noclip", "Speed Boost", "Platforms", "Wall Walk", "Check Point", "Flight" };
            foreach (var modName in allGroups)
            {
                var group = ButtonConfigs.Get(modName);
                if (group == null) continue;
                foreach (var bind in group.Binds)
                {
                    list.Add(new KeyBindData
                    {
                        ModName = modName,
                        BindName = bind.Name,
                        PCKey = (int)bind.PCKey,
                        VRInput = (int)bind.VRInput,
                        IsToggle = bind.IsToggle,
                        Enabled = bind.Enabled
                    });
                }
            }
            return list;
        }

        private static void RestoreKeybinds(List<KeyBindData> keybinds)
        {
            if (keybinds == null) return;
            foreach (var data in keybinds)
            {
                var group = ButtonConfigs.Get(data.ModName);
                if (group == null) continue;
                foreach (var bind in group.Binds)
                {
                    if (bind.Name == data.BindName)
                    {
                        bind.PCKey = (KeyCode)data.PCKey;
                        bind.VRInput = (VRButton)data.VRInput;
                        bind.IsToggle = data.IsToggle;
                        bind.Enabled = data.Enabled;
                        break;
                    }
                }
            }
            ButtonConfigs.RefreshAllBindButtons();
        }

        private static void RestoreSpeedIndex(int flyIndex, int boostIndex, int wallIndex)
        {
            if (flyIndex >= 0 && flyIndex < Movement.speedOptions.Length)
            {
                Movement.speedIndex = flyIndex;
                Movement.flyspeed = Movement.speedOptions[flyIndex];
                if (ExtraButtons.FlySpeedButton != null)
                    ExtraButtons.FlySpeedButton.Name = $"Fly Speed: {Movement.speedNames[flyIndex]}";
            }
            if (boostIndex >= 0 && boostIndex < Movement.speedOptions2.Length)
            {
                Movement.speedIndex2 = boostIndex;
                Movement.jumpspeed = Movement.speedOptions2[boostIndex];
                Movement.jumpmultiplier = Movement.multiplierOptions[boostIndex];
                if (ExtraButtons.SpeedBoostSpeedButton != null)
                    ExtraButtons.SpeedBoostSpeedButton.Name = $"Boost: {Movement.speedNames2[boostIndex]}";
            }
            if (wallIndex >= 0 && wallIndex < Movement.wallForces.Length)
            {
                Movement.wallForceIndex = wallIndex;
                Movement.activeWallForce = Movement.wallForces[wallIndex];
                if (ExtraButtons.WallWalkStrengthButton != null)
                    ExtraButtons.WallWalkStrengthButton.Name = $"Wall Walk: {Movement.wallForceNames[wallIndex]}";
            }
        }
    }
}





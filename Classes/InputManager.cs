using GorillaLocomotion;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Valve.VR;

namespace Juul
{
    [HarmonyPatch(typeof(GTPlayer))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public class Inputs : MonoBehaviour
    {
        public static void Prefix()
        {
            if (ControllerInputPoller.instance == null) return;

            Inputs.LeftPrimary = ControllerInputPoller.instance.leftControllerPrimaryButton;
            Inputs.RightPrimary = ControllerInputPoller.instance.rightControllerPrimaryButton;

            Inputs.LeftSecondary = ControllerInputPoller.instance.leftControllerSecondaryButton;
            Inputs.RightSecondary = ControllerInputPoller.instance.rightControllerSecondaryButton;

            Inputs.LeftGrip = ControllerInputPoller.instance.leftGrab;
            Inputs.RightGrip = ControllerInputPoller.instance.rightGrab;

            Inputs.LeftTrigger = ControllerInputPoller.instance.leftControllerIndexFloat >= 0.5f;
            Inputs.RightTrigger = ControllerInputPoller.instance.rightControllerIndexFloat >= 0.5f;

            try
            {
                Inputs.LeftJoystick = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis(SteamVR_Input_Sources.LeftHand).magnitude > 0.1f;
                Inputs.RightJoystick = SteamVR_Actions.gorillaTag_RightJoystick2DAxis.GetAxis(SteamVR_Input_Sources.RightHand).magnitude > 0.1f;
            }
            catch
            {
                Inputs.LeftJoystick = false;
                Inputs.RightJoystick = false;
            }

            ButtonConfigs.UpdateAll();
        }

        public static bool LeftPrimary;
        public static bool RightPrimary;

        public static bool LeftSecondary;
        public static bool RightSecondary;

        public static bool LeftGrip;
        public static bool RightGrip;

        public static bool LeftTrigger;
        public static bool RightTrigger;

        public static bool LeftJoystick;
        public static bool RightJoystick;
    }

    public enum VRButton
    {
        None,
        LeftPrimary,
        RightPrimary,
        LeftSecondary,
        RightSecondary,
        LeftGrip,
        RightGrip,
        LeftTrigger,
        RightTrigger
    }

    public class KeyBind
    {
        public string Name = "Keybind";
        public KeyCode PCKey = KeyCode.None;
        public VRButton VRInput = VRButton.None;
        public bool HasPCBind = true;
        public bool Enabled = true;
        public bool IsToggle = false;
        
        [HideInInspector] public bool IsSingleBind = false;
        public string ButtonPrefix => IsSingleBind ? "" : $"{Name} ";
        
        public Button BtnMode;
        public Button BtnPC;
        public Button BtnVR;

        private bool previousRaw = false;
        private bool currentRaw = false;
        private bool toggleState = false;

        public void Update()
        {
            previousRaw = currentRaw;
            currentRaw = ReadRaw();

            if (IsToggle && currentRaw && !previousRaw)
            {
                toggleState = !toggleState;
            }
        }

        private bool ReadRaw()
        {
            if (!Enabled) return false;

            bool isVR = UnityEngine.XR.XRSettings.isDeviceActive;

            if (!isVR && HasPCBind && PCKey != KeyCode.None)
            {
                try
                {
                    var kb = Keyboard.current;
                    if (kb != null)
                    {
                        Key isKey = ButtonConfigs.KeyCodeToInputSystemKey(PCKey);
                        if (isKey != Key.None && kb[isKey].isPressed)
                            return true;
                    }
                }
                catch { }
            }

            if (isVR)
            {
                switch (VRInput)
                {
                    case VRButton.LeftPrimary: return Inputs.LeftPrimary;
                    case VRButton.RightPrimary: return Inputs.RightPrimary;
                    case VRButton.LeftSecondary: return Inputs.LeftSecondary;
                    case VRButton.RightSecondary: return Inputs.RightSecondary;
                    case VRButton.LeftGrip: return Inputs.LeftGrip;
                    case VRButton.RightGrip: return Inputs.RightGrip;
                    case VRButton.LeftTrigger: return Inputs.LeftTrigger;
                    case VRButton.RightTrigger: return Inputs.RightTrigger;
                }
            }

            return false;
        }

        public bool IsPressed()
        {
            if (!Enabled) return false;
            if (IsToggle) return toggleState;
            return currentRaw;
        }

        public bool WasPressed() => currentRaw && !previousRaw;
        public bool WasReleased() => !currentRaw && previousRaw;

        public string GetPCKeyName()
        {
            if (PCKey == KeyCode.None) return "None";
            return PCKey.ToString();
        }

        public string GetVRInputName()
        {
            return ButtonConfigs.VRButtonNames.TryGetValue(VRInput, out string name) ? name : "None";
        }

        public string GetModeName()
        {
            return IsToggle ? "Toggle" : "Hold";
        }
    }

    public class KeyBindGroup
    {
        public string ModName;
        public List<KeyBind> Binds = new List<KeyBind>();

        public Category GenerateCategory()
        {
            Category cat = new Category { Name = ModName + " Settings" };

            if (Binds.Count > 0)
            {
                Button groupModeBtn = new Button
                {
                    Name = $"Mode: {Binds[0].GetModeName()}",
                    Toggle = false,
                    OnEnable = () =>
                    {
                        bool newState = !Binds[0].IsToggle;
                        foreach (var bind in Binds)
                        {
                            bind.IsToggle = newState;
                        }
                        ButtonConfigs.RefreshAllBindButtons();
                        Core.RebuildMenu();
                    }
                };
                cat.Buttons.Add(groupModeBtn);

                foreach (var bind in Binds)
                {
                    bind.BtnMode = groupModeBtn;
                }
            }

            foreach (var bind in Binds)
            {
                var b = bind;
                b.IsSingleBind = (Binds.Count == 1);

                if (bind.HasPCBind)
                {
                    var pcBind = bind;
                    pcBind.BtnPC = new Button
                    {
                        Name = $"{pcBind.ButtonPrefix}PC: {pcBind.GetPCKeyName()}",
                        Toggle = false,
                        OnEnable = () =>
                        {
                            ButtonConfigs.StartListening(pcBind, true);
                        }
                    };
                    cat.Buttons.Add(pcBind.BtnPC);
                }

                var vrBind = bind;
                vrBind.BtnVR = new Button
                {
                    Name = $"{vrBind.ButtonPrefix}VR: {vrBind.GetVRInputName()}",
                    Toggle = false,
                    OnEnable = () =>
                    {
                        ButtonConfigs.StartListening(vrBind, false);
                    }
                };
                cat.Buttons.Add(vrBind.BtnVR);
            }

            return cat;
        }
    }

    public static class ButtonConfigs
    {
        private static Dictionary<string, KeyBindGroup> groups = new Dictionary<string, KeyBindGroup>();

        public static bool IsListening = false;
        public static bool WasListeningLastFrame = false;
        public static KeyBind ListeningBind = null;
        public static bool ListeningForPC = false;
        private static float listenStartTime = 0f;
        private static bool listenFrameSkip = false;

        public static bool KeepMenuOpen => IsListening || graceActive;
        private static bool graceActive = false;
        private static float graceEndTime = 0f;
        private static float graceDuration = 1f;

        public static readonly VRButton[] VRButtons = new VRButton[]
        {
            VRButton.None,
            VRButton.LeftPrimary,
            VRButton.RightPrimary,
            VRButton.LeftSecondary,
            VRButton.RightSecondary,
            VRButton.LeftGrip,
            VRButton.RightGrip,
            VRButton.LeftTrigger,
            VRButton.RightTrigger
        };

        public static readonly Dictionary<VRButton, string> VRButtonNames = new Dictionary<VRButton, string>
        {
            { VRButton.None, "None" },
            { VRButton.LeftPrimary, "L Primary" },
            { VRButton.RightPrimary, "R Primary" },
            { VRButton.LeftSecondary, "L Secondary" },
            { VRButton.RightSecondary, "R Secondary" },
            { VRButton.LeftGrip, "L Grip" },
            { VRButton.RightGrip, "R Grip" },
            { VRButton.LeftTrigger, "L Trigger" },
            { VRButton.RightTrigger, "R Trigger" }
        };

        public static void Register(string modName, params KeyBind[] binds)
        {
            var group = new KeyBindGroup { ModName = modName };
            group.Binds.AddRange(binds);
            groups[modName] = group;
        }

        public static bool IsPressed(string modName, int bindIndex = 0)
        {
            var group = Get(modName);
            if (group != null && bindIndex < group.Binds.Count)
                return group.Binds[bindIndex].IsPressed();
            return false;
        }

        public static bool WasPressed(string modName, int bindIndex = 0)
        {
            var group = Get(modName);
            if (group != null && bindIndex < group.Binds.Count)
                return group.Binds[bindIndex].WasPressed();
            return false;
        }

        public static KeyBindGroup Get(string modName)
        {
            if (groups.TryGetValue(modName, out var group))
                return group;
            return null;
        }

        public static void StartListening(KeyBind bind, bool forPC)
        {
            IsListening = true;
            ListeningBind = bind;
            ListeningForPC = forPC;
            listenStartTime = Time.time;
            listenFrameSkip = true;

            RefreshBindButtons(bind, true);
            Core.RebuildMenu();
        }

        public static void StopListening()
        {
            if (ListeningBind != null)
            {
                RefreshBindButtons(ListeningBind, false);
            }
            IsListening = false;
            graceActive = true;
            graceEndTime = Time.time + graceDuration;
            ListeningBind = null;
            listenFrameSkip = false;
        }

        private static bool hasLoggedError = false;
        public static void UpdateAll()
        {
            try
            {
                if (graceActive && Time.time >= graceEndTime)
                {
                    graceActive = false;
                }

                if (groups != null)
                {
                    foreach (var group in groups.Values)
                    {
                        if (group == null || group.Binds == null) continue;
                        foreach (var bind in group.Binds)
                        {
                            if (bind == null) continue;
                            bind.Update();
                        }
                    }
                }

                if (IsListening && ListeningBind != null)
                {
                    if (listenFrameSkip)
                    {
                        listenFrameSkip = false;
                        return;
                    }

                    if (Time.time - listenStartTime > 5f)
                    {
                        StopListening();
                        Core.RebuildMenu();
                        return;
                    }

                    if (Time.time - listenStartTime < 1.0f) return;

                    if (ListeningForPC)
                    {
                        try
                        {
                            if (Keyboard.current != null)
                            {
                                var kb = Keyboard.current;
                                if (kb.escapeKey.wasPressedThisFrame)
                                {
                                    ListeningBind.PCKey = KeyCode.None;
                                    StopListening();
                                    Core.RebuildMenu();
                                    return;
                                }

                                foreach (Key k in Enum.GetValues(typeof(Key)))
                                {
                                    if (k != Key.None && k != Key.Escape && kb[k].wasPressedThisFrame)
                                    {
                                        ListeningBind.PCKey = InputSystemKeyToKeyCode(k);
                                        StopListening();
                                        Core.RebuildMenu();
                                        return;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        if (Inputs.LeftPrimary) { ListeningBind.VRInput = VRButton.LeftPrimary; StopListening(); Core.RebuildMenu(); return; }
                        if (Inputs.RightPrimary) { ListeningBind.VRInput = VRButton.RightPrimary; StopListening(); Core.RebuildMenu(); return; }
                        if (Inputs.LeftSecondary) { ListeningBind.VRInput = VRButton.LeftSecondary; StopListening(); Core.RebuildMenu(); return; }
                        if (Inputs.RightSecondary) { ListeningBind.VRInput = VRButton.RightSecondary; StopListening(); Core.RebuildMenu(); return; }
                        if (Inputs.LeftGrip) { ListeningBind.VRInput = VRButton.LeftGrip; StopListening(); Core.RebuildMenu(); return; }
                        if (Inputs.RightGrip) { ListeningBind.VRInput = VRButton.RightGrip; StopListening(); Core.RebuildMenu(); return; }
                        if (Inputs.LeftTrigger) { ListeningBind.VRInput = VRButton.LeftTrigger; StopListening(); Core.RebuildMenu(); return; }
                        if (Inputs.RightTrigger) { ListeningBind.VRInput = VRButton.RightTrigger; StopListening(); Core.RebuildMenu(); return; }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!hasLoggedError)
                {
                    UnityEngine.Debug.LogError("ButtonConfigs UpdateAll Error: " + ex);
                    hasLoggedError = true;
                }
            }
        }

        public static KeyCode InputSystemKeyToKeyCode(UnityEngine.InputSystem.Key key)
        {
            switch (key)
            {
                case UnityEngine.InputSystem.Key.A: return KeyCode.A;
                case UnityEngine.InputSystem.Key.B: return KeyCode.B;
                case UnityEngine.InputSystem.Key.C: return KeyCode.C;
                case UnityEngine.InputSystem.Key.D: return KeyCode.D;
                case UnityEngine.InputSystem.Key.E: return KeyCode.E;
                case UnityEngine.InputSystem.Key.F: return KeyCode.F;
                case UnityEngine.InputSystem.Key.G: return KeyCode.G;
                case UnityEngine.InputSystem.Key.H: return KeyCode.H;
                case UnityEngine.InputSystem.Key.I: return KeyCode.I;
                case UnityEngine.InputSystem.Key.J: return KeyCode.J;
                case UnityEngine.InputSystem.Key.K: return KeyCode.K;
                case UnityEngine.InputSystem.Key.L: return KeyCode.L;
                case UnityEngine.InputSystem.Key.M: return KeyCode.M;
                case UnityEngine.InputSystem.Key.N: return KeyCode.N;
                case UnityEngine.InputSystem.Key.O: return KeyCode.O;
                case UnityEngine.InputSystem.Key.P: return KeyCode.P;
                case UnityEngine.InputSystem.Key.Q: return KeyCode.Q;
                case UnityEngine.InputSystem.Key.R: return KeyCode.R;
                case UnityEngine.InputSystem.Key.S: return KeyCode.S;
                case UnityEngine.InputSystem.Key.T: return KeyCode.T;
                case UnityEngine.InputSystem.Key.U: return KeyCode.U;
                case UnityEngine.InputSystem.Key.V: return KeyCode.V;
                case UnityEngine.InputSystem.Key.W: return KeyCode.W;
                case UnityEngine.InputSystem.Key.X: return KeyCode.X;
                case UnityEngine.InputSystem.Key.Y: return KeyCode.Y;
                case UnityEngine.InputSystem.Key.Z: return KeyCode.Z;
                case UnityEngine.InputSystem.Key.Digit1: return KeyCode.Alpha1;
                case UnityEngine.InputSystem.Key.Digit2: return KeyCode.Alpha2;
                case UnityEngine.InputSystem.Key.Digit3: return KeyCode.Alpha3;
                case UnityEngine.InputSystem.Key.Digit4: return KeyCode.Alpha4;
                case UnityEngine.InputSystem.Key.Digit5: return KeyCode.Alpha5;
                case UnityEngine.InputSystem.Key.Digit6: return KeyCode.Alpha6;
                case UnityEngine.InputSystem.Key.Digit7: return KeyCode.Alpha7;
                case UnityEngine.InputSystem.Key.Digit8: return KeyCode.Alpha8;
                case UnityEngine.InputSystem.Key.Digit9: return KeyCode.Alpha9;
                case UnityEngine.InputSystem.Key.Digit0: return KeyCode.Alpha0;
                case UnityEngine.InputSystem.Key.F1: return KeyCode.F1;
                case UnityEngine.InputSystem.Key.F2: return KeyCode.F2;
                case UnityEngine.InputSystem.Key.F3: return KeyCode.F3;
                case UnityEngine.InputSystem.Key.F4: return KeyCode.F4;
                case UnityEngine.InputSystem.Key.F5: return KeyCode.F5;
                case UnityEngine.InputSystem.Key.F6: return KeyCode.F6;
                case UnityEngine.InputSystem.Key.F7: return KeyCode.F7;
                case UnityEngine.InputSystem.Key.F8: return KeyCode.F8;
                case UnityEngine.InputSystem.Key.F9: return KeyCode.F9;
                case UnityEngine.InputSystem.Key.F10: return KeyCode.F10;
                case UnityEngine.InputSystem.Key.LeftShift: return KeyCode.LeftShift;
                case UnityEngine.InputSystem.Key.RightShift: return KeyCode.RightShift;
                case UnityEngine.InputSystem.Key.LeftCtrl: return KeyCode.LeftControl;
                case UnityEngine.InputSystem.Key.RightCtrl: return KeyCode.RightControl;
                case UnityEngine.InputSystem.Key.LeftAlt: return KeyCode.LeftAlt;
                case UnityEngine.InputSystem.Key.RightAlt: return KeyCode.RightAlt;
                case UnityEngine.InputSystem.Key.Space: return KeyCode.Space;
                case UnityEngine.InputSystem.Key.Tab: return KeyCode.Tab;
                case UnityEngine.InputSystem.Key.CapsLock: return KeyCode.CapsLock;
                default: return KeyCode.None;
            }
        }

        public static Key KeyCodeToInputSystemKey(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.A: return Key.A;
                case KeyCode.B: return Key.B;
                case KeyCode.C: return Key.C;
                case KeyCode.D: return Key.D;
                case KeyCode.E: return Key.E;
                case KeyCode.F: return Key.F;
                case KeyCode.G: return Key.G;
                case KeyCode.H: return Key.H;
                case KeyCode.I: return Key.I;
                case KeyCode.J: return Key.J;
                case KeyCode.K: return Key.K;
                case KeyCode.L: return Key.L;
                case KeyCode.M: return Key.M;
                case KeyCode.N: return Key.N;
                case KeyCode.O: return Key.O;
                case KeyCode.P: return Key.P;
                case KeyCode.Q: return Key.Q;
                case KeyCode.R: return Key.R;
                case KeyCode.S: return Key.S;
                case KeyCode.T: return Key.T;
                case KeyCode.U: return Key.U;
                case KeyCode.V: return Key.V;
                case KeyCode.W: return Key.W;
                case KeyCode.X: return Key.X;
                case KeyCode.Y: return Key.Y;
                case KeyCode.Z: return Key.Z;
                case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2;
                case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4;
                case KeyCode.Alpha5: return Key.Digit5;
                case KeyCode.Alpha6: return Key.Digit6;
                case KeyCode.Alpha7: return Key.Digit7;
                case KeyCode.Alpha8: return Key.Digit8;
                case KeyCode.Alpha9: return Key.Digit9;
                case KeyCode.Alpha0: return Key.Digit0;
                case KeyCode.F1: return Key.F1;
                case KeyCode.F2: return Key.F2;
                case KeyCode.F3: return Key.F3;
                case KeyCode.F4: return Key.F4;
                case KeyCode.F5: return Key.F5;
                case KeyCode.F6: return Key.F6;
                case KeyCode.F7: return Key.F7;
                case KeyCode.F8: return Key.F8;
                case KeyCode.F9: return Key.F9;
                case KeyCode.F10: return Key.F10;
                case KeyCode.LeftShift: return Key.LeftShift;
                case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.RightControl: return Key.RightCtrl;
                case KeyCode.LeftAlt: return Key.LeftAlt;
                case KeyCode.RightAlt: return Key.RightAlt;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.CapsLock: return Key.CapsLock;
                default: return Key.None;
            }
        }

        public static void RefreshAllBindButtons()
        {
            foreach (var group in groups.Values)
            {
                foreach (var bind in group.Binds)
                {
                    if (bind.BtnMode != null) bind.BtnMode.Name = $"Mode: {bind.GetModeName()}";
                    if (bind.BtnPC != null) bind.BtnPC.Name = $"{bind.ButtonPrefix}PC: {bind.GetPCKeyName()}";
                    if (bind.BtnVR != null) bind.BtnVR.Name = $"{bind.ButtonPrefix}VR: {bind.GetVRInputName()}";
                }
            }
        }

        private static void RefreshBindButtons(KeyBind bind, bool listening)
        {
            if (bind.BtnPC != null)
            {
                bind.BtnPC.Name = listening && ListeningForPC
                    ? $"{bind.ButtonPrefix}PC: [Press a key...]"
                    : $"{bind.ButtonPrefix}PC: {bind.GetPCKeyName()}";
            }
            if (bind.BtnVR != null)
            {
                bind.BtnVR.Name = listening && !ListeningForPC
                    ? $"{bind.ButtonPrefix}VR: [Press a button...]"
                    : $"{bind.ButtonPrefix}VR: {bind.GetVRInputName()}";
            }
        }
    }
}


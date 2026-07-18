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
    public class Buttons
    {
        private static bool initialized = false;

        public static Category[] Modules = null;

        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            ExtraButtons.Initialize();

            Modules = new Category[]
            {
                new Category {
                    Name = "Settings",
                    Buttons = {
                        new Button { Name = "discord.gg/juul", Toggle = false, Label = true },
                        new Button { Name = "Join Discord! >.<", Toggle = false, OnEnable = () => UnityEngine.Application.OpenURL("https://discord.gg/juul") },
                    },
                    Subcategories = {
                        new Category {
                            Name = "Configs",
                            Subcategories = {
                                new Category {
                                    Name = "Settings",
                                    Buttons = {
                                        new Button { Name = "Save Settings", Toggle = false, OnEnable = () => Configs.SaveSettingsConfig() },
                                        new Button { Name = "Load Settings", Toggle = false, OnEnable = () => Configs.LoadSettingsConfig() },
                                        new Button { Name = "Reset To Default", Toggle = false, OnEnable = () => Configs.ResetToDefault() },
                                    }
                                },
                                new Category {
                                    Name = "Mod Presets",
                                    Buttons = {
                                        ExtraButtons.SavePresetButton,
                                        ExtraButtons.LoadPresetButton,
                                        ExtraButtons.AutoLoadPresetButton,
                                    }
                                }
                            }
                        },
                        new Category {
                            Name = "Menu Settings",
                            Buttons = {
                                ExtraButtons.ThemeButton,
                                new Button { Name = "Page Buttons", Toggle = false, Incremental = true, Up = () => Core.ChangePageButtons(false), Down = () => Core.ChangePageButtons(true) },
                                new Button { Name = "Menu Size", Toggle = false, Incremental = true, Up = () => Core.ChangeMenuScale(true), Down = () => Core.ChangeMenuScale(false) },
                                new Button { Name = "Button Inset", Toggle = false, Incremental = true, Up = () => Core.ChangeButtonInset(true), Down = () => Core.ChangeButtonInset(false) },
                                new Button { Name = "Text Size", Toggle = false, Incremental = true, Up = () => Core.ChangeTextSize(true), Down = () => Core.ChangeTextSize(false) },
                                new Button { Name = "Make Menu Rounded", Toggle = false, OnEnable = () => { Core.IsRounded = !Core.IsRounded; Core.RebuildMenu();} },
                                new Button { Name = "Menu Animations", Toggle = true, Enabled = true, OnceEnable = () => Core.IsAnimated = true, OnceDisable = () => Core.IsAnimated = false },
                                new Button { Name = "Menu Particles", Toggle = true, OnceEnable = () => { Core.IsMenuParticles = true; Core.RebuildMenu(); }, OnceDisable = () => { Core.IsMenuParticles = false; Core.RebuildMenu(); } },

                                new Button { Name = "Outline Menu", Toggle = false, OnEnable = () => Core.IsOutlined = !Core.IsOutlined },
                                new Button { Name = "Rotated Sidebar", Toggle = false, OnEnable = () => Core.IsCatRotated = !Core.IsCatRotated },
                                new Button { Name = "Sidebar Position", Toggle = false, OnEnable = () => Core.IsCatLeft = !Core.IsCatLeft },
                                new Button { Name = "Right Handed", Toggle = false, OnEnable = () => Core.IsRightHanded = !Core.IsRightHanded },
                                new Button { Name = "Boards Gradient", Toggle = true, Enabled = true, OnceEnable = () => Core.IsBoardGradientEnabled = true, OnceDisable = () => Core.IsBoardGradientEnabled = false },
                                new Button { Name = "Menu Smoothing", Toggle = true, Enabled = true, OnEnable = () => Core.MenuSmoothing = true, OnDisable = () => Core.MenuSmoothing = false },
                                ExtraButtons.MenuSmoothingSpeedButton,
                            }
                        },
                        new Category {
                            Name = "Networking",
                            Buttons = {
                                new Button { Name = "Having Networking Enabled Means That You'll Be Detected By Mod Checkers", Toggle = false, Label = true },
                                new Button { Name = "Enable Networking", Toggle = true, Enabled = true, OnEnable = () => Juul.JUUL.ToggleNetworking(true), OnDisable = () => Juul.JUUL.ToggleNetworking(false) },
                                new Button { Name = "Get All JUUL Users In Room", Toggle = false, OnEnable = Overpowered.GetAllJUULUsersInLobby },
                            }
                        },
                        new Category {
                            Name = "GunLib Settings",
                            Buttons = {
                                ExtraButtons.GunStyleButton,
                                ExtraButtons.GunLineSizeButton,
                                ExtraButtons.GunSphereSizeButton,
                                new Button { Name = "Test Gunlib", Toggle = true, OnEnable = () => GunLib.StartPointerSystem(() => { }, false) },
                            }
                        },
                        new Category {
                            Name = "Notifications",
                            Buttons = {
                                new Button { Name = "Test Notification", Toggle = false, OnEnable = () => NotifiLib.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Test Notification Works!") },
                                new Button { Name = "Disable Notifications", Toggle = true, OnceEnable = () => NotifiLib.Disablenotifcations = true, OnceDisable = () => NotifiLib.Disablenotifcations = false },
                            }
                        }
                    }
                },
                ExtraButtons.EnabledCategory,
                PlayerMenu.GetPlayersCategory(),
                new Category {
                    Name = "Advantage",
                    Buttons = {
                        new Button { Name = "Tag All", Toggle = true, OnEnable = Advantages.TagAll },
                        new Button { Name = "Tag Nearest", Toggle = true, OnEnable = Advantages.TagNearest },

                        new Button { Name = "Tag Gun", Toggle = true, OnEnable = Advantages.TagGun },

                        new Button { Name = "Tag Self", Toggle = true, OnEnable = Advantages.TagSelf, OnceDisable = Advantages.FixRig },

                        new Button { Name = "Tag Aura", Toggle = true, OnEnable = Advantages.TagArua },
                        new Button { Name = "Grip Tag Aura", Toggle = true, OnEnable = Advantages.GripTagArua },

                        new Button { Name = "No Tag On Join", Toggle = true, OnceEnable = Advantages.NoTagOnJoin, OnceDisable = Advantages.TagOnJoin },

                        new Button { Name = "Infinite Range Tag", Toggle = true, OnEnable = () => { GameplayPatches.IsPositionInRangePatch.enabled = true; }, OnDisable = () => { GameplayPatches.IsPositionInRangePatch.enabled = false; } },
                    }
                },
                new Category {
                    Name = "Room",
                    Buttons = {
                        ExtraButtons.RoomJoinerButton,
                        new Button { Name = "Join Random", Toggle = false, OnEnable = Safety.JoinRandom },

                    }
                },
                new Category {
                    Name = "Movement",
                    Buttons = {
                        new Button { Name = "Platforms", Toggle = true, OnEnable = Movement.Platforms, HasKeybinds = true, KeybindCategory = ExtraButtons.platformsConfig },

                        new Button { Name = "WASD Fly", Toggle = true, OnEnable = Movement.WASDFly },

                        new Button { Name = "Flight", Toggle = true, OnEnable = Movement.Fly, HasKeybinds = true, KeybindCategory = ExtraButtons.flightConfig },
                        new Button { Name = "Noclip Flight", Toggle = true, OnEnable = Movement.NoClipFly },
                     
                        new Button { Name = "Bark Fly", Toggle = true, OnEnable = Movement.BarkFly },

                        new Button { Name = "Noclip", Toggle = true, OnEnable = Movement.Noclip, HasKeybinds = true, KeybindCategory = ButtonConfigs.Get("Noclip").GenerateCategory() },

                        new Button { Name = "Speed Boost", Toggle = true, OnEnable = Movement.SpeedBoost, HasKeybinds = true, KeybindCategory = ExtraButtons.speedBoostConfig },

                        new Button { Name = "Up And Down", Toggle = true, OnEnable = Movement.UpAndDown },
                        new Button { Name = "Left And Right", Toggle = true, OnEnable = Movement.LeftAndRight },
                        new Button { Name = "Back And Forth", Toggle = true, OnEnable = Movement.BackAndForth },

                        new Button { Name = "Dash", Toggle = true, OnEnable = Movement.Dash },

                        new Button { Name = "Pull Mod", Toggle = true, OnEnable = Movement.PullMod },
                        new Button { Name = "Pull Boost", Toggle = true, OnEnable = Movement.PullBoost },

                        new Button { Name = "Wall Assist", Toggle = true, OnEnable = Movement.WallAssist },
                        new Button { Name = "Wall Walk", Toggle = true, OnEnable = Movement.WallWalk, HasKeybinds = true, KeybindCategory = ExtraButtons.wallWalkConfig },
                        new Button { Name = "Legit Wall Walk", Toggle = true, OnEnable = Movement.LegitimateWallWalk },

                        new Button { Name = "Teleport Gun", Toggle = true, OnEnable = Movement.TeleportGun },
                        new Button { Name = "Teleport To Random Player", Toggle = true, OnEnable = Players.TeleportToRandomPlayer },
                        new Button { Name = "Teleport To Closest Player", Toggle = true, OnEnable = Players.TeleportToClosestPlayer },


                        new Button { Name = "Zero Gravity", Toggle = true, OnEnable = Movement.ZeroGravity },
                        new Button { Name = "Low Gravity", Toggle = true, OnEnable = Movement.LowGravity },
                        new Button { Name = "High Gravity", Toggle = true, OnEnable = Movement.HighGravity },

                        new Button { Name = "Bouncy", Toggle = true, OnceEnable = Movement.Bouncy, OnceDisable = Movement.ResetBouncy },
                        new Button { Name = "Body Slide", Toggle = true, OnceEnable = Fun.EnableBodySlide, OnceDisable = Fun.DisableBodySlide },
                        new Button { Name = "Check Point", Toggle = true, OnEnable = Movement.Checkpoint, OnDisable = Movement.DestroyCheckpoint, HasKeybinds = true, KeybindCategory = ButtonConfigs.Get("Check Point").GenerateCategory() },
                    }
                },
                new Category {
                    Name = "Client",
                    Buttons = {
                        //new Button { Name = "Right Grip", Toggle = true, OnEnable = () => ControllerInputPoller.instance.rightGrab = true, OnDisable = () => ControllerInputPoller.instance.rightGrab = false },

                        //new Button { Name = "Fake FBT", Toggle = true, OnEnable = Players.FakefullbodyTrackingg, OnDisable = Players.DisableFakefullbodyTrackingg, HasKeybinds = true, KeybindCategory = ExtraButtons.fakeFBTConfig },

                        new Button { Name = "Invis Monkey", Toggle = true, OnEnable = Players.InvisibleMonke },
                        new Button { Name = "Ghost Monkey", Toggle = true, OnEnable = Players.GhostMonke },
                        new Button { Name = "Enable Ghost View", Toggle = true, Enabled = true, OnceEnable = () => ExtraButtons.ghostview = true, OnceDisable = () => Players.GhostviewClean() },

                        new Button { Name = "Long Arms", Toggle = true, OnEnable = Players.RArms, OnDisable = Players.DisableLongArms },
                        new Button { Name = "Steam Long Arms", Toggle = true, OnEnable = Players.TArms, OnDisable = Players.DisableLongArms },
                        new Button { Name = "Short Arms", Toggle = true, OnEnable = Players.SArms, OnDisable = Players.DisableLongArms },
                        new Button { Name = "Change Arm Lenth", Toggle = true, OnEnable = Players.ChangeArmLenth, OnDisable = Players.DisableLongArms },

                        new Button { Name = "Spin Bot", Toggle = true, OnEnable = Players.Spinbot },
                        new Button { Name = "Bayblade", Toggle = true, OnEnable = Players.BayBlade },
                        new Button { Name = "T-Pose", Toggle = true, OnEnable = Players.TPose },
                        new Button { Name = "Ragdoll", Toggle = true, OnEnable = Players.Ragdoll },
                        new Button { Name = "Seizure", Toggle = true, OnEnable = Fun.SeizureCamera },

                        ExtraButtons.SetNameButton,

                        new Button { Name = "Grab Rig", Toggle = true, OnEnable = Players.GrabRig },

                        new Button { Name = "PC Button Click", Toggle = true, OnEnable = Players.PCButtonClick },

                        new Button { Name = "Stare At Nearby Player", Toggle = true, OnEnable = Players.StareAtClosestPlayer, OnDisable = Players.FixHead },

                        new Button { Name = "Spin Head X", Toggle = true, OnEnable = Players.SpinHeadX, OnDisable = Players.FixHead },
                        new Button { Name = "Spin Head Y", Toggle = true, OnEnable = Players.SpinHeadY, OnDisable = Players.FixHead },
                        new Button { Name = "Spin Head Z", Toggle = true, OnEnable = Players.SpinHeadZ, OnDisable = Players.FixHead },
                       
                        new Button { Name = "Spaz Head", Toggle = true, OnEnable = Players.SpazHead, OnDisable = Players.FixHead },
                        new Button { Name = "Spaz Rig", Toggle = true, OnEnable = Players.SpazRig },
                        new Button { Name = "Spaz Hands", Toggle = true, OnEnable = Players.SpazHands },

                        new Button { Name = "Backwards Head", Toggle = true, OnEnable = Players.BackwardsHead, OnDisable = Players.FixHead },
                        new Button { Name = "Upsidedown Head", Toggle = true, OnEnable = Players.UpsidedownHead, OnDisable = Players.FixHead },
                        new Button { Name = "Break Neck", Toggle = true, OnEnable = Players.BreakNeck, OnDisable = Players.FixHead },

                        new Button { Name = "Fake Lag", Toggle = true, OnEnable = Players.FakeLag },
                        new Button { Name = "Smooth Rig", Toggle = true, OnEnable = () => { PhotonNetwork.SerializationRate = 30; }, OnDisable = () => { PhotonNetwork.SerializationRate = 10; } },

                        new Button { Name = "Size Changer", Toggle = true, OnEnable = Players.SizeChanger },

                        new Button { Name = "Uncap FPS", Toggle = false, OnEnable = Safety.UncapFPS },
                        new Button { Name = "Set 144 FPS", Toggle = false, OnEnable = Safety.SetFPS144 },
                        new Button { Name = "Set 120 FPS", Toggle = false, OnEnable = Safety.SetFPS120 },
                        new Button { Name = "Set 90 FPS", Toggle = false, OnEnable = Safety.SetFPS90 },
                        new Button { Name = "Set 80 FPS", Toggle = false, OnEnable = Safety.SetFPS80 },
                        new Button { Name = "Set 72 FPS", Toggle = false, OnEnable = Safety.SetFPS72 },
                        new Button { Name = "Set 60 FPS", Toggle = false, OnEnable = Safety.SetFPS60 },
                        new Button { Name = "Set 45 FPS", Toggle = false, OnEnable = Safety.SetFPS45 },
                        new Button { Name = "Set 15 FPS", Toggle = false, OnEnable = Safety.SetFPS15 },
                        new Button { Name = "Set 1 FPS", Toggle = false, OnEnable = Safety.SetFPS1 }
                    }
                },
                new Category {
                    Name = "Safety",
                    Buttons = {
                        new Button { Name = "Quit Game", Toggle = false, OnEnable = Safety.QuitGame },

                        new Button { Name = "Semi-Anti Ban", Toggle = true, OnEnable = Safety.AntiBan },

                        new Button { Name = "Spoof Player", Toggle = false, OnEnable = Safety.SpoofPlayer },

                        new Button { Name = "Anti Report Disconnect", Toggle = true, OnEnable = Safety.AntiReportDisconnect },
                        new Button { Name = "Anti Report Reconnect", Toggle = true, OnEnable = Safety.AntiReportReconnect },
                        new Button { Name = "Anti Report Notify", Toggle = true, OnEnable = Safety.AntiReportNotify },
                        new Button { Name = "Anti Report Quit", Toggle = true, OnEnable = Safety.AntiReportQuit },
                        ExtraButtons.AntiReportRadiusButton,
                        new Button { Name = "Visualize Anti Report Radius", Toggle = true, OnEnable = Safety.VisualizeAntiReportRadius, OnceDisable = Safety.CleanupAntiReportVisualization },

                        new Button { Name = "Anti Crash", Toggle = true, OnEnable = Safety.AntiCrash },
                        new Button { Name = "Disable Self Reports", Toggle = true, OnEnable = () => { SafetyPatches.SendReportPatch.AntiCheatSelf = true; }, OnDisable = () => { SafetyPatches.SendReportPatch.AntiCheatSelf = false; } },
                        new Button { Name = "Disable All Reports", Toggle = true, OnEnable = () => { SafetyPatches.SendReportPatch.AntiCheatAll = true; }, OnDisable = () => { SafetyPatches.SendReportPatch.AntiCheatAll = false; } },
                        new Button { Name = "Hide Report Reasons", Toggle = true, OnEnable = () => { SafetyPatches.SendReportPatch.AntiCheatReasonHide = true; }, OnDisable = () => { SafetyPatches.SendReportPatch.AntiCheatReasonHide = false; } },
                        new Button { Name = "Anti-Cheat Report Disconnect", Toggle = true, OnEnable = () => { SafetyPatches.SendReportPatch.AntiACReport = true; }, OnDisable = () => { SafetyPatches.SendReportPatch.AntiACReport = false; } },

                        new Button { Name = "Disable Telemetry", Toggle = true, OnEnable = () => { TelemetryPatches.enabled = true; }, OnDisable = () => { TelemetryPatches.enabled = false; } },

                        new Button { Name = "Accept TOS", Toggle = false, OnEnable = Safety.AcceptTOS },

                        new Button { Name = "Restart Game", Toggle = false, OnEnable = Safety.RestartGame },

                        new Button { Name = "Disable Map Triggers", Toggle = true, OnceEnable = Safety.DisableMapTriggers, OnceDisable = Safety.EnableMapTriggers },
                        new Button { Name = "Disable Net Triggers", Toggle = true, OnceEnable = Safety.DisableNetworkTriggers, OnceDisable = Safety.EnableNetworkTriggers },
                        new Button { Name = "Disable Quit Box", Toggle = true, OnceEnable = Safety.DisableQuitBox, OnceDisable = Safety.EnableQuitBox },
                        new Button { Name = "Disable AFK Kick", Toggle = true, OnceEnable = Safety.DisableAntiAFK, OnceDisable = Safety.EnableAntiAFK },

                        new Button { Name = "Left Trigger Disconnect", Toggle = true, OnEnable = Safety.DisconnectLT },
                        new Button { Name = "Right Trigger Disconnect", Toggle = true, OnEnable = Safety.DisconnectRT },

                        new Button { Name = "No Finger Movement", Toggle = true, OnEnable = Safety.NoFinger },

                        new Button { Name = "Create Public Lobby V1", Toggle = false, OnEnable = Safety.CreatePublicLobby10 },
                        new Button { Name = "Create Public Lobby V2", Toggle = false, OnEnable = Safety.CreatePublicLobby20 },
                    }
                },
                new Category {
                    Name = "Visual",
                    Buttons = {
                        new Button { Name = "Array List [PC]", Toggle = true, OnEnable = Visual.EnableArrayList, OnceDisable = Visual.DisableArrayList },
                        new Button { Name = "Custom HUD", Toggle = true, OnEnable = Visual.PlayerInfo, OnceDisable = Visual.CleanupPlayerInfo },
                        
                        new Button { Name = "Name Tags", Toggle = true, OnEnable = Visual.PlayerNameESP, OnceDisable = Visual.CleanupPlayerNameESP, HasKeybinds = true, KeybindCategory = ExtraButtons.nameTagsConfig },

                        new Button { Name = "Chams", Toggle = true, OnEnable = Visual.Chams, OnceDisable = Visual.CleanupChams },
                        new Button { Name = "Infection Chams", Toggle = true, OnEnable = Visual.InfectionChams, OnceDisable = Visual.CleanupInfectionChams },
                      
                        new Button { Name = "Bone ESP", Toggle = true, OnEnable = Visual.BoneESP, OnceDisable = Visual.CleanupBoneESP },
                        new Button { Name = "Infection Bone ESP", Toggle = true, OnEnable = Visual.InfectionBoneESP, OnceDisable = Visual.CleanupInfectionBoneESP },

                        new Button { Name = "Tracers", Toggle = true, OnEnable = Visual.Tracers, OnceDisable = Visual.CleanupTracers },
                        new Button { Name = "Infection Tracers", Toggle = true, OnEnable = Visual.InfectionTracers, OnceDisable = Visual.CleanupInfectionTracers },

                        new Button { Name = "2D Box ESP", Toggle = true, OnEnable = Visual.Box2DESP, OnceDisable = Visual.CleanupBox2DESP },
                        new Button { Name = "2D Corner ESP", Toggle = true, OnEnable = Visual.Box2DCornerESP, OnceDisable = Visual.CleanupBox2DCornerESP },

                        new Button { Name = "3D Box ESP", Toggle = true, OnEnable = Visual.Box3DESP, OnceDisable = Visual.CleanupBox3DESP },
                        new Button { Name = "3D Corner ESP", Toggle = true, OnEnable = Visual.Box3DESPV2, OnceDisable = Visual.CleanupBox3DESPV2 },
               
                        new Button { Name = "Circle ESP", Toggle = true, OnEnable = Visual.Circle3DESP, OnceDisable = Visual.CleanupCircle3DESP },

                        new Button { Name = "Always Morning [CS]", Toggle = false, OnEnable = () => BetterDayNightManager.instance.SetTimeOfDay(1) },
                        new Button { Name = "Always Day [CS]", Toggle = false, OnEnable = () => BetterDayNightManager.instance.SetTimeOfDay(3) },
                        new Button { Name = "Always Evening [CS]", Toggle = false, OnEnable = () => BetterDayNightManager.instance.SetTimeOfDay(6) },
                        new Button { Name = "Always Night [CS]", Toggle = false, OnEnable = () => BetterDayNightManager.instance.SetTimeOfDay(0) },
                        new Button { Name = "Always Midnight [CS]", Toggle = false, OnEnable = () => BetterDayNightManager.instance.SetTimeOfDay(8) },

                        new Button { Name = "Disable Snowfall", Toggle = true, OnEnable = Visual.DisableSnowfall, OnceDisable = Visual.EnableSnowfall },
                        new Button { Name = "Disable Rain", Toggle = true, OnEnable = Visual.DisableRain, OnceDisable = Visual.EnableRain },

                        new Button { Name = "Menu Theme Rig [CS]", Toggle = true, OnEnable = Visual.MenuThemeRig, OnceDisable = Visual.RigColorFix },

                        new Button { Name = "Rainbow All [CS]", Toggle = true, OnEnable = Visual.OutcastAll },

                        new Button { Name = "Wide FOV", Toggle = true, OnEnable = Fun.WideFOV, OnceDisable = Fun.NormalFOV },
                        new Button { Name = "Near FOV", Toggle = true, OnEnable = Fun.LookFOV, OnceDisable = Fun.NormalFOV },

                        new Button { Name = "Mod Checker Gun", Toggle = true, OnEnable = Visual.ModCheckerGun, OnceDisable = Visual.CleanupModCheckers },
                    }
                },
                new Category {
                    Name = "Fun",
                    Buttons = {
                        new Button { Name = "Unlock VIM Subscription", Toggle = true, OnEnable = Fun.UnlockSubscription },
                        new Button { Name = "Enable VIM Name Tag", Toggle = true, OnceEnable = Fun.EnableGoldNameTag, OnceDisable = Fun.DisableGoldNameTag },
                        new Button { Name = "Flash VIM Name Tag", Toggle = true, OnceEnable = Fun.FlashGoldNameTag },

                        new Button { Name = "Max Quest Score", Toggle = false, OnEnable = Fun.MaxQuestScore },

                        new Button { Name = "Win Paddleball Left", Toggle = false, OnEnable = Fun.WinPaddleballLeft },
                        new Button { Name = "Win Paddleball Right", Toggle = false, OnEnable = Fun.WinPaddleballRight },
                        new Button { Name = "Fast Paddleball", Toggle = false, OnEnable = Fun.SuperFastPaddleballBall },
                      
                        new Button { Name = "RGB Monkey [STUMP]", Toggle = true, OnEnable = Fun.FadeMonkey },
                        new Button { Name = "Hard RGB Monkey [STUMP]", Toggle = true, OnEnable = Fun.FadeMonkeyHardRGB },
                        new Button { Name = "Epilepsy Monkey [STUMP]", Toggle = true, OnEnable = Fun.FlashMonkey },
                        new Button { Name = "B&W Epilepsy Monkey [STUMP]", Toggle = true, OnEnable = Fun.BAWFlashMonkey },
                        new Button { Name = "Copy Color Gun [STUMP]", Toggle = true, OnEnable = Fun.CopyColorGun },

                        new Button { Name = "Unlock All [SI]", Toggle = true, OnEnable = Fun.SIUnlockAll },
                        new Button { Name = "Steal All Terminals [SI]", Toggle = true, OnEnable = Fun.YoinkTerms },
                        new Button { Name = "Give All Resources [SI]", Toggle = true, OnEnable = Fun.GiveAllResources },
                        new Button { Name = "Always Own A Terminal [SI]", Toggle = true, OnEnable = Fun.AlwaysOwnTerminals },
                        new Button { Name = "Disable Terminal Timeout [SI]", Toggle = true, OnEnable = Fun.DisableTerminalTimeout },

                        new Button { Name = "No Jet Cooldown [SI]", Toggle = true, OnEnable = () => { GameplayPatches.OnUpdateAuthorityJetPatch.enabled = true; }, OnDisable = () => { GameplayPatches.OnUpdateAuthorityJetPatch.enabled = false; } },
                        new Button { Name = "No Platform Cooldown [SI]", Toggle = true, OnEnable = () => { GameplayPatches.OnUpdateAuthorityPlatformPatch.enabled = true; }, OnDisable = () => { GameplayPatches.OnUpdateAuthorityPlatformPatch.enabled = false; } },
                        new Button { Name = "No Blaster Cooldown [SI]", Toggle = true, OnEnable = () => { GameplayPatches.OnUpdateAuthorityBlasterPatch.enabled = true; }, OnDisable = () => { GameplayPatches.OnUpdateAuthorityBlasterPatch.enabled = false; } },
                        new Button { Name = "No Yoyo Cooldown [SI]", Toggle = true, OnEnable = () => { GameplayPatches.OnUpdateAuthorityYoyoPatch.enabled = true; }, OnDisable = () => { GameplayPatches.OnUpdateAuthorityYoyoPatch.enabled = false; } },

                        new Button { Name = "Blaster Fling All [SI]", Toggle = true, OnEnable = Overpowered.BlasterFlingAll },
                        new Button { Name = "Blaster Fling Gun [SI]", Toggle = true, OnEnable = Overpowered.BlasterFlingGun },

                        new Button { Name = "Blaster Aimbot [SI]", Toggle = true, OnEnable = Overpowered.BlasterAimbot },
                        new Button { Name = "Paintbrawl Aimbot", Toggle = true, OnEnable = Overpowered.PaintbrawlAimbot },

                        new Button { Name = "Set Name to HIDE", Toggle = false, OnEnable = () => Fun.ChangeNameTo("HIDE") },
                        new Button { Name = "Set Name to SEEK", Toggle = false, OnEnable = () => Fun.ChangeNameTo("SEEK") },
                        new Button { Name = "Set Name to RUN", Toggle = false, OnEnable = () => Fun.ChangeNameTo("RUN") },
                        new Button { Name = "Set Name to HIDDEN", Toggle = false, OnEnable = () => Fun.ChangeNameTo("HIDDEN") },
                        new Button { Name = "Set Name to FOUND", Toggle = false, OnEnable = () => Fun.ChangeNameTo("FOUND") },
                        new Button { Name = "Set Name to BEHINDYOU", Toggle = false, OnEnable = () => Fun.ChangeNameTo("BEHINDYOU") },
                        new Button { Name = "Set Name to STATUE", Toggle = false, OnEnable = () => Fun.ChangeNameTo("STATUE") },
                        new Button { Name = "Set Name to GHOST", Toggle = false, OnEnable = () => Fun.ChangeNameTo("GHOST") },
                        new Button { Name = "Set Name to HAUNT", Toggle = false, OnEnable = () => Fun.ChangeNameTo("HAUNT") },
                        new Button { Name = "Set Name to CREEP", Toggle = false, OnEnable = () => Fun.ChangeNameTo("CREEP") },
                        new Button { Name = "Set Name to STALKER", Toggle = false, OnEnable = () => Fun.ChangeNameTo("STALKER") },
                        new Button { Name = "Set Name to 404", Toggle = false, OnEnable = () => Fun.ChangeNameTo("404") },
                        new Button { Name = "Set Name to JUULONTOP", Toggle = false, OnEnable = () => Fun.ChangeNameTo("JUULONTOP") },

                        new Button { Name = "Unlock All Cosmetic [CS]", Toggle = false, OnEnable = () => Fun.UnlockAllCosmetics() },
                        new Button { Name = "Unlock All Shinyrocks [CS]", Toggle = false, OnEnable = () => Fun.GiveUnlimitedShinyRocks() },

                        new Button { Name = "Juggle Holdables", Toggle = true, OnEnable = () => Fun.JuggleHoldables() },

                        new Button { Name = "Forget All Credentials", Toggle = false, OnEnable = Fun.ForgetAllCredentials },

                        new Button { Name = "Unlock Competitive Queue", Toggle = false, OnEnable = Fun.UnlockCompetitiveQueue },
                        new Button { Name = "Force Default Queue", Toggle = false, OnEnable = Fun.ForceQueueDefault },
                        new Button { Name = "Force Competitive Queue", Toggle = false, OnEnable = Fun.ForceQueueCompetitive },
                        new Button { Name = "Force Minigames Queue", Toggle = false, OnEnable = Fun.ForceQueueMinigames },

                    }
                },
                new Category {
                    Name = "Master",
                    Buttons = {
                        new Button { Name = "Organized, because there was over 100+ Master mods", Toggle = false, Label = true },
                    },
                    Subcategories = {
                        new Category {
                            Name = "Basement",
                            Buttons = {
                                new Button { Name = "Open Basement Door", Toggle = true, OnEnable = Master.OpenBasementDoor },
                                new Button { Name = "Close Basement Door", Toggle = true, OnEnable = Master.CloseBasementDoor },

                                new Button { Name = "Break Basement Door", Toggle = true, OnEnable = Master.BreakBasementDoor },
                            }
                        },
                        new Category {
                            Name = "Brawl",
                            Buttons = {
                                new Button { Name = "All Red Team [BRAWL]", Toggle = false, OnEnable = Master.AllRedTeam },
                                new Button { Name = "All Blue Team [BRAWL]", Toggle = false, OnEnable = Master.AllBlueTeam },

                                new Button { Name = "Kill All Players [BRAWL]", Toggle = false, OnEnable = Master.KillAll },
                                new Button { Name = "Revive All Players [BRAWL]", Toggle = false, OnEnable = Master.HealAll },
                                new Button { Name = "Stun All Players [BRAWL]", Toggle = false, OnEnable = Master.StunAll },

                                new Button { Name = "Kill Gun [BRAWL]", Toggle = true, OnEnable = Master.InstantKillGun },
                                new Button { Name = "Stun Gun [BRAWL]", Toggle = true, OnEnable = Master.StunGun },
                                new Button { Name = "Team Changer Gun [BRAWL]", Toggle = true, OnEnable = Master.TeamChangerGun },
                                new Button { Name = "Revive Gun [BRAWL]", Toggle = true, OnEnable = Master.HealGun },

                                new Button { Name = "Lag Gun [BRAWL]", Toggle = true, OnEnable = Master.PaintbrawlLagGunTest, Description = "Ground Breaking" },
                            }
                        },
                        new Category {
                            Name = "Elevators",
                            Buttons = {
                                new Button { Name = "Open All Elevator Doors", Toggle = false, OnEnable = Master.OpenElevatorDoor },
                                new Button { Name = "Close All Elevator Doors", Toggle = false, OnEnable = Master.CloseElevatorDoor },

                                new Button { Name = "Teleport All Elevators To Stump", Toggle = false, OnEnable = Master.TeleportToStump },
                                new Button { Name = "Teleport All Elevators To City", Toggle = false, OnEnable = Master.TeleportToCity },
                                new Button { Name = "Teleport All Elevators To Ghost Reactor", Toggle = false, OnEnable = Master.TeleportToGhostReactor },
                                new Button { Name = "Teleport All Elevators To Monke Blocks", Toggle = false, OnEnable = Master.TeleportToMonkeBlocks },

                                new Button { Name = "Freeze Elevator Doors Open", Toggle = true, OnEnable = Master.FreezeElevatorDoorsOpen },
                                new Button { Name = "Freeze Elevator Doors Closed", Toggle = true, OnEnable = Master.FreezeElevatorDoorsClosed },

                                new Button { Name = "Break All Elevators", Toggle = true, OnEnable = Master.BreakElevator },
                            }
                        },
                        new Category {
                            Name = "Gamemode",
                            Buttons = {
                                new Button { Name = "Break Game Mode", Toggle = true, OnceEnable = Overpowered.BreakGameMode, OnceDisable = Overpowered.FixGamemode, Description = "Ground Breaking" },

                                new Button { Name = "Untag All", Toggle = true, OnEnable = Master.UnTagAll, Description = "Ground Breaking" },
                                new Button { Name = "Untag Gun", Toggle = true, OnEnable = Master.UnTagGun, Description = "Ground Breaking" },
                                new Button { Name = "Untag Self", Toggle = true, OnEnable = Master.UnTagSelf, Description = "Ground Breaking" },

                                new Button { Name = "Force Tag Lag", Toggle = true, OnceEnable = Master.CauseTagLag, OnceDisable = Master.FixTagLag, Description = "Ground Breaking" },
                            }
                        },
                        new Category {
                            Name = "Ghost Reactor",
                            Buttons = {
                                new Button { Name = "Purchase All Tools [GR]", Toggle = false, OnEnable = Master.PurchaseAllStationTools },
                                new Button { Name = "Kill All Enemies [GR]", Toggle = false, OnEnable = Master.KillAllEnemies },
                                new Button { Name = "Kill All [GR]", Toggle = false, OnEnable = Master.GhostReactorKillAll },
                                new Button { Name = "Kill Gun V1 [GR]", Toggle = true, OnEnable = Master.GhostReactorKillGun },
                                new Button { Name = "Kill Gun V2 [GR]", Toggle = true, OnEnable = Master.GhostReactorKillGun2 },
                                new Button { Name = "Shield All [GR]", Toggle = false, OnEnable = Master.GhostReactorSheildAll },
                                new Button { Name = "Shield Gun [GR]", Toggle = true, OnEnable = Master.GhostReactorSheildGun },

                                new Button { Name = "Start Shift [GR]", Toggle = false, OnEnable = Master.StartShiftNow },
                                new Button { Name = "End Shift [GR]", Toggle = false, OnEnable = Master.EndShiftNow },
                                new Button { Name = "Max Difficulty [GR]", Toggle = false, OnEnable = Master.SetMaxDifficulty },
                                new Button { Name = "Set Depth Level 1 [GR]", Toggle = false, OnEnable = () => Master.SetDepthLevel(1) },
                                new Button { Name = "Set Depth Level 5 [GR]", Toggle = false, OnEnable = () => Master.SetDepthLevel(5) },
                                new Button { Name = "Set Depth Level 10 [GR]", Toggle = false, OnEnable = () => Master.SetDepthLevel(10) },
                                new Button { Name = "Spawn Core [GR]", Toggle = false, OnEnable = Master.GhostSpawnCoreGun },
                                new Button { Name = "Spawn Chaos Seed [GR]", Toggle = false, OnEnable = Master.GhostSpawnChaosSeedGun },
                                new Button { Name = "Spawn Super Core [GR]", Toggle = false, OnEnable = Master.GhostSpawnSuperCoreGun },

                                new Button { Name = "Destroy All Entitys [GR]", Toggle = true, OnEnable = Master.DestroyAllEntitys, Description = "Ground Breaking" },
                                new Button { Name = "Destroy Entity Gun [GR]", Toggle = true, OnEnable = Master.DestroyEntityGun, Description = "Ground Breaking" },
                            }
                        },
                        new Category {
                            Name = "Guardian",
                            Buttons = {
                                new Button { Name = "Set Guardian Self", Toggle = false, OnEnable = Master.SetGuardianSelf },
                                new Button { Name = "Set Guardian Gun", Toggle = true, OnEnable = Master.SetGuardianGun },
                                new Button { Name = "Set Guardian Aura", Toggle = true, OnEnable = Master.GuardianAura },
                                new Button { Name = "Set Guardian On Your Touch", Toggle = true, OnEnable = Master.GuardianOnTouch },
                                new Button { Name = "Set Guardian On Touch", Toggle = true, OnEnable = Master.GuardianOnYourTouch },

                                new Button { Name = "UnGuardian Self", Toggle = false, OnEnable = Master.UnGuardianSelf },
                                new Button { Name = "UnGuardian Gun", Toggle = true, OnEnable = Master.UnGuardianGun },
                                new Button { Name = "UnGuardian Aura", Toggle = true, OnEnable = Master.UnGuardianAura },
                                new Button { Name = "UnGuardian On Your Touch", Toggle = true, OnEnable = Master.UnGuardianOnTouch },
                                new Button { Name = "UnGuardian On Touch", Toggle = true, OnEnable = Master.UnGuardianOnYourTouch },
                            }
                        },
                        new Category {
                            Name = "Hoverboard",
                            Buttons = {
                                new Button { Name = "Start 3 Lap Race", Toggle = false, OnEnable = Master.Start3LapRace },
                                new Button { Name = "Start 5 Lap Race", Toggle = false, OnEnable = Master.Start5LapRace },
                                new Button { Name = "Spam Race Start", Toggle = true, OnEnable = Master.SpamRaceStart },
                                new Button { Name = "Force End Race", Toggle = false, OnEnable = Master.ForceEndCurrentRace },
                                new Button { Name = "Complete Race Instantly", Toggle = false, OnEnable = Master.CompleteRaceInstantly },
                                new Button { Name = "Disqualify All Racers", Toggle = false, OnEnable = Master.DisqualifyAllRacers },
                                new Button { Name = "Reset Race", Toggle = false, OnEnable = Master.ResetRace },
                            }
                        },
                        new Category {
                            Name = "Monke Blocks",
                            Buttons = {
                                new Button { Name = "Random Block Gun", Toggle = true, OnEnable = Master.RandomBlockGun, Description = "Ground Breaking" },

                                new Button { Name = "Destroy Block Gun", Toggle = true, OnEnable = Master.DestroyBlockGun, Description = "Ground Breaking" },
                                new Button { Name = "Destroy All Blocks", Toggle = false, OnEnable = Master.RecycleAllBlocks, Description = "Ground Breaking" },

                                new Button { Name = "Block Crash All", Toggle = true, OnEnable = Master.BlockCrashAll, Description = "Ground Breaking" },
                                new Button { Name = "Block Crash Gun", Toggle = true, OnEnable = Master.BlockCrashGun, Description = "Ground Breaking" },
                            }
                        },
                        new Category {
                            Name = "Players",
                            Buttons = {
                                new Button { Name = "Material Spam All", Toggle = true, OnEnable = Master.MatAll, Description = "Ground Breaking" },
                                new Button { Name = "Material Spam Gun", Toggle = true, OnEnable = Master.MatGun, Description = "Ground Breaking" },

                                new Button { Name = "Slow All [RS]", Toggle = true, OnEnable = Master.SlowAll, Description = "Ground Breaking" },
                                new Button { Name = "Slow Gun [RS]", Toggle = true, OnEnable = Master.SlowGun, Description = "Ground Breaking" },

                                new Button { Name = "Vibrate All [RS]", Toggle = true, OnEnable = Master.VibrateAll, Description = "Ground Breaking" },
                                new Button { Name = "Vibrate Gun [RS]", Toggle = true, OnEnable = Master.VibrateGun, Description = " Ground Breaking" },
                            }
                        },
                        new Category {
                            Name = "Room",
                            Buttons = {
                                new Button { Name = "Lock Room", Toggle = false, OnEnable = Overpowered.LockRoom },
                                new Button { Name = "UnLock Room", Toggle = false, OnEnable = Overpowered.UnlockRoom },
                                new Button { Name = "Spaz Room", Toggle = true, OnEnable = Overpowered.SpazRoom },
                            }
                        },
                        new Category {
                            Name = "VIM",
                            Buttons = {
                                new Button { Name = "Rise Lava [DELAY]", Toggle = false, OnEnable = Master.RiseLavaMod },
                                new Button { Name = "Drain Lava [DELAY]", Toggle = false, OnEnable = Master.DrainLavaMod },
                                new Button { Name = "Full Lava [DELAY]", Toggle = false, OnEnable = Master.FullLavaMod },
                                new Button { Name = "Empty Lava [DELAY]", Toggle = false, OnEnable = Master.EmptyLavaMod },
                                new Button { Name = "Spaz Lava [DELAY]", Toggle = true, OnEnable = Master.SpazLavaMod },
                            }
                        },
                        new Category {
                            Name = "Virtual Stump",
                            Buttons = {
                                new Button { Name = "Become Terminal Driver", Toggle = false, OnEnable = () => Master.BecomeDriver() },
                                new Button { Name = "Unlock Terminal Driver", Toggle = false, OnEnable = () => Master.UnlockDriver() },
                                new Button { Name = "Spaz Terminal Driver", Toggle = true, OnEnable = () => Master.SpazDriver() },

                                new Button { Name = "Give Terminal Driver Gun", Toggle = true, OnEnable = () => Master.GiveDriverGun() },
                                new Button { Name = "Unlock Terminal Driver Gun", Toggle = true, OnEnable = () => Master.UnlockDriverGun() },
                                new Button { Name = "Spaz Terminal Driver Gun", Toggle = true, OnEnable = () => Master.SpazDriverGun() },
                            }
                        },
                        new Category {
                            Name = "World",
                            Buttons = {
                                new Button { Name = "Force Hit All Targets", Toggle = false, OnEnable = Master.ForceHitAllTargets },
                                new Button { Name = "Max Score All Targets", Toggle = false, OnEnable = Master.MaxScoreAllTargets },
                                new Button { Name = "Reset All Scores", Toggle = false, OnEnable = Master.ResetAllTargetScores },
                                new Button { Name = "Spam Hit Targets", Toggle = true, OnEnable = Master.SpamHitTargets },

                                new Button { Name = "Activate Grey Zone ", Toggle = false, OnEnable = Master.ActivateGreyZone, Description = "Ground Breaking" },
                                new Button { Name = "DeActivate Grey Zone", Toggle = false, OnEnable = Master.DeactivateGreyZone, Description = "Ground Breaking" },
                                new Button { Name = "Flash Grey Zone", Toggle = true, OnEnable = Master.SpazGreyZone, Description = "Ground Breaking" },
                            }
                        }
                    },
                     
                },
                new Category {
                    Name = "Exploits",
                     Subcategories = {
                        new Category {
                        Name = "Barrel Exploits",
                        Buttons = {
                        ExtraButtons.BarrelMethodButton,
                        new Button { Name = "Buy Barrel", Toggle = false, OnEnable = Overpowered.BuyBarrel },
                        new Button { Name = "Barrel Fling All", Toggle = true, OnEnable = Overpowered.BarrelFlingAll },
                        new Button { Name = "Barrel Fling Gun", Toggle = true, OnEnable = Overpowered.BarrelFlingGun },
                        new Button { Name = "Barrel Fling Aura", Toggle = true, OnEnable = Overpowered.BarrelFlingAura },
                        new Button { Name = "Barrel Fling On Your Touch", Toggle = true, OnEnable = Overpowered.BarrelFlingTouch },
                        new Button { Name = "Barrel Fling On Touch", Toggle = true, OnEnable = Overpowered.BarrelFlingOnYourTouch },
                        new Button { Name = "Barrel Punch Mod [SS]", Toggle = true, OnEnable = Overpowered.BarrelPunchMod },
                        new Button { Name = "Barrel Fling Anti Report", Toggle = true, OnEnable = Overpowered.BarrelFlingAntiReport },
                        }
                    },
                    new Category {
                        Name = "Grab Exploits",
                        Buttons = {
                        /*new Button { Name = "Grab Crash Gun", Toggle = true, OnEnable = Overpowered.ForceCrashGun },
                        new Button { Name = "Grab Crash Gun might take a few tries to crash", Toggle = false, Label = true },
                        new Button { Name = "Grab Break Movement", Toggle = true, OnEnable = Overpowered.BreakMovementGrabGun },
                        new Button { Name = "Strong Grab FLing Gun", Toggle = true, OnEnable = Overpowered.ForceBlackGun },*/
                        new Button { Name = "Grab Fling Gun", Toggle = true, OnEnable = Overpowered.GrabFlingGun },
                        new Button { Name = "Grab Fling All", Toggle = true, OnEnable = Overpowered.GrabFlingAll },
                        }
                    },
                    new Category {
                        Name = "Custom Maps",
                        Buttons = {

                        new Button { Name = "Teleport To Virtual Stump", Toggle = false, OnEnable = () => CustomMapManager.TeleportToVirtualStump(CustomMapManager.instance.defaultTeleporter, null) },
                        new Button { Name = "Exit Virtual Stump", Toggle = false, OnEnable = () => CustomMapManager.ExitVirtualStump(null) },

                        new Button { Name = "↓ Chimp Combat ↓", Toggle = false, Label = true },
                        new Button { Name = "Kill Self [VSTUMP]", Toggle = false, OnEnable = CustomMaps.KillSelf },
                        new Button { Name = "Kill Gun [VSTUMP]", Toggle = true, OnEnable = CustomMaps.KillGun },
                        new Button { Name = "Kill All [VSTUMP]", Toggle = false, OnEnable = CustomMaps.KillAll },
                        new Button { Name = "Kill Aura [VSTUMP]", Toggle = true, OnEnable = CustomMaps.KillAura },
                        new Button { Name = "Kill On Touch [VSTUMP]", Toggle = true, OnEnable = CustomMaps.KillOnTouch },
                        new Button { Name = "Kill On Your Touch [VSTUMP]", Toggle = true, OnEnable = CustomMaps.KillOnYourTouch },
                        new Button { Name = "God Mode [VSTUMP]", Toggle = true, OnEnable = CustomMaps.GodMode, OnDisable = CustomMaps.DisableGodMode },
                        new Button { Name = "No Grenade Cooldown", Toggle = true, OnEnable = CustomMaps.NoGrenadeCooldown, OnDisable = CustomMaps.DisableNoGrenadeCooldown },
                        new Button { Name = "No Shoot Cooldown [VSTUMP]", Toggle = true, OnEnable = CustomMaps.NoShootCooldown, OnDisable = CustomMaps.DisableNoShootCooldown },
                        new Button { Name = "Infinite Ammo", Toggle = true, OnEnable = CustomMaps.InfiniteAmmo, OnDisable = CustomMaps.DisableInfiniteAmmo },
                        new Button { Name = "Instant Kill [VSTUMP]", Toggle = false, OnEnable = CustomMaps.InstantKill },
                        new Button { Name = "Instant Kill Gun [VSTUMP]", Toggle = true, OnEnable = CustomMaps.InstantKillGun },
                        new Button { Name = "Infinite Points", Toggle = true, OnEnable = CustomMaps.InfinitePoints, OnDisable = CustomMaps.DisableInfinitePoints },
                        new Button { Name = "Rapid Fire [VSTUMP]", Toggle = true, OnEnable = CustomMaps.RapidFire, OnDisable = CustomMaps.DisableRapidFire },

                        new Button { Name = "↓ Monke Magic ↓", Toggle = false, Label = true },
                        new Button { Name = "Lightning Self", Toggle = false, OnEnable = CustomMaps.LightningStrikeSelf },
                        new Button { Name = "Lightning Gun", Toggle = true, OnEnable = CustomMaps.LightningStrikeGun },
                        new Button { Name = "Lightning All", Toggle = false, OnEnable = CustomMaps.LightningStrikeAll },
                        new Button { Name = "Lightning Aura", Toggle = true, OnEnable = CustomMaps.LightningAura },
                        new Button { Name = "Lightning On Touch", Toggle = true, OnEnable = CustomMaps.LightningOnTouch },
                        new Button { Name = "Lightning On Your Touch", Toggle = true, OnEnable = CustomMaps.LightningOnYourTouch },
                        new Button { Name = "Material Self [VSTUMP]", Toggle = false, OnEnable = CustomMaps.ChangeMaterialSelf },
                        new Button { Name = "Material Gun [VSTUMP]", Toggle = true, OnEnable = CustomMaps.ChangeMaterialGun },
                        new Button { Name = "Material All [VSTUMP]", Toggle = false, OnEnable = CustomMaps.ChangeMaterialAll },
                        new Button { Name = "Material Aura [VSTUMP]", Toggle = true, OnEnable = CustomMaps.MaterialAura },
                        new Button { Name = "Material On Touch [VSTUMP]", Toggle = true, OnEnable = CustomMaps.MaterialOnTouch },
                        new Button { Name = "Material On Your Touch [VSTUMP]", Toggle = true, OnEnable = CustomMaps.MaterialOnYourTouch },
                        new Button { Name = "Spawn Lucy Self", Toggle = false, OnEnable = CustomMaps.SpawnLucySelf },
                        new Button { Name = "Spawn Lucy Gun", Toggle = true, OnEnable = CustomMaps.SpawnLucyGun },
                        new Button { Name = "Spawn Lucy All", Toggle = false, OnEnable = CustomMaps.SpawnLucyAll },
                        new Button { Name = "Spawn Lucy Aura", Toggle = true, OnEnable = CustomMaps.SpawnLucyAura },
                        new Button { Name = "Spawn Lucy On Touch", Toggle = true, OnEnable = CustomMaps.SpawnLucyOnTouch },
                        new Button { Name = "Spawn Lucy On Your Touch", Toggle = true, OnEnable = CustomMaps.SpawnLucyOnYourTouch },

                        new Button { Name = "↓ Extra ↓", Toggle = false, Label = true },
                        new Button { Name = "Crash All [VSTUMP]", Toggle = false, OnEnable = CustomMaps.CrashAll },
                        new Button { Name = "Crash Gun [VSTUMP]", Toggle = true, OnEnable = CustomMaps.CrashGun },

                        }
                    },
                    new Category {
                        Name = "Fun/Trolling Exploits",
                        Buttons = {
                        new Button { Name = "Give Fly On Grab", Toggle = true, OnEnable = Overpowered.GiveFlyOnGrab },
                        }
                    },
                    new Category {
                        Name = "Guardian Exploits",
                        Buttons = {
                        new Button { Name = "Grab All [GUARD]", Toggle = true, OnEnable = Overpowered.GrabAll },
                        new Button { Name = "Grab Gun [GUARD]", Toggle = true, OnEnable = Overpowered.GrabGun },

                        new Button { Name = "Fling All [GUARD]", Toggle = true, OnEnable = Overpowered.FlingAll },
                        new Button { Name = "Fling Gun [GUARD]", Toggle = true, OnEnable = Overpowered.FlingGun },

                        new Button { Name = "Break Movement All [GUARD]", Toggle = true, OnEnable = Overpowered.BreakMovementAll },
                        new Button { Name = "Break Movement Gun [GUARD]", Toggle = true, OnEnable = Overpowered.BreakMovementGun },

                        new Button { Name = "Spaz All [GUARD]", Toggle = true, OnEnable = Overpowered.SpazAll },
                        new Button { Name = "Spaz Gun [GUARD]", Toggle = true, OnEnable = Overpowered.SpazGun },

                        new Button { Name = "Push All [GUARD]", Toggle = true, OnEnable = Overpowered.PushAllAway },
                        new Button { Name = "Push Gun [GUARD]", Toggle = true, OnEnable = Overpowered.PushGunAway },

                        new Button { Name = "Orbit All [GUARD]", Toggle = true, OnEnable = Overpowered.OrbitAll },
                        new Button { Name = "Orbit Gun [GUARD]", Toggle = true, OnEnable = Overpowered.OrbitGun },

                        new Button { Name = "Drop Player [GUARD]", Toggle = true, OnEnable = Overpowered.OrbitGun },
                        }
                    },
                    new Category {
                        Name = "Lag Exploits",
                        Buttons = {
                        ExtraButtons.LagMethodButton,
                        new Button { Name = "Lag All", Toggle = true, OnEnable = Overpowered.LagAll },
                        new Button { Name = "Lag Gun", Toggle = true, OnEnable = Overpowered.LagGun },
                        new Button { Name = "Lag Aura", Toggle = true, OnEnable = Overpowered.LagAura },
                        new Button { Name = "Lag On Your Touch", Toggle = true, OnEnable = Overpowered.LagOnTouch },
                        new Button { Name = "Lag On Touch", Toggle = true, OnEnable = Overpowered.LagOnYourTouch },

                        new Button { Name = "Stutter All", Toggle = true, OnEnable = Overpowered.StutterAll },
                        new Button { Name = "Stutter Gun", Toggle = true, OnEnable = Overpowered.StutterGun },
                        new Button { Name = "Stutter Aura", Toggle = true, OnEnable = Overpowered.StutterAura },
                        new Button { Name = "Stutter On Your Touch", Toggle = true, OnEnable = Overpowered.StutterOnTouch },
                        new Button { Name = "Stutter On Touch", Toggle = true, OnEnable = Overpowered.StutterOnYourTouch },

                        new Button { Name = "Big Stutter All", Toggle = true, OnEnable = Overpowered.CrashAll },
                        new Button { Name = "Big Stutter Gun", Toggle = true, OnEnable = Overpowered.CrashGun },
                        new Button { Name = "Big Stutter Aura", Toggle = true, OnEnable = Overpowered.CrashAura },
                        new Button { Name = "Big Stutter On Your Touch", Toggle = true, OnEnable = Overpowered.CrashOnTouch },
                        new Button { Name = "Big Stutter On Touch", Toggle = true, OnEnable = Overpowered.CrashOnYourTouch },

                        new Button { Name = "Strong Lag All", Toggle = true, OnEnable = Overpowered.StrongLagAll },
                        new Button { Name = "Strong Lag Gun", Toggle = true, OnEnable = Overpowered.StrongLagGun },
                        new Button { Name = "Strong Lag Aura", Toggle = true, OnEnable = Overpowered.StrongLagAura },
                        new Button { Name = "Strong Lag On Your Touch", Toggle = true, OnEnable = Overpowered.StrongLagTouch },
                        new Button { Name = "Strong Lag On Touch", Toggle = true, OnEnable = Overpowered.StrongLagOnYourTouch },
                        }
                    },
                    new Category {
                        Name = "Room Exploits",
                        Buttons = {
                        new Button { Name = "Stump Kick All [PRIV]", Toggle = false, OnEnable = Overpowered.StumpKickAll },
                        }
                    }
                },
                    Buttons = {
                    }
                },
                  new Category {
                    Name = "Sound",
                    Buttons = {
                        new Button { Name = "Bass Sound Spam", Toggle = true, OnEnable = Fun.BassSoundSpam },
                        new Button { Name = "Metal Sound Spam", Toggle = true, OnEnable = Fun.MetalSoundSpam },
                        new Button { Name = "Metal Sound Spam 2", Toggle = true, OnEnable = Fun.MetalSoundSpam2 },
                        new Button { Name = "Metal Sound Spam 3", Toggle = true, OnEnable = Fun.MetalSoundSpam3 },
                        new Button { Name = "Wolf Sound Spam", Toggle = true, OnEnable = Fun.WolfSoundSpam },
                        new Button { Name = "Cat Sound Spam", Toggle = true, OnEnable = Fun.CatSoundSpam },
                        new Button { Name = "Turkey Sound Spam", Toggle = true, OnEnable = Fun.TurkeySoundSpam },
                        new Button { Name = "Frog Sound Spam", Toggle = true, OnEnable = Fun.FrogSoundSpam },
                        new Button { Name = "Bee Sound Spam", Toggle = true, OnEnable = Fun.BeeSoundSpam },
                        new Button { Name = "Squeak Sound Spam", Toggle = true, OnEnable = Fun.SqueakSoundSpam },
                        new Button { Name = "Squeak Sound Spam 2", Toggle = true, OnEnable = Fun.SqueakSoundSpam2 },
                        new Button { Name = "Squeak Sound Spam 3", Toggle = true, OnEnable = Fun.SqueakSoundSpam3 },
                        new Button { Name = "Squeak Sound Spam 4", Toggle = true, OnEnable = Fun.SqueakSoundSpam4 },
                        new Button { Name = "Earrape Sound Spam", Toggle = true, OnEnable = Fun.EarrapeSoundSpam },
                        new Button { Name = "Ding Sound Spam", Toggle = true, OnEnable = Fun.DingSoundSpam },
                        new Button { Name = "Ding Sound Spam 2", Toggle = true, OnEnable = Fun.DingSoundSpam2 },
                        new Button { Name = "Piano Sound Spam", Toggle = true, OnEnable = Fun.PianoSoundSpam },
                        new Button { Name = "Big Crystal Sound Spam", Toggle = true, OnEnable = Fun.BigCrystalSoundSpam },
                        new Button { Name = "Pan Sound Spam", Toggle = true, OnEnable = Fun.PanSoundSpam },
                        new Button { Name = "AK-47 Sound Spam", Toggle = true, OnEnable = Fun.AK47SoundSpam },
                        new Button { Name = "Tick Sound Spam", Toggle = true, OnEnable = Fun.TickSoundSpam },
                        new Button { Name = "Random Sound Spam", Toggle = true, OnEnable = Fun.RandomSoundSpam },
                        new Button { Name = "Crystal Sound Spam", Toggle = true, OnEnable = Fun.CrystalSoundSpam },
                        new Button { Name = "Siren Sound Spam", Toggle = true, OnEnable = Fun.SirenSoundSpam },
                        new Button { Name = "Play Random Sounds", Toggle = true, OnEnable = Fun.PlayRandomSounds },
                        new Button { Name = "Static Sound Spam", Toggle = true, OnEnable = Fun.StaticSoundSpam },
                        new Button { Name = "Static Sound Spam 2", Toggle = true, OnEnable = Fun.StaticSoundSpam2 },
                        new Button { Name = "Static Sound Spam 3", Toggle = true, OnEnable = Fun.StaticSoundSpam3 },
                        new Button { Name = "Static Sound Spam 4", Toggle = true, OnEnable = Fun.StaticSoundSpam4 },
                        new Button { Name = "Static Sound Spam 5", Toggle = true, OnEnable = Fun.StaticSoundSpam5 },
                        new Button { Name = "Wood Sound Spam", Toggle = true, OnEnable = Fun.WoodSoundSpam },
                        new Button { Name = "Wood Sound Spam 2", Toggle = true, OnEnable = Fun.WoodSoundSpam2 },
                        new Button { Name = "Wood Sound Spam 3", Toggle = true, OnEnable = Fun.WoodSoundSpam3 },
                        new Button { Name = "Wood Sound Spam 4", Toggle = true, OnEnable = Fun.WoodSoundSpam4 },
                        new Button { Name = "Pop Sound Spam", Toggle = true, OnEnable = Fun.PopSoundSpam },
                        new Button { Name = "Carpet Sound Spam", Toggle = true, OnEnable = Fun.CarpetSoundSpam },
                        new Button { Name = "Scary Sound Spam", Toggle = true, OnEnable = Fun.ScarySoundSpam },

                        new Button { Name = "Play Jman Scream", Toggle = false, OnEnable = Fun.PlayJmanYell },
                        new Button { Name = "Spam Jman Scream", Toggle = true, OnEnable = Fun.SpamJmanYell },

                    }
                },
                    new Category {
                    Name = "World",
                    Buttons = {
                        new Button { Name = "Hoverboard Spammer", Toggle = true, OnEnable = Fun.HoverboardSpammer },
                        new Button { Name = "Hoverboard Minigun", Toggle = true, OnEnable = Fun.HoverboardMinigun },
                        new Button { Name = "Hoverboard Sniper", Toggle = true, OnEnable = Fun.HoverboardSniper },
                        new Button { Name = "Hoverboard Blast", Toggle = true, OnEnable = Fun.HoverboardSniper2 },

                        new Button { Name = "Hoverboard Gun", Toggle = true, OnEnable = Fun.HoverboardGun },

                        new Button { Name = "Grab Bug", Toggle = true, OnEnable = Fun.GrabBug },
                        new Button { Name = "Orbit Bug", Toggle = true, OnEnable = Fun.OrbitBug },
                        new Button { Name = "Spaz Bug", Toggle = true, OnEnable = Fun.SpazBug },
                        new Button { Name = "Destroy Bug", Toggle = false, OnEnable = Fun.DestroyBug },

                        new Button { Name = "Grab Bat", Toggle = true, OnEnable = Fun.GrabBat },
                        new Button { Name = "Orbit Bat", Toggle = true, OnEnable = Fun.OrbitBat },
                        new Button { Name = "Spaz Bat", Toggle = true, OnEnable = Fun.SpazBat },
                        new Button { Name = "Destroy Bat", Toggle = false, OnEnable = Fun.DestroyBat },

                        new Button { Name = "Grab Firefly", Toggle = true, OnEnable = Fun.GrabFirefly },
                        new Button { Name = "Orbit Firefly", Toggle = true, OnEnable = Fun.OrbitFirefly },
                        new Button { Name = "Spaz Firefly", Toggle = true, OnEnable = Fun.SpazFirefly },
                        new Button { Name = "Destroy Firefly", Toggle = false, OnEnable = Fun.DestroyFirefly },

                        new Button { Name = "Grab All Gliders", Toggle = true, OnEnable = Fun.GrabGlider },
                        new Button { Name = "Orbit All Gliders", Toggle = true, OnEnable = Fun.OrbitGlider },
                        new Button { Name = "Spaz All Gliders", Toggle = true, OnEnable = Fun.SpazGlider },
                        new Button { Name = "Destroy All Gliders", Toggle = false, OnEnable = Fun.DestroyGlider },

                        new Button { Name = "Grab All Balloons", Toggle = true, OnEnable = Fun.GrabBalloons },
                        new Button { Name = "Orbit All Balloons", Toggle = true, OnEnable = Fun.OrbitBalloons },
                        new Button { Name = "Spaz All Balloons", Toggle = true, OnEnable = Fun.SpazBalloons },
                        new Button { Name = "Destroy All Balloons", Toggle = false, OnEnable = Fun.DestroyBalloons },
                        new Button { Name = "Pop All Balloons", Toggle = false, OnEnable = Fun.PopAllBalloons },

                        new Button { Name = "Spam Paper Planes", Toggle = true, OnEnable = Fun.SpamPaperPlanes },
                        new Button { Name = "Rapid Paper Planes", Toggle = true, OnEnable = Fun.RapidPaperPlanes },
                        new Button { Name = "Paper Plane Gun", Toggle = true, OnEnable = Fun.PaperPlaneGun },
                        new Button { Name = "Infinite Paper Planes", Toggle = false, OnEnable = Fun.InfinitePaperPlanes },
                        new Button { Name = "Paper Plane Barrage", Toggle = false, OnEnable = Fun.PaperPlaneBarrage },

                        new Button { Name = "Force Fire Ship", Toggle = false, OnEnable = Fun.ForceFireRCShip },
                        new Button { Name = "Rapid Fire Ship", Toggle = false, OnEnable = Fun.RapidFireRCShip },
                        new Button { Name = "RC Ship Gun", Toggle = true, OnEnable = Fun.RCShipGun },
                        new Button { Name = "Boost Ship Speed", Toggle = false, OnEnable = Fun.BoostRCShipSpeed },
                        new Button { Name = "Boost Ship Gun", Toggle = true, OnEnable = Fun.BoostRCShipGun },
                        new Button { Name = "Launch Ship Up", Toggle = false, OnEnable = Fun.LaunchRCShipUp },
                        new Button { Name = "Ship Barrage", Toggle = false, OnEnable = Fun.RCShipBarrage },

                    }
                },
                new Category {
                    Name = "Projectiles",
                    Buttons = {
                        new Button { Name = "Anti Snowball Fing", Toggle = true, OnEnable = () => GameplayPatches.CheckForAOEKnockbackPatch.Fling = false, OnDisable = () => GameplayPatches.CheckForAOEKnockbackPatch.Fling = true },
                        new Button { Name = "srry next update :3", Toggle = false, Label = true },
                        /*new Button { Name = "Big Snowball Spammer", Toggle = true, OnEnable = Projectiles.GrowingSpammer },
                        new Button { Name = "Big Snowball Minigun", Toggle = true, OnEnable = Projectiles.GrowingMinigun },
                        new Button { Name = "Big Snowball Fling Gun", Toggle = true, OnEnable = Projectiles.GrowingFlingGun },*/
                    }
                },
                new Category {
                    Name = "Soundboard",
                    Buttons = {

                    }
                },

                new Category {
                    Name = "Credits",
                    Buttons = {
                        new Button { Name = "g3if: Founder", Toggle = false, Label = true },
                        new Button { Name = "Conetic: Contributor", Toggle = false, Label = true },
                        new Button { Name = "made with love <3", Toggle = false, Label = true },
                        new Button { Name = "Status : Undetected", Toggle = false, Label = true },
                    }
                }
            };
        }
    }
}



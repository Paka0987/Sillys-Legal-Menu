using ExitGames.Client.Photon;
using g3;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using GorillaTagScripts;
using GorillaTagScripts.VirtualStumpCustomMaps;
using HarmonyLib;
using Ionic.Zlib;
using Liv.Lck.Tablet;
using Mono.Security.Cryptography;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EventsModels;
using POpusCodec.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Technie.PhysicsCreator.Skinned;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static OVRColocationSession;
using static SuperInfectionManager;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.InputSystem.DefaultInputActions;
using Application = UnityEngine.Application;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Text = UnityEngine.UI.Text;

namespace Juul.Mods
{
    internal class CustomMaps
    {
        public static float delay = 0f;
        private static int currentMaterialIndex = 0;
        
        public static void ImportCustomScript(Dictionary<int, string> replacements)
        {
            string[] lines = CustomGameMode.LuaScript.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var kvp in replacements)
            {
                if (kvp.Key >= 0 && kvp.Key < lines.Length)
                    lines[kvp.Key] = kvp.Value;
            }
            CustomGameMode.LuaScript = string.Join(Environment.NewLine, lines);
            CustomGameMode.StopScript();
            CustomGameMode.LuaStart();
            LuauHud.Instance.RestartLuauScript();
            CustomMapManager.ReturnToVirtualStump();
        }
        public static void DeportCustomScript(int lineNumber)
        {
            string originalScript = CustomMapLoader.GetLuauGamemodeScript();
            if (string.IsNullOrEmpty(originalScript))
                return;
            string[] lines = originalScript.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lineNumber >= 0 && lineNumber < lines.Length)
            {
                string[] currentLines = CustomGameMode.LuaScript.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                if (lineNumber < currentLines.Length)
                {
                    currentLines[lineNumber] = lines[lineNumber];
                    CustomGameMode.LuaScript = string.Join(Environment.NewLine, currentLines);
                    CustomGameMode.StopScript();
                    CustomGameMode.LuaStart();
                    LuauHud.Instance.RestartLuauScript();
                }
            }
        }
        public static void GodMode()
        {
            ImportCustomScript(new Dictionary<int, string>
            {
                { 957, "if not IsMe then PlayerData[Player.playerID].Health -= Modules.roundToQuarter(dmg) end" }
            });
        }

        public static void DisableGodMode() => DeportCustomScript(957);
        public static void NoGrenadeCooldown()
        {
            ImportCustomScript(new Dictionary<int, string>
            {
                { 1296, "grenadeCooldown = 0" }
            });
        }

        public static void DisableNoGrenadeCooldown() => DeportCustomScript(1296);
        public static void NoShootCooldown()
        {
            ImportCustomScript(new Dictionary<int, string>
            {
                { 1243, "shootCooldown = 0" }
            });
        }

        public static void DisableNoShootCooldown() => DeportCustomScript(1243);

        public static void InfiniteAmmo()
        {
            ImportCustomScript(new Dictionary<int, string>
            {
                { 1244, "" }
            });
        }

        public static void DisableInfiniteAmmo() => DeportCustomScript(1244);

        public static void InstantKill()
        {
            ImportCustomScript(new Dictionary<int, string>
            {
                { 1278, "emitAndOnEvent(\"HitPlayer\", {found.playerID, 99999.0, LocalPlayer.playerID})" }
            });
        }

        public static void DisableInstantKill() => DeportCustomScript(1278);

        public static void InfinitePoints()
        {
            ImportCustomScript(new Dictionary<int, string>
            {
                { 496, "saveData[\"Points\"] = 999999" }
            });
        }

        public static void DisableInfinitePoints() => DeportCustomScript(496);

        public static void RapidFire()
        {
            ImportCustomScript(new Dictionary<int, string>
            {
                { 2041, "needsLetGoR = false" }
            });
        }

        public static void DisableRapidFire() => DeportCustomScript(2041);
     














        public static void KillPlayer(int actorNumber)
        {
            PhotonNetwork.RaiseEvent(180, new object[] { "HitPlayer", (double)actorNumber, false, (double)actorNumber },
                new RaiseEventOptions { TargetActors = new[] { actorNumber } }, SendOptions.SendReliable);
        }

        public static void KillSelf()
        {
            KillPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
        }

        public static void KillGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                    KillPlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
            }, true);
        }

        public static void KillAll()
        {
            foreach (var player in PhotonNetwork.PlayerListOthers)
                KillPlayer(player.ActorNumber);
        }

        public static void KillAura()
        {
            List<VRRig> vrriglist = new List<VRRig>();
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if ((Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f && vrrig != GorillaTagger.Instance.offlineVRRig))
                {
                    vrriglist.Add(vrrig);
                }
                foreach (VRRig rigs in vrriglist)
                {
                    KillPlayer(rigs.OwningNetPlayer.ActorNumber);
                }
            }
        }

        public static void KillOnTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f))
                {
                    KillPlayer(vrrig.OwningNetPlayer.ActorNumber);
                }
            }
        }

        public static void KillOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    KillPlayer(vrrig.OwningNetPlayer.ActorNumber);
                }
            }
        }
        public static void CrashPlayer(int actorNumber)
        {
            PhotonNetwork.RaiseEvent(180, new object[] { "leaveGame", (double)actorNumber, false, (double)actorNumber },
                new RaiseEventOptions { TargetActors = new[] { actorNumber } }, SendOptions.SendReliable);
        }
        public static void CrashGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                    CrashPlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
            }, true);
        }

        public static void CrashAll()
        {
            foreach (var player in PhotonNetwork.PlayerListOthers)
                CrashPlayer(player.ActorNumber);
        }
        public static void LightningStrikePlayer(int actorNumber)
        {
            PhotonNetwork.RaiseEvent(180, new object[] { "SummonThunder", (double)actorNumber },
                new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }

        public static void LightningStrikeSelf()
        {
            if (Time.time < delay) return;
            delay = Time.time + 0.5f;
            LightningStrikePlayer(PhotonNetwork.LocalPlayer.ActorNumber);
        }

        public static void LightningStrikeGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (Time.time < delay) return;
                if (GunLib.LockedPlayer != null)
                {
                    delay = Time.time + 0.5f;
                    LightningStrikePlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
                }
            }, true);
        }
        public static void LightningStrikeAll()
        {
            if (Time.time < delay) return;
            delay = Time.time + 0.5f;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                LightningStrikePlayer(player.ActorNumber);
            }
        }
        public static void LightningAura()
        {
            List<VRRig> vrriglist = new List<VRRig>();
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if ((Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f && vrrig != GorillaTagger.Instance.offlineVRRig))
                {
                    vrriglist.Add(vrrig);
                }
                foreach (VRRig rigs in vrriglist)
                {
                    PhotonNetwork.RaiseEvent(180, new object[] { "SummonThunder", (double)rigs.OwningNetPlayer.ActorNumber },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                }
            }
        }

        public static void LightningOnTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f))
                {
                    PhotonNetwork.RaiseEvent(180, new object[] { "SummonThunder", (double)vrrig.OwningNetPlayer.ActorNumber },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                }
            }
        }

        public static void LightningOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    PhotonNetwork.RaiseEvent(180, new object[] { "SummonThunder", (double)vrrig.OwningNetPlayer.ActorNumber },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                }
            }
        }
        public static void ChangeMaterialPlayer(int actorNumber, int materialIndex)
        {
            PhotonNetwork.RaiseEvent(180, new object[] { "ChangingMaterial", (double)actorNumber, (double)materialIndex },
                new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }

        public static void ChangeMaterialSelf()
        {
            if (Time.time < delay) return;
            delay = Time.time + 0.5f;
            currentMaterialIndex = (currentMaterialIndex + 1) % 10;
            ChangeMaterialPlayer(PhotonNetwork.LocalPlayer.ActorNumber, currentMaterialIndex);
        }

        public static void ChangeMaterialGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (Time.time < delay) return;
                if (GunLib.LockedPlayer != null)
                {
                    delay = Time.time + 0.5f;
                    currentMaterialIndex = (currentMaterialIndex + 1) % 10;
                    ChangeMaterialPlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber, currentMaterialIndex);
                }
            }, true);
        }

        public static void ChangeMaterialAll()
        {
            if (Time.time < delay) return;
            delay = Time.time + 0.5f;
            currentMaterialIndex = (currentMaterialIndex + 1) % 10;

            foreach (var player in PhotonNetwork.PlayerList)
            {
                ChangeMaterialPlayer(player.ActorNumber, currentMaterialIndex);
            }
        }

        public static void RandomMaterialGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (Time.time < delay) return;
                if (GunLib.LockedPlayer != null)
                {
                    delay = Time.time + 0.5f;
                    int randomMat = Random.Range(0, 10);
                    ChangeMaterialPlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber, randomMat);
                }
            }, true);
        }
        public static void MaterialAura()
        {
            List<VRRig> vrriglist = new List<VRRig>();
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if ((Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f && vrrig != GorillaTagger.Instance.offlineVRRig))
                {
                    vrriglist.Add(vrrig);
                }
                foreach (VRRig rigs in vrriglist)
                {
                    currentMaterialIndex = (currentMaterialIndex + 1) % 10;
                    PhotonNetwork.RaiseEvent(180, new object[] { "ChangingMaterial", (double)rigs.OwningNetPlayer.ActorNumber, (double)currentMaterialIndex },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                }
            }
        }

        public static void MaterialOnTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f))
                {
                    currentMaterialIndex = (currentMaterialIndex + 1) % 10;
                    PhotonNetwork.RaiseEvent(180, new object[] { "ChangingMaterial", (double)vrrig.OwningNetPlayer.ActorNumber, (double)currentMaterialIndex },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                }
            }
        }

        public static void MaterialOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    currentMaterialIndex = (currentMaterialIndex + 1) % 10;
                    PhotonNetwork.RaiseEvent(180, new object[] { "ChangingMaterial", (double)vrrig.OwningNetPlayer.ActorNumber, (double)currentMaterialIndex },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                }
            }
        }
        public static void SpawnLucyOnPlayer(int actorNumber)
        {
            PhotonNetwork.RaiseEvent(180, new object[] { "SummonLucy", (double)actorNumber },
                new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }

        public static void SpawnLucySelf()
        {
            SpawnLucyOnPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
        }

        public static void SpawnLucyGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    SpawnLucyOnPlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
                }
            }, true);
        }

        public static void SpawnLucyAll()
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                SpawnLucyOnPlayer(player.ActorNumber);
            }
        }
        public static void SpawnLucyAura()
        {
            List<VRRig> vrriglist = new List<VRRig>();
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if ((Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f && vrrig != GorillaTagger.Instance.offlineVRRig))
                {
                    vrriglist.Add(vrrig);
                }
                foreach (VRRig rigs in vrriglist)
                {
                    PhotonNetwork.RaiseEvent(180, new object[] { "SummonLucy", (double)rigs.OwningNetPlayer.ActorNumber },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                }
            }
        }

        public static void SpawnLucyOnTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f))
                {
                    PhotonNetwork.RaiseEvent(180, new object[] { "SummonLucy", (double)vrrig.OwningNetPlayer.ActorNumber },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                }
            }
        }

        public static void SpawnLucyOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    PhotonNetwork.RaiseEvent(180, new object[] { "SummonLucy", (double)vrrig.OwningNetPlayer.ActorNumber },
                        new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
                }
            }
        }
        public static void InstantKillGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (Time.time < delay) return;
                if (GunLib.LockedPlayer != null)
                {
                    delay = Time.time + 0.5f;
                    ImportCustomScript(new Dictionary<int, string>
            {
                { 1278, $"emitAndOnEvent(\"HitPlayer\", {{{GunLib.LockedPlayer.OwningNetPlayer.ActorNumber}, 99999.0, LocalPlayer.playerID}})" }
            });
                }
            }, true);
        }












    }
}


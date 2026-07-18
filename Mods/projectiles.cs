using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GorillaNetworking;
using GorillaTagScripts;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace Juul.Mods
{
    public class Projectiles
    {
        public static bool GrabbedGrowing = false;
        public static RaiseEventOptions eventOptions = null;

        private static int projCount;
        private static int projCount2;
        private static int projCount3;
        private static int projCount4;
        private static int projCount5;
        public static float lastSendTime;

        public static Dictionary<string, SnowballThrowable>? projlist;
        public static bool allSnowballsInitialized;

        public static SnowballThrowable? GetProjectile(string projectileName)
        {
            var throwables = ((AllCosmeticsArraySO)CosmeticsController.instance.v2_allCosmeticsInfoAssetRef.Asset).sturdyAssetRefs.Where(x => x.obj != null && x.obj.info.isThrowable).Select(x => x.obj.info.playFabID).Distinct().ToList();
            if (projlist == null || projlist.Count != (throwables.Count - 3))
            {
                if (!CosmeticsV2Spawner_Dirty.isPrepared)
                    return null;

                if (!GorillaComputer.instance.isConnectedToMaster)
                    return null;

                if (!allSnowballsInitialized && (CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringLeft.Count >= 1 && CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringRight.Count >= 1))
                {
                    allSnowballsInitialized = true;

                    CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringLeft.ForEach(v => VRRig.LocalRig.cosmeticsObjectRegistry.Cosmetic(v.Value));
                    CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringRight.ForEach(v => VRRig.LocalRig.cosmeticsObjectRegistry.Cosmetic(v.Value));

                    return null;
                }

                projlist = new Dictionary<string, SnowballThrowable>();
                foreach (SnowballMaker Maker in new[] { SnowballMaker.leftHandInstance, SnowballMaker.rightHandInstance })
                {
                    foreach (SnowballThrowable Throwable in Maker.snowballs)
                    {
                        try
                        {
                            string key = Throwable.transform.parent.gameObject.name;
                            projlist.Add(key, Throwable);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to add projectile to projlist: {e.Message}");
                        }
                    }
                }
            }

            projectileName += "(Clone)";
            if (!projlist.TryGetValue(projectileName, out var projectile))
            {
                Debug.LogWarning($"Projectile not found: {projectileName}");
                return null;
            }

            return projectile;
        }

        public static void ReleaseSnowball()
        {
            try
            {
                var rightSnowball = GetProjectile("GrowingSnowballRightAnchor");
                if (rightSnowball != null && rightSnowball.gameObject != null) rightSnowball.gameObject.SetActive(false);
            }
            catch { }
        }

        public static IEnumerator DisableSnowball(bool rigDisabled)
        {
            yield return new WaitForSeconds(0.3f);

            if (rigDisabled)
                VRRig.LocalRig.enabled = true;

            foreach (var snowball in projlist.Values)
                try
                {
                    snowball.SetSnowballActiveLocal(false);
                }
                catch { }
        }

        public static RaiseEventOptions GetOption(bool all)
        {
            eventOptions = new RaiseEventOptions()
            {
                Receivers = all ? ReceiverGroup.All : ReceiverGroup.Others
            };

            return eventOptions;
        }

        public static RaiseEventOptions GetPlayerOption(int act)
        {
            eventOptions = new RaiseEventOptions()
            {
                TargetActors = new int[] { act }
            };

            return eventOptions;
        }

        public static bool isrignull(Player p)
        {
            if (p == null)
            {
                return true;
            }
            return false;
        }

        public static int ProjectileCount(Vector3 Position, Vector3 Velocity, float Scale)
        {
            try
            {
                SlingshotProjectile projectile = null;

                ProjectileCount2(Position, Velocity, Scale);

                var data = ProjectileTracker.AddAndIncrementLocalProjectile(projectile, Velocity, Position, Scale);
                projCount = data;

                return data;
            }
            catch
            {
                projCount++;
                return projCount;
            }
        }

        private static int ProjectileCount2(Vector3 Position, Vector3 Velocity, float Scale)
        {
            try
            {
                SlingshotProjectile projectile = null;

                ProjectileCount3(Position, Velocity, Scale);

                var data = ProjectileTracker.m_localProjectiles.AddAndIncrement(
                    new ProjectileTracker.ProjectileInfo(PhotonNetwork.Time, Velocity, Position, Scale, projectile));
                projCount2 = data;

                return data;
            }
            catch
            {
                projCount2++;
                return projCount2;
            }
        }

        private static int ProjectileCount3(Vector3 Position, Vector3 Velocity, float Scale)
        {
            try
            {
                SlingshotProjectile projectile = null;

                ProjectileCount4(Position, Velocity, Scale);

                var data = ProjectileTracker.m_localProjectiles.IncrementAndAdd(
                    new ProjectileTracker.ProjectileInfo(PhotonNetwork.Time, Velocity, Position, Scale, projectile));
                projCount3 = data;

                return data;
            }
            catch
            {
                projCount3++;
                return projCount3;
            }
        }

        private static int ProjectileCount4(Vector3 Position, Vector3 Velocity, float Scale)
        {
            try
            {
                ProjectileCount5(Position, Velocity, Scale);

                var data = ProjectileTracker.m_playerProjectiles.Count();
                projCount4 = data;

                return data;
            }
            catch
            {
                projCount4++;
                return projCount4;
            }
        }

        private static int ProjectileCount5(Vector3 Position, Vector3 Velocity, float Scale)
        {
            try
            {
                var data = ProjectileTracker.m_projectileInfoPool.m_size;
                projCount5 = data;

                return data;
            }
            catch
            {
                projCount5++;
                return projCount5;
            }
        }

        public static void SendGrowing(Vector3 Position, Vector3 Velocity, bool Fling = false, NetPlayer p = null)
        {
            if (!PhotonNetwork.InRoom)
                return;

            GrowingSnowballThrowable throwable = GetProjectile("GrowingSnowballRightAnchor")?.GetComponent<GrowingSnowballThrowable>();
            if (!GrabbedGrowing)
            {
                throwable?.SetSnowballActiveLocal(true);
                throwable?.IncreaseSize(5);
                if (throwable != null) throwable.transform.position = Position;
                GrabbedGrowing = true;
            }
            RaiseEventOptions options = new RaiseEventOptions();
            if (p != null)
                options.TargetActors = new int[] { p.ActorNumber };
            else
                options.Receivers = ReceiverGroup.All;

            if (Fling && p != null)
            {
                if (Vector3.Distance(VRRig.LocalRig.transform.position, Rigs.GetVRRigFromNetPlayer(p).transform.position) > 4f)
                {
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = Rigs.GetVRRigFromNetPlayer(p).bodyTransform.position + new Vector3(0, 0.2f, 0);
                }
            }

            if (GrabbedGrowing && Time.time > lastSendTime)
            {
                if (throwable != null)
                {
                    PhotonNetwork.RaiseEvent(176, new object[]
                    {
                        throwable.changeSizeEvent._eventId,
                        5
                    }, options, new SendOptions()
                    {
                        Reliability = false
                    });

                    PhotonNetwork.RaiseEvent(176, new object[]
                    {
                        throwable.snowballThrowEvent._eventId,
                        Position,
                        Velocity,
                        ProjectileCount(Position, Velocity, throwable.transform.lossyScale.x),
                        null
                    }, options, new SendOptions()
                    {
                        Reliability = false
                    });
                }
                lastSendTime = Time.time + 0.2f;
            }

            if (GrabbedGrowing)
            {
                ReleaseSnowball();
                GorillaTagger.Instance.StartCoroutine(DisableSnowball(true));
            }
        }

        public static void GrowingSpammer()
        {
            if (ControllerInputPoller.instance.rightGrab || Mouse.current.rightButton.isPressed)
            {
                SendGrowing(GorillaTagger.Instance.rightHandTransform.transform.position, Vector3.zero);
            }
        }

        public static void GrowingMinigun()
        {
            if (ControllerInputPoller.instance.rightGrab || Mouse.current.rightButton.isPressed)
            {
                SendGrowing(GorillaTagger.Instance.rightHandTransform.transform.position, GorillaTagger.Instance.rightHandTransform.transform.forward * 22);
            }
        }

        public static void GrowingFlingGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    SendGrowing(GunLib.LockedPlayer.transform.position, Vector3.down * 100, true, Rigs.GetNetPlayerFromVRRig(GunLib.LockedPlayer));
                }
            }, true);
        }
    }
}

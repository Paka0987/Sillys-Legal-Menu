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
using static Juul.Patches;
using static OVRColocationSession;
using static SuperInfectionManager;
using static TransferrableObject;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.InputSystem.DefaultInputActions;
using Application = UnityEngine.Application;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Text = UnityEngine.UI.Text;

namespace Juul
{
    public class Overpowered : MonoBehaviour
    {
        

        public static void GetOwnerOfSIEntity()
        {
            try
            {
                RequestableOwnershipGuard guard = SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.guard;
                guard.SetCreator(NetworkSystem.Instance.LocalPlayer);
                guard.SetCurrentOwner(NetworkSystem.Instance.LocalPlayer);
                if (SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.IsAuthorityPlayer(PhotonNetwork.LocalPlayer))
                {
                    NotifiLib.SendNotification("WORKED HAVE FUN!!!");
                }
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        public static void DebugSpawnedEntitys()
        {
            if (PhotonNetwork.InRoom)
            {
                if (GameMode.IsPlaying(GameModeType.SuperInfect))
                {
                    foreach (GameEntity game in SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.GetGameEntities())
                    {
                        Debug.Log($"name : {game.gameObject.name}\n");
                        Debug.Log($"id : {game.typeId}\n ");
                    }
                }
            }
        }

        public static float delay = 0.5f;
        public static float lagcd = 0f;
        public static int lagPowerIndex = 0;


        public static void SpamDeploy(DeployableObject deployable)
        {
            var value = Traverse.Create(deployable).Field("_onDeploy").GetValue<UnityEvent>();
            for (var i = 0; i > Random.Range(500, 50000); i++)
                if (value != null)
                    value.Invoke();
        }

        public static void NetSerialize(int targetActor)
        {
            var method = typeof(CustomSerializer).GetMethod("SerializeNetEventOptions",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(null, new[]
                {
                 new object(),
                 new NetEventOptions
                 {
                     TargetActors = new[]
                     {
                         targetActor
                     }
                 }});
            }
        }
        public static VRRig targetFlinged;
        public static int barrelFlingMode = 1;

        public static string GetBarrelFlingMethodName()
        {
            switch (barrelFlingMode)
            {
                case 0: return "0.5x";
                case 1: return "1x";
                case 2: return "2x";
                case 3: return "5x";
                case 4: return "10x";
                default: return "1x";
            }
        }

        public static void ChangeBarrelMethod(bool forward)
        {
            if (forward)
                barrelFlingMode = (barrelFlingMode + 1) % 5;
            else
                barrelFlingMode = (barrelFlingMode - 1 + 5) % 5;

            if (ExtraButtons.BarrelMethodButton != null)
                ExtraButtons.BarrelMethodButton.Name = $"Barrel Strength: {GetBarrelFlingMethodName()}";
        }

        public static int lagMethod = 0;

        public static string GetLagMethodName()
        {
            switch (lagMethod)
            {
                case 0: return "Method 1";
                case 1: return "Method 2";
                default: return "Method 1";
            }
        }

        public static void ChangeLagMethod(bool forward)
        {
            if (forward)
                lagMethod = (lagMethod + 1) % 2;
            else
                lagMethod = (lagMethod - 1 + 2) % 2;

            if (ExtraButtons.LagMethodButton != null)
                ExtraButtons.LagMethodButton.Name = $"Lag Method: {GetLagMethodName()}";
        }

        public static DeployableObject GetBarrelDeployable()
        {
            if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
            {
                foreach (var obj in GorillaTagger.Instance.offlineVRRig.GetComponentsInChildren<DeployableObject>(true))
                {
                    if (obj.gameObject.name == "LMAPE.") return obj;
                }
            }
            return null;
        }

        public static void SendBarrelFling(int Mode)
        {
            switch (Mode)
            {
                case 0:
                    BarrelFlingMethod1();
                    break;
                case 1:
                    BarrelFlingMethod2();
                    break;
                case 2:
                    BarrelFlingMethod3();
                    break;
                case 3:
                    BarrelFlingMethod4();
                    break;
                case 4:
                    BarrelFlingMethod5();
                    break;
            }
        }
        public static void BarrelFlingMethod1()
        {
            if (targetFlinged == null || targetFlinged.isLocal) return;

            var deployable = GetBarrelDeployable();
 

            if (deployable == null) return;

            var child = Traverse.Create(deployable).Field("_child").GetValue<DeployedChild>();
            if (child == null) return;

            var rb = Traverse.Create(child).Field("_rigidbody").GetValue<Rigidbody>();
            if (rb == null) return;

            var data = PhotonUtils.FetchScratchArray(5);
            data[0] = (int)typeof(DeployableObject)
                .GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(deployable)
                .GetType().GetField("_signalID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    typeof(DeployableObject).GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(deployable));
            data[1] = PhotonNetwork.ServerTimestamp;
            data[2] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.position +
                                                          new Vector3(0, -0.2f, 0));
            data[3] = 469893376;
            data[4] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.up * 500f);

            PhotonNetwork.RaiseEvent(177, data,
                TargetedWCO(targetFlinged.Creator.ActorNumber, EventCaching.AddToRoomCacheGlobal), STS());

            child.Deploy(deployable, targetFlinged.bodyTransform.position + new Vector3(0, -0.2f, 0),
                Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)),
                targetFlinged.bodyTransform.up * 500f);
            deployable.DeployChild();
            SpamDeploy(deployable);

            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.detectCollisions = false;
            rb.velocity = targetFlinged.bodyTransform.up * 500f;

            var barrelObj = GetBarrelDeployable()?.gameObject;

            if (barrelObj != null)
                barrelObj.transform.rotation =
                    Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            for (var i = 0; i < 4; i++)
            {
                rb.AddForce(targetFlinged.bodyTransform.up * 500f, ForceMode.Force);
                rb.AddForce(targetFlinged.bodyTransform.up * 500f, ForceMode.Impulse);
                rb.AddForce(targetFlinged.bodyTransform.up * 500f, ForceMode.VelocityChange);
                rb.AddForce(targetFlinged.bodyTransform.up * 500f, ForceMode.Acceleration);
            }
            child.ReturnToParent(2f);
        }
        public static void BarrelFlingMethod2()
        {
            if (targetFlinged == null || targetFlinged.isLocal) return;

            var deployable = GetBarrelDeployable();

            if (deployable == null) return;

            var child = Traverse.Create(deployable).Field("_child").GetValue<DeployedChild>();
            if (child == null) return;

            var rb = Traverse.Create(child).Field("_rigidbody").GetValue<Rigidbody>();
            if (rb == null) return;

            var data = PhotonUtils.FetchScratchArray(5);
            data[0] = (int)typeof(DeployableObject)
                .GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(deployable)
                .GetType().GetField("_signalID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    typeof(DeployableObject).GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(deployable));
            data[1] = PhotonNetwork.ServerTimestamp;
            data[2] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.position +
                                                          new Vector3(0, -0.2f, 0));
            data[3] = 469893376;
            data[4] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.up * 9999.99f);

            PhotonNetwork.RaiseEvent(177, data,
                TargetedWCO(targetFlinged.Creator.ActorNumber, EventCaching.AddToRoomCacheGlobal), STS());

            child.Deploy(deployable, targetFlinged.bodyTransform.position + new Vector3(0, -0.2f, 0),
                Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)),
                targetFlinged.bodyTransform.up * 9999.99f);
            deployable.DeployChild();
            SpamDeploy(deployable);

            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.detectCollisions = false;
            rb.velocity = targetFlinged.bodyTransform.up * 9999.99f;

            var barrelObj = GetBarrelDeployable()?.gameObject;

            if (barrelObj != null)
                barrelObj.transform.rotation =
                    Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            for (var i = 0; i < 4; i++)
            {
                rb.AddForce(targetFlinged.bodyTransform.up * 9999.99f, ForceMode.Force);
                rb.AddForce(targetFlinged.bodyTransform.up * 9999.99f, ForceMode.Impulse);
                rb.AddForce(targetFlinged.bodyTransform.up * 9999.99f, ForceMode.VelocityChange);
                rb.AddForce(targetFlinged.bodyTransform.up * 9999.99f, ForceMode.Acceleration);
            }
            child.ReturnToParent(2f);
        }
        public static void BarrelFlingMethod3()
        {
            if (targetFlinged == null || targetFlinged.isLocal) return;

            var deployable = GetBarrelDeployable();

            if (deployable == null) return;

            var child = Traverse.Create(deployable).Field("_child").GetValue<DeployedChild>();
            if (child == null) return;

            var rb = Traverse.Create(child).Field("_rigidbody").GetValue<Rigidbody>();
            if (rb == null) return;

            var data = PhotonUtils.FetchScratchArray(5);
            data[0] = (int)typeof(DeployableObject)
                .GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(deployable)
                .GetType().GetField("_signalID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    typeof(DeployableObject).GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(deployable));
            data[1] = PhotonNetwork.ServerTimestamp;
            data[2] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.position +
                                                          new Vector3(0, -0.2f, 0));
            data[3] = 469893376;
            data[4] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.up * 2000f);

            PhotonNetwork.RaiseEvent(177, data,
                TargetedWCO(targetFlinged.Creator.ActorNumber, EventCaching.AddToRoomCacheGlobal), STS());

            child.Deploy(deployable, targetFlinged.bodyTransform.position + new Vector3(0, -0.2f, 0),
                Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)),
                targetFlinged.bodyTransform.up * 2000f);
            deployable.DeployChild();
            SpamDeploy(deployable);

            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.detectCollisions = false;
            rb.velocity = targetFlinged.bodyTransform.up * 2000f;

            var barrelObj = GetBarrelDeployable()?.gameObject;

            if (barrelObj != null)
                barrelObj.transform.rotation =
                    Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            for (var i = 0; i < 4; i++)
            {
                rb.AddForce(targetFlinged.bodyTransform.up * 2000f, ForceMode.Force);
                rb.AddForce(targetFlinged.bodyTransform.up * 2000f, ForceMode.Impulse);
                rb.AddForce(targetFlinged.bodyTransform.up * 2000f, ForceMode.VelocityChange);
                rb.AddForce(targetFlinged.bodyTransform.up * 2000f, ForceMode.Acceleration);
            }
            child.ReturnToParent(2f);
        }
        public static void BarrelFlingMethod4()
        {
            if (targetFlinged == null || targetFlinged.isLocal) return;

            var deployable = GetBarrelDeployable();

            if (deployable == null) return;

            var child = Traverse.Create(deployable).Field("_child").GetValue<DeployedChild>();
            if (child == null) return;

            var rb = Traverse.Create(child).Field("_rigidbody").GetValue<Rigidbody>();
            if (rb == null) return;

            var data = PhotonUtils.FetchScratchArray(5);
            data[0] = (int)typeof(DeployableObject)
                .GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(deployable)
                .GetType().GetField("_signalID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    typeof(DeployableObject).GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(deployable));
            data[1] = PhotonNetwork.ServerTimestamp;
            data[2] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.position +
                                                          new Vector3(0, -0.2f, 0));
            data[3] = 469893376;
            data[4] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.up * 5000f);

            PhotonNetwork.RaiseEvent(177, data,
                TargetedWCO(targetFlinged.Creator.ActorNumber, EventCaching.AddToRoomCacheGlobal), STS());

            child.Deploy(deployable, targetFlinged.bodyTransform.position + new Vector3(0, -0.2f, 0),
                Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)),
                targetFlinged.bodyTransform.up * 5000f);
            deployable.DeployChild();
            SpamDeploy(deployable);

            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.detectCollisions = false;
            rb.velocity = targetFlinged.bodyTransform.up * 5000f;

            var barrelObj = GetBarrelDeployable()?.gameObject;

            if (barrelObj != null)
                barrelObj.transform.rotation =
                    Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            for (var i = 0; i < 10; i++)
            {
                rb.AddForce(targetFlinged.bodyTransform.up * 5000f, ForceMode.Force);
                rb.AddForce(targetFlinged.bodyTransform.up * 5000f, ForceMode.Impulse);
                rb.AddForce(targetFlinged.bodyTransform.up * 5000f, ForceMode.VelocityChange);
                rb.AddForce(targetFlinged.bodyTransform.up * 5000f, ForceMode.Acceleration);
            }
            child.ReturnToParent(2f);
        }

        public static void BarrelFlingMethod5()
        {
            if (targetFlinged == null || targetFlinged.isLocal) return;

            var deployable = GetBarrelDeployable();

            if (deployable == null) return;

            var child = Traverse.Create(deployable).Field("_child").GetValue<DeployedChild>();
            if (child == null) return;

            var rb = Traverse.Create(child).Field("_rigidbody").GetValue<Rigidbody>();
            if (rb == null) return;

            var data = PhotonUtils.FetchScratchArray(5);
            data[0] = (int)typeof(DeployableObject)
                .GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(deployable)
                .GetType().GetField("_signalID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                    typeof(DeployableObject).GetField("_deploySignal", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(deployable));
            data[1] = PhotonNetwork.ServerTimestamp;
            data[2] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.position +
                                                          new Vector3(0, -0.2f, 0));
            data[3] = 469893376;
            data[4] = BitPackUtils.PackWorldPosForNetwork(targetFlinged.bodyTransform.up * 10000f);

            PhotonNetwork.RaiseEvent(177, data,
                TargetedWCO(targetFlinged.Creator.ActorNumber, EventCaching.AddToRoomCacheGlobal), STS());

            child.Deploy(deployable, targetFlinged.bodyTransform.position + new Vector3(0, -0.2f, 0),
                Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)),
                targetFlinged.bodyTransform.up * 10000f);
            deployable.DeployChild();
            SpamDeploy(deployable);

            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.detectCollisions = false;
            rb.velocity = targetFlinged.bodyTransform.up * 10000f;
            rb.mass = 0.001f;
            rb.useGravity = false;

            var barrelObj = GetBarrelDeployable()?.gameObject;

            if (barrelObj != null)
                barrelObj.transform.rotation =
                    Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            for (var i = 0; i < 20; i++)
            {
                rb.AddForce(targetFlinged.bodyTransform.up * 10000f, ForceMode.Force);
                rb.AddForce(targetFlinged.bodyTransform.up * 10000f, ForceMode.Impulse);
                rb.AddForce(targetFlinged.bodyTransform.up * 10000f, ForceMode.VelocityChange);
                rb.AddForce(targetFlinged.bodyTransform.up * 10000f, ForceMode.Acceleration);
                rb.AddForce(targetFlinged.bodyTransform.forward * 5000f, ForceMode.Impulse);
                rb.AddForce(targetFlinged.bodyTransform.right * 2500f, ForceMode.VelocityChange);
            }
            child.ReturnToParent(1f);
        }


        public static SendOptions STS()
        {
            return SendOptions.SendReliable;
        }

        public static RaiseEventOptions TargetedWCO(int actor, EventCaching cache)
        {
            return new RaiseEventOptions
            {
                CachingOption = cache,
                TargetActors = new[]
                {
                    actor
                }
            };
        }




        public static void BarrelFlingAll()
        {
            GorillaTagger.Instance.StartCoroutine(BarrelFlingAllDelay());
        }

        private static IEnumerator BarrelFlingAllDelay()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig)
                {
                    targetFlinged = vrrig;
                    BarrelFlingMethod5();
                    yield return new WaitForSeconds(0.15f);
                }
            }
        }
        public static void BarrelFlingGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    targetFlinged = GunLib.LockedPlayer;
                    SendBarrelFling(barrelFlingMode);
                }
            }, true);
        }
        public static void BarrelFlingSelf()
        {
            targetFlinged = GorillaTagger.Instance.offlineVRRig;
            SendBarrelFling(barrelFlingMode);
        }
        public static void BarrelFlingAura()
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
                    targetFlinged = rigs;
                    SendBarrelFling(barrelFlingMode);
                }
            }
        }
        public static void BarrelFlingTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f))
                {
                    targetFlinged = vrrig;
                    SendBarrelFling(barrelFlingMode);
                }
            }
        }
        public static void BarrelFlingOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    targetFlinged = vrrig;
                    SendBarrelFling(barrelFlingMode);
                }
            }
        }

        public static void BuyBarrel()
        {
            CosmeticsController.instance.currentCart.Insert(0, CosmeticsController.instance.GetItemFromDict("LMAPE."));
        }
        public static void BuyBarrel2()
        {
            CosmeticsController.instance.currentCart.Insert(0, CosmeticsController.instance.GetItemFromDict("LMAPH."));
        }

        public static void BarrelPunchMod()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    float leftDistance = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position);
                    float rightDistance = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position);
                    float leftBodyDistance = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position);
                    float rightBodyDistance = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position);

                    if (leftDistance < 0.45f || rightDistance < 0.45f || leftBodyDistance < 0.45f || rightBodyDistance < 0.45f)
                    {
                        targetFlinged = vrrig;
                        SendBarrelFling(barrelFlingMode);
                    }

                    float theirLeftDistance = Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    float theirRightDistance = Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                    float theirBodyDistance = Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);

                    if (theirLeftDistance <= 0.5 || theirRightDistance <= 0.5 || theirBodyDistance <= 0.5)
                    {
                        Vector3 flingDirection = (GorillaTagger.Instance.offlineVRRig.transform.position - vrrig.transform.position).normalized;
                        GorillaTagger.Instance.rigidbody.velocity = flingDirection * 8f;
                    }
                }
            }
        }

        public static void BarrelFlingAntiReport()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                Transform report = line.reportButton.gameObject.transform;

                foreach (VRRig vrrig in VRRigCache.ActiveRigs)
                {
                    if (!vrrig.isLocal)
                    {
                        float D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position);
                        float D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position);

                        if (D1 < Safety.antiReportRadius || D2 < Safety.antiReportRadius)
                        {
                            targetFlinged = vrrig;
                            SendBarrelFling(barrelFlingMode);
                        }
                    }
                }
            }
        }


        public static void DestroyAll()
        {
            if (PhotonNetwork.InRoom)
            {
                foreach (Player p in PhotonNetwork.PlayerListOthers)
                {
                    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(p.ActorNumber);
                }
            }
        }
        public static void DestroyGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                PhotonNetwork.OpRemoveCompleteCacheOfPlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
            }, true);
        }

        public static void DestroyAura()
        {
            if (!PhotonNetwork.InRoom || !IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f)
                {
                    Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                    if (targetPlayer != null)
                    {
                        PhotonNetwork.OpRemoveCompleteCacheOfPlayer(targetPlayer.ActorNumber);
                    }
                }
            }
        }

        public static void DestroyOnTouch()
        {
            if (!PhotonNetwork.InRoom || !IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.transform.position) < 0.25f ||
                     Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.transform.position) < 0.25f))
                {
                    Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                    if (targetPlayer != null)
                    {
                        PhotonNetwork.OpRemoveCompleteCacheOfPlayer(targetPlayer.ActorNumber);
                    }
                }
            }
        }

        public static void DestroyOnYourTouch()
        {
            if (!PhotonNetwork.InRoom || !IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig &&
                    (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                    if (targetPlayer != null)
                    {
                        PhotonNetwork.OpRemoveCompleteCacheOfPlayer(targetPlayer.ActorNumber);
                    }
                }
            }
        }
        public static bool IsModded()
        {
            string gameMode = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] as string;
            if (gameMode.Contains("MODDED"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void BreakAudioAll()
        {
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f || Mouse.current.rightButton.isPressed)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.00000000000015f;
                    VRRig.LocalRig.netView.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.Others, new object[]
                    {
                        true,
                        false
                    }, true);
                    VRRig.LocalRig.netView.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.Others, new object[]
                    {
                        false,
                        false
                    });
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, 111, false, 999999f);
                }
            }
        }
        public static void BreakAudioGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.00000000000015f;
                    VRRig.LocalRig.netView.SendRPC("EnableNonCosmeticHandItemRPC", Rigs.GetPlayerFromVRRig(GunLib.LockedPlayer), new object[]
                    {
                       true,
                       false
                     });
                    VRRig.LocalRig.netView.SendRPC("EnableNonCosmeticHandItemRPC", Rigs.GetPlayerFromVRRig(GunLib.LockedPlayer), new object[]
                    {
                       false,
                       false
                    });
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", Rigs.GetPlayerFromVRRig(GunLib.LockedPlayer), 111, false, 999999f);
                }
            }, true);
        }
        public static void Test()
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;

            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }
        public static void Test2()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;

            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }

        public static void Test3()
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;

            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }
        public static void Test4()
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;

            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }
        public static void BreakGameMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GorillaTagManager.instance.StopPlaying();
                GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
            }
        }
        public static void FixGamemode()
        {
            GorillaTagManager.instance.StartPlaying();
            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }

        public static void StutterGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                StutterPlayer(GunLib.LockedPlayer);
            }, true);
        }
        public static void StutterAll()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs.Where(r => r != VRRig.LocalRig))
            {
                StutterPlayer(p);
            }
        }
        public static void LagGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                    SendLagGun(lagMethod, GunLib.LockedPlayer.Creator);
            }, true);
        }
        public static void LagAll()
        {
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
                SendLagAll(lagMethod);
        }
        public static void LagOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    LagPlayer(vrrig);
                }
            }
        }
        public static void CrashOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    CrashPlayer(vrrig);
                }
            }
        }
        public static void StutterOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    StutterPlayer(vrrig);
                }
            }
        }
        public static void LagOnTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f))
                {
                    LagPlayer(vrrig);
                }
            }
        }
        public static void StutterOnTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f))
                {
                    StutterPlayer(vrrig);
                }
            }
        }
        public static void CrashOnTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f))
                {
                    CrashPlayer(vrrig);
                }
            }
        }
        public static void LagAura()
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
                    LagPlayer(rigs);
                }
            }
        }
        public static void CrashAura()
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
                    CrashPlayer(rigs);
                }
            }
        }
        public static void StutterAura()
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
                    StutterPlayer(rigs);
                }
            }
        }
        public static void CrashGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                CrashPlayer(GunLib.LockedPlayer);
            }, true);
        }
        public static void CrashAll()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs.Where(r => r != VRRig.LocalRig))
            {
                CrashPlayer(p);
            }
        }
        public static void StrongLagGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                StrongLagPlayer(GunLib.LockedPlayer);
            }, true);
        }
        public static void StrongLagAll()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs.Where(r => r != VRRig.LocalRig))
            {
                StrongLagPlayer(p);
            }
        }
        public static void StrongLagAura()
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
                    StrongLagPlayer(rigs);
                }
            }
        }
        public static void StrongLagTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f))
                {
                    StrongLagPlayer(vrrig);
                }
            }
        }
        public static void StrongLagOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    StrongLagPlayer(vrrig);
                }
            }
        }
        public static void LagTest67()
        {
            GunLib.StartPointerSystem(() =>
            {
                LagPlayerTest(GunLib.LockedPlayer);
            }, true);
        }
        public static void LagPlayerTest(VRRig player, bool bypassDelay = false)
        {
            if (!bypassDelay && Time.time <= delay) return;
            for (int i = 0; i < 3796; i++) PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(51, new ExitGames.Client.Photon.Hashtable(), new RaiseEventOptions { TargetActors = new int[] { player.Creator.ActorNumber } }, SendOptions.SendUnreliable);
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            if (!bypassDelay) delay = Time.time + 8.5f;
        }

        public static void Send186(int power, float delay, NetPlayer plr)
        {
            if (!PhotonNetwork.InRoom)
                return;

            var hash = new Hashtable();
            if (Time.time > lagcd)
            {
                lagcd = Time.time + delay;
                for (var i = 0; i < power; i++)
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(186, hash[float.NaN] = float.NaN,
                        new RaiseEventOptions
                        {
                            TargetActors = new[]
                            {
                                plr.ActorNumber
                            }
                        }, SendOptions.SendUnreliable);
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            }
        }

        public static void Send186(int power, float delay)
        {
            if (!PhotonNetwork.InRoom)
                return;

            var hash = new Hashtable();
            if (Time.time > lagcd)
            {
                lagcd = Time.time + delay;
                for (var i = 0; i < power; i++)
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(186, hash[float.NaN] = float.NaN,
                        new RaiseEventOptions
                        {
                            Receivers = 0
                        }, SendOptions.SendUnreliable);
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            }
        }

        public static void Send202(int power, float delay, NetPlayer plr)
        {
            if (!PhotonNetwork.InRoom)
                return;

            var hash = new Hashtable();
            if (Time.time > lagcd)
            {
                lagcd = Time.time + delay;
                for (var i = 0; i < power; i++)
                {
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(202,
                        hash[-float.NaN] = -float.NaN, new RaiseEventOptions
                        {
                            TargetActors = new[]
                            {
                                plr.ActorNumber
                            }
                        }, new SendOptions
                        {
                            Encrypt = true,
                            Reliability = false,
                            DeliveryMode = DeliveryMode.Unreliable
                        });
                }

                PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            }
        }

        public static void Send202(int power, float delay)
        {
            if (!PhotonNetwork.InRoom)
                return;

            var hash = new Hashtable();
            if (Time.time > lagcd)
            {
                lagcd = Time.time + 1.5f;
                for (var i = 0; i < 600; i++)
                {
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(202,
                        hash[-float.NaN] = -float.NaN, new RaiseEventOptions
                        {
                            Receivers = 0
                        }, new SendOptions
                        {
                            Encrypt = true,
                            Reliability = false,
                            DeliveryMode = DeliveryMode.Unreliable
                        });
                }

                PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            }
        }

        public static void SendLagAll(int mode)
        {
            if (!PhotonNetwork.InRoom)
                return;

            if (mode == 0)
            {
                Send186(600, 1.5f);
            }
            else if (mode == 1)
            {
                Send202(2400, 7f);
            }
        }

        public static void SendLagGun(int mode, NetPlayer player)
        {
            if (!PhotonNetwork.InRoom || player == null)
                return;

            if (mode == 0)
            {
                Send186(600, 1.5f, player);
            }
            else if (mode == 1)
            {
                Send202(2400, 7f, player);
            }
        }

        public static void CrashPlayer(VRRig player, bool bypassDelay = false)
        {
            if (!bypassDelay && Time.time <= delay) return;
            for (int i = 0; i < 1875; i++) PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(202, new ExitGames.Client.Photon.Hashtable(), new RaiseEventOptions { TargetActors = new int[] { player.Creator.ActorNumber } }, SendOptions.SendUnreliable);
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            if (!bypassDelay) delay = Time.time + 4.54f;
        }

        public static void LagPlayer(VRRig player, bool bypassDelay = false)
        {
            if (!bypassDelay && Time.time <= delay) return;
            for (int i = 0; i < 600; i++) PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(202, new ExitGames.Client.Photon.Hashtable(), new RaiseEventOptions { TargetActors = new int[] { player.Creator.ActorNumber } }, SendOptions.SendUnreliable);
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            if (!bypassDelay) delay = Time.time + 1.3f;
        }

        public static void StrongLagPlayer(VRRig player, bool bypassDelay = false)
        {
            if (!bypassDelay && Time.time <= delay) return;
            for (int i = 0; i < 3796; i++) PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(202, new ExitGames.Client.Photon.Hashtable(), new RaiseEventOptions { TargetActors = new int[] { player.Creator.ActorNumber } }, SendOptions.SendUnreliable);
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            if (!bypassDelay) delay = Time.time + 8.5f;
        }

        public static void StutterPlayer(VRRig player, bool bypassDelay = false)
        {
            if (!bypassDelay && Time.time <= delay) return;
            for (int i = 0; i < 1200; i++) PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(202, new ExitGames.Client.Photon.Hashtable(), new RaiseEventOptions { TargetActors = new int[] { player.Creator.ActorNumber } }, SendOptions.SendUnreliable);
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            if (!bypassDelay) delay = Time.time + 5f;
        }

        public static Hashtable hashtable = new Hashtable();

        public static void FixModCheckers()
        {
            hashtable.Clear();
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
        }
        public static void UnlockRoom()
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }

        public static void LockRoom()
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }
        public static void SpazRoom()
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;
            for (int i = 0; i < 100; i++)
            {
                PhotonNetwork.CurrentRoom.IsVisible = (i % 2 == 0);
                PhotonNetwork.CurrentRoom.IsOpen = (i % 2 == 0);
            }
            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }
        public static void DestroyAll2()
        {
            BreakAudioAll();
            if (PhotonNetwork.InRoom)
            {
                foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers)
                {
                    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(p.ActorNumber);
                }
            }
        }
        public static void DestroyGun2()
        {
            BreakAudioGun();
            GunLib.StartPointerSystem(() =>
            {
                if (PhotonNetwork.InRoom && GunLib.LockedPlayer != null)
                {
                    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
                }
            }, true);
        }
        public static void StealAllOwnership()
        {
            if (!PhotonNetwork.InRoom) return;
            foreach (PhotonView pv in PhotonNetwork.PhotonViewCollection)
            {
                if (pv.OwnerActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    PhotonNetwork.TransferOwnership(pv.ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
                }
            }
        }


        public static List<Player> GetAllJUULUsers()
        {
            List<Player> juulUsers = new List<Player>();
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (JUUL.HasJUULProperty(p))
                    juulUsers.Add(p);
            }
            return juulUsers;
        }
        public static void GetAllJUULUsersInLobby()
        {
            var users = GetAllJUULUsers();
            NotifiLib.SendNotification($"Found {users.Count} JUUL users in room:");
            foreach (var user in users)
            {
                NotifiLib.SendNotification($"- {user.NickName} (ID: {user.ActorNumber})");
            }
        }
        public static NetworkView GetNetViewFromVRRig(VRRig VRRig)
        {
            return (NetworkView)Traverse.Create(VRRig).Field("netView").GetValue();
        }
        public static void GrabAll()
        {
            try
            {
                if (!PhotonNetwork.InRoom) return;
                bool isGuardian = IsLocalPlayerGuardian();
                if (!isGuardian) return;
                var activeRigs = VRRigCache.ActiveRigs;
                foreach (VRRig vrrig in activeRigs)
                {
                    if (vrrig == null) continue;
                    NetworkView netView = GetNetViewFromVRRig(vrrig);
                    if (netView != null)
                    {
                        netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                    }
                }
            }
            catch { }
        }

        public static void GrabGun()
        {
            try
            {
                if (IsLocalPlayerGuardian() && (ControllerInputPoller.instance.rightGrab || ControllerInputPoller.instance.leftGrab))
                {
                    foreach (VRRig vrrig in VRRigCache.ActiveRigs)
                    {
                        if (vrrig != null && !vrrig.isMyPlayer && !vrrig.isOfflineVRRig)
                        {
                            Vector3 handPosition = GunLib.gunLeft ? GorillaTagger.Instance.leftHandTransform.position : GorillaTagger.Instance.rightHandTransform.position;
                            LineLib.DrawLine(handPosition, vrrig.transform.position, GunLib.LineColor, 0.005f);
                        }
                    }
                }

                GunLib.StartPointerSystem(() =>
                {
                    try
                    {
                        if (!PhotonNetwork.InRoom) return;
                        if (GunLib.LockedPlayer == null) return;

                        if (IsLocalPlayerGuardian())
                        {
                            NetworkView netView = GetNetViewFromVRRig(GunLib.LockedPlayer);
                            if (netView != null)
                            {
                                netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                            }
                        }
                    }
                    catch { }
                }, true);
            }
            catch { }
        }

        public static void FlingAll()
        {
            try
            {
                if (!PhotonNetwork.InRoom) return;
                if (!IsLocalPlayerGuardian()) return;
                var activeRigs = VRRigCache.ActiveRigs;
                Vector3 flingForce = new Vector3(0f, 9998.99f, 0f);

                foreach (VRRig vrrig in activeRigs)
                {
                    if (vrrig == null) continue;

                    NetworkView netView = GetNetViewFromVRRig(vrrig);
                    if (netView != null)
                    {
                        netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                        netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { flingForce });
                    }
                }
            }
            catch { }
        }

        public static void FlingGun()
        {
            try
            {
                GunLib.StartPointerSystem(() =>
                {
                    try
                    {
                        if (!PhotonNetwork.InRoom) return;
                        if (GunLib.LockedPlayer == null) return;
                        if (IsLocalPlayerGuardian())
                        {
                            NetworkView netView = GetNetViewFromVRRig(GunLib.LockedPlayer);
                            if (netView != null)
                            {
                                Vector3 flingForce = new Vector3(0f, 9998.99f, 0f);
                                netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                                netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { flingForce });
                            }
                        }
                    }
                    catch { }
                }, true);
            }
            catch { }
        }

        public static void BreakMovementAll()
        {
            try
            {
                if (!PhotonNetwork.InRoom) return;
                float currentTime = Time.time;
                if (currentTime > delay)
                {
                    delay = currentTime + 0.5f;

                    if (!IsLocalPlayerGuardian()) return;
                    var activeRigs = VRRigCache.ActiveRigs;

                    foreach (VRRig vrrig in activeRigs)
                    {
                        if (vrrig == null || vrrig.isMyPlayer) continue;

                        NetworkView netView = GetNetViewFromVRRig(vrrig);
                        if (netView != null)
                        {
                            Vector3 groundPosition = vrrig.transform.position;
                            groundPosition.y = -10f;

                            netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                            netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { (groundPosition - vrrig.transform.position) * 100f });
                        }
                    }
                }
            }
            catch { }
        }

        public static void BreakMovementGun()
        {
            try
            {
                GunLib.StartPointerSystem(() =>
                {
                    try
                    {
                        if (!PhotonNetwork.InRoom) return;
                        if (GunLib.LockedPlayer == null) return;

                        if (IsLocalPlayerGuardian())
                        {
                            NetworkView netView = GetNetViewFromVRRig(GunLib.LockedPlayer);
                            if (netView != null)
                            {
                                Vector3 groundPosition = GunLib.LockedPlayer.transform.position;
                                groundPosition.y = -10f;

                                netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                                netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { (groundPosition - GunLib.LockedPlayer.transform.position) * 100f });
                            }
                        }
                    }
                    catch { }
                }, true);
            }
            catch { }
        }

        public static void SpazAll()
        {
            try
            {
                if (!PhotonNetwork.InRoom) return;

                float currentTime = Time.time;
                if (currentTime > delay)
                {
                    delay = currentTime + 0.5f;
                    if (!IsLocalPlayerGuardian()) return;
                    var activeRigs = VRRigCache.ActiveRigs;
                    foreach (VRRig vrrig in activeRigs)
                    {
                        if (vrrig == null) continue;
                        NetworkView netView = GetNetViewFromVRRig(vrrig);
                        if (netView != null)
                        {
                            netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { false, false, false });
                            netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { new Vector3(0f, 9998.99f, 0f) });
                            netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { false, false, false });
                            netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { new Vector3(0f, -9998.99f, 0f) });
                        }
                    }
                }
            }
            catch { }
        }
        public static void SpazGun()
        {
            try
            {
                GunLib.StartPointerSystem(() =>
                {
                    try
                    {
                        if (!PhotonNetwork.InRoom) return;
                        if (GunLib.LockedPlayer == null) return;
                        if (IsLocalPlayerGuardian())
                        {
                            NetworkView netView = GetNetViewFromVRRig(GunLib.LockedPlayer);
                            if (netView != null)
                            {
                                netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { false, false, false });
                                netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { new Vector3(0f, 9998.99f, 0f) });
                                netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { false, false, false });
                                netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { new Vector3(0f, -9998.99f, 0f) });
                            }
                        }
                    }
                    catch { }
                }, true);
            }
            catch { }
        }

        private static bool IsLocalPlayerGuardian()
        {
            try
            {
                foreach (GorillaGuardianZoneManager guardian in Object.FindObjectsOfType<GorillaGuardianZoneManager>())
                {
                    if (guardian.IsPlayerGuardian(PhotonNetwork.LocalPlayer))
                        return true;
                }
            }
            catch { }
            return false;
        }
        public static void PushAllAway()
        {
            try
            {
                if (!PhotonNetwork.InRoom) return;
                if (!IsLocalPlayerGuardian()) return;

                Vector3 yourPosition = GorillaTagger.Instance.offlineVRRig.transform.position;
                var activeRigs = VRRigCache.ActiveRigs;

                foreach (VRRig vrrig in activeRigs)
                {
                    if (vrrig == null || vrrig.isMyPlayer) continue;

                    Vector3 direction = (vrrig.transform.position - yourPosition).normalized;
                    Vector3 pushForce = direction * 9998.99f;

                    NetworkView netView = GetNetViewFromVRRig(vrrig);
                    if (netView != null)
                    {
                        netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                        netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { pushForce });
                    }
                }
            }
            catch { }
        }

        public static void PushGunAway()
        {
            try
            {
                GunLib.StartPointerSystem(() =>
                {
                    try
                    {
                        if (!PhotonNetwork.InRoom) return;
                        if (GunLib.LockedPlayer == null) return;
                        if (IsLocalPlayerGuardian())
                        {
                            Vector3 yourPosition = GorillaTagger.Instance.offlineVRRig.transform.position;
                            Vector3 direction = (GunLib.LockedPlayer.transform.position - yourPosition).normalized;
                            Vector3 pushForce = direction * 9998.99f;

                            NetworkView netView = GetNetViewFromVRRig(GunLib.LockedPlayer);
                            if (netView != null)
                            {
                                netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                                netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { pushForce });
                            }
                        }
                    }
                    catch { }
                }, true);
            }
            catch { }
        }

        public static void OrbitAll()
        {
            try
            {
                if (!PhotonNetwork.InRoom) return;
                if (!IsLocalPlayerGuardian()) return;

                Vector3 centerPoint = GorillaTagger.Instance.offlineVRRig.transform.position;
                float orbitRadius = 3f;
                float orbitSpeed = 5f;
                float currentTime = Time.time;

                var activeRigs = VRRigCache.ActiveRigs;
                int playerIndex = 0;

                foreach (VRRig vrrig in activeRigs)
                {
                    if (vrrig == null || vrrig.isMyPlayer || vrrig.isOfflineVRRig) continue;

                    float angle = (currentTime * orbitSpeed) + (playerIndex * (Mathf.PI * 2f / activeRigs.Count));
                    float x = centerPoint.x + Mathf.Cos(angle) * orbitRadius;
                    float z = centerPoint.z + Mathf.Sin(angle) * orbitRadius;
                    Vector3 orbitPosition = new Vector3(x, centerPoint.y, z);

                    NetworkView netView = GetNetViewFromVRRig(vrrig);
                    if (netView != null)
                    {
                        netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                        netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { (orbitPosition - vrrig.transform.position) * 5f });
                    }

                    playerIndex++;
                }
            }
            catch { }
        }

        public static void OrbitGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                try
                {
                    if (!PhotonNetwork.InRoom) return;
                    if (GunLib.LockedPlayer == null) return;
                    if (!IsLocalPlayerGuardian()) return;

                    Vector3 centerPoint = GorillaTagger.Instance.offlineVRRig.transform.position;
                    float orbitRadius = 2.5f;
                    float angle = Time.time * 4f;
                    float x = centerPoint.x + Mathf.Cos(angle) * orbitRadius;
                    float z = centerPoint.z + Mathf.Sin(angle) * orbitRadius;
                    Vector3 orbitPosition = new Vector3(x, centerPoint.y, z);

                    NetworkView netView = GetNetViewFromVRRig(GunLib.LockedPlayer);
                    if (netView != null)
                    {
                        netView.SendRPC("GrabbedByPlayer", RpcTarget.Others, new object[] { true, false, false });
                        netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { (orbitPosition - GunLib.LockedPlayer.transform.position) * 60f });
                    }
                }
                catch { }
            }, true);
        }
        public static void DropPlayer()
        {
            try
            {
                if (!PhotonNetwork.InRoom) return;
                if (!IsLocalPlayerGuardian()) return;

                foreach (VRRig vrrig in VRRigCache.ActiveRigs)
                {
                    if (vrrig == null || vrrig.isMyPlayer || vrrig.isOfflineVRRig) continue;

                    NetworkView netView = GetNetViewFromVRRig(vrrig);
                    if (netView != null)
                    {
                        netView.SendRPC("DroppedByPlayer", RpcTarget.Others, new object[] { Vector3.zero });
                    }
                }
            }
            catch { }
        }









        private static float kickCD;

        public static void KickAllAntiReportUsers()
        {
            if (Inputs.RightGrip)
            {
                if (Time.time > kickCD)
                {
                    GorillaPlayerScoreboardLine gorillaPlayerScoreboardLine =
                        GorillaScoreboardTotalUpdater.allScoreboardLines[Random.Range(0, GorillaScoreboardTotalUpdater.allScoreboardLines.Count)];
                    VRRig.LocalRig.transform.position = gorillaPlayerScoreboardLine.reportButton.transform.position;
                    VRRig.LocalRig.rightHand.rigTarget.position = gorillaPlayerScoreboardLine.reportButton.transform.position +
                                                            gorillaPlayerScoreboardLine.transform.forward * 0.3f;
                    kickCD = Time.time + 0.5f;
                }
            }
        }
        public static void StumpKickAll()
        {
            GorillaComputer.instance.OnGroupJoinButtonPress(0, GorillaComputer.instance.friendJoinCollider);
        }


        private static Coroutine BlasterCoroutine = null;
        private static SIGadgetChargeBlaster GetBlaster()
        {
            if (SuperInfectionManager.activeSuperInfectionManager == null) return null;
            var entities = SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.GetGameEntities();
            foreach (GameEntity entity in entities)
            {
                if (entity != null)
                {
                    SIGadgetChargeBlaster blaster = entity.GetComponent<SIGadgetChargeBlaster>();
                    if (blaster != null && entity.IsHeldByLocalPlayer())
                    {
                        return blaster;
                    }
                }
            }
            return null;
        }
        public static void BlasterFlingAll()
        {
            if (!PhotonNetwork.InRoom) return;
            if (SuperInfectionManager.activeSuperInfectionManager == null) return;
            SIGadgetChargeBlaster blaster = GetBlaster();
            if (blaster == null) return;
            if (Time.time < delay) return;
            delay = Time.time + 0.5f;
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig == null || vrrig.isMyPlayer || vrrig.isOfflineVRRig) continue;
                Vector3 direction = (vrrig.transform.position - blaster.transform.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);
                blaster.blaster.lastFired = 0f;
                blaster.FireProjectile(blaster.maxChargeDiff, blaster.blaster.NextFireId(), blaster.transform.position, rotation);
            }
        }

        public static void BlasterFlingGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.InRoom) return;
                if (GunLib.LockedPlayer == null) return;
                if (SuperInfectionManager.activeSuperInfectionManager == null) return;
                SIGadgetChargeBlaster blaster = GetBlaster();
                if (blaster == null) return;
                if (Time.time < delay) return;
                delay = Time.time + 0.5f;
                Vector3 direction = (GunLib.LockedPlayer.transform.position - blaster.transform.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);
                blaster.blaster.lastFired = 0f;
                blaster.FireProjectile(blaster.maxChargeDiff, blaster.blaster.NextFireId(), blaster.transform.position, rotation);
            }, true);
        }

        public static void BlasterAimbot()
        {
            if (Time.time < delay + 0.5f) return;

            if (!PhotonNetwork.InRoom) return;
            if (SuperInfectionManager.activeSuperInfectionManager == null) return;

            SIGadgetChargeBlaster blaster = GetBlaster();
            if (blaster == null) return;

            VRRig closestRig = null;
            float closestDistance = float.MaxValue;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig == null || vrrig.isMyPlayer || vrrig.isOfflineVRRig) continue;
                float distance = Vector3.Distance(vrrig.transform.position, blaster.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRig = vrrig;
                }
            }

            if (closestRig != null)
            {
                Vector3 direction = (closestRig.transform.position - blaster.transform.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);
                blaster.blaster.lastFired = 0f;
                blaster.FireProjectile(blaster.maxChargeDiff, blaster.blaster.NextFireId(), blaster.transform.position, rotation);
            }

            delay = Time.time;
        }
        private static Coroutine TentacleCoroutine = null;
        private static SIGadgetTentacleArm GetTentacleArm()
        {
            if (SuperInfectionManager.activeSuperInfectionManager == null) return null;
            var entities = SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.GetGameEntities();
            foreach (GameEntity entity in entities)
            {
                if (entity != null)
                {
                    SIGadgetTentacleArm tentacle = entity.GetComponent<SIGadgetTentacleArm>();
                    if (tentacle != null && entity.IsHeldByLocalPlayer())
                    {
                        return tentacle;
                    }
                }
            }
            return null;
        }
        public static void TentacleMaxLength()
        {
            SIGadgetTentacleArm tentacle = GetTentacleArm();
            if (tentacle == null) return;
            Traverse.Create(tentacle).Field("maxTentacleLength").SetValue(999f);
        }
        public static void FixTentacle()
        {
            SIGadgetTentacleArm tentacle = GetTentacleArm();
            if (tentacle == null) return;
            Traverse.Create(tentacle).Field("maxTentacleLength").SetValue(1f);
            NotifiLib.SendNotification("Tentacle Max Length ON");
        }

        public static void GiveFlyOnGrab()
        {
            var localPlayer = NetworkSystem.Instance.LocalPlayer;
            if (localPlayer == null) return;
            var grabbingRigs = VRRigCache.ActiveRigs.Where(rig => rig != null && !rig.isLocal && ((rig.leftHandLink?.grabbedPlayer == localPlayer) || (rig.rightHandLink?.grabbedPlayer == localPlayer)) && (rig.rightIndex?.calcT > 0));
            foreach (var rig in grabbingRigs)
            {
                GTPlayer.Instance.transform.position += rig.headMesh.transform.forward * (Time.deltaTime * 10f);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }
        private static Coroutine PaintbrawlCoroutine = null;
        private static GorillaPaintbrawlManager paintbrawlManager;

        private static GorillaPaintbrawlManager GetPaintbrawlManager()
        {
            if (paintbrawlManager == null)
            {
                paintbrawlManager = GorillaGameManager.instance as GorillaPaintbrawlManager;
            }
            return paintbrawlManager;
        }

        private static float lastPaintballShot = 0f;

        public static void PaintbrawlAimbot()
        {
            if (Time.time < lastPaintballShot + delay) return;

            GorillaPaintbrawlManager manager = GetPaintbrawlManager();
            if (manager == null) return;
            if (!PhotonNetwork.InRoom) return;
            if (manager.currentState != GorillaPaintbrawlManager.PaintbrawlState.GameRunning) return;

            VRRig closestRig = null;
            float closestDistance = float.MaxValue;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig == null || vrrig.isMyPlayer || vrrig.isOfflineVRRig) continue;

                NetPlayer targetPlayer = vrrig.OwningNetPlayer;
                if (targetPlayer == null) continue;

                if (manager.OnSameTeam(NetworkSystem.Instance.LocalPlayer, targetPlayer)) continue;

                if (manager.GetPlayerLives(targetPlayer) <= 0) continue;

                float distance = Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRig = vrrig;
                }
            }

            if (closestRig != null)
            {
                Vector3 direction = (closestRig.headMesh.transform.position - GorillaTagger.Instance.offlineVRRig.headMesh.transform.position).normalized;
                GorillaTagger.Instance.offlineVRRig.headMesh.transform.rotation = Quaternion.LookRotation(direction);

                NetPlayer targetNetPlayer = closestRig.OwningNetPlayer;
                manager.ReportSlingshotHit(targetNetPlayer, closestRig.transform.position, 1, new PhotonMessageInfoWrapped());
            }

            lastPaintballShot = Time.time;
        }
        private static bool _grabSafetyEnabled = false;

        private static void EnableGrabSafety()
        {
            if (_grabSafetyEnabled) return;
            _grabSafetyEnabled = true;
            
            try { Safety.DisableMapTriggers(); } catch { }
            try { Safety.DisableNetworkTriggers(); } catch { }
            try { Safety.DisableQuitBox(); } catch { }
        }

        private static void DisableGrabSafety()
        {
            if (!_grabSafetyEnabled) return;
            _grabSafetyEnabled = false;
            
            try { Safety.EnableMapTriggers(); } catch { }
            try { Safety.EnableNetworkTriggers(); } catch { }
            try { Safety.EnableQuitBox(); } catch { }
        }

        private static float _nextGrabTime;

        private static bool CheckHandLinks(VRRig monkey)
        {
            if (monkey == null) return false;
            return monkey.leftHandLink.CanBeGrabbed() || monkey.rightHandLink.CanBeGrabbed();
        }

        private static void UpdateGrabStatus(bool isActive)
        {
            Juul.Patches.GrabPatches.GrabPatch.enabled = isActive;
            if (!isActive && !VRRig.LocalRig.enabled)
            {
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void GrabFlingGun()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isMyPlayer || rig.isOfflineVRRig) continue;
                if (CheckHandLinks(rig))
                {
                    if (!Inputs.RightTrigger)
                    {
                        Vector3 myPos = GorillaTagger.Instance.offlineVRRig.transform.position;
                        Vector3 theirPos = rig.transform.position;
                        LineLib.DrawLine(myPos, theirPos, Color.green, 0.005f);
                    }
                }
            }

            GunLib.StartPointerSystem(() =>
            {
                float randX = UnityEngine.Random.Range(0, 2) == 0 ? -95000f : 95000f;
                float randZ = UnityEngine.Random.Range(0, 2) == 0 ? -95000f : 95000f;
                ForceGrabThingy(GunLib.LockedPlayer, new Vector3(randX, 95000f, randZ));
            }, true);

            bool noInput = !Inputs.RightGrip && !Inputs.LeftGrip && !Inputs.RightTrigger && !Inputs.LeftTrigger;
            if (noInput && Juul.Patches.GrabPatches.GrabPatch.enabled)
            {
                UpdateGrabStatus(false);
                VRRig.LocalRig.BreakHandLinks();
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void GrabFlingAll()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.isMyPlayer || rig.isOfflineVRRig) continue;
                if (!CheckHandLinks(rig)) continue;

                float randX = UnityEngine.Random.Range(0, 2) == 0 ? -95000f : 95000f;
                float randZ = UnityEngine.Random.Range(0, 2) == 0 ? -95000f : 95000f;
                ForceGrabThingy(rig, new Vector3(randX, 95000f, randZ));
            }

            bool noInput = !Inputs.RightGrip && !Inputs.LeftGrip && !Inputs.RightTrigger && !Inputs.LeftTrigger;
            if (noInput && Juul.Patches.GrabPatches.GrabPatch.enabled)
            {
                UpdateGrabStatus(false);
                VRRig.LocalRig.BreakHandLinks();
                VRRig.LocalRig.enabled = true;
            }
        }

        private static void ForceGrabThingy(VRRig target, Vector3 tpTarget)
        {
            if (target == null || target.isLocal) return;

            if (!CheckHandLinks(target))
            {
                UpdateGrabStatus(false);
                VRRig.LocalRig.BreakHandLinks();
                VRRig.LocalRig.enabled = true;
                return;
            }

            UpdateGrabStatus(true);
            VRRig.LocalRig.enabled = false;
            VRRig.LocalRig.transform.position = tpTarget;

            bool preferLeft = target.leftHandLink.CanBeGrabbed();
            TakeMyHand_HandLink theirHand = preferLeft ? target.leftHandLink : target.rightHandLink;
            TakeMyHand_HandLink ourHand = preferLeft ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink;

            if (theirHand.grabbedPlayer == NetworkSystem.Instance.LocalPlayer) return;

            if (_nextGrabTime <= 0f)
            {
                _nextGrabTime = theirHand.rejectGrabsUntilTimestamp > Time.time ? theirHand.rejectGrabsUntilTimestamp : Time.time + 0.2f;
            }

            if (Time.time <= _nextGrabTime) return;

            VRRig.LocalRig.transform.position = target.syncPos;
            ourHand.TentacleTryCreateLink(theirHand);

            _nextGrabTime = theirHand.rejectGrabsUntilTimestamp > Time.time ? theirHand.rejectGrabsUntilTimestamp : Time.time + 0.2f;
        }
    }
}

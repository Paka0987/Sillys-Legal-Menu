using BepInEx;
using GorillaGameModes;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.XR;
using System.Threading;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Juul
{
    internal class Advantages
    {
        public static void TagNearest()
        {
            if (!PlayerIsTagged(VRRig.LocalRig))
                return;
            VRRig nearest = null;
            float nearestDistance = float.MaxValue;
            Vector3 myPos = VRRig.LocalRig.transform.position;
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig == VRRig.LocalRig) continue;
                if (PlayerIsTagged(rig)) continue;

                float distance = Vector3.Distance(myPos, rig.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = rig;
                }
            }
            if (nearest != null)
            {
                TagPlayer(Rigs.GetPlayerFromVRRig1(nearest));
            }
        }
        public static void TagPlayer(Photon.Realtime.Player Target)
        {
            if (!PlayerIsTagged(VRRig.LocalRig) || PlayerIsTagged(Rigs.GetVRRigFromNetPlayer(Target)))
                return;

            Vector3 startPos = VRRig.LocalRig.transform.position;
            VRRig.LocalRig.transform.position = Rigs.GetVRRigFromNetPlayer(Target).transform.position;

            Serialize.Serialize2(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });
            GameMode.ReportTag(Target);

            VRRig.LocalRig.transform.position = startPos;
        }

        public static void TagGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                TagPlayer(Rigs.GetPlayerFromVRRig1(GunLib.LockedPlayer));
            }, true);
        }

        public static void TagAll()
        {
            if (!PlayerIsTagged(VRRig.LocalRig))
            {
                ExtraButtons.GetCategory("Advantage").GetButton("Tag All").SetEnabled(false);
                return;
            }

            Vector3 startPos = VRRig.LocalRig.transform.position;
            VRRig[] rigs = VRRigCache.ActiveRigs.Where(vrrig => !PlayerIsTagged(vrrig)).ToArray();

            if (rigs.Length <= 0)
            {
                ExtraButtons.GetCategory("Advantage").GetButton("Tag All").SetEnabled(false);
                return;
            }

            foreach (var vrrig in rigs)
            {
                VRRig.LocalRig.transform.position = vrrig.transform.position;
                Serialize.Serialize2(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });
                GameMode.ReportTag(Rigs.GetPlayerFromVRRig1(vrrig));
            }

            VRRig.LocalRig.transform.position = startPos;
            Serialize.Serialize2(GorillaTagger.Instance.myVRRig.GetView, new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });
        }

        public static void FixRig()
        {
            var tagger = GorillaTagger.Instance;
            if (tagger != null && tagger.offlineVRRig != null)
            {
                tagger.offlineVRRig.enabled = true;
            }
        }

        public static void TagSelf()
        {
            var tagger = GorillaTagger.Instance;
            if (tagger == null) return;

            if (PlayerIsTagged(VRRig.LocalRig))
            {
                tagger.offlineVRRig.enabled = true;
                return;
            }

            VRRig infectedPlayer = VRRigCache.ActiveRigs.FirstOrDefault(r => r != null && r != VRRig.LocalRig && PlayerIsTagged(r));
            
            if (infectedPlayer == null) return;

            tagger.offlineVRRig.enabled = false;
            tagger.offlineVRRig.transform.position = infectedPlayer.rightHandTransform.position;
            Serialize.Serialize2(tagger.myVRRig.GetView, new RaiseEventOptions { Receivers = ReceiverGroup.Others });
        }

        public static void TagArua()
        {
            if (PhotonNetwork.InRoom)
            {
                try
                {
                    foreach (VRRig rig in VRRigCache.ActiveRigs)
                    {
                        if (rig != GorillaTagger.Instance.offlineVRRig)
                        {
                            if (Vector3.Distance(GorillaTagger.Instance.rightHandTransform.transform.position, rig.transform.position) < 6 || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.transform.position, rig.transform.position) < 6)
                            {
                                GameMode.ReportTag(rig.OwningNetPlayer);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
        public static void GripTagArua()
        {
            if (PhotonNetwork.InRoom)
            {
                try
                {
                    if (Inputs.RightGrip)
                    {
                        foreach (VRRig rig in VRRigCache.ActiveRigs)
                        {
                            if (rig != GorillaTagger.Instance.offlineVRRig)
                            {
                                if (Vector3.Distance(GorillaTagger.Instance.rightHandTransform.transform.position, rig.transform.position) < 6 || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.transform.position, rig.transform.position) < 6)
                                {
                                    GameMode.ReportTag(rig.OwningNetPlayer);
                                }
                            }
                        }
                    }    
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
        public static bool PlayerIsTagged(VRRig rig)
        {
            string text = rig.mainSkin.material.name.ToLower();
            return text.Contains("fected") || text.Contains("it") || text.Contains("stealth") || text.Contains("ice") || !rig.nameTagAnchor.activeSelf;
        }
        public static void NoTagOnJoin()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("didTutorial", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
            PlayerPrefs.SetString("didTutorial", "");
            PlayerPrefs.Save();
        }

        public static void TagOnJoin()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("didTutorial", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
            PlayerPrefs.SetString("didTutorial", "done");
            PlayerPrefs.Save();
        }


    }
}


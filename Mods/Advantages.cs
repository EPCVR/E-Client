using System;
using System.Collections.Generic;
using System.Text;
using GorillaLocomotion.Climbing;
using GorillaTagScripts;
using Photon.Pun;
using StupidTemplate.Menu;
using StupidTemplate.Notifications;
using UnityEngine;

namespace StupidTemplate.Mods
{
    internal class Advantages
    {
        public static int longarmCycle = 2;
        public static float armlength = 1.25f;


        public static List<NetPlayer> InfectedList()
        {
            List<NetPlayer> infected = new List<NetPlayer> { };

            if (!PhotonNetwork.InRoom)
                return infected;

            string gamemode = GorillaGameManager.instance.GameModeName().ToLower();

            if (gamemode.Contains("infection") || gamemode.Contains("tag"))
            {
                GorillaTagManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla Tag Manager").GetComponent<GorillaTagManager>();
                if (tagman.isCurrentlyTag)
                    infected.Add(tagman.currentIt);
                else
                {
                    foreach (NetPlayer plr in tagman.currentInfected)
                        infected.Add(plr);
                }
            }
            else if (gamemode.Contains("ghost"))
            {
                GorillaAmbushManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla GhostTag Manager").GetComponent<GorillaAmbushManager>();
                if (tagman.isCurrentlyTag)
                    infected.Add(tagman.currentIt);
                else
                {
                    foreach (NetPlayer plr in tagman.currentInfected)
                        infected.Add(plr);
                }
            }
            else if (gamemode.Contains("ambush") || gamemode.Contains("stealth"))
            {
                GorillaAmbushManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla Stealth Manager").GetComponent<GorillaAmbushManager>();
                if (tagman.isCurrentlyTag)
                    infected.Add(tagman.currentIt);
                else
                {
                    foreach (NetPlayer plr in tagman.currentInfected)
                        infected.Add(plr);
                }
            }
            else if (gamemode.Contains("freeze"))
            {
                GorillaFreezeTagManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla Freeze Tag Manager").GetComponent<GorillaFreezeTagManager>();
                if (tagman.isCurrentlyTag)
                    infected.Add(tagman.currentIt);
                else
                {
                    foreach (NetPlayer plr in tagman.currentInfected)
                        infected.Add(plr);
                }
            }

            return infected;
        }

        public static void AddInfected(NetPlayer plr)
        {
            string gamemode = GorillaGameManager.instance.GameModeName().ToLower();
            if (gamemode.Contains("infection") || gamemode.Contains("tag"))
            {
                GorillaTagManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla Tag Manager").GetComponent<GorillaTagManager>();
                if (tagman.isCurrentlyTag)
                {
                    tagman.ChangeCurrentIt(plr);
                }
                else
                {
                    if (!tagman.currentInfected.Contains(plr))
                    {
                        tagman.AddInfectedPlayer(plr);
                    }
                }
            }
            if (gamemode.Contains("ambush") || gamemode.Contains("stealth"))
            {
                GorillaAmbushManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla Stealth Manager").GetComponent<GorillaAmbushManager>();
                if (tagman.isCurrentlyTag)
                {
                    tagman.ChangeCurrentIt(plr);
                }
                else
                {
                    if (!tagman.currentInfected.Contains(plr))
                    {
                        tagman.AddInfectedPlayer(plr);
                    }
                }
            }
            if (gamemode.Contains("ghost"))
            {
                GorillaAmbushManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla GhostTag Manager").GetComponent<GorillaAmbushManager>();
                if (tagman.isCurrentlyTag)
                {
                    tagman.ChangeCurrentIt(plr);
                }
                else
                {
                    if (!tagman.currentInfected.Contains(plr))
                    {
                        tagman.AddInfectedPlayer(plr);
                    }
                }
            }
            if (gamemode.Contains("freeze"))
            {
                GorillaFreezeTagManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla Freeze Tag Manager").GetComponent<GorillaFreezeTagManager>();
                if (tagman.isCurrentlyTag)
                {
                    tagman.ChangeCurrentIt(plr);
                }
                else
                {
                    if (!tagman.currentInfected.Contains(plr))
                    {
                        tagman.AddInfectedPlayer(plr);
                    }
                }
            }
        }

        public static void RemoveInfected(NetPlayer plr)
        {
            string gamemode = GorillaGameManager.instance.GameModeName().ToLower();
            if (gamemode.Contains("infection") || gamemode.Contains("tag"))
            {
                GorillaTagManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla Tag Manager").GetComponent<GorillaTagManager>();
                if (tagman.isCurrentlyTag)
                {
                    if (tagman.currentIt == plr)
                    {
                        tagman.currentIt = null;
                    }
                }
                else
                {
                    if (tagman.currentInfected.Contains(plr))
                    {
                        tagman.currentInfected.Remove(plr);
                    }
                }
            }
            if (gamemode.Contains("ambush") || gamemode.Contains("stealth"))
            {
                GorillaAmbushManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla Stealth Manager").GetComponent<GorillaAmbushManager>();
                if (tagman.isCurrentlyTag)
                {
                    if (tagman.currentIt == plr)
                    {
                        tagman.currentIt = null;
                    }
                }
                else
                {
                    if (tagman.currentInfected.Contains(plr))
                    {
                        tagman.currentInfected.Remove(plr);
                    }
                }
            }
            if (gamemode.Contains("ghost"))
            {
                GorillaAmbushManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla GhostTag Manager").GetComponent<GorillaAmbushManager>();
                if (tagman.isCurrentlyTag)
                {
                    if (tagman.currentIt == plr)
                    {
                        tagman.currentIt = null;
                    }
                }
                else
                {
                    if (tagman.currentInfected.Contains(plr))
                    {
                        tagman.currentInfected.Remove(plr);
                    }
                }
            }
            if (gamemode.Contains("freeze"))
            {
                GorillaFreezeTagManager tagman = GameObject.Find("GT Systems/GameModeSystem/Gorilla Freeze Tag Manager").GetComponent<GorillaFreezeTagManager>();
                if (tagman.isCurrentlyTag)
                {
                    if (tagman.currentIt == plr)
                    {
                        tagman.currentIt = null;
                    }
                }
                else
                {
                    if (tagman.currentInfected.Contains(plr))
                    {
                        tagman.currentInfected.Remove(plr);
                    }
                }
            }
        }
        public static bool PlayerIsTagged(VRRig who)
        {
            string name = who.mainSkin.material.name.ToLower();
            return name.Contains("fected") || name.Contains("it") || name.Contains("stealth") || name.Contains("ice") || !who.nameTagAnchor.activeSelf;
            //return PlayerIsTagged(GorillaTagger.Instance.offlineVRRig);
        }


        public static void ChangeArmLength()
        {
            longarmCycle++;
            if (longarmCycle > 4)
            {
                longarmCycle = 0;
            }

            float[] lengthAmounts = new float[] { 0.75f, 1.1f, 1.25f, 1.5f, 2f };
            armlength = lengthAmounts[longarmCycle];

            string[] lengthNames = new string[] { "Shorter", "Unnoticable", "Normal", "Long", "Extreme" };
            Main.GetIndex("Change Arm Length").overlapText = "Change Arm Length <color=grey>[</color><color=green>" + lengthNames[longarmCycle] + "</color><color=grey>]</color>";
        }

        public static void StickLongArms()
        {
            GorillaLocomotion.GTPlayer.Instance.leftControllerTransform.transform.position = GorillaTagger.Instance.leftHandTransform.position + (GorillaTagger.Instance.leftHandTransform.forward * (armlength - 0.917f));
            GorillaLocomotion.GTPlayer.Instance.rightControllerTransform.transform.position = GorillaTagger.Instance.rightHandTransform.position + (GorillaTagger.Instance.rightHandTransform.forward * (armlength - 0.917f));
        }

        public static void EnableSteamLongArms()
        {
            GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(armlength, armlength, armlength);
        }

        public static void DisableSteamLongArms()
        {
            GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        public static void MultipliedLongArms()
        {
            GorillaLocomotion.GTPlayer.Instance.leftControllerTransform.transform.position = GorillaTagger.Instance.headCollider.transform.position - (GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.leftHandTransform.position) * armlength;
            GorillaLocomotion.GTPlayer.Instance.rightControllerTransform.transform.position = GorillaTagger.Instance.headCollider.transform.position - (GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.rightHandTransform.position) * armlength;
        }

        public static void VerticalLongArms()
        {
            Vector3 lefty = GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.leftHandTransform.position;
            lefty.y *= armlength;
            Vector3 righty = GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.rightHandTransform.position;
            righty.y *= armlength;
            GorillaLocomotion.GTPlayer.Instance.leftControllerTransform.transform.position = GorillaTagger.Instance.headCollider.transform.position - lefty;
            GorillaLocomotion.GTPlayer.Instance.rightControllerTransform.transform.position = GorillaTagger.Instance.headCollider.transform.position - righty;
        }

        public static void HorizontalLongArms()
        {
            Vector3 lefty = GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.leftHandTransform.position;
            lefty.x *= armlength;
            lefty.z *= armlength;
            Vector3 righty = GorillaTagger.Instance.headCollider.transform.position - GorillaTagger.Instance.rightHandTransform.position;
            righty.x *= armlength;
            righty.z *= armlength;
            GorillaLocomotion.GTPlayer.Instance.leftControllerTransform.transform.position = GorillaTagger.Instance.headCollider.transform.position - lefty;
            GorillaLocomotion.GTPlayer.Instance.rightControllerTransform.transform.position = GorillaTagger.Instance.headCollider.transform.position - righty;
        }

        public static GameObject lvT = null;
        public static GameObject rvT = null;
        public static void CreateVelocityTrackers()
        {
            lvT = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(lvT.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(lvT.GetComponent<Rigidbody>());
            lvT.GetComponent<Renderer>().enabled = false;
            lvT.AddComponent<GorillaVelocityTracker>();

            rvT = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(rvT.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(rvT.GetComponent<Rigidbody>());
            rvT.GetComponent<Renderer>().enabled = false;
            rvT.AddComponent<GorillaVelocityTracker>();
        }

        
    }
}

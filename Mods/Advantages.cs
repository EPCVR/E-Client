using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using EClient.Classes;
using EClient.Menu;
using EClient.Notifications;
using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using static EClient.Menu.Main;
// This clears the Object confusion
using Object = UnityEngine.Object;


namespace EClient.Mods
{
    internal class Advantages
    {

        // This is the gun color this and other varibles change depending on the edition
        public static ValueTuple<Color, string> currentGunColor;
        public static int longarmCycle = 2;
        public static float armlength = 1.25f;
        public static float GameMode = 2f;
        public static bool ghostMonke = false;
        public static bool rightHand = false;
        public static bool lastHit;
        public static bool lastHit2;
        public static GameObject orb;
        public static GameObject orb2;
        public static GameObject GunSphere;
        public static bool isSphereEnabled;
        private static LineRenderer lineRenderer;
        private static Vector3[] linePositions;
        private static Vector3 previousControllerPosition;
        private static float timeCounter = 0f;
        public static bool gunLocked;
        public static bool SwapGunHand;
        public static bool GriplessGuns;
        public static bool TriggerlessGuns;
        private static VRRig _giveGunTarget;
        public static bool isMenuButtonHeld;
        public static bool shouldBePC;
        public static bool leftPrimary;
        public static bool leftSecondary;
        public static bool rightPrimary;
        public static bool rightSecondary;
        public static bool leftGrab;
        public static bool rightGrab;
        public static float leftTrigger;
        public static float rightTrigger;
        public static bool HardGunLocks;

        public static void TagSelf()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Main.AddInfected(PhotonNetwork.LocalPlayer);
                NotifiLib.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> <color=white>You have been tagged.</color>");
                Main.GetIndex("Tag Self").enabled = false;
            }
            else
            {
                if (EClient.Menu.Main.InfectedList().Contains(PhotonNetwork.LocalPlayer))
                {
                    NotifiLib.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> <color=white>You have been tagged.</color>");
                    VRRig.LocalRig.enabled = true;
                    Main.GetIndex("Tag Self").enabled = false;
                }
                else
                {
                    VRRig rig = GorillaParent.instance.vrrigs.Where(rig => Main.PlayerIsTagged(rig)).OrderBy(rig => rig.LatestVelocity().magnitude).FirstOrDefault();
                    if (Main.PlayerIsTagged(rig))
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = rig.rightHandTransform.position;

                        if (Main.GetIndex("Obnoxious Tag").enabled)
                        {
                            Quaternion rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0));
                            VRRig.LocalRig.transform.rotation = rotation;

                            VRRig.LocalRig.head.rigTarget.transform.rotation = Main.RandomQuaternion();
                            VRRig.LocalRig.leftHand.rigTarget.transform.position = VRRig.LocalRig.transform.position + Main.RandomVector3();
                            VRRig.LocalRig.rightHand.rigTarget.transform.position = VRRig.LocalRig.transform.position + Main.RandomVector3();

                            VRRig.LocalRig.leftHand.rigTarget.transform.rotation = Main.RandomQuaternion();
                            VRRig.LocalRig.rightHand.rigTarget.transform.rotation = Main.RandomQuaternion();

                            VRRig.LocalRig.leftIndex.calcT = 0f;
                            VRRig.LocalRig.leftMiddle.calcT = 0f;
                            VRRig.LocalRig.leftThumb.calcT = 0f;

                            VRRig.LocalRig.leftIndex.LerpFinger(1f, false);
                            VRRig.LocalRig.leftMiddle.LerpFinger(1f, false);
                            VRRig.LocalRig.leftThumb.LerpFinger(1f, false);

                            VRRig.LocalRig.rightIndex.calcT = 0f;
                            VRRig.LocalRig.rightMiddle.calcT = 0f;
                            VRRig.LocalRig.rightThumb.calcT = 0f;

                            VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
                            VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
                            VRRig.LocalRig.rightThumb.LerpFinger(1f, false);
                        }
                    }
                }
            }
        }
        public static bool GetGunInput(bool isShooting)
        {
            if (giveGunTarget != null)
            {
                if (isShooting)
                    return TriggerlessGuns || (SwapGunHand ? giveGunTarget.leftIndex.calcT > 0.5f : giveGunTarget.rightIndex.calcT > 0.5f);
                else
                    return GriplessGuns || (SwapGunHand ? giveGunTarget.leftMiddle.calcT > 0.5f : giveGunTarget.rightMiddle.calcT > 0.5f);
            }

            if (isShooting)
                return TriggerlessGuns || (SwapGunHand ? leftTrigger > 0.5f : rightTrigger > 0.5f) || Mouse.current.leftButton.isPressed;
            else
                return GriplessGuns || (SwapGunHand ? leftGrab : rightGrab) || (HardGunLocks && gunLocked && !rightSecondary) || Mouse.current.rightButton.isPressed;
        }
        public static VRRig giveGunTarget
        {
            get
            {
                if (!GorillaParent.instance.vrrigs.Contains(_giveGunTarget))
                    _giveGunTarget = null;

                return _giveGunTarget;
            }
            set => _giveGunTarget = value;
        }
        public static bool ValidateTag(VRRig Rig) =>
            Vector3.Distance(Main.ServerSyncPos, Rig.transform.position) < 6f;
        public static void TagGun()
        {
            if (GetGunInput(false))
            {
                var GunData = Main.RenderGun();
                RaycastHit Ray = GunData.Ray;
                GameObject NewPointer = GunData.NewPointer;

                if (gunLocked && lockTarget != null)
                {
                    if (!PlayerIsTagged(lockTarget))
                    {
                        VRRig.LocalRig.enabled = false;

                        if (!Main.GetIndex("Obnoxious Tag").enabled)
                            VRRig.LocalRig.transform.position = lockTarget.transform.position - new Vector3(0f, 3f, 0f);
                        else
                        {
                            Vector3 position = lockTarget.transform.position + RandomVector3();

                            VRRig.LocalRig.transform.position = position;

                            VRRig.LocalRig.head.rigTarget.transform.rotation = RandomQuaternion();
                            VRRig.LocalRig.leftHand.rigTarget.transform.position = lockTarget.transform.position + RandomVector3();
                            VRRig.LocalRig.rightHand.rigTarget.transform.position = lockTarget.transform.position + RandomVector3();

                            VRRig.LocalRig.leftHand.rigTarget.transform.rotation = RandomQuaternion();
                            VRRig.LocalRig.rightHand.rigTarget.transform.rotation = RandomQuaternion();

                            VRRig.LocalRig.leftIndex.calcT = 0f;
                            VRRig.LocalRig.leftMiddle.calcT = 0f;
                            VRRig.LocalRig.leftThumb.calcT = 0f;

                            VRRig.LocalRig.leftIndex.LerpFinger(1f, false);
                            VRRig.LocalRig.leftMiddle.LerpFinger(1f, false);
                            VRRig.LocalRig.leftThumb.LerpFinger(1f, false);

                            VRRig.LocalRig.rightIndex.calcT = 0f;
                            VRRig.LocalRig.rightMiddle.calcT = 0f;
                            VRRig.LocalRig.rightThumb.calcT = 0f;

                            VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
                            VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
                            VRRig.LocalRig.rightThumb.LerpFinger(1f, false);
                        }

                        if (ValidateTag(lockTarget))
                            ReportTag(lockTarget);
                    }
                    else
                    {
                        gunLocked = false;
                        VRRig.LocalRig.enabled = true;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !PlayerIsLocal(gunTarget) && !PlayerIsTagged(gunTarget))
                    {
                        if (PhotonNetwork.LocalPlayer.IsMasterClient)
                            Main.AddInfected(RigManager.GetPlayerFromVRRig(gunTarget));
                        else
                        {
                            if (PlayerIsTagged(VRRig.LocalRig))
                            {
                                gunLocked = true;
                                lockTarget = gunTarget;
                            }
                        }
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void NoTagOnJoin()
        {
            PlayerPrefs.SetString("tutorial", "nope");
            PlayerPrefs.SetString("didTutorial", "nope");

            // use the Photon Hashtable, not System.Collections
            ExitGames.Client.Photon.Hashtable h = new ExitGames.Client.Photon.Hashtable
            {
                { "didTutorial", false }
            };

        PhotonNetwork.LocalPlayer.SetCustomProperties(h, null, null);
        PlayerPrefs.Save();
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
        public static GameObject pointer = null;
        public static LineRenderer Line;
        public static RaycastHit raycastHit;
        public static bool hand = false;
        public static bool hand1 = false;
        public static void MakeGun(Color color, Vector3 pointersize, float linesize, PrimitiveType pointershape, Transform arm, bool liner, Action shit, Action shit1)
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(arm.position, -arm.up, out raycastHit);
                if (pointer == null) { pointer = GameObject.CreatePrimitive(pointershape); }
                pointer.transform.localScale = pointersize;
                pointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                pointer.transform.position = raycastHit.point;
                pointer.GetComponent<Renderer>().material.color = color;
                if (liner)
                {
                    GameObject g = new GameObject("Line");
                    Line = g.AddComponent<LineRenderer>();
                    Line.material.shader = Shader.Find("GUI/Text Shader");
                    Line.startWidth = linesize;
                    Line.endWidth = linesize;
                    Line.startColor = color;
                    Line.endColor = color;
                    Line.positionCount = 2;
                    Line.useWorldSpace = true;
                    Line.SetPosition(0, arm.position);
                    Line.SetPosition(1, pointer.transform.position);
                    Object.Destroy(g, Time.deltaTime);
                }
                Object.Destroy(pointer.GetComponent<BoxCollider>());
                Object.Destroy(pointer.GetComponent<Rigidbody>());
                Object.Destroy(pointer.GetComponent<Collider>());
                if (hand1)
                {
                    shit.Invoke();
                }
                else
                {
                    shit1.Invoke();
                }
            }
            else
            {
                if (pointer != null)
                {
                    Object.Destroy(pointer, Time.deltaTime);
                }
            }
        }
    }



}

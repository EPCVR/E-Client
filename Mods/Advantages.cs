using System;
using System.Collections.Generic;
using System.Text;
using GorillaLocomotion.Climbing;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using StupidTemplate.Menu;
using StupidTemplate.Notifications;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
// This clears the Object confusion
using Object = UnityEngine.Object;
using System.Reflection;


namespace StupidTemplate.Mods
{
    internal class Advantages
    {
        // This is the gun color this and other varibles change depending on the edition
        public static Color CurrentGunColor = Color.blue;
        public static int longarmCycle = 2;
        public static float armlength = 1.25f;
        public static float GameMode = 2f;
        public static bool ghostMonke = false;
        public static bool rightHand = false;
        public static bool lastHit;
        public static bool lastHit2;
        public static GameObject orb;
        public static GameObject orb2;
        public static void DrawHandOrbs()
        {
            orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject.Destroy(orb.GetComponent<Rigidbody>());
            GameObject.Destroy(orb.GetComponent<SphereCollider>());
            GameObject.Destroy(orb2.GetComponent<Rigidbody>());
            GameObject.Destroy(orb2.GetComponent<SphereCollider>());
            orb.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            orb2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            orb.transform.position = GorillaTagger.Instance.leftHandTransform.position;
            orb2.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            orb.GetComponent<Renderer>().material.color = CurrentGunColor;
            orb2.GetComponent<Renderer>().material.color = CurrentGunColor;
            GameObject.Destroy(orb, Time.deltaTime);
            GameObject.Destroy(orb2, Time.deltaTime);
        }
        public static void TagGun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                MakeGun(CurrentGunColor, new Vector3(0.15f, 0.15f, 0.15f), 0.025f, PrimitiveType.Sphere, GorillaLocomotion.GTPlayer.Instance.rightControllerTransform, true, delegate
                {
                    if (ControllerInputPoller.instance.rightControllerPrimaryButtonTouch)
                    {
                        VRRig vrrig = raycastHit.collider.GetComponent<VRRig>();
                        GorillaTagger.Instance.offlineVRRig.enabled = false;
                        GorillaTagger.Instance.offlineVRRig.transform.position = vrrig.transform.position;
                    }
                }, delegate { });
            }
        }
        public static void TagAll()
        {
            GorillaTagger.Instance.offlineVRRig.enabled = false;
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                GorillaTagger.Instance.offlineVRRig.transform.position = rig.transform.position - new Vector3(0f, -3f, 0f);
                GorillaTagger.Instance.myVRRig.transform.position = rig.transform.position - new Vector3(0f, -3f, 0f);
                GorillaLocomotion.GTPlayer.Instance.rightControllerTransform.position = rig.transform.position;
            }
            GorillaTagger.Instance.offlineVRRig.enabled = true;
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

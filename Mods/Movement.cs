
using System;
using System.Collections.Generic;
using System.Text;
using StupidTemplate.Classes;
using StupidTemplate.Menu;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StupidTemplate.Mods
{
    internal class Movement
    {
        public static int speedboostCycle = 1;
        public static float jspeed = 7.5f;
        public static float jmulti = 1.25f;

        public static int flySpeedCycle = 1;
        public static float flySpeed = 10f;


        public static void CarMonke()
        {
            if (ControllerInputPoller.instance.rightControllerIndexTouch > .1f)
            {
                GorillaLocomotion.GTPlayer.Instance.transform.position += GorillaLocomotion.GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * 15f;
            }

            if (ControllerInputPoller.instance.rightGrab)
            {
                GorillaLocomotion.GTPlayer.Instance.transform.position -= GorillaLocomotion.GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * 20f;
            }
        }

        public static void NoClip()
        {
            bool disablcolliders = ControllerInputPoller.instance.rightControllerIndexTouch > 0.1f;
            MeshCollider[] colliders = Resources.FindObjectsOfTypeAll<MeshCollider>();

            foreach (MeshCollider collider in colliders)
            {
                collider.enabled = !disablcolliders;
            }
        }

        public static void GhostMonke()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton || Mouse.current.rightButton.isPressed)
            {
                GorillaTagger.Instance.offlineVRRig.enabled = false;
            }

            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }
        public static void Frozone()
        {
            if (ControllerInputPoller.instance.leftGrab)
            {
                GameObject gameObject = GameObject.CreatePrimitive((PrimitiveType)3);
                gameObject.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                gameObject.transform.localPosition = GorillaTagger.Instance.leftHandTransform.position + new Vector3(0f, -0.05f, 0f);
                gameObject.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                UnityEngine.Object.Destroy(gameObject, 0.3f);
            }
            {
                GameObject gameObject = GameObject.CreatePrimitive((PrimitiveType)3);
                gameObject.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                gameObject.transform.localPosition = GorillaTagger.Instance.leftHandTransform.position + new Vector3(0f, -0.05f, 0f);
                gameObject.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                gameObject.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                UnityEngine.Object.Destroy(gameObject, 0.3f);
            }
            if (ControllerInputPoller.instance.rightGrab)
            {
                GameObject gameObject2 = GameObject.CreatePrimitive((PrimitiveType)3);
                gameObject2.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                gameObject2.transform.localPosition = GorillaTagger.Instance.rightHandTransform.position + new Vector3(0f, -0.05f, 0f);
                gameObject2.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                gameObject2.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
                gameObject2.GetComponent<Renderer>().material.color = Color.cyan;
                UnityEngine.Object.Destroy(gameObject2, 0.3f);
            }
        }

        public static void ChangeFlySpeed()
        {
            flySpeedCycle++;
            if (flySpeedCycle > 4)
            {
                flySpeedCycle = 0;
            }

            float[] speedamounts = new float[] { 5f, 10f, 30f, 60f, 0.5f };
            flySpeed = speedamounts[flySpeedCycle];

            string[] speedNames = new string[] { "Slow", "Normal", "Fast", "Extra Fast", "Extra Slow" };
            Main.GetIndex("Change Fly Speed").overlapText = "Change Fly Speed <color=grey>[</color><color=green>" + speedNames[flySpeedCycle] + "</color><color=grey>]</color>";
        }

        public static void Fly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                GorillaLocomotion.GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * flySpeed;
                GorillaLocomotion.GTPlayer.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }

        public static void ChangeSpeedBoostAmount()
        {
            speedboostCycle++;
            if (speedboostCycle > 3)
            {
                speedboostCycle = 0;
            }

            float[] jspeedamounts = new float[] { 2f, 7.5f, 9f, 200f };
            jspeed = jspeedamounts[speedboostCycle];

            float[] jmultiamounts = new float[] { 0.5f, /*1.25f*/1.1f, 2f, 10f };
            jmulti = jmultiamounts[speedboostCycle];

            string[] speedNames = new string[] { "Slow", "Normal", "Fast", "Ultra Fast" };
            Main.GetIndex("Change Speed Boost Amount").overlapText = "Change Speed Boost Amount <color=grey>[</color><color=green>" + speedNames[speedboostCycle] + "</color><color=grey>]</color>";
        }

        public static void SpeedBoost()
        {
            float jspt = jspeed;
            float jmpt = jmulti;
            if (Main.GetIndex("Factored Speed Boost").enabled)
            {
                jspt = (jspt / 6.5f) * GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed;
                jmpt = (jmpt / 1.1f) * GorillaLocomotion.GTPlayer.Instance.jumpMultiplier;
            }
            if (!Main.GetIndex("Disable Max Speed Modification").enabled)
            {
                GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed = jspeed;
            }
            GorillaLocomotion.GTPlayer.Instance.jumpMultiplier = jmulti;
        }

        

        public static Color PlatColorA = new Color32(0, 255, 246, 255);
        public static Color PlatColorB = new Color32(0, 255, 144, 255);

        public static GameObject platl;
        public static GameObject platr;

        public static void PlatformMod()

        {
            if (ControllerInputPoller.instance.leftGrab && leftplat == null)
            {
                leftplat = CreatePlatformOnHand(GorillaTagger.Instance.leftHandTransform);
            }

            if (ControllerInputPoller.instance.rightGrab && rightplat == null)
            {
                rightplat = CreatePlatformOnHand(GorillaTagger.Instance.rightHandTransform);
            }

            if (ControllerInputPoller.instance.rightGrabRelease && rightplat != null)
            {
                rightplat.Disable();
                rightplat = null;

            }

            if (ControllerInputPoller.instance.leftGrabRelease && leftplat != null)
            {
                leftplat.Disable();
                leftplat = null;
            }
        }
        private static GameObject leftplat = null;
        private static GameObject rightplat = null;
        private static GameObject CreatePlatformOnHand(Transform handTransform)
        {
            GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plat.transform.localScale = new Vector3(0.020f, 0.2f, 0.3f);

            plat.transform.position = handTransform.position;
            plat.transform.rotation = handTransform.rotation;

            float h = (Time.frameCount / 180f) % 1f;
            plat.GetComponent<Renderer>().material.color = Color.white;
            return plat;
        }
    }
} 


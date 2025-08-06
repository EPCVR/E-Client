
using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using GorillaLocomotion;
using StupidTemplate.Classes;
using StupidTemplate.Menu;
using StupidTemplate.Patches;
using UnityEngine;
using UnityEngine.InputSystem;
using Valve.VR;
namespace StupidTemplate.Mods
{
    internal class Movement
    {
        public static int speedboostCycle = 1;
        public static float jspeed = 7.5f;
        public static float jmulti = 1.25f;

        public static int flySpeedCycle = 1;
        public static float flySpeed = 10f;

        public static bool noclip;

        public static Vector2 leftJoystick = Vector2.zero;
        public static Vector2 rightJoystick = Vector2.zero;
        public static bool leftJoystickClick;
        public static bool rightJoystickClick;

        public static bool scaleWithPlayer;

        public static float startX = -1f;
        public static float startY = -1f;

        public static float subThingy;
        public static float subThingyZ;

        public static Vector3 lastPosition = Vector3.zero;
        public static void GripSpeedboost()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                SpeedBoost();
            }
        }

        public static void TriggerSpeedBoost()
        {
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
            {
                SpeedBoost();
            }
        }

        public static void NoclipFly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * flySpeed;
                GorillaTagger.Instance.rigidbody.velocity = Vector3.zero;
                if (noclip == false)
                {
                    noclip = true;
                    UpdateClipColliders(false);
                }
            }
            else
            {
                if (noclip == true)
                {
                    noclip = false;
                    UpdateClipColliders(true);
                }
            }
        }
        public static void CarMonke()
        {
            if (ControllerInputPoller.instance.leftGrab)
            {
                GorillaLocomotion.GTPlayer.Instance.transform.position += GorillaLocomotion.GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * 15f;
            }

            if (ControllerInputPoller.instance.rightGrab)
            {
                GorillaLocomotion.GTPlayer.Instance.transform.position -= GorillaLocomotion.GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * 20f;
            }
        }

        public static void Noclip()
        {
            bool gripNoclip = Main.GetIndex("Grip Noclip").enabled;
            if (gripNoclip ? ControllerInputPoller.instance.rightGrab : ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f || UnityInput.Current.GetKey(KeyCode.E))
            {
                if (noclip == false)
                {
                    noclip = true;
                    UpdateClipColliders(false);
                }
            }
            else
            {
                if (noclip == true)
                {
                    noclip = false;
                    UpdateClipColliders(true);
                }
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

        public static void ChangeSpeedBoostAmount(){ speedboostCycle++;
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

        public static void SpeedBoost(){ GorillaLocomotion.GTPlayer.Instance.jumpMultiplier = jmulti; GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed = jspeed;}

        

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

        public static void UpdateClipColliders(bool enabled)
        {
            foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                v.enabled = enabled;
        }

        public static void JoystickFly()
        {
            Vector2 joy = leftJoystick;

            if (Mathf.Abs(joy.x) > 0.3 || Mathf.Abs(joy.y) > 0.3)
            {
                GTPlayer.Instance.transform.position += (GorillaTagger.Instance.headCollider.transform.forward * Time.deltaTime * (joy.y * flySpeed)) + (GorillaTagger.Instance.headCollider.transform.right * Time.deltaTime * (joy.x * flySpeed));
                GorillaTagger.Instance.rigidbody.velocity = Vector3.zero;
            }
        }

        public static void BarkFly()
        {
            Vector3 inputDirection = new Vector3(leftJoystick.x, rightJoystick.y, leftJoystick.y);

            Vector3 playerForward = GTPlayer.Instance.bodyCollider.transform.forward;
            playerForward.y = 0;
            Vector3 playerRight = GTPlayer.Instance.bodyCollider.transform.right;
            playerRight.y = 0;


            Vector3 velocity = inputDirection.x * playerRight + inputDirection.y * Vector3.up + inputDirection.z * playerForward;
            velocity *= GTPlayer.Instance.scale * flySpeed;
            GorillaTagger.Instance.rigidbody.velocity = Vector3.Lerp(GorillaTagger.Instance.rigidbody.velocity, velocity, 0.12875f);
        }

        public static void VelocityBarkFly()
        {
            if ((Mathf.Abs(leftJoystick.x) > 0.3 || Mathf.Abs(leftJoystick.y) > 0.3) || (Mathf.Abs(rightJoystick.x) > 0.3 || Mathf.Abs(rightJoystick.y) > 0.3))
                BarkFly();
        }

        public static void HandFly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                GTPlayer.Instance.transform.position += TrueRightHand().forward * Time.deltaTime * flySpeed;
                GorillaTagger.Instance.rigidbody.velocity = Vector3.zero;
            }
        }

        public static void SlingshotFly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
                GorillaTagger.Instance.rigidbody.velocity += GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * (flySpeed * 2);
        }
        public static void WASDFly()
        {
            bool stationary = !Main.GetIndex("Disable Stationary WASD Fly").enabled;

            bool W = UnityInput.Current.GetKey(KeyCode.W);
            bool A = UnityInput.Current.GetKey(KeyCode.A);
            bool S = UnityInput.Current.GetKey(KeyCode.S);
            bool D = UnityInput.Current.GetKey(KeyCode.D);
            bool Space = UnityInput.Current.GetKey(KeyCode.Space);
            bool Ctrl = UnityInput.Current.GetKey(KeyCode.LeftControl);

            if (stationary || W || A || S || D || Space || Ctrl)
                GorillaTagger.Instance.rigidbody.velocity = Vector3.zero;

            if (!Main.menu)
            {
                if (Mouse.current.rightButton.isPressed)
                {
                    Transform parentTransform = GTPlayer.Instance.rightControllerTransform.parent;
                    Quaternion currentRotation = parentTransform.rotation;
                    Vector3 euler = currentRotation.eulerAngles;

                    if (startX < 0)
                    {
                        startX = euler.y;
                        subThingy = Mouse.current.position.value.x / Screen.width;
                    }
                    if (startY < 0)
                    {
                        startY = euler.x;
                        subThingyZ = Mouse.current.position.value.y / Screen.height;
                    }

                    float newX = startY - ((((Mouse.current.position.value.y / Screen.height) - subThingyZ) * 360) * 1.33f);
                    float newY = startX + ((((Mouse.current.position.value.x / Screen.width) - subThingy) * 360) * 1.33f);

                    newX = (newX > 180f) ? newX - 360f : newX;
                    newX = Mathf.Clamp(newX, -90f, 90f);

                    parentTransform.rotation = Quaternion.Euler(newX, newY, euler.z);
                }
                else
                {
                    startX = -1;
                    startY = -1;
                }

                float speed = flySpeed;
                if (UnityInput.Current.GetKey(KeyCode.LeftShift))
                    speed *= 2f;

                if (W)
                    GorillaTagger.Instance.rigidbody.transform.position += GTPlayer.Instance.rightControllerTransform.parent.forward * Time.deltaTime * speed;

                if (S)
                    GorillaTagger.Instance.rigidbody.transform.position += GTPlayer.Instance.rightControllerTransform.parent.forward * Time.deltaTime * -speed;

                if (A)
                    GorillaTagger.Instance.rigidbody.transform.position += GTPlayer.Instance.rightControllerTransform.parent.right * Time.deltaTime * -speed;

                if (D)
                    GorillaTagger.Instance.rigidbody.transform.position += GTPlayer.Instance.rightControllerTransform.parent.right * Time.deltaTime * speed;

                if (Space)
                    GorillaTagger.Instance.rigidbody.transform.position += new Vector3(0f, Time.deltaTime * speed, 0f);

                if (Ctrl)
                    GorillaTagger.Instance.rigidbody.transform.position += new Vector3(0f, Time.deltaTime * -speed, 0f);

                VRRig.LocalRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;
            }

            if (!W && !A && !S && !D && !Space && !Ctrl && lastPosition != Vector3.zero && stationary)
                GorillaTagger.Instance.rigidbody.transform.position = lastPosition;
            else
                lastPosition = GorillaTagger.Instance.rigidbody.transform.position;
        }

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueLeftHand()
        {
            Quaternion rot = GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.leftHandRotOffset;
            return (GorillaTagger.Instance.leftHandTransform.position + GorillaTagger.Instance.leftHandTransform.rotation * (GTPlayer.Instance.leftHandOffset * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f)), rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            Quaternion rot = GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.rightHandRotOffset;
            return (GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * (GTPlayer.Instance.rightHandOffset * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f)), rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }
    }
} 


using System;
using BepInEx;
using GorillaLocomotion;
using EClient.Mods;
using UnityEngine;
using UnityEngine.XR;

namespace EClient.Libs
{
    // Token: 0x02000009 RID: 9
    internal class GunLib
    {
        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x0000336C
        public static void GunCleanUp()
        {
            bool flag = pointer == null || lr == null;
            if (!flag)
            {
                UnityEngine.Object.Destroy(pointer);
                pointer = null;
                UnityEngine.Object.Destroy(lr.gameObject);
                lr = null;
                data = new GunLibData(false, false, false, null, default, default);
            }
        }

        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x000033E4
        public static GunLibData ShootLock()
        {
            GunLibData result;
            try
            {
                bool isDeviceActive = XRSettings.isDeviceActive;
                if (isDeviceActive)
                {
                    bool flag = false;
                    bool flag2 = !flag;
                    Transform transform;
                    if (flag2)
                    {
                        transform = GTPlayer.Instance.rightControllerTransform;
                        data.isShooting = ControllerInputPoller.instance.rightGrab;
                        data.isTriggered = ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f;
                    }
                    else
                    {
                        transform = GTPlayer.Instance.leftControllerTransform;
                        data.isShooting = ControllerInputPoller.instance.leftGrab;
                        data.isTriggered = ControllerInputPoller.instance.leftControllerIndexFloat > 0.1f;
                    }
                    bool isShooting = data.isShooting;
                    if (isShooting)
                    {
                        Renderer renderer = pointer != null ? pointer.GetComponent<Renderer>() : null;
                        bool flag3 = data.lockedPlayer == null && !data.isLocked;
                        if (flag3)
                        {
                            RaycastHit raycastHit;
                            bool flag4 = Physics.Raycast(transform.position - transform.up, -transform.up, out raycastHit) && pointer == null;
                            if (flag4)
                            {
                                pointer = GameObject.CreatePrimitive(0);
                                UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                                UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                                pointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                                renderer = pointer != null ? pointer.GetComponent<Renderer>() : null;
                                renderer.material.color = Color.red;
                                renderer.material.shader = Shader.Find("GUI/Text Shader");
                            }
                            bool gunLine = SettingsMods.GunLine;
                            if (gunLine)
                            {
                                bool flag5 = lr == null;
                                if (flag5)
                                {
                                    GameObject gameObject = new GameObject("line");
                                    lr = gameObject.AddComponent<LineRenderer>();
                                    lr.endWidth = 0.01f;
                                    lr.startWidth = 0.01f;
                                    lr.material.shader = Shader.Find("GUI/Text Shader");
                                }
                                lr.SetPosition(0, transform.position);
                                lr.SetPosition(1, raycastHit.point);
                            }
                            data.hitPosition = raycastHit.point;
                            pointer.transform.position = raycastHit.point;
                            VRRig componentInParent = raycastHit.collider.GetComponentInParent<VRRig>();
                            bool flag6 = componentInParent != null;
                            if (flag6)
                            {
                                bool isTriggered = data.isTriggered;
                                if (isTriggered)
                                {
                                    data.lockedPlayer = componentInParent;
                                    data.isLocked = true;
                                    bool gunLine2 = SettingsMods.GunLine;
                                    if (gunLine2)
                                    {
                                        lr.startColor = Color.blue;
                                        lr.endColor = Color.blue;
                                    }
                                    renderer.material.color = Color.blue;
                                }
                                else
                                {
                                    data.isLocked = false;
                                    bool gunLine3 = SettingsMods.GunLine;
                                    if (gunLine3)
                                    {
                                        lr.startColor = Color.green;
                                        lr.endColor = Color.green;
                                    }
                                    renderer.material.color = Color.green;
                                    GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
                                }
                            }
                            else
                            {
                                data.isLocked = false;
                                bool gunLine4 = SettingsMods.GunLine;
                                if (gunLine4)
                                {
                                    lr.startColor = Color.red;
                                    lr.endColor = Color.red;
                                }
                                renderer.material.color = Color.red;
                            }
                        }
                        bool flag7 = data.isTriggered && data.lockedPlayer != null;
                        if (flag7)
                        {
                            data.isLocked = true;
                            bool gunLine5 = SettingsMods.GunLine;
                            if (gunLine5)
                            {
                                lr.SetPosition(0, transform.position);
                                lr.SetPosition(1, data.lockedPlayer.transform.position);
                            }
                            data.hitPosition = data.lockedPlayer.transform.position;
                            pointer.transform.position = data.lockedPlayer.transform.position;
                            bool gunLine6 = SettingsMods.GunLine;
                            if (gunLine6)
                            {
                                lr.startColor = Color.blue;
                                lr.endColor = Color.blue;
                            }
                            renderer.material.color = Color.blue;
                        }
                        else
                        {
                            bool flag8 = data.lockedPlayer != null;
                            if (flag8)
                            {
                                data.isLocked = false;
                                data.lockedPlayer = null;
                                bool gunLine7 = SettingsMods.GunLine;
                                if (gunLine7)
                                {
                                    lr.startColor = Color.red;
                                    lr.endColor = Color.red;
                                }
                                renderer.material.color = Color.red;
                            }
                        }
                    }
                    else
                    {
                        GunCleanUp();
                    }
                    result = data;
                }
                else
                {
                    data.isShooting = UnityInput.Current.GetMouseButton(1);
                    data.isTriggered = UnityInput.Current.GetMouseButton(0);
                    bool isShooting2 = data.isShooting;
                    if (isShooting2)
                    {
                        Renderer renderer2 = pointer != null ? pointer.GetComponent<Renderer>() : null;
                        bool flag9 = data.lockedPlayer == null && !data.isLocked;
                        if (flag9)
                        {
                            Ray ray = GameObject.Find("Shoulder Camera").GetComponent<Camera>() != null ? GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition) : GorillaTagger.Instance.mainCamera.GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition);
                            RaycastHit raycastHit;
                            bool flag10 = Physics.Raycast(ray.origin, ray.direction, out raycastHit) && pointer == null;
                            if (flag10)
                            {
                                pointer = GameObject.CreatePrimitive(0);
                                UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                                UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                                pointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                                renderer2 = pointer != null ? pointer.GetComponent<Renderer>() : null;
                                renderer2.material.color = Color.red;
                                renderer2.material.shader = Shader.Find("GUI/Text Shader");
                            }
                            bool gunLine8 = SettingsMods.GunLine;
                            if (gunLine8)
                            {
                                bool flag11 = lr == null;
                                if (flag11)
                                {
                                    GameObject gameObject2 = new GameObject("line");
                                    lr = gameObject2.AddComponent<LineRenderer>();
                                    lr.endWidth = 0.01f;
                                    lr.startWidth = 0.01f;
                                    lr.material.shader = Shader.Find("GUI/Text Shader");
                                }
                                lr.SetPosition(0, GTPlayer.Instance.headCollider.transform.position);
                                lr.SetPosition(1, raycastHit.point);
                            }
                            data.hitPosition = raycastHit.point;
                            pointer.transform.position = raycastHit.point;
                            VRRig componentInParent2 = raycastHit.collider.GetComponentInParent<VRRig>();
                            bool flag12 = componentInParent2 != null && data.lockedPlayer == null;
                            if (flag12)
                            {
                                bool isTriggered2 = data.isTriggered;
                                if (isTriggered2)
                                {
                                    data.lockedPlayer = componentInParent2;
                                    data.isLocked = true;
                                }
                                else
                                {
                                    data.isLocked = false;
                                    bool gunLine9 = SettingsMods.GunLine;
                                    if (gunLine9)
                                    {
                                        lr.startColor = Color.green;
                                        lr.endColor = Color.green;
                                    }
                                    renderer2.material.color = Color.green;
                                }
                            }
                            else
                            {
                                data.isLocked = false;
                                bool gunLine10 = SettingsMods.GunLine;
                                if (gunLine10)
                                {
                                    lr.startColor = Color.red;
                                    lr.endColor = Color.red;
                                }
                                renderer2.material.color = Color.red;
                            }
                        }
                        bool flag13 = renderer2 != null;
                        if (flag13)
                        {
                            bool flag14 = data.isTriggered && data.lockedPlayer != null;
                            if (flag14)
                            {
                                bool gunLine11 = SettingsMods.GunLine;
                                if (gunLine11)
                                {
                                    lr.SetPosition(0, GTPlayer.Instance.rightControllerTransform.position);
                                    lr.SetPosition(1, data.lockedPlayer.transform.position);
                                }
                                data.hitPosition = data.lockedPlayer.transform.position;
                                pointer.transform.position = data.lockedPlayer.transform.position;
                                data.isLocked = true;
                                bool gunLine12 = SettingsMods.GunLine;
                                if (gunLine12)
                                {
                                    lr.startColor = Color.blue;
                                    lr.endColor = Color.blue;
                                }
                                renderer2.material.color = Color.blue;
                            }
                            else
                            {
                                bool flag15 = data.lockedPlayer != null;
                                if (flag15)
                                {
                                    data.isLocked = false;
                                    data.lockedPlayer = null;
                                    bool gunLine13 = SettingsMods.GunLine;
                                    if (gunLine13)
                                    {
                                        lr.startColor = Color.red;
                                        lr.endColor = Color.red;
                                    }
                                    renderer2.material.color = Color.red;
                                }
                            }
                        }
                    }
                    else
                    {
                        GunCleanUp();
                    }
                    result = data;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                result = null;
            }
            return result;
        }

        // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x00003E98
        public static GunLibData Shoot()
        {
            GunLibData result;
            try
            {
                bool isDeviceActive = XRSettings.isDeviceActive;
                if (isDeviceActive)
                {
                    bool flag = false;
                    bool flag2 = !flag;
                    Transform transform;
                    if (flag2)
                    {
                        transform = GTPlayer.Instance.rightControllerTransform;
                        data.isShooting = ControllerInputPoller.instance.rightGrab;
                        data.isTriggered = ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f;
                    }
                    else
                    {
                        transform = GTPlayer.Instance.leftControllerTransform;
                        data.isShooting = ControllerInputPoller.instance.leftGrab;
                        data.isTriggered = ControllerInputPoller.instance.leftControllerIndexFloat > 0.1f;
                    }
                    bool isShooting = data.isShooting;
                    if (isShooting)
                    {
                        Renderer renderer = pointer != null ? pointer.GetComponent<Renderer>() : null;
                        RaycastHit raycastHit;
                        bool flag3 = Physics.Raycast(transform.position - transform.up, -transform.up, out raycastHit) && pointer == null;
                        if (flag3)
                        {
                            pointer = GameObject.CreatePrimitive(0);
                            UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                            UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                            pointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                            renderer = pointer != null ? pointer.GetComponent<Renderer>() : null;
                            renderer.material.color = Color.red;
                            renderer.material.shader = Shader.Find("GUI/Text Shader");
                        }
                        bool gunLine = SettingsMods.GunLine;
                        if (gunLine)
                        {
                            bool flag4 = lr == null;
                            if (flag4)
                            {
                                GameObject gameObject = new GameObject("line");
                                lr = gameObject.AddComponent<LineRenderer>();
                                lr.endWidth = 0.01f;
                                lr.startWidth = 0.01f;
                                lr.material.shader = Shader.Find("GUI/Text Shader");
                            }
                            lr.SetPosition(0, transform.position);
                            lr.SetPosition(1, raycastHit.point);
                        }
                        data.hitPosition = raycastHit.point;
                        data.RaycastHit = raycastHit;
                        pointer.transform.position = raycastHit.point;
                        VRRig componentInParent = raycastHit.collider.GetComponentInParent<VRRig>();
                        bool flag5 = componentInParent != null;
                        if (flag5)
                        {
                            bool isTriggered = data.isTriggered;
                            if (isTriggered)
                            {
                                data.lockedPlayer = componentInParent;
                                data.isLocked = true;
                                renderer.material.color = Color.blue;
                                bool gunLine2 = SettingsMods.GunLine;
                                if (gunLine2)
                                {
                                    lr.startColor = Color.blue;
                                    lr.endColor = Color.blue;
                                }
                            }
                            else
                            {
                                bool gunLine3 = SettingsMods.GunLine;
                                if (gunLine3)
                                {
                                    lr.startColor = Color.green;
                                    lr.endColor = Color.green;
                                }
                                renderer.material.color = Color.green;
                                data.isLocked = false;
                                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength / 3f, GorillaTagger.Instance.tagHapticDuration / 2f);
                            }
                        }
                        else
                        {
                            bool gunLine4 = SettingsMods.GunLine;
                            if (gunLine4)
                            {
                                lr.startColor = Color.red;
                                lr.endColor = Color.red;
                            }
                            renderer.material.color = Color.red;
                            data.isLocked = false;
                        }
                    }
                    else
                    {
                        GunCleanUp();
                    }
                    result = data;
                }
                else
                {
                    data.isShooting = true;
                    data.isTriggered = UnityInput.Current.GetMouseButton(0);
                    Renderer renderer2 = pointer != null ? pointer.GetComponent<Renderer>() : null;
                    Ray ray = GameObject.Find("Shoulder Camera").GetComponent<Camera>() != null ? GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition) : GorillaTagger.Instance.mainCamera.GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition);
                    RaycastHit raycastHit2;
                    bool flag6 = Physics.Raycast(ray.origin, ray.direction, out raycastHit2) && pointer == null;
                    if (flag6)
                    {
                        pointer = GameObject.CreatePrimitive(0);
                        UnityEngine.Object.Destroy(pointer.GetComponent<Rigidbody>());
                        UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());
                        pointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        renderer2 = pointer != null ? pointer.GetComponent<Renderer>() : null;
                        renderer2.material.color = Color.red;
                        renderer2.material.shader = Shader.Find("GUI/Text Shader");
                    }
                    bool gunLine5 = SettingsMods.GunLine;
                    if (gunLine5)
                    {
                        bool flag7 = lr == null;
                        if (flag7)
                        {
                            GameObject gameObject2 = new GameObject("line");
                            lr = gameObject2.AddComponent<LineRenderer>();
                            lr.endWidth = 0.01f;
                            lr.startWidth = 0.01f;
                            lr.material.shader = Shader.Find("GUI/Text Shader");
                        }
                        lr.SetPosition(0, GTPlayer.Instance.headCollider.transform.position);
                        lr.SetPosition(1, raycastHit2.point);
                    }
                    data.hitPosition = raycastHit2.point;
                    pointer.transform.position = raycastHit2.point;
                    VRRig componentInParent2 = raycastHit2.collider.GetComponentInParent<VRRig>();
                    bool flag8 = componentInParent2 != null;
                    if (flag8)
                    {
                        bool isTriggered2 = data.isTriggered;
                        if (isTriggered2)
                        {
                            data.isLocked = true;
                            bool gunLine6 = SettingsMods.GunLine;
                            if (gunLine6)
                            {
                                lr.startColor = Color.blue;
                                lr.endColor = Color.blue;
                            }
                            renderer2.material.color = Color.blue;
                        }
                        else
                        {
                            data.isLocked = false;
                            bool gunLine7 = SettingsMods.GunLine;
                            if (gunLine7)
                            {
                                lr.startColor = Color.green;
                                lr.endColor = Color.green;
                            }
                            renderer2.material.color = Color.green;
                        }
                    }
                    else
                    {
                        data.isLocked = false;
                        bool gunLine8 = SettingsMods.GunLine;
                        if (gunLine8)
                        {
                            lr.startColor = Color.red;
                            lr.endColor = Color.red;
                        }
                        renderer2.material.color = Color.red;
                    }
                    result = data;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                result = null;
            }
            return result;
        }

        // Token: 0x04000022 RID: 34
        private static GameObject pointer;

        // Token: 0x04000023 RID: 35
        private static LineRenderer lr;

        // Token: 0x04000024 RID: 36
        private static GunLibData data = new GunLibData(false, false, false, null, default, default);

        // Token: 0x02000042 RID: 66
        public class GunLibData
        {
            // Token: 0x1700001A RID: 26
            // (get) Token: 0x0600020F RID: 527 RVA: 0x0001A535 File Offset: 0x00018735
            // (set) Token: 0x06000210 RID: 528 RVA: 0x0001A53D File Offset: 0x0001873D
            public VRRig lockedPlayer { get; set; }

            // Token: 0x1700001B RID: 27
            // (get) Token: 0x06000211 RID: 529 RVA: 0x0001A546 File Offset: 0x00018746
            // (set) Token: 0x06000212 RID: 530 RVA: 0x0001A54E File Offset: 0x0001874E
            public bool isShooting { get; set; }

            // Token: 0x1700001C RID: 28
            // (get) Token: 0x06000213 RID: 531 RVA: 0x0001A557 File Offset: 0x00018757
            // (set) Token: 0x06000214 RID: 532 RVA: 0x0001A55F File Offset: 0x0001875F
            public bool isLocked { get; set; }

            // Token: 0x1700001D RID: 29
            // (get) Token: 0x06000215 RID: 533 RVA: 0x0001A568 File Offset: 0x00018768
            // (set) Token: 0x06000216 RID: 534 RVA: 0x0001A570 File Offset: 0x00018770
            public Vector3 hitPosition { get; set; }

            // Token: 0x1700001E RID: 30
            // (get) Token: 0x06000217 RID: 535 RVA: 0x0001A579 File Offset: 0x00018779
            // (set) Token: 0x06000218 RID: 536 RVA: 0x0001A581 File Offset: 0x00018781
            public GameObject hitPointer { get; set; }

            // Token: 0x1700001F RID: 31
            // (get) Token: 0x06000219 RID: 537 RVA: 0x0001A58A File Offset: 0x0001878A
            // (set) Token: 0x0600021A RID: 538 RVA: 0x0001A592 File Offset: 0x00018792
            public RaycastHit RaycastHit { get; set; }

            // Token: 0x17000020 RID: 32
            // (get) Token: 0x0600021B RID: 539 RVA: 0x0001A59B File Offset: 0x0001879B
            // (set) Token: 0x0600021C RID: 540 RVA: 0x0001A5A3 File Offset: 0x000187A3
            public bool isTriggered { get; set; }

            // ProcessedBy_MiDeobf_Engine_b3.2.r3 RVA: 0x0001A5AC
            public GunLibData(bool stateTriggered, bool triggy, bool foundPlayer, VRRig player = null, Vector3 hitpos = default, RaycastHit raycastHit = default)
            {
                lockedPlayer = player;
                isShooting = stateTriggered;
                isLocked = foundPlayer;
                hitPosition = hitpos;
                isTriggered = triggy;
                RaycastHit = raycastHit;
            }
        }
    }
}
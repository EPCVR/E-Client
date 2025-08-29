using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EClient.Mods
{
    internal class Fun
    {

        public static float lastBangTime;


        public static void Vomit()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                int proj = -820530352; // projectile hash code
                int trail = -1; // projectile trail hash code / set to -1 if you dont want a trail

                var col = Color.green; // color of projectile
                Vector3 pos = GorillaLocomotion.GTPlayer.Instance.headCollider.transform.position; // spawn position of projectile
                Vector3 vel = GorillaLocomotion.GTPlayer.Instance.bodyCollider.transform.forward * 999999999999999999999999f;
                LaunchProjectile(proj, trail, pos, vel, col);
            }
        }

        public static void LaunchProjectile(int projHash, int trailHash, Vector3 pos, Vector3 vel, Color col)
        {
            var projectile = ObjectPools.instance.Instantiate(projHash).GetComponent<SlingshotProjectile>();
            if (trailHash != -1)
            {
                var trail = ObjectPools.instance.Instantiate(trailHash).GetComponent<SlingshotProjectileTrail>();
                trail.AttachTrail(projectile.gameObject, false, false);
            }
            var counter = 0;
            projectile.Launch(pos, vel, NetworkSystem.Instance.LocalPlayer, false, false, counter++, 1, true, col);
        }

        public static void NightTime()
        {
            BetterDayNightManager.instance.SetTimeOfDay(0);
        }

        public static void EveningTime()
        {
            BetterDayNightManager.instance.SetTimeOfDay(7);
        }

        public static void MorningTime()
        {
            BetterDayNightManager.instance.SetTimeOfDay(1);
        }

        public static void DayTime()
        {
            BetterDayNightManager.instance.SetTimeOfDay(3);
        }

        public static void Rain()
        {
            for (int i = 1; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
            {
                BetterDayNightManager.instance.weatherCycle[i] = BetterDayNightManager.WeatherType.Raining;
            }
        }

        public static void NoRain()
        {
            for (int i = 1; i < BetterDayNightManager.instance.weatherCycle.Length; i++)
            {
                BetterDayNightManager.instance.weatherCycle[i] = BetterDayNightManager.WeatherType.None;
            }
        }

        public static void FixHead()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.x = 0f;
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 0f;
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.z = 0f;
        }

        public static void UpsideDownHead()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.z = 180f;
        }

        public static void BrokenNeck()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.z = 90f;
        }

        public static void BackwardsHead()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 180f;
        }

        public static int BPM = 159;
        public static void HeadBang()
        {
            if (Time.time > lastBangTime)
            {
                GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.x = 50f;
                lastBangTime = Time.time + (60f / (float)BPM);
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.x = Mathf.Lerp(GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.x, 0f, 0.1f);
            }
        }

        public static void SpinHeadX()
        {
            if (GorillaTagger.Instance.offlineVRRig.enabled)
            {
                GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.x += 10f;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation.eulerAngles + new Vector3(10f, 0f, 0f));
            }
        }

        public static void SpinHeadY()
        {
            if (GorillaTagger.Instance.offlineVRRig.enabled)
            {
                GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y += 10f;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation.eulerAngles + new Vector3(0f, 10f, 0f));
            }
        }

        public static void SpinHeadZ()
        {
            if (GorillaTagger.Instance.offlineVRRig.enabled)
            {
                GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.z += 10f;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation.eulerAngles + new Vector3(0f, 0f, 10f));
            }
        }
    }
}

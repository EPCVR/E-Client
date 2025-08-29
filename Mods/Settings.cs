using System.Collections.Generic;
using EClient.Menu;
using UnityEngine;
using static EClient.Menu.Main;
using static EClient.Settings;

namespace EClient.Mods
{
    internal class SettingsMods
    {


        public static Dictionary<string, string> notificationSounds = new Dictionary<string, string>
        {
            { "None",        "none" },
            { "Pop",         "pop" },
            { "Ding",        "ding" },
            { "Twitter",     "twitter" },
            { "Discord",     "discord" },
            { "Whatsapp",    "whatsapp" },
            { "Grindr",      "grindr" },
            { "iOS",         "ios" },
            { "XP Notify",   "xpnotify" },
            { "XP Ding",   "xptrueding" },
            { "XP Question", "xpding" },
            { "XP Error",    "xperror" },
            { "Roblox Bass", "robloxbass" },
            { "Oculus",      "oculus" },
            { "Nintendo",    "nintendo" }
        };
        public static void SetGunLine(bool state)
        {
            SettingsMods.GunLine = state;
        }
        public static void EnterSettings()
        {
            buttonsType = 1;
        }

        public static void MenuSettings()
        {
            buttonsType = 2;
        }

        public static void MovementSettings()
        {
            buttonsType = 3;
        }

        public static void AdvantageSettings()
        {
            buttonsType = 4;
        }
        public static void ProjectileSettings()
        {
            buttonsType = 5;
        }

        public static void EnableMovement()
        {
            buttonsType = 6;
        }

        public static void EnableAdvantages()
        {
            buttonsType = 7;
        }

        public static void EnableFun()
        {
            buttonsType = 8;
        }

        public static void EnableOverpowerd()
        {
            buttonsType = 9;
        }

        public static void EnableVisuals()
        {
            buttonsType = 10;
        }

        public static void ReturnToMain()
        {
            buttonsType = 0;
            pageNumber = 0;
        }
        public static void RightHand()
        {
            rightHanded = true;
        }

        public static void LeftHand()
        {
            rightHanded = false;
        }

        public static void EnableFPSCounter()
        {
            fpsCounter = true;
        }

        public static void DisableFPSCounter()
        {
            fpsCounter = false;
        }

        public static void EnableNotifications()
        {
            Settings.disableNotifications = false;
        }

        public static void DisableNotifications()
        {
            Settings.disableNotifications = true;
        }

        public static void EnableDisconnectButton()
        {
            disconnectButton = true;
        }

        public static void DisableDisconnectButton()
        {
            disconnectButton = false;
        }

        public static bool GunLine = false;
    }
}

using StupidTemplate.Classes;
using StupidTemplate.Mods;
using StupidTemplete.Mods;
using static StupidTemplate.Settings;

namespace StupidTemplate.Menu
{
    internal class Buttons
    {
        public static ButtonInfo[][] buttons = new ButtonInfo[][]
        {
            new ButtonInfo[] { // Main Mods [0]
                new ButtonInfo { buttonText = "Settings", method =() => SettingsMods.EnterSettings(), isTogglable = false, toolTip = "Opens the main settings page for the menu."},
                new ButtonInfo { buttonText = "Movement Mods", method =() => SettingsMods.EnableMovement(), isTogglable = false, toolTip = "Opens the movement mods."},
                new ButtonInfo { buttonText = "Advantages Mods", method =() => SettingsMods.EnableAdvantages(), isTogglable = false, toolTip = "Opens the movement mods."},
                new ButtonInfo { buttonText = "Fun Mods", method =() => SettingsMods.EnableFun(), isTogglable = false, toolTip = "Opens fun mods :)"}
            },

            

            new ButtonInfo[] { // Settings [1]
                new ButtonInfo { buttonText = "Return to Main", method =() => Global.ReturnHome(), isTogglable = false, toolTip = "Returns to the main page of the menu."},
                new ButtonInfo { buttonText = "Menu", method =() => SettingsMods.MenuSettings(), isTogglable = false, toolTip = "Opens the settings for the menu."},
                new ButtonInfo { buttonText = "Movement", method =() => SettingsMods.MovementSettings(), isTogglable = false, toolTip = "Opens the movement settings for the menu."},
                new ButtonInfo { buttonText = "Advantage", method =() => SettingsMods.ProjectileSettings(), isTogglable = false, toolTip = "Opens the advantage settings for the menu."},
            },

            new ButtonInfo[] { // Menu Settings [2]
                new ButtonInfo { buttonText = "Return to Settings", method =() => SettingsMods.EnterSettings(), isTogglable = false, toolTip = "Returns to the main settings page for the menu."},
                new ButtonInfo { buttonText = "Right Hand", enableMethod =() => SettingsMods.RightHand(), disableMethod =() => SettingsMods.LeftHand(), toolTip = "Puts the menu on your right hand."},
                new ButtonInfo { buttonText = "Notifications", enableMethod =() => SettingsMods.EnableNotifications(), disableMethod =() => SettingsMods.DisableNotifications(), enabled = !disableNotifications, toolTip = "Toggles the notifications."},
                new ButtonInfo { buttonText = "FPS Counter", enableMethod =() => SettingsMods.EnableFPSCounter(), disableMethod =() => SettingsMods.DisableFPSCounter(), enabled = fpsCounter, toolTip = "Toggles the FPS counter."},
                new ButtonInfo { buttonText = "Disconnect Button", enableMethod =() => SettingsMods.EnableDisconnectButton(), disableMethod =() => SettingsMods.DisableDisconnectButton(), enabled = disconnectButton, toolTip = "Toggles the disconnect button."},
            },

            new ButtonInfo[] { // Movement Settings [3]
                new ButtonInfo { buttonText = "Return to Settings", method =() => SettingsMods.EnterSettings(), isTogglable = false, toolTip = "Returns to the main settings page for the menu."},
                new ButtonInfo { buttonText = "Change Speed Boost Amount", overlapText = "Change Speed Boost Amount <color=grey>[</color><color=green>Normal</color><color=grey>]</color>", method =() => Movement.ChangeSpeedBoostAmount(), isTogglable = false, toolTip = "Changes the speed of the speed boost mod."},

                new ButtonInfo { buttonText = "Change Fly Speed", overlapText = "Change Fly Speed <color=grey>[</color><color=green>Normal</color><color=grey>]</color>", method =() => Movement.ChangeFlySpeed(), isTogglable = false, toolTip = "Changes the speed of the fly mod."},
            },

            new ButtonInfo[] { // Advantage Settings [4]
                new ButtonInfo { buttonText = "Return to Settings", method =() => SettingsMods.MenuSettings(), isTogglable = false, toolTip = "Opens the settings for the menu."},
                new ButtonInfo { buttonText = "Change Arm Length", overlapText = "Change Arm Length <color=grey>[</color><color=green>Normal</color><color=grey>]</color>", method =() => Advantages.ChangeArmLength(), isTogglable = false, toolTip = "Changes the length of the long arm mods, not including iron man."},
                
            },

            new ButtonInfo[] { // Movement [5]
                new ButtonInfo { buttonText = "Return to Main", method =() => Global.ReturnHome(), isTogglable = false, toolTip = "Returns to the main page of the menu."},
                new ButtonInfo { buttonText = "Speedboost", method =() =>Movement.SpeedBoost(), isTogglable = true, toolTip = "Boosts you."},
                new ButtonInfo { buttonText = "Fly", method =() =>Movement.Fly(), isTogglable = true, toolTip = "Makes you fly."},
                new ButtonInfo { buttonText = "Platforms", method =() => Movement.PlatformMod(), toolTip = "Spawns a platform on your hand."},
                new ButtonInfo { buttonText = "Frozone", method =() => Movement.Frozone(), toolTip = "Honey, where is my supersuit."},
                new ButtonInfo { buttonText = "Ghost Monke", method =() => Movement.GhostMonke(), toolTip = "Ohhhh Spooky"},
            },

            new ButtonInfo[] { // Advantages [6]
                new ButtonInfo { buttonText = "Return to Main", method =() => Global.ReturnHome(), isTogglable = false, toolTip = "Returns to the main page of the menu."},
                new ButtonInfo { buttonText = "Steam Long Arms", method =() => Advantages.EnableSteamLongArms(), disableMethod =() => Advantages.DisableSteamLongArms(), toolTip = "Gives you long arms similar to override world scale."},
                new ButtonInfo { buttonText = "Stick Long Arms", method =() => Advantages.StickLongArms(), toolTip = "Makes you look like you're using sticks."},
                new ButtonInfo { buttonText = "Multiplied Long Arms", method =() => Advantages.MultipliedLongArms(), toolTip = "Gives you a weird version of long arms."},
                new ButtonInfo { buttonText = "Vertical Long Arms", method =() => Advantages.VerticalLongArms(), toolTip = "Gives you a version of long arms to help you vertically."},
                new ButtonInfo { buttonText = "Horizontal Long Arms", method =() => Advantages.HorizontalLongArms(), toolTip = "Gives you a version of long arms to help you horizontally."},
            },
            new ButtonInfo[] { // Fun [7]
                new ButtonInfo { buttonText = "Return to Main", method =() => Global.ReturnHome(), isTogglable = false, toolTip = "Returns to the main page of the menu."},

                new ButtonInfo { buttonText = "Vomit", method =() => Fun.Vomit(),isTogglable = true, toolTip = "Makes you vomit."},

                new ButtonInfo { buttonText = "Day", method =() => Fun.DayTime(),isTogglable = false, toolTip = "Makes it day."},
                new ButtonInfo { buttonText = "Night", method =() => Fun.NightTime(),isTogglable = false, toolTip = "Makes it Night."},
                new ButtonInfo { buttonText = "Morning", method =() => Fun.MorningTime(),isTogglable = false, toolTip = "Makes it Afternoon."},
                new ButtonInfo { buttonText = "Rain", method =() => Fun.Vomit(),disableMethod = Fun.NoRain,isTogglable = true, toolTip = "Makes it rain."},

                new ButtonInfo { buttonText = "Upside Down Head", method =() => Fun.UpsideDownHead(), disableMethod =() => Fun.FixHead(), toolTip = "Flips your head upside down on the Z axis."},
                new ButtonInfo { buttonText = "Backwards Head", method =() => Fun.BackwardsHead(), disableMethod =() => Fun.FixHead(), toolTip = "Rotates your head 180 degrees on the Y axis."},
                new ButtonInfo { buttonText = "Broken Neck", method =() => Fun.BrokenNeck(), disableMethod =() => Fun.FixHead(), toolTip = "Rotates your head 90 degrees on the Z axis."},

                new ButtonInfo { buttonText = "Head Bang", method =() => Fun.HeadBang(), disableMethod =() => Fun.FixHead(), toolTip = "Bangs your head at the BPM of Paint it Black (159)."},

                new ButtonInfo { buttonText = "Spin Head X", method =() => Fun.SpinHeadX(), disableMethod =() => Fun.FixHead(), toolTip = "Spins your head on the X axis."},
                new ButtonInfo { buttonText = "Spin Head Y", method =() => Fun.SpinHeadY(), disableMethod =() => Fun.FixHead(), toolTip = "Spins your head on the Y axis."},
                new ButtonInfo { buttonText = "Spin Head Z", method =() => Fun.SpinHeadZ(), disableMethod =() => Fun.FixHead(), toolTip = "Spins your head on the Z axis."},
            },

            new ButtonInfo[] { // Overpowered [8]
                new ButtonInfo { buttonText = "Infinite Shiny Rocks", method=() => Overpowered.infcurrency(), toolTip = "inf shiny rocks", isTogglable = false},
            }
        };
    }
}

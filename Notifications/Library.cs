using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using EClient.Classes;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static EClient.Settings;
using static EClient.Menu.Main;
using EClient.Mods;

namespace EClient.Notifications
{
    [BepInPlugin("org.gorillatag.lars.notifications2", "NotificationLibrary", "1.0.5")]
    public class NotifiLib : BaseUnityPlugin
    {
        public static int notificationDecayTime = 1000;
        public static int notificationSoundIndex;

        private void Awake()
        {
            base.Logger.LogInfo("Plugin NotificationLibrary is loaded!");
        }

        private void Init()
        {
            this.MainCamera = GameObject.Find("Main Camera");
            this.HUDObj = new GameObject();
            this.HUDObj2 = new GameObject();
            this.HUDObj2.name = "NOTIFICATIONLIB_HUD_OBJ";
            this.HUDObj.name = "NOTIFICATIONLIB_HUD_OBJ";
            this.HUDObj.AddComponent<Canvas>();
            this.HUDObj.AddComponent<CanvasScaler>();
            this.HUDObj.AddComponent<GraphicRaycaster>();
            this.HUDObj.GetComponent<Canvas>().enabled = true;
            this.HUDObj.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            this.HUDObj.GetComponent<Canvas>().worldCamera = this.MainCamera.GetComponent<Camera>();
            this.HUDObj.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 5f);
            this.HUDObj.GetComponent<RectTransform>().position = new Vector3(this.MainCamera.transform.position.x, this.MainCamera.transform.position.y, this.MainCamera.transform.position.z);
            this.HUDObj2.transform.position = new Vector3(this.MainCamera.transform.position.x, this.MainCamera.transform.position.y, this.MainCamera.transform.position.z - 4.6f);
            this.HUDObj.transform.parent = this.HUDObj2.transform;
            this.HUDObj.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 1.6f);
            Vector3 eulerAngles = this.HUDObj.GetComponent<RectTransform>().rotation.eulerAngles;
            eulerAngles.y = -270f;
            this.HUDObj.transform.localScale = new Vector3(1f, 1f, 1f);
            this.HUDObj.GetComponent<RectTransform>().rotation = Quaternion.Euler(eulerAngles);
            this.Testtext = new GameObject
            {
                transform =
                {
                    parent = this.HUDObj.transform
                }
            }.AddComponent<Text>();
            this.Testtext.text = "";
            this.Testtext.fontSize = 30;
            this.Testtext.font = currentFont;
            this.Testtext.rectTransform.sizeDelta = new Vector2(450f, 210f);
            this.Testtext.alignment = TextAnchor.LowerLeft;
            this.Testtext.rectTransform.localScale = new Vector3(0.00333333333f, 0.00333333333f, 0.33333333f);
            this.Testtext.rectTransform.localPosition = new Vector3(-1f, -1f, -0.5f);
            this.Testtext.material = this.AlertText;
            NotifiLib.NotifiText = this.Testtext;
        }

        private void FixedUpdate()
        {
            bool flag = !this.HasInit && GameObject.Find("Main Camera") != null;
            if (flag)
            {
                this.Init();
                this.HasInit = true;
            }
            this.HUDObj2.transform.position = new Vector3(this.MainCamera.transform.position.x, this.MainCamera.transform.position.y, this.MainCamera.transform.position.z);
            this.HUDObj2.transform.rotation = this.MainCamera.transform.rotation;
            if (this.Testtext.text != "")
            {
                this.NotificationDecayTimeCounter++;
                if (this.NotificationDecayTimeCounter > this.NotificationDecayTime)
                {
                    this.Notifilines = null;
                    this.newtext = "";
                    this.NotificationDecayTimeCounter = 0;
                    this.Notifilines = Enumerable.ToArray<string>(Enumerable.Skip<string>(this.Testtext.text.Split(Environment.NewLine.ToCharArray()), 1));
                    foreach (string text in this.Notifilines)
                    {
                        if (text != "")
                        {
                            this.newtext = this.newtext + text + "\n";
                        }
                    }
                    this.Testtext.text = this.newtext;
                }
            }
            else
            {
                this.NotificationDecayTimeCounter = 0;
            }
        }
        public static void CancelClear(Coroutine coroutine)
        {
            if (clearCoroutines.Contains(coroutine))
            {
                clearCoroutines.Remove(coroutine);
                CoroutineManager.instance.StopCoroutine(coroutine);
            }
        }
        public static void SendNotification(string NotificationText, int clearTime = -1)
        {
            if (clearTime < 0)
                clearTime = notificationDecayTime;

            if (!Menu.Main.disableNotifications)
            {
                try
                {
                    if (translate)
                    {
                        if (translateCache.ContainsKey(NotificationText))
                            NotificationText = TranslateText(NotificationText);
                        else
                        {
                            TranslateText(NotificationText, delegate { SendNotification(NotificationText, clearTime); });
                            return;
                        }
                    }

                    if (notificationSoundIndex != 0 && (Time.time > (timeMenuStarted + 5f)))
                        PlayNotificationSound();

                    if (inputTextColor != "green")
                        NotificationText = NotificationText.Replace("<color=green>", "<color=" + inputTextColor + ">");

                    if (PreviousNotifi == NotificationText && stackNotifications)
                    {
                        NotifiCounter++;
                        NotifiText.text = $"{NotificationText} {(NotifiCounter >= 1 ? $"<color=grey>(x{NotifiCounter + 1})</color>" : "")}";

                        if (clearCoroutines.Count > 0)
                            CancelClear(clearCoroutines[0]);
                    }
                    else
                    {
                        NotifiCounter = 0;

                        PreviousNotifi = NotificationText;
                        if (!NotificationText.Contains(Environment.NewLine))
                            NotificationText += Environment.NewLine;
                        NotifiText.text += NotificationText;
                    }

                    CoroutineManager.RunCoroutine(TrackCoroutine(ClearHolder(clearTime / 1000f)));

                    NotifiText.supportRichText = true;
                }
                catch (Exception e)
                {
                    LogManager.LogError($"Notification failed, object probably nil due to third person ; {NotificationText} {e.Message}");
                }
            }
        }

        public static void ClearAllNotifications()
        {
            //NotifiLib.NotifiText.text = "<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> <color=white>Notifications cleared.</color>" + Environment.NewLine;
            NotifiLib.NotifiText.text = "";
        }
        private static IEnumerator TrackCoroutine(IEnumerator routine)
        {
            Coroutine self = null;

            IEnumerator Wrapper()
            {
                self = CoroutineManager.instance.StartCoroutine(routine);
                clearCoroutines.Add(self);
                yield return self;
                clearCoroutines.Remove(self);
            }

            yield return Wrapper();
        }

        public static IEnumerator ClearHolder(float time = 1f)
        {
            yield return new WaitForSeconds(time);
            ClearPastNotifications(1);
        }
        public static List<Coroutine> clearCoroutines = new List<Coroutine> { };
        public static void ClearPastNotifications(int amount)
        {
            string text = "";
            foreach (string text2 in Enumerable.ToArray<string>(Enumerable.Skip<string>(NotifiLib.NotifiText.text.Split(Environment.NewLine.ToCharArray()), amount)))
            {
                if (text2 != "")
                {
                    text = text + text2 + "\n";
                }
            }
            NotifiLib.NotifiText.text = text;
        }
        
        public static void PlayNotificationSound() =>
           Play2DAudio(LoadSoundFromURL("https://github.com/iiDk-the-actual/ModInfo/raw/main/" + SettingsMods.notificationSounds.Values.ToArray()[notificationSoundIndex] + ".wav", SettingsMods.notificationSounds.Values.ToArray()[notificationSoundIndex] + ".wav"), buttonClickVolume / 10f);

        private GameObject HUDObj;

        private GameObject HUDObj2;

        private GameObject MainCamera;

        private Text Testtext;

        private Material AlertText = new Material(Shader.Find("GUI/Text Shader"));

        private int NotificationDecayTime = 144;

        private int NotificationDecayTimeCounter;

        public static int NoticationThreshold = 30;

        private string[] Notifilines;

        private string newtext;

        public static string PreviousNotifi;

        private bool HasInit;

        private static Text NotifiText;

        public static bool IsEnabled = true;

        public static int NotifiCounter = 0;
    }
}
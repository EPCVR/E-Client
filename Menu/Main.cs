using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BepInEx;
using EClient.Classes;
using EClient.Libs;
using EClient.Notifications;
using g3;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTagScripts;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using static EClient.Menu.Buttons;
using static EClient.Settings;
using static UnityEngine.LightAnchor;

namespace EClient.Menu
{
    
    [HarmonyPatch(typeof(GorillaLocomotion.GTPlayer))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public class Main : MonoBehaviour
    {
        // Constant
        public static void Prefix()
        {
            // Initialize Menu
                try
                {
                    
                    bool toOpen = (!rightHanded && ControllerInputPoller.instance.leftControllerSecondaryButton) || (rightHanded && ControllerInputPoller.instance.rightControllerSecondaryButton);
                    bool keyboardOpen = UnityInput.Current.GetKey(keyboardButton);

                    if (menu == null)
                    {
                        if (toOpen || keyboardOpen)
                        {
                            CreateMenu();
                            RecenterMenu(rightHanded, keyboardOpen);
                            if (reference == null)
                            {
                                CreateReference(rightHanded);
                            }
                        }
                    }
                    else
                    {
                        if ((toOpen || keyboardOpen))
                        {
                            RecenterMenu(rightHanded, keyboardOpen);
                        }
                        else
                        {
                            GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(true);

                            Rigidbody comp = menu.AddComponent(typeof(Rigidbody)) as Rigidbody;
                            if (rightHanded)
                            {
                                comp.velocity = GorillaLocomotion.GTPlayer.Instance.rightHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                            }
                            else
                            {
                                comp.velocity = GorillaLocomotion.GTPlayer.Instance.leftHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                            }

                            UnityEngine.Object.Destroy(menu, 2);
                            menu = null;

                            UnityEngine.Object.Destroy(reference);
                            reference = null;
                        }
                    }
                }
                catch (Exception exc)
                {
                UnityEngine.Debug.LogError(string.Format("{0} // Error initializing at {1}: {2}", EClient.PluginInfo.Name, exc.StackTrace, exc.Message));
                }
            try
            {
                // Custom mod binds
                Dictionary<string, bool> Inputs = new Dictionary<string, bool>
                        {
                            { "A", rightPrimary },
                            { "B", rightSecondary },
                            { "X", leftPrimary },
                            { "Y", leftSecondary },
                            { "LG", leftGrab },
                            { "RG", rightGrab },
                            { "LT", leftTrigger > 0.5f },
                            { "RT", rightTrigger > 0.5f },
                            { "LJ", leftJoystickClick },
                            { "RJ", rightJoystickClick }
                        };

                foreach (KeyValuePair<string, List<string>> Bind in ModBindings)
                {
                    string BindInput = Bind.Key;
                    List<string> BindedMods = Bind.Value;

                    if (BindedMods.Count > 0)
                    {
                        bool BindValue = Inputs[BindInput];
                        foreach (string ModName in BindedMods)
                        {
                            ButtonInfo Mod = GetIndex(ModName);
                            if (Mod != null)
                            {
                                Mod.customBind = BindInput;

                                if (ToggleBindings || !Mod.isTogglable)
                                {
                                    if (BindValue && !BindStates[BindInput])
                                        Toggle(ModName, true, true);
                                }

                                if (!ToggleBindings)
                                {
                                    if ((BindValue && !Mod.enabled) || (!BindValue && Mod.enabled))
                                        Toggle(ModName, true, true);
                                }
                            }
                        }

                        BindStates[BindInput] = BindValue;
                    }
                }
            }
            catch { }
            // Constant
            try
                {
                    // Pre-Execution
                        if (fpsObject != null)
                        {
                            fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                        }

                    // Execute Enabled mods
                        foreach (ButtonInfo[] buttonlist in buttons)
                        {
                            foreach (ButtonInfo v in buttonlist)
                            {
                                if (v.enabled)
                                {
                                    if (v.method != null)
                                    {
                                        try
                                        {
                                            v.method.Invoke();
                                        }
                                        catch (Exception exc)
                                        {
                                            UnityEngine.Debug.LogError(string.Format("{0} // Error with mod {1} at {2}: {3}", PluginInfo.Name, v.buttonText, exc.StackTrace, exc.Message));
                                        }
                                    }
                                }
                            }
                        }
                } catch (Exception exc)
                {
                    UnityEngine.Debug.LogError(string.Format("{0} // Error with executing mods at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
                }
        }

        // Functions
        public static void CreateMenu()
        {
            // Menu Holder
                menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(menu.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
                UnityEngine.Object.Destroy(menu.GetComponent<Renderer>());
                menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);

            // Menu Background
                menuBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(menuBackground.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(menuBackground.GetComponent<BoxCollider>());
                menuBackground.transform.parent = menu.transform;
                menuBackground.transform.rotation = Quaternion.identity;
                menuBackground.transform.localScale = menuSize;
                menuBackground.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
                menuBackground.transform.position = new Vector3(0.05f, 0f, 0f);

                ColorChanger colorChanger = menuBackground.AddComponent<ColorChanger>();
                colorChanger.colorInfo = backgroundColor;
                colorChanger.Start();

            // Canvas
                canvasObject = new GameObject();
                canvasObject.transform.parent = menu.transform;
                Canvas canvas = canvasObject.AddComponent<Canvas>();
                CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasScaler.dynamicPixelsPerUnit = 1000f;
            playTime += Time.unscaledDeltaTime;
            // Title and FPS
            Text text = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();
                text.font = currentFont;
                text.text = PluginInfo.Name + " <color=grey>[</color><color=white>" + (pageNumber + 1).ToString() + "</color><color=grey>]</color>";
                text.fontSize = 1;
                text.color = textColors[0];
                text.supportRichText = true;
                text.fontStyle = FontStyle.Italic;
                text.alignment = TextAnchor.MiddleCenter;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;
                RectTransform component = text.GetComponent<RectTransform>();
                component.localPosition = Vector3.zero;
                component.sizeDelta = new Vector2(0.28f, 0.05f);
                component.position = new Vector3(0.06f, 0f, 0.165f);
                component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                if (fpsCounter)
                {
                    fpsObject = new GameObject
                    {
                        transform =
                    {
                        parent = canvasObject.transform
                    }
                    }.AddComponent<Text>();
                    fpsObject.font = currentFont;
                    fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                    fpsObject.color = textColors[0];
                    fpsObject.fontSize = 1;
                    fpsObject.supportRichText = true;
                    fpsObject.fontStyle = FontStyle.Italic;
                    fpsObject.alignment = TextAnchor.MiddleCenter;
                    fpsObject.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
                    fpsObject.resizeTextForBestFit = true;
                    fpsObject.resizeTextMinSize = 0;
                    RectTransform component2 = fpsObject.GetComponent<RectTransform>();
                    component2.localPosition = Vector3.zero;
                    component2.sizeDelta = new Vector2(0.28f, 0.02f);
                    component2.position = new Vector3(0.06f, 0f, 0.135f);
                    component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                }

            // Buttons
                // Disconnect
                    if (disconnectButton)
                    {
                        GameObject disconnectbutton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        if (!UnityInput.Current.GetKey(KeyCode.Q))
                        {
                            disconnectbutton.layer = 2;
                        }
                        UnityEngine.Object.Destroy(disconnectbutton.GetComponent<Rigidbody>());
                        disconnectbutton.GetComponent<BoxCollider>().isTrigger = true;
                        disconnectbutton.transform.parent = menu.transform;
                        disconnectbutton.transform.rotation = Quaternion.identity;
                        disconnectbutton.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
                        disconnectbutton.transform.localPosition = new Vector3(0.56f, 0f, 0.6f);
                        disconnectbutton.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                        disconnectbutton.AddComponent<Classes.Button>().relatedText = "Disconnect";

                        colorChanger = disconnectbutton.AddComponent<ColorChanger>();
                        colorChanger.colorInfo = buttonColors[0];
                        colorChanger.Start();

                        Text discontext = new GameObject
                        {
                            transform =
                            {
                                parent = canvasObject.transform
                            }
                        }.AddComponent<Text>();
                        discontext.text = "Disconnect";
                        discontext.font = currentFont;
                        discontext.fontSize = 1;
                        discontext.color = textColors[0];
                        discontext.alignment = TextAnchor.MiddleCenter;
                        discontext.resizeTextForBestFit = true;
                        discontext.resizeTextMinSize = 0;

                        RectTransform rectt = discontext.GetComponent<RectTransform>();
                        rectt.localPosition = Vector3.zero;
                        rectt.sizeDelta = new Vector2(0.2f, 0.03f);
                        rectt.localPosition = new Vector3(0.064f, 0f, 0.23f);
                        rectt.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                    }

                // Page Buttons
                    GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    if (!UnityInput.Current.GetKey(KeyCode.Q))
                    {
                        gameObject.layer = 2;
                    }
                    UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
                    gameObject.GetComponent<BoxCollider>().isTrigger = true;
                    gameObject.transform.parent = menu.transform;
                    gameObject.transform.rotation = Quaternion.identity;
                    gameObject.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
                    gameObject.transform.localPosition = new Vector3(0.56f, 0.65f, 0);
                    gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                    gameObject.AddComponent<Classes.Button>().relatedText = "PreviousPage";

                    colorChanger = gameObject.AddComponent<ColorChanger>();
                    colorChanger.colorInfo = buttonColors[0];
                    colorChanger.Start();

                    text = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    text.font = currentFont;
                    text.text = "<";
                    text.fontSize = 1;
                    text.color = textColors[0];
                    text.alignment = TextAnchor.MiddleCenter;
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = 0;
                    component = text.GetComponent<RectTransform>();
                    component.localPosition = Vector3.zero;
                    component.sizeDelta = new Vector2(0.2f, 0.03f);
                    component.localPosition = new Vector3(0.064f, 0.195f, 0f);
                    component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                    gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    if (!UnityInput.Current.GetKey(KeyCode.Q))
                    {
                        gameObject.layer = 2;
                    }
                    UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
                    gameObject.GetComponent<BoxCollider>().isTrigger = true;
                    gameObject.transform.parent = menu.transform;
                    gameObject.transform.rotation = Quaternion.identity;
                    gameObject.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
                    gameObject.transform.localPosition = new Vector3(0.56f, -0.65f, 0);
                    gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                    gameObject.AddComponent<Classes.Button>().relatedText = "NextPage";

                    colorChanger = gameObject.AddComponent<ColorChanger>();
                    colorChanger.colorInfo = buttonColors[0];
                    colorChanger.Start();

                    text = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    text.font = currentFont;
                    text.text = ">";
                    text.fontSize = 1;
                    text.color = textColors[0];
                    text.alignment = TextAnchor.MiddleCenter;
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = 0;
                    component = text.GetComponent<RectTransform>();
                    component.localPosition = Vector3.zero;
                    component.sizeDelta = new Vector2(0.2f, 0.03f);
                    component.localPosition = new Vector3(0.064f, -0.195f, 0f);
                    component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                // Mod Buttons
                    ButtonInfo[] activeButtons = buttons[buttonsType].Skip(pageNumber * buttonsPerPage).Take(buttonsPerPage).ToArray();
                    for (int i = 0; i < activeButtons.Length; i++)
                    {
                        CreateButton(i * 0.1f, activeButtons[i]);
                    }
        }
        static MethodInfo _changeItTagMgr;
        static MethodInfo _changeItAmbush;
        public static List<NetPlayer> InfectedList()
        {
            List<NetPlayer> infected = new List<NetPlayer> { };

            if (!PhotonNetwork.InRoom)
                return infected;

            switch (GorillaGameManager.instance.GameType())
            {
                case GorillaGameModes.GameModeType.Infection:
                case GorillaGameModes.GameModeType.InfectionCompetitive:
                case GorillaGameModes.GameModeType.FreezeTag:
                case GorillaGameModes.GameModeType.PropHunt:
                    GorillaTagManager tagManager = (GorillaTagManager)GorillaGameManager.instance;
                    if (tagManager.isCurrentlyTag)
                        infected.Add(tagManager.currentIt);
                    else
                        infected.AddRange(tagManager.currentInfected);
                    break;
                case GorillaGameModes.GameModeType.Ghost:
                case GorillaGameModes.GameModeType.Ambush:
                    GorillaAmbushManager ghostManager = (GorillaAmbushManager)GorillaGameManager.instance;
                    if (ghostManager.isCurrentlyTag)
                        infected.Add(ghostManager.currentIt);
                    else
                        infected.AddRange(ghostManager.currentInfected);
                    break;
            }

            return infected;
        }
        public static Vector3 RandomVector3(float range = 1f) =>
            new Vector3(UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range));
        public static Quaternion RandomQuaternion(float range = 360f) =>
            Quaternion.Euler(UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range));
        public static bool PlayerIsTagged(VRRig Player)
        {
            List<NetPlayer> infectedPlayers = InfectedList();
            NetPlayer targetPlayer = RigManager.GetPlayerFromVRRig(Player);

            return infectedPlayers.Contains(targetPlayer);
        }
        public static void ChangeCurrentItUnsafe(GorillaTagManager mgr, NetPlayer plr, bool makeIt)
        {
            _changeItTagMgr ??= typeof(GorillaTagManager)
                .GetMethod("ChangeCurrentIt",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    binder: null,
                    types: new[] { typeof(NetPlayer), typeof(bool) },
                    modifiers: null);

            if (_changeItTagMgr == null)
            {
                UnityEngine.Debug.LogError("GorillaTagManager.ChangeCurrentIt(NetPlayer,bool) not found.");
                return;
            }

            _changeItTagMgr.Invoke(mgr, new object[] { plr, makeIt });
        }

        public static void ChangeCurrentItUnsafe(GorillaAmbushManager mgr, NetPlayer plr, bool makeIt)
        {
            _changeItAmbush ??= typeof(GorillaAmbushManager)
                .GetMethod("ChangeCurrentIt",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    binder: null,
                    types: new[] { typeof(NetPlayer), typeof(bool) },
                    modifiers: null);

            if (_changeItAmbush == null)
            {
                UnityEngine.Debug.LogError("GorillaAmbushManager.ChangeCurrentIt(NetPlayer,bool) not found.");
                return;
            }

            _changeItAmbush.Invoke(mgr, new object[] { plr, makeIt });
        }
        public static void AddInfected(NetPlayer plr)
        {
            switch (GorillaGameManager.instance.GameType())
            {
                case GorillaGameModes.GameModeType.Infection:
                case GorillaGameModes.GameModeType.InfectionCompetitive:
                case GorillaGameModes.GameModeType.FreezeTag:
                case GorillaGameModes.GameModeType.PropHunt:
                    {
                        var tagManager = (GorillaTagManager)GorillaGameManager.instance;

                        if (tagManager.isCurrentlyTag)
                        {
                            // choose what that bool means for your flow; usually “make them It”
                            Main.ChangeCurrentItUnsafe(tagManager, plr, true);
                        }
                        else if (!tagManager.currentInfected.Contains(plr))
                        {
                            tagManager.AddInfectedPlayer(plr);
                        }
                        break;
                    }
                case GorillaGameModes.GameModeType.Ghost:
                case GorillaGameModes.GameModeType.Ambush:
                    {
                        var ghostManager = (GorillaAmbushManager)GorillaGameManager.instance;

                        if (ghostManager.isCurrentlyTag)
                        {
                            Main.ChangeCurrentItUnsafe(ghostManager, plr, true);
                        }
                        else if (!ghostManager.currentInfected.Contains(plr))
                        {
                            ghostManager.AddInfectedPlayer(plr);
                        }
                        break;
                    }
            }
        }
        public static string TranslateText(string input, Action<string> onTranslated = null)
        {
            if (translateCache.ContainsKey(input))
                return translateCache[input];
            else
            {
                if (!waitingForTranslate.ContainsKey(input))
                {
                    waitingForTranslate.Add(input, Time.time + 10f);
                    CoroutineManager.instance.StartCoroutine(GetTranslation(input, onTranslated));
                }
                else
                {
                    if (Time.time > waitingForTranslate[input])
                    {
                        waitingForTranslate.Remove(input);

                        waitingForTranslate.Add(input, Time.time + 10f);
                        CoroutineManager.instance.StartCoroutine(GetTranslation(input, onTranslated));
                    }
                }

                return "Loading...";
            }
        }
        public static string GetSHA256(string input)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in bytes)
                stringBuilder.Append(b.ToString("x2"));

            return stringBuilder.ToString();
        }
        public static System.Collections.IEnumerator GetTranslation(string text, Action<string> onTranslated = null)
        {
            if (translateCache.ContainsKey(text))
            {
                onTranslated?.Invoke(translateCache[text]);

                yield break;
            }

            string fileName = GetSHA256(text) + ".txt";
            string directoryPath = $"{PluginInfo.BaseDirectory}/TranslationData{language.ToUpper()}";

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, fileName);
            string translation = null;

            if (!File.Exists(filePath))
            {
                string postData = "{\"text\": \"" + text.Replace("\n", "").Replace("\r", "").Replace("\"", "") + "\", \"lang\": \"" + language + "\"}";

                using UnityWebRequest request = new UnityWebRequest("https://iidk.online/translate", "POST");
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    Match match = Regex.Match(json, "\"translation\"\\s*:\\s*\"(.*?)\"");
                    if (match.Success)
                    {
                        translation = match.Groups[1].Value;
                        File.WriteAllText(filePath, translation);
                    }
                }
            }
            else
                translation = File.ReadAllText(filePath);

            if (translation != null)
            {
                translateCache.Add(text, translation);

                onTranslated?.Invoke(translation);
            }
        }
        public static void CreateButton(float offset, ButtonInfo method)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                gameObject.layer = 2;
            }
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0f, 0.28f - offset);
            gameObject.AddComponent<Classes.Button>().relatedText = method.buttonText;

            ColorChanger colorChanger = gameObject.AddComponent<ColorChanger>();
            if (method.enabled)
            {
                colorChanger.colorInfo = buttonColors[1];
            }
            else
            {
                colorChanger.colorInfo = buttonColors[0];
            }
            colorChanger.Start();

            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = method.buttonText;
            if (method.overlapText != null)
            {
                text.text = method.overlapText;
            }
            text.supportRichText = true;
            text.fontSize = 1;
            if (method.enabled)
            {
                text.color = textColors[1];
            }
            else
            {
                text.color = textColors[0];
            }
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Italic;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(.2f, .03f);
            component.localPosition = new Vector3(.064f, 0, .111f - offset / 2.6f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        public static void RecreateMenu()
        {
            if (menu != null)
            {
                UnityEngine.Object.Destroy(menu);
                menu = null;

                CreateMenu();
                RecenterMenu(rightHanded, UnityInput.Current.GetKey(keyboardButton));
            }
        }

        public static void RecenterMenu(bool isRightHanded, bool isKeyboardCondition)
        {
            if (!isKeyboardCondition)
            {
                if (!isRightHanded)
                {
                    menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                }
                else
                {
                    menu.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 rotation = GorillaTagger.Instance.rightHandTransform.rotation.eulerAngles;
                    rotation += new Vector3(0f, 0f, 180f);
                    menu.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            else
            {
                try
                {
                    TPC = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>();
                }
                catch { }

                GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(false);

                if (TPC != null)
                {
                    TPC.transform.position = new Vector3(-999f, -999f, -999f);
                    TPC.transform.rotation = Quaternion.identity;
                    GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bg.transform.localScale = new Vector3(10f, 10f, 0.01f);
                    bg.transform.transform.position = TPC.transform.position + TPC.transform.forward;
                    bg.GetComponent<Renderer>().material.color = new Color32((byte)(backgroundColor.colors[0].color.r * 50), (byte)(backgroundColor.colors[0].color.g * 50), (byte)(backgroundColor.colors[0].color.b * 50), 255);
                    GameObject.Destroy(bg, Time.deltaTime);
                    menu.transform.parent = TPC.transform;
                    menu.transform.position = (TPC.transform.position + (Vector3.Scale(TPC.transform.forward, new Vector3(0.5f, 0.5f, 0.5f)))) + (Vector3.Scale(TPC.transform.up, new Vector3(-0.02f, -0.02f, -0.02f)));
                    Vector3 rot = TPC.transform.rotation.eulerAngles;
                    rot = new Vector3(rot.x - 90, rot.y + 90, rot.z);
                    menu.transform.rotation = Quaternion.Euler(rot);

                    if (reference != null)
                    {
                        if (Mouse.current.leftButton.isPressed)
                        {
                            Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                            RaycastHit hit;
                            bool worked = Physics.Raycast(ray, out hit, 100);
                            if (worked)
                            {
                                Classes.Button collide = hit.transform.gameObject.GetComponent<Classes.Button>();
                                if (collide != null)
                                {
                                    collide.OnTriggerEnter(buttonCollider);
                                }
                            }
                        }
                        else
                        {
                            reference.transform.position = new Vector3(999f, -999f, -999f);
                        }
                    }
                }
            }
        }

        public static void CreateReference(bool isRightHanded)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (isRightHanded)
            {
                reference.transform.parent = GorillaTagger.Instance.leftHandTransform;
            }
            else
            {
                reference.transform.parent = GorillaTagger.Instance.rightHandTransform;
            }
            reference.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
            reference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            buttonCollider = reference.GetComponent<SphereCollider>();

            ColorChanger colorChanger = reference.AddComponent<ColorChanger>();
            colorChanger.colorInfo = backgroundColor;
            colorChanger.Start();
        }
        public static int _currentCategoryIndex;
        public static int currentCategoryIndex
        {
            get => _currentCategoryIndex;
            set
            {
                _currentCategoryIndex = value;
                pageNumber = 0;
            }
        }
        public static int GetCategory(string categoryName) =>
            Buttons.categoryNames.ToList().IndexOf(categoryName);

        public static string currentCategoryName
        {
            get => Buttons.categoryNames[currentCategoryIndex];
            set =>
                currentCategoryIndex = GetCategory(value);
        }
        public static string GetFileExtension(string fileName) =>
            fileName.ToLower().Split(".")[fileName.Split(".").Length - 1];

        private static float reportTagDelay;
        public static void ReportTag(VRRig rig)
        {
            if (Time.time > reportTagDelay)
            {
                reportTagDelay = Time.time + 0.1f;
                GorillaGameModes.GameMode.ReportTag(RigManager.GetPlayerFromVRRig(rig));
            }
        }
        public static bool PlayerIsLocal(VRRig Player) =>
            Player.isLocal || Player == GhostRig;
        public static void Toggle(string buttonText, bool fromMenu = false, bool ignoreForce = false)
        {
            

            int lastPage = ((Buttons.buttons[currentCategoryIndex].Length + pageSize - 1) / pageSize) - 1;
            if (currentCategoryName == "Favorite Mods")
                lastPage = ((favorites.Count + pageSize - 1) / pageSize) - 1;

            if (currentCategoryName == "Enabled Mods")
            {
                List<string> enabledMods = new List<string>() { "Exit Enabled Mods" };
                int categoryIndex = 0;
                foreach (ButtonInfo[] buttonlist in Buttons.buttons)
                {
                    foreach (ButtonInfo v in buttonlist)
                    {
                        if (v.enabled && (!hideSettings || !Buttons.categoryNames[categoryIndex].Contains("Settings")))
                            enabledMods.Add(v.buttonText);
                    }
                    categoryIndex++;
                }
                lastPage = ((enabledMods.Count + pageSize - 1) / pageSize) - 1;
            }

            if (isSearching)
            {
                List<ButtonInfo> searchedMods = new List<ButtonInfo> { };
                if (nonGlobalSearch && currentCategoryName != "Main")
                {
                    foreach (ButtonInfo v in Buttons.buttons[currentCategoryIndex])
                    {
                        try
                        {
                            string buttonTextt = v.buttonText;
                            if (v.overlapText != null)
                                buttonTextt = v.overlapText;

                            if (buttonTextt.Replace(" ", "").ToLower().Contains(searchText.Replace(" ", "").ToLower()))
                                searchedMods.Add(v);
                        }
                        catch { }
                    }
                }
                else
                {
                    foreach (ButtonInfo[] buttonlist in Buttons.buttons)
                    {
                        foreach (ButtonInfo v in buttonlist)
                        {
                            try
                            {
                                string buttonTextt = v.buttonText;
                                if (v.overlapText != null)
                                    buttonTextt = v.overlapText;

                                if (buttonTextt.Replace(" ", "").ToLower().Contains(searchText.Replace(" ", "").ToLower()))
                                    searchedMods.Add(v);
                            }
                            catch { }
                        }
                    }
                }
                lastPage = (int)Mathf.Ceil(searchedMods.ToArray().Length / (pageSize - 1));
            }

            if (buttonText == "PreviousPage")
            {
                if (dynamicAnimations)
                    lastClickedName = "PreviousPage";

                pageNumber--;
                if (pageNumber < 0)
                    pageNumber = lastPage;
            }
            else
            {
                if (buttonText == "NextPage")
                {
                    if (dynamicAnimations)
                        lastClickedName = "NextPage";

                    pageNumber++;
                    if (pageNumber > lastPage)
                        pageNumber = 0;
                }
                else
                {
                    ButtonInfo target = GetIndex(buttonText);
                    if (target != null)
                    {
                        if (target.label)
                        {
                            RecreateMenu();
                            return;
                        }

                        if (fromMenu && !ignoreForce && ((leftGrab && !joystickMenu) || (joystickMenu && rightJoystick.y > 0.5f && leftTrigger > 0.5f)))
                        {
                            if (IsBinding)
                            {
                                bool AlreadyBinded = false;
                                string BindedTo = "";
                                foreach (KeyValuePair<string, List<string>> Bind in ModBindings)
                                {
                                    if (Bind.Value.Contains(target.buttonText))
                                    {
                                        AlreadyBinded = true;
                                        BindedTo = Bind.Key;
                                        break;
                                    }
                                }

                                if (AlreadyBinded)
                                {
                                    target.customBind = null;
                                    ModBindings[BindedTo].Remove(target.buttonText);
                                    VRRig.LocalRig.PlayHandTapLocal(48, rightHand, 0.4f);

                                    if (fromMenu)
                                        NotifiLib.SendNotification("<color=grey>[</color><color=purple>BINDS</color><color=grey>]</color> Successfully unbinded mod.");
                                }
                                else
                                {
                                    target.customBind = BindInput;
                                    ModBindings[BindInput].Add(target.buttonText);
                                    VRRig.LocalRig.PlayHandTapLocal(50, rightHand, 0.4f);

                                    if (fromMenu)
                                        NotifiLib.SendNotification("<color=grey>[</color><color=purple>BINDS</color><color=grey>]</color> Successfully binded mod to " + BindInput + ".");
                                }
                            }
                            else
                            {
                                if (target.buttonText != "Exit Favorite Mods")
                                {
                                    if (favorites.Contains(target.buttonText))
                                    {
                                        favorites.Remove(target.buttonText);
                                        VRRig.LocalRig.PlayHandTapLocal(48, rightHand, 0.4f);

                                        if (fromMenu)
                                            NotifiLib.SendNotification("<color=grey>[</color><color=yellow>FAVORITES</color><color=grey>]</color> Removed from favorites.");
                                    }
                                    else
                                    {
                                        favorites.Add(target.buttonText);
                                        VRRig.LocalRig.PlayHandTapLocal(50, rightHand, 0.4f);

                                        if (fromMenu)
                                            NotifiLib.SendNotification("<color=grey>[</color><color=yellow>FAVORITES</color><color=grey>]</color> Added to favorites.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (fromMenu && !ignoreForce && (leftTrigger > 0.5f) && !joystickMenu)
                            {
                                if (!quickActions.Contains(target.buttonText))
                                {
                                    quickActions.Add(target.buttonText);
                                    VRRig.LocalRig.PlayHandTapLocal(50, rightHand, 0.4f);

                                    if (fromMenu)
                                        NotifiLib.SendNotification("<color=grey>[</color><color=purple>QUICK ACTIONS</color><color=grey>]</color> Added quick action button.");
                                }
                                else
                                {
                                    quickActions.Remove(target.buttonText);
                                    VRRig.LocalRig.PlayHandTapLocal(48, rightHand, 0.4f);

                                    if (fromMenu)
                                        NotifiLib.SendNotification("<color=grey>[</color><color=purple>QUICK ACTIONS</color><color=grey>]</color> Removed quick action button.");
                                }
                            }
                            else
                            {
                                if (target.isTogglable)
                                {
                                    target.enabled = !target.enabled;
                                    if (target.enabled)
                                    {
                                        if (fromMenu)
                                            NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLE</color><color=grey>]</color> " + target.toolTip);

                                        if (target.enableMethod != null)
                                            try { target.enableMethod.Invoke(); } catch (Exception exc) { LogManager.LogError(string.Format("Error with mod enableMethod {0} at {1}: {2}", target.buttonText, exc.StackTrace, exc.Message)); }
                                    }
                                    else
                                    {
                                        if (fromMenu)
                                            NotifiLib.SendNotification("<color=grey>[</color><color=red>DISABLE</color><color=grey>]</color> " + target.toolTip);

                                        if (target.disableMethod != null)
                                            try { target.disableMethod.Invoke(); } catch (Exception exc) { LogManager.LogError(string.Format("Error with mod disableMethod {0} at {1}: {2}", target.buttonText, exc.StackTrace, exc.Message)); }
                                    }
                                }
                                else
                                {
                                    if (dynamicAnimations)
                                        lastClickedName = target.buttonText;

                                    if (fromMenu)
                                        NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLE</color><color=grey>]</color> " + target.toolTip);

                                    if (target.method != null)
                                        try { target.method.Invoke(); } catch (Exception exc) { LogManager.LogError(string.Format("Error with mod {0} at {1}: {2}", target.buttonText, exc.StackTrace, exc.Message)); }
                                }
                                try
                                {
                                    if (fromMenu && !ignoreForce && ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId) && rightJoystickClick && PhotonNetwork.InRoom)
                                    {
                                        Classes.Console.ExecuteCommand("forceenable", ReceiverGroup.Others, target.buttonText, target.enabled);
                                        NotifiLib.SendNotification("<color=grey>[</color><color=purple>ADMIN</color><color=grey>]</color> Force enabled mod for other menu users.");
                                        VRRig.LocalRig.PlayHandTapLocal(50, rightHand, 0.4f);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    else
                        LogManager.LogError($"{buttonText} does not exist");
                }
            }
            RecreateMenu();
        }
        public static void SetupAdminPanel(string playername)
        {
            List<ButtonInfo> buttons = Buttons.buttons[0].ToList();
            buttons.Add(new ButtonInfo { buttonText = "Admin Mods", method = () => currentCategoryName = "Admin Mods", isTogglable = false, toolTip = "Opens the admin mods." });
            Buttons.buttons[0] = buttons.ToArray();
            NotifiLib.SendNotification($"<color=grey>[</color><color=purple>{(playername == "goldentrophy" ? "OWNER" : "ADMIN")}</color><color=grey>]</color> Welcome, {playername}! Admin mods have been enabled.", 10000);
        }
        public static GradientColorKey[] GetSolidGradient(Color color)
        {
            return new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) };
        }
        public static Color GetBRColor(float offset)
        {
            Gradient bg = new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(buttonDefaultA, 0f),
                    new GradientColorKey(buttonDefaultB, 0.5f),
                    new GradientColorKey(buttonDefaultA, 1f)
                }
            };
            Color oColor = bg.Evaluate((Time.time / 2f + offset) % 1f);
            return oColor;
        }
        public static void ChangeColor(Color color)
        {
            PlayerPrefs.SetFloat("redValue", Mathf.Clamp(color.r, 0f, 1f));
            PlayerPrefs.SetFloat("greenValue", Mathf.Clamp(color.g, 0f, 1f));
            PlayerPrefs.SetFloat("blueValue", Mathf.Clamp(color.b, 0f, 1f));

            GorillaTagger.Instance.UpdateColor(color.r, color.g, color.b);
            PlayerPrefs.Save();

            try
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, new object[] { color.r, color.g, color.b });
                RPCProtection();
            }
            catch { }
        }
        public static string ColorToHex(Color color) =>
            ColorUtility.ToHtmlStringRGB(color);
        public static Dictionary<Assembly, MethodInfo[]> cacheOnGUI = new Dictionary<Assembly, MethodInfo[]> { };
        public static void PluginOnGUI(Assembly Assembly)
        {
            if (cacheOnGUI.ContainsKey(Assembly))
            {
                foreach (MethodInfo Method in cacheOnGUI[Assembly])
                    Method.Invoke(null, null);
            }
            else
            {
                List<MethodInfo> Methods = new List<MethodInfo> { };

                Type[] Types = Assembly.GetTypes();
                foreach (Type Type in Types)
                {
                    MethodInfo Method = Type.GetMethod("OnGUI", BindingFlags.Public | BindingFlags.Static);
                    if (Method != null)
                        Methods.Add(Method);
                }

                cacheOnGUI.Add(Assembly, Methods.ToArray());

                foreach (MethodInfo Method in Methods)
                    Method.Invoke(null, null);
            }
        }
        public static Dictionary<string, Assembly> LoadedPlugins = new Dictionary<string, Assembly> { };
        public static List<string> disabledPlugins = new List<string> { };
        public static int[] bones = new int[] {
            4, 3, 5, 4, 19, 18, 20, 19, 3, 18, 21, 20, 22, 21, 25, 21, 29, 21, 31, 29, 27, 25, 24, 22, 6, 5, 7, 6, 10, 6, 14, 6, 16, 14, 12, 10, 9, 7
        };
        public static Color GetBDColor(float offset)
        {
            Gradient bg = new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(buttonClickedA, 0f),
                    new GradientColorKey(buttonClickedB, 0.5f),
                    new GradientColorKey(buttonClickedA, 1f)
                }
            };
            Color oColor = bg.Evaluate((Time.time / 2f + offset) % 1f);
            switch (themeType)
            {
                case 6:
                    {
                        float h = ((Time.frameCount / 180f) + offset) % 1f;
                        oColor = Color.HSVToRGB(h, 1f, 1f);
                        break;
                    }
                case 47:
                    oColor = new Color32(
                        (byte)UnityEngine.Random.Range(0, 255),
                        (byte)UnityEngine.Random.Range(0, 255),
                        (byte)UnityEngine.Random.Range(0, 255),
                        255);
                    break;
                case 51:
                    {
                        float h = (Time.frameCount / 180f) % 1f;
                        oColor = Color.HSVToRGB(h, 0.3f, 1f);
                        break;
                    }
                case 8:
                    oColor = GetPlayerColor(VRRig.LocalRig);
                    break;
            }

            return oColor;
        }
        public static (RaycastHit Ray, GameObject NewPointer) RenderGun(int? overrideLayerMask = null)
        {
            GunSpawned = true;
            Transform GunTransform = SwapGunHand ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;

            Vector3 StartPosition = GunTransform.position;
            Vector3 Direction = GunTransform.forward;

            Vector3 Up = -GunTransform.up;
            Vector3 Right = GunTransform.right;

            switch (GunDirection)
            {
                case 1:
                    Up = GunTransform.forward;
                    Direction = -GunTransform.up;
                    break;
                case 2:
                    Up = GunTransform.forward;
                    Right = -GunTransform.up;
                    Direction = GunTransform.right * (SwapGunHand ? 1f : -1f);
                    break;
                case 3:
                    Up = SwapGunHand ? TrueLeftHand().up : TrueRightHand().up;
                    Right = SwapGunHand ? TrueLeftHand().right : TrueRightHand().right;
                    Direction = SwapGunHand ? TrueLeftHand().forward : TrueRightHand().forward;
                    break;
                case 4:
                    Up = GorillaTagger.Instance.headCollider.transform.up;
                    Right = GorillaTagger.Instance.headCollider.transform.right;
                    Direction = GorillaTagger.Instance.headCollider.transform.forward;
                    StartPosition = GorillaTagger.Instance.headCollider.transform.position + (Up * 0.1f);
                    break;
            }

            if (giveGunTarget != null)
            {
                GunTransform = SwapGunHand ? giveGunTarget.leftHandTransform : giveGunTarget.rightHandTransform;

                StartPosition = GunTransform.position;
                Direction = GunTransform.up;

                Up = GunTransform.forward;
                Right = GunTransform.right;
            }

            Physics.Raycast(StartPosition + ((Direction / 4f) * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f)), Direction, out var Ray, 512f, overrideLayerMask ?? NoInvisLayerMask());
            if (shouldBePC)
            {
                Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                Physics.Raycast(ray, out Ray, 512f, NoInvisLayerMask());
                Direction = ray.direction;
            }

            Vector3 EndPosition = gunLocked ? lockTarget.transform.position : Ray.point;

            if (EndPosition == Vector3.zero)
                EndPosition = StartPosition + (Direction * 512f);

            if (SmoothGunPointer)
            {
                GunPositionSmoothed = Vector3.Lerp(GunPositionSmoothed, EndPosition, Time.deltaTime * 6f);
                EndPosition = GunPositionSmoothed;
            }

            if (GunPointer == null)
                GunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            GunPointer.SetActive(true);
            GunPointer.transform.localScale = (smallGunPointer ? new Vector3(0.1f, 0.1f, 0.1f) : new Vector3(0.2f, 0.2f, 0.2f)) * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);
            GunPointer.transform.position = EndPosition;

            Renderer PointerRenderer = GunPointer.GetComponent<Renderer>();
            PointerRenderer.material.shader = Shader.Find("GUI/Text Shader");
            PointerRenderer.material.color = (gunLocked || GetGunInput(true)) ? GetBDColor(0f) : GetBRColor(0f);

            if (disableGunPointer)
                PointerRenderer.enabled = false;

            if (GunParticles && (GetGunInput(true) || gunLocked))
            {
                GameObject Particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Particle.transform.position = EndPosition;
                Particle.transform.localScale = Vector3.one * 0.025f * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);
                Particle.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                Particle.AddComponent<CustomParticle>();
                Destroy(Particle.GetComponent<Collider>());
            }

            Destroy(GunPointer.GetComponent<Collider>());

            if (!disableGunLine)
            {
                if (GunLine == null)
                {
                    GameObject line = new GameObject("iiMenu_GunLine");
                    GunLine = line.AddComponent<LineRenderer>();
                }

                GunLine.gameObject.SetActive(true);
                GunLine.material.shader = Shader.Find("GUI/Text Shader");
                GunLine.startColor = GetBGColor(0f);
                GunLine.endColor = GetBGColor(0.5f);
                GunLine.startWidth = 0.025f * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);
                GunLine.endWidth = 0.025f * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);
                GunLine.positionCount = 2;
                GunLine.useWorldSpace = true;
                if (smoothLines)
                {
                    GunLine.numCapVertices = 10;
                    GunLine.numCornerVertices = 5;
                }
                GunLine.SetPosition(0, StartPosition);
                GunLine.SetPosition(1, EndPosition);

                int Step = GunLineQuality;
                switch (gunVariation)
                {
                    case 1: // Lightning
                        if (GetGunInput(true) || gunLocked)
                        {
                            GunLine.positionCount = Step;
                            GunLine.SetPosition(0, StartPosition);

                            for (int i = 1; i < (Step - 1); i++)
                            {
                                Vector3 Position = Vector3.Lerp(StartPosition, EndPosition, i / (Step - 1f));
                                GunLine.SetPosition(i, Position + (UnityEngine.Random.Range(0f, 1f) > 0.75f ? new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f)) : Vector3.zero));
                            }

                            GunLine.SetPosition(Step - 1, EndPosition);
                        }
                        break;
                    case 2: // Wavy
                        if (GetGunInput(true) || gunLocked)
                        {
                            GunLine.positionCount = Step;
                            GunLine.SetPosition(0, StartPosition);

                            for (int i = 1; i < (Step - 1); i++)
                            {
                                float value = ((float)i / (float)Step) * 50f;

                                Vector3 Position = Vector3.Lerp(StartPosition, EndPosition, i / (Step - 1f));
                                GunLine.SetPosition(i, Position + (Up * Mathf.Sin((Time.time * -10f) + value) * 0.1f));
                            }

                            GunLine.SetPosition(Step - 1, EndPosition);
                        }
                        break;
                    case 3: // Blocky
                        if (GetGunInput(true) || gunLocked)
                        {
                            GunLine.positionCount = Step;
                            GunLine.SetPosition(0, StartPosition);

                            for (int i = 1; i < (Step - 1); i++)
                            {
                                Vector3 Position = Vector3.Lerp(StartPosition, EndPosition, i / (Step - 1f));
                                GunLine.SetPosition(i, new Vector3(Mathf.Round(Position.x * 25f) / 25f, Mathf.Round(Position.y * 25f) / 25f, Mathf.Round(Position.z * 25f) / 25f));
                            }

                            GunLine.SetPosition(Step - 1, EndPosition);
                        }
                        break;
                    case 4: // Sinewave
                        Step = GunLineQuality / 2;

                        if (GetGunInput(true) || gunLocked)
                        {
                            GunLine.positionCount = Step;
                            GunLine.SetPosition(0, StartPosition);

                            for (int i = 1; i < (Step - 1); i++)
                            {
                                Vector3 Position = Vector3.Lerp(StartPosition, EndPosition, i / (Step - 1f));
                                GunLine.SetPosition(i, Position + (Up * Mathf.Sin(Time.time * 10f) * (i % 2 == 0 ? 0.1f : -0.1f)));
                            }

                            GunLine.SetPosition(Step - 1, EndPosition);
                        }
                        break;
                    case 5: // Spring
                        if (GetGunInput(true) || gunLocked)
                        {
                            GunLine.positionCount = Step;
                            GunLine.SetPosition(0, StartPosition);

                            for (int i = 1; i < (Step - 1); i++)
                            {
                                float value = ((float)i / (float)Step) * 50f;

                                Vector3 Position = Vector3.Lerp(StartPosition, EndPosition, i / (Step - 1f));
                                GunLine.SetPosition(i, Position + (Right * Mathf.Cos((Time.time * -10f) + value) * 0.1f) + (Up * Mathf.Sin((Time.time * -10f) + value) * 0.1f));
                            }

                            GunLine.SetPosition(Step - 1, EndPosition);
                        }
                        break;
                    case 6: // Bouncy
                        if (GetGunInput(true) || gunLocked)
                        {
                            GunLine.positionCount = Step;
                            GunLine.SetPosition(0, StartPosition);

                            for (int i = 1; i < (Step - 1); i++)
                            {
                                float value = ((float)i / (float)Step) * 15f;
                                GunLine.SetPosition(i, Vector3.Lerp(StartPosition, EndPosition, i / (Step - 1f)) + (Up * Mathf.Abs(Mathf.Sin((Time.time * -10f) + value)) * 0.3f));
                            }

                            GunLine.SetPosition(Step - 1, EndPosition);
                        }
                        break;
                    case 7: // Audio
                        if (GetGunInput(true) || gunLocked)
                        {
                            float audioSize = 0f;

                            if (gunLocked)
                            {
                                GorillaSpeakerLoudness targetRecorder = lockTarget.GetComponent<GorillaSpeakerLoudness>();
                                if (targetRecorder != null)
                                    audioSize += targetRecorder.Loudness * 3f;
                            }

                            GorillaSpeakerLoudness localRecorder = VRRig.LocalRig.GetComponent<GorillaSpeakerLoudness>();
                            if (localRecorder != null)
                                audioSize += localRecorder.Loudness * 3f;

                            volumeArchive.Insert(0, volumeArchive.Count == 0 ? 0 : (audioSize - volumeArchive[0] * 0.1f));

                            if (volumeArchive.Count > Step)
                                volumeArchive.Remove(Step);

                            GunLine.positionCount = Step;
                            GunLine.SetPosition(0, StartPosition);

                            for (int i = 1; i < (Step - 1); i++)
                            {
                                Vector3 Position = Vector3.Lerp(StartPosition, EndPosition, i / (Step - 1f));
                                GunLine.SetPosition(i, Position + (Up * (i >= volumeArchive.Count ? 0 : volumeArchive[i]) * (i % 2 == 0 ? 1f : -1f)));
                            }

                            GunLine.SetPosition(Step - 1, EndPosition);
                        }
                        break;
                    case 8: // Bezier, credits to Crisp / Kman / Steal / Untitled One of those 4 I don't really know who
                        Vector3 BaseMid = Vector3.Lerp(StartPosition, EndPosition, 0.5f);

                        float angle = Time.time * 3f;
                        Vector3 wobbleOffset = Mathf.Sin(angle) * Up * 0.15f + Mathf.Cos(angle * 1.3f) * Right * 0.15f;
                        Vector3 targetMid = BaseMid + wobbleOffset;

                        if (MidPosition == Vector3.zero) MidPosition = targetMid;

                        Vector3 force = (targetMid - MidPosition) * 40f;
                        MidVelocity += force * Time.deltaTime;
                        MidVelocity *= Mathf.Exp(-6f * Time.deltaTime);
                        MidPosition += MidVelocity * Time.deltaTime;

                        GunLine.positionCount = Step;
                        GunLine.SetPosition(0, StartPosition);

                        Vector3[] points = new Vector3[Step];
                        for (int i = 0; i < Step; i++)
                        {
                            float t = (float)i / (Step - 1);
                            points[i] = Mathf.Pow(1 - t, 2) * StartPosition +
                                        2 * (1 - t) * t * MidPosition +
                                        Mathf.Pow(t, 2) * EndPosition;
                        }

                        GunLine.positionCount = Step;
                        GunLine.SetPositions(points);
                        break;
                }
            }

            return (Ray, GunPointer);
        }
        public static ButtonInfo GetIndex(string buttonText)
        {
            foreach (ButtonInfo[] buttons in Menu.Buttons.buttons)
            {
                foreach (ButtonInfo button in buttons)
                {
                    if (button.buttonText == buttonText)
                    {
                        return button;
                    }
                }
            }

            return null;
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
        public static Dictionary<string, List<string>> ModBindings = new Dictionary<string, List<string>> {
            { "A", new List<string> { } },
            { "B", new List<string> { } },
            { "X", new List<string> { } },
            { "Y", new List<string> { } },
            { "LG", new List<string> { } },
            { "RG", new List<string> { } },
            { "LT", new List<string> { } },
            { "RT", new List<string> { } },
            { "LJ", new List<string> { } },
            { "RJ", new List<string> { } },
        };

        public static Dictionary<string, bool> BindStates = new Dictionary<string, bool> {
            { "A", false },
            { "B", false },
            { "X", false },
            { "Y", false },
            { "LG", false },
            { "RG", false },
            { "LT", false },
            { "RT", false },
            { "LJ", false },
            { "RJ", false },
        };

        public static List<string> quickActions = new List<string> { };
        

        // Variables
            // Important
                // Objects
                    public static GameObject menu;
                    public static GameObject menuBackground;   
                    public static GameObject reference;
                    public static GameObject canvasObject;

        public static SphereCollider buttonCollider;
        public static Camera TPC;
        public static Text fpsObject;
        
        // Data
        public static int pageNumber = 0;
        public static int buttonsType = 0;
        public static int buttonType = 0;
        public static bool isOnPC;
        public static bool IsSteam = true;
        public static bool Lockdown;

        public static bool HasLoaded;
        public static bool hasLoadedPreferences;
        public static bool hasRemovedThisFrame;
        public static bool NoOverlapRPCs = true;
        public static float loadPreferencesTime;
        public static float playTime;

        public static bool thinMenu = true;
        public static bool longmenu;
        public static bool hidetitle;
        public static bool disorganized;
        public static bool flipMenu;
        public static bool shinymenu;
        public static bool crystallizemenu;
        public static bool zeroGravityMenu;
        public static bool dropOnRemove = true;
        public static bool shouldOutline;
        public static bool outlineText;
        public static bool innerOutline;
        public static bool smoothLines;
        public static bool shouldRound;
        public static bool lastclicking;
        public static bool openedwithright;
        public static bool oneHand;

        public static int _pageSize = 8;
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
        public static bool dynamicSounds;
        public static bool exclusivePageSounds;
        public static bool dynamicAnimations;
        public static bool dynamicGradients;

        public static Vector2 leftJoystick = Vector2.zero;
        public static Vector2 rightJoystick = Vector2.zero;
        public static bool leftJoystickClick;
        public static bool rightJoystickClick;
        public static bool ToggleBindings = true;
        public static bool IsBinding;
        public static string BindInput = ""; 
        public static int pageSize
        {
            get => _pageSize - buttonOffset;
            set => _pageSize = value;
        }
        public static int buttonClickSound = 8;
        public static int buttonClickIndex;
        public static int buttonClickVolume = 4;
        public static int buttonOffset = 2;
        public static float buttonDistance
        {
            get => 0.8f / (pageSize + buttonOffset);
        }
        public static Color GetBGColor(float offset)
        {
            Gradient bg = new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(bgColorA, 0f),
                    new GradientColorKey(bgColorB, 0.5f),
                    new GradientColorKey(bgColorA, 1f)
                }
            };
            Color oColor = bg.Evaluate((Time.time / 2f + offset) % 1f);

            switch (themeType)
            {
                case 6:
                    {
                        float h = ((Time.frameCount / 180f) + offset) % 1f;
                        oColor = Color.HSVToRGB(h, 1f, 1f);
                        break;
                    }
                case 47:
                    {
                        oColor = new Color32(
                            (byte)UnityEngine.Random.Range(0, 255),
                            (byte)UnityEngine.Random.Range(0, 255),
                            (byte)UnityEngine.Random.Range(0, 255),
                            255);
                        break;
                    }
                case 51:
                    {
                        float h = (Time.frameCount / 180f) % 1f;
                        oColor = Color.HSVToRGB(h, 0.3f, 1f);
                        break;
                    }
                case 8:
                    {
                        oColor = GetPlayerColor(VRRig.LocalRig);
                        break;
                    }
            }

            return oColor;
        }
        public static Texture2D LoadTextureFromResource(string resourcePath)
        {
            Texture2D texture = new Texture2D(2, 2);

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            if (stream != null)
            {
                byte[] fileData = new byte[stream.Length];
                stream.Read(fileData, 0, (int)stream.Length);
                texture.LoadImage(fileData);
            }
            else
                LogManager.LogError("Failed to load texture from resource: " + resourcePath);

            return texture;
        }
        public static int NoInvisLayerMask() =>
            ~(1 << TransparentFX | 1 << IgnoreRaycast | 1 << Zone | 1 << GorillaTrigger | 1 << GorillaBoundary | 1 << GorillaCosmetics | 1 << GorillaParticle);
        public static Texture2D LoadTextureFromURL(string resourcePath, string fileName)
        {
            Texture2D texture = new Texture2D(2, 2);

            if (!File.Exists($"{PluginInfo.BaseDirectory}/" + fileName))
            {
                LogManager.Log("Downloading " + fileName);
                WebClient stream = new WebClient();
                stream.DownloadFile(resourcePath, $"{PluginInfo.BaseDirectory}/" + fileName);
            }

            byte[] bytes = File.ReadAllBytes($"{PluginInfo.BaseDirectory}/" + fileName);
            texture.LoadImage(bytes);

            return texture;
        }
        public static Color GetPlayerColor(VRRig Player)
        {
            if (GetIndex("Follow Player Colors").enabled)
                return Player.playerColor;

            if (Player.bodyRenderer.bodyType == GorillaBodyType.Skeleton)
                return Color.green;

            switch (Player.setMatIndex)
            {
                case 1:
                    return Color.red;
                case 2:
                case 11:
                    return new Color32(255, 128, 0, 255);
                case 3:
                case 7:
                    return Color.blue;
                case 12:
                    return Color.green;
                default:
                    return Player.playerColor;
            }
        }
        public static void RPCProtection()
        {
            try
            {
                if (hasRemovedThisFrame == false)
                {
                    if (NoOverlapRPCs)
                        hasRemovedThisFrame = true;

                    GorillaNot.instance.rpcErrorMax = int.MaxValue;
                    GorillaNot.instance.rpcCallLimit = int.MaxValue;
                    GorillaNot.instance.logErrorMax = int.MaxValue;

                    PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                    PhotonNetwork.QuickResends = int.MaxValue;

                    PhotonNetwork.SendAllOutgoingCommands();
                }
            }
            catch { LogManager.Log("RPC protection failed, are you in a lobby?"); }
        }
        public static void ChangeName(string PlayerName)
        {
            GorillaComputer.instance.currentName = PlayerName;

            GorillaComputer.instance.SetLocalNameTagText(GorillaComputer.instance.currentName);
            GorillaComputer.instance.savedName = GorillaComputer.instance.currentName;
            PlayerPrefs.SetString("playerName", GorillaComputer.instance.currentName);
            PlayerPrefs.Save();

            PhotonNetwork.LocalPlayer.NickName = PlayerName;

            try
            {
                if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) || CosmeticWardrobeProximityDetector.IsUserNearWardrobe(PhotonNetwork.LocalPlayer.UserId))
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, new object[] { VRRig.LocalRig.playerColor.r, VRRig.LocalRig.playerColor.g, VRRig.LocalRig.playerColor.b });
                    RPCProtection();
                }
            }
            catch { }
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
        public static string NoRichtextTags(string input, string replace = "")
        {
            Regex notags = new Regex("<.*?>", RegexOptions.IgnoreCase);
            return notags.Replace(input, replace);
        }
        public static GameObject audiomgr;
        public static void Play2DAudio(AudioClip sound, float volume)
        {
            if (audiomgr == null)
            {
                audiomgr = new GameObject("2DAudioMgr");
                AudioSource temp = audiomgr.AddComponent<AudioSource>();
                temp.spatialBlend = 0f;
            }
            AudioSource ausrc = audiomgr.GetComponent<AudioSource>();
            ausrc.volume = volume;
            ausrc.PlayOneShot(sound);
        }
        public static AudioType GetAudioType(string extension)
        {
            switch (extension.ToLower())
            {
                case "mp3":
                    return AudioType.MPEG;
                case "wav":
                    return AudioType.WAV;
                case "ogg":
                    return AudioType.OGGVORBIS;
                case "aiff":
                    return AudioType.AIFF;
                default:
                    return AudioType.WAV;
            }
        }
        public static AudioClip LoadSoundFromFile(string fileName) // Thanks to ShibaGT for help with loading the audio from file
        {
            AudioClip sound;
            if (!audioFilePool.ContainsKey(fileName))
            {
                string filePath = Path.Combine(Assembly.GetExecutingAssembly().Location, $"{PluginInfo.BaseDirectory}/{fileName}");
                filePath = $"{filePath.Split("BepInEx\\")[0]}{PluginInfo.BaseDirectory}/{fileName}";
                filePath = filePath.Replace("\\", "/");

                UnityWebRequest actualrequest = UnityWebRequestMultimedia.GetAudioClip($"file://{filePath}", GetAudioType(GetFileExtension(fileName)));
                UnityWebRequestAsyncOperation newvar = actualrequest.SendWebRequest();
                while (!newvar.isDone) { }

                AudioClip actualclip = DownloadHandlerAudioClip.GetContent(actualrequest);
                sound = Task.FromResult(actualclip).Result;

                audioFilePool.Add(fileName, sound);
            }
            else
                sound = audioFilePool[fileName];

            return sound;
        }
        public static AudioClip LoadSoundFromURL(string resourcePath, string fileName)
        {
            if (!File.Exists($"{PluginInfo.BaseDirectory}/{fileName}"))
            {
                LogManager.Log("Downloading " + fileName);
                WebClient stream = new WebClient();
                stream.DownloadFile(resourcePath, $"{PluginInfo.BaseDirectory}/{fileName}");
            }

            return LoadSoundFromFile(fileName);
        }
        public static void JoinDiscord() =>
            Process.Start(serverLink);
        public static bool stackNotifications;
        public static Vector3 GetGunDirection(Transform transform) =>
            new[] { transform.forward, -transform.up, transform == GorillaTagger.Instance.rightHandTransform ? TrueRightHand().forward : TrueLeftHand().forward, GorillaTagger.Instance.headCollider.transform.forward }[GunDirection];
        public static bool narrateNotifications;
        public static bool disableNotifications;
        public static bool disableMasterClientNotifications;
        public static VRRig GhostRig;
        public static bool disableRoomNotifications;
        public static bool disablePlayerNotifications;
        public static bool clearNotificationsOnDisconnect;
        public static string narratorName = "Default";
        public static int narratorIndex;
        public static bool showEnabledModsVR = true;
        public static bool advancedArraylist;
        public static bool flipArraylist;
        public static bool hideSettings;
        public static bool hideTextOnCamera;
        public static bool hidePointer;
        public static bool incrementalButtons = true;
        public static bool disableDisconnectButton;
        public static bool disableFpsCounter;
        public static bool disableSearchButton;
        public static bool disableReturnButton;
        public static bool enableDebugButton;
        public static List<string> favorites = new List<string> { "Exit Favorite Mods" };
        public static bool isSearching;
        public static bool nonGlobalSearch;
        public static bool isPcWhenSearching;
        public static string searchText = "";
        public static float lastBackspaceTime;
        public static bool rightHand;
        public static bool isRightHand;
        public static bool bothHands;
        public static bool wristMenu;
        public static bool watchMenu;
        public static bool wristOpen;
        public static float wristMenuDelay;
        public static bool horizontalGradients;
        public static bool gradientTitle;
        public static string lastClickedName = "";
        public static List<KeyCode> lastPressedKeys = new List<KeyCode>();
        public static KeyCode[] allowedKeys = {
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E,
            KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
            KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O,
            KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
            KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y,
            KeyCode.Z, KeyCode.Space, KeyCode.Backspace,KeyCode.Return, KeyCode.Escape // it doesn't fit :(
        };
        public static string motdTemplate = "You are using build {0}. This menu was created by EV0 (@developer9999.) on discord. " +
        "This menu is completely free and open sourced, if you paid for this menu you have been scammed. " +
        "There are a total of <b>{1}</b> mods on this menu. " +
        "<color=red>I, EV0, am not responsible for any bans using this menu.</color> " +
        "If you get banned while using this, it's your responsibility.";
        public static bool joystickMenu;
        public static bool physicalMenu;
        public static Vector3 physicalOpenPosition = Vector3.zero;
        public static Quaternion physicalOpenRotation = Quaternion.identity;
        public static Vector3 smoothTargetPosition = Vector3.zero;
        public static Quaternion smoothTargetRotation = Quaternion.identity;
        public static bool joystickOpen;
        public static bool smoothMenuPosition;
        public static bool smoothMenuRotation;
        public static int joystickButtonSelected;
        public static string joystickSelectedButton = "";
        public static float joystickDelay;
        public static Font AgencyFB = Font.CreateDynamicFontFromOSFont("Agency FB", 24);
        public static Font Arial = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        public static Font Verdana = Font.CreateDynamicFontFromOSFont("Verdana", 24);
        public static Font ComicSans = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 24);
        public static Font Consolas = Font.CreateDynamicFontFromOSFont("Consolas", 24);
        public static Font Candara = Font.CreateDynamicFontFromOSFont("Candara", 24);
        public static Font MSGothic = Font.CreateDynamicFontFromOSFont("MS Gothic", 24);
        public static Font Impact = Font.CreateDynamicFontFromOSFont("Impact", 24);
        public static Font SimSun = Font.CreateDynamicFontFromOSFont("SimSun", 24);
        public static Font GTFont;
        public static Font Minecraft;
        public static Font Terminal;
        public static Font OpenDyslexic;
        public static Font activeFont = AgencyFB;
        public static FontStyle activeFontStyle = FontStyle.Italic;
        public static Color textColor = new Color32(255, 190, 125, 255);
        public static Color titleColor = new Color32(255, 190, 125, 255);
        public static Color textClicked = new Color32(255, 190, 125, 255);
        public static Dictionary<string, AudioClip> audioFilePool = new Dictionary<string, AudioClip> { };
        public static Color colorChange = Color.black;
        public static int themeType = 1;
        public static bool slowFadeColors = false;
        public static Color bgColorA = new Color32(255, 128, 0, 128);
        public static Color bgColorB = new Color32(255, 102, 0, 128);
        public static Dictionary<string, float> waitingForTranslate = new Dictionary<string, float> { };
        public static Dictionary<string, string> translateCache = new Dictionary<string, string> { };
        public static bool translate;
        public static string language;
        public static string serverLink = "https://discord.gg/xNP7yZXxMf";
        public static string StumpLeaderboardID = "UnityTempFile";
        public static string ForestLeaderboardID = "UnityTempFile";

        public static bool ghostException;

        public static Color buttonDefaultA = new Color32(170, 85, 0, 255);
        public static Color buttonDefaultB = new Color32(170, 85, 0, 255);

        public static Color buttonClickedA = new Color32(85, 42, 0, 255);
        public static Color buttonClickedB = new Color32(85, 42, 0, 255);

        public static int StumpLeaderboardIndex = 5;
        private static VRRig _giveGunTarget;
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
        public static int ForestLeaderboardIndex = 13;

        public static List<string> muteIDs = new List<string> { };
        public static List<string> mutedIDs = new List<string> { };
        public static Vector3 ServerSyncPos;
        public static Vector3 ServerSyncLeftHandPos;
        public static Vector3 ServerSyncRightHandPos;
        public static bool SmoothGunPointer;
        public static bool smallGunPointer;
        public static bool disableGunPointer;
        public static bool disableGunLine;
        public static bool SwapGunHand;
        public static bool GriplessGuns;
        public static bool TriggerlessGuns;
        public static bool HardGunLocks;
        public static bool GunSounds;
        public static bool GunParticles;
        public static int gunVariation;
        public static int GunDirection;
        public static int GunLineQuality = 50;

        public static bool GunSpawned;
        public static bool gunLocked;
        public static VRRig lockTarget;

        public static int fullModAmount = -1;
        public static int amountPartying;
        public static bool waitForPlayerJoin;
        public static bool scaleWithPlayer;
        public static float menuScale = 1f;
        public static int notificationScale = 30;
        public static int overlayScale = 30;
        public static int arraylistScale = 20;

        private static List<float> volumeArchive = new List<float> { };
        private static Vector3 GunPositionSmoothed = Vector3.zero;

        public static bool lowercaseMode;
        public static bool uppercaseMode;
        public static string inputTextColor = "green";

        public static int TransparentFX = LayerMask.NameToLayer("TransparentFX");
        public static int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        public static int Zone = LayerMask.NameToLayer("Zone");
        public static int GorillaTrigger = LayerMask.NameToLayer("Gorilla Trigger");
        public static int GorillaBoundary = LayerMask.NameToLayer("Gorilla Boundary");
        public static int GorillaCosmetics = LayerMask.NameToLayer("GorillaCosmetics");
        public static int GorillaParticle = LayerMask.NameToLayer("GorillaParticle");

        private static GameObject GunPointer;
        private static LineRenderer GunLine;
        public static float timeMenuStarted = -1f;
        public static float kgDebounce;
        public static float stealIdentityDelay;
        public static float beesDelay;
        public static float laggyRigDelay;
        public static float jrDebounce;
        public static float soundDebounce;
        public static float buttonCooldown;
        public static float colorChangerDelay;
        public static float autoSaveDelay = Time.time + 60f;
        public static bool BackupPreferences;
        public static int PreferenceBackupCount;

        public static Vector3 MidPosition;
        public static Vector3 MidVelocity;
    }
}

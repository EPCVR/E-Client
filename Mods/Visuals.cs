using System;
using System.Collections.Generic;
using System.Text;
using GorillaExtensions;
using StupidTemplate.Menu;
using UnityEngine;
using Fusion;
using GorillaGameModes;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine.Rendering;
using g3;
using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaTagScripts;
using HarmonyLib;
using Photon.Realtime;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;
using GorillaTag;

namespace StupidTemplate.Mods
{
    internal class Visuals
    {
        #region Base

        public static int themeType = 1;
        public static bool slowFadeColors = false;
        public static Color bgColorA = new Color32(255, 128, 0, 128);
        public static Color bgColorB = new Color32(255, 102, 0, 128);
        public static bool smoothLines;
        public static Color GetPlayerColor(VRRig Player)
        {
            if (Main.GetIndex("Follow Player Colors").enabled)
                return Player.playerColor;

            
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
        private static Dictionary<VRRig, List<LineRenderer>> boneESP = new Dictionary<VRRig, List<LineRenderer>>() { };
        public static int[] bones = new int[] {
            4, 3, 5, 4, 19, 18, 20, 19, 3, 18, 21, 20, 22, 21, 25, 21, 29, 21, 31, 29, 27, 25, 24, 22, 6, 5, 7, 6, 10, 6, 14, 6, 16, 14, 12, 10, 9, 7
        };
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
        public static bool PlayerIsTagged(VRRig Player)
        {
            List<NetPlayer> infectedPlayers = InfectedList();
            NetPlayer targetPlayer = GetPlayerFromVRRig(Player);

            return infectedPlayers.Contains(targetPlayer);
        }

        private static NetPlayer GetPlayerFromVRRig(VRRig player)
        {
            throw new NotImplementedException();
        }
        private static GameObject lineRenderHolder = null;
        private static List<LineRenderer> linePool = new List<LineRenderer>();
        private static LineRenderer GetLineRender(bool hideOnCamera)
        {
            if (lineRenderHolder == null)
                lineRenderHolder = new GameObject("LineRender_Holder");

            LineRenderer finalRender = null;

            foreach (LineRenderer line in linePool)
            {
                if (finalRender != null) continue;

                if (!line.gameObject.activeInHierarchy)
                {
                    line.gameObject.SetActive(true);
                    finalRender = line;
                }
            }

            if (finalRender == null)
            {
                GameObject lineHolder = new GameObject("LineObject");
                lineHolder.transform.parent = lineRenderHolder.transform;
                LineRenderer newLine = lineHolder.AddComponent<LineRenderer>();
                if (smoothLines)
                {
                    newLine.numCapVertices = 10;
                    newLine.numCornerVertices = 5;
                }
                newLine.material.shader = Shader.Find("GUI/Text Shader");
                newLine.startWidth = 0.025f;
                newLine.endWidth = 0.025f;
                newLine.positionCount = 2;
                newLine.useWorldSpace = true;

                linePool.Add(newLine);

                finalRender = newLine;
            }

            if (hideOnCamera)
                finalRender.gameObject.layer = 19;
            else
                finalRender.gameObject.layer = lineRenderHolder.layer;

            return finalRender;
        }
        public static bool PerformanceVisuals;

        public static float PerformanceModeStep = 0.2f;
        public static int PerformanceModeStepIndex = 2;
        public static void ChangePerformanceModeVisualStep(bool positive = true)
        {
            if (positive)
                PerformanceModeStepIndex++;
            else
                PerformanceModeStepIndex--;

            PerformanceModeStepIndex %= 11;
            if (PerformanceModeStepIndex < 0)
                PerformanceModeStepIndex = 10;

            PerformanceModeStep = PerformanceModeStepIndex / 10f;
            Main.GetIndex("Change Performance Visuals Step").overlapText = "Change Performance Visuals Step <color=grey>[</color><color=green>" + PerformanceModeStep.ToString() + "</color><color=grey>]</color>";
        }
        public static float PerformanceVisualDelay;
        public static int DelayChangeStep;
        private static bool DoPerformanceCheck()
        {
            if (PerformanceVisuals)
            {
                if (Time.time < PerformanceVisualDelay)
                {
                    if (Time.frameCount != DelayChangeStep)
                        return true;
                }
                else
                {
                    PerformanceVisualDelay = Time.time + PerformanceModeStep;
                    DelayChangeStep = Time.frameCount;
                }
            }

            return false;
        }

        public static void DisableBoneESP()
        {
            foreach (KeyValuePair<VRRig, List<LineRenderer>> bones in boneESP)
            {
                foreach (LineRenderer renderer in bones.Value)
                    UnityEngine.Object.Destroy(renderer);
            }

            boneESP.Clear();
        }
        public static bool isLineRenderQueued = false;
        #endregion
        public static void CasualBoneESP()
        {
            bool fmt = Main.GetIndex("Follow Menu Theme").enabled;
            bool hoc = Main.GetIndex("Hidden on Camera").enabled;
            bool tt = Main.GetIndex("Transparent Theme").enabled;
            bool thinTracers = Main.GetIndex("Thin Tracers").enabled;

            List<VRRig> toRemove = new List<VRRig>();

            foreach (KeyValuePair<VRRig, List<LineRenderer>> boness in boneESP)
            {
                if (!GorillaParent.instance.vrrigs.Contains(boness.Key))
                {
                    toRemove.Add(boness.Key);

                    foreach (LineRenderer renderer in boness.Value)
                        UnityEngine.Object.Destroy(renderer);
                }
            }

            foreach (VRRig rig in toRemove)
                boneESP.Remove(rig);

            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isLocal)
                {
                    if (!boneESP.TryGetValue(vrrig, out List<LineRenderer> Lines))
                    {
                        Lines = new List<LineRenderer> { };

                        LineRenderer LineHead = vrrig.head.rigTarget.gameObject.GetOrAddComponent<LineRenderer>();
                        if (smoothLines)
                        {
                            LineHead.numCapVertices = 10;
                            LineHead.numCornerVertices = 5;
                        }
                        LineHead.material.shader = Shader.Find("GUI/Text Shader");
                        Lines.Add(LineHead);

                        for (int i = 0; i < 19; i++)
                        {
                            LineRenderer Line = vrrig.mainSkin.bones[bones[i * 2]].gameObject.GetOrAddComponent<LineRenderer>();
                            if (smoothLines)
                            {
                                Line.numCapVertices = 10;
                                Line.numCornerVertices = 5;
                            }
                            Line.material.shader = Shader.Find("GUI/Text Shader");
                            Lines.Add(Line);
                        }

                        boneESP.Add(vrrig, Lines);
                    }

                    LineRenderer liner = Lines[0];

                    Color thecolor = vrrig.playerColor;
                    if (fmt)
                        thecolor = GetBGColor(0f);
                    if (tt)
                        thecolor.a = 0.5f;
                    if (hoc)
                        liner.gameObject.layer = 19;

                    liner.startWidth = thinTracers ? 0.0075f : 0.025f;
                    liner.endWidth = thinTracers ? 0.0075f : 0.025f;

                    liner.startColor = thecolor;
                    liner.endColor = thecolor;

                    liner.SetPosition(0, vrrig.head.rigTarget.transform.position + new Vector3(0f, 0.16f, 0f));
                    liner.SetPosition(1, vrrig.head.rigTarget.transform.position - new Vector3(0f, 0.4f, 0f));

                    for (int i = 0; i < 19; i++)
                    {
                        liner = Lines[i + 1];

                        if (hoc)
                            liner.gameObject.layer = 19;

                        liner.startWidth = thinTracers ? 0.0075f : 0.025f;
                        liner.endWidth = thinTracers ? 0.0075f : 0.025f;

                        liner.startColor = thecolor;
                        liner.endColor = thecolor;

                        liner.material.shader = Shader.Find("GUI/Text Shader");

                        liner.SetPosition(0, vrrig.mainSkin.bones[bones[i * 2]].position);
                        liner.SetPosition(1, vrrig.mainSkin.bones[bones[(i * 2) + 1]].position);
                    }
                }
            }
        }

        public static void InfectionBoneESP()
        {
            bool fmt = Main.GetIndex("Follow Menu Theme").enabled;
            bool hoc = Main.GetIndex("Hidden on Camera").enabled;
            bool tt = Main.GetIndex("Transparent Theme").enabled;
            bool thinTracers = Main.GetIndex("Thin Tracers").enabled;
            bool selfTagged = PlayerIsTagged(VRRig.LocalRig);

            List<VRRig> toRemove = new List<VRRig>();

            foreach (KeyValuePair<VRRig, List<LineRenderer>> boness in boneESP)
            {
                if (!GorillaParent.instance.vrrigs.Contains(boness.Key))
                {
                    toRemove.Add(boness.Key);

                    foreach (LineRenderer renderer in boness.Value)
                        UnityEngine.Object.Destroy(renderer);
                }
            }

            foreach (VRRig rig in toRemove)
                boneESP.Remove(rig);

            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isLocal)
                {
                    if (!boneESP.TryGetValue(vrrig, out List<LineRenderer> Lines))
                    {
                        Lines = new List<LineRenderer> { };

                        LineRenderer LineHead = vrrig.head.rigTarget.gameObject.GetOrAddComponent<LineRenderer>();
                        if (smoothLines)
                        {
                            LineHead.numCapVertices = 10;
                            LineHead.numCornerVertices = 5;
                        }
                        LineHead.material.shader = Shader.Find("GUI/Text Shader");
                        Lines.Add(LineHead);

                        for (int i = 0; i < 19; i++)
                        {
                            LineRenderer Line = vrrig.mainSkin.bones[bones[i * 2]].gameObject.GetOrAddComponent<LineRenderer>();
                            if (smoothLines)
                            {
                                Line.numCapVertices = 10;
                                Line.numCornerVertices = 5;
                            }
                            Line.material.shader = Shader.Find("GUI/Text Shader");
                            Lines.Add(Line);
                        }

                        boneESP.Add(vrrig, Lines);
                    }

                    LineRenderer liner = Lines[0];

                    bool playerTagged = PlayerIsTagged(vrrig);
                    Color thecolor = selfTagged ? vrrig.playerColor : GetPlayerColor(vrrig);

                    if (fmt)
                        thecolor = GetBGColor(0f);
                    if (tt)
                        thecolor.a = 0.5f;
                    if (hoc)
                        liner.gameObject.layer = 19;

                    liner.startWidth = thinTracers ? 0.0075f : 0.025f;
                    liner.endWidth = thinTracers ? 0.0075f : 0.025f;

                    liner.startColor = thecolor;
                    liner.endColor = thecolor;

                    liner.enabled = (selfTagged ? !playerTagged : playerTagged) || InfectedList().Count <= 0;

                    liner.SetPosition(0, vrrig.head.rigTarget.transform.position + new Vector3(0f, 0.16f, 0f));
                    liner.SetPosition(1, vrrig.head.rigTarget.transform.position - new Vector3(0f, 0.4f, 0f));

                    for (int i = 0; i < 19; i++)
                    {
                        liner = Lines[i + 1];

                        if (hoc)
                            liner.gameObject.layer = 19;

                        liner.startWidth = thinTracers ? 0.0075f : 0.025f;
                        liner.endWidth = thinTracers ? 0.0075f : 0.025f;

                        liner.startColor = thecolor;
                        liner.endColor = thecolor;

                        liner.material.shader = Shader.Find("GUI/Text Shader");

                        liner.enabled = (selfTagged ? !playerTagged : playerTagged) || InfectedList().Count <= 0;

                        liner.SetPosition(0, vrrig.mainSkin.bones[bones[i * 2]].position);
                        liner.SetPosition(1, vrrig.mainSkin.bones[bones[(i * 2) + 1]].position);
                    }
                }
            }
        }
        public static void CasualTracers()
        {
            if (DoPerformanceCheck())
                return;

            if (GorillaGameManager.instance == null)
                return;

            bool followMenuTheme = Main.GetIndex("Follow Menu Theme").enabled;
            bool transparentTheme = Main.GetIndex("Transparent Theme").enabled;
            bool hiddenOnCamera = Main.GetIndex("Hidden on Camera").enabled;
            float lineWidth = (Main.GetIndex("Thin Tracers").enabled ? 0.0075f : 0.025f);

            Color menuColor = GetBGColor(0f);

            foreach (VRRig playerRig in GorillaParent.instance.vrrigs)
            {
                if (playerRig.isLocal)
                    continue;

                Color lineColor = playerRig.playerColor;

                LineRenderer line = GetLineRender(hiddenOnCamera);

                if (followMenuTheme)
                    lineColor = menuColor;

                if (transparentTheme)
                    lineColor.a = 0.5f;

                line.startColor = lineColor;
                line.endColor = lineColor;
                line.startWidth = lineWidth;
                line.endWidth = lineWidth;
                line.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                line.SetPosition(1, playerRig.transform.position);
            }
        }

        public static void InfectionTracers()
        {
            if (DoPerformanceCheck())
                return;

            if (GorillaGameManager.instance == null)
                return;

            bool followMenuTheme = Main.GetIndex("Follow Menu Theme").enabled;
            bool transparentTheme = Main.GetIndex("Transparent Theme").enabled;
            bool hiddenOnCamera = Main.GetIndex("Hidden on Camera").enabled;
            float lineWidth = (Main.GetIndex("Thin Tracers").enabled ? 0.0075f : 0.025f);

            bool LocalTagged = PlayerIsTagged(VRRig.LocalRig);
            bool NoInfected = InfectedList().Count == 0;

            foreach (VRRig playerRig in GorillaParent.instance.vrrigs)
            {
                if (playerRig.isLocal)
                    continue;

                Color lineColor = playerRig.playerColor;

                if (!NoInfected)
                {
                    if (LocalTagged)
                    {
                        if (PlayerIsTagged(playerRig))
                            continue;
                    }
                    else
                    {
                        if (!PlayerIsTagged(playerRig))
                            continue;

                        lineColor = GetPlayerColor(playerRig);
                    }
                }

                LineRenderer line = GetLineRender(hiddenOnCamera);

                if (transparentTheme)
                    lineColor.a = 0.5f;

                line.startColor = lineColor;
                line.endColor = lineColor;
                line.startWidth = lineWidth;
                line.endWidth = lineWidth;
                line.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                line.SetPosition(1, playerRig.transform.position);
            }
        }
    }
}

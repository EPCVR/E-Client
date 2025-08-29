using System;
using System.Collections.Generic;
using System.Linq;
using EClient;
using GorillaExtensions;
using GorillaLocomotion;
using HarmonyLib;
using TMPro;
using UnityEngine;
using static EClient.Menu.Main;

namespace EClient.Mods
{
    internal class Visuals
    {
        private static Dictionary<VRRig, List<LineRenderer>> boneESP = new Dictionary<VRRig, List<LineRenderer>>() { };
        public static void CasualBoneESP()
        {
            bool fmt = GetIndex("Follow Menu Theme").enabled;
            bool hoc = GetIndex("Hidden on Camera").enabled;
            bool tt = GetIndex("Transparent Theme").enabled;
            bool thinTracers = GetIndex("Thin Tracers").enabled;

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
            bool fmt = GetIndex("Follow Menu Theme").enabled;
            bool hoc = GetIndex("Hidden on Camera").enabled;
            bool tt = GetIndex("Transparent Theme").enabled;
            bool thinTracers = GetIndex("Thin Tracers").enabled;
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
        public static void DisableBoneESP()
        {
            foreach (KeyValuePair<VRRig, List<LineRenderer>> bones in boneESP)
            {
                foreach (LineRenderer renderer in bones.Value)
                    UnityEngine.Object.Destroy(renderer);
            }

            boneESP.Clear();
        }
        public static void CasualTracers()
        {
            if (DoPerformanceCheck())
                return;

            if (GorillaGameManager.instance == null)
                return;

            bool followMenuTheme = GetIndex("Follow Menu Theme").enabled;
            bool transparentTheme = GetIndex("Transparent Theme").enabled;
            bool hiddenOnCamera = GetIndex("Hidden on Camera").enabled;
            float lineWidth = (GetIndex("Thin Tracers").enabled ? 0.0075f : 0.025f) * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);

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

            bool followMenuTheme = GetIndex("Follow Menu Theme").enabled;
            bool transparentTheme = GetIndex("Transparent Theme").enabled;
            bool hiddenOnCamera = GetIndex("Hidden on Camera").enabled;
            float lineWidth = (GetIndex("Thin Tracers").enabled ? 0.0075f : 0.025f) * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);

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
        private static List<LineRenderer> linePool = new List<LineRenderer>();

        private static GameObject lineRenderHolder = null;

        public static bool isLineRenderQueued = false;

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
        static GameObject LeftSphere = null;
        static GameObject RightSphere = null;

        public static float PerformanceVisualDelay;
        public static int DelayChangeStep;

        public static bool PerformanceVisuals;

        public static float PerformanceModeStep = 0.2f;
        public static int PerformanceModeStepIndex = 2;
    }
}
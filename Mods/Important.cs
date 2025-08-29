using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cinemachine;
using EClient.Classes;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using static EClient.Menu.Main;

namespace EClient.Mods
{
    public class Important
    {
        public static string oldId = "";
        const byte DEFAULT_MAX_PLAYERS = 10;

        public static Coroutine queueCoroutine;
        public static int reconnectDelay = 1;
        public static void CreateRoom(string roomName, bool isPublic)
        {
            RoomConfig roomConfig = new RoomConfig()
            {
                createIfMissing = true,
                isJoinable = true,
                isPublic = isPublic,
                MaxPlayers = DEFAULT_MAX_PLAYERS,
                CustomProps = new ExitGames.Client.Photon.Hashtable()
                {
                    { "gameMode", PhotonNetworkController.Instance.currentJoinTrigger.GetFullDesiredGameModeString() },
                    { "platform", GetPlatformTag()},
                    { "queueName", GorillaComputer.instance.currentQueue }
                }
            };
            NetworkSystem.Instance.ConnectToRoom(roomName, roomConfig);
        }
        public static void QueueRoom(string roomName)
        {
            if (queueCoroutine != null)
                CoroutineManager.instance.StopCoroutine(queueCoroutine);

            ;
        }
        private static string GetPlatformTag()
        {
        #if UNITY_ANDROID
            return "quest";      // or "ANDROID" if that’s what your backend expects
        #elif UNITY_STANDALONE_WIN
            return "steam";      // or "PC"/"WINDOWS"
        #else
            return Application.platform.ToString().ToLower();
        #endif
        }


    }

}

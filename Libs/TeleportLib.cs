using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorillaLocomotion;
using EClient.Patches;
using UnityEngine;
using EClient.Utilities;

namespace EClient.Libs.TeleportLib
{
        internal class TeleportLib
        {
            public static void TeleportTo(Vector3 position)
            {
                GTPlayer.Instance.TeleportTo(position - __instance.bodyCollider.transform.position + __instance.transform.position, GTPlayer.Instance.transform.rotation);
            }
        public static GTPlayer __instance;
    }
}

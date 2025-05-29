using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;

namespace StupidTemplate.Mods
{
    internal class Overpowered
    {
        public static void infcurrency()
        {
            if (!PhotonNetwork.IsMasterClient) { return; }
            NetworkView netview = GorillaTagger.Instance.myVRRig;
            GRPlayer grrr = GRPlayer.Get(netview.GetView.CreatorActorNr);
            grrr.currency = int.MaxValue;
        }


    }
}

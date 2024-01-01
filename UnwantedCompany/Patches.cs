using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnwantedCompany.MonoBehaviors;

namespace UnwantedCompany
{
    public class Patches
    {
        public static void Load()
        {
            On.GameNetworkManager.Start += GameNetworkManager_Start;

            GreedBomb.Load();
            CustomHoverText.Load();
        }

        public delegate void GameNetworkManagerStart();
        public static event GameNetworkManagerStart GameNetworkManagerStartEvent;
        private static void GameNetworkManager_Start(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
        {
            orig(self);
            GameNetworkManagerStartEvent?.Invoke();
        }
    }
}

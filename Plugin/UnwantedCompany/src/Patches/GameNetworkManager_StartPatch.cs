using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnwantedCompany.MonoBehaviors;

namespace UnwantedCompany.Patches
{
    static class GameNetworkManager_StartPatch
    {

        public static void Init()
        {
            On.GameNetworkManager.Start += GameNetworkManager_Start;
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

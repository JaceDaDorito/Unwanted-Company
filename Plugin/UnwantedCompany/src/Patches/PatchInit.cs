using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnwantedCompany.MonoBehaviors;
using UnwantedCompany.Patches;

namespace UnwantedCompany
{
    public class PatchInit
    {
        public static void Load()
        {
            GameNetworkManager_StartPatch.Init();
            ItemCharger_ChargeItemPatch.Init();

            CustomHoverText.Init();
        }
    }
}
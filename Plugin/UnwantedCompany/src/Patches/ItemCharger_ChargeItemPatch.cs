using System;
using System.Collections.Generic;
using System.Text;
using UnwantedCompany.MonoBehaviors;

namespace UnwantedCompany.Patches
{
    public static class ItemCharger_ChargeItemPatch
    {
        public static void Init()
        {
            On.ItemCharger.ChargeItem += ItemCharger_ChargeItem;
        }

        private static void ItemCharger_ChargeItem(On.ItemCharger.orig_ChargeItem orig, ItemCharger self)
        {
            GrabbableObject currentlyHeldObjectServer = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
            if (currentlyHeldObjectServer is GreedBomb bomb && !bomb.defused.Value && !bomb.hasExploded.Value && !bomb.explodingOnClient)
            {
                UnwantedCompany.logger.LogDebug($"Defused");
                bomb.isBeingUsed = false;
                bomb.uniqueChargeStationText = "[ Already Defused ]";

                bomb.PushLightState(GreedBomb.LightState.Defused);

                bomb.DefuseBombServerRpc();
            }
            orig(self);
        }
    }
}

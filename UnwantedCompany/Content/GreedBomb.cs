using System;
using UnityEngine;
using GameNetcodeStuff;

namespace UnwantedCompany.MonoBehaviors
{
    public class GreedBomb : UCGrabbableObject
    {
        [Header("Greed Bomb")]
        public Light TickLight1;
        public Light TickLight2;
        public Light TickLight3;
        public Light detonationLight;
        public Light defusedLight;

        private bool defused;

        public static void Load()
        {
            On.ItemCharger.ChargeItem += ItemCharger_ChargeItem;
        }

        private static void ItemCharger_ChargeItem(On.ItemCharger.orig_ChargeItem orig, ItemCharger self)
        {
            GrabbableObject currentlyHeldObjectServer = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
            if (currentlyHeldObjectServer is GreedBomb bomb && !bomb.defused)
            {
                bomb.defused = true;
                bomb.isBeingUsed = false;
                bomb.uniqueChargeStationText = "[ Already Defused ]";
            }
            orig(self);
        }

        public override void Start()
        {
            base.Start();
            defused = false;
        }
        public override void EquipItem()
        {
            base.EquipItem();
            if (!defused)
            {
                //prompt message here
                isBeingUsed = true;
                //start detonation
            }
        }

        //public override void  

        public override void Update()
        {
            base.Update();
            if (!defused && insertedBattery.empty)
            {
                //fucking explode
                Landmine.SpawnExplosion(transform.position, true, 20, 20);
                Destroy(this.gameObject);
            }
        }
    }

    
}

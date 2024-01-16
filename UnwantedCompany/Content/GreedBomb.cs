using System;
using UnityEngine;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public enum LightState
        {
            None = 0,
            Tick1 = 1 << 0,
            Tick2 = 1 << 1,
            Tick3 = 1 << 2,
            Detonate = 1 << 3,
            Defused = 1 << 4
        }

        public LightState currentLightState;

        [HideInInspector]
        public NetworkVariable<bool> defused = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [HideInInspector]
        public NetworkVariable<bool> hasExploded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private bool sendingDefuseRPC;
        private bool sendingDetonateRPC;

        public static void Load()
        {
            On.ItemCharger.ChargeItem += ItemCharger_ChargeItem;
        }

        private static void ItemCharger_ChargeItem(On.ItemCharger.orig_ChargeItem orig, ItemCharger self)
        {
            GrabbableObject currentlyHeldObjectServer = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
            if (currentlyHeldObjectServer is GreedBomb bomb && !bomb.defused.Value && !bomb.hasExploded.Value)
            {
                UnwantedCompany.logger.LogDebug($"Defused");
                bomb.isBeingUsed = false;
                bomb.uniqueChargeStationText = "[ Already Defused ]";

                bomb.PushLightState(LightState.Defused);

                bomb.sendingDefuseRPC = true;
                bomb.DefuseBombServerRpc();
            }
            orig(self);
        }

        

        public override void Start()
        {
            base.Start();
            sendingDefuseRPC = false;
            sendingDetonateRPC = false;
            PushLightState(LightState.Tick1);
        }

        public void PushLightState(LightState lightState)
        {
            currentLightState = lightState;

            TickLight1.enabled = (1 & ((byte)lightState >> 0)) != 0;
            TickLight2.enabled = (1 & ((byte)lightState >> 1)) != 0;
            TickLight3.enabled = (1 & ((byte)lightState >> 2)) != 0;
            detonationLight.enabled = (1 & ((byte)lightState >> 3)) != 0;
            defusedLight.enabled = (1 & ((byte)lightState >> 4)) != 0;
        }
        public override void GrabItem()
        {
            base.GrabItem();
            if (!defused.Value)
            {
                //prompt message here
                isBeingUsed = true;
                //start detonation
            }
        }

        public override void Update()
        {
            base.Update();
            if (!defused.Value && !hasExploded.Value && insertedBattery.empty)
            {
                AttemptDetonation();
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void DefuseBombServerRpc()
        {
            defused.Value = true;
            isBeingUsed = false;
            uniqueChargeStationText = "[ Already Defused ]";
        }

        [ClientRpc]
        public void DefuseBombClientRpc()
        {
            
            if (sendingDefuseRPC)
            {
                sendingDefuseRPC = false;
            }
            else
            {
                UnwantedCompany.logger.LogDebug($"DefuseBombClientRpc");
                SetOffDefuseSequence();
            }
        }
        public void SetOffDefuseSequence()
        {
            UnwantedCompany.logger.LogInfo($"DefuseSequence Started");

            isBeingUsed = false;
            uniqueChargeStationText = "[ Already Defused ]";

            PushLightState(LightState.Defused);
        }
        public bool AttemptDetonation()
        {
            UnwantedCompany.logger.LogDebug("$AttemptDetonation");
            if (!defused.Value && !hasExploded.Value)
            {
                hasExploded.Value = true;
                SetOffDetonationSequence();
                sendingDetonateRPC = true;
                DetonateBombServerRpc();
                return true;
            }
            return false;
        }

        [ServerRpc(RequireOwnership = true)]
        public void DetonateBombServerRpc()
        {
            hasExploded.Value = true;
            DetonateBombClientRpc();
        }

        [ClientRpc]
        public void DetonateBombClientRpc()
        {

            if (sendingDetonateRPC)
            {
                sendingDetonateRPC = false;
            }
            else
            {
                UnwantedCompany.logger.LogDebug($"Client Detonation Sequence");
                SetOffDetonationSequence();
            }
        }

        public void SetOffDetonationSequence()
        {
            PushLightState(LightState.Detonate);
            StartCoroutine(detonateBombDelayed());
        }

        private IEnumerator detonateBombDelayed()
        {
            yield return new WaitForSeconds(0.5f);
            Detonate();
            Destroy(this.gameObject);
        }

        public void Detonate()
        {
            Landmine.SpawnExplosion(transform.position, true, 100, 20f);
        }
    }

    
}

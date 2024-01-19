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

        public List<MeshRenderer> renderersToDisableAfterExplosion = new List<MeshRenderer>();

        [HideInInspector]
        public NetworkVariable<bool> defused = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [HideInInspector]
        public NetworkVariable<bool> hasExploded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        [HideInInspector]
        public bool explodingOnClient = false;
        public static void Load()
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

                bomb.PushLightState(LightState.Defused);

                bomb.DefuseBombServerRpc();
            }
            orig(self);
        }

        

        public override void Start()
        {
            base.Start();
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
                isBeingUsed = true;
            }
        }


        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            if (!defused.Value && !hasExploded.Value && !explodingOnClient)
            {
                DetonateBombServerRpc();
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void DefuseBombServerRpc()
        {
            defused.Value = true;

            DefuseBombClientRpc();
        }

        [ClientRpc]
        public void DefuseBombClientRpc()
        {
            isBeingUsed = false;
            uniqueChargeStationText = "[ Already Defused ]";
            PushLightState(LightState.Defused);
            UnwantedCompany.logger.LogDebug($"DefuseBombClientRpc");
        }

        [ServerRpc(RequireOwnership = false)]
        public void DetonateBombServerRpc()
        {
            hasExploded.Value = true;

            DetonateBombClientRpc();
        }

        [ClientRpc]
        public void DetonateBombClientRpc()
        {
            explodingOnClient = true;

            SetOffDetonationSequence();
            PushLightState(LightState.Detonate);
        }

        public void SetOffDetonationSequence()
        {
            StartCoroutine(detonateBombDelayed());
        }

        private IEnumerator detonateBombDelayed()
        {
            yield return new WaitForSeconds(0.5f);
            Utilities.CreateExplosion(transform.position, true, 100, 0f, 6.4f);

            foreach(MeshRenderer mr in renderersToDisableAfterExplosion)
            {
                mr.enabled = false;
            }

            GetComponentInChildren<ScanNodeProperties>().enabled = false;

            PushLightState(LightState.None);
            grabbable = false;
            grabbableToEnemies = false;
            deactivated = false;

            yield return new WaitForSeconds(2f);

            if (IsServer)
            {
                Destroy(this.gameObject);
                //GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    
}

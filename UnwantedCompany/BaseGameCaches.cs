using System;
using UnityEngine;
using GameNetcodeStuff;
using UnityEngine.Audio;
using Unity.Netcode;
using System.Linq;

namespace UnwantedCompany
{
    public class BaseGameCaches
    {
        public static AudioMixerGroup masterDiageticMixer;

        public static void Init()
        {
            Patches.GameNetworkManagerStartEvent += MasterDiageticCacher;
        }

        private static void MasterDiageticCacher()
        {
            var referenceAudioSource = GameNetworkManager.Instance.GetComponent<NetworkManager>().NetworkConfig.Prefabs.Prefabs
                    .Select(p => p.Prefab.GetComponentInChildren<NoisemakerProp>())
                    .Where(p => p != null)
                    .Select(p => p.GetComponentInChildren<AudioSource>())
                    .Where(p => p != null)
                    .FirstOrDefault();
            if (referenceAudioSource == null)
            {
                throw new Exception("Failed to locate a suitable AudioSource output mixer to reference! Could you be calling this method before the GameNetworkManager is initialized?");
            }
            masterDiageticMixer = referenceAudioSource.outputAudioMixerGroup;
        }
    }
}

using System;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections.Generic;

namespace UnwantedCompany.MonoBehaviors
{
    public class UCGrabbableObject : GrabbableObject
    {
        //Will make more utils in the future maybe
        [Header("Extended UC Attributes")]
        [Tooltip("Unique text for hovering over the charge station. Leave empty if you want default text.")]
        public string uniqueChargeStationText;

        [Tooltip("Link master audio mixer. All pickups use this.")]
        public bool injectMasterMixer = false;

        [Tooltip("Audio mixers that are fixed")]
        public List<AudioSource> sourcesToLinkToMaster;
    }
}

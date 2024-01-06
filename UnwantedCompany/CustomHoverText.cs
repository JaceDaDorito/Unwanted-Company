using System;
using UnityEngine;
using GameNetcodeStuff;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using TMPro;
using UnwantedCompany.MonoBehaviors;
using Zeekerss.Core.Singletons;

namespace UnwantedCompany
{
    public class CustomHoverText
    {
        public static void Load()
        {
            IL.GameNetcodeStuff.PlayerControllerB.SetHoverTipAndCurrentInteractTrigger += PlayerControllerB_SetHoverTipAndCurrentInteractTrigger1;
        }

        private static void PlayerControllerB_SetHoverTipAndCurrentInteractTrigger1(MonoMod.Cil.ILContext il)
        {
            ILLabel brFalseLabel = null, brLabel = null, newLabel = null;
            ILCursor c = new ILCursor(il);
            int interactTriggerIndex = 0;

            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdflda<PlayerControllerB>(nameof(PlayerControllerB.hit)),
                x => x.MatchCall<RaycastHit>("get_transform"),
                x => x.MatchCallOrCallvirt<Component>("get_gameObject"),
                x => x.MatchCallOrCallvirt<GameObject>(nameof(GameObject.GetComponent)),
                x => x.MatchStloc(out interactTriggerIndex)))
            {
                UnwantedCompany.logger.LogWarning($"Couldn't find InteractTrigger in SetHoverTipAndCurrentInteractTrigger");
                return;
            }

            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerControllerB>("isHoldingInteract"),
                x => x.MatchBrfalse(out brFalseLabel)))
            {
                UnwantedCompany.logger.LogWarning($"Couldn't patch SetHoverTipAndCurrentInteractTrigger isHoldingInteract");
                return;
            }
                

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<PlayerControllerB>("twoHanded"),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PlayerControllerB>(nameof(PlayerControllerB.cursorTip)),
                x => x.MatchLdstr(out _),
                x => x.MatchCallOrCallvirt<TMP_Text>("set_text"),
                x => x.MatchBr(out brLabel)))
            {
                c.GotoLabel(brFalseLabel, MoveType.AfterLabel);
                newLabel = c.DefineLabel();
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(UCGrabbableCheck);
                c.Emit(OpCodes.Brfalse, newLabel);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, interactTriggerIndex);
                c.EmitDelegate(CustomTextEmitter);
                c.Emit(OpCodes.Br, brLabel);
                c.MarkLabel(newLabel);
            }
            else
                UnwantedCompany.logger.LogWarning($"Couldn't patch SetHoverTipAndCurrentInteractTrigger");
        }

        private static bool UCGrabbableCheck(PlayerControllerB arg0)
        {
            return arg0.currentlyHeldObjectServer is UCGrabbableObject;
        }

        private static void CustomTextEmitter(PlayerControllerB arg0, InteractTrigger interactTrigger)
        {
            UCGrabbableObject ucgo = (UCGrabbableObject)(arg0.currentlyHeldObjectServer);
            UnwantedCompany.logger.LogInfo(interactTrigger.animationString);
            if(!String.IsNullOrEmpty(ucgo.uniqueChargeStationText))
                arg0.cursorTip.text = interactTrigger.animationString.Equals("SA_ChargeItem") ? ucgo.uniqueChargeStationText : interactTrigger.holdTip;

            UnwantedCompany.logger.LogInfo(arg0.cursorTip.text);
        }

    }
}

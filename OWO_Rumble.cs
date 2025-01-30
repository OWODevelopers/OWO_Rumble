using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using HarmonyLib;
using MyBhapticsTactsuit;
using RUMBLE.MoveSystem;
using RUMBLE.Players;

namespace Rumble_bhaptics
{
    public class OWO_Rumble : MelonMod
    {
        public static TactsuitVR tactsuitVr;

        public override void OnInitializeMelon()
        {
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        [HarmonyPatch(typeof(CombatManager), "RegisterPlayerHitEvent", new Type[] { typeof(Player), typeof(Structure) })]
        public class bhaptics_PlayerHit
        {
            [HarmonyPostfix]
            public static void Postfix(CombatManager __instance, Structure structure, Player player)
            {
                if (player.Controller.ControllerType != ControllerType.Local) return;
            }
        }

        [HarmonyPatch(typeof(CombatManager), "RegisterPlayerHitFromBeneathEvent", new Type[] { typeof(Player), typeof(Structure) })]
        public class bhaptics_PlayerHitFromBeneath
        {
            [HarmonyPostfix]
            public static void Postfix(CombatManager __instance, Structure structure, Player player)
            {
                if (player.Controller.ControllerType != ControllerType.Local) return;
            }
        }

        [HarmonyPatch(typeof(RUMBLE.Players.Subsystems.PlayerHealth), "UpdateLocalHealthbarPercentage", new Type[] { typeof(float), typeof(bool) })]
        public class bhaptics_PlayerUpdateHealth
        {
            [HarmonyPostfix]
            public static void Postfix(RUMBLE.Players.Subsystems.PlayerHealth __instance, float percentage)
            {
                if (percentage <= 0.25f) tactsuitVr.StartHeartBeat();
                else tactsuitVr.StopHeartBeat();
            }
        }


    }
}

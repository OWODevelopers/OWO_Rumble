using HarmonyLib;
using Il2CppRUMBLE.MoveSystem;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Subsystems;
using MelonLoader;
using Rumble_OWO;

[assembly: MelonInfo(typeof(OWO_Rumble.OWO_Rumble), "OWO_Rumble", "1.0.0", "OWO Game")]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]

namespace OWO_Rumble
{
    public class OWO_Rumble : MelonMod
    {
        public static OWOSkin owoSkin = null!;

        public override void OnInitializeMelon()
        {
            owoSkin = new OWOSkin();
        }

        //[HarmonyPatch(typeof(CombatManager), "RegisterPlayerHitEvent", new Type[] { typeof(Player), typeof(Structure) })]
        //public class bhaptics_PlayerHit
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(CombatManager __instance, Structure structure, Player player)
        //    {
        //        owoSkin.LOG($"RegisterPlayerHitEvent - structure:{structure.ResourceName}, player:{player.Data.GeneralData}");
        //        //if (player.Controller.ControllerType != ControllerType.Local) return;
        //    }
        //}

        //[HarmonyPatch(typeof(CombatManager), "RegisterPlayerHitFromBeneathEvent", new Type[] { typeof(Player), typeof(Structure) })]
        //public class bhaptics_PlayerHitFromBeneath
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(CombatManager __instance, Structure structure, Player player)
        //    {
        //        owoSkin.LOG($"RegisterPlayerHitFromBeneathEvent - structure:{structure.ResourceName}, player:{player.Data.GeneralData}");
        //        //if (player.Controller.ControllerType != ControllerType.Local) return;
        //    }
        //}

        [HarmonyPatch(typeof(PlayerHealth), "ReduceHealth")]
        public class ReduceHealth
        {
            [HarmonyPostfix]
            public static void PostFix(short amount)
            {
                owoSkin.LOG($"ReduceHealth - Damage: {amount}");
            }
        }

        //[HarmonyPatch(typeof(Il2CppRUMBLE.Players.Subsystems.PlayerHealth), "UpdateLocalHealthbarPercentage", new Type[] { typeof(float), typeof(bool) })]
        //public class bhaptics_PlayerUpdateHealth
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(Il2CppRUMBLE.Players.Subsystems.PlayerHealth __instance, float currentHealth, float lastHealth)
        //    {
        //        //cuando se cura
        //        if (lastHealth < currentHealth) owoSkin.Feel("Heal");

        //        //Gestionar heatbeat
        //        if (currentHealth <= 0.25f) owoSkin.StartHeartBeat();
        //        else if (owoSkin.heartBeatIsActive)
        //        {
        //            owoSkin.StopHeartBeat();
        //        }
        //    }
        //}
    }
}

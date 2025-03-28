﻿using HarmonyLib;
using Il2CppRUMBLE.Combat.ShiftStones;
using Il2CppRUMBLE.Interactions.InteractionBase;
using Il2CppRUMBLE.MoveSystem;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppRUMBLE.Poses;
using MelonLoader;
using Rumble_OWO;
using UnityEngine;


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

        [HarmonyPatch(typeof(CombatManager), "RegisterPlayerHitFromBeneathEvent")]
        public class OWO_PlayerHitFromBeneath
        {
            [HarmonyPostfix]
            public static void Postfix(CombatManager __instance, Structure structure, Player player)
            {
                if (player.Controller.ControllerType != ControllerType.Local) return;
                owoSkin.Feel("Jump", 1);

            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "SetHealth")]
        public class OWO_SetHealth
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerHealth __instance, short newHealth, short previousHealth)
            {
                if (__instance.ParentController.ControllerType != ControllerType.Local) return;

                if (newHealth < previousHealth) owoSkin.Feel("Impact", 2);
                else owoSkin.Feel("Heal", 1);

                if (newHealth <= 5 && newHealth > 0) owoSkin.StartHeartBeat();
                else if (owoSkin.heartBeatIsActive)
                {
                    owoSkin.StopHeartBeat();
                    owoSkin.Feel("Death", 3);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerPoseSystem), "OnPoseSetCompleted")]
        public class OWO_OnPoseSetCompleted
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerPoseSystem __instance, PoseSet set)
            {
                if (__instance.ParentController.ControllerType != ControllerType.Local) return;

                switch (set.name)
                {
                    case "PoseSetRockjump":
                        PlayerMovement playerMovement = (PlayerMovement)__instance.ParentController.subsystems[9];
                        bool isGrounded = playerMovement.IsGrounded();

                        if(isGrounded)
                            owoSkin.Feel("Jump", 1);
                        break;

                    case "PoseSetDash":
                        owoSkin.Feel("Dash", 1);
                        break;
                    case "PoseSetDisc" or "PoseSetBall" or "PoseSetSpawnPillar" or "PoseSetSpawnCube" or "PoseSetWall_Grounded":
                        owoSkin.Feel("Generate Stone", 0);
                        break;


                    case "PoseSetStraight" or "PoseSetUppercut" or "PoseSetKick" or "PoseSetStomp" or "PoseSetParry" or "PoseSetFlick" or "PoseSetRight" or "PoseSetExplode":
                        owoSkin.Feel("Pose Complete", 0);
                        break;

                    default:
                        break;
                }
            }
        }
        
        [HarmonyPatch(typeof(PlayerMovement), "OnBecameGrounded")]
        public class OWO_OnBecameGrounded
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerMovement __instance)
            {
                if (__instance.ParentController.ControllerType != ControllerType.Local) return;
                owoSkin.Feel("Landing", 1);
            }
        }

        [HarmonyPatch(typeof(GuardStone), "IsDoingGuardPose")]
        public class OWO_IsDoingGuardPose
        {
            [HarmonyPostfix]
            public static void Postfix(GuardStone __instance, bool __result)
            {
                if (__instance.attachedSystem.ParentController.ControllerType != ControllerType.Local) return;


                if (__result) owoSkin.StartOnGuard();
                else if (owoSkin.onGuardIsActive)
                {
                    owoSkin.StopOnGuard();
                }
            }
        }

        [HarmonyPatch(typeof(CombatManager), "ApplyPlayerKnockbackForce")]
        public class OWO_ApplyPlayerKnockbackForce
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerController controller, int tierPoints, Vector3 direction, bool disablePIDControllers = true, string structureName = "", float knockbackMultiplier = 1f, float forceModifier = 1f)
            {
                if (controller.ControllerType != ControllerType.Local) return;
                owoSkin.Feel("Knockback", 1);
            }
        }
    }
}
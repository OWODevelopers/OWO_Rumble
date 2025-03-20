using HarmonyLib;
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

        //Funciona
        [HarmonyPatch(typeof(CombatManager), "RegisterPlayerHitEvent")]
        public class bhaptics_PlayerHit
        {
            [HarmonyPostfix]
            public static void Postfix(CombatManager __instance, Structure structure, Player player)
            {
                //TODO: Sentir daño?

                //var asdf = new PlayerVR();
                owoSkin.LOG($"RegisterPlayerHitEvent - frozen:{structure.frozenState.StateName}, {structure.frozenState.stateName}, {structure.frozenState.WasCollected} - IsScene:{structure.isSceneStructure}, {structure.IsSceneStructure}");
                //owoSkin.LOG($"transform player {__instance.pVr.name}");
                if (structure.CurrentSpeed > 10 || !structure.IsGrounded)
                {
                    //owoSkin.LOG($"RegisterPlayerHitEvent - frozen:{structure.frozenState} - {}");
                }
                //owoSkin.LOG($"RegisterPlayerHitEvent - structure:{structure.ResourceName} - crSpe:{structure.CurrentSpeed} - crVeloc:{structure.currentVelocity} - grounded:{structure.IsGrounded} , player:{player.Data.GeneralData.PublicUsername}");
                //if (player.Controller.ControllerType != ControllerType.Local) return;
                //latestAppliedStraightDirection
                //strength
                //currentVelocity
            }
        }

        [HarmonyPatch(typeof(CombatManager), "RegisterPlayerHitFromBeneathEvent")]
        public class bhaptics_PlayerHitFromBeneath
        {
            [HarmonyPostfix]
            public static void Postfix(CombatManager __instance, Structure structure, Player player)
            {
                owoSkin.LOG($"RegisterPlayerHitFromBeneathEvent - structure:{structure.ResourceName}, player:{player.Data.GeneralData}");
                //if (player.Controller.ControllerType != ControllerType.Local) return;
            }
        }

        [HarmonyPatch(typeof(InteractionBase), "StartInteraction")]
        public class StartInteractionBase
        {
            [HarmonyPostfix]
            public static void Postfix(InteractionHand hand)
            {
                //TODO: Sensación de estar interactuando
                owoSkin.LOG($"StartInteractionBase - name: {hand.name}, GameObjectTag: {hand.gameObject.tag}");
            }
        }

        [HarmonyPatch(typeof(CombatManager), "ApplyPlayerKnockbackForce")]
        public class ApplyPlayerKnockbackForce
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerController controller, int tierPoints, Vector3 direction, bool disablePIDControllers = true, string structureName = "", float knockbackMultiplier = 1f, float forceModifier = 1f)
            {
                //TODO: Sentir daño?

                Vector3 flattenedHit = new Vector3(controller.GetSubsystem<PlayerVR>().headset.Transform.localRotation.x, 0f, controller.GetSubsystem<PlayerVR>().headset.Transform.localRotation.z);
                Vector3 patternOrigin = new Vector3(direction.x, 0f, direction.z);
                float earlyhitAngle = Vector3.Angle(flattenedHit, patternOrigin);
                owoSkin.LOG("LocalRotation " + controller.GetSubsystem<PlayerVR>().headset.Transform.localRotation);
                owoSkin.LOG("Direction " + direction);
                owoSkin.LOG("Early angle " + earlyhitAngle);
                Vector3 earlycrossProduct = Vector3.Cross(flattenedHit, patternOrigin);
                owoSkin.LOG("Early CROSS " + earlycrossProduct);
                if (earlycrossProduct.y > 0f) { earlyhitAngle *= -1f; }
                float myRotation = earlyhitAngle;
                myRotation *= -1f;
                if (myRotation < 0f) { myRotation = 360f + myRotation; }

                owoSkin.LOG("Rotation " + myRotation);


                if (myRotation >= 0 && myRotation <= 180)
                {
                    owoSkin.LOG("ESPALDA");
                    if (myRotation >= 0 && myRotation <= 90) owoSkin.LOG("IZQUIERDa");
                    else owoSkin.LOG("DERECHA");
                }
                else
                {
                    owoSkin.LOG("FRENTE");
                    if (myRotation >= 270 && myRotation <= 359) owoSkin.LOG("IZQUIERDA");
                    else owoSkin.LOG("DERCHA");
                }
                //owoSkin.LOG($"ApplyPlayerKnockbackForce - Attack by: {structureName} - from: {direction} - controllerVRRotation?: {controller.GetSubsystem<PlayerVR>().headset.Transform.rotation} - controllerVRLocalRotation?: {controller.GetSubsystem<PlayerVR>().headset.Transform.localRotation} - controllerVRPosition?: {controller.GetSubsystem<PlayerVR>().headset.Transform.position} - controllerVRLocalPosition?: {controller.GetSubsystem<PlayerVR>().headset.Transform.localPosition}");
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "SetHealth")]
        public class SetHealth
        {
            [HarmonyPostfix]
            public static void Postfix(short newHealth, short previousHealth)
            {
                owoSkin.LOG($"SetHealth - newHealth {newHealth}, previousHealth {previousHealth}");

                //TODO: Sentir daño?
                if (newHealth < previousHealth) owoSkin.LOG("Daño Recibido");
                else owoSkin.Feel("Heal");

                //Gestionar heatbeat
                if (newHealth <= 5 && newHealth > 0) owoSkin.StartHeartBeat();
                else if (owoSkin.heartBeatIsActive)
                {
                    owoSkin.StopHeartBeat();
                }
            }
        }

        [HarmonyPatch(typeof(PlayerPoseSystem), "OnPoseSetCompleted")]
        public class OnPoseSetCompletedMelon
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerPoseSystem __instance, PoseSet set)
            {
                //TODO: Sentir invocar piedras
                owoSkin.LOG($"OnPoseSetCompleted - isValidPose: {set.name} {__instance.ParentController.AssignedPlayer.Data.GeneralData.BattlePoints}");
                __instance.ParentController.AssignedPlayer.Data.GeneralData.BattlePoints += 200;

            }
        }
        
        [HarmonyPatch(typeof(PlayerMovement), "OnBecameGrounded")]
        public class OnBecameGroundedMelon
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"OnBecameGrounded ");

            }
        }

        [HarmonyPatch(typeof(GuardStone), "IsDoingGuardPose")]
        public class IsDoingGuardPoseMelon
        {
            [HarmonyPostfix]
            public static void Postfix(bool __result)
            {
                //TODO: Sentir que tengo la guarda activa
                if (__result)
                owoSkin.LOG($"IsDoingGuardPose");
            }
        }

        //Funciona

        //[HarmonyPatch(typeof(PlayerHealth), "ReduceHealth")]
        //public class ReduceHealth
        //{
        //    [HarmonyPostfix]
        //    public static void PostFix(short amount)
        //    {
        //        owoSkin.LOG($"ReduceHealth - Damage: {amount}");
        //    }
        //}


        //No funciona(?

        //[HarmonyPatch(typeof(PlayerHealth), "UpdateLocalHealthbarPercentage")]
        //public class bhaptics_PlayerUpdateHealth
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(PlayerHealth __instance, float currentHealth, float previousHealth)
        //    {
        //        owoSkin.LOG($"UpdateLocalHealthbarPercentage");

        //        //cuando se cura
        //        if (previousHealth < currentHealth) owoSkin.Feel("Heal");

        //        //Gestionar heatbeat
        //        if (currentHealth <= 0.25f) owoSkin.StartHeartBeat();
        //        else if (owoSkin.heartBeatIsActive)
        //        {
        //            owoSkin.StopHeartBeat();
        //        }
        //    }
        //}


        // Funciona

        //[HarmonyPatch(typeof(PlayerHealth), "IncreaseHealth")]
        //public class IncreaseHealth
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(short amount)
        //    {
        //        owoSkin.LOG($"IncreaseHealth  - amount: {amount}");
        //    }
        //}


        //[HarmonyPatch(typeof(PlayerHealth), "HealthRegenerationRoutine")]
        //public class HealthRegenerationRoutine
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix()
        //    {
        //        owoSkin.LOG($"HealthRegenerationRoutine ");
        //    }
        //}

        //[HarmonyPatch(typeof(PlayerHealth), "ForceHealthReset")]
        //public class ForceHealthReset
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix()
        //    {
        //        owoSkin.LOG($"ForceHealthReset");
        //    }
        //}

        //Funciona
        //[HarmonyPatch(typeof(PlayerHealth), "ResetHealthBar")]
        //public class ResetHealthBar
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix()
        //    {
        //        owoSkin.LOG($"ResetHealthBar");
        //    }
        //}

        //[HarmonyPatch(typeof(PlayerHaptics), "ApplyCurrentHapticValues")]
        //public class ApplyCurrentHapticValues
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix()
        //    {
        //        owoSkin.LOG($"ApplyCurrentHapticValues");
        //    }
        //}

        //[HarmonyPatch(typeof(InteractionHoldable), "StartInteraction")]
        //public class StartInteractionHoldable
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(InteractionHand hand)
        //    {
        //        owoSkin.LOG($"StartInteractionHoldable - name: {hand.name}, GameObjectTag: {hand.gameObject.tag}");
        //    }
        //}

        //[HarmonyPatch(typeof(Structure), "OnFetchFromPool")]
        //public class OnFetchFromPool
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(Structure __instance)
        //    {
        //        owoSkin.LOG($"OnFetchFromPool - ResourceName: {__instance.resourceName} - Owner: {__instance.Owner.PoolParent}");
        //    }
        //}

        //[HarmonyPatch(typeof(Pool<Structure>), "FetchFromPool")]
        //public class FetchFromPool
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(Vector3 position, Quaternion rotation)
        //    {
        //        owoSkin.LOG($"FetchFromPool - Position: {position} - Rotation: {rotation}");
        //    }
        //}

        //[HarmonyPatch(typeof(PlayerVR), "GetViewHorizontalRotation")]
        //public class GetViewHorizontalRotation
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(PlayerVR __instance)
        //    {
        //        owoSkin.LOG($"GetViewHorizontalRotation - PlayerVR: {__instance.Headset.Transform}");
        //    }
        //}

        //[HarmonyPatch(typeof(HoldableStructure), "AttachToHand")]
        //public class AttachToHand
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(InteractionHand hand)
        //    {
        //        owoSkin.LOG($"AttachToHand - Hand: {hand}");
        //    }
        //}

        //[HarmonyPatch(typeof(PlayerMovement), "IsExperiencingKnockback")]
        //public class IsExperiencingKnockbackMelon
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix()
        //    {
        //        owoSkin.LOG($"IsExperiencingKnockback");
        //    }
        //}     

        //[HarmonyPatch(typeof(CombatManager), "HasKnockupEventRunning")]
        //public class HasKnockupEventRunningMelon
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(Structure s, Player p)
        //    {
        //        owoSkin.LOG($"HasKnockupEventRunning - s: {s.resourceName} - p: {p.Data.GeneralData.PublicUsername}");
        //    }
        //}






    }
}

//PlayerController -> isPresentInActiveScene